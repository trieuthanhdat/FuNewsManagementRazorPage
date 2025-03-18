using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using FUNewsManagement.App.Interfaces;


namespace FUNewsManagement.App.Ultilities
{

    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (!storedHash.StartsWith("$2a$") && !storedHash.StartsWith("$2b$"))
            {
                // Stored password is plain-text; compare directly
                return enteredPassword == storedHash;
            }
            // Stored password is hashed; verify with BCrypt
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
    }


}
