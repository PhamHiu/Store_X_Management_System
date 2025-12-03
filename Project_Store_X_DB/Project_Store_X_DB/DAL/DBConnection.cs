using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration; // Cần Add Reference: System.Configuration

namespace Project_Store_X_DB.DAL
{
    public class DBConnection
    {
        // 1. DEClARE connection (Class-level)
        protected SqlConnection _conn;
        private readonly string _connectionString = @"Data Source=AQUA\SQLEXPRESS;Initial Catalog=Project_Store_X_DB;Integrated Security=True";

        public DBConnection()
        {
            _conn = new SqlConnection(_connectionString);
        }
        protected void OpenConnection()//Check Open 
        {
            if (_conn.State == ConnectionState.Closed) { _conn.Open(); }
        }
        protected void CloseConnection() // Check close
        {
            if (_conn.State == ConnectionState.Open) { _conn.Close(); }
        }

        /// <summary>
        /// 4. Hàm thực thi INSERT / UPDATE / DELETE (Dùng chung)
        /// </summary>
        /// <param name="queryOrSpName">Câu lệnh SQL hoặc tên Procedure</param>
        /// <param name="parameters">Danh sách tham số (nếu có)</param>
        /// <param name="isProcedure">Có phải là Stored Procedure không?</param>
        /// <returns>Số dòng bị ảnh hưởng</returns>
        public bool ExecuteNonQuery(string queryOrSpName, SqlParameter[] parameters = null, bool isProcedure = false)
        {
            int result = 0;
            try
            {
                OpenConnection();
                using (SqlCommand cmd = new SqlCommand(queryOrSpName, _conn))
                {
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi tại đây nếu cần
                throw new Exception("NonQuery execution error: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return result > 0;
        }

        /// <summary>
        /// Hàm lấy dữ liệu dạng bảng (SELECT - trả về DataTable)
        public DataTable ExecuteQuery(string queryOrSpName, SqlParameter[] parameters = null, bool isProcedure = false)
        {
            DataTable dt = new DataTable();
            try
            {
                OpenConnection();
                using (SqlCommand cmd = new SqlCommand(queryOrSpName, _conn))
                {
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Query execution error: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return dt;
        }
    }
}