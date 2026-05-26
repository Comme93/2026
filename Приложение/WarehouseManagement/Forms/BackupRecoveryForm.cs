using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;
using WarehouseManagement.Services;

namespace WarehouseManagement.Forms;

public partial class BackupRecoveryForm : Form
{
    private readonly BackupService _backupService = new();
    private List<dynamic> _backups = new();

    public BackupRecoveryForm()
    {
        InitializeComponent();
        SetupUI();
        LoadBackups();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ResumeLayout(false);
    }

    private void SetupUI()
    {
        this.Text = "💾 Управление резервными копиями";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 10);

        var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(20) };

        var titleLabel = new Label
        {
            Text = "💾 Управление резервными копиями",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var backupListBox = new ListBox
        {
            Location = new Point(20, 70),
            Size = new Size(760, 350),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var createButton = new Button
        {
            Text = "➕ Создать резервную копию",
            Location = new Point(20, 440),
            Size = new Size(200, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(34, 197, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        createButton.FlatAppearance.BorderSize = 0;
        createButton.Click += (s, e) => CreateBackup();

        var restoreButton = new Button
        {
            Text = "↩️ Восстановить",
            Location = new Point(230, 440),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        restoreButton.FlatAppearance.BorderSize = 0;
        restoreButton.Click += (s, e) => RestoreBackup(backupListBox.SelectedIndex);

        var deleteButton = new Button
        {
            Text = "🗑️ Удалить",
            Location = new Point(390, 440),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(239, 68, 68),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        deleteButton.FlatAppearance.BorderSize = 0;
        deleteButton.Click += (s, e) => DeleteBackup(backupListBox.SelectedIndex);

        var refreshButton = new Button
        {
            Text = "🔄 Обновить",
            Location = new Point(550, 440),
            Size = new Size(120, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        refreshButton.FlatAppearance.BorderSize = 0;
        refreshButton.Click += (s, e) => LoadBackups();

        var closeButton = new Button
        {
            Text = "✕ Закрыть",
            Location = new Point(690, 440),
            Size = new Size(90, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) => this.Close();

        var infoLabel = new Label
        {
            Text = "ℹ️ Резервные копии хранятся автоматически каждые 30 минут. Последние 10 копий сохраняются.",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(20, 500)
        };

        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(backupListBox);
        mainPanel.Controls.Add(createButton);
        mainPanel.Controls.Add(restoreButton);
        mainPanel.Controls.Add(deleteButton);
        mainPanel.Controls.Add(refreshButton);
        mainPanel.Controls.Add(closeButton);
        mainPanel.Controls.Add(infoLabel);

        this.Controls.Add(mainPanel);
    }

    private void LoadBackups()
    {
        try
        {
            _backups = new List<dynamic>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT BackupName, BackupPath, BackupDate, BackupSize, IsAutomatic FROM Backups ORDER BY BackupDate DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dynamic backup = new System.Dynamic.ExpandoObject();
                backup.BackupName = reader["BackupName"].ToString();
                backup.BackupPath = reader["BackupPath"].ToString();
                backup.BackupDate = (DateTime)reader["BackupDate"];
                backup.BackupSize = (long)reader["BackupSize"];
                backup.IsAutomatic = (bool)reader["IsAutomatic"];
                _backups.Add(backup);
            }
            RefreshList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshList()
    {
        var listBox = this.Controls[0]?.Controls.OfType<ListBox>().FirstOrDefault();
        if (listBox != null)
        {
            listBox.Items.Clear();
            foreach (var backup in _backups)
            {
                string size = FormatSize(backup.BackupSize);
                string type = backup.IsAutomatic ? "🤖 Авто" : "👤 Ручная";
                listBox.Items.Add($"{type} | {backup.BackupName} | {size} | {backup.BackupDate:g}");
            }
        }
    }

    private void CreateBackup()
    {
        try
        {
            _backupService.CreateBackup();
            MessageBox.Show("✓ Резервная копия создана", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadBackups();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RestoreBackup(int index)
    {
        if (index < 0 || index >= _backups.Count)
        {
            MessageBox.Show("⚠️ Выберите резервную копию", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show("⚠️ Восстановление перезапишет текущие данные. Продолжить?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            try
            {
                _backupService.RestoreBackup(_backups[index].BackupPath);
                MessageBox.Show("✓ Данные восстановлены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBackups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void DeleteBackup(int index)
    {
        if (index < 0 || index >= _backups.Count)
        {
            MessageBox.Show("⚠️ Выберите резервную копию", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show("⚠️ Удалить резервную копию?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            try
            {
                _backupService.DeleteBackup(_backups[index].BackupPath);
                MessageBox.Show("✓ Резервная копия удалена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBackups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
