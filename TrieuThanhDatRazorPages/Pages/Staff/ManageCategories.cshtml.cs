using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TrieuThanhDatRazorPages.Pages.Staff
{
    public class ManageCategoriesModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public ManageCategoriesModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public List<CategoryDTO> CategoryList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            CategoryList = (await _categoryService.GetAllCategoriesAsync())
                .Select(c => new CategoryDTO
                {
                    CategoryID = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryDescription = c.CategoryDesciption,
                    ParentCategoryID = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.CategoryName : "None",
                    IsActive = c.IsActive.GetValueOrDefault()
                })
                .ToList();

            return Page();
        }
        public async Task<IActionResult> OnGetGetCategoryByIdAsync(short id)
        {
            if (id == -1)
            {
                return new JsonResult ( new { success = false, message = "CategoryId should not be null!!"} );
            }
            Category category = await _categoryService.GetCategoryByIdAsync(id);
            bool isSuccess = category != null;
            return new JsonResult(new { success = isSuccess, message = $"Category Get with id {id} {(isSuccess ? "Successfully" : "Failed")}!", data = category });
        }
        public async Task<IActionResult> OnPostCreateOrUpdateCategoryAsync([FromBody] CategoryDTO categoryDto)
        {
            bool isSuccess = false;
            if (categoryDto.CategoryID == 0)
            {
                isSuccess = await _categoryService.AddCategoryAsync(categoryDto);
            }
            else
            {
                isSuccess =await _categoryService.UpdateCategoryAsync(categoryDto);
            }

            return new JsonResult(new { success = isSuccess, message = $"Category saved {(isSuccess ? "Successfully" : "Failed")}!" });
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync([FromBody] short categoryId)
        {
            var deleted = await _categoryService.DeleteCategoryAsync(categoryId);
            if (!deleted)
            {
                return BadRequest(new { success = false, message = "Cannot delete category with subcategories or news." });
            }

            return new JsonResult(new { success = true });
        }
    }

}
