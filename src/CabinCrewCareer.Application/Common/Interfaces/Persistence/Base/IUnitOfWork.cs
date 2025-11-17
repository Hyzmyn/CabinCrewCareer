namespace CabinCrewCareer.Application.Common.Interfaces.Persistence.Base
{
    using CabinCrewCareer.Application.Common.Interfaces.Persistence;
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }

        IRoleRepository Roles { get; }

        ICityRepository Cities { get; }

        IWardRepository Wards { get; }

        ICampaignRequestRepository CampaignRequests { get; }

        ICampaignRepository Campaigns { get; }

        ICampaignRoundRepository CampaignRounds { get; }

        IRoundRepository Rounds { get; }

        ITestRepository Tests { get; }

        ITestQuestionRepository TestQuestions { get; }

        ITestOptionRepository TestOptions { get; }

        ICampaignAssignmentRepository CampaignAssignments { get; }

        INotificationRepository Notifications { get; }

        IRoundActivityRepository RoundActivites { get; }

        Task CompleteAsync();
    }
}
