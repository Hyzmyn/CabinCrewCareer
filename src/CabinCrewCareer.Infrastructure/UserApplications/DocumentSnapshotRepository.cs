using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.UserApplications
{
    public class DocumentSnapshotRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<DocumentSnapshot>(dbContext, logger), IDocumentSnapshotRepository
    {
    }
}
