using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.RoundActivities.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.RoundActivities.Queries
{
    public class GetRoundActivitiesQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService) : IRequestHandler<GetRoundActivitiesQuery, Response<PaginatedList<GetActivityResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<PaginatedList<GetActivityResponse>>> Handle(GetRoundActivitiesQuery request, CancellationToken cancellationToken)
        {
            int userId = 0;

            var user = _currentUserService.CurrentPrincipal;

            if (user is not null) userId = Int32.Parse(user);

            string userRole = "";

            if (_currentUserService.Role is not null) userRole = _currentUserService.Role;

            RoundType type = 0;

            if (request.RoundId is not null)
            {
                var round = await _unitOfWork.Rounds.GetByIdAsync(request.RoundId);

                if (round is not null) type = round.RoundType;
            }

            if (type == RoundType.Appearance && userRole.Equals(AvailableRole.Examiner.ToString()))
            {
                var rounds = _mapper.Map<PaginatedList<GetActivityResponse>>(await _unitOfWork.RoundActivites.PaginatedListAsync(
                    request.SearchTerm,
                    request.SortColumn,
                    request.SortOrder,
                    request.Status,
                    request.RoundId,
                    request.Page,
                    request.PageSize
                ));

                foreach (var item in rounds.Items)
                {
                    var imgURLQuery = await _unitOfWork.DocumentSnapshots.GetAsync(x => x.ApplicationSnapshotId == item.ApplicationSnapshotId && x.Type == DocumentType.ProfilePhoto);

                    var imgURL = await imgURLQuery.Select(x => x.DocumentURL).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (!string.IsNullOrEmpty(imgURL)) item.ImgURL = imgURL;

                    item.HasAppearanceEvaluated = await _unitOfWork.EvaluationForms.HasExistedAppearanceForm(item.ActivityId);
                }

                return new Response<PaginatedList<GetActivityResponse>>(code: (int)ResponseCode.SUCCESS,
                    data: rounds,
                    message: ResponseCode.SUCCESS.ToString());
            }

            if (type == RoundType.Interview && userRole.Equals(AvailableRole.Examiner.ToString()))
            {
                var rounds = _mapper.Map<PaginatedList<GetActivityResponse>>(await _unitOfWork.RoundActivites.PaginatedListAsync(
                    request.SearchTerm,
                    request.SortColumn,
                    request.SortOrder,
                    request.Status,
                    request.RoundId,
                    request.Page,
                    request.PageSize
                ));

                foreach (var item in rounds.Items)
                {
                    var imgURLQuery = await _unitOfWork.DocumentSnapshots.GetAsync(x => x.ApplicationSnapshotId == item.ApplicationSnapshotId && x.Type == DocumentType.ProfilePhoto);

                    var imgURL = await imgURLQuery.Select(x => x.DocumentURL).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (!string.IsNullOrEmpty(imgURL)) item.ImgURL = imgURL;

                    item.HasInterviewEvaluated = await _unitOfWork.EvaluationForms.HasEvaluatedRound(item.ActivityId, userId);
                }

                return new Response<PaginatedList<GetActivityResponse>>(code: (int)ResponseCode.SUCCESS,
                    data: rounds,
                    message: ResponseCode.SUCCESS.ToString());
            }

            var roundActivities = _mapper.Map<PaginatedList<GetActivityResponse>>(await _unitOfWork.RoundActivites.PaginatedListAsync(
                    request.SearchTerm,
                    request.SortColumn,
                    request.SortOrder,
                    request.Status,
                    request.RoundId,
                    request.Page,
                    request.PageSize
                ));

            foreach (var item in roundActivities.Items)
            {
                var imgURLQuery = await _unitOfWork.DocumentSnapshots.GetAsync(x => x.ApplicationSnapshotId == item.ApplicationSnapshotId && x.Type == DocumentType.ProfilePhoto);

                var imgURL = await imgURLQuery.Select(x => x.DocumentURL).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (!string.IsNullOrEmpty(imgURL)) item.ImgURL = imgURL;
            }

            return new Response<PaginatedList<GetActivityResponse>>(code: (int)ResponseCode.SUCCESS,
                data: roundActivities,
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
