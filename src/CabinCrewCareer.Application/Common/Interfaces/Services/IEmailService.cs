using CabinCrewCareer.Application.Common.Models.Requests;

namespace CabinCrewCareer.Application.Common.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
