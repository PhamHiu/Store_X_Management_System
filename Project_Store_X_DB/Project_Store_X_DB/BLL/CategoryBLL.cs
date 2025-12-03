using System;
using System.Data;
using Project_Store_X_DB.DAL;

namespace Project_Store_X_DB.BLL
{
    public class CategoryBLL
    {
        private CategoryDAL _dal = new CategoryDAL();
        // get all categories
        public DataTable GetAllCategories()
        {
            return _dal.GetCategories();
        }
        // search categories by keyword
        public DataTable FindCategory(string keyword)
        {
            return _dal.SearchCategories(keyword);
        }
        // logic add new category
        public string AddCategory(string name, string desc)
        {
            if (string.IsNullOrWhiteSpace(name)) //check null or empty
            {
                return "Category Name is required.";
            }
            if (_dal.InsertCategory(name, desc))
            {
                return "Success";
            }
            return "Failed to add category.";
        }
        // Logic update
        public string EditCategory(int id, string name, string desc)
        {
            if (id <= 0) return "Invalid ID selected.";
            if (string.IsNullOrWhiteSpace(name)) return "Category Name is required.";

            if (_dal.UpdateCategory(id, name, desc))
            {
                return "Success";
            }
            return "Failed to update category.";
        }
        // Logic delete
        public string RemoveCategory(int id)
        {
            try
            {
                if (_dal.DeleteCategory(id))
                {
                    return "Success";
                }
                return "Failed to delete (ID not found).";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK") || ex.Message.Contains("REFERENCE"))
                {
                    return "Cannot delete: This category is being used by some products.";
                }
                return "System Error: " + ex.Message;
            }
        }
    }
}