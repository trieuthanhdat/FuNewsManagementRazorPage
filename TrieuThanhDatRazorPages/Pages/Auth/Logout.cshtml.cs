using FUNewsManagement.App.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TrieuThanhDatRazorPages.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly IAccountService _accountService;

        public LogoutModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await _accountService.LogoutAsync();
            return RedirectToPage("/Auth/Login");
        }
    }
}
