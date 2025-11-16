using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using System.Text.Json.Serialization;

namespace CabinCrewCareer.Application.CampaignAssignments.Commands.CancelAssignment
{
    /// <summary>
    /// Command để Senior cancel assignment cho subordinate
    /// </summary>
    public class CancelAssignmentCommand : IRequest<Response<bool>>
    {
        [JsonIgnore]
        public int AssignmentId { get; set; }
        public string CancelReason { get; set; } = string.Empty;
    }
}
