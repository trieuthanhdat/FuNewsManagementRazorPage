using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Interfaces
{
    public interface INewsService
    {
        // News Retrieval
        Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<NewsArticle>> GetAllNewsAsync();
        Task<IEnumerable<NewsArticleDTO>> GetNewsByAuthorAsync(short authorId);
        Task<NewsArticleDTO> GetNewsByIdAsync(int newsId);
        Task<NewsArticleDTO> GetNewsByIdAsync(string newsId);

        // News Analytics
        Task<Dictionary<string, int>> GetNewsCountByDateAsync();
        Task<Dictionary<string, int>> GetNewsCountByCategoryAsync();
        Task<Dictionary<string, int>> GetNewsCountByAuthorAsync();

        // News Management (CRUD)
        Task<bool> AddNewsAsync(NewsArticleDTO newsArticle);
        Task<bool> UpdateNewsAsync(NewsArticleDTO newsArticle);
        Task<bool> DeleteNewsAsync(int newsId);
    }

}
