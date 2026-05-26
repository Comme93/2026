using Microsoft.Data.SqlClient;
using WarehouseManagement.Models;

namespace WarehouseManagement.Repositories;

public class ProductRepository
{
    public List<Product> GetAll()
    {
        var products = new List<Product>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            SELECT p.Id, p.Article, p.Name, p.CategoryId, c.Name as CategoryName, p.Unit, p.Price,
                   p.Quantity, p.MinQuantity, p.Description, p.SupplierId, s.Name as SupplierName,
                   p.CreatedDate, p.IsActive
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
            WHERE p.IsActive = 1
            ORDER BY p.Name", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            products.Add(new Product
            {
                Id = (int)reader["Id"],
                Article = reader["Article"].ToString() ?? "",
                Name = reader["Name"].ToString() ?? "",
                CategoryId = (int)reader["CategoryId"],
                CategoryName = reader["CategoryName"].ToString() ?? "",
                Unit = reader["Unit"].ToString() ?? "",
                Price = (decimal)reader["Price"],
                Quantity = (int)reader["Quantity"],
                MinQuantity = (int)reader["MinQuantity"],
                Description = reader["Description"].ToString() ?? "",
                SupplierId = reader["SupplierId"] != DBNull.Value ? (int)reader["SupplierId"] : 0,
                SupplierName = reader["SupplierName"].ToString() ?? "",
                CreatedDate = (DateTime)reader["CreatedDate"],
                IsActive = (bool)reader["IsActive"]
            });
        }
        return products;
    }

    public Product? GetById(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            SELECT p.Id, p.Article, p.Name, p.CategoryId, c.Name as CategoryName, p.Unit, p.Price,
                   p.Quantity, p.MinQuantity, p.Description, p.SupplierId, s.Name as SupplierName,
                   p.CreatedDate, p.IsActive
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
            WHERE p.Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Product
            {
                Id = (int)reader["Id"],
                Article = reader["Article"].ToString() ?? "",
                Name = reader["Name"].ToString() ?? "",
                CategoryId = (int)reader["CategoryId"],
                CategoryName = reader["CategoryName"].ToString() ?? "",
                Unit = reader["Unit"].ToString() ?? "",
                Price = (decimal)reader["Price"],
                Quantity = (int)reader["Quantity"],
                MinQuantity = (int)reader["MinQuantity"],
                Description = reader["Description"].ToString() ?? "",
                SupplierId = reader["SupplierId"] != DBNull.Value ? (int)reader["SupplierId"] : 0,
                SupplierName = reader["SupplierName"].ToString() ?? "",
                CreatedDate = (DateTime)reader["CreatedDate"],
                IsActive = (bool)reader["IsActive"]
            };
        }
        return null;
    }

    public void Insert(Product product)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId)
            VALUES (@article, @name, @categoryId, @unit, @price, @quantity, @minQuantity, @description, @supplierId)", conn);

        cmd.Parameters.AddWithValue("@article", product.Article);
        cmd.Parameters.AddWithValue("@name", product.Name);
        cmd.Parameters.AddWithValue("@categoryId", product.CategoryId);
        cmd.Parameters.AddWithValue("@unit", product.Unit ?? "");
        cmd.Parameters.AddWithValue("@price", product.Price);
        cmd.Parameters.AddWithValue("@quantity", product.Quantity);
        cmd.Parameters.AddWithValue("@minQuantity", product.MinQuantity);
        cmd.Parameters.AddWithValue("@description", product.Description ?? "");
        cmd.Parameters.AddWithValue("@supplierId", product.SupplierId > 0 ? product.SupplierId : DBNull.Value);

        cmd.ExecuteNonQuery();
    }

    public void Update(Product product)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            UPDATE Products
            SET Article = @article, Name = @name, CategoryId = @categoryId, Unit = @unit,
                Price = @price, Quantity = @quantity, MinQuantity = @minQuantity,
                Description = @description, SupplierId = @supplierId
            WHERE Id = @id", conn);

        cmd.Parameters.AddWithValue("@id", product.Id);
        cmd.Parameters.AddWithValue("@article", product.Article);
        cmd.Parameters.AddWithValue("@name", product.Name);
        cmd.Parameters.AddWithValue("@categoryId", product.CategoryId);
        cmd.Parameters.AddWithValue("@unit", product.Unit ?? "");
        cmd.Parameters.AddWithValue("@price", product.Price);
        cmd.Parameters.AddWithValue("@quantity", product.Quantity);
        cmd.Parameters.AddWithValue("@minQuantity", product.MinQuantity);
        cmd.Parameters.AddWithValue("@description", product.Description ?? "");
        cmd.Parameters.AddWithValue("@supplierId", product.SupplierId > 0 ? product.SupplierId : DBNull.Value);

        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("UPDATE Products SET IsActive = 0 WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
