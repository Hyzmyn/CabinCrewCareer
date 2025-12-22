using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class TestRepository(
        CabinCrewCareerDbContext dbContext,
        ILogger logger) : Repository<Test>(dbContext, logger), ITestRepository
    {
        public async Task<PaginatedList<Test>> PaginatedListAsync(
            string? searchTerm,
            string? sortColumn,
            string? sortOrder,
            int? testType,
            int page,
            int pageSize)
        {
            IQueryable<Test> testsQuery = _dbContext.Tests;

            // Search by test name or purpose
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                testsQuery = testsQuery.Where(t =>
                    t.TestName.Contains(searchTerm) ||
                    t.Purpose.Contains(searchTerm));
            }

            // Filter by test type
            if (testType != null)
            {
                testsQuery = testsQuery.Where(t => t.TestType == (TestType)testType);
            }

            // Sorting
            if (sortOrder?.ToLower() == "asc")
            {
                testsQuery = testsQuery.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                testsQuery = testsQuery.OrderByDescending(GetSortProperty(sortColumn));
            }

            var tests = await PaginatedList<Test>.CreateAsync(testsQuery, page, pageSize);

            return tests;
        }

        private static Expression<Func<Test, object>> GetSortProperty(string? sortColumn)
            => sortColumn?.ToLower() switch
            {
                "name" => test => test.TestName,
                "testtype" => test => test.TestType,
                "maxscore" => test => test.MaxScore,
                "duration" => test => test.DurationInMinutes,
                "createdat" => test => test.CreatedAt ?? DateTime.MinValue,
                _ => test => test.TestId
            };

        public async Task<List<Test>> GetTestsAsync(int? testType)
        {
            IQueryable<Test> testsQuery = _dbContext.Tests
                .Select(t => new Test
                {
                    TestId = t.TestId,
                    TestName = t.TestName,
                    TestType = t.TestType,
                    TestQuestions = t.TestQuestions.Select(t => new TestQuestion
                    {
                        QuestionId = t.QuestionId
                    }).ToList()
                });

            if (testType != null)
            {
                testsQuery = testsQuery.OrderByDescending(t => t.TestId).Where(t => t.TestType == (TestType)testType);
            }

            return await testsQuery.ToListAsync();
        }
    }
}
