using MediatR;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetUserHistoryByMonth
{
    public class GetUserHistoryByMonthQuery : IRequest<List<UserHistoryByMonthDto>>
    {
        public int Year { get; set; }
        public GetUserHistoryByMonthQuery(int year)
        {
            Year = year;
        }
    }
}
