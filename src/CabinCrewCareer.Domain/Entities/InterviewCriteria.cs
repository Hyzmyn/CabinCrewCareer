using CabinCrewCareer.Domain.Common;

namespace CabinCrewCareer.Domain.Entities
{
    public class InterviewCriteria
    {
        public int InterviewCriteriaId { get; set; }

        public string Title { get; set; } = string.Empty;

        public CampaignType Type { get; set; }

        public bool Status { get; set; } = false;

        public DateOnly? EffectiveDate { get; set; }

        // Foreign Keys

        // Navigation Properties
        public virtual ICollection<InterviewCriteriaItem> InterviewCriteriaItems { get; set; } = [];
    }
}
