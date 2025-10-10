namespace CabinCrewCareer.Domain.Entities.Base.Implementations
{
    public class BasePaginationEntity<TEntity> where TEntity : class
    {
        public long Total { get; set; }
        public List<TEntity>? Data { get; set; }
    }
}
