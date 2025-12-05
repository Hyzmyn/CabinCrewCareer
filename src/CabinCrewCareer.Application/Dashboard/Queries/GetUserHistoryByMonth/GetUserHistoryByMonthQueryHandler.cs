using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Dashboard.Common;
using CabinCrewCareer.Domain.Common;
using System.Collections.Generic;
using System.Linq;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetUserHistoryByMonth
{
    public class GetUserHistoryByMonthQueryHandler : IRequestHandler<GetUserHistoryByMonthQuery, List<UserHistoryByMonthDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetUserHistoryByMonthQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<UserHistoryByMonthDto>> Handle(GetUserHistoryByMonthQuery request, CancellationToken cancellationToken)
        {
            var histories = await _unitOfWork.UserHistories.GetAllAsync();
            var filtered = histories.Where(x => x.ParticipatedDate.Year == request.Year);
            var result = Enumerable.Range(1, 12)
                .Select(month => new UserHistoryByMonthDto
                {
                    Month = month,
                    PassCount = filtered.Count(x => x.ParticipatedDate.Month == month && x.Type == HistoryType.Completed),
                    FailedCount = filtered.Count(x => x.ParticipatedDate.Month == month && x.Type == HistoryType.NotCompleted)
                })
                .ToList();
            return result;
        }
    }
}
