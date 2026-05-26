using System;
using System.Windows.Forms;
using System.Drawing;

namespace WarehouseManagement.Controls;

public class NotificationForm : Form
{
    private Label messageLabel;
    private System.Windows.Forms.Timer hideTimer;
    private System.Windows.Forms.Timer fadeTimer;
    private float opacity = 1f;

    public NotificationForm(string message, string title, NotificationType type)
    {
        InitializeComponent();
        SetupUI(message, title, type);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ResumeLayout(false);
    }

    private void SetupUI(string message, string title, NotificationType type)
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.Size = new Size(350, 100);
        this.TopMost = true;
        this.ShowInTaskbar = false;
        this.DoubleBuffered = true;

        var (bgColor, titleColor, icon) = type switch
        {
            NotificationType.Success => (Color.FromArgb(34, 197, 94), Color.FromArgb(22, 163, 74), "✅"),
            NotificationType.Error => (Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38), "❌"),
            NotificationType.Warning => (Color.FromArgb(251, 146, 60), Color.FromArgb(234, 88, 12), "⚠️"),
            NotificationType.Info => (Color.FromArgb(59, 130, 246), Color.FromArgb(37, 99, 235), "ℹ️"),
            _ => (Color.FromArgb(100, 116, 139), Color.FromArgb(71, 85, 105), "•")
        };

        this.BackColor = bgColor;

        var titleLabel = new Label
        {
            Text = $"{icon} {title}",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Location = new Point(15, 10),
            Size = new Size(320, 25)
        };

        messageLabel = new Label
        {
            Text = message,
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            AutoSize = false,
            Location = new Point(15, 40),
            Size = new Size(320, 45),
            TextAlign = ContentAlignment.TopLeft
        };

        this.Controls.Add(titleLabel);
        this.Controls.Add(messageLabel);

        var screen = Screen.PrimaryScreen;
        this.Location = new Point(screen.WorkingArea.Right - this.Width - 20, screen.WorkingArea.Bottom - this.Height - 20);

        hideTimer = new System.Windows.Forms.Timer();
        hideTimer.Interval = 4000;
        hideTimer.Tick += (s, e) =>
        {
            hideTimer.Stop();
            StartFadeOut();
        };
        hideTimer.Start();

        this.MouseClick += (s, e) => this.Close();
    }

    private void StartFadeOut()
    {
        fadeTimer = new System.Windows.Forms.Timer();
        fadeTimer.Interval = 30;
        fadeTimer.Tick += (s, e) =>
        {
            opacity -= 0.05f;
            if (opacity <= 0)
            {
                fadeTimer.Stop();
                this.Close();
            }
            else
            {
                this.Opacity = opacity;
            }
        };
        fadeTimer.Start();
    }
}

public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info
}
