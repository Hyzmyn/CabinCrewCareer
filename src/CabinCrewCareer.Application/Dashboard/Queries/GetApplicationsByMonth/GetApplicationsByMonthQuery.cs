using MediatR;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetApplicationsByMonth
{
    public class GetApplicationsByMonthQuery : IRequest<List<CountByMonthDto>>
    {
        public int Year { get; set; }
        public GetApplicationsByMonthQuery(int year)
        {
            Year = year;
        }
    }
}
