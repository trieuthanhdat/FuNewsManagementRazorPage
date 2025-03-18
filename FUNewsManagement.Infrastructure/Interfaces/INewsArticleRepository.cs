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
        Task<NewsArticle> GetNewsByIdAsync(int id);
        Task<int> DeleteAsync(int newsArticleID);
        Task<IEnumerable<NewsArticleDTO>> GetAllNewsAsync();
        //Task<int> GetMaxArticleIdAsync();
        Task<List<NewsArticle>> GetNewsByAuthorAsync(int authorId);
        Task<List<NewsArticle>> GetNewsByCategoryIdAsync(int categoryId);
    }
}
