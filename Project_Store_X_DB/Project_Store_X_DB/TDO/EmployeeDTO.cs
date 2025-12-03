using System;

namespace Project_Store_X_DB.DTO
{
    public class EmployeeDTO
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string RoleName { get; set; } // Lấy từ bảng Role thông qua Join và là cơ chế khi set 
    }
}