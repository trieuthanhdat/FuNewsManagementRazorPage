using FUNewsManagement.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Interfaces
{
    public interface IUserService
    {
        Task<(int Status, string Message)> RegisterAsync(RegisterDTO registerDTO);
    }
}
