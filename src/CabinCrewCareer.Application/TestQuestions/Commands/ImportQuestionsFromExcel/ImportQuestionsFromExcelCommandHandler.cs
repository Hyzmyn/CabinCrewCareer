using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using OfficeOpenXml;

namespace CabinCrewCareer.Application.TestQuestions.Commands.ImportQuestionsFromExcel
{
    public class ImportQuestionsFromExcelCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<ImportQuestionsFromExcelCommand, Response<CreateBulkTestQuestionsResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<CreateBulkTestQuestionsResponse>> Handle(ImportQuestionsFromExcelCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new ImportQuestionsFromExcelCommandValidator();
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

            // Check if test exists
            var test = await _unitOfWork.Tests.GetByIdAsync(request.TestId);
            if (test == null)
            {
                throw new ApiException(ResponseCode.TEST_NOT_FOUND);
            }

            // Get existing questions to validate total score
            var existingQuestions = await _unitOfWork.TestQuestions.GetAllAsync(q => q.TestId == request.TestId);
            var currentTotalScore = existingQuestions.Sum(q => q.Score);

            // Parse Excel file
            List<QuestionCreatedDto> createdQuestions = [];
            int newQuestionsScore = 0;
            
            try
            {
                // License is set globally in Program.cs
                using var stream = request.ExcelFile.OpenReadStream();
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0]; // Get first worksheet

                if (worksheet == null || worksheet.Dimension == null)
                {
                    errorMessages.Add("Excel file is empty or invalid");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                int questionRow = 2; // Start from row 2 (row 1 is header)
                
                while (questionRow <= worksheet.Dimension.End.Row)
                {
                    // Check if row has data
                    var orderNumberValue = worksheet.Cells[questionRow, 1].Value;
                    if (orderNumberValue == null)
                        break;

                    try
                    {
                        // A1: Order Number
                        if (!int.TryParse(orderNumberValue.ToString(), out int orderNumber))
                        {
                            errorMessages.Add($"Row {questionRow}: Invalid order number");
                            questionRow++;
                            continue;
                        }

                        // A2: Question Content
                        var questionContent = worksheet.Cells[questionRow, 2].Value?.ToString();
                        if (string.IsNullOrWhiteSpace(questionContent))
                        {
                            errorMessages.Add($"Row {questionRow}: Question content is required");
                            questionRow++;
                            continue;
                        }

                        // A3: Score
                        if (!int.TryParse(worksheet.Cells[questionRow, 3].Value?.ToString(), out int score) || score <= 0 || score > 100)
                        {
                            errorMessages.Add($"Row {questionRow}: Invalid score (must be 1-100)");
                            questionRow++;
                            continue;
                        }

                        // A4-A9: Options
                        List<TestOption> options = [];
                        string? correctAnswerText = null;

                        // Read options from columns 4-9 (A4-A9)
                        for (int col = 4; col <= 9; col++)
                        {
                            var optionContent = worksheet.Cells[questionRow, col].Value?.ToString();
                            if (string.IsNullOrWhiteSpace(optionContent))
                                continue; // Skip empty options

                            // Check if text is BOLD
                            bool isBold = worksheet.Cells[questionRow, col].Style.Font.Bold;
                            
                            options.Add(new TestOption
                            {
                                OptionContent = optionContent.Trim(),
                                IsCorrect = isBold // If bold = correct answer
                            });
                        }

                        // A10: Correct answer (if bold detection failed)
                        var correctAnswerCell = worksheet.Cells[questionRow, 10].Value?.ToString();
                        if (!string.IsNullOrWhiteSpace(correctAnswerCell))
                        {
                            correctAnswerText = correctAnswerCell.Trim();
                        }

                        // If no bold option found, use A10 to find correct answer
                        if (options.Any() && !options.Any(o => o.IsCorrect) && !string.IsNullOrWhiteSpace(correctAnswerText))
                        {
                            var correctOption = options.FirstOrDefault(o => 
                                o.OptionContent.Equals(correctAnswerText, StringComparison.OrdinalIgnoreCase));
                            
                            if (correctOption != null)
                            {
                                correctOption.IsCorrect = true;
                            }
                            else
                            {
                                errorMessages.Add($"Row {questionRow}: Correct answer '{correctAnswerText}' not found in options");
                                questionRow++;
                                continue;
                            }
                        }

                        // Validate based on test type
                        if (test.TestType == TestType.EnglishListening || test.TestType == TestType.Practical)
                        {
                            if (options.Count < 4)
                            {
                                errorMessages.Add($"Row {questionRow}: At least 4 options required for {test.TestType} test");
                                questionRow++;
                                continue;
                            }

                            if (options.Count > 6)
                            {
                                errorMessages.Add($"Row {questionRow}: Maximum 6 options allowed");
                                questionRow++;
                                continue;
                            }

                            if (options.Count(o => o.IsCorrect) != 1)
                            {
                                errorMessages.Add($"Row {questionRow}: Exactly one correct answer required");
                                questionRow++;
                                continue;
                            }
                        }
                        else if (test.TestType == TestType.EnglishSpeaking)
                        {
                            if (options.Any())
                            {
                                errorMessages.Add($"Row {questionRow}: {test.TestType} test should not have options");
                                questionRow++;
                                continue;
                            }
                        }

                        // Create TestQuestion
                        var testQuestion = new TestQuestion
                        {
                            QuestionContent = questionContent,
                            Score = score,
                            OrderNumber = orderNumber,
                            TestId = request.TestId
                        };

                        await _unitOfWork.TestQuestions.AddAsync(testQuestion);
                        await _unitOfWork.CompleteAsync();

                        // Track new questions score
                        newQuestionsScore += score;

                        // Create TestOptions
                        List<TestOptionResponse> optionResponses = [];
                        foreach (var option in options)
                        {
                            option.QuestionId = testQuestion.QuestionId;
                            await _unitOfWork.TestOptions.AddAsync(option);
                            await _unitOfWork.CompleteAsync();

                            optionResponses.Add(new TestOptionResponse
                            {
                                OptionId = option.OptionId,
                                OptionContent = option.OptionContent,
                                IsCorrect = option.IsCorrect
                            });
                        }

                        createdQuestions.Add(new QuestionCreatedDto
                        {
                            QuestionId = testQuestion.QuestionId,
                            QuestionContent = testQuestion.QuestionContent,
                            Score = testQuestion.Score,
                            OrderNumber = testQuestion.OrderNumber,
                            Options = optionResponses
                        });

                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add($"Row {questionRow}: {ex.Message}");
                    }

                    questionRow++;
                }

                if (!createdQuestions.Any() && errorMessages.Any())
                {
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                // Validate total score constraint after importing all questions
                var finalTotalScore = currentTotalScore + newQuestionsScore;
                if (finalTotalScore != test.MaxScore)
                {
                    // Rollback: delete all created questions
                    foreach (var createdQ in createdQuestions)
                    {
                        var questionToDelete = await _unitOfWork.TestQuestions.GetByIdAsync(createdQ.QuestionId);
                        if (questionToDelete != null)
                        {
                            await _unitOfWork.TestQuestions.DeleteAsync(questionToDelete);
                        }
                    }
                    await _unitOfWork.CompleteAsync();

                    errorMessages.Add($"Total score of all questions must equal test MaxScore ({test.MaxScore}). Current total: {currentTotalScore}, imported questions: {newQuestionsScore}, final total: {finalTotalScore}");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Error reading Excel file: {ex.Message}");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Create response
            var response = new CreateBulkTestQuestionsResponse
            {
                TestId = test.TestId,
                TestName = test.TestName,
                TestType = test.TestType.ToString(),
                TotalQuestionsCreated = createdQuestions.Count,
                Questions = createdQuestions
            };

            return new Response<CreateBulkTestQuestionsResponse>(
                data: response,
                code: (int)ResponseCode.CREATED_SUCCESS,
                message: $"{createdQuestions.Count} questions imported successfully. {(errorMessages.Any() ? $"Warnings: {string.Join(", ", errorMessages)}" : "")}");
        }
    }
}
