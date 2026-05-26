using Microsoft.Data.SqlClient;
using WarehouseManagement.Models;

namespace WarehouseManagement.Repositories;

public class PermissionRepository
{
    public List<Permission> GetUserPermissions(int userId)
    {
        var permissions = new List<Permission>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            SELECT DISTINCT p.Id, p.Name, p.Description
            FROM Permissions p
            INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId
            INNER JOIN Roles r ON rp.RoleId = r.Id
            INNER JOIN AppUsers u ON u.RoleId = r.Id
            WHERE u.Id = @userId AND u.IsActive = 1 AND r.IsActive = 1", conn);

        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            permissions.Add(new Permission
            {
                Id = (int)reader["Id"],
                Name = reader["Name"].ToString() ?? "",
                Description = reader["Description"].ToString() ?? ""
            });
        }
        return permissions;
    }

    public bool HasPermission(int userId, string permissionName)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            SELECT COUNT(*) as cnt
            FROM Permissions p
            INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId
            INNER JOIN Roles r ON rp.RoleId = r.Id
            INNER JOIN AppUsers u ON u.RoleId = r.Id
            WHERE u.Id = @userId AND p.Name = @permissionName AND u.IsActive = 1 AND r.IsActive = 1", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@permissionName", permissionName);

        var result = cmd.ExecuteScalar();
        return result != null && (int)result > 0;
    }

    public List<Permission> GetRolePermissions(int roleId)
    {
        var permissions = new List<Permission>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            SELECT p.Id, p.Name, p.Description
            FROM Permissions p
            INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId
            WHERE rp.RoleId = @roleId", conn);

        cmd.Parameters.AddWithValue("@roleId", roleId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            permissions.Add(new Permission
            {
                Id = (int)reader["Id"],
                Name = reader["Name"].ToString() ?? "",
                Description = reader["Description"].ToString() ?? ""
            });
        }
        return permissions;
    }
}
