using System.ComponentModel.DataAnnotations;

namespace CabinCrewCareer.Application.UserApplications.Commands.Requests
{
    public class UpdateUserApplicationStatusRequest
    {
        [Required]
        public int ApplicationId { get; set; }
    }
}
