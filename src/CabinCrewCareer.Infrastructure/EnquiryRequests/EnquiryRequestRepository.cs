using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.EnquiryRequests
{
    public class EnquiryRequestRepository(CabinCrewCareerDbContext dbContext, ILogger logger) 
        : Repository<EnquiryRequest>(dbContext, logger), IEnquiryRequestRepository
    {
    }
}
