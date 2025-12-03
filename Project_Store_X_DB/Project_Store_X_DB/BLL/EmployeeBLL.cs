using System;
using System.Data;
using Project_Store_X_DB.DAL;
using BCrypt.Net; // Thư viện mã hóa

namespace Project_Store_X_DB.BLL
{
    public class EmployeeBLL
    {
        private EmployeeDAL _dal = new EmployeeDAL();
        // function hash password
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public DataTable GetEmployeeList()
        {
            return _dal.GetEmployees();
        }
        public DataTable GetRoleList()
        {
            return _dal.GetRoles();
        }

        // Logic add new
        public string AddEmployee(string name, string position, int roleID, string username, string rawPassword)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(name)) return "Employee Name is required.";
            if (string.IsNullOrWhiteSpace(username)) return "Username is required.";
            if (string.IsNullOrWhiteSpace(rawPassword)) return "Password is required.";
            if (roleID <= 0) return "Please select a Role.";
            // hash password before storing
            string passwordHash = HashPassword(rawPassword);
            try
            {
                bool result = _dal.InsertEmployee(name, position, roleID, username, passwordHash);
                return result ? "Success" : "Failed to create employee.";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("Username"))
                    return "Username already exists.";
                return "System Error: " + ex.Message;
            }
        }
        // Logic update
        public string EditEmployee(int empID, int accID, string name, string position, int roleID, string newRawPassword)
        {
            if (empID <= 0) return "Invalid Employee ID.";
            if (string.IsNullOrWhiteSpace(name)) return "Employee Name is required.";

            string newHash = null;

            // if newRawPassword hasheđ or null
            if (!string.IsNullOrWhiteSpace(newRawPassword))
            {
                newHash = HashPassword(newRawPassword);
            }
            if (_dal.UpdateEmployee(empID, accID, name, position, roleID, newHash))
            {
                return "Success";
            }
            return "Failed to update employee.";
        }
        // Logic delete
        public string RemoveEmployee(int id)
        {
            try
            {
                if (_dal.DeleteEmployee(id))
                    return "Success";
                return "Failed to delete.";
            }
            catch (Exception ex)
            {
                // throw  errorr delete admmin

                return "Error: " + ex.Message;
            }
        }
    }
}