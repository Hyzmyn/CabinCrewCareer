using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using System.Threading;
using System.Threading.Tasks;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetTotalCampaigns
{
    public class GetTotalCampaignsQueryHandler : IRequestHandler<GetTotalCampaignsQuery, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetTotalCampaignsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<int> Handle(GetTotalCampaignsQuery request, CancellationToken cancellationToken)
        {
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync();
            return campaigns.Count();
        }
    }
}
