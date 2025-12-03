using System;

namespace Project_Store_X_DB.DTO // Đảm bảo namespace là .DTO
{
    // Class tĩnh để lưu phiên đăng nhập
    public static class UserSession
    {

        public static EmployeeDTO CurrentEmployee { get; set; }
     
        public static bool IsLoggedIn => CurrentEmployee != null;

        public static void Clear()
        {
            CurrentEmployee = null;
        }
        
        public static void tests()
        {
            Console.WriteLine("test");
        }
    }
}