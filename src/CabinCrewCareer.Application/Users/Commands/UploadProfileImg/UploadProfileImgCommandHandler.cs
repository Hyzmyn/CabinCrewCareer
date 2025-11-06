using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Commands.UploadProfileImg
{
    public class UploadProfileImgCommandHandler(
        IUnitOfWork unitOfWork,
        IUploadFileService uploadFileService,
        ICurrentUserService currentUserService) : IRequestHandler<UploadProfileImgCommand, Response<GetUserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<GetUserResponse>> Handle(UploadProfileImgCommand request, CancellationToken cancellationToken)
        {
            var fileExtension = Path.GetExtension(request.File.FileName);
            switch (fileExtension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                    break;
                default:
                    throw new ApiException(ResponseCode.INVALID_FILE_EXTENSION);
            }

            long size = request.File.Length;
            if (size > 5 * 1024 * 1024)
                throw new ApiException(ResponseCode.INVALID_FILE_SIZE);

            string imgUrl = await _uploadFileService.CloudinaryStorage(request.File);

            string userId = _currentUserService.CurrentPrincipal ?? throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);

            var userQuery = await _unitOfWork.Users.GetAsync(expression: u => u.UserId == Int32.Parse(userId), disableTracking: true);

            var user = userQuery.FirstOrDefault() ?? throw new ApiException(ResponseCode.USER_NOT_FOUND);

            user.ImgURL = imgUrl;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return new Response<GetUserResponse>(code: (int)ResponseCode.UPDATED_SUCCESS, message: ResponseCode.UPDATED_SUCCESS.ToString());
        }
    }
}
