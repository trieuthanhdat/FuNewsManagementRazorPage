using System.Security.Claims;

namespace TrieuThanhDatRazorPages.Ultilities
{
    public class GlobalHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor Injection for Dependency Injection
        public GlobalHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Get Logged-In User ID
        public int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Convert.ToInt32(userIdClaim.Value) : 0;
        }

        // Get Logged-In User Name
        public string GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Guest";
        }

        // Get User Role
        public int GetCurrentUserRole()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
            return roleClaim != null ? Convert.ToInt32(roleClaim.Value) : -1;
        }

        // Check if User is Admin
        public bool IsAdmin()
        {
            return GetCurrentUserRole() == 0; // Role 0 = Admin
        }

        // Check if User is Staff
        public bool IsStaff()
        {
            return GetCurrentUserRole() == 1; // Role 1 = Staff
        }

        // Check if User is Lecturer
        public bool IsLecturer()
        {
            return GetCurrentUserRole() == 2; // Role 2 = Lecturer
        }
    }
}
