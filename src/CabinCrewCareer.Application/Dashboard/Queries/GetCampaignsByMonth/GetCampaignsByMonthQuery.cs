using MediatR;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetCampaignsByMonth
{
    public class GetCampaignsByMonthQuery : IRequest<List<CountByMonthDto>>
    {
        public int Year { get; set; }
        public GetCampaignsByMonthQuery(int year)
        {
            Year = year;
        }
    }
}
