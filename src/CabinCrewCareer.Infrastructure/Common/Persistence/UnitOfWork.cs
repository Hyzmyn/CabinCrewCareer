using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Infrastructure.CampaignAssignments;
using CabinCrewCareer.Infrastructure.CampaignRequests;
using CabinCrewCareer.Infrastructure.Campaigns;
using CabinCrewCareer.Infrastructure.Cities;
using CabinCrewCareer.Infrastructure.InterviewCriterias;
using CabinCrewCareer.Infrastructure.Notifications;
using CabinCrewCareer.Infrastructure.ScoringCriterias;
using CabinCrewCareer.Infrastructure.TestAnswers;
using CabinCrewCareer.Infrastructure.TestOptions;
using CabinCrewCareer.Infrastructure.TestQuestions;
using CabinCrewCareer.Infrastructure.TestSessions;
using CabinCrewCareer.Infrastructure.Tests;
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

        public ITestRepository Tests { get; private set; }

        public ITestQuestionRepository TestQuestions { get; private set; }

        public ITestOptionRepository TestOptions { get; private set; }

        public ICampaignRoundRepository CampaignRounds { get; private set; }

        public IRoundRepository Rounds { get; private set; }

        public ICampaignAssignmentRepository CampaignAssignments { get; private set; }

        public INotificationRepository Notifications { get; private set; }

        public IRoundActivityRepository RoundActivites { get; private set; }

        public IScoringCriteriaRepository ScoringCriterias { get; set; }

        public IInterviewCriteriaRepository InterviewCriterias { get; set; }

        public ITestSessionRepository TestSessions { get; set; }

        public ITestAnswerRepository TestAnswers { get; set; }

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

            Campaigns = new CampaignRepository(_dbContext, _logger);

            CampaignRounds = new CampaignRoundRepository(_dbContext, _logger);

            Rounds = new RoundRepository(_dbContext, _logger);

            Tests = new TestRepository(_dbContext, _logger);

            TestQuestions = new TestQuestionRepository(_dbContext, _logger);

            TestOptions = new TestOptionRepository(_dbContext, _logger);

            CampaignAssignments = new CampaignAssignmentRepository(_dbContext, _logger);

            Notifications = new NotificationRepository(_dbContext, _logger);

            RoundActivites = new RoundActitityRepository(_dbContext, _logger);

            ScoringCriterias = new ScoringCriteriaRepository(_dbContext, _logger);

            InterviewCriterias = new InterviewCriteriaRepository(_dbContext, _logger);

            TestSessions = new TestSessionRepository(_dbContext, _logger);

            TestAnswers = new TestAnswerRepository(_dbContext, _logger);
        }

        public async Task CompleteAsync() => await _dbContext.SaveChangesAsync();
    }
}
