using ClosedXML.Excel;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base; // Nếu cần dùng Application Layer
using CabinCrewCareer.Domain.Entities; // Nếu cần dùng Entity

namespace CabinCrewCareer.Infrastructure.Services
{
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
                return CreateExcelFile(new List<User>(), representativeName);
            }

            // 2. Lấy round activities status Ongoing
            var activities = (await _unitOfWork.RoundActivites.GetAllAsync(a => a.RoundId == roundId && a.Status == Domain.Common.ActivityStatus.Ongoing)).ToList();
            var userIds = activities.Select(a => a.UserId ?? 0).Where(id => id != 0).Distinct().ToList();
            var users = (await _unitOfWork.Users.GetAllAsync(u => userIds.Contains(u.UserId))).ToList();
            var applications = (await _unitOfWork.UserApplications.GetAllAsync(app => userIds.Contains(app.UserId ?? 0))).ToList();

            // Gán Application cho user
            foreach (var user in users)
            {
                user.Application = applications.FirstOrDefault(app => app.UserId == user.UserId);
            }
            // 3. Tạo file Excel theo mẫu
            return CreateExcelFile(users, representativeName);
            // 3. Tạo file Excel theo mẫu
            // dòng này đã thay bằng return CreateExcelFile(users) phía trên
        }

        private byte[] CreateExcelFile(List<User> users, string representativeName)
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
                        var app = user.Application;
                        ws.Cell(row, 1).Value = stt;
                        ws.Cell(row, 2).Value = user.FullName;
                        ws.Cell(row, 3).Value = user.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";
                        ws.Cell(row, 4).Value = app?.CitizenId ?? "";
                        ws.Cell(row, 5).Value = app?.EnglishTestDate?.ToString("dd/MM/yyyy") ?? "";
                        ws.Cell(row, 6).Value = app?.EnglishDegreeNumber ?? "";
                        ws.Cell(row, 7).Value = app?.ListeningScore ?? 0;
                        ws.Cell(row, 8).Value = app?.ReadingScore ?? 0;
                        ws.Cell(row, 9).Value = app?.TotalScore ?? 0;
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
        }
    }

