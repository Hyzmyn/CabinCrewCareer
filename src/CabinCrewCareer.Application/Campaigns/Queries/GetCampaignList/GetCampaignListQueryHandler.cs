using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList
{
    public class GetCampaignListQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetCampaignListQuery, Response<PaginatedList<GetCampaignListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<PaginatedList<GetCampaignListDto>>> Handle(GetCampaignListQuery request, CancellationToken cancellationToken)
        {
            // Get both role names from database in a single query
            var rolesQuery = await _unitOfWork.Roles.GetAsync(r => r.RoleId == 6 || r.RoleId == 9);
            var rolesList = await rolesQuery.ToListAsync(cancellationToken);
            var roles = rolesList.ToDictionary(r => r.RoleId, r => r.RoleName);
            
            var cabinCrewRoleName = roles.GetValueOrDefault(6, string.Empty);
            var purserRoleName = roles.GetValueOrDefault(9, string.Empty);

            var paginatedCampaigns = await _unitOfWork.Campaigns.PaginatedListAsync(
                request.SearchTerm,
                request.SortColumn,
                request.SortOrder,
                request.CampaignType,
                request.CampaignStatus,
                request.Page,
                request.PageSize);

            var mappedData = _mapper.Map<PaginatedList<GetCampaignListDto>>(paginatedCampaigns);
            
            // Set Position based on CampaignType and actual Role names
            foreach (var campaign in mappedData.Items)
            {
                campaign.Position = campaign.CampaignType == "Recruitment" ? cabinCrewRoleName : purserRoleName;
            }

            return new Response<PaginatedList<GetCampaignListDto>>(
                code: (int)ResponseCode.SUCCESS,
                data: mappedData,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
