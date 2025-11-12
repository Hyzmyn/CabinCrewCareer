using AutoMapper;
using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.CampaignRounds.Common;
using CabinCrewCareer.Application.Cities.Common;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Models;
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
                .ForMember(cr => cr.RoundName, cr => cr.MapFrom(cr => cr.RoundType.GetDescription()));
            #endregion
        }
    }
}
