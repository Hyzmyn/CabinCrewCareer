namespace CabinCrewCareer.Application.Common.Interfaces.Persistence.Base
{
    using CabinCrewCareer.Application.Common.Interfaces.Persistence;
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }

        IUserApplicationRepository UserApplications { get; }

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

        IScoringCriteriaRepository ScoringCriterias { get; }

        IInterviewCriteriaRepository InterviewCriterias { get; }

        ITestSessionRepository TestSessions { get; }

        ITestAnswerRepository TestAnswers { get; }

        IDocumentRepository Documents { get; }

        IEvaluationFormRepository EvaluationForms { get; }

        IUserHistoryRepository UserHistories { get; }

        IEnquiryRequestRepository EnquiryRequests { get; }

        IUserApplicationSnapshotRepository UserApplicationSnapshots { get; }

        IDocumentSnapshotRepository DocumentSnapshots { get; }

        Task CompleteAsync();
    }
}
