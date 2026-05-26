using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;

namespace WarehouseManagement.Services;

public class NotificationService
{
    public void SendStockAlert(int productId, string productName, int currentQuantity, int minQuantity)
    {
        var message = $"Товар '{productName}' имеет низкий запас: {currentQuantity} шт. (минимум: {minQuantity} шт.)";
        var adminUsers = GetAdminUsers();

        foreach (var userId in adminUsers)
        {
            CreateNotification(userId, "⚠️ Низкий запас", message, "StockAlert", null);
        }
    }

    public void SendPriceChangeAlert(int productId, string productName, decimal oldPrice, decimal newPrice)
    {
        var change = newPrice > oldPrice ? "повышена" : "снижена";
        var message = $"Цена товара '{productName}' {change}: {oldPrice:C} → {newPrice:C}";
        var adminUsers = GetAdminUsers();

        foreach (var userId in adminUsers)
        {
            CreateNotification(userId, "💰 Изменение цены", message, "PriceAlert", null);
        }
    }

    public void SendOrderAlert(int userId, string orderInfo)
    {
        var message = $"Ваш заказ обновлен: {orderInfo}";
        CreateNotification(userId, "📦 Обновление заказа", message, "OrderAlert", null);
    }

    public void SendSystemAlert(string title, string message)
    {
        var allUsers = GetAllUsers();
        foreach (var userId in allUsers)
        {
            CreateNotification(userId, title, message, "SystemAlert", null);
        }
    }

    public void CreateNotification(int userId, string title, string message, string type, string actionUrl)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            INSERT INTO Notifications (UserId, Title, Message, Type, ActionUrl)
            VALUES (@userId, @title, @message, @type, @actionUrl)", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@message", message);
        cmd.Parameters.AddWithValue("@type", type ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@actionUrl", actionUrl ?? (object)DBNull.Value);

        cmd.ExecuteNonQuery();
    }

    public List<(int Id, string Title, string Message, string Type, bool IsRead, DateTime CreatedAt)> GetUserNotifications(int userId, bool unreadOnly = false)
    {
        var notifications = new List<(int, string, string, string, bool, DateTime)>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        var query = @"
            SELECT Id, Title, Message, Type, IsRead, CreatedAt
            FROM Notifications
            WHERE UserId = @userId";

        if (unreadOnly)
            query += " AND IsRead = 0";

        query += " ORDER BY CreatedAt DESC";

        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            notifications.Add((
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetBoolean(4),
                reader.GetDateTime(5)
            ));
        }

        return notifications;
    }

    public int GetUnreadNotificationCount(int userId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT COUNT(*)
            FROM Notifications
            WHERE UserId = @userId AND IsRead = 0", conn);

        cmd.Parameters.AddWithValue("@userId", userId);

        return (int)cmd.ExecuteScalar();
    }

    public void MarkAsRead(int notificationId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            UPDATE Notifications
            SET IsRead = 1
            WHERE Id = @id", conn);

        cmd.Parameters.AddWithValue("@id", notificationId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteNotification(int notificationId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            DELETE FROM Notifications
            WHERE Id = @id", conn);

        cmd.Parameters.AddWithValue("@id", notificationId);
        cmd.ExecuteNonQuery();
    }

    public void SetNotificationPreferences(int userId, bool stockAlerts, bool priceAlerts, bool orderAlerts, bool systemAlerts, string deliveryMethod)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            IF EXISTS (SELECT 1 FROM NotificationPreferences WHERE UserId = @userId)
                UPDATE NotificationPreferences
                SET StockAlerts = @stock, PriceAlerts = @price, OrderAlerts = @order, SystemAlerts = @system, DeliveryMethod = @method
                WHERE UserId = @userId
            ELSE
                INSERT INTO NotificationPreferences (UserId, StockAlerts, PriceAlerts, OrderAlerts, SystemAlerts, DeliveryMethod)
                VALUES (@userId, @stock, @price, @order, @system, @method)", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@stock", stockAlerts ? 1 : 0);
        cmd.Parameters.AddWithValue("@price", priceAlerts ? 1 : 0);
        cmd.Parameters.AddWithValue("@order", orderAlerts ? 1 : 0);
        cmd.Parameters.AddWithValue("@system", systemAlerts ? 1 : 0);
        cmd.Parameters.AddWithValue("@method", deliveryMethod);

        cmd.ExecuteNonQuery();
    }

    public (bool StockAlerts, bool PriceAlerts, bool OrderAlerts, bool SystemAlerts, string DeliveryMethod) GetNotificationPreferences(int userId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT StockAlerts, PriceAlerts, OrderAlerts, SystemAlerts, DeliveryMethod
            FROM NotificationPreferences
            WHERE UserId = @userId", conn);

        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return (
                reader.GetBoolean(0),
                reader.GetBoolean(1),
                reader.GetBoolean(2),
                reader.GetBoolean(3),
                reader.GetString(4)
            );
        }

        return (true, true, true, true, "InApp");
    }

    private List<int> GetAdminUsers()
    {
        var adminUsers = new List<int>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT Id
            FROM AppUsers
            WHERE RoleId = 1 AND IsActive = 1", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            adminUsers.Add(reader.GetInt32(0));
        }

        return adminUsers;
    }

    private List<int> GetAllUsers()
    {
        var allUsers = new List<int>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT Id
            FROM AppUsers
            WHERE IsActive = 1", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            allUsers.Add(reader.GetInt32(0));
        }

        return allUsers;
    }
}
