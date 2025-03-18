using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Interfaces
{
    public interface IAuthService
    {
        Task<(int Status, string Role)> TryAuthenticateAsync(LoginDTO loginDTO);
        Task SignInUser(SystemAccount userEntity);
        Task LogoutAsync();
    }
}
