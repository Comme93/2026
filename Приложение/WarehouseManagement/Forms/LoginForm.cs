using WarehouseManagement.Repositories;
using WarehouseManagement.Services;
using WarehouseManagement.Controls;

namespace WarehouseManagement.Forms;

public partial class LoginForm : Form
{
    private readonly AuthRepository _authRepo = new();
    private float _opacity = 0;
    private System.Windows.Forms.Timer? _fadeInTimer;
    private bool _isDragging = false;
    private Point _dragStartPoint;

    public LoginForm()
    {
        InitializeComponent();
        SetupUI();
        StartFadeInAnimation();
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
        this.Text = "Управление складом - Вход";
        this.Size = new Size(500, 450);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.Opacity = 0;
        this.DoubleBuffered = true;

        var mainContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42) };

        var titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(30, 41, 59),
            BorderStyle = BorderStyle.None
        };

        var titleBarLabel = new Label
        {
            Text = "📦 Управление складом",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };
        titleBarLabel.MouseDown += TitleBar_MouseDown;
        titleBarLabel.MouseMove += TitleBar_MouseMove;
        titleBarLabel.MouseUp += TitleBar_MouseUp;

        var closeBtn = new RoundedButton
        {
            Text = "✕",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Size = new Size(40, 40),
            Dock = DockStyle.Right,
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.FromArgb(100, 116, 139),
            Cursor = Cursors.Hand,
            BorderRadius = 6
        };
        closeBtn.MouseEnter += (s, e) => { closeBtn.BackColor = Color.FromArgb(100, 116, 139); closeBtn.ForeColor = Color.White; };
        closeBtn.MouseLeave += (s, e) => { closeBtn.BackColor = Color.FromArgb(30, 41, 59); closeBtn.ForeColor = Color.FromArgb(100, 116, 139); };
        closeBtn.Click += (s, e) => CloseWithAnimation();

        var minBtn = new RoundedButton
        {
            Text = "−",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Size = new Size(40, 40),
            Dock = DockStyle.Right,
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.FromArgb(148, 163, 184),
            Cursor = Cursors.Hand,
            BorderRadius = 6
        };
        minBtn.MouseEnter += (s, e) => minBtn.BackColor = Color.FromArgb(45, 60, 80);
        minBtn.MouseLeave += (s, e) => minBtn.BackColor = Color.FromArgb(30, 41, 59);
        minBtn.Click += (s, e) => MinimizeWithAnimation();

        titleBar.Controls.Add(titleBarLabel);
        titleBar.Controls.Add(closeBtn);
        titleBar.Controls.Add(minBtn);
        titleBar.MouseDown += TitleBar_MouseDown;

        var contentPanel = new Panel
        {
            Location = new Point(0, 40),
            Size = new Size(500, 410),
            BackColor = Color.FromArgb(15, 23, 42),
            Padding = new Padding(20)
        };

        var titleLabel = new Label
        {
            Text = "📦 Склад",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var subtitleLabel = new Label
        {
            Text = "Система управления",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(20, 50)
        };

        var loginLabel = new Label
        {
            Text = "Логин",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 90)
        };

        var loginTextBox = new RoundedTextBox
        {
            Name = "LoginTextBox",
            Location = new Point(20, 115),
            Size = new Size(460, 45),
            Font = new Font("Segoe UI", 12),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            BorderRadius = 8,
            Text = "admin"
        };
        loginTextBox.GotFocus += (s, e) => loginTextBox.BackColor = Color.FromArgb(45, 60, 80);
        loginTextBox.LostFocus += (s, e) => loginTextBox.BackColor = Color.FromArgb(30, 41, 59);

        var passwordLabel = new Label
        {
            Text = "Пароль",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 170)
        };

        var passwordTextBox = new RoundedTextBox
        {
            Name = "PasswordTextBox",
            Location = new Point(20, 195),
            Size = new Size(460, 45),
            Font = new Font("Segoe UI", 12),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            UseSystemPasswordChar = true,
            BorderRadius = 8,
            Text = "admin1234"
        };
        passwordTextBox.GotFocus += (s, e) => passwordTextBox.BackColor = Color.FromArgb(45, 60, 80);
        passwordTextBox.LostFocus += (s, e) => passwordTextBox.BackColor = Color.FromArgb(30, 41, 59);

        var loginButton = new RoundedButton
        {
            Text = "Вход",
            Location = new Point(20, 260),
            Size = new Size(460, 45),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        loginButton.MouseEnter += (s, e) => loginButton.BackColor = Color.FromArgb(37, 99, 235);
        loginButton.MouseLeave += (s, e) => loginButton.BackColor = Color.FromArgb(59, 130, 246);
        loginButton.Click += (s, e) => LoginClick(loginTextBox.Text, passwordTextBox.Text);

        var demoLabel = new Label
        {
            Text = "Демо: admin/admin1234",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Location = new Point(20, 320)
        };

        contentPanel.Controls.Add(titleLabel);
        contentPanel.Controls.Add(subtitleLabel);
        contentPanel.Controls.Add(loginLabel);
        contentPanel.Controls.Add(loginTextBox);
        contentPanel.Controls.Add(passwordLabel);
        contentPanel.Controls.Add(passwordTextBox);
        contentPanel.Controls.Add(loginButton);
        contentPanel.Controls.Add(demoLabel);

        mainContainer.Controls.Add(titleBar);
        mainContainer.Controls.Add(contentPanel);

        this.Controls.Add(mainContainer);
    }

    private void StartFadeInAnimation()
    {
        _fadeInTimer = new System.Windows.Forms.Timer();
        _fadeInTimer.Interval = 20;
        _fadeInTimer.Tick += (s, e) =>
        {
            _opacity += 0.05f;
            if (_opacity >= 1)
            {
                _opacity = 1;
                _fadeInTimer?.Stop();
            }
            this.Opacity = _opacity;
        };
        _fadeInTimer.Start();
    }

    private void LoginClick(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var user = _authRepo.Login(login, password);
        if (user != null)
        {
            CurrentSession.CurrentUser = user;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        else
        {
            MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = true;
            _dragStartPoint = new Point(e.X, e.Y);
        }
    }

    private void TitleBar_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            Point p = PointToScreen(e.Location);
            this.Location = new Point(p.X - _dragStartPoint.X, p.Y - _dragStartPoint.Y);
        }
    }

    private void TitleBar_MouseUp(object? sender, MouseEventArgs e)
    {
        _isDragging = false;
    }

    private void CloseWithAnimation()
    {
        var closeTimer = new System.Windows.Forms.Timer();
        closeTimer.Interval = 20;
        closeTimer.Tick += (s, e) =>
        {
            this.Opacity -= 0.1;
            if (this.Opacity <= 0)
            {
                closeTimer.Stop();
                this.Close();
            }
        };
        closeTimer.Start();
    }

    private void MinimizeWithAnimation()
    {
        var minTimer = new System.Windows.Forms.Timer();
        minTimer.Interval = 20;
        minTimer.Tick += (s, e) =>
        {
            this.Opacity -= 0.15;
            if (this.Opacity <= 0)
            {
                minTimer.Stop();
                this.WindowState = FormWindowState.Minimized;
                this.Opacity = 1;
            }
        };
        minTimer.Start();
    }
}
