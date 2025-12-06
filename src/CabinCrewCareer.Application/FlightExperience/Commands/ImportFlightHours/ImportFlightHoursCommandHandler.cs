using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using ClosedXML.Excel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.FlightExperience.Commands.ImportFlightHours
{
    public class ImportFlightHoursCommandHandler(IUnitOfWork unitOfWork) 
        : IRequestHandler<ImportFlightHoursCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(ImportFlightHoursCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // 1. Validate round exists and is FlightHoursConfirmation type
            var roundQuery = await _unitOfWork.Rounds.GetAsync(
                expression: r => r.RoundId == request.RoundId,
                includeFunc: query => query.Include(r => r.RoundActivities)
            );

            var round = await roundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (round == null)
            {
                errorMessages.Add("Round not found.");
                throw new ValidationException(ResponseCode.GET_UNSUCC, errorMessages);
            }

            if (round.RoundType != RoundType.FlightHoursConfirmation)
            {
                errorMessages.Add($"Round type must be FlightHoursConfirmation. Current type: {round.RoundType}");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // 2. Get next round (should be Practical Test round)
            var nextRoundQuery = await _unitOfWork.Rounds.GetAsync(
                expression: r => r.CampaignRoundId == round.CampaignRoundId 
                              && r.RoundType == RoundType.PracticalTest
            );

            var nextRound = await nextRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            // 3. Parse Excel file
            var importResults = await ParseExcelFile(request.File);

            if (!importResults.Any())
            {
                errorMessages.Add("No valid data found in Excel file.");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // 4. Process each row
            int totalProcessed = 0;
            int totalPassed = 0;
            int totalFailed = 0;
            List<string> detailResults = [];

            foreach (var result in importResults)
            {
                totalProcessed++;

                // Find RoundActivity for this user
                var roundActivity = round.RoundActivities
                    .FirstOrDefault(ra => ra.UserId == result.UserId && ra.Status == ActivityStatus.Ongoing);

                if (roundActivity == null)
                {
                    detailResults.Add($"User {result.UserId}: Round activity not found or not Ongoing - SKIPPED");
                    continue;
                }

                if (result.IsPassed)
                {
                    // PASS: Update status to Passed and create next round activity
                    roundActivity.Status = ActivityStatus.Passed;
                    totalPassed++;

                    // Create Practical Test RoundActivity if next round exists
                    if (nextRound != null)
                    {
                        var existingNextActivity = await _unitOfWork.RoundActivites.GetAsync(
                            expression: x => x.RoundId == nextRound.RoundId 
                                          && x.UserId == result.UserId
                        );

                        if (!await existingNextActivity.AnyAsync(cancellationToken: cancellationToken))
                        {
                            await _unitOfWork.RoundActivites.AddAsync(new RoundActivity
                            {
                                RoundId = nextRound.RoundId,
                                UserId = result.UserId,
                                ApplicationSnapshotId = roundActivity.ApplicationSnapshotId,
                                Status = ActivityStatus.Ongoing
                            });
                        }
                    }

                    detailResults.Add($"User {result.UserId}: PASSED - Moved to next round");
                }
                else
                {
                    // FAIL: Update status to Failed and create UserHistory
                    roundActivity.Status = ActivityStatus.Failed;
                    totalFailed++;

                    // Create UserHistory
                    await _unitOfWork.UserHistories.AddAsync(new UserHistory
                    {
                        UserId = result.UserId,
                        CampaignRoundId = round.CampaignRoundId,
                        ParticipatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        Type = HistoryType.NotCompleted
                    });

                    detailResults.Add($"User {result.UserId}: FAILED - UserHistory created");
                }

                await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
            }

            await _unitOfWork.CompleteAsync();

            // 5. Build response message
            string summaryMessage = $"Processed {totalProcessed} records from Excel file:\n" +
                                   $"- Passed: {totalPassed}\n" +
                                   $"- Failed: {totalFailed}\n";

            if (nextRound == null && totalPassed > 0)
            {
                summaryMessage += "\nWarning: Next round (Practical Test) not found. Activities marked as Passed but no next round activity created.\n";
            }

            string detailMessage = string.Join("\n", detailResults);

            return new Response<string>(
                code: (int)ResponseCode.UPDATED_SUCCESS,
                data: $"Passed: {totalPassed}, Failed: {totalFailed}",
                message: $"{summaryMessage}\nDetails:\n{detailMessage}"
            );
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

                    // Start from row 3 (row 1 is title, row 2 is header)
                    int currentRow = 3;

                    while (!worksheet.Cell(currentRow, 1).IsEmpty())
                    {
                        try
                        {
                            var userIdCell = worksheet.Cell(currentRow, 1);
                            var passFailCell = worksheet.Cell(currentRow, 9);

                            // Parse UserId
                            if (!int.TryParse(userIdCell.GetString(), out int userId))
                            {
                                currentRow++;
                                continue;
                            }

                            // Parse Pass/Fail status
                            var passFailValue = passFailCell.GetValue<string>().Trim().ToLower();
                            
                            bool? isPassed = null;
                            if (passFailValue == "pass" || passFailValue == "☑")
                            {
                                isPassed = true;
                            }
                            else if (passFailValue == "fail")
                            {
                                isPassed = false;
                            }

                            // Only add if status is determined
                            if (isPassed.HasValue)
                            {
                                results.Add(new ImportResult
                                {
                                    UserId = userId,
                                    IsPassed = isPassed.Value
                                });
                            }
                        }
                        catch
                        {
                            // Skip invalid rows
                        }

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
