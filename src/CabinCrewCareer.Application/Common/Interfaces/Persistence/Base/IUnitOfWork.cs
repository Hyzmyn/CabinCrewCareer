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

        ITestRepository Tests { get; }

        Task CompleteAsync();
    }
}
