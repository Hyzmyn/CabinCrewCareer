using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
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
        IMapper mapper) : IRequestHandler<GetRoundActivitiesQuery, Response<PaginatedList<GetActivityResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<PaginatedList<GetActivityResponse>>> Handle(GetRoundActivitiesQuery request, CancellationToken cancellationToken)
        {
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
