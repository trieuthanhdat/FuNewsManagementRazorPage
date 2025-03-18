using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Interfaces
{
    public interface IAccountService
    {
        Task<SystemAccount> GetAccountByEmailAsync(string email);
        /// **🔹 Authenticate User & Return Status Code**
        Task<(int Status, int Role)> AuthenticateAsync(LoginDTO loginDTO);

        /// **🔹 Logout the User**
        Task LogoutAsync();

        /// **🔹 Register a New User**
        Task<(int Status, string Message)> RegisterAsync(RegisterDTO registerDTO);
        Task<(int Status, string Message, short NewAccountID)> RegisterAsyncAutoGenID(RegisterDTO registerDTO);

        /// **🔹 Update an Existing User**
        Task<short> UpdateAccountAsync(RegisterDTO registerDTO);

        /// **🔹 Get a User By ID**
        Task<RegisterDTO> GetAccountByIdAsync(short id);

        /// **🔹 Get All User Accounts**
        Task<List<SystemAccountDTO>> GetAllAccountsAsync();
        Task<bool> DeleteAccountAsync(short accountId);
        /// **🔹 Hash Existing Plain-Text Passwords (One-time use)**
        Task HashExistingPasswords();
    }
}
