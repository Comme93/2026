using WarehouseManagement.Repositories;
using WarehouseManagement.Models;
using WarehouseManagement.Controls;
using WarehouseManagement.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Net;
using System.Net.Mail;

namespace WarehouseManagement.Forms;

public partial class MainForm : Form
{
    private readonly ProductRepository _productRepo = new();
    private readonly CategoryRepository _categoryRepo = new();
    private readonly SupplierRepository _supplierRepo = new();
    private readonly IncomeTransactionRepository _incomeRepo = new();
    private readonly OutcomeTransactionRepository _outcomeRepo = new();
    private readonly UserActionLogRepository _logRepo = new();
    private Panel? _currentPanel;
    private float _opacity = 0;
    private System.Windows.Forms.Timer? _fadeInTimer;
    private bool _isDragging = false;
    private Point _dragStartPoint;
    private DataGridView? _currentDataGridView;

    private readonly NotificationService _notificationService = new();
    private readonly RecommendationService _recommendationService = new();
    private readonly DemandForecastingService _forecastingService = new();
    private readonly ChartService _chartService = new();
    private readonly BackupService _backupService = new();
    private readonly TwoFactorAuditService _twoFactorAuditService = new();
    private readonly TimezoneService _timezoneService = new();
    private readonly EncryptionService _encryptionService = new();

    public MainForm()
    {
        InitializeComponent();
        SetupUI();
        StartFadeInAnimation();
        CheckCriticalStock();
        InitializeServices();
    }

