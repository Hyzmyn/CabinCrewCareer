using AutoMapper;
using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.CampaignRounds.Common;
using CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList;
using CabinCrewCareer.Application.Cities.Common;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Application.InterviewCriterias.Common;
using CabinCrewCareer.Application.Roles.Common;
using CabinCrewCareer.Application.RoundActivities.Common;
using CabinCrewCareer.Application.ScoringCriterias.Common;
using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.Common.Mappings
{
    internal class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            #region User
            CreateMap<User, GetUserResponse>()
                .ForMember(u => u.AirlinePartner, u => u.MapFrom(u => u.PartnerId != null ? u.AirlinePartner.PartnerName : "N/A"))
                .ForMember(u => u.Role, u => u.MapFrom(u => u.Role.RoleName))
                .ForMember(u => u.Gender, u => u.MapFrom(u => u.Gender.GetDescription()))
                .ForMember(m => m.DateOfBirth, m => m.MapFrom(m => m.DateOfBirth.HasValue ? m.DateOfBirth.Value.GetFormatDate() : null));

            CreateMap<User, GetUserDetailResponse>()
                .ForMember(u => u.AirlinePartner, u => u.MapFrom(u => u.PartnerId != null ? u.AirlinePartner.PartnerName : "N/A"))
                .ForMember(u => u.Role, u => u.MapFrom(u => u.Role.RoleName))
                .ForMember(m => m.DateOfBirth, m => m.MapFrom(m => m.DateOfBirth.HasValue ? m.DateOfBirth.Value.GetFormatDate() : null))
                .ForMember(u => u.CityId, u => u.MapFrom(u => u.WardId != null ? u.Ward.CityId : 0))
                .ForMember(u => u.WardId, u => u.MapFrom(u => u.WardId != null ? u.WardId : 0));

            CreateMap<User, GetRoleResponse>();
            CreateMap<PaginatedList<User>, PaginatedList<GetUserResponse>>();
            #endregion

            #region City
            CreateMap<City, GetCityResponse>();

            CreateMap<Ward, GetWardResponse>();
            #endregion

            #region CampaignRequest
            CreateMap<CampaignRequest, GetCampaignRequestResponse>()
                .ForMember(cr => cr.RequestType, cr => cr.MapFrom(cr => cr.RequestType.GetDescription()))
                .ForMember(cr => cr.Status, cr => cr.MapFrom(cr => cr.Status.GetDescription()))
                .ForMember(cr => cr.PartnerName, cr => cr.MapFrom(cr => cr.PartnerId != null ? cr.AirlinePartner.PartnerName : null))
                .ForMember(cr => cr.DirectorName, cr => cr.MapFrom(cr => cr.DirectorId != null ? cr.Director.FullName : null))
                .ForMember(cr => cr.ApprovedAt, cr => cr.MapFrom(cr => cr.ApprovedAt.HasValue ? cr.ApprovedAt.Value.GetFormatDateTime() : null))
                .ForMember(cr => cr.RejectedAt, cr => cr.MapFrom(cr => cr.RejectedAt.HasValue ? cr.RejectedAt.Value.GetFormatDateTime() : null))
                .ForMember(cr => cr.CreatedAt, cr => cr.MapFrom(cr => cr.CreatedAt ?? DateTime.MinValue));

            CreateMap<PaginatedList<CampaignRequest>, PaginatedList<GetCampaignRequestResponse>>();
            #endregion

            #region CampaignRound
            CreateMap<CampaignRound, GetCampaignRoundResponse>()
                .ForMember(cr => cr.StartDate, cr => cr.MapFrom(cr => cr.StartDate.HasValue ? cr.StartDate.Value.GetFormatDateTime() : null))
                .ForMember(cr => cr.EndDate, cr => cr.MapFrom(cr => cr.EndDate.HasValue ? cr.EndDate.Value.GetFormatDateTime() : null));

            CreateMap<Round, GetRoundResponse>()
                .ForMember(cr => cr.RoundName, cr => cr.MapFrom(cr => cr.RoundType.GetDescription()))
                .ForMember(cr => cr.StartDate, cr => cr.MapFrom(cr => cr.StartDate.HasValue ? cr.StartDate.Value.GetFormatDateTime() : null))
                .ForMember(cr => cr.EndDate, cr => cr.MapFrom(cr => cr.EndDate.HasValue ? cr.EndDate.Value.GetFormatDateTime() : null));

            CreateMap<CampaignRound, GetUserCampaignResponse>()
                .ForMember(u => u.CampaignName, u => u.MapFrom(u => u.Campaign.CampaignName))
                .ForMember(u => u.RoundStatus, u => u.MapFrom(u => u.RoundStatus.GetDescription()))
                .ForMember(u => u.Description, u => u.MapFrom(u => u.Campaign.Description))
                .ForMember(u => u.AirlinePartner, u => u.MapFrom(u => u.Campaign.CampaignRequest.AirlinePartner.PartnerName));

            CreateMap<Round, GetRoundActivityResponse>()
                .ForMember(u => u.RoundName, u => u.MapFrom(u => u.RoundType.GetDescription()));

            CreateMap<RoundActivity, GetActivityResponse>()
                .ForMember(u => u.Status, u => u.MapFrom(u => u.Status.GetDescription()))
                .ForMember(u => u.ImgURL, u => u.MapFrom(u => u.User.ImgURL))
                .ForMember(u => u.FullName, u => u.MapFrom(u => u.User.FullName))
                .ForMember(u => u.Email, u => u.MapFrom(u => u.User.Email))
                .ForMember(u => u.PhoneNumber, u => u.MapFrom(u => u.User.PhoneNumber))
                .ForMember(u => u.RoundName, u => u.MapFrom(u => u.Round.RoundType.GetDescription()));

            CreateMap<PaginatedList<RoundActivity>, PaginatedList<GetActivityResponse>>();
            #endregion

            #region Scoring Criterias
            CreateMap<ScoringCriteria, GetScoringCriteriaResponse>()
                .ForMember(u => u.Items, u => u.MapFrom(u => u.ScoringCriteriaItems));

            CreateMap<ScoringCriteriaItem, GetScoringCriteriaItemResponse>()
                .ForMember(u => u.Details, u => u.MapFrom(u => u.ScoringCriteriaDetails));

            CreateMap<ScoringCriteriaDetail, GetScoringCriteriaDetailResponse>();

            CreateMap<InterviewCriteria, GetInterviewCriteriaResponse>()
                .ForMember(u => u.Items, u => u.MapFrom(u => u.InterviewCriteriaItems));

            CreateMap<InterviewCriteriaItem, GetInterviewCriteriaItemResponse>();
            #endregion

            #region Campaign
            CreateMap<Campaign, CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList.GetCampaignListDto>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.HasValue ? s.CreatedAt.Value.GetFormatDateTime() : null));

            CreateMap<Campaign, CabinCrewCareer.Application.Campaigns.Queries.GetCampaignDetail.GetCampaignDetailDto>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.HasValue ? s.CreatedAt.Value.GetFormatDateTime() : null));
            #endregion

            #region Test
            CreateMap<Test, GetAvailableTestResponse>()
                .ForMember(u => u.NumberOfQuestions, u => u.MapFrom(u => u.TestQuestions.Count))
                .ForMember(u => u.TestType, u => u.MapFrom(u => u.TestType.GetDescription()));
            #endregion

            #region Campaign
            CreateMap<Campaign, GetCampaignListDto>()
                .ForMember(u => u.CampaignType, u => u.MapFrom(u => u.CampaignType.ToString()))
                .ForMember(u => u.Status, u => u.MapFrom(u => u.CampaignStatus.ToString()))
                .ForMember(u => u.StartDate, u => u.MapFrom(u => u.StartDate.HasValue ? u.StartDate.Value.GetFormatDateTime() : null))
                .ForMember(u => u.EndDate, u => u.MapFrom(u => u.EndDate.HasValue ? u.EndDate.Value.GetFormatDateTime() : null))
                .ForMember(u => u.RejectedAt, u => u.MapFrom(u => u.RejectedAt.HasValue ? u.RejectedAt.Value.GetFormatDateTime() : null))
                .ForMember(u => u.ApprovedAt, u => u.MapFrom(u => u.ApprovedAt.HasValue ? u.ApprovedAt.Value.GetFormatDateTime() : null))
                .ForMember(u => u.CreatedAt, u => u.MapFrom(u => u.CreatedAt.HasValue ? u.CreatedAt.Value.GetFormatDateTime() : null))
                .ForMember(u => u.PartnerName, u => u.MapFrom(u => u.CampaignRequest != null && u.CampaignRequest.AirlinePartner != null ? u.CampaignRequest.AirlinePartner.PartnerName : null));

            CreateMap<PaginatedList<Campaign>, PaginatedList<GetCampaignListDto>>();
            #endregion

            #region Evaluation Form
            CreateMap<EvaluationForm, GetAppearanceEvaluationFormResponse>()
                .ForMember(u => u.EvaluatedDate, u => u.MapFrom(u => u.EvaluatedDate.GetFormatDateTime()))
                .ForMember(u => u.RoundName, u => u.MapFrom(u => u.RoundActivity.Round.RoundType.ToString()))
                .ForMember(u => u.Candidate, u => u.MapFrom(u => u.RoundActivity.User.FullName))
                .ForMember(u => u.Examiner, u => u.MapFrom(u => u.Examiner.FullName));

            CreateMap<AppearanceResult, GetAppearanceResultResponse>();

            CreateMap<EvaluationForm, GetInterviewEvaluationFormResponse>()
                .ForMember(u => u.EvaluatedDate, u => u.MapFrom(u => u.EvaluatedDate.GetFormatDateTime()))
                .ForMember(u => u.RoundName, u => u.MapFrom(u => u.RoundActivity.Round.RoundType.ToString()))
                .ForMember(u => u.Candidate, u => u.MapFrom(u => u.RoundActivity.User.FullName))
                .ForMember(u => u.Examiner, u => u.MapFrom(u => u.Examiner.FullName))
                .ForMember(u => u.FinalScore, u => u.MapFrom(u => u.InterviewResults.Sum(x => x.Score)));

            CreateMap<EvaluationForm, GetInterviewEvaluationFormDetailResponse>()
                .ForMember(u => u.EvaluatedDate, u => u.MapFrom(u => u.EvaluatedDate.GetFormatDateTime()))
                .ForMember(u => u.RoundName, u => u.MapFrom(u => u.RoundActivity.Round.RoundType.ToString()))
                .ForMember(u => u.Candidate, u => u.MapFrom(u => u.RoundActivity.User.FullName))
                .ForMember(u => u.Examiner, u => u.MapFrom(u => u.Examiner.FullName))
                .ForMember(u => u.FinalScore, u => u.MapFrom(u => u.InterviewResults.Sum(x => x.Score)));

            CreateMap<InterviewResult, GetInterviewResultResponse>();
            #endregion
        }
    }
}
