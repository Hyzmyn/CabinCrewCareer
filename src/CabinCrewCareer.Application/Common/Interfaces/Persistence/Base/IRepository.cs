using CabinCrewCareer.Domain.Entities.Base.Implementations;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace CabinCrewCareer.Application.Common.Interfaces.Persistence.Base
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IQueryable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? expression = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null,
            bool disableTracking = true);

        Task<IQueryable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? expression = null);

        Task<IQueryable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? expression = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null,
            bool disableTracking = true);

        Task<TEntity?> GetByIdAsync(object id);

        Task<TEntity> AddAsync(TEntity entity);

        Task AddWithNoTrackingAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);

        Task AddRangeAsync(IEnumerable<TEntity> entities);

        Task DeleteRangeAsync(IEnumerable<TEntity> entities);

        Task UpdateRangeAsync(IEnumerable<TEntity> entities);

        Task DeleteAsync(object id);

        bool Any(Expression<Func<TEntity, bool>> predicate);

        Task<BasePaginationEntity<TEntity>> PaginationAsync(int page = 0,
        int pageSize = 20,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includeFunc = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
         CancellationToken cancellationToken = default);
    }
}
