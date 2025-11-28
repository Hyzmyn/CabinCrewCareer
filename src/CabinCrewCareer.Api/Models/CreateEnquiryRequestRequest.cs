namespace CabinCrewCareer.Api.Models
{
    public class CreateEnquiryRequestRequest
    {
        /// <summary>
        /// Test session ID to create enquiry request for
        /// </summary>
        public int TestSessionId { get; set; }

        /// <summary>
        /// Reason for the enquiry request
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Status of the enquiry request (true = approved, false = pending)
        /// </summary>
        public bool Status { get; set; }
    }
}
