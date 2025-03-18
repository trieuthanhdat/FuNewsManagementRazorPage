using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Services
{
    public class NewsService : INewsService
    {
        private readonly IGenericRepository<NewsArticle> _newsRepository;

        public NewsService(IGenericRepository<NewsArticle> newsRepository)
        {
            _newsRepository = newsRepository;
        }

        // Get All News Articles
        public async Task<IEnumerable<NewsArticle>> GetAllNewsAsync() =>
            await _newsRepository.GetAllAsync();

        // Add New Article
        public async Task AddNewsAsync(NewsArticle newsArticle)
        {
            newsArticle.CreatedDate = DateTime.Now;
            await _newsRepository.AddAsync(newsArticle);
        }
        // Get News Report for a Specific Date Range
        public async Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var newsList = await _newsRepository.GetAllAsync();
            return newsList.Where(n => n.CreatedDate >= startDate && n.CreatedDate <= endDate);
        }

        // Get News Count by Author (for detailed reports)
        public async Task<Dictionary<string, int>> GetNewsCountByAuthorAsync()
        {
            var newsList = await _newsRepository.GetAllAsync();
            return newsList.GroupBy(n => n.CreatedBy.AccountName)
                           .ToDictionary(g => g.Key, g => g.Count());
        }
     
    }

}
