using MediatR;
using Microsoft.AspNetCore.Http;
using CabinCrewCareer.Domain.Wrappers;

namespace CabinCrewCareer.Application.Rounds.Commands.ImportFinalRoundResult
{
    public class ImportFinalRoundResultCommand : IRequest<Response<string>>
    {
        public int RoundId { get; set; }
        public IFormFile File { get; set; }
        public ImportFinalRoundResultCommand(int roundId, IFormFile file)
        {
            RoundId = roundId;
            File = file;
        }
    }
}
