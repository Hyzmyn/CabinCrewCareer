using AutoMapper;
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
                .ForMember(m => m.DateOfBirth, m => m.MapFrom(m => m.DateOfBirth.GetFormatDate()));

            CreateMap<User, GetUserDetailResponse>()
                .ForMember(u => u.AirlinePartner, u => u.MapFrom(u => u.PartnerId != null ? u.AirlinePartner.PartnerName : "N/A"))
                .ForMember(u => u.Role, u => u.MapFrom(u => u.Role.RoleName))
                .ForMember(u => u.CityId, u => u.MapFrom(u => u.WardId != null ? u.Ward.CityId : 0))
                .ForMember(u => u.WardId, u => u.MapFrom(u => u.WardId != null ? u.WardId : 0));

            CreateMap<PaginatedList<User>, PaginatedList<GetUserResponse>>();
            #endregion

            #region City
            CreateMap<City, GetCityResponse>();

            CreateMap<Ward, GetWardResponse>();
            #endregion
        }
    }
}
