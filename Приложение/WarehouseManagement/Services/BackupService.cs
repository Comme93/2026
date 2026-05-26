using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;

namespace WarehouseManagement.Services;

public class BackupService
{
    private readonly string _backupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

    public BackupService()
    {
        if (!Directory.Exists(_backupDirectory))
            Directory.CreateDirectory(_backupDirectory);
    }

    public string CreateBackup(bool isAutomatic = false)
    {
        try
        {
            var backupName = $"Backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            var backupPath = Path.Combine(_backupDirectory, $"{backupName}.bak");

            using var conn = new SqlConnection(@"Server=(localdb)\mssqllocaldb;Integrated Security=true;");
            conn.Open();

            var backupCommand = $@"
                BACKUP DATABASE [WarehouseManagementDB]
                TO DISK = '{backupPath}'
                WITH FORMAT, INIT, NAME = '{backupName}'";

            using var cmd = new SqlCommand(backupCommand, conn);
            cmd.CommandTimeout = 300;
            cmd.ExecuteNonQuery();

            var fileInfo = new FileInfo(backupPath);
            var backupSize = fileInfo.Length;

            SaveBackupRecord(backupName, backupPath, backupSize, "Full", isAutomatic);

            CleanupOldBackups();

            return backupPath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при создании резервной копии: {ex.Message}");
        }
    }

    public bool RestoreBackup(int backupId)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT BackupPath
                FROM Backups
                WHERE Id = @id", conn);

            cmd.Parameters.AddWithValue("@id", backupId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return false;

            var backupPath = reader.GetString(0);
            reader.Close();

            if (!File.Exists(backupPath))
                return false;

            using var masterConn = new SqlConnection(@"Server=(localdb)\mssqllocaldb;Integrated Security=true;");
            masterConn.Open();

            using var killCmd = new SqlCommand(@"
                ALTER DATABASE [WarehouseManagementDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", masterConn);
            killCmd.ExecuteNonQuery();

            var restoreCommand = $@"
                RESTORE DATABASE [WarehouseManagementDB]
                FROM DISK = '{backupPath}'
                WITH REPLACE";

            using var restoreCmd = new SqlCommand(restoreCommand, masterConn);
            restoreCmd.CommandTimeout = 300;
            restoreCmd.ExecuteNonQuery();

            using var normalCmd = new SqlCommand(@"
                ALTER DATABASE [WarehouseManagementDB] SET MULTI_USER", masterConn);
            normalCmd.ExecuteNonQuery();

            MarkBackupAsRestored(backupId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public List<(int Id, string BackupName, DateTime BackupDate, long BackupSize, string BackupType, bool IsAutomatic)> GetAllBackups()
    {
        var backups = new List<(int, string, DateTime, long, string, bool)>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT Id, BackupName, BackupDate, BackupSize, BackupType, IsAutomatic
            FROM Backups
            ORDER BY BackupDate DESC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            backups.Add((
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDateTime(2),
                reader.GetInt64(3),
                reader.GetString(4),
                reader.GetBoolean(5)
            ));
        }

        return backups;
    }

    public void DeleteBackup(int backupId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT BackupPath
            FROM Backups
            WHERE Id = @id", conn);

        cmd.Parameters.AddWithValue("@id", backupId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var backupPath = reader.GetString(0);
            reader.Close();

            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }

        using var deleteCmd = new SqlCommand(@"
            DELETE FROM Backups
            WHERE Id = @id", conn);

        deleteCmd.Parameters.AddWithValue("@id", backupId);
        deleteCmd.ExecuteNonQuery();
    }

    private void SaveBackupRecord(string backupName, string backupPath, long backupSize, string backupType, bool isAutomatic)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            INSERT INTO Backups (BackupName, BackupPath, BackupSize, BackupType, IsAutomatic)
            VALUES (@name, @path, @size, @type, @auto)", conn);

        cmd.Parameters.AddWithValue("@name", backupName);
        cmd.Parameters.AddWithValue("@path", backupPath);
        cmd.Parameters.AddWithValue("@size", backupSize);
        cmd.Parameters.AddWithValue("@type", backupType);
        cmd.Parameters.AddWithValue("@auto", isAutomatic ? 1 : 0);

        cmd.ExecuteNonQuery();
    }

    private void MarkBackupAsRestored(int backupId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            UPDATE Backups
            SET IsRestored = 1
            WHERE Id = @id", conn);

        cmd.Parameters.AddWithValue("@id", backupId);
        cmd.ExecuteNonQuery();
    }

    private void CleanupOldBackups()
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP 1 Id, BackupPath
            FROM Backups
            WHERE IsAutomatic = 1
            ORDER BY BackupDate ASC
            OFFSET 10 ROWS", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var path = reader.GetString(1);

            if (File.Exists(path))
                File.Delete(path);

            reader.Close();

            using var deleteCmd = new SqlCommand("DELETE FROM Backups WHERE Id = @id", conn);
            deleteCmd.Parameters.AddWithValue("@id", id);
            deleteCmd.ExecuteNonQuery();
        }
    }
}
