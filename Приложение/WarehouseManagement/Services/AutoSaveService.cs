using System;
using System.Windows.Forms;

namespace WarehouseManagement.Services;

public class AutoSaveService
{
    private readonly BackupService _backupService;
    private System.Windows.Forms.Timer _autoSaveTimer;
    private int _backupIntervalMinutes = 30;

    public AutoSaveService()
    {
        _backupService = new BackupService();
    }

    public void StartAutoSave(int intervalMinutes = 30)
    {
        _backupIntervalMinutes = intervalMinutes;

        _autoSaveTimer = new System.Windows.Forms.Timer();
        _autoSaveTimer.Interval = intervalMinutes * 60 * 1000;
        _autoSaveTimer.Tick += (s, e) => PerformAutoBackup();
        _autoSaveTimer.Start();
    }

    public void StopAutoSave()
    {
        if (_autoSaveTimer != null)
        {
            _autoSaveTimer.Stop();
            _autoSaveTimer.Dispose();
        }
    }

    private void PerformAutoBackup()
    {
        try
        {
            _backupService.CreateBackup(isAutomatic: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при автоматическом резервном копировании: {ex.Message}");
        }
    }

    public void SetBackupInterval(int intervalMinutes)
    {
        _backupIntervalMinutes = intervalMinutes;

        if (_autoSaveTimer != null)
        {
            _autoSaveTimer.Stop();
            _autoSaveTimer.Interval = intervalMinutes * 60 * 1000;
            _autoSaveTimer.Start();
        }
    }

    public int GetBackupInterval()
    {
        return _backupIntervalMinutes;
    }
}
