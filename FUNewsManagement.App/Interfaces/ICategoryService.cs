using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(short categoryId);
        Task<bool> AddCategoryAsync(CategoryDTO category);
        Task<bool> UpdateCategoryAsync(CategoryDTO category);
        Task<bool> DeleteCategoryAsync(short categoryId);
    }

}
