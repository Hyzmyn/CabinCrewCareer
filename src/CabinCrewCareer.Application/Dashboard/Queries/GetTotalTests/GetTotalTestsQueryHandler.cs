using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using System.Threading;
using System.Threading.Tasks;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetTotalTests
{
    public class GetTotalTestsQueryHandler : IRequestHandler<GetTotalTestsQuery, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetTotalTestsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<int> Handle(GetTotalTestsQuery request, CancellationToken cancellationToken)
        {
            var tests = await _unitOfWork.Tests.GetAllAsync();
            return tests.Count();
        }
    }
}
