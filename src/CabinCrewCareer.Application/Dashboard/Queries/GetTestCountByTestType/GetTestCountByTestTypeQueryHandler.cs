using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;
using System.Linq;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetTestCountByTestType
{
    public class GetTestCountByTestTypeQueryHandler : IRequestHandler<GetTestCountByTestTypeQuery, List<CountByTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetTestCountByTestTypeQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<CountByTypeDto>> Handle(GetTestCountByTestTypeQuery request, CancellationToken cancellationToken)
        {
            var tests = await _unitOfWork.Tests.GetAllAsync();
            var result = System.Enum.GetValues(typeof(CabinCrewCareer.Domain.Common.TestType))
                .Cast<CabinCrewCareer.Domain.Common.TestType>()
                .Select(type => new CountByTypeDto
                {
                    Type = type.ToString(),
                    Count = tests.Count(x => x.TestType == type)
                })
                .ToList();
            return result;
        }
    }
}
