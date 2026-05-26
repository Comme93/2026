using Microsoft.Data.SqlClient;
using WarehouseManagement.Models;
using System.Security.Cryptography;
using System.Text;

namespace WarehouseManagement.Repositories;

public class AuthRepository
{
    public AppUser? Login(string login, string password)
    {
        var passwordHash = HashPassword(password);
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            SELECT u.Id, u.Login, u.FullName, u.Email, u.RoleId, u.IsActive, u.CreatedAt, u.LastLoginAt, r.Name as RoleName
            FROM AppUsers u
            LEFT JOIN Roles r ON u.RoleId = r.Id
            WHERE u.Login = @login AND u.PasswordHash = @passwordHash AND u.IsActive = 1", conn);

        cmd.Parameters.AddWithValue("@login", login);
        cmd.Parameters.AddWithValue("@passwordHash", passwordHash);

        AppUser? user = null;
        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                user = new AppUser
                {
                    Id = (int)reader["Id"],
                    Login = reader["Login"].ToString() ?? "",
                    FullName = reader["FullName"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    RoleId = (int)reader["RoleId"],
                    RoleName = reader["RoleName"].ToString() ?? "User",
                    IsActive = (bool)reader["IsActive"],
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    LastLoginAt = reader["LastLoginAt"] != DBNull.Value ? (DateTime)reader["LastLoginAt"] : null
                };
            }
        }

        if (user != null)
        {
            using var updateCmd = new SqlCommand("UPDATE AppUsers SET LastLoginAt = GETDATE() WHERE Id = @id", conn);
            updateCmd.Parameters.AddWithValue("@id", user.Id);
            updateCmd.ExecuteNonQuery();
            return user;
        }
        return null;
    }

    public List<AppUser> GetAll()
    {
        var users = new List<AppUser>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT Id, Login, FullName, Email, RoleId, IsActive, CreatedAt, LastLoginAt FROM AppUsers ORDER BY FullName", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new AppUser
            {
                Id = (int)reader["Id"],
                Login = reader["Login"].ToString() ?? "",
                FullName = reader["FullName"].ToString() ?? "",
                Email = reader["Email"].ToString() ?? "",
                RoleId = (int)reader["RoleId"],
                IsActive = (bool)reader["IsActive"],
                CreatedAt = (DateTime)reader["CreatedAt"],
                LastLoginAt = reader["LastLoginAt"] != DBNull.Value ? (DateTime)reader["LastLoginAt"] : null
            });
        }
        return users;
    }

    public void Insert(AppUser user)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            INSERT INTO AppUsers (Login, PasswordHash, FullName, Email, RoleId)
            VALUES (@login, @passwordHash, @fullName, @email, @roleId)", conn);

        cmd.Parameters.AddWithValue("@login", user.Login);
        cmd.Parameters.AddWithValue("@passwordHash", HashPassword("password123"));
        cmd.Parameters.AddWithValue("@fullName", user.FullName ?? "");
        cmd.Parameters.AddWithValue("@email", user.Email ?? "");
        cmd.Parameters.AddWithValue("@roleId", user.RoleId);

        cmd.ExecuteNonQuery();
    }

    public void Update(AppUser user)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            UPDATE AppUsers
            SET FullName = @fullName, Email = @email, RoleId = @roleId, IsActive = @isActive
            WHERE Id = @id", conn);

        cmd.Parameters.AddWithValue("@id", user.Id);
        cmd.Parameters.AddWithValue("@fullName", user.FullName ?? "");
        cmd.Parameters.AddWithValue("@email", user.Email ?? "");
        cmd.Parameters.AddWithValue("@roleId", user.RoleId);
        cmd.Parameters.AddWithValue("@isActive", user.IsActive);

        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("UPDATE AppUsers SET IsActive = 0 WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }
}
