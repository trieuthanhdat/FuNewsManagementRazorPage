using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Domain;
using FUNewsManagement.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FUNewsManagement.Domain.DTOs;

namespace FUNewsManagement.Infrastructure.Repositories
{
    public class NewsArticleRepository : GenericRepository<NewsArticle>, INewsArticleRepository
    {
        private readonly FunewsManagementContext _context;

        public NewsArticleRepository(FunewsManagementContext context) : base(context)
        {
            _context = context;
        }

        // Get News by Category ID
        public async Task<List<NewsArticle>> GetNewsByCategoryIdAsync(short categoryId)
        {
            return await _context.NewsArticles
                .Where(n => n.CategoryId.GetValueOrDefault(-1) == categoryId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        // Get Active News Articles
        public async Task<IEnumerable<NewsArticle>> GetActiveNewsAsync()
        {
            return await GetAsync(
                filter: n => n.NewsStatus.HasValue && n.NewsStatus.Value,
                orderBy: q => q.OrderByDescending(n => n.CreatedDate)
            );
        }

        // Get News by Author ID
        public async Task<List<NewsArticle>> GetNewsByAuthorAsync(short authorId)
        {
            return await _context.NewsArticles
                .Where(n => n.CreatedById.GetValueOrDefault(-1) == authorId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        // Get News by Date Range (For Reports)
        public async Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetAsync(
                filter: n => n.CreatedDate >= startDate && n.CreatedDate <= endDate,
                orderBy: q => q.OrderByDescending(n => n.CreatedDate),
                include: q => q.Include(n => n.CreatedBy).Include(n => n.Category)
            );
        }

        // Search News Articles (Title, Category, Date)
        public async Task<IEnumerable<NewsArticle>> SearchNewsAsync(string keyword, int? categoryId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.NewsArticles.AsQueryable();

            query = query.Where(n => n != null && n.NewsStatus.Value); // Only active news

            if (startDate.HasValue && endDate.HasValue)
                query = query.Where(n => n.CreatedDate >= startDate.Value && n.CreatedDate <= endDate.Value);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(n => n.NewsTitle.Contains(keyword) || n.Headline.Contains(keyword));

            return await query.OrderByDescending(n => n.CreatedDate).ToListAsync();
        }

        // Delete News Article
        public async Task<bool> DeleteAsync(int newsArticleId)
        {
            var newsArticle = await _context.NewsArticles.FindAsync(newsArticleId);
            if (newsArticle == null)
                return false; // Not found

            _context.NewsArticles.Remove(newsArticle);
            await _context.SaveChangesAsync();

            return true; // Success
        }

        // Get All News (Including Category & Author)
        public async Task<IEnumerable<NewsArticleDTO>> GetAllNewsAsync()
        {
            return await _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Select(news => new NewsArticleDTO
                {
                    NewsArticleID = news.NewsArticleId,
                    NewsTitle = news.NewsTitle != null ? news.NewsTitle.ToString() : "",
                    Headline = news.Headline,
                    CreatedDate = news.CreatedDate != null ? news.CreatedDate.Value : DateTime.UtcNow,
                    NewsContent = news.NewsContent ?? "",
                    NewsSource = news.NewsSource,
                    CategoryID = news.CategoryId ?? 0,
                    CategoryName = news.Category != null ? news.Category.CategoryName : "Uncategorized",
                    NewsStatus = news.NewsStatus != null ? news.NewsStatus.Value : false,
                    CreatedByID = news.CreatedBy != null ? news.CreatedBy.AccountId : (short?)-1
                })
                .ToListAsync();
        }

        // Get News Count by Author (For Reports)
        public async Task<Dictionary<string, int>> GetNewsCountByAuthorAsync()
        {
            var newsList = await GetAllAsync(include: q => q.Include(n => n.CreatedBy));
            return newsList.GroupBy(n => n.CreatedBy?.AccountName ?? "Unknown Author")
                           .ToDictionary(g => g.Key, g => g.Count());
        }
        // Get News Count by Date (For Reports)
        public async Task<Dictionary<string, int>> GetNewsCountByDateAsync()
        {
            var newsList = await GetAllAsync();
            return newsList.GroupBy(n => n.CreatedDate?.ToString("yyyy-MM-dd") ?? "Unknown Date")
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        // Get News Count by Category (For Reports)
        public async Task<Dictionary<string, int>> GetNewsCountByCategoryAsync()
        {
            var newsList = await GetAllAsync(include: q => q.Include(n => n.Category));
            return newsList.GroupBy(n => n.Category?.CategoryName ?? "Uncategorized")
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        // Fetch a Single News Article by ID
        public async Task<NewsArticle> GetNewsByIdAsync(string id)
        {
            return await _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .FirstOrDefaultAsync(n => n.NewsArticleId == id);
        }
    }
}
