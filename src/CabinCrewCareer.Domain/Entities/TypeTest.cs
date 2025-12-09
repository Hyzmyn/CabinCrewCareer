namespace CabinCrewCareer.Domain.Entities
{
    public class TypeTest
    {
        public int TestTypeId { get; set; }

        public string TestTypeName { get; set; } = string.Empty;

        public virtual ICollection<Test> Tests { get; set; } = [];
    }
}
