using FUNewsManagement.Domain.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using FUNewsManagement.App.Interfaces;

namespace TrieuThanhDatRazorPages.ViewModels
{
    public class LoginViewModel
    {
        public string AccountEmail { get; set; }
        public string Password { get; set; }
    }
}
