using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Common.Models
{
    public class PaginatedList<T>(List<T> items, int currentPage, int pageSize, int totalRecords)
    {
        public List<T> Items { get; set; } = items;

        public int CurrentPage { get; set; } = currentPage;

        public int PageSize { get; set; } = pageSize;

        public int TotalRecords { get; set; } = totalRecords;

        public int TotalPages { get; set; }

        public bool HasNextPage => CurrentPage * PageSize < TotalRecords;

        public bool HasPreviousPage => CurrentPage > 1;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
        {
            var totalRecords = await query.CountAsync();

            var totalPages = ((double)totalRecords / (double)pageSize);

            int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var paginatedList = new PaginatedList<T>(items, page, pageSize, totalRecords)
            {
                TotalPages = roundedTotalPages
            };

            return paginatedList;
        }
    }
}
