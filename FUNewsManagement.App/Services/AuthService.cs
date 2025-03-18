using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Services
{
    public class AuthService : IAuthService
    {
        private readonly FunewsManagementContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(FunewsManagementContext context, IPasswordHasher passwordHasher, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(int Status, string Role)> TryAuthenticateAsync(LoginDTO loginDTO)
        {
            var userEntity = await _context.SystemAccounts.FirstOrDefaultAsync(u => u.AccountEmail == loginDTO.AccountEmail);

            if (userEntity == null)
                return (1, null); // User does not exist

            if (!_passwordHasher.VerifyPassword(loginDTO.Password, userEntity.AccountPassword))
            {
                return (2, null); // Invalid password
            }

            await SignInUser(userEntity);
            return (0, userEntity.AccountRole.ToString()); // Success
        }

        public async Task SignInUser(SystemAccount userEntity)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userEntity.AccountId.ToString()),
            new Claim(ClaimTypes.Email, userEntity.AccountEmail),
            new Claim(ClaimTypes.Role, userEntity.AccountRole.ToString())
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }


}
