using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using System.Threading;
using System.Threading.Tasks;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetTotalUsers
{
    public class GetTotalUsersQueryHandler : IRequestHandler<GetTotalUsersQuery, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetTotalUsersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<int> Handle(GetTotalUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            // Nếu cần lọc theo loại user (Candidate, CabinCrew), có thể bổ sung điều kiện tại đây
            return users.Count(u => u.Role.RoleId == 6 || u.Role.RoleId == 7);
        }
    }
}
