using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.AirlinePartners
{
    public class AirlinePartnerRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<AirlinePartner>(dbContext, logger), IAirlinePartnerRepository
    {
    }
}
