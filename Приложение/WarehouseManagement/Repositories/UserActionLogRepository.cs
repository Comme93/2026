using Microsoft.Data.SqlClient;
using WarehouseManagement.Models;

namespace WarehouseManagement.Repositories;

public class UserActionLogRepository
{
    public void LogAction(int userId, string userName, string action, string details)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            INSERT INTO UserActionLogs (UserId, UserName, Action, Details, IpAddress)
            VALUES (@userId, @userName, @action, @details, @ipAddress)", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@userName", userName);
        cmd.Parameters.AddWithValue("@action", action);
        cmd.Parameters.AddWithValue("@details", details ?? "");
        cmd.Parameters.AddWithValue("@ipAddress", "127.0.0.1");

        cmd.ExecuteNonQuery();
    }

    public List<UserActionLog> GetAll()
    {
        var logs = new List<UserActionLog>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT Id, UserId, UserName, Action, Details, ActionDate, IpAddress FROM UserActionLogs ORDER BY ActionDate DESC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            logs.Add(new UserActionLog
            {
                Id = (int)reader["Id"],
                UserId = (int)reader["UserId"],
                UserName = reader["UserName"].ToString() ?? "",
                Action = reader["Action"].ToString() ?? "",
                Details = reader["Details"].ToString() ?? "",
                ActionDate = (DateTime)reader["ActionDate"],
                IpAddress = reader["IpAddress"].ToString() ?? ""
            });
        }
        return logs;
    }
}
