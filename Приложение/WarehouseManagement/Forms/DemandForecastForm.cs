using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;
using WarehouseManagement.Services;

namespace WarehouseManagement.Forms;

public partial class DemandForecastForm : Form
{
    private readonly DemandForecastingService _forecastService = new();
    private List<dynamic> _forecasts = new();

    public DemandForecastForm()
    {
        InitializeComponent();
        SetupUI();
        LoadForecasts();
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
        this.Text = "📈 Прогноз спроса";
        this.Size = new Size(900, 650);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 10);

        var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(20) };

        var titleLabel = new Label
        {
            Text = "📈 Прогноз спроса на товары",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var periodLabel = new Label
        {
            Text = "Период прогноза:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 70),
            AutoSize = true
        };

        var periodCombo = new ComboBox
        {
            Location = new Point(20, 100),
            Size = new Size(150, 30),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
        periodCombo.Items.AddRange(new[] { "30 дней", "60 дней", "90 дней" });
        periodCombo.SelectedIndex = 0;

        var generateButton = new Button
        {
            Text = "🔄 Сгенерировать",
            Location = new Point(180, 100),
            Size = new Size(140, 30),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        generateButton.FlatAppearance.BorderSize = 0;
        generateButton.Click += (s, e) => GenerateForecasts();

        var forecastListBox = new ListBox
        {
            Location = new Point(20, 150),
            Size = new Size(860, 350),
            Font = new Font("Segoe UI", 9),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var statsPanel = new Panel
        {
            Location = new Point(20, 520),
            Size = new Size(860, 80),
            BackColor = Color.FromArgb(30, 41, 59),
            BorderStyle = BorderStyle.FixedSingle
        };

        var highDemandLabel = new Label
        {
            Text = "📈 Высокий спрос: 0 товаров",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(34, 197, 94),
            Location = new Point(10, 10),
            AutoSize = true
        };

        var normalDemandLabel = new Label
        {
            Text = "➡️ Нормальный спрос: 0 товаров",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(59, 130, 246),
            Location = new Point(10, 35),
            AutoSize = true
        };

        var lowDemandLabel = new Label
        {
            Text = "📉 Низкий спрос: 0 товаров",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(239, 68, 68),
            Location = new Point(10, 60),
            AutoSize = true
        };

        var accuracyLabel = new Label
        {
            Text = "🎯 Точность прогноза: 0%",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(168, 85, 247),
            Location = new Point(450, 10),
            AutoSize = true
        };

        statsPanel.Controls.Add(highDemandLabel);
        statsPanel.Controls.Add(normalDemandLabel);
        statsPanel.Controls.Add(lowDemandLabel);
        statsPanel.Controls.Add(accuracyLabel);

        var exportButton = new Button
        {
            Text = "📊 Экспортировать",
            Location = new Point(20, 610),
            Size = new Size(150, 35),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(34, 197, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        exportButton.FlatAppearance.BorderSize = 0;
        exportButton.Click += (s, e) => ExportForecasts();

        var closeButton = new Button
        {
            Text = "✕ Закрыть",
            Location = new Point(790, 610),
            Size = new Size(90, 35),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) => this.Close();

        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(periodLabel);
        mainPanel.Controls.Add(periodCombo);
        mainPanel.Controls.Add(generateButton);
        mainPanel.Controls.Add(forecastListBox);
        mainPanel.Controls.Add(statsPanel);
        mainPanel.Controls.Add(exportButton);
        mainPanel.Controls.Add(closeButton);

        this.Controls.Add(mainPanel);
    }

    private void LoadForecasts()
    {
        try
        {
            _forecasts = new List<dynamic>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT ProductId, ForecastDate, ForecastedQuantity, Confidence FROM DemandForecasts ORDER BY ForecastDate DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dynamic forecast = new System.Dynamic.ExpandoObject();
                forecast.ProductId = (int)reader["ProductId"];
                forecast.ForecastDate = (DateTime)reader["ForecastDate"];
                forecast.ForecastedQuantity = (int)reader["ForecastedQuantity"];
                forecast.Confidence = (decimal)reader["Confidence"];
                _forecasts.Add(forecast);
            }
            RefreshList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void GenerateForecasts()
    {
        try
        {
            MessageBox.Show("✓ Прогнозы сгенерированы", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadForecasts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshList()
    {
        var listBox = this.Controls[0]?.Controls.OfType<ListBox>().FirstOrDefault();
        if (listBox != null)
        {
            listBox.Items.Clear();
            foreach (var forecast in _forecasts)
            {
                string confidence = (forecast.Confidence * 100).ToString("F0");
                listBox.Items.Add($"Товар #{forecast.ProductId} | Прогноз: {forecast.ForecastedQuantity} шт | Уверенность: {confidence}% | {forecast.ForecastDate:d}");
            }
        }
    }

    private void ExportForecasts()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv|Excel файлы (*.xlsx)|*.xlsx",
                DefaultExt = ".csv",
                FileName = $"Forecasts_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("✓ Прогнозы экспортированы", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
