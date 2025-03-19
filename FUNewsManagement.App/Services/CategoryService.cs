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

        public async Task<Category?> GetCategoryByIdAsync(short categoryId)
        {
            return await _categoryRepository.GetByIdAsync(categoryId);
        }

        public async Task<bool> AddCategoryAsync(CategoryDTO category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.CategoryName))
                return false; // Validation failed

            // Check if category with the same name already exists
            var existingCategory = await _categoryRepository.GetFirstOrDefaultAsync(c => c.CategoryName == category.CategoryName);
            if (existingCategory != null)
                return false;
            var parentCate = category.ParentCategoryID != null
                ? await GetCategoryByIdAsync(category.ParentCategoryID.Value)
                : null;

            var newCategory = new Category
            {
                CategoryName = category.CategoryName,
                CategoryId = category.CategoryID, 
                ParentCategory = parentCate,
                ParentCategoryId = category.ParentCategoryID,
                CategoryDesciption = category.CategoryDescription, 
                IsActive = category.IsActive, 
                InverseParentCategory = null, 
                NewsArticles = null 
            };

            await _categoryRepository.AddAsync(newCategory);
            return true;
        }

        public async Task<bool> UpdateCategoryAsync(CategoryDTO category)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(category.CategoryID);
            if (existingCategory == null)
                return false; // Category not found

            var duplicateCategory = await _categoryRepository.GetFirstOrDefaultAsync(c => c.CategoryName == category.CategoryName && c.CategoryId != category.CategoryID);
            if (duplicateCategory != null)
                return false; // Duplicate name found

            var parentCate = category.ParentCategoryID != null
                ? await GetCategoryByIdAsync(category.ParentCategoryID.Value)
                : null;

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDesciption = category.CategoryDescription;
            existingCategory.ParentCategoryId = category.ParentCategoryID;
            existingCategory.ParentCategory = parentCate;
            existingCategory.IsActive = category.IsActive;

            await _categoryRepository.UpdateAsync(existingCategory);
            return true;
        }

        // Delete a category (only if it's not linked to NewsArticles or Child Categories)
        public async Task<bool> DeleteCategoryAsync(short categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return false; // Category not found

            // Check if category has linked news articles
            var relatedNews = await _newsRepository.GetAsync(n => n.CategoryId == categoryId);
            if (relatedNews.Any())
                return false; // Cannot delete category with associated news

            // Check if category has child categories
            var childCategories = await _categoryRepository.GetAsync(c => c.ParentCategoryId == categoryId);
            if (childCategories.Any())
                return false; // Cannot delete category with child categories

            await _categoryRepository.DeleteAsync(category.CategoryId);
            return true;
        }
    }


}
