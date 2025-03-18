using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace TrieuThanhDatRazorPages.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageAccountsModel : PageModel
    {
        private readonly IAccountService _accountService;
        public string CurrentUserEmail { get; private set; } = string.Empty;
        public ManageAccountsModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public List<SystemAccountDTO> Users { get; set; } = new List<SystemAccountDTO>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current logged-in user email
            CurrentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

            Console.WriteLine("⚠️ OnGetAsync() was called instead of OnGetUserAsync().");
            Users = await _accountService.GetAllAccountsAsync() ?? new List<SystemAccountDTO>();
            return Page();
        }
        public async Task<IActionResult> OnGetGetUserAsync(short? id)
        {
            Console.WriteLine("⚠️  OnGetUserAsync().");
            RegisterDTO user = await _accountService.GetAccountByIdAsync(id.Value);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found" });
            }

            var userDto = new EditUserDTO
            {
                AccountID = user.AccountID.Value,
                AccountName = user.AccountName,
                AccountEmail = user.AccountEmail,
                AccountRole = user.AccountRole ?? 1
            };

            return new JsonResult(new { success = true, data = userDto });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostCreateUserAsync([FromBody] RegisterDTO user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.AccountName) || string.IsNullOrWhiteSpace(user.AccountEmail))
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            var (status, message, newAccountID) = await _accountService.RegisterAsyncAutoGenID(user);

            if (status != 0)
            {
                return new JsonResult(new { success = false, message });
            }

            // Update user object with the new ID
            user.AccountID = newAccountID;

            return new JsonResult(new { success = true, message = "User created successfully.", accountID = newAccountID });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostEditAsync([FromBody] RegisterDTO user)
        {
            if (user == null || user.AccountID <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid user ID" });
            }

            var status = await _accountService.UpdateAccountAsync(user);

            if (status != 0)
            {
                return new JsonResult(new { success = false, message = "Failed to update user." });
            }

            return new JsonResult(new { success = true, message = "User updated successfully." });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAccountAsync(short? id)
        {
            if (id == null)
            {
                return new JsonResult(new { success = false, message = $"Invalid user ID {id}" });
            }

            var success = await _accountService.DeleteAccountAsync(id.Value);
            return new JsonResult(new { success });
        }
    }
}

