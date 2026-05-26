using Microsoft.Data.SqlClient;
using WarehouseManagement.Models;

namespace WarehouseManagement.Repositories;

public class CategoryRepository
{
    public List<Category> GetAll()
    {
        var categories = new List<Category>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT Id, Name, Description, IsActive FROM Categories WHERE IsActive = 1 ORDER BY Name", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(new Category
            {
                Id = (int)reader["Id"],
                Name = reader["Name"].ToString() ?? "",
                Description = reader["Description"].ToString() ?? "",
                IsActive = (bool)reader["IsActive"]
            });
        }
        return categories;
    }

    public void Insert(Category category)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("INSERT INTO Categories (Name, Description) VALUES (@name, @description)", conn);
        cmd.Parameters.AddWithValue("@name", category.Name);
        cmd.Parameters.AddWithValue("@description", category.Description ?? "");
        cmd.ExecuteNonQuery();
    }

    public void Update(Category category)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("UPDATE Categories SET Name = @name, Description = @description WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", category.Id);
        cmd.Parameters.AddWithValue("@name", category.Name);
        cmd.Parameters.AddWithValue("@description", category.Description ?? "");
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("UPDATE Categories SET IsActive = 0 WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
