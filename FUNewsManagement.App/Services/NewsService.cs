using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace FUNewsManagement.App.Services
{
    public class NewsService : INewsService
    {
        private readonly IGenericRepository<NewsArticle> _newsRepository;
        private readonly IGenericRepository<Tag> _tagRepository;
        public NewsService(IGenericRepository<NewsArticle> newsRepository, IGenericRepository<Tag> tagRepository)
        {
            _newsRepository = newsRepository;
            _tagRepository = tagRepository;
        }

        // Get All News Articles (For Admin Dashboard)
        public async Task<IEnumerable<NewsArticle>> GetAllNewsAsync() =>
            await _newsRepository.GetAllAsync(include: q => q.Include(n => n.CreatedBy).Include(n => n.Category));

        // Add New Article
        public async Task<bool> AddNewsAsync(NewsArticleDTO newsArticleDto)
        {
            var newsArticle = new NewsArticle
            {
                NewsTitle = newsArticleDto.NewsTitle,
                Headline = newsArticleDto.Headline,
                CreatedDate = DateTime.UtcNow,
                NewsContent = newsArticleDto.NewsContent,
                NewsSource = newsArticleDto.NewsSource,
                CategoryId = newsArticleDto.CategoryID,
                NewsStatus = newsArticleDto.NewsStatus,
                CreatedById = newsArticleDto.CreatedByID,
                ModifiedDate = DateTime.UtcNow,
                Tags = new List<Tag>() // Initialize tag list
            };

            // Handle Tags
            if (!string.IsNullOrEmpty(newsArticleDto.TagNames))
            {
                var tagNames = newsArticleDto.TagNames.Split(',')
                    .Select(tag => tag.Trim())
                    .Distinct()
                    .ToList();

                foreach (var tagName in tagNames)
                {
                    var existingTag = await _tagRepository.GetAsync(t => t.TagName == tagName);
                    Tag tag;

                    if (existingTag.Any())
                    {
                        tag = existingTag.First(); // Use existing tag
                    }
                    else
                    {
                        tag = new Tag { TagName = tagName };
                        await _tagRepository.AddAsync(tag);
                    }

                    // Add the relationship in NewsTag table
                    newsArticle.Tags.Add(tag);
                }
            }

            await _newsRepository.AddAsync(newsArticle);
            return true; 
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

        // Get News Articles Created by Specific Author
        public async Task<IEnumerable<NewsArticleDTO>> GetNewsByAuthorAsync(short authorId)
        {
            var newsList = await _newsRepository.GetAsync(
                filter: n => n.CreatedById == authorId,
                orderBy: q => q.OrderByDescending(n => n.CreatedDate),
                include: q => q.Include(n => n.Category)
            );

            return newsList.Select(n => new NewsArticleDTO
            {
                NewsArticleID = n.NewsArticleId,
                NewsTitle = n.NewsTitle,
                Headline = n.Headline,
                CreatedDate = n.CreatedDate.Value,
                NewsContent = n.NewsContent,
                NewsSource = n.NewsSource,
                CategoryID = n.CategoryId.Value,
                CreatedByID = n.CreatedById.Value,
                NewsStatus = n.NewsStatus.Value,
                Tags = n.Tags
            }).ToList();
        }

        // Get a Single News Article by ID
        public async Task<NewsArticleDTO> GetNewsByIdAsync(int newsId)
        {
            var news = await _newsRepository.GetByIdAsync(newsId);
            if (news == null) return null;

            return new NewsArticleDTO
            {
                NewsArticleID = news.NewsArticleId,
                NewsTitle = news.NewsTitle,
                Headline = news.Headline,
                CreatedDate = news.CreatedDate.Value,
                NewsContent = news.NewsContent,
                NewsSource = news.NewsSource,
                CategoryID = news.CategoryId.Value,
                CreatedByID = news.CreatedById.Value,
                NewsStatus = news.NewsStatus.Value,
                Tags = news.Tags
                
            };
        }
        public async Task<NewsArticleDTO> GetNewsByIdAsync(string newsId)
        {
            var news = await _newsRepository.GetByIdAsync(newsId);
            if (news == null) return null;

            return new NewsArticleDTO
            {
                NewsArticleID = news.NewsArticleId,
                NewsTitle = news.NewsTitle,
                Headline = news.Headline,
                CreatedDate = news.CreatedDate.Value,
                NewsContent = news.NewsContent,
                NewsSource = news.NewsSource,
                CategoryID = news.CategoryId.Value,
                CreatedByID = news.CreatedById.Value,
                NewsStatus = news.NewsStatus.Value,
                Tags = news.Tags
            };
        }

        // Update Existing News Article
        public async Task<bool> UpdateNewsAsync(NewsArticleDTO newsArticleDTO)
        {
            var existingNews = await _newsRepository.GetByIdAsync(newsArticleDTO.NewsArticleID);
            if (existingNews == null) return false;

            existingNews.NewsTitle = newsArticleDTO.NewsTitle;
            existingNews.Headline = newsArticleDTO.Headline;
            existingNews.NewsContent = newsArticleDTO.NewsContent;
            existingNews.NewsSource = newsArticleDTO.NewsSource;
            existingNews.CategoryId = newsArticleDTO.CategoryID;
            existingNews.Tags = newsArticleDTO.Tags;
            existingNews.ModifiedDate = DateTime.UtcNow;

            await _newsRepository.UpdateAsync(existingNews);
            return true;
        }

        // Delete News Article
        public async Task<bool> DeleteNewsAsync(int newsId)
        {
            var existingNews = await _newsRepository.GetByIdAsync(newsId);
            if (existingNews == null) return false;

            await _newsRepository.DeleteAsync(newsId);
            return true;
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
