using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;

namespace WarehouseManagement.Services;

public class TwoFactorService
{
    private const int CodeExpiryMinutes = 10;
    private const int CodeLength = 6;
    private const int MaxAttempts = 5;

    public string GenerateCode()
    {
        using var rng = new RNGCryptoServiceProvider();
        var tokenData = new byte[4];
        rng.GetBytes(tokenData);
        var code = (Math.Abs(BitConverter.ToInt32(tokenData, 0)) % 1000000).ToString("D6");
        return code;
    }

    public List<string> GenerateBackupCodes(int count = 10)
    {
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            using var rng = new RNGCryptoServiceProvider();
            var tokenData = new byte[6];
            rng.GetBytes(tokenData);
            var code = Convert.ToBase64String(tokenData).Substring(0, 8).ToUpper();
            codes.Add(code);
        }
        return codes;
    }

    public bool SendCodeViaEmail(int userId, string email, string code)
    {
        try
        {
            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential("warehouse.report@gmail.com", "your_app_password")
            };

            var message = new MailMessage("warehouse.report@gmail.com", email)
            {
                Subject = "Код двухфакторной аутентификации",
                Body = $@"
                    <h2>Код двухфакторной аутентификации</h2>
                    <p>Ваш код: <strong>{code}</strong></p>
                    <p>Код действителен 10 минут.</p>
                    <p>Если вы не запрашивали этот код, проигнорируйте это письмо.</p>
                ",
                IsBodyHtml = true
            };

            client.Send(message);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void SaveCodeAttempt(int userId, string code)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            INSERT INTO TwoFactorAttempts (UserId, Code, ExpiresAt, AttemptCount)
            VALUES (@userId, @code, @expiresAt, 0)", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@code", code);
        cmd.Parameters.AddWithValue("@expiresAt", DateTime.Now.AddMinutes(CodeExpiryMinutes));

        cmd.ExecuteNonQuery();
    }

    public bool VerifyCode(int userId, string code)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT Id, ExpiresAt, IsUsed, AttemptCount
            FROM TwoFactorAttempts
            WHERE UserId = @userId AND Code = @code
            ORDER BY CreatedAt DESC", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@code", code);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return false;

        var expiresAt = reader.GetDateTime(1);
        var isUsed = reader.GetBoolean(2);
        var attemptCount = reader.GetInt32(3);
        var attemptId = reader.GetInt32(0);

        if (DateTime.Now > expiresAt || isUsed || attemptCount >= MaxAttempts)
            return false;

        reader.Close();

        using var updateCmd = new SqlCommand(@"
            UPDATE TwoFactorAttempts
            SET IsUsed = 1
            WHERE Id = @id", conn);

        updateCmd.Parameters.AddWithValue("@id", attemptId);
        updateCmd.ExecuteNonQuery();

        return true;
    }

    public bool ValidateBackupCode(int userId, string code)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT BackupCodes
            FROM TwoFactorSettings
            WHERE UserId = @userId", conn);

        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return false;

        var backupCodesStr = reader.GetString(0);
        var backupCodes = backupCodesStr.Split(',').ToList();

        if (!backupCodes.Contains(code))
            return false;

        reader.Close();

        backupCodes.Remove(code);
        var updatedCodes = string.Join(",", backupCodes);

        using var updateCmd = new SqlCommand(@"
            UPDATE TwoFactorSettings
            SET BackupCodes = @codes
            WHERE UserId = @userId", conn);

        updateCmd.Parameters.AddWithValue("@codes", updatedCodes);
        updateCmd.Parameters.AddWithValue("@userId", userId);
        updateCmd.ExecuteNonQuery();

        return true;
    }

    public void EnableTwoFactor(int userId, string method = "Email", string phoneNumber = null)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        var backupCodes = GenerateBackupCodes();
        var backupCodesStr = string.Join(",", backupCodes);

        using var cmd = new SqlCommand(@"
            IF EXISTS (SELECT 1 FROM TwoFactorSettings WHERE UserId = @userId)
                UPDATE TwoFactorSettings
                SET IsEnabled = 1, Method = @method, PhoneNumber = @phone, BackupCodes = @codes, UpdatedAt = GETDATE()
                WHERE UserId = @userId
            ELSE
                INSERT INTO TwoFactorSettings (UserId, IsEnabled, Method, PhoneNumber, BackupCodes)
                VALUES (@userId, 1, @method, @phone, @codes)", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@method", method);
        cmd.Parameters.AddWithValue("@phone", phoneNumber ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@codes", backupCodesStr);

        cmd.ExecuteNonQuery();
    }

    public void DisableTwoFactor(int userId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            UPDATE TwoFactorSettings
            SET IsEnabled = 0
            WHERE UserId = @userId", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.ExecuteNonQuery();
    }

    public bool IsTwoFactorEnabled(int userId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT IsEnabled
            FROM TwoFactorSettings
            WHERE UserId = @userId", conn);

        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return reader.GetBoolean(0);

        return false;
    }

    public string GetTwoFactorMethod(int userId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT Method
            FROM TwoFactorSettings
            WHERE UserId = @userId", conn);

        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return reader.GetString(0);

        return "Email";
    }

    public List<string> GetBackupCodes(int userId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT BackupCodes
            FROM TwoFactorSettings
            WHERE UserId = @userId", conn);

        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var codesStr = reader.GetString(0);
            return codesStr.Split(',').ToList();
        }

        return new List<string>();
    }
}
