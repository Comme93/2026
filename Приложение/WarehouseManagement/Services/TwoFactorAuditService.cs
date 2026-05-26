using System;
using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;

namespace WarehouseManagement.Services;

public class TwoFactorAuditService
{
    public enum TwoFactorEventType
    {
        CodeGenerated,
        CodeSent,
        CodeVerified,
        CodeExpired,
        CodeFailed,
        BackupCodeUsed,
        BackupCodeGenerated,
        TwoFactorEnabled,
        TwoFactorDisabled,
        MethodChanged,
        PhoneNumberUpdated
    }

    public void LogTwoFactorEvent(int userId, TwoFactorEventType eventType, string details = null, bool success = true)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            var eventDescription = eventType switch
            {
                TwoFactorEventType.CodeGenerated => "Код 2FA сгенерирован",
                TwoFactorEventType.CodeSent => "Код 2FA отправлен",
                TwoFactorEventType.CodeVerified => "Код 2FA верифицирован",
                TwoFactorEventType.CodeExpired => "Код 2FA истёк",
                TwoFactorEventType.CodeFailed => "Ошибка верификации кода 2FA",
                TwoFactorEventType.BackupCodeUsed => "Использован код восстановления",
                TwoFactorEventType.BackupCodeGenerated => "Коды восстановления сгенерированы",
                TwoFactorEventType.TwoFactorEnabled => "2FA включена",
                TwoFactorEventType.TwoFactorDisabled => "2FA отключена",
                TwoFactorEventType.MethodChanged => "Метод 2FA изменён",
                TwoFactorEventType.PhoneNumberUpdated => "Номер телефона обновлён",
                _ => "Событие 2FA"
            };

            var fullDetails = $"{eventDescription}. {details}";

            using var cmd = new SqlCommand(@"
                INSERT INTO UserActionLogs (UserId, UserName, Action, Details, IpAddress)
                SELECT @userId, Login, @action, @details, @ip
                FROM AppUsers
                WHERE Id = @userId", conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@action", $"2FA_{eventType}");
            cmd.Parameters.AddWithValue("@details", fullDetails);
            cmd.Parameters.AddWithValue("@ip", GetClientIpAddress());

            cmd.ExecuteNonQuery();
        }
        catch
        {
        }
    }

    public void LogTwoFactorCodeGeneration(int userId, string method)
    {
        LogTwoFactorEvent(userId, TwoFactorEventType.CodeGenerated, $"Метод: {method}");
    }

    public void LogTwoFactorCodeSent(int userId, string destination)
    {
        LogTwoFactorEvent(userId, TwoFactorEventType.CodeSent, $"Отправлено на: {destination}");
    }

    public void LogTwoFactorCodeVerification(int userId, bool success)
    {
        var eventType = success ? TwoFactorEventType.CodeVerified : TwoFactorEventType.CodeFailed;
        LogTwoFactorEvent(userId, eventType, success ? "Успешно" : "Неверный код или истёк срок");
    }

    public void LogTwoFactorCodeExpiry(int userId)
    {
        LogTwoFactorEvent(userId, TwoFactorEventType.CodeExpired, "Код истёк по времени");
    }

    public void LogBackupCodeUsage(int userId, int remainingCodes)
    {
        LogTwoFactorEvent(userId, TwoFactorEventType.BackupCodeUsed, $"Осталось кодов: {remainingCodes}");
    }

    public void LogBackupCodeGeneration(int userId, int codeCount)
    {
        LogTwoFactorEvent(userId, TwoFactorEventType.BackupCodeGenerated, $"Сгенерировано кодов: {codeCount}");
    }

    public void LogTwoFactorEnabled(int userId, string method)
    {
        LogTwoFactorEvent(userId, TwoFactorEventType.TwoFactorEnabled, $"Метод: {method}");
    }

    public void LogTwoFactorDisabled(int userId)
    {
        LogTwoFactorEvent(userId, TwoFactorEventType.TwoFactorDisabled, "2FA отключена пользователем");
    }

    public void LogMethodChange(int userId, string oldMethod, string newMethod)
    {
        LogTwoFactorEvent(userId, TwoFactorEventType.MethodChanged, $"Изменено с {oldMethod} на {newMethod}");
    }

    public void LogPhoneNumberUpdate(int userId, string phoneNumber)
    {
        var maskedPhone = MaskPhoneNumber(phoneNumber);
        LogTwoFactorEvent(userId, TwoFactorEventType.PhoneNumberUpdated, $"Номер: {maskedPhone}");
    }

    public int GetTwoFactorEventCount(int userId, int daysBack = 30)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT COUNT(*)
            FROM UserActionLogs
            WHERE UserId = @userId
            AND Action LIKE '2FA_%'
            AND ActionDate >= DATEADD(DAY, -@days, GETDATE())", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@days", daysBack);

        return (int)cmd.ExecuteScalar();
    }

    public int GetFailedVerificationAttempts(int userId, int minutesBack = 60)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT COUNT(*)
            FROM UserActionLogs
            WHERE UserId = @userId
            AND Action = '2FA_CodeFailed'
            AND ActionDate >= DATEADD(MINUTE, -@minutes, GETDATE())", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@minutes", minutesBack);

        return (int)cmd.ExecuteScalar();
    }

    public bool IsUserLockedOut(int userId, int maxAttempts = 5, int minutesBack = 60)
    {
        return GetFailedVerificationAttempts(userId, minutesBack) >= maxAttempts;
    }

    private string GetClientIpAddress()
    {
        try
        {
            var hostName = System.Net.Dns.GetHostName();
            var ipHostEntry = System.Net.Dns.GetHostEntry(hostName);
            foreach (var ip in ipHostEntry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }
        }
        catch
        {
        }

        return "127.0.0.1";
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return "****";

        return phoneNumber.Substring(0, phoneNumber.Length - 4).Replace(char.Parse("[0-9]"), '*') +
               phoneNumber.Substring(phoneNumber.Length - 4);
    }
}
