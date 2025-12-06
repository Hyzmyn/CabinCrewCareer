using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base; // Nếu cần dùng Application Layer
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Entities; // Nếu cần dùng Entity
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Infrastructure.Services
{
    // Helper class for flight hours data
    public class FlightHoursData
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public Domain.Common.Gender Gender { get; set; }
        public string CitizenId { get; set; } = string.Empty;
        public string? Experience { get; set; }
        public int TotalFlightHours { get; set; }
    }

    public class UserExportService : IUserExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        // Inject các service/repository cần thiết
        public UserExportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> ExportUsersByRoundAsync(int roundId, string representativeName)
        {
            // 1. Lấy round
            var round = (await _unitOfWork.Rounds.GetAllAsync(r => r.RoundId == roundId)).FirstOrDefault();
            if (round == null || round.RoundType != Domain.Common.RoundType.Final)
            {
                // Không phải round Final, trả về file mẫu trống
                return CreateExcelFile([], representativeName);
            }

            // 2. Lấy round activities status Ongoing
            var activities = (await _unitOfWork.RoundActivites.GetAllAsync(a => a.RoundId == roundId && a.Status == Domain.Common.ActivityStatus.Ongoing)).ToList();
            var userIds = activities.Select(a => a.ApplicationSnapshotId ?? 0).Where(id => id != 0).Distinct().ToList();
            //var users = (await _unitOfWork.Users.GetAllAsync(u => userIds.Contains(u.UserId))).ToList();
            var applications = (await _unitOfWork.UserApplicationSnapshots.GetAllAsync(app => userIds.Contains(app.ApplicationSnapshotId))).ToList();

            // Gán Application cho user
            //foreach (var user in users)
            //{
            //    user.Application = applications.FirstOrDefault(app => app.UserId == user.UserId);
            //}
            // 3. Tạo file Excel theo mẫu
            return CreateExcelFile(applications, representativeName);
            // 3. Tạo file Excel theo mẫu
            // dòng này đã thay bằng return CreateExcelFile(users) phía trên
        }

        private byte[] CreateExcelFile(List<UserApplicationSnapshot> users, string representativeName)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Users");
                // Header mẫu
                ws.Cell(1, 1).Value = "MẪU ĐỀ NGHỊ CUNG CẤP DỊCH VỤ HẬU KIỂM";
                ws.Range(1, 1, 1, 10).Merge().Style.Font.SetBold().Font.FontSize = 14;
                ws.Cell(2, 1).Value = "Tên đơn vị: Cabin Crew Career";
                ws.Cell(2, 6).Value = "Chức vụ: Recruiter";
                ws.Cell(3, 1).Value = "Địa chỉ: N/A";
                ws.Cell(3, 6).Value = "Điện thoại: N/A";
                ws.Cell(4, 1).Value = $"Đại diện: {representativeName}";
                ws.Range(4, 1, 4, 10).Merge();
                ws.Cell(5, 1).Value = "Mục đích sử dụng TOEIC: Sử dụng cho quá trình tuyển dụng tiếp viên hàng không";
                ws.Range(5, 1, 5, 10).Merge();
                ws.Cell(6, 1).Value = "Đề nghị cung cấp dịch vụ hậu kiểm đối với kết quả thi TOEIC của những cá nhân có tên và thông tin liên quan như sau:";
                ws.Range(6, 1, 6, 10).Merge();

                // Tiêu đề bảng
                ws.Cell(8, 1).Value = "STT";
                ws.Cell(8, 2).Value = "Họ và tên";
                ws.Cell(8, 3).Value = "Ngày sinh";
                ws.Cell(8, 4).Value = "Số CMND/HC";
                ws.Cell(8, 5).Value = "Ngày thi";
                ws.Cell(8, 6).Value = "Số điểm TOEIC hiện tại";
                ws.Cell(8, 7).Value = "Nghe";
                ws.Cell(8, 8).Value = "Đọc";
                ws.Cell(8, 9).Value = "Tổng";

                int row = 9;
                if (users != null && users.Count > 0)
                {
                    int stt = 1;
                    foreach (var user in users)
                    {
                        ws.Cell(row, 1).Value = stt;
                        ws.Cell(row, 2).Value = user.FullName;
                        ws.Cell(row, 3).Value = user.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";
                        ws.Cell(row, 4).Value = user?.CitizenId ?? "";
                        ws.Cell(row, 5).Value = user?.EnglishTestDate?.ToString("dd/MM/yyyy") ?? "";
                        ws.Cell(row, 6).Value = user?.EnglishDegreeNumber ?? "";
                        ws.Cell(row, 7).Value = user?.ListeningScore ?? 0;
                        ws.Cell(row, 8).Value = user?.ReadingScore ?? 0;
                        ws.Cell(row, 9).Value = user?.TotalScore ?? 0;
                        row++;
                        stt++;
                    }
                }
                // Định dạng bảng
                ws.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> ExportFlightHoursConfirmationAsync(int roundId)
        {
            // 1. Lấy round và validate RoundType là FlightHoursConfirmation (3)
            var round = (await _unitOfWork.Rounds.GetAllAsync(r => r.RoundId == roundId)).FirstOrDefault();
            if (round == null || round.RoundType != Domain.Common.RoundType.FlightHoursConfirmation)
            {
                // Không phải round FlightHoursConfirmation, trả về file trống
                return CreateFlightHoursExcelFile([]);
            }

            // 2. Lấy các RoundActivity thuộc round này
            var activities = (await _unitOfWork.RoundActivites.GetAllAsync(
                a => a.RoundId == roundId,
                includeFunc: query => query
                    .Include(a => a.User)
                    .Include(a => a.UserApplicationSnapshot)
            )).ToList();

            // 3. Lấy thông tin từ ApplicationSnapshot
            var data = activities
                .Where(a => a.UserApplicationSnapshot != null && a.User != null)
                .Select(a => new FlightHoursData
                {
                    UserId = a.UserId ?? 0,
                    FullName = a.UserApplicationSnapshot!.FullName,
                    PhoneNumber = a.UserApplicationSnapshot.PhoneNumber,
                    DateOfBirth = a.UserApplicationSnapshot.DateOfBirth,
                    Gender = a.UserApplicationSnapshot.Gender,
                    CitizenId = a.UserApplicationSnapshot.CitizenId,
                    Experience = a.UserApplicationSnapshot.Experience,
                    TotalFlightHours = a.UserApplicationSnapshot.TotalFightHours
                })
                .ToList();

            // 4. Tạo file Excel
            return CreateFlightHoursExcelFile(data);
        }

        private byte[] CreateFlightHoursExcelFile(List<FlightHoursData> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Flight Hours Confirmation");

                // Hàng 1: Tiêu đề
                ws.Cell(1, 1).Value = "BẢNG XÁC NHẬN GIỜ BAY VÀ KINH NGHIỆM";
                ws.Range(1, 1, 1, 9).Merge();
                ws.Cell(1, 1).Style
                    .Font.SetBold()
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Row(1).Height = 30;

                // Hàng 2: Header columns
                ws.Cell(2, 1).Value = "Mã Số Ứng Viên";
                ws.Cell(2, 2).Value = "Họ và Tên";
                ws.Cell(2, 3).Value = "Số điện thoại";
                ws.Cell(2, 4).Value = "Ngày sinh";
                ws.Cell(2, 5).Value = "Giới tính";
                ws.Cell(2, 6).Value = "CCCD";
                ws.Cell(2, 7).Value = "Kinh nghiệm";
                ws.Cell(2, 8).Value = "Tổng giờ bay";
                ws.Cell(2, 9).Value = "Pass/Fail";

                // Style cho header
                var headerRange = ws.Range(2, 1, 2, 9);
                headerRange.Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.LightBlue)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin);
                ws.Row(2).Height = 25;

                // Hàng 3 trở đi: Data
                int currentRow = 3;
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        ws.Cell(currentRow, 1).Value = item.UserId;
                        ws.Cell(currentRow, 2).Value = item.FullName;
                        ws.Cell(currentRow, 3).Value = item.PhoneNumber;
                        ws.Cell(currentRow, 4).Value = item.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";
                        ws.Cell(currentRow, 5).Value = item.Gender == Domain.Common.Gender.Male ? "Nam" : 
                                                       item.Gender == Domain.Common.Gender.Female ? "Nữ" : "Khác";
                        ws.Cell(currentRow, 6).Value = item.CitizenId ?? "";
                        ws.Cell(currentRow, 7).Value = item.Experience ?? "";
                        ws.Cell(currentRow, 8).Value = item.TotalFlightHours;

                        // Cột Pass/Fail - tạo dropdown list với Data Validation
                        ws.Cell(currentRow, 9).Value = ""; // Để trống ban đầu
                        ws.Cell(currentRow, 9).Style
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                        
                        // Thêm Data Validation cho dropdown list
                        var validation = ws.Cell(currentRow, 9).CreateDataValidation();
                        validation.List("\"☐,☑,Pass,Fail\"");
                        validation.ShowErrorMessage = true;
                        validation.ErrorTitle = "Invalid Selection";
                        validation.ErrorMessage = "Please select from the dropdown list.";

                        // Style cho data row
                        ws.Range(currentRow, 1, currentRow, 9).Style
                            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                            .Border.SetInsideBorder(XLBorderStyleValues.Thin);

                        currentRow++;
                    }
                }

                // Auto-fit columns
                ws.Columns(1, 8).AdjustToContents();
                ws.Column(9).Width = 12; // Fixed width cho checkbox column

                // Freeze header rows
                ws.SheetView.FreezeRows(2);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}

