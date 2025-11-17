using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CabinCrewCareer.Application.TestQuestions.Commands.ImportQuestionsFromExcel
{
    public record ImportQuestionsFromExcelCommand(
        int TestId,
        IFormFile ExcelFile) : IRequest<Response<CreateBulkTestQuestionsResponse>>;
}
