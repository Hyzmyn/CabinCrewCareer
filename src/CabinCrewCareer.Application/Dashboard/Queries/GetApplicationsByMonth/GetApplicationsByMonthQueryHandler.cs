using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;
using System.Linq;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetApplicationsByMonth
{
    public class GetApplicationsByMonthQueryHandler : IRequestHandler<GetApplicationsByMonthQuery, List<CountByMonthDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetApplicationsByMonthQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<CountByMonthDto>> Handle(GetApplicationsByMonthQuery request, CancellationToken cancellationToken)
        {
            var snapshots = await _unitOfWork.UserApplicationSnapshots.GetAllAsync();
            var result = Enumerable.Range(1, 12)
                .Select(month => new CabinCrewCareer.Application.Dashboard.Common.CountByMonthDto
                {
                    Month = month,
                    Count = snapshots.Count(x => x.SubmissionDate.HasValue && x.SubmissionDate.Value.Year == request.Year && x.SubmissionDate.Value.Month == month)
                })
                .ToList();
            return result;
        }
    }
}
