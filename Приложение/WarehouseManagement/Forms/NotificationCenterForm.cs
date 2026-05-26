using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;
using WarehouseManagement.Services;
using WarehouseManagement.Controls;

namespace WarehouseManagement.Forms;

public partial class NotificationCenterForm : Form
{
    private readonly NotificationService _notificationService = new();
    private readonly int _userId;
    private List<dynamic> _notifications = new();

    public NotificationCenterForm(int userId)
    {
        InitializeComponent();
        _userId = userId;
        SetupUI();
        LoadNotifications();
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
        this.Text = "🔔 Центр уведомлений";
        this.Size = new Size(700, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 10);

        var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(20) };

        var titleLabel = new Label
        {
            Text = "🔔 Центр уведомлений",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var filterPanel = new Panel
        {
            Location = new Point(20, 60),
            Size = new Size(660, 50),
            BackColor = Color.FromArgb(30, 41, 59),
            BorderStyle = BorderStyle.FixedSingle
        };

        var allButton = new RoundedButton
        {
            Text = "📋 Все",
            Location = new Point(10, 10),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 6
        };
        allButton.MouseEnter += (s, e) => allButton.BackColor = Color.FromArgb(37, 99, 235);
        allButton.MouseLeave += (s, e) => allButton.BackColor = Color.FromArgb(59, 130, 246);
        allButton.Click += (s, e) => FilterNotifications("All");

        var unreadButton = new RoundedButton
        {
            Text = "📌 Непрочитанные",
            Location = new Point(100, 10),
            Size = new Size(120, 30),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 6
        };
        unreadButton.MouseEnter += (s, e) => unreadButton.BackColor = Color.FromArgb(120, 136, 159);
        unreadButton.MouseLeave += (s, e) => unreadButton.BackColor = Color.FromArgb(100, 116, 139);
        unreadButton.Click += (s, e) => FilterNotifications("Unread");

        var stockButton = new RoundedButton
        {
            Text = "📦 Склад",
            Location = new Point(230, 10),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 6
        };
        stockButton.MouseEnter += (s, e) => stockButton.BackColor = Color.FromArgb(120, 136, 159);
        stockButton.MouseLeave += (s, e) => stockButton.BackColor = Color.FromArgb(100, 116, 139);
        stockButton.Click += (s, e) => FilterNotifications("Stock");

        var priceButton = new RoundedButton
        {
            Text = "💰 Цена",
            Location = new Point(320, 10),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 6
        };
        priceButton.MouseEnter += (s, e) => priceButton.BackColor = Color.FromArgb(120, 136, 159);
        priceButton.MouseLeave += (s, e) => priceButton.BackColor = Color.FromArgb(100, 116, 139);
        priceButton.Click += (s, e) => FilterNotifications("Price");

        filterPanel.Controls.Add(allButton);
        filterPanel.Controls.Add(unreadButton);
        filterPanel.Controls.Add(stockButton);
        filterPanel.Controls.Add(priceButton);

        var notificationListBox = new ListBox
        {
            Location = new Point(20, 120),
            Size = new Size(660, 350),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var markReadButton = new RoundedButton
        {
            Text = "✓ Отметить как прочитанное",
            Location = new Point(20, 480),
            Size = new Size(200, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        markReadButton.MouseEnter += (s, e) => markReadButton.BackColor = Color.FromArgb(37, 99, 235);
        markReadButton.MouseLeave += (s, e) => markReadButton.BackColor = Color.FromArgb(59, 130, 246);
        markReadButton.Click += (s, e) => MarkAsRead(notificationListBox.SelectedIndex);

        var deleteButton = new RoundedButton
        {
            Text = "🗑️ Удалить",
            Location = new Point(230, 480),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        deleteButton.MouseEnter += (s, e) => deleteButton.BackColor = Color.FromArgb(120, 136, 159);
        deleteButton.MouseLeave += (s, e) => deleteButton.BackColor = Color.FromArgb(100, 116, 139);
        deleteButton.Click += (s, e) => DeleteNotification(notificationListBox.SelectedIndex);

        var closeButton = new RoundedButton
        {
            Text = "✕ Закрыть",
            Location = new Point(590, 480),
            Size = new Size(90, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        closeButton.MouseEnter += (s, e) => closeButton.BackColor = Color.FromArgb(120, 136, 159);
        closeButton.MouseLeave += (s, e) => closeButton.BackColor = Color.FromArgb(100, 116, 139);
        closeButton.Click += (s, e) => this.Close();

        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(filterPanel);
        mainPanel.Controls.Add(notificationListBox);
        mainPanel.Controls.Add(markReadButton);
        mainPanel.Controls.Add(deleteButton);
        mainPanel.Controls.Add(closeButton);

        this.Controls.Add(mainPanel);
    }

    private void LoadNotifications()
    {
        try
        {
            _notifications = new List<dynamic>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT Id, Title, Message, Type, IsRead, CreatedAt FROM Notifications WHERE UserId = @userId ORDER BY CreatedAt DESC", conn);
            cmd.Parameters.AddWithValue("@userId", _userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dynamic notif = new System.Dynamic.ExpandoObject();
                notif.Id = (int)reader["Id"];
                notif.Title = reader["Title"].ToString();
                notif.Message = reader["Message"].ToString();
                notif.Type = reader["Type"].ToString();
                notif.IsRead = (bool)reader["IsRead"];
                notif.CreatedAt = (DateTime)reader["CreatedAt"];
                _notifications.Add(notif);
            }
            RefreshList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FilterNotifications(string filter)
    {
        RefreshList();
    }

    private void RefreshList()
    {
        var listBox = this.Controls[0]?.Controls.OfType<ListBox>().FirstOrDefault();
        if (listBox != null)
        {
            listBox.Items.Clear();
            foreach (var notif in _notifications)
            {
                string icon = notif.Type == "Stock" ? "📦" : notif.Type == "Price" ? "💰" : "📌";
                listBox.Items.Add($"{icon} {notif.Title} - {notif.CreatedAt:g}");
            }
        }
    }

    private void MarkAsRead(int index)
    {
        if (index >= 0 && index < _notifications.Count)
        {
            MessageBox.Show("✓ Отмечено как прочитанное", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadNotifications();
        }
    }

    private void DeleteNotification(int index)
    {
        if (index >= 0 && index < _notifications.Count)
        {
            MessageBox.Show("✓ Удалено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadNotifications();
        }
    }
}
