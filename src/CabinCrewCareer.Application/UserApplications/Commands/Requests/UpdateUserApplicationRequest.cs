using Microsoft.AspNetCore.Http;
using System;

namespace CabinCrewCareer.Application.UserApplications.Commands.Requests
{
    public class UpdateUserApplicationRequest
    {
        public string? Experience { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public string? EnglishDegreeNumber { get; set; }
        public DateOnly? EndDate { get; set; }
        public IFormFile? ApplicationForm { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
        public IFormFile? EducationDegree { get; set; }
        public IFormFile? EnglishCertificate { get; set; }
        public IFormFile? PassportOrID { get; set; }
    }
}
