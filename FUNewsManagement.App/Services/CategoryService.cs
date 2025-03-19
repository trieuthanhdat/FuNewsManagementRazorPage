using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<NewsArticle> _newsRepository;

        public CategoryService(
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<NewsArticle> newsRepository)
        {
            _categoryRepository = categoryRepository;
            _newsRepository = newsRepository;
        }

        // Get all categories
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        // Get a single category by ID
        public async Task<Category?> GetCategoryByIdAsync(short categoryId)
        {
            return await _categoryRepository.GetByIdAsync(categoryId);
        }

        // Add a new category
        public async Task<bool> AddCategoryAsync(Category category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.CategoryName))
                return false; // Validation failed

            await _categoryRepository.AddAsync(category);
            return true;
        }

        // Update an existing category
        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(category.CategoryId);
            if (existingCategory == null)
                return false; // Category not found

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDesciption = category.CategoryDesciption;
            existingCategory.ParentCategoryId = category.ParentCategoryId;
            existingCategory.IsActive = category.IsActive;

            await _categoryRepository.UpdateAsync(existingCategory);
            return true;
        }

        // Delete category (only if it's not associated with any NewsArticle)
        public async Task<bool> DeleteCategoryAsync(short categoryId)
        {
            // Check if any news articles are linked to this category
            var relatedNews = await _newsRepository.GetAsync(n => n.CategoryId == categoryId);
            if (relatedNews.Any())
                return false; // Cannot delete if category is linked to news

            await _categoryRepository.DeleteAsync(categoryId);
            return true;
        }
    }

}
