using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace TrieuThanhDatRazorPages.Pages.Lecturer
{
    [AllowAnonymous] // Public Access Allowed
    public class ManageNewsModel : PageModel
    {
        private readonly INewsService _newsService;
        private readonly UserManager<SystemAccount> _userManager;

        public ManageNewsModel(INewsService newsService, UserManager<SystemAccount> userManager)
        {
            _newsService = newsService;
            _userManager = userManager;
        }

        public List<NewsArticleDTO> NewsList { get; set; } = new();
        public string UserRole { get; set; } = "Guest"; // Default: Public User

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.IsInRole("Lecturer") ? "Lecturer" : "Guest";
            UserRole = userRole;

            if (userRole == "Lecturer" && !string.IsNullOrEmpty(userId) && short.TryParse(userId, out short lecturereID))
            {
                NewsList = (await _newsService.GetLecturerNewsAsync(lecturereID)).ToList();
            }
            else
            {
                NewsList = (await _newsService.GetPublicNewsAsync()).ToList();
            }

            return Page();
        }
    }

}
