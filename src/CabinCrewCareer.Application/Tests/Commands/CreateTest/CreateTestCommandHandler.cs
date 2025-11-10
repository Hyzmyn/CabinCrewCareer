using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace CabinCrewCareer.Application.Tests.Commands.CreateTest
{
    public class CreateTestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IFirebaseStorageService firebaseStorageService) : IRequestHandler<CreateTestCommand, Response<GetTestResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
        private readonly IFirebaseStorageService _firebaseStorageService = firebaseStorageService;
        private const string JoinCodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Loại bỏ các ký tự dễ nhầm lẫn

        public async Task<Response<GetTestResponse>> Handle(CreateTestCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new CreateTestCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
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

            // Generate unique join code
            string joinCode = await GenerateUniqueJoinCode(cancellationToken);

            // Upload audio file to Firebase if provided
            string? audioFileUrl = null;
            if (request.TestType == TestType.English && request.AudioFile != null)
            {
                using var stream = request.AudioFile.OpenReadStream();
                audioFileUrl = await _firebaseStorageService.UploadAudioFileAsync(
                    stream,
                    request.AudioFile.FileName,
                    request.AudioFile.ContentType);
            }

            // Create test
            var test = new Test
            {
                TestName = request.TestName,
                Purpose = request.Purpose,
                TestType = request.TestType,
                MaxScore = request.MaxScore,
                AudioFileURL = audioFileUrl ?? string.Empty,
                JoinCode = joinCode,
                CreatedBy = user.FullName,
                CreatedAt = _dateTimeProvider.UtcNow
            };

            await _unitOfWork.Tests.AddAsync(test);
            await _unitOfWork.CompleteAsync();

            // Create response
            var response = new GetTestResponse
            {
                TestId = test.TestId,
                TestName = test.TestName,
                Purpose = test.Purpose,
                TestType = test.TestType.ToString(),
                MaxScore = test.MaxScore,
                AudioFileURL = string.IsNullOrEmpty(test.AudioFileURL) ? null : test.AudioFileURL,
                JoinCode = test.JoinCode,
                CreatedAt = test.CreatedAt?.GetFormatDateTime(),
                CreatedBy = test.CreatedBy
            };

            return new Response<GetTestResponse>(
                data: response,
                code: (int)ResponseCode.CREATED_SUCCESS,
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }

        /// <summary>
        /// Generate unique random join code with 10 characters (letters and numbers)
        /// </summary>
        private async Task<string> GenerateUniqueJoinCode(CancellationToken cancellationToken)
        {
            const int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                string joinCode = GenerateRandomJoinCode(10);

                // Check if join code already exists
                var existingTest = await _unitOfWork.Tests.GetAsync(
                    t => t.JoinCode == joinCode,
                    disableTracking: true);

                if (!existingTest.Any())
                {
                    return joinCode;
                }

                attempts++;
            }

            throw new ApiException(ResponseCode.DUPLICATE_JOIN_CODE);
        }

        /// <summary>
        /// Generate random alphanumeric string
        /// </summary>
        private static string GenerateRandomJoinCode(int length)
        {
            var result = new StringBuilder(length);
            var randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            foreach (byte b in randomBytes)
            {
                result.Append(JoinCodeCharacters[b % JoinCodeCharacters.Length]);
            }

            return result.ToString();
        }
    }
}
