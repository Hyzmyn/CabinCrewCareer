using MediatR;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetTestCountByTestType
{
    public class GetTestCountByTestTypeQuery : IRequest<List<CountByTypeDto>>
    {
    }
}
