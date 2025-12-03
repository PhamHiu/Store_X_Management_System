using System;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    public class CategoryDAL : DBConnection
    {
        // 1. get all categories
        public DataTable GetCategories()
        {
            string query = "SELECT CategoryID, CategoryName, CategoryDesc FROM Categories";
            return ExecuteQuery(query);
        }
        // 2. search categories by keyword
        public DataTable SearchCategories(string keyword)
        {
            string query = "SELECT CategoryID, CategoryName, CategoryDesc " +
                           "FROM Categories WHERE CategoryName LIKE @Keyword";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };

            return ExecuteQuery(query, parameters);
        }
        // 3. add new category
        public bool InsertCategory(string name, string desc)
        {
            string query = "INSERT INTO Categories (CategoryName, CategoryDesc) VALUES (@Name, @Desc)";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", name),
                new SqlParameter("@Desc", string.IsNullOrEmpty(desc) ? (object)DBNull.Value : desc)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 4. update category
        public bool UpdateCategory(int id, string name, string desc)
        {
            string query = "UPDATE Categories SET CategoryName = @Name, CategoryDesc = @Desc WHERE CategoryID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id),
                new SqlParameter("@Name", name),
                new SqlParameter("@Desc", string.IsNullOrEmpty(desc) ? (object)DBNull.Value : desc)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 5. delete category
        public bool DeleteCategory(int id)
        {
            string query = "DELETE FROM Categories WHERE CategoryID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id)
            };
            return ExecuteNonQuery(query, parameters);
        }
    }
}