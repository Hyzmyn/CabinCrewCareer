using System.Threading.Tasks;

namespace CabinCrewCareer.Application.Common.Interfaces.Services
{
    public interface IUserExportService
    {
        Task<byte[]> ExportUsersByRoundAsync(int roundId, string representativeName);
        Task<byte[]> ExportFlightHoursConfirmationAsync(int roundId);
    }
}
