namespace CabinCrewCareer.Application.Locations.Common
{
    public class EditLocationRequest
    {
        public string Address { get; set; } = string.Empty;

        public int WardId { get; set; }
    }
}
