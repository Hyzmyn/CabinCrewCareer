using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Roles.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Roles.Queries.GetUsersByRoleId
{
    public class GetUsersByRoleIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetUsersByRoleIdQuery, Response<List<GetRoleResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetRoleResponse>>> Handle(GetUsersByRoleIdQuery request, CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Users.GetUsersByRoleIdAsync(request.RoleId);
            var result = new List<GetRoleResponse>();
            foreach (var user in users)
            {
                var userDto = new GetRoleResponse
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    AssignedCampaigns = new List<AssignedCampaignSimpleDto>()
                };
                var assignments = await _unitOfWork.CampaignAssignments.GetAssignmentsByUserIdAsync(user.UserId);
                foreach (var assignment in assignments)
                {
                    if (assignment.Campaign != null)
                    {
                        userDto.AssignedCampaigns.Add(new AssignedCampaignSimpleDto
                        {
                            StartDate = assignment.Campaign.StartDate?.ToString("yyyy-MM-dd"),
                            EndDate = assignment.Campaign.EndDate?.ToString("yyyy-MM-dd")
                        });
                    }
                }
                result.Add(userDto);
            }
            return new Response<List<GetRoleResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: result,
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
