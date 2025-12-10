using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.TestTypes.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestTypes.Queries.GetTestTypes
{
    public class GetTestTypesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTestTypesQuery, Response<List<GetTestTypeResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<List<GetTestTypeResponse>>> Handle(GetTestTypesQuery request, CancellationToken cancellationToken)
        {
            var roundTypesQuery = await _unitOfWork.TestTypes.GetAllAsync();

            var roundTypes = await roundTypesQuery.ToListAsync(cancellationToken: cancellationToken);

            List<GetTestTypeResponse> types = [];

            foreach (var item in roundTypes)
            {
                types.Add(new GetTestTypeResponse
                {
                    TestTypeId = item.TestTypeId,
                    TestTypeName = item.TestTypeName
                });
            }

            return new Response<List<GetTestTypeResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: types,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
