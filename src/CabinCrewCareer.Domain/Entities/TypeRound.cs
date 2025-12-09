using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class TypeRound
    {
        public int RoundTypeId { get; set; }

        public string RoundTypeName { get; set; } = string.Empty;

        public CampaignType CampaignType { get; set; }

        public virtual ICollection<Round> Rounds { get; set; } = [];
    }
}