    private void InitializeServices()
    {
        try
        {
            _recommendationService.UpdateProductSimilarity();
            _forecastingService.UpdateAllForecasts(30);
        }
        catch
        {
        }
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
        this.Text = "Управление складом";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.Opacity = 0;
        this.FormBorderStyle = FormBorderStyle.None;
        this.DoubleBuffered = true;

        var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42) };

        var titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(30, 41, 59),
            BorderStyle = BorderStyle.None
        };

        var titleLabel = new Label
        {
            Text = "📦 Управление складом",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };
        titleLabel.MouseDown += TitleBar_MouseDown;
        titleLabel.MouseMove += TitleBar_MouseMove;
        titleLabel.MouseUp += TitleBar_MouseUp;

        var maxBtn = new Button
        {
            Text = "□",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Size = new Size(40, 40),
            Dock = DockStyle.Right,
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.FromArgb(148, 163, 184),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        maxBtn.FlatAppearance.BorderSize = 0;
        maxBtn.MouseEnter += (s, e) => maxBtn.BackColor = Color.FromArgb(45, 60, 80);
        maxBtn.MouseLeave += (s, e) => maxBtn.BackColor = Color.FromArgb(30, 41, 59);
        maxBtn.Click += (s, e) => this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;

        var closeBtn = new Button
        {
            Text = "X",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Size = new Size(40, 40),
            Dock = DockStyle.Right,
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.FromArgb(239, 68, 68),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        closeBtn.FlatAppearance.BorderSize = 0;
        closeBtn.MouseEnter += (s, e) => { closeBtn.BackColor = Color.FromArgb(239, 68, 68); closeBtn.ForeColor = Color.White; };
        closeBtn.MouseLeave += (s, e) => { closeBtn.BackColor = Color.FromArgb(30, 41, 59); closeBtn.ForeColor = Color.FromArgb(239, 68, 68); };
        closeBtn.Click += (s, e) => CloseWithAnimation();

        var minBtn = new Button
        {
            Text = "-",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Size = new Size(40, 40),
            Dock = DockStyle.Right,
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.FromArgb(148, 163, 184),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        minBtn.FlatAppearance.BorderSize = 0;
        minBtn.MouseEnter += (s, e) => minBtn.BackColor = Color.FromArgb(45, 60, 80);
        minBtn.MouseLeave += (s, e) => minBtn.BackColor = Color.FromArgb(30, 41, 59);
        minBtn.Click += (s, e) => MinimizeWithAnimation();

        titleBar.Controls.Add(titleLabel);
        titleBar.Controls.Add(closeBtn);
        titleBar.Controls.Add(maxBtn);
        titleBar.Controls.Add(minBtn);

        var headerPanel = CreateHeaderPanel();
        var sidebarPanel = CreateSidebarPanel();
        var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 30, 48), Name = "ContentPanel" };

        mainPanel.Controls.Add(contentPanel);
        mainPanel.Controls.Add(sidebarPanel);
        mainPanel.Controls.Add(headerPanel);
        mainPanel.Controls.Add(titleBar);

        this.Controls.Add(mainPanel);

        _currentPanel = contentPanel;
        ShowDashboard();
    }

    private Panel CreateHeaderPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(30, 41, 59),
            BorderStyle = BorderStyle.FixedSingle
        };

        var titleLabel = new Label
        {
            Text = "📦 Управление складом",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = false,
            Width = 400,
            Height = 70,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0),
            Dock = DockStyle.Left
        };

        var rightPanel = new Panel
        {
            Dock = DockStyle.Right,
            Width = 280,
            BackColor = Color.FromArgb(30, 41, 59)
        };

        var logoutButton = new RoundedButton
        {
            Text = "🚪 Выход",
            Size = new Size(260, 40),
            Location = new Point(10, 15),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        logoutButton.MouseEnter += (s, e) => logoutButton.BackColor = Color.FromArgb(37, 99, 235);
        logoutButton.MouseLeave += (s, e) => logoutButton.BackColor = Color.FromArgb(59, 130, 246);
        logoutButton.Click += (s, e) => Logout();

        var themeBtn = new RoundedButton
        {
            Text = "🌙 Тема",
            Size = new Size(80, 40),
            Location = new Point(280, 15),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        themeBtn.MouseEnter += (s, e) => themeBtn.BackColor = Color.FromArgb(120, 136, 159);
        themeBtn.MouseLeave += (s, e) => themeBtn.BackColor = Color.FromArgb(100, 116, 139);
        themeBtn.Click += (s, e) =>
        {
            _isDarkTheme = !_isDarkTheme;
            themeBtn.Text = _isDarkTheme ? "🌙 Тема" : "☀️ Тема";
            ShowNotification(_isDarkTheme ? "Тёмная тема активирована" : "Светлая тема активирована", "Тема", NotificationType.Info);
        };

        var infoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 41, 59)
        };

        var roleLabel = new Label
        {
            Text = $"👤 {CurrentSession.CurrentUser?.RoleName ?? "Unknown"}",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(20, 10)
        };

        var userLabel = new Label
        {
            Text = CurrentSession.CurrentUser?.FullName ?? "Unknown",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(200, 200, 200),
            AutoSize = true,
            Location = new Point(20, 35)
        };

        infoPanel.Controls.Add(roleLabel);
        infoPanel.Controls.Add(userLabel);
        rightPanel.Controls.Add(logoutButton);
        rightPanel.Controls.Add(themeBtn);
        rightPanel.Controls.Add(infoPanel);

        panel.Controls.Add(infoPanel);
        panel.Controls.Add(rightPanel);
        panel.Controls.Add(titleLabel);

        return panel;
    }

    private Panel CreateSidebarPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 250,
            BackColor = Color.FromArgb(25, 35, 50),
            BorderStyle = BorderStyle.FixedSingle
        };

        var buttons = new List<(string text, string tag, string icon)>
        {
            ("Панель управления", "Dashboard", "📊"),
            ("Товары", "Products", "📦"),
            ("Категории", "Categories", "📂"),
            ("Поставщики", "Suppliers", "🚚"),
            ("Приход", "Income", "📥"),
            ("Расход", "Outcome", "📤"),
            ("Логи", "Logs", "📋")
        };

        if (CurrentSession.IsAdmin)
        {
            buttons.Add(("Пользователи", "Users", "👥"));
        }

        int y = 20;
        foreach (var (text, tag, icon) in buttons)
        {
            var btn = new RoundedButton
            {
                Text = $"{icon} {text}",
                Location = new Point(10, y),
                Size = new Size(230, 50),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.FromArgb(148, 163, 184),
                Cursor = Cursors.Hand,
                Tag = tag,
                BorderRadius = 8
            };
            btn.Click += (s, e) => NavigateTo(tag);
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(59, 130, 246);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(30, 41, 59);

            panel.Controls.Add(btn);
            y += 60;
        }

        return panel;
    }

    private void NavigateTo(string page)
    {
        _currentPanel?.Controls.Clear();

        switch (page)
        {
            case "Dashboard": ShowDashboard(); break;
            case "Products": ShowProducts(); break;
            case "Categories": ShowCategories(); break;
            case "Suppliers": ShowSuppliers(); break;
            case "Income": ShowIncome(); break;
            case "Outcome": ShowOutcome(); break;
            case "Logs": ShowLogs(); break;
            case "Users": if (CurrentSession.IsAdmin) ShowUsers(); break;
            case "Favorites": ShowFavoriteProducts(); break;
            case "Cart": ShowCart(); break;
        }

        _logRepo.LogAction(CurrentSession.CurrentUser?.Id ?? 0, CurrentSession.CurrentUser?.FullName ?? "Unknown", $"Перейдено на {page}", "");
    }

    private void ShowDashboard()
    {
        var products = _productRepo.GetAll();
        var categories = _categoryRepo.GetAll();
        var lowStockProducts = products.Where(p => p.Quantity <= p.MinQuantity).ToList();
        var criticalProducts = products.Where(p => p.Quantity == 0).ToList();
        var totalValue = products.Sum(p => p.Price * p.Quantity);
        var avgPrice = products.Count > 0 ? products.Average(p => p.Price) : 0;
        var totalCategories = categories.Count;

        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };

        var titleLabel = new Label
        {
            Text = "📊 Панель управления",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var statsPanel = new FlowLayoutPanel
        {
            Location = new Point(20, 70),
            Size = new Size(1300, 160),
            BackColor = Color.FromArgb(15, 23, 42),
            AutoScroll = false,
            WrapContents = true
        };

        var stats = new[] {
            ("Всего товаров", products.Count.ToString(), "📦", Color.FromArgb(59, 130, 246)),
            ("Критичные", criticalProducts.Count.ToString(), "🔴", Color.FromArgb(239, 68, 68)),
            ("Низкий запас", lowStockProducts.Count.ToString(), "⚠️", Color.FromArgb(251, 146, 60)),
            ("Категорий", totalCategories.ToString(), "📂", Color.FromArgb(168, 85, 247)),
            ("Общая стоимость", totalValue.ToString("F0") + " руб", "💰", Color.FromArgb(34, 197, 94)),
            ("Средняя цена", avgPrice.ToString("F0") + " руб", "💵", Color.FromArgb(59, 130, 246))
        };

        foreach (var (title, value, icon, color) in stats)
        {
            var statBox = new Panel
            {
                Size = new Size(200, 130),
                BackColor = Color.FromArgb(30, 41, 59),
                Margin = new Padding(5),
                BorderStyle = BorderStyle.None
            };

            var iconLbl = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = color,
                AutoSize = true,
                Location = new Point(140, 10),
                TextAlign = ContentAlignment.TopRight
            };

            var titleLbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = false,
                Width = 190,
                Height = 30,
                Location = new Point(10, 15),
                TextAlign = ContentAlignment.TopLeft
            };

            var valueLbl = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = color,
                AutoSize = false,
                Width = 190,
                Height = 50,
                Location = new Point(10, 50),
                TextAlign = ContentAlignment.TopLeft
            };

            statBox.Controls.Add(iconLbl);
            statBox.Controls.Add(titleLbl);
            statBox.Controls.Add(valueLbl);
            statsPanel.Controls.Add(statBox);
        }

        var lowStockLabel = new Label
        {
            Text = "⚠️ Товары требующие внимания",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(251, 146, 60),
            AutoSize = true,
            Location = new Point(20, 250)
        };

        var dgv = CreateStyledDataGridView(20, 290, 1300, 300);
        var lowStockData = lowStockProducts.Select(p => new {
            Артикул = p.Article,
            Название = p.Name,
            Категория = p.CategoryName,
            Текущий = p.Quantity,
            Минимум = p.MinQuantity,
            Цена = p.Price,
            Статус = p.Quantity == 0 ? "🔴 КРИТИЧНО" : "🟡 НИЗКИЙ"
        }).ToList();
        dgv.DataSource = lowStockData;

        var quickAccessLabel = new Label
        {
            Text = "⚡ Быстрый доступ",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 610)
        };

        var quickPanel = new FlowLayoutPanel
        {
            Location = new Point(20, 650),
            Size = new Size(1300, 120),
            BackColor = Color.FromArgb(15, 23, 42),
            AutoScroll = false,
            WrapContents = true
        };

        var quickButtons = new[] {
            ("📦 Товары", "Products", Color.FromArgb(59, 130, 246)),
            ("📂 Категории", "Categories", Color.FromArgb(168, 85, 247)),
            ("🚚 Поставщики", "Suppliers", Color.FromArgb(34, 197, 94)),
            ("📥 Приход", "Income", Color.FromArgb(251, 146, 60)),
            ("📤 Расход", "Outcome", Color.FromArgb(239, 68, 68)),
            ("📋 Логи", "Logs", Color.FromArgb(100, 116, 139))
        };

        if (CurrentSession.IsAdmin)
        {
            var userBtn = new RoundedButton
            {
                Text = "👥 Пользователи",
                Size = new Size(180, 50),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5),
                BorderRadius = 8
            };
            userBtn.MouseEnter += (s, e) => userBtn.BackColor = Color.FromArgb(37, 99, 235);
            userBtn.MouseLeave += (s, e) => userBtn.BackColor = Color.FromArgb(59, 130, 246);
            userBtn.Click += (s, e) => NavigateTo("Users");
            quickPanel.Controls.Add(userBtn);

            var statsBtn = new RoundedButton
            {
                Text = "📈 Статистика",
                Size = new Size(180, 50),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5),
                BorderRadius = 8
            };
            statsBtn.MouseEnter += (s, e) => statsBtn.BackColor = Color.FromArgb(120, 136, 159);
            statsBtn.MouseLeave += (s, e) => statsBtn.BackColor = Color.FromArgb(100, 116, 139);
            statsBtn.Click += (s, e) => { _currentPanel?.Controls.Clear(); ShowAdminStats(); };
            quickPanel.Controls.Add(statsBtn);
        }

        foreach (var (text, tag, color) in quickButtons)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Size = new Size(180, 50),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5),
                Tag = tag,
                BorderRadius = 8
            };
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(37, 99, 235);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(59, 130, 246);
            btn.Click += (s, e) => NavigateTo((string)btn.Tag);
            quickPanel.Controls.Add(btn);
        }

        if (!CurrentSession.IsAdmin)
        {
            var favBtn = new RoundedButton
            {
                Text = "⭐ Избранное",
                Size = new Size(180, 50),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5),
                BorderRadius = 8
            };
            favBtn.MouseEnter += (s, e) => favBtn.BackColor = Color.FromArgb(120, 136, 159);
            favBtn.MouseLeave += (s, e) => favBtn.BackColor = Color.FromArgb(100, 116, 139);
            favBtn.Click += (s, e) => NavigateTo("Favorites");
            quickPanel.Controls.Add(favBtn);

            var cartBtn = new RoundedButton
            {
                Text = "🛒 Корзина",
                Size = new Size(180, 50),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5),
                BorderRadius = 8
            };
            cartBtn.MouseEnter += (s, e) => cartBtn.BackColor = Color.FromArgb(37, 99, 235);
            cartBtn.MouseLeave += (s, e) => cartBtn.BackColor = Color.FromArgb(59, 130, 246);
            cartBtn.Click += (s, e) => NavigateTo("Cart");
            quickPanel.Controls.Add(cartBtn);
        }

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(statsPanel);
        panel.Controls.Add(lowStockLabel);
        panel.Controls.Add(dgv);
        panel.Controls.Add(quickAccessLabel);
        panel.Controls.Add(quickPanel);

        _currentPanel?.Controls.Add(panel);
    }

    private Color AdjustBrightness(Color color, float factor)
    {
        return Color.FromArgb(
            Math.Min(255, (int)(color.R * factor)),
            Math.Min(255, (int)(color.G * factor)),
            Math.Min(255, (int)(color.B * factor))
        );
    }

    private void ShowProducts()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };
        var titleLabel = new Label
        {
            Text = "📦 Товары",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        // Search and Filter Panel - FlowLayoutPanel для правильного расположения
        var filterPanel = new FlowLayoutPanel
        {
            Location = new Point(20, 70),
            Size = new Size(1300, 60),
            BackColor = Color.FromArgb(30, 41, 59),
            AutoScroll = false,
            WrapContents = false
        };

        var searchLabel = new Label
        {
            Text = "🔍",
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(10, 15, 5, 0)
        };

        var searchBox = new TextBox
        {
            Size = new Size(200, 35),
            Font = new Font("Segoe UI", 11),
            BackColor = Color.FromArgb(45, 60, 80),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(10),
            Margin = new Padding(0, 10, 10, 0)
        };

        var statusLabel = new Label
        {
            Text = "Статус:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(10, 15, 5, 0)
        };

        var statusCombo = new ComboBox
        {
            Size = new Size(140, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(45, 60, 80),
            ForeColor = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 10, 10, 0)
        };
        statusCombo.Items.AddRange(new[] { "Все", "✅ В наличии", "⚠️ Низкий запас", "🔴 Нет в наличии" });
        statusCombo.SelectedIndex = 0;

        var categoryLabel = new Label
        {
            Text = "Категория:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(10, 15, 5, 0)
        };

        var categoryCombo = new ComboBox
        {
            Size = new Size(140, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(45, 60, 80),
            ForeColor = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 10, 10, 0)
        };
        categoryCombo.Items.Add("Все");
        foreach (var cat in _categoryRepo.GetAll())
        {
            categoryCombo.Items.Add(cat.Name);
        }
        categoryCombo.SelectedIndex = 0;

        var clearBtn = new Button
        {
            Text = "✕ Очистить",
            Size = new Size(100, 35),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(10, 10, 0, 0)
        };
        clearBtn.FlatAppearance.BorderSize = 0;
        clearBtn.MouseEnter += (s, e) => clearBtn.BackColor = Color.FromArgb(120, 136, 159);
        clearBtn.MouseLeave += (s, e) => clearBtn.BackColor = Color.FromArgb(100, 116, 139);
        clearBtn.Click += (s, e) =>
        {
            searchBox.Text = "";
            statusCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndex = 0;
        };

        filterPanel.Controls.Add(searchLabel);
        filterPanel.Controls.Add(searchBox);
        filterPanel.Controls.Add(statusLabel);
        filterPanel.Controls.Add(statusCombo);
        filterPanel.Controls.Add(categoryLabel);
        filterPanel.Controls.Add(categoryCombo);
        filterPanel.Controls.Add(clearBtn);

        // Toolbar
        var toolbarPanel = new FlowLayoutPanel
        {
            Location = new Point(20, 140),
            Size = new Size(1300, 50),
            BackColor = Color.FromArgb(30, 41, 59),
            AutoScroll = false,
            WrapContents = false
        };

        var addBtn = new RoundedButton
        {
            Text = "➕ Добавить",
            Size = new Size(130, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(5),
            BorderRadius = 8
        };
        addBtn.MouseEnter += (s, e) => addBtn.BackColor = Color.FromArgb(37, 99, 235);
        addBtn.MouseLeave += (s, e) => addBtn.BackColor = Color.FromArgb(59, 130, 246);
        addBtn.Click += (s, e) => AddProduct();

        var editBtn = new RoundedButton
        {
            Text = "✏️ Редактировать",
            Size = new Size(130, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(5),
            BorderRadius = 8
        };
        editBtn.MouseEnter += (s, e) => editBtn.BackColor = Color.FromArgb(37, 99, 235);
        editBtn.MouseLeave += (s, e) => editBtn.BackColor = Color.FromArgb(59, 130, 246);
        editBtn.Click += (s, e) => EditSelectedProduct(_currentDataGridView);

        var deleteBtn = new RoundedButton
        {
            Text = "🗑️ Удалить",
            Size = new Size(130, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(5),
            BorderRadius = 8
        };
        deleteBtn.MouseEnter += (s, e) => deleteBtn.BackColor = Color.FromArgb(120, 136, 159);
        deleteBtn.MouseLeave += (s, e) => deleteBtn.BackColor = Color.FromArgb(100, 116, 139);
        deleteBtn.Click += (s, e) => DeleteSelectedProduct(_currentDataGridView);

        var exportBtn = new RoundedButton
        {
            Text = "📥 Выгрузить",
            Size = new Size(130, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(5),
            BorderRadius = 8
        };
        exportBtn.MouseEnter += (s, e) => exportBtn.BackColor = Color.FromArgb(120, 136, 159);
        exportBtn.MouseLeave += (s, e) => exportBtn.BackColor = Color.FromArgb(100, 116, 139);
        exportBtn.Click += (s, e) => ExportToCSV(_productRepo.GetAll(), "Товары");

        var pdfBtn = new RoundedButton
        {
            Text = "📄 PDF",
            Size = new Size(130, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(5),
            BorderRadius = 8
        };
        pdfBtn.MouseEnter += (s, e) => pdfBtn.BackColor = Color.FromArgb(120, 136, 159);
        pdfBtn.MouseLeave += (s, e) => pdfBtn.BackColor = Color.FromArgb(100, 116, 139);
        pdfBtn.Click += (s, e) => ExportToPDF(_productRepo.GetAll(), "Товары");

        var refreshBtn = new RoundedButton
        {
            Text = "🔄 Обновить",
            Size = new Size(130, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(5),
            BorderRadius = 8
        };
        refreshBtn.MouseEnter += (s, e) => refreshBtn.BackColor = Color.FromArgb(120, 136, 159);
        refreshBtn.MouseLeave += (s, e) => refreshBtn.BackColor = Color.FromArgb(100, 116, 139);
        refreshBtn.Click += (s, e) => NavigateTo("Products");

        if (CurrentSession.IsAdmin)
        {
            toolbarPanel.Controls.Add(addBtn);
            toolbarPanel.Controls.Add(editBtn);
            toolbarPanel.Controls.Add(deleteBtn);
            toolbarPanel.Controls.Add(exportBtn);
            toolbarPanel.Controls.Add(pdfBtn);
            toolbarPanel.Controls.Add(refreshBtn);
        }
        else
        {
            toolbarPanel.Controls.Add(exportBtn);
            toolbarPanel.Controls.Add(pdfBtn);
            toolbarPanel.Controls.Add(refreshBtn);
        }

        var dgv = CreateStyledDataGridView(20, 200, 1300, 550);
        var products = _productRepo.GetAll();
        var dataSource = products.Select(p => new {
            ID = p.Id,
            Артикул = p.Article,
            Название = p.Name,
            Категория = p.CategoryName,
            Единица = p.Unit,
            Цена = p.Price,
            Количество = p.Quantity,
            МинКол = p.MinQuantity,
            Поставщик = p.SupplierName
        }).ToList();
        dgv.DataSource = dataSource;
        dgv.CellDoubleClick += (s, e) => OnDataGridViewCellDoubleClick(dgv, "Products");

        _currentDataGridView = dgv;

        if (!CurrentSession.IsAdmin)
        {
            var favBtn = new RoundedButton
            {
                Text = "❤️ В избранное",
                Size = new Size(130, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5),
                BorderRadius = 8
            };
            favBtn.MouseEnter += (s, e) => favBtn.BackColor = Color.FromArgb(120, 136, 159);
            favBtn.MouseLeave += (s, e) => favBtn.BackColor = Color.FromArgb(100, 116, 139);
            favBtn.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count == 0)
                {
                    ShowNotification("Выберите товар для добавления в избранное", "Ошибка", NotificationType.Warning);
                    return;
                }
                var selectedRow = dgv.SelectedRows[0];
                var productId = (int)selectedRow.Cells["ID"].Value;
                if (!_favoriteProductIds.Contains(productId))
                {
                    _favoriteProductIds.Add(productId);
                    ShowNotification("Товар добавлен в избранное", "Успех", NotificationType.Success);
                }
                else
                {
                    ShowNotification("Товар уже в избранном", "Информация", NotificationType.Info);
                }
            };
            toolbarPanel.Controls.Add(favBtn);

            var cartBtn = new RoundedButton
            {
                Text = "🛒 В корзину",
                Size = new Size(130, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5),
                BorderRadius = 8
            };
            cartBtn.MouseEnter += (s, e) => cartBtn.BackColor = Color.FromArgb(37, 99, 235);
            cartBtn.MouseLeave += (s, e) => cartBtn.BackColor = Color.FromArgb(59, 130, 246);
            cartBtn.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count == 0)
                {
                    ShowNotification("Выберите товар для добавления в корзину", "Ошибка", NotificationType.Warning);
                    return;
                }
                var selectedRow = dgv.SelectedRows[0];
                var productId = (int)selectedRow.Cells["ID"].Value;
                if (_cartItems.ContainsKey(productId))
                {
                    _cartItems[productId]++;
                }
                else
                {
                    _cartItems[productId] = 1;
                }
                ShowNotification("Товар добавлен в корзину", "Успех", NotificationType.Success);
            };
            toolbarPanel.Controls.Add(cartBtn);
        }

        Action<string, int, int> UpdateDataGrid = (searchText, statusIndex, categoryIndex) =>
        {
            var filtered = products.Where(p =>
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   p.Article.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                   p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                   p.CategoryName.Contains(searchText, StringComparison.OrdinalIgnoreCase);

                bool matchesStatus = statusIndex switch
                {
                    1 => p.Quantity > p.MinQuantity,
                    2 => p.Quantity <= p.MinQuantity && p.Quantity > 0,
                    3 => p.Quantity == 0,
                    _ => true
                };

                bool matchesCategory = categoryIndex == 0 || p.CategoryName == (string)categoryCombo.Items[categoryIndex];

                return matchesSearch && matchesStatus && matchesCategory;
            }).Select(p => new {
                ID = p.Id,
                Артикул = p.Article,
                Название = p.Name,
                Категория = p.CategoryName,
                Единица = p.Unit,
                Цена = p.Price,
                Количество = p.Quantity,
                МинКол = p.MinQuantity,
                Поставщик = p.SupplierName
            }).ToList();
            dgv.DataSource = filtered;
        };

        searchBox.TextChanged += (s, e) => UpdateDataGrid(searchBox.Text, statusCombo.SelectedIndex, categoryCombo.SelectedIndex);
        statusCombo.SelectedIndexChanged += (s, e) => UpdateDataGrid(searchBox.Text, statusCombo.SelectedIndex, categoryCombo.SelectedIndex);
        categoryCombo.SelectedIndexChanged += (s, e) => UpdateDataGrid(searchBox.Text, statusCombo.SelectedIndex, categoryCombo.SelectedIndex);

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(filterPanel);
        panel.Controls.Add(toolbarPanel);
        panel.Controls.Add(dgv);

        _currentPanel?.Controls.Add(panel);
    }

    private void AddProduct()
    {
        var form = new ProductEditForm();
        if (form.ShowDialog() == DialogResult.OK)
        {
            NavigateTo("Products");
            ShowNotification("Товар успешно добавлен", "Успех", NotificationType.Success);
        }
    }

    private void EditSelectedProduct(DataGridView dgv)
    {
        if (dgv.SelectedRows.Count == 0)
        {
            ShowNotification("Выберите товар для редактирования", "Ошибка", NotificationType.Warning);
            return;
        }

        var selectedRow = dgv.SelectedRows[0];
        var productId = (int)selectedRow.Cells["Id"].Value;
        var product = _productRepo.GetAll().FirstOrDefault(p => p.Id == productId);

        if (product != null)
        {
            var form = new ProductEditForm(product);
            if (form.ShowDialog() == DialogResult.OK)
            {
                NavigateTo("Products");
                ShowNotification("Товар успешно обновлен", "Успех", NotificationType.Success);
            }
        }
    }

    private void DeleteSelectedProduct(DataGridView dgv)
    {
        if (dgv.SelectedRows.Count == 0)
        {
            ShowNotification("Выберите товар для удаления", "Ошибка", NotificationType.Warning);
            return;
        }

        if (MessageBox.Show("Вы уверены, что хотите удалить этот товар?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            var selectedRow = dgv.SelectedRows[0];
            var productId = (int)selectedRow.Cells["Id"].Value;
            _productRepo.Delete(productId);
            NavigateTo("Products");
            ShowNotification("Товар успешно удален", "Успех", NotificationType.Success);
        }
    }

    private void OnDataGridViewCellDoubleClick(DataGridView dgv, string type)
    {
        if (CurrentSession.IsAdmin && dgv.SelectedRows.Count > 0 && type == "Products")
        {
            EditSelectedProduct(dgv);
        }
    }

    private void ShowCategories()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };
        var titleLabel = new Label { Text = "📂 Категории", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(59, 130, 246), AutoSize = true, Location = new Point(20, 20) };

        var toolbarPanel = new Panel { Location = new Point(20, 60), Size = new Size(1300, 50), BackColor = Color.FromArgb(30, 41, 59) };
        var exportBtn = new RoundedButton { Text = "📥 Выгрузить", Location = new Point(10, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        exportBtn.Click += (s, e) => ExportToCSV(_categoryRepo.GetAll(), "Категории");
        exportBtn.MouseEnter += (s, e) => exportBtn.BackColor = Color.FromArgb(120, 136, 159);
        exportBtn.MouseLeave += (s, e) => exportBtn.BackColor = Color.FromArgb(100, 116, 139);
        var refreshBtn = new RoundedButton { Text = "🔄 Обновить", Location = new Point(160, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        refreshBtn.Click += (s, e) => NavigateTo("Categories");
        refreshBtn.MouseEnter += (s, e) => refreshBtn.BackColor = Color.FromArgb(120, 136, 159);
        refreshBtn.MouseLeave += (s, e) => refreshBtn.BackColor = Color.FromArgb(100, 116, 139);
        toolbarPanel.Controls.Add(exportBtn);
        toolbarPanel.Controls.Add(refreshBtn);

        var dgv = CreateStyledDataGridView(20, 130, 1300, 650);
        var categories = _categoryRepo.GetAll();
        var dataSource = categories.Select(c => new { ID = c.Id, Название = c.Name, Описание = c.Description }).ToList();
        dgv.DataSource = dataSource;

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(toolbarPanel);
        panel.Controls.Add(dgv);
        _currentPanel?.Controls.Add(panel);
    }

    private void ShowSuppliers()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };
        var titleLabel = new Label { Text = "🚚 Поставщики", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(59, 130, 246), AutoSize = true, Location = new Point(20, 20) };

        var toolbarPanel = new Panel { Location = new Point(20, 60), Size = new Size(1300, 50), BackColor = Color.FromArgb(30, 41, 59) };
        var exportBtn = new RoundedButton { Text = "📥 Выгрузить", Location = new Point(10, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        exportBtn.Click += (s, e) => ExportToCSV(_supplierRepo.GetAll(), "Поставщики");
        exportBtn.MouseEnter += (s, e) => exportBtn.BackColor = Color.FromArgb(120, 136, 159);
        exportBtn.MouseLeave += (s, e) => exportBtn.BackColor = Color.FromArgb(100, 116, 139);
        var refreshBtn = new RoundedButton { Text = "🔄 Обновить", Location = new Point(160, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        refreshBtn.Click += (s, e) => NavigateTo("Suppliers");
        refreshBtn.MouseEnter += (s, e) => refreshBtn.BackColor = Color.FromArgb(120, 136, 159);
        refreshBtn.MouseLeave += (s, e) => refreshBtn.BackColor = Color.FromArgb(100, 116, 139);
        toolbarPanel.Controls.Add(exportBtn);
        toolbarPanel.Controls.Add(refreshBtn);

        var dgv = CreateStyledDataGridView(20, 130, 1300, 650);
        var suppliers = _supplierRepo.GetAll();
        var dataSource = suppliers.Select(s => new { ID = s.Id, Название = s.Name, Контакт = s.ContactPerson, Телефон = s.Phone, Email = s.Email }).ToList();
        dgv.DataSource = dataSource;

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(toolbarPanel);
        panel.Controls.Add(dgv);
        _currentPanel?.Controls.Add(panel);
    }

    private void ShowIncome()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };
        var titleLabel = new Label { Text = "📥 Приход товаров", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(59, 130, 246), AutoSize = true, Location = new Point(20, 20) };

        var toolbarPanel = new Panel { Location = new Point(20, 60), Size = new Size(1300, 50), BackColor = Color.FromArgb(30, 41, 59) };
        var exportBtn = new RoundedButton { Text = "📥 Выгрузить", Location = new Point(10, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        exportBtn.Click += (s, e) => ExportToCSV(_incomeRepo.GetAll(), "Приход");
        exportBtn.MouseEnter += (s, e) => exportBtn.BackColor = Color.FromArgb(120, 136, 159);
        exportBtn.MouseLeave += (s, e) => exportBtn.BackColor = Color.FromArgb(100, 116, 139);
        var refreshBtn = new RoundedButton { Text = "🔄 Обновить", Location = new Point(160, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        refreshBtn.Click += (s, e) => NavigateTo("Income");
        refreshBtn.MouseEnter += (s, e) => refreshBtn.BackColor = Color.FromArgb(120, 136, 159);
        refreshBtn.MouseLeave += (s, e) => refreshBtn.BackColor = Color.FromArgb(100, 116, 139);
        toolbarPanel.Controls.Add(exportBtn);
        toolbarPanel.Controls.Add(refreshBtn);

        var dgv = CreateStyledDataGridView(20, 130, 1300, 650);
        var income = _incomeRepo.GetAll();
        var dataSource = income.Select(t => new { ID = t.Id, Товар = t.ProductName, Количество = t.Quantity, Цена = t.Price, Поставщик = t.SupplierName, Дата = t.TransactionDate }).ToList();
        dgv.DataSource = dataSource;

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(toolbarPanel);
        panel.Controls.Add(dgv);
        _currentPanel?.Controls.Add(panel);
    }

    private void ShowOutcome()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };
        var titleLabel = new Label { Text = "📤 Расход товаров", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(59, 130, 246), AutoSize = true, Location = new Point(20, 20) };

        var toolbarPanel = new Panel { Location = new Point(20, 60), Size = new Size(1300, 50), BackColor = Color.FromArgb(30, 41, 59) };
        var exportBtn = new RoundedButton { Text = "📥 Выгрузить", Location = new Point(10, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        exportBtn.Click += (s, e) => ExportToCSV(_outcomeRepo.GetAll(), "Расход");
        exportBtn.MouseEnter += (s, e) => exportBtn.BackColor = Color.FromArgb(120, 136, 159);
        exportBtn.MouseLeave += (s, e) => exportBtn.BackColor = Color.FromArgb(100, 116, 139);
        var refreshBtn = new RoundedButton { Text = "🔄 Обновить", Location = new Point(160, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        refreshBtn.Click += (s, e) => NavigateTo("Outcome");
        refreshBtn.MouseEnter += (s, e) => refreshBtn.BackColor = Color.FromArgb(120, 136, 159);
        refreshBtn.MouseLeave += (s, e) => refreshBtn.BackColor = Color.FromArgb(100, 116, 139);
        toolbarPanel.Controls.Add(exportBtn);
        toolbarPanel.Controls.Add(refreshBtn);

        var dgv = CreateStyledDataGridView(20, 130, 1300, 650);
        var outcome = _outcomeRepo.GetAll();
        var dataSource = outcome.Select(t => new { ID = t.Id, Товар = t.ProductName, Количество = t.Quantity, Цена = t.Price, Причина = t.Reason, Дата = t.TransactionDate }).ToList();
        dgv.DataSource = dataSource;

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(toolbarPanel);
        panel.Controls.Add(dgv);
        _currentPanel?.Controls.Add(panel);
    }

    private void ShowLogs()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };
        var titleLabel = new Label { Text = "📋 Логи действий", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(59, 130, 246), AutoSize = true, Location = new Point(20, 20) };

        var toolbarPanel = new Panel { Location = new Point(20, 60), Size = new Size(1300, 50), BackColor = Color.FromArgb(30, 41, 59) };
        var exportBtn = new RoundedButton { Text = "📥 Выгрузить", Location = new Point(10, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        exportBtn.Click += (s, e) => ExportToCSV(_logRepo.GetAll(), "Логи");
        exportBtn.MouseEnter += (s, e) => exportBtn.BackColor = Color.FromArgb(120, 136, 159);
        exportBtn.MouseLeave += (s, e) => exportBtn.BackColor = Color.FromArgb(100, 116, 139);
        var refreshBtn = new RoundedButton { Text = "🔄 Обновить", Location = new Point(160, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        refreshBtn.Click += (s, e) => NavigateTo("Logs");
        refreshBtn.MouseEnter += (s, e) => refreshBtn.BackColor = Color.FromArgb(120, 136, 159);
        refreshBtn.MouseLeave += (s, e) => refreshBtn.BackColor = Color.FromArgb(100, 116, 139);
        toolbarPanel.Controls.Add(exportBtn);
        toolbarPanel.Controls.Add(refreshBtn);

        var dgv = CreateStyledDataGridView(20, 130, 1300, 650);
        var logs = _logRepo.GetAll();
        var dataSource = logs.Select(l => new { ID = l.Id, Пользователь = l.UserName, Действие = l.Action, Детали = l.Details, Дата = l.ActionDate }).ToList();
        dgv.DataSource = dataSource;

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(toolbarPanel);
        panel.Controls.Add(dgv);
        _currentPanel?.Controls.Add(panel);
    }

    private void ShowUsers()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };
        var titleLabel = new Label { Text = "👥 Пользователи", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(59, 130, 246), AutoSize = true, Location = new Point(20, 20) };

        var toolbarPanel = new Panel { Location = new Point(20, 60), Size = new Size(1300, 50), BackColor = Color.FromArgb(30, 41, 59) };
        var exportBtn = new RoundedButton { Text = "📥 Выгрузить", Location = new Point(10, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        var authRepo = new AuthRepository();
        exportBtn.Click += (s, e) => ExportToCSV(authRepo.GetAll(), "Пользователи");
        exportBtn.MouseEnter += (s, e) => exportBtn.BackColor = Color.FromArgb(120, 136, 159);
        exportBtn.MouseLeave += (s, e) => exportBtn.BackColor = Color.FromArgb(100, 116, 139);
        var refreshBtn = new RoundedButton { Text = "🔄 Обновить", Location = new Point(160, 5), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, Cursor = Cursors.Hand, BorderRadius = 8 };
        refreshBtn.Click += (s, e) => NavigateTo("Users");
        refreshBtn.MouseEnter += (s, e) => refreshBtn.BackColor = Color.FromArgb(120, 136, 159);
        refreshBtn.MouseLeave += (s, e) => refreshBtn.BackColor = Color.FromArgb(100, 116, 139);
        toolbarPanel.Controls.Add(exportBtn);
        toolbarPanel.Controls.Add(refreshBtn);

        var dgv = CreateStyledDataGridView(20, 130, 1300, 650);
        var users = authRepo.GetAll();
        var dataSource = users.Select(u => new { ID = u.Id, Логин = u.Login, ПолноеИмя = u.FullName, Email = u.Email, Роль = u.RoleName, Активен = u.IsActive }).ToList();
        dgv.DataSource = dataSource;

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(toolbarPanel);
        panel.Controls.Add(dgv);
        _currentPanel?.Controls.Add(panel);
    }

    private DataGridView CreateStyledDataGridView(int x, int y, int width, int height)
    {
        var dgv = new DataGridView
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackgroundColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            ReadOnly = true,
            GridColor = Color.FromArgb(45, 60, 80),
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 60, 80);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        dgv.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
        dgv.DefaultCellStyle.ForeColor = Color.White;
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(59, 130, 246);
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;

        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(25, 35, 50);
        dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
        dgv.AlternatingRowsDefaultCellStyle.Font = new Font("Segoe UI", 10);
        dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(59, 130, 246);
        dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

        dgv.RowsDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
        dgv.RowsDefaultCellStyle.ForeColor = Color.White;

        return dgv;
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

    private void ShowNotification(string message, string title, NotificationType type)
    {
        var notification = new NotificationForm(message, title, type);
        notification.Show();
    }

    private void CheckCriticalStock()
    {
        var products = _productRepo.GetAll();
        var criticalProducts = products.Where(p => p.Quantity == 0).ToList();
        var lowStockProducts = products.Where(p => p.Quantity > 0 && p.Quantity <= p.MinQuantity).ToList();

        if (criticalProducts.Count > 0)
        {
            var message = $"⚠️ ВНИМАНИЕ!\n\n🔴 Товаров без запаса: {criticalProducts.Count}\n";
            foreach (var p in criticalProducts.Take(3))
            {
                message += $"  • {p.Article} - {p.Name}\n";
            }
            if (criticalProducts.Count > 3)
                message += $"  ... и еще {criticalProducts.Count - 3}\n";

            MessageBox.Show(message, "Критический уровень запаса", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        else if (lowStockProducts.Count > 0)
        {
            var message = $"⚠️ ВНИМАНИЕ!\n\n🟡 Товаров с низким запасом: {lowStockProducts.Count}\n";
            foreach (var p in lowStockProducts.Take(3))
            {
                message += $"  • {p.Article} - {p.Name} ({p.Quantity} шт)\n";
            }
            if (lowStockProducts.Count > 3)
                message += $"  ... и еще {lowStockProducts.Count - 3}\n";

            MessageBox.Show(message, "Низкий уровень запаса", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private List<int> _favoriteProductIds = new();
    private bool _isDarkTheme = true;
    private Dictionary<int, int> _cartItems = new();

    private void ShowAdminStats()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };

        var titleLabel = new Label
        {
            Text = "📈 Расширенная статистика",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var products = _productRepo.GetAll();
        var suppliers = _supplierRepo.GetAll();
        var categories = _categoryRepo.GetAll();

        var supplierStats = suppliers.Select(s => new
        {
            Поставщик = s.Name,
            Товаров = products.Count(p => p.SupplierId == s.Id),
            СредняяЦена = products.Where(p => p.SupplierId == s.Id).Average(p => (decimal?)p.Price) ?? 0,
            ОбщаяСтоимость = products.Where(p => p.SupplierId == s.Id).Sum(p => p.Price * p.Quantity)
        }).OrderByDescending(x => x.Товаров).ToList();

        var categoryStats = categories.Select(c => new
        {
            Категория = c.Name,
            Товаров = products.Count(p => p.CategoryId == c.Id),
            СредняяЦена = products.Where(p => p.CategoryId == c.Id).Average(p => (decimal?)p.Price) ?? 0,
            ОбщаяСтоимость = products.Where(p => p.CategoryId == c.Id).Sum(p => p.Price * p.Quantity)
        }).OrderByDescending(x => x.Товаров).ToList();

        var supplierLabel = new Label
        {
            Text = "🚚 Статистика по поставщикам",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 70)
        };

        var supplierDgv = CreateStyledDataGridView(20, 110, 1300, 180);
        supplierDgv.DataSource = supplierStats;

        var categoryLabel = new Label
        {
            Text = "📂 Статистика по категориям",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 310)
        };

        var categoryDgv = CreateStyledDataGridView(20, 350, 1300, 180);
        categoryDgv.DataSource = categoryStats;

        var chartsLabel = new Label
        {
            Text = "📊 Графики",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 550)
        };

        var chartPanel = new Panel
        {
            Location = new Point(20, 590),
            Size = new Size(1300, 250),
            BackColor = Color.FromArgb(30, 41, 59)
        };

        var supplierChartLabel = new Label
        {
            Text = "Товаров по поставщикам: " + string.Join(", ", supplierStats.Select(s => $"{s.Поставщик}({s.Товаров})")),
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(10, 10)
        };

        var categoryChartLabel = new Label
        {
            Text = "Товаров по категориям: " + string.Join(", ", categoryStats.Select(c => $"{c.Категория}({c.Товаров})")),
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(10, 120)
        };

        chartPanel.Controls.Add(supplierChartLabel);
        chartPanel.Controls.Add(categoryChartLabel);

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(supplierLabel);
        panel.Controls.Add(supplierDgv);
        panel.Controls.Add(categoryLabel);
        panel.Controls.Add(categoryDgv);
        panel.Controls.Add(chartsLabel);
        panel.Controls.Add(chartPanel);

        _currentPanel?.Controls.Add(panel);
    }

    private void ShowCart()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };

        var titleLabel = new Label
        {
            Text = "🛒 Корзина товаров",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var products = _productRepo.GetAll();
        var cartData = _cartItems.Select(ci => new
        {
            ID = ci.Key,
            Артикул = products.FirstOrDefault(p => p.Id == ci.Key)?.Article ?? "N/A",
            Название = products.FirstOrDefault(p => p.Id == ci.Key)?.Name ?? "N/A",
            Цена = products.FirstOrDefault(p => p.Id == ci.Key)?.Price ?? 0,
            Количество = ci.Value,
            Сумма = (products.FirstOrDefault(p => p.Id == ci.Key)?.Price ?? 0) * ci.Value
        }).ToList();

        if (cartData.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "Корзина пуста. Добавьте товары на странице товаров!",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(20, 80)
            };
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(emptyLabel);
            _currentPanel?.Controls.Add(panel);
            return;
        }

        var dgv = CreateStyledDataGridView(20, 70, 1300, 500);
        dgv.DataSource = cartData;

        var totalLabel = new Label
        {
            Text = $"Итого: {cartData.Sum(c => c.Сумма):F2} руб",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(34, 197, 94),
            AutoSize = true,
            Location = new Point(20, 590)
        };

        var clearBtn = new RoundedButton
        {
            Text = "🗑️ Очистить корзину",
            Size = new Size(200, 40),
            Location = new Point(20, 630),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        clearBtn.MouseEnter += (s, e) => clearBtn.BackColor = Color.FromArgb(120, 136, 159);
        clearBtn.MouseLeave += (s, e) => clearBtn.BackColor = Color.FromArgb(100, 116, 139);
        clearBtn.Click += (s, e) =>
        {
            _cartItems.Clear();
            NavigateTo("Cart");
            ShowNotification("Корзина очищена", "Успех", NotificationType.Success);
        };

        var orderBtn = new RoundedButton
        {
            Text = "✅ Оформить заказ",
            Size = new Size(200, 40),
            Location = new Point(230, 630),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        orderBtn.MouseEnter += (s, e) => orderBtn.BackColor = Color.FromArgb(37, 99, 235);
        orderBtn.MouseLeave += (s, e) => orderBtn.BackColor = Color.FromArgb(59, 130, 246);
        orderBtn.Click += (s, e) =>
        {
            ShowNotification($"Заказ оформлен! Сумма: {cartData.Sum(c => c.Сумма):F2} руб", "Успех", NotificationType.Success);
            _cartItems.Clear();
            NavigateTo("Cart");
        };

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(dgv);
        panel.Controls.Add(totalLabel);
        panel.Controls.Add(clearBtn);
        panel.Controls.Add(orderBtn);

        _currentPanel?.Controls.Add(panel);
    }

    private void ShowFavoriteProducts()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 23, 42) };

        var titleLabel = new Label
        {
            Text = "⭐ Избранные товары",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(251, 146, 60),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var products = _productRepo.GetAll();
        var favorites = products.Where(p => _favoriteProductIds.Contains(p.Id)).ToList();

        if (favorites.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "Нет избранных товаров. Добавьте товары в избранное на странице товаров!",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(20, 80)
            };
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(emptyLabel);
            _currentPanel?.Controls.Add(panel);
            return;
        }

        var dgv = CreateStyledDataGridView(20, 70, 1300, 600);
        var dataSource = favorites.Select(p => new {
            ID = p.Id,
            Артикул = p.Article,
            Название = p.Name,
            Категория = p.CategoryName,
            Цена = p.Price,
            Количество = p.Quantity,
            Поставщик = p.SupplierName
        }).ToList();
        dgv.DataSource = dataSource;

        var clearBtn = new RoundedButton
        {
            Text = "🗑️ Очистить избранное",
            Size = new Size(200, 40),
            Location = new Point(20, 680),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        clearBtn.MouseEnter += (s, e) => clearBtn.BackColor = Color.FromArgb(120, 136, 159);
        clearBtn.MouseLeave += (s, e) => clearBtn.BackColor = Color.FromArgb(100, 116, 139);
        clearBtn.Click += (s, e) =>
        {
            _favoriteProductIds.Clear();
            NavigateTo("Favorites");
        };

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(dgv);
        panel.Controls.Add(clearBtn);

        _currentPanel?.Controls.Add(panel);
    }

    private void Logout()
    {
        CurrentSession.Logout();
        this.DialogResult = DialogResult.Cancel;
        this.Close();
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

    private void ExportToCSV(dynamic data, string fileName)
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                FileName = $"{fileName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv",
                Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                {
                    var list = (System.Collections.IEnumerable)data;
                    var enumerator = list.GetEnumerator();

                    if (enumerator.MoveNext())
                    {
                        var firstItem = enumerator.Current;
                        var properties = firstItem.GetType().GetProperties();

                        var headers = new List<string>();
                        foreach (var prop in properties)
                        {
                            headers.Add(prop.Name);
                        }
                        writer.WriteLine(string.Join(";", headers));

                        var values = new List<string>();
                        foreach (var prop in properties)
                        {
                            values.Add(firstItem.GetType().GetProperty(prop.Name)?.GetValue(firstItem)?.ToString() ?? "");
                        }
                        writer.WriteLine(string.Join(";", values));

                        while (enumerator.MoveNext())
                        {
                            var item = enumerator.Current;
                            values.Clear();
                            foreach (var prop in properties)
                            {
                                values.Add(item.GetType().GetProperty(prop.Name)?.GetValue(item)?.ToString() ?? "");
                            }
                            writer.WriteLine(string.Join(";", values));
                        }
                    }
                }
                ShowNotification("Данные успешно выгружены в CSV", "Успех", NotificationType.Success);
            }
        }
        catch (Exception ex)
        {
            ShowNotification($"Ошибка при выгрузке: {ex.Message}", "Ошибка", NotificationType.Error);
        }
    }

    private void SendEmailReport(string recipientEmail, string subject, string body, string attachmentPath = null)
    {
        try
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential("warehouse.report@gmail.com", "your_app_password");

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("warehouse.report@gmail.com", "Warehouse Management"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(recipientEmail);

                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                {
                    mailMessage.Attachments.Add(new Attachment(attachmentPath));
                }

                client.Send(mailMessage);
                ShowNotification($"Отчёт отправлен на {recipientEmail}", "Успех", NotificationType.Success);
            }
        }
        catch (Exception ex)
        {
            ShowNotification($"Ошибка при отправке: {ex.Message}", "Ошибка", NotificationType.Error);
        }
    }

    private void ExportToPDF(dynamic data, string fileName)
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                FileName = $"{fileName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt",
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var list = (System.Collections.IEnumerable)data;
                var items = new List<dynamic>();
                foreach (var item in list)
                {
                    items.Add(item);
                }

                if (items.Count == 0)
                {
                    ShowNotification("Нет данных для экспорта", "Информация", NotificationType.Info);
                    return;
                }

                var firstItem = items[0];
                var properties = firstItem.GetType().GetProperties();

                using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("═══════════════════════════════════════════════════════════════");
                    writer.WriteLine($"  {fileName} - {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                    writer.WriteLine("═══════════════════════════════════════════════════════════════\n");

                    var headers = new List<string>();
                    foreach (var prop in properties)
                    {
                        headers.Add(prop.Name.PadRight(15));
                    }
                    writer.WriteLine(string.Join(" | ", headers));
                    writer.WriteLine(new string('─', properties.Length * 17));

                    foreach (var item in items)
                    {
                        var values = new List<string>();
                        foreach (var prop in properties)
                        {
                            values.Add((prop.GetValue(item)?.ToString() ?? "").PadRight(15));
                        }
                        writer.WriteLine(string.Join(" | ", values));
                    }

                    writer.WriteLine("\n═══════════════════════════════════════════════════════════════");
                    writer.WriteLine($"Всего записей: {items.Count}");
                    writer.WriteLine("═══════════════════════════════════════════════════════════════");
                }

                ShowNotification("Отчёт успешно создан", "Успех", NotificationType.Success);
            }
        }
        catch (Exception ex)
        {
            ShowNotification($"Ошибка при создании отчёта: {ex.Message}", "Ошибка", NotificationType.Error);
        }
    }
}
