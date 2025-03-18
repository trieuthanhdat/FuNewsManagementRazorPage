using FUNewsManagement.App.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace TrieuThanhDatRazorPages.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAccountService _accountService;

        public string AccountName { get; private set; }
        public string UserEmail { get; private set; }

        public IndexModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _accountService.GetAccountByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            AccountName = user.AccountName;
            UserEmail = user.AccountEmail;

            return Page();
        }
    }
}
