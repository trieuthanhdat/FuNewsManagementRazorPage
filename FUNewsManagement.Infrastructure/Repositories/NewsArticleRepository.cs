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
   
        public async Task<List<NewsArticle>> GetNewsByCategoryIdAsync(int categoryId)
        {
            return await _context.NewsArticles
                .Where(n => n.CategoryId == categoryId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<NewsArticle>> GetActiveNewsAsync()
        {
            return await _context.NewsArticles
                .Where(n => n != null &&  n.NewsStatus.Value) // 1 = Active, 0 = Inactive
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }
        public async Task<List<NewsArticle>> GetNewsByAuthorAsync(int authorId)
        {
            return await _context.NewsArticles
                .Where(n => n.CreatedById == authorId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.NewsArticles
                .Where(n => n.CreatedDate >= startDate && n.CreatedDate <= endDate)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        // Search News Articles (Title, Category, Date)
        public async Task<IEnumerable<NewsArticle>> SearchNewsAsync(string keyword, int? categoryId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.NewsArticles.AsQueryable();

            // Ensure NewsStatus is explicitly checked against int values
            query = query.Where(n => n != null && n.NewsStatus.Value); // Only active news

            if (startDate.HasValue && endDate.HasValue)
                query = query.Where(n => n.CreatedDate >= startDate.Value && n.CreatedDate <= endDate.Value);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(n => n.NewsTitle.Contains(keyword) || n.Headline.Contains(keyword));

            return await query.OrderByDescending(n => n.CreatedDate).ToListAsync();

        }
        public async Task<int> DeleteAsync(int newsArticleId)
        {
            var newsArticle = await _context.NewsArticles.FindAsync(newsArticleId);
            if (newsArticle == null)
                return 1; // News article not found

            _context.NewsArticles.Remove(newsArticle);
            await _context.SaveChangesAsync();

            return 0; // Success
        }
        public async Task<IEnumerable<NewsArticleDTO>> GetAllNewsAsync()
        {
            return await _context.NewsArticles
                .Join(_context.Categories,
                      news => news.CategoryId,
                      category => category.CategoryId,
                      (news, category) => new NewsArticleDTO
                      {
                          NewsArticleID = news.NewsArticleId,
                          NewsTitle = news.NewsTitle,
                          Headline = news.Headline,
                          CreatedDate = news.CreatedDate.Value,
                          NewsContent = news.NewsContent,
                          NewsSource = news.NewsSource,
                          CategoryID = news.CategoryId.Value,
                          CategoryName = category.CategoryName, // Get Category Name
                          NewsStatus = news.NewsStatus.Value
                      })
                .ToListAsync();
        }


        //Fetch a Single News Article by ID
        public async Task<NewsArticle> GetNewsByIdAsync(int id)
        {
            return await _context.NewsArticles.FindAsync(id);
        }
    }
}
