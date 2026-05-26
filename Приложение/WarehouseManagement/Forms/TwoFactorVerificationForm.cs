using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;
using WarehouseManagement.Services;
using WarehouseManagement.Controls;

namespace WarehouseManagement.Forms;

public partial class TwoFactorVerificationForm : Form
{
    private readonly TwoFactorService _twoFactorService = new();
    private readonly int _userId;
    private int _attemptsRemaining = 5;

    public TwoFactorVerificationForm(int userId)
    {
        InitializeComponent();
        _userId = userId;
        SetupUI();
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
        this.Size = new Size(500, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 10);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(30) };

        var titleLabel = new Label
        {
            Text = "🔐 Введите код",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(30, 30)
        };

        var subtitleLabel = new Label
        {
            Text = "Введите 6-значный код из вашего приложения или email",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(30, 70)
        };

        var codeLabel = new Label
        {
            Text = "Код:",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(30, 120)
        };

        var codeTextBox = new TextBox
        {
            Name = "CodeTextBox",
            Location = new Point(30, 150),
            Size = new Size(440, 45),
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(10),
            TextAlign = HorizontalAlignment.Center,
            MaxLength = 6
        };
        codeTextBox.KeyPress += (s, e) =>
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        };

        var attemptsLabel = new Label
        {
            Text = $"⏱️ Попыток осталось: {_attemptsRemaining}",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(239, 68, 68),
            AutoSize = true,
            Location = new Point(30, 210)
        };

        var verifyButton = new RoundedButton
        {
            Text = "✓ Проверить",
            Location = new Point(30, 250),
            Size = new Size(200, 45),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        verifyButton.MouseEnter += (s, e) => verifyButton.BackColor = Color.FromArgb(37, 99, 235);
        verifyButton.MouseLeave += (s, e) => verifyButton.BackColor = Color.FromArgb(59, 130, 246);
        verifyButton.Click += (s, e) => VerifyCode(codeTextBox.Text, attemptsLabel);

        var useBackupButton = new RoundedButton
        {
            Text = "🔑 Резервный код",
            Location = new Point(240, 250),
            Size = new Size(230, 45),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        useBackupButton.MouseEnter += (s, e) => useBackupButton.BackColor = Color.FromArgb(120, 136, 159);
        useBackupButton.MouseLeave += (s, e) => useBackupButton.BackColor = Color.FromArgb(100, 116, 139);
        useBackupButton.Click += (s, e) => UseBackupCode();

        var cancelButton = new RoundedButton
        {
            Text = "✕ Отмена",
            Location = new Point(30, 310),
            Size = new Size(440, 40),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        cancelButton.MouseEnter += (s, e) => cancelButton.BackColor = Color.FromArgb(120, 136, 159);
        cancelButton.MouseLeave += (s, e) => cancelButton.BackColor = Color.FromArgb(100, 116, 139);
        cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(subtitleLabel);
        mainPanel.Controls.Add(codeLabel);
        mainPanel.Controls.Add(codeTextBox);
        mainPanel.Controls.Add(attemptsLabel);
        mainPanel.Controls.Add(verifyButton);
        mainPanel.Controls.Add(useBackupButton);
        mainPanel.Controls.Add(cancelButton);

        this.Controls.Add(mainPanel);
    }

    private void VerifyCode(string code, Label attemptsLabel)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
        {
            MessageBox.Show("⚠️ Введите 6-значный код", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (_twoFactorService.VerifyCode(_userId, code))
            {
                MessageBox.Show("✓ Код верен! Добро пожаловать.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                _attemptsRemaining--;
                attemptsLabel.Text = $"⏱️ Попыток осталось: {_attemptsRemaining}";

                if (_attemptsRemaining <= 0)
                {
                    MessageBox.Show("✗ Превышено максимальное количество попыток", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"✗ Неверный код. Осталось {_attemptsRemaining} попыток", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UseBackupCode()
    {
        var backupForm = new Form
        {
            Text = "🔑 Резервный код",
            Size = new Size(400, 200),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(15, 23, 42),
            ForeColor = Color.White,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var label = new Label
        {
            Text = "Введите резервный код:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            Location = new Point(20, 20),
            AutoSize = true
        };

        var textBox = new TextBox
        {
            Location = new Point(20, 50),
            Size = new Size(360, 35),
            Font = new Font("Segoe UI", 11),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(10)
        };

        var okButton = new RoundedButton
        {
            Text = "✓ OK",
            Location = new Point(20, 100),
            Size = new Size(170, 35),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        okButton.MouseEnter += (s, e) => okButton.BackColor = Color.FromArgb(37, 99, 235);
        okButton.MouseLeave += (s, e) => okButton.BackColor = Color.FromArgb(59, 130, 246);
        okButton.Click += (s, e) =>
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM TwoFactorAttempts WHERE UserId = @userId AND Code = @code AND IsUsed = 0", conn);
                cmd.Parameters.AddWithValue("@userId", _userId);
                cmd.Parameters.AddWithValue("@code", textBox.Text);
                var count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    MessageBox.Show("✓ Резервный код верен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    backupForm.Close();
                }
                else
                {
                    MessageBox.Show("✗ Неверный резервный код", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        var cancelButton = new RoundedButton
        {
            Text = "✕ Отмена",
            Location = new Point(200, 100),
            Size = new Size(180, 35),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        cancelButton.MouseEnter += (s, e) => cancelButton.BackColor = Color.FromArgb(120, 136, 159);
        cancelButton.MouseLeave += (s, e) => cancelButton.BackColor = Color.FromArgb(100, 116, 139);
        cancelButton.Click += (s, e) => backupForm.Close();

        backupForm.Controls.Add(label);
        backupForm.Controls.Add(textBox);
        backupForm.Controls.Add(okButton);
        backupForm.Controls.Add(cancelButton);

        backupForm.ShowDialog();
    }
}
