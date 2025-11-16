using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Notifications
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : base(dbContext, logger)
        {
        }
    }
}
