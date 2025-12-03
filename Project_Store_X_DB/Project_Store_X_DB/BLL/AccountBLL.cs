using System;
using System.Data;
using Project_Store_X_DB.DAL;
using Project_Store_X_DB.DTO;
using BCrypt.Net;// using thư viện BCrypt

namespace Project_Store_X_DB.BLL
{
    public class AccountBLL
    {
        private AccountDAL _dal = new AccountDAL();
        public EmployeeDTO CheckLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {return null;}
            DataTable dt = _dal.GetAccountByUsername(username);            
            if (dt.Rows.Count == 0) {return null;}
            DataRow row = dt.Rows[0];
            string storedHash = row["PasswordHash"].ToString();
            // 3. Use BCrypt to compare password( split Salt and Hash to verify)
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, storedHash);
            if (isPasswordCorrect)
            {              
                return new EmployeeDTO// Send back EmployeeDTO if login is successful
                {
                    EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                    EmployeeName = row["EmployeeName"].ToString(),
                    RoleName = row["RoleName"].ToString()
                };
            }
            return null; // Password is incorrect
        }
        
        
    }
}