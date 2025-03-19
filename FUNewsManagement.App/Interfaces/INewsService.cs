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
        Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<NewsArticle>> GetAllNewsAsync();
        Task<Dictionary<string, int>> GetNewsCountByDateAsync();
        Task<Dictionary<string, int>> GetNewsCountByCategoryAsync();
        Task<Dictionary<string, int>> GetNewsCountByAuthorAsync();
        Task AddNewsAsync(NewsArticle newsArticle);
    }
}
