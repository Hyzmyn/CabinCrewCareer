using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EnquiryRequests.Commands.CreateEnquiryRequest
{
    public class CreateEnquiryRequestCommand : IRequest<Response<int>>
    {
        public int TestSessionId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool Status { get; set; }
    }
}
