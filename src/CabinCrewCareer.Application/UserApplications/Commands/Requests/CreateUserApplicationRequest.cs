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
        public int Height { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        // public string EnglishDegreeNumber { get; set; } = string.Empty;
        // public DateOnly? EndDate { get; set; }

        // Thông tin đợt tuyển
        public int? CampaignRoundId { get; set; }

        // Thông tin snapshot ứng viên
        [Required]
         public string Email { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public DateOnly? DateOfBirth { get; set; }
        [Required]
        public Gender Gender { get; set; }
        // 5 loại tài liệu
        public IFormFile ApplicationForm { get; set; }
        public IFormFile ProfilePhoto { get; set; }
        public IFormFile EducationDegree { get; set; }
        public IFormFile EnglishCertificate { get; set; }
        public IFormFile PassportOrID { get; set; }

        public IFormFile? PassportOrIDBack { get; set; } // Mặt sau CCCD
    }
}
