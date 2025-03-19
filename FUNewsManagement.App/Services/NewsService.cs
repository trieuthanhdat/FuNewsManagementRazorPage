using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        // Get All News Articles (For Admin Dashboard)
        public async Task<IEnumerable<NewsArticle>> GetAllNewsAsync() =>
            await _newsRepository.GetAllAsync(include: q => q.Include(n => n.CreatedBy).Include(n => n.Category));

        // Add New Article
        public async Task AddNewsAsync(NewsArticle newsArticle)
        {
            newsArticle.CreatedDate = DateTime.UtcNow; // Use UTC for consistency
            await _newsRepository.AddAsync(newsArticle);
        }

        // Get News Report for a Specific Date Range (SORTED DESCENDING)
        public async Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _newsRepository.GetAsync(
                filter: n => n.CreatedDate >= startDate && n.CreatedDate <= endDate,
                orderBy: q => q.OrderByDescending(n => n.CreatedDate),
                include: q => q.Include(n => n.CreatedBy).Include(n => n.Category)
            );
        }

        // Get News Count by Author (For Reports)
        public async Task<Dictionary<string, int>> GetNewsCountByAuthorAsync()
        {
            var newsList = await _newsRepository.GetAllAsync(include: q => q.Include(n => n.CreatedBy));

            return newsList.GroupBy(n => n.CreatedBy?.AccountName ?? "Unknown Author")
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        // Get News Count by Date (For Graphs & Analytics)
        public async Task<Dictionary<string, int>> GetNewsCountByDateAsync()
        {
            var newsList = await _newsRepository.GetAllAsync();

            return newsList.GroupBy(n => n.CreatedDate?.ToString("yyyy-MM-dd") ?? "Unknown Date")
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        // Get News Count by Category (For Detailed Reports)
        public async Task<Dictionary<string, int>> GetNewsCountByCategoryAsync()
        {
            var newsList = await _newsRepository.GetAllAsync(include: q => q.Include(n => n.Category));

            return newsList.GroupBy(n => n.Category?.CategoryName ?? "Uncategorized")
                           .ToDictionary(g => g.Key, g => g.Count());
        }
    }

}
