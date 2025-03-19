using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace TrieuThanhDatRazorPages.Pages.Staff
{
    [Authorize(Roles = "Staff")]
    public class ProfileModel : PageModel
    {
        private readonly IAccountService _accountService;

        public ProfileModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [BindProperty]
        public ProfileUpdateDTO ProfileUpdate { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !short.TryParse(userIdClaim, out short userId))
            {
                return Unauthorized();
            }

            var user = await _accountService.GetAccountByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            ProfileUpdate = new ProfileUpdateDTO
            {
                AccountID = user.AccountID ?? -1, // Ensure ID is set
                AccountName = user.AccountName,
                AccountEmail = user.AccountEmail
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !short.TryParse(userIdClaim, out short userId))
            {
                TempData["ErrorMessage"] = "Unauthorized access.";
                return RedirectToPage();
            }

            var user = await _accountService.GetAccountByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            // Ensure the user can only update their own profile
            if (user.AccountID != ProfileUpdate.AccountID)
            {
                TempData["ErrorMessage"] = $"Unauthorized action. curret accountID {ProfileUpdate.AccountID.GetValueOrDefault(-1)} - user validate ID {user.AccountID.GetValueOrDefault(-1)}";
                return RedirectToPage();
            }

            // Validate password if changing password
            if (!string.IsNullOrEmpty(ProfileUpdate.NewPassword))
            {
                if (ProfileUpdate.NewPassword != ProfileUpdate.ConfirmNewPassword)
                {
                    TempData["ErrorMessage"] = "New passwords do not match.";
                    return RedirectToPage();
                }

                var isPasswordValid = await _accountService.VerifyPasswordAsync(user.AccountID.GetValueOrDefault(-1), ProfileUpdate.CurrentPassword);
                if (!isPasswordValid)
                {
                    TempData["ErrorMessage"] = "Current password is incorrect.";
                    return RedirectToPage();
                }
            }

            // Perform update
            await _accountService.UpdateAccountAsync(ProfileUpdate);
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToPage();
        }

    }

}
