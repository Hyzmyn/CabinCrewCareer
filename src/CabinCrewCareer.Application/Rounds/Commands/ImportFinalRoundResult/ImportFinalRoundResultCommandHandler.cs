using MediatR;
using CabinCrewCareer.Domain.Wrappers;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using ClosedXML.Excel;
using System.Text;
using CabinCrewCareer.Domain.Common;
using Microsoft.AspNetCore.Http;
using CabinCrewCareer.Application.Common.Models.Requests;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace CabinCrewCareer.Application.Rounds.Commands.ImportFinalRoundResult
{
    public class ImportFinalRoundResultCommandHandler : IRequestHandler<ImportFinalRoundResultCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public ImportFinalRoundResultCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task<Response<string>> Handle(ImportFinalRoundResultCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = new();
            var roundQuery = await _unitOfWork.Rounds.GetAsync(
                expression: r => r.RoundId == request.RoundId,
                includeFunc: query => query.Include(r => r.RoundActivities)
            );
            var round = await roundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (round == null || round.RoundType != Domain.Common.RoundType.Final)
                return new Response<string>(null, "Invalid round or not Final round", 400);

            var importResults = await ParseExcelFile(request.File);
            if (!importResults.Any())
                return new Response<string>(null, "No valid data found in Excel file.", 400);

            var ongoingActivities = round.RoundActivities.Where(ra => ra.Status == Domain.Common.ActivityStatus.Ongoing).ToList();
            var userIdsInExcel = importResults.Select(r => r.UserId).ToHashSet();

            var roles = await _unitOfWork.Roles.GetAllAsync(r => r.RoleName == Domain.Common.AvailableRole.Candidate.ToString() || r.RoleName == Domain.Common.AvailableRole.CabinCrew.ToString());
            var candidateRole = roles.FirstOrDefault(r => r.RoleName == Domain.Common.AvailableRole.Candidate.ToString());
            var cabinCrewRole = roles.FirstOrDefault(r => r.RoleName == Domain.Common.AvailableRole.CabinCrew.ToString());

            int totalProcessed = 0, totalPassed = 0, totalFailed = 0, totalAutoFailed = 0;
            List<string> detailResults = new();

            foreach (var result in importResults)
            {
                totalProcessed++;
                var roundActivity = round.RoundActivities.FirstOrDefault(ra => ra.UserId == result.UserId && ra.Status == Domain.Common.ActivityStatus.Ongoing);
                if (roundActivity == null)
                {
                    detailResults.Add($"User {result.UserId}: Round activity not found or not Ongoing - SKIPPED");
                    continue;
                }
                var user = await _unitOfWork.Users.GetByIdAsync(result.UserId);
                if (result.IsPassed)
                {
                    roundActivity.Status = Domain.Common.ActivityStatus.Passed;
                    totalPassed++;
                    // Update UserHistory
                    var userHistoryQuery = await _unitOfWork.UserHistories.GetAsync(x => x.UserId == result.UserId && x.CampaignRoundId == round.CampaignRoundId);
                    var userHistory = await userHistoryQuery.FirstOrDefaultAsync(cancellationToken);
                    if (userHistory != null)
                    {
                        userHistory.Type = Domain.Common.HistoryType.Completed;
                        await _unitOfWork.UserHistories.UpdateAsync(userHistory);
                    }
                    // Đổi role nếu là candidate, log giá trị trước/sau
                    if (user != null && user.RoleId == candidateRole?.RoleId && cabinCrewRole != null)
                    {
                        var oldRoleId = user.RoleId;
                        var newRoleId = cabinCrewRole.RoleId;
                        Console.WriteLine($"[ImportFinalRoundResult] UserId: {user.UserId}, OldRoleId: {oldRoleId}, NewRoleId: {newRoleId}");
                        user.RoleId = newRoleId;
                        await _unitOfWork.Users.UpdateAsync(user);
                        Console.WriteLine($"[ImportFinalRoundResult] UserId: {user.UserId}, RoleId after update: {user.RoleId}");
                    }
                    // Gửi email theo template
                    await SendResultEmailAsync(user, true, cancellationToken);
                    detailResults.Add($"User {result.UserId}: PASSED - Role updated, email sent");
                }
                else
                {
                    roundActivity.Status = Domain.Common.ActivityStatus.Failed;
                    totalFailed++;
                    var userHistoryQuery = await _unitOfWork.UserHistories.GetAsync(x => x.UserId == result.UserId && x.CampaignRoundId == round.CampaignRoundId);
                    var userHistory = await userHistoryQuery.FirstOrDefaultAsync(cancellationToken);
                    if (userHistory != null)
                    {
                        userHistory.Type = Domain.Common.HistoryType.NotCompleted;
                        await _unitOfWork.UserHistories.UpdateAsync(userHistory);
                    }
                    await SendResultEmailAsync(user, false, cancellationToken);
                    detailResults.Add($"User {result.UserId}: FAILED - email sent");
                }
                await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
            }
            // Auto-fail users not in Excel file
            foreach (var activity in ongoingActivities)
            {
                if (!userIdsInExcel.Contains(activity.UserId ?? 0))
                {
                    totalProcessed++;
                    totalAutoFailed++;
                    activity.Status = Domain.Common.ActivityStatus.Failed;
                    var userHistoryQuery = await _unitOfWork.UserHistories.GetAsync(x => x.UserId == activity.UserId && x.CampaignRoundId == round.CampaignRoundId);
                    var userHistory = await userHistoryQuery.FirstOrDefaultAsync(cancellationToken);
                    if (userHistory != null)
                    {
                        userHistory.Type = Domain.Common.HistoryType.NotCompleted;
                        userHistory.ParticipatedDate = DateOnly.FromDateTime(DateTime.Now);
                        await _unitOfWork.UserHistories.UpdateAsync(userHistory);
                    }
                    var user = await _unitOfWork.Users.GetByIdAsync(activity.UserId ?? 0);
                    await SendResultEmailAsync(user, false, cancellationToken);
                    await _unitOfWork.RoundActivites.UpdateAsync(activity);
                    detailResults.Add($"User {activity.UserId}: AUTO-FAILED - Not found in Excel file, email sent");
                }
            }
            await _unitOfWork.CompleteAsync();
            string summaryMessage = $"Processed {totalProcessed} records:\n" +
                                   $"- Passed: {totalPassed}\n" +
                                   $"- Failed (from Excel): {totalFailed}\n" +
                                   $"- Auto-Failed (not in Excel): {totalAutoFailed}\n";
            string detailMessage = string.Join("\n", detailResults);
            return new Response<string>(
                code: 200,
                data: $"Passed: {totalPassed}, Failed: {totalFailed}",
                message: $"{summaryMessage}\nDetails:\n{detailMessage}"
            );
        }

        private async Task SendResultEmailAsync(
            CabinCrewCareer.Domain.Entities.User user,
            bool isPassed,
            CancellationToken cancellationToken)
        {
            if (user == null || string.IsNullOrEmpty(user.Email)) return;
            // Đọc template email dùng chung
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
            string templateBody = File.Exists(templatePath)
                ? await File.ReadAllTextAsync(templatePath, cancellationToken)
                : $"Kết quả vòng Final: {{Result}}";
            templateBody = templateBody
                .Replace("{FullName}", user.FullName ?? "")
                .Replace("{Result}", isPassed ? "Đạt" : "Không đạt")
                .Replace("{RoundName}", "Vòng Final")
                .Replace("{CampaignName}", "Cabin Crew Career")
                .Replace("{ExtraMessage}", isPassed ? "Chúc mừng bạn đã vượt qua vòng Final và trở thành Cabin Crew." : "Rất tiếc bạn chưa vượt qua vòng Final.")
                .Replace("{NextRoundInfo}", "");
            var mailRequest = new MailRequest
            {
                ToEmail = user.Email,
                Subject = "Kết quả vòng Final - Cabin Crew Career",
                Body = templateBody
            };
            await _emailService.SendEmailAsync(mailRequest);
        }

        private async Task<List<ImportResult>> ParseExcelFile(Microsoft.AspNetCore.Http.IFormFile file)
        {
            var results = new List<ImportResult>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.First();
                    int currentRow = 9;
                    while (!worksheet.Cell(currentRow, 2).IsEmpty())
                    {
                        try
                        {
                            var userIdCell = worksheet.Cell(currentRow, 2);
                            var passFailCell = worksheet.Cell(currentRow, 11);
                            if (!int.TryParse(userIdCell.GetString(), out int userId))
                            {
                                currentRow++;
                                continue;
                            }
                            var passFailValue = passFailCell.GetValue<string>().Trim().ToLower();
                            bool? isPassed = null;
                            if (string.IsNullOrEmpty(passFailValue))
                                isPassed = false;
                            else if (passFailValue == "pass" || passFailValue == "☑")
                                isPassed = true;
                            else if (passFailValue == "fail")
                                isPassed = false;
                            if (isPassed.HasValue)
                            {
                                results.Add(new ImportResult
                                {
                                    UserId = userId,
                                    IsPassed = isPassed.Value
                                });
                            }
                        }
                        catch { }
                        currentRow++;
                    }
                }
            }
            return results;
        }
        private class ImportResult
        {
            public int UserId { get; set; }
            public bool IsPassed { get; set; }
        }
    }
}
