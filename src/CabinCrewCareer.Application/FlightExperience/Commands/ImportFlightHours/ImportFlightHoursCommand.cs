using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CabinCrewCareer.Application.FlightExperience.Commands.ImportFlightHours
{
    /// <summary>
    /// Command to import flight hours confirmation results from Excel file
    /// </summary>
    public class ImportFlightHoursCommand : IRequest<Response<string>>
    {
        /// <summary>
        /// The ID of the round to import data for
        /// </summary>
        public int RoundId { get; set; }

        /// <summary>
        /// The Excel file containing flight hours confirmation results
        /// </summary>
        public IFormFile File { get; set; } = null!;
    }
}
