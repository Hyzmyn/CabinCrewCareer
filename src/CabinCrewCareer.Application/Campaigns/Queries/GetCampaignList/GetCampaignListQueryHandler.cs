using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

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
            //var campaigns = await unitOfWork.Campaigns.GetAllAsync(disableTracking: true);
            //var result = await campaigns
            //    .Include(c => c.CampaignRequest)
            //    .ThenInclude(cr => cr.AirlinePartner)
            //    .Select(c => new GetCampaignListDto
            //    {
            //        CampaignId = c.CampaignId,
            //        CampaignName = c.CampaignName,
            //        Description = c.Description,
            //        TargetQuantity = c.TargetQuantity,
            //        CampaignType = c.CampaignType.ToString(),
            //        Status = c.CampaignStatus.ToString(),
            //        StartDate = c.StartDate.HasValue ? c.StartDate.Value.GetFormatDateTime() : null,
            //        EndDate = c.EndDate.HasValue ? c.EndDate.Value.GetFormatDateTime() : null,
            //        ApprovedAt = c.ApprovedAt.HasValue ? c.ApprovedAt.Value.GetFormatDateTime() : null,
            //        RejectedAt = c.RejectedAt.HasValue ? c.RejectedAt.Value.GetFormatDateTime() : null,
            //        ReviewedBy = c.ReviewedBy,
            //        RejectedReason = c.RejectedReason,
            //        RejectedCount = c.RejectedCount,
            //        PartnerName = c.CampaignRequest != null && c.CampaignRequest.AirlinePartner != null ? c.CampaignRequest.AirlinePartner.PartnerName : null,
            //        CreatedAt = c.CreatedAt.HasValue ? c.CreatedAt.Value.GetFormatDateTime() : null
            //    })
            //    .ToListAsync(cancellationToken);
            //return result;

            return new Response<PaginatedList<GetCampaignListDto>>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<PaginatedList<GetCampaignListDto>>(await _unitOfWork.Campaigns.PaginatedListAsync(
                    request.SearchTerm,
                    request.SortColumn,
                    request.SortOrder,
                    request.CampaignType,
                    request.CampaignStatus,
                    request.Page,
                    request.PageSize)),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
