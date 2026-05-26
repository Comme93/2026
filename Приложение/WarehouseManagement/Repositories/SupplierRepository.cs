using Microsoft.Data.SqlClient;
using WarehouseManagement.Models;

namespace WarehouseManagement.Repositories;

public class SupplierRepository
{
    public List<Supplier> GetAll()
    {
        var suppliers = new List<Supplier>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT Id, Name, ContactPerson, Phone, Email, Address, IsActive FROM Suppliers WHERE IsActive = 1 ORDER BY Name", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            suppliers.Add(new Supplier
            {
                Id = (int)reader["Id"],
                Name = reader["Name"].ToString() ?? "",
                ContactPerson = reader["ContactPerson"].ToString() ?? "",
                Phone = reader["Phone"].ToString() ?? "",
                Email = reader["Email"].ToString() ?? "",
                Address = reader["Address"].ToString() ?? "",
                IsActive = (bool)reader["IsActive"]
            });
        }
        return suppliers;
    }

    public void Insert(Supplier supplier)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, Address)
            VALUES (@name, @contactPerson, @phone, @email, @address)", conn);
        cmd.Parameters.AddWithValue("@name", supplier.Name);
        cmd.Parameters.AddWithValue("@contactPerson", supplier.ContactPerson ?? "");
        cmd.Parameters.AddWithValue("@phone", supplier.Phone ?? "");
        cmd.Parameters.AddWithValue("@email", supplier.Email ?? "");
        cmd.Parameters.AddWithValue("@address", supplier.Address ?? "");
        cmd.ExecuteNonQuery();
    }

    public void Update(Supplier supplier)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            UPDATE Suppliers
            SET Name = @name, ContactPerson = @contactPerson, Phone = @phone, Email = @email, Address = @address
            WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", supplier.Id);
        cmd.Parameters.AddWithValue("@name", supplier.Name);
        cmd.Parameters.AddWithValue("@contactPerson", supplier.ContactPerson ?? "");
        cmd.Parameters.AddWithValue("@phone", supplier.Phone ?? "");
        cmd.Parameters.AddWithValue("@email", supplier.Email ?? "");
        cmd.Parameters.AddWithValue("@address", supplier.Address ?? "");
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("UPDATE Suppliers SET IsActive = 0 WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
