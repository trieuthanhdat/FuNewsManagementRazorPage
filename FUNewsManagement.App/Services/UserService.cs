using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Services
{
    public class UserService : IUserService
    {
        private readonly FunewsManagementContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(FunewsManagementContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<(int Status, string Message)> RegisterAsync(RegisterDTO registerDTO)
        {
            if (await _context.SystemAccounts.AnyAsync(u => u.AccountEmail == registerDTO.AccountEmail))
                return (1, "Email already exists");

            if (!IsValidEmail(registerDTO.AccountEmail))
                return (2, "Invalid email format");

            var hashedPassword = _passwordHasher.HashPassword(registerDTO.Password);

            var userEntity = new SystemAccount
            {
                AccountName = registerDTO.AccountName,
                AccountEmail = registerDTO.AccountEmail,
                AccountPassword = hashedPassword,
                AccountRole = registerDTO.AccountRole
            };

            _context.SystemAccounts.Add(userEntity);
            await _context.SaveChangesAsync();
            return (0, "Success");
        }
        private bool IsValidEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }
    }

}
