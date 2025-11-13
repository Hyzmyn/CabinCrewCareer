using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Tests.Commands.UpdateTest
{
    public class UpdateTestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IFirebaseStorageService firebaseStorageService) : IRequestHandler<UpdateTestCommand, Response<GetTestResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
        private readonly IFirebaseStorageService _firebaseStorageService = firebaseStorageService;

        public async Task<Response<GetTestResponse>> Handle(UpdateTestCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new UpdateTestCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // Get current user from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            // Get user and check role
            var user = await _unitOfWork.Users.GetByIdAsync(int.Parse(currentUserId));
            if (user == null)
            {
                throw new ApiException(ResponseCode.USER_NOT_FOUND);
            }

            // Check if user is Examiner
            if (user.RoleId != (int)AvailableRole.Examiner)
            {
                throw new ApiException(ResponseCode.UNAUTHORIZED_EXAMINER_ONLY);
            }

            // Get existing test
            var test = await _unitOfWork.Tests.GetByIdAsync(request.TestId);
            if (test == null)
            {
                throw new ApiException(ResponseCode.TEST_NOT_FOUND);
            }

            // Handle audio file update
            string? audioFileUrl = test.AudioFileURL;
            
            // Remove existing audio if requested
            if (request.RemoveAudio && !string.IsNullOrEmpty(test.AudioFileURL))
            {
                await _firebaseStorageService.DeleteFileAsync(test.AudioFileURL);
                audioFileUrl = null;
            }
            
            // Upload new audio file if provided
            if (request.AudioFile != null)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(test.AudioFileURL))
                {
                    await _firebaseStorageService.DeleteFileAsync(test.AudioFileURL);
                }

                using var stream = request.AudioFile.OpenReadStream();
                audioFileUrl = await _firebaseStorageService.UploadAudioFileAsync(
                    stream,
                    request.AudioFile.FileName,
                    request.AudioFile.ContentType);
            }

            // Set audio to null for EnglishSpeaking and Practical tests if no file provided
            if ((request.TestType == TestType.EnglishSpeaking || request.TestType == TestType.Practical) 
                && request.AudioFile == null && !request.RemoveAudio)
            {
                // Keep existing audio or set to null
                audioFileUrl = audioFileUrl ?? null;
            }

            // Update test
            test.TestName = request.TestName;
            test.Purpose = request.Purpose;
            test.TestType = request.TestType;
            test.MaxScore = request.MaxScore;
            test.DurationInMinutes = request.DurationInMinutes;
            test.AudioFileURL = audioFileUrl ?? string.Empty;
            test.UpdatedBy = user.FullName;
            test.UpdatedAt = _dateTimeProvider.UtcNow;

            await _unitOfWork.Tests.UpdateAsync(test);
            await _unitOfWork.CompleteAsync();

            // Create response
            var response = new GetTestResponse
            {
                TestId = test.TestId,
                TestName = test.TestName,
                Purpose = test.Purpose,
                TestType = test.TestType.ToString(),
                MaxScore = test.MaxScore,
                DurationInMinutes = test.DurationInMinutes,
                AudioFileURL = string.IsNullOrEmpty(test.AudioFileURL) ? null : test.AudioFileURL,
                JoinCode = test.JoinCode,
                CreatedAt = test.CreatedAt?.GetFormatDateTime(),
                CreatedBy = test.CreatedBy
            };

            return new Response<GetTestResponse>(
                data: response,
                code: (int)ResponseCode.UPDATED_SUCCESS,
                message: ResponseCode.UPDATED_SUCCESS.GetDescription());
        }
    }
}