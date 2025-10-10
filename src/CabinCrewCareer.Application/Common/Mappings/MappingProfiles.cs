using AutoMapper;
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
            CreateMap<User, GetUserResponse>();

            CreateMap<PaginatedList<User>, PaginatedList<GetUserResponse>>();
            #endregion
        }
    }
}
