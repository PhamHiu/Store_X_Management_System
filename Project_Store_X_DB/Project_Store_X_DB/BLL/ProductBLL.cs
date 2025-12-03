using System;
using System.Data;
using Project_Store_X_DB.DAL;

namespace Project_Store_X_DB.BLL
{
    public class ProductBLL
    {
        private ProductDAL _dal = new ProductDAL();

        public DataTable GetProductList() => _dal.GetProducts();
        public DataTable GetCategoryList() => _dal.GetCategoriesForCombo();
        public DataTable GetSupplierList() => _dal.GetSuppliersForCombo();
        public DataTable SearchProduct(string keyword) => _dal.SearchProducts(keyword);

        // Logic Thêm
        public string AddProduct(string name, string desc, string imageURL, 
               string priceStr, string qtyStr, int catID, int supID)
        {
            // Validate data
            if (string.IsNullOrWhiteSpace(name)) return "Product Name is required.";
            if (catID <= 0) return "Please select a Category.";
            if (supID <= 0) return "Please select a Supplier.";

            // Parse imputs
            if (!decimal.TryParse(priceStr, out decimal price) || price < 0)
                return "Invalid Price. Must be >= 0.";

            if (!int.TryParse(qtyStr, out int quantity) || quantity < 0)
                return "Invalid Quantity. Must be an integer >= 0.";

            if (_dal.InsertProduct(name, desc, imageURL, price, quantity, catID, supID))
                return "Success";

            return "Failed to add product.";
        }

        // Logic update
        public string EditProduct(int id, string name, string desc, string imageURL, 
               string priceStr, string qtyStr, int catID, int supID)
        {
            if (id <= 0) return "Invalid Product ID.";
            if (string.IsNullOrWhiteSpace(name)) return "Product Name is required.";

            if (!decimal.TryParse(priceStr, out decimal price) || price < 0)
                return "Invalid Price.";

            if (!int.TryParse(qtyStr, out int quantity) || quantity < 0)
                return "Invalid Quantity.";

            if (_dal.UpdateProduct(id, name, desc, imageURL, price, quantity, catID, supID))
                return "Success";

            return "Failed to update product.";
        }

        // Logic delete
        public string RemoveProduct(int id)
        {
            try
            {
                if (_dal.DeleteProduct(id))
                    return "Success";
                return "Failed to delete.";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                    return "Cannot delete: Product has been ordered.";
                return "Error: " + ex.Message;
            }
        }

        public DataTable GetProductInventoryReport(DateTime snapshot)
        {
            return _dal.GetProductInventoryReport(snapshot);
        }
    }
}