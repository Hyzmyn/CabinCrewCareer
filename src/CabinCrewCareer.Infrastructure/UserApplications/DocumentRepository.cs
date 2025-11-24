using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.UserApplications
{
    public class DocumentRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<Document>(dbContext, logger), IDocumentRepository
    {
    }
}
