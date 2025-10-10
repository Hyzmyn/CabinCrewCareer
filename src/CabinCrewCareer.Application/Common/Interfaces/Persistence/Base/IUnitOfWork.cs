namespace CabinCrewCareer.Application.Common.Interfaces.Persistence.Base
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
    }
}
