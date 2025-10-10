using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Infrastructure.Users;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Common.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CabinCrewCareerDbContext _dbContext;

        private readonly ILogger _logger;

        public IUserRepository Users { get; private set; }

        public UnitOfWork(
            CabinCrewCareerDbContext dbContext,
            ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;

            _logger = loggerFactory.CreateLogger("logs");

            Users = new UserRepository(_dbContext, _logger);
        }
    }
}
