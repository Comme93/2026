using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;
using WarehouseManagement.Services;

namespace WarehouseManagement.Forms;

public partial class TwoFactorSettingsForm : Form
{
    private readonly TwoFactorService _twoFactorService = new();
    private readonly int _userId;

    public TwoFactorSettingsForm(int userId)
    {
        InitializeComponent();
        _userId = userId;
        SetupUI();
        LoadSettings();
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
        this.Text = "🔐 Двухфакторная аутентификация";
        this.Size = new Size(600, 500);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 10);

        var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(20) };

        var titleLabel = new Label
        {
            Text = "🔐 Параметры двухфакторной аутентификации",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var enableCheckBox = new CheckBox
        {
            Text = "✓ Включить 2FA",
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(15, 23, 42),
            Location = new Point(20, 70),
            Size = new Size(300, 30),
            Checked = false
        };

        var methodLabel = new Label
        {
            Text = "Метод доставки:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 120),
            AutoSize = true
        };

        var methodCombo = new ComboBox
        {
            Location = new Point(20, 150),
            Size = new Size(300, 30),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
        methodCombo.Items.AddRange(new[] { "Email", "SMS", "Authenticator App" });
        methodCombo.SelectedIndex = 0;

        var phoneLabel = new Label
        {
            Text = "Номер телефона (для SMS):",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 200),
            AutoSize = true
        };

        var phoneTextBox = new TextBox
        {
            Location = new Point(20, 230),
            Size = new Size(300, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(10),
            PlaceholderText = "+7 (999) 123-45-67"
        };

        var backupCodesLabel = new Label
        {
            Text = "📋 Резервные коды (сохраните в безопасном месте):",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(34, 197, 94),
            Location = new Point(20, 280),
            AutoSize = true
        };

        var backupCodesTextBox = new TextBox
        {
            Location = new Point(20, 310),
            Size = new Size(540, 80),
            Font = new Font("Courier New", 9),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.FromArgb(34, 197, 94),
            BorderStyle = BorderStyle.FixedSingle,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical
        };

        var saveButton = new Button
        {
            Text = "💾 Сохранить",
            Location = new Point(20, 410),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (s, e) => SaveSettings(enableCheckBox.Checked, methodCombo.SelectedItem?.ToString() ?? "Email", phoneTextBox.Text);

        var cancelButton = new Button
        {
            Text = "✕ Отмена",
            Location = new Point(180, 410),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (s, e) => this.Close();

        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(enableCheckBox);
        mainPanel.Controls.Add(methodLabel);
        mainPanel.Controls.Add(methodCombo);
        mainPanel.Controls.Add(phoneLabel);
        mainPanel.Controls.Add(phoneTextBox);
        mainPanel.Controls.Add(backupCodesLabel);
        mainPanel.Controls.Add(backupCodesTextBox);
        mainPanel.Controls.Add(saveButton);
        mainPanel.Controls.Add(cancelButton);

        this.Controls.Add(mainPanel);
    }

    private void LoadSettings()
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT IsEnabled FROM TwoFactorSettings WHERE UserId = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", _userId);
            var result = cmd.ExecuteScalar();

            var enableCheckBox = this.Controls[0]?.Controls.OfType<CheckBox>().FirstOrDefault();
            if (enableCheckBox != null && result != null)
                enableCheckBox.Checked = (bool)result;
        }
        catch { }
    }

    private void SaveSettings(bool isEnabled, string method, string phoneNumber)
    {
        try
        {
            _twoFactorService.EnableTwoFactor(_userId, method, phoneNumber);
            MessageBox.Show("✓ Параметры 2FA сохранены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
