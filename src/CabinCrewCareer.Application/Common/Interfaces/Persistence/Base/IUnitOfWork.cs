namespace CabinCrewCareer.Application.Common.Interfaces.Persistence.Base
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }

        IRoleRepository Roles { get; }

        ICityRepository Cities { get; }

        IWardRepository Wards { get; }

        ICampaignRequestRepository CampaignRequests { get; }

        Task CompleteAsync();
    }
}
