using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;
using System.Linq;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetCampaignsByMonth
{
    public class GetCampaignsByMonthQueryHandler : IRequestHandler<GetCampaignsByMonthQuery, List<CountByMonthDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetCampaignsByMonthQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<CountByMonthDto>> Handle(GetCampaignsByMonthQuery request, CancellationToken cancellationToken)
        {
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync();
            var result = Enumerable.Range(1, 12)
                .Select(month => new CabinCrewCareer.Application.Dashboard.Common.CountByMonthDto
                {
                    Month = month,
                    Count = campaigns.Count(x => x.StartDate.HasValue && x.StartDate.Value.Year == request.Year && x.StartDate.Value.Month == month)
                })
                .ToList();
            return result;
        }
    }
}
