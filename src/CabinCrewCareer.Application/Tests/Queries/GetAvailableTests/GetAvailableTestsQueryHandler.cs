using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Tests.Queries.GetAvailableTests
{
    public class GetAvailableTestsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetAvailableTestsQuery, Response<List<GetAvailableTestResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetAvailableTestResponse>>> Handle(GetAvailableTestsQuery request, CancellationToken cancellationToken)
        {
            var temp = _mapper.Map<List<GetAvailableTestResponse>>(await _unitOfWork.Tests.GetTestsAsync(request.TestType));

            List<GetAvailableTestResponse> tests = [];

            foreach (var item in temp)
            {
                if (item.NumberOfQuestions > 0)
                {
                    tests.Add(new GetAvailableTestResponse
                    {
                        TestId = item.TestId,
                        TestName = item.TestName,
                        TestType = item.TestType,
                        NumberOfQuestions = item.NumberOfQuestions
                    });
                }
            }

            return new Response<List<GetAvailableTestResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: tests,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
