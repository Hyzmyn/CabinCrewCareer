using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Application.UserApplications.Commands.Requests
{
    public class CreateUserApplicationRequest
    {
        // Các trường thông tin ứng viên
        [Required]
        public string Experience { get; set; } = string.Empty;
        [Required]
        public int Height { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        // public string EnglishDegreeNumber { get; set; } = string.Empty;
        // public DateOnly? EndDate { get; set; }

        // Thông tin đợt tuyển
        public int? CampaignRoundId { get; set; }

        // 5 loại tài liệu
        public IFormFile ApplicationForm { get; set; }
        public IFormFile ProfilePhoto { get; set; }
        public IFormFile EducationDegree { get; set; }
        public IFormFile EnglishCertificate { get; set; }
        public IFormFile PassportOrID { get; set; }
    }
}
