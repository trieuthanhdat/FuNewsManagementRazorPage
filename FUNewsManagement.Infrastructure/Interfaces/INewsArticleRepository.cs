using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.Infrastructure.Interfaces
{
    public interface INewsArticleRepository : IGenericRepository<NewsArticle>
    {
        Task<IEnumerable<NewsArticle>> GetActiveNewsAsync();
        Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<NewsArticle>> SearchNewsAsync(string keyword, int? categoryId, DateTime? startDate, DateTime? endDate);
        Task<NewsArticle> GetNewsByIdAsync(string id);
        Task<bool> DeleteAsync(int newsArticleID); // Changed to bool for success/failure
        Task<IEnumerable<NewsArticleDTO>> GetAllNewsAsync();

        // Report-Specific Queries
        Task<Dictionary<string, int>> GetNewsCountByAuthorAsync();
        Task<Dictionary<string, int>> GetNewsCountByDateAsync();
        Task<Dictionary<string, int>> GetNewsCountByCategoryAsync();

        // Get News by Author & Category
        Task<List<NewsArticle>> GetNewsByAuthorAsync(short authorId);
        Task<List<NewsArticle>> GetNewsByCategoryIdAsync(short categoryId);
    }

}
