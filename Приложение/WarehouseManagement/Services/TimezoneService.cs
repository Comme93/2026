using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;

namespace WarehouseManagement.Services;

public class TimezoneService
{
    public static List<(string Id, string DisplayName)> GetAvailableTimezones()
    {
        var timezones = new List<(string, string)>();

        foreach (var tzInfo in TimeZoneInfo.GetSystemTimeZones())
        {
            timezones.Add((tzInfo.Id, tzInfo.DisplayName));
        }

        return timezones;
    }

    public static void SetUserTimezone(int userId, string timezoneId)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                UPDATE AppUsers
                SET PreferredTimezone = @timezone
                WHERE Id = @userId", conn);

            cmd.Parameters.AddWithValue("@timezone", timezoneId);
            cmd.Parameters.AddWithValue("@userId", userId);

            cmd.ExecuteNonQuery();
        }
        catch
        {
        }
    }

    public static string GetUserTimezone(int userId)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT PreferredTimezone
                FROM AppUsers
                WHERE Id = @userId", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var timezone = reader.GetString(0);
                if (!string.IsNullOrEmpty(timezone))
                    return timezone;
            }
        }
        catch
        {
        }

        return TimeZoneInfo.Local.Id;
    }

    public static DateTime ConvertToUserTimezone(DateTime utcDateTime, int userId)
    {
        var timezoneId = GetUserTimezone(userId);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timezone);
    }

    public static DateTime ConvertFromUserTimezone(DateTime localDateTime, int userId)
    {
        var timezoneId = GetUserTimezone(userId);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timezone);
    }

    public static DateTime ConvertToUserTimezone(DateTime utcDateTime, string timezoneId)
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timezone);
    }

    public static DateTime ConvertFromUserTimezone(DateTime localDateTime, string timezoneId)
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timezone);
    }

    public static string FormatDateTimeForUser(DateTime dateTime, int userId, string format = "yyyy-MM-dd HH:mm:ss")
    {
        var userDateTime = ConvertToUserTimezone(dateTime, userId);
        return userDateTime.ToString(format);
    }

    public static string GetTimezoneOffset(string timezoneId)
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        var offset = timezone.GetUtcOffset(DateTime.Now);
        return $"UTC{(offset >= TimeSpan.Zero ? "+" : "")}{offset.Hours:D2}:{offset.Minutes:D2}";
    }
}
