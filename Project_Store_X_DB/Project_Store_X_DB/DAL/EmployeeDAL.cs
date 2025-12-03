using System;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    public class EmployeeDAL : DBConnection
    {
        // 1. Get list Employees
        public DataTable GetEmployees()
        {
            string query = @"
                SELECT 
                    e.EmployeeID, 
                    e.EmployeeName, 
                    e.Position, 
                    e.RoleID,
                    r.RoleName, 
                    e.AccountID,
                    a.Username 
                FROM Employees e
                JOIN Roles r ON e.RoleID = r.RoleID
                JOIN Accounts a ON e.AccountID = a.AccountID";
            return ExecuteQuery(query);
        }
        // 2. Get list Roles
        public DataTable GetRoles()
        {
            string query = "SELECT RoleID, RoleName FROM Roles";
            return ExecuteQuery(query);
        }
        // 3. add new Employee with Account from stored procedure
        public bool InsertEmployee(string name, string position, int roleID, string username, string passwordHash)
        {
            string spName = "usp_CreateEmployeeWithAccount";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@EmployeeName", name),
                new SqlParameter("@Position", position),
                new SqlParameter("@RoleID", roleID),
                new SqlParameter("@Username", username),
                new SqlParameter("@PasswordHash", passwordHash) // string hashed
            };
            return ExecuteNonQuery(spName, parameters, true);
        }
        // 4. update Employee (can change pass )
        public bool UpdateEmployee(int employeeID, int accountID, string name, string position, int roleID, string newPasswordHash = null)
        {
            string query;
            SqlParameter[] parameters;

            if (string.IsNullOrEmpty(newPasswordHash))
            {
                // Case 1: not change password -> only update Employees table
                query = @"UPDATE Employees 
                          SET EmployeeName = @Name, Position = @Pos, RoleID = @RoleID 
                          WHERE EmployeeID = @EmpID";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmpID", employeeID),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Pos", position),
                    new SqlParameter("@RoleID", roleID)
                };
            }
            else
            {
                // Case 2: change password -> update both Employees and Accounts tables
                query = @"
                    BEGIN TRANSACTION;
                        UPDATE Employees 
                        SET EmployeeName = @Name, Position = @Pos, RoleID = @RoleID 
                        WHERE EmployeeID = @EmpID;
                        UPDATE Accounts
                        SET PasswordHash = @Pass
                        WHERE AccountID = @AccID;
                    COMMIT TRANSACTION;";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmpID", employeeID),
                    new SqlParameter("@AccID", accountID),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Pos", position),
                    new SqlParameter("@RoleID", roleID),
                    new SqlParameter("@Pass", newPasswordHash) // password hashed (new)
                };
            }
            return ExecuteNonQuery(query, parameters);
        }
        // 5. Delete Employee
        public bool DeleteEmployee(int id)
        {
            string query = "DELETE FROM Employees WHERE EmployeeID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id)
            };
            return ExecuteNonQuery(query, parameters);
        }
    }
}