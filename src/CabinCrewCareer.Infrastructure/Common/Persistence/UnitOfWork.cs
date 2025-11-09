using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Infrastructure.CampaignRequests;
using CabinCrewCareer.Infrastructure.Cities;
using CabinCrewCareer.Infrastructure.Users;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.Common.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CabinCrewCareerDbContext _dbContext;

        private readonly ILogger _logger;

        public IUserRepository Users { get; private set; }

        public IRoleRepository Roles { get; private set; }

        public ICityRepository Cities { get; private set; }

        public IWardRepository Wards { get; private set; }

        public ICampaignRequestRepository CampaignRequests { get; private set; }

        public ICampaignRepository Campaigns { get; private set; }

        public UnitOfWork(
            CabinCrewCareerDbContext dbContext,
            ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;

            _logger = loggerFactory.CreateLogger("logs");

            Users = new UserRepository(_dbContext, _logger);

            Roles = new RoleRepository(_dbContext, _logger);

            Cities = new CityRepository(_dbContext, _logger);

            Wards = new WardRepository(_dbContext, _logger);

            CampaignRequests = new CampaignRequestRepository(_dbContext, _logger);

            Campaigns = new CabinCrewCareer.Infrastructure.Campaigns.CampaignRepository(_dbContext, _logger);
        }

        public async Task CompleteAsync() => await _dbContext.SaveChangesAsync();
    }
}
