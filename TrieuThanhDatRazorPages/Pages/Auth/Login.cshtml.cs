using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrieuThanhDatRazorPages.ViewModels;

namespace TrieuThanhDatRazorPages.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAccountService _accountService;

        public LoginModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [BindProperty]
        public LoginViewModel LoginVM { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var loginDTO = new LoginDTO
            {
                AccountEmail = LoginVM.AccountEmail,
                Password = LoginVM.Password
            };

            var (status, role) = await _accountService.AuthenticateAsync(loginDTO);

            if (status == 1)
            {
                ModelState.AddModelError(string.Empty, "User does not exist.");
                return Page();
            }
            if (status == 2)
            {
                ModelState.AddModelError(string.Empty, "Invalid password.");
                return Page();
            }

            // Redirect based on user role
            if (role == 0) //admin
            {
                return RedirectToPage("/Admin/Index");
            }
            else if(role == 1) //staff
            {
                return RedirectToPage("/Staff/StaffDashboard");
            }
            else if (role == 2) //Lecturer
            {
                return RedirectToPage("/Lecturer/ManageNews");
            }
            else
            {
                return RedirectToPage("/Auth/Login");
            }
        }
    }
}
