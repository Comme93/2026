using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using WarehouseManagement.Services;

namespace WarehouseManagement.Forms;

public partial class ChartViewerForm : Form
{
    private readonly ChartService _chartService = new();
    private PlotView _plotView;

    public ChartViewerForm()
    {
        InitializeComponent();
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
        this.Text = "📊 Интерактивные графики";
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 10);

        var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(10) };

        var titleLabel = new Label
        {
            Text = "📊 Интерактивные графики",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(10, 10)
        };

        var chartTypeLabel = new Label
        {
            Text = "Тип графика:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(10, 50),
            AutoSize = true
        };

        var chartTypeCombo = new ComboBox
        {
            Location = new Point(10, 75),
            Size = new Size(200, 30),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
        chartTypeCombo.Items.AddRange(new[]
        {
            "📈 Тренд продаж",
            "📊 Распределение",
            "🔄 Сравнение",
            "📦 Уровни запасов",
            "💰 Тренд цен",
            "💵 Стоимость инвентаря",
            "📋 Объем транзакций",
            "⭐ Топ товары",
            "⚠️ Низкие запасы",
            "📉 Спрос"
        });
        chartTypeCombo.SelectedIndex = 0;

        var refreshButton = new Button
        {
            Text = "🔄 Обновить",
            Location = new Point(220, 75),
            Size = new Size(120, 30),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        refreshButton.FlatAppearance.BorderSize = 0;
        refreshButton.Click += (s, e) => RefreshChart(chartTypeCombo.SelectedIndex);

        var exportButton = new Button
        {
            Text = "📥 Экспортировать",
            Location = new Point(350, 75),
            Size = new Size(140, 30),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(34, 197, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        exportButton.FlatAppearance.BorderSize = 0;
        exportButton.Click += (s, e) => ExportChart();

        var closeButton = new Button
        {
            Text = "✕ Закрыть",
            Location = new Point(900, 75),
            Size = new Size(90, 30),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) => this.Close();

        _plotView = new PlotView
        {
            Location = new Point(10, 120),
            Size = new Size(980, 550),
            BackColor = Color.FromArgb(30, 41, 59)
        };

        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(chartTypeLabel);
        mainPanel.Controls.Add(chartTypeCombo);
        mainPanel.Controls.Add(refreshButton);
        mainPanel.Controls.Add(exportButton);
        mainPanel.Controls.Add(closeButton);
        mainPanel.Controls.Add(_plotView);

        this.Controls.Add(mainPanel);

        RefreshChart(0);
    }

    private void RefreshChart(int chartType)
    {
        try
        {
            var model = new PlotModel
            {
                Title = GetChartTitle(chartType),
                TitleFontSize = 16,
                TitleColor = OxyColor.FromRgb(59, 130, 246),
                Background = OxyColor.FromRgb(30, 41, 59),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                PlotAreaBackground = OxyColor.FromRgb(20, 30, 50)
            };

            switch (chartType)
            {
                case 0: CreateSalesTrendChart(model); break;
                case 1: CreateDistributionChart(model); break;
                case 2: CreateComparisonChart(model); break;
                case 3: CreateStockLevelChart(model); break;
                case 4: CreatePriceTrendChart(model); break;
                case 5: CreateInventoryValueChart(model); break;
                case 6: CreateTransactionVolumeChart(model); break;
                case 7: CreateTopProductsChart(model); break;
                case 8: CreateLowStockChart(model); break;
                case 9: CreateDemandChart(model); break;
            }

            _plotView.Model = model;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CreateSalesTrendChart(PlotModel model)
    {
        var lineSeries = new LineSeries
        {
            Title = "Продажи",
            Color = OxyColor.FromRgb(59, 130, 246),
            StrokeThickness = 2
        };

        for (int i = 0; i < 12; i++)
        {
            lineSeries.Points.Add(new DataPoint(i, 1000 + (i * 150) + (i % 3) * 200));
        }

        model.Series.Add(lineSeries);
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, TextColor = OxyColor.FromRgb(255, 255, 255) });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, TextColor = OxyColor.FromRgb(255, 255, 255) });
    }

    private void CreateDistributionChart(PlotModel model)
    {
        var pieSeries = new PieSeries
        {
            InnerDiameter = 0,
            StartAngle = 0,
            Diameter = 0.8
        };

        pieSeries.Slices.Add(new PieSlice("Категория A", 30) { Fill = OxyColor.FromRgb(59, 130, 246) });
        pieSeries.Slices.Add(new PieSlice("Категория B", 25) { Fill = OxyColor.FromRgb(34, 197, 94) });
        pieSeries.Slices.Add(new PieSlice("Категория C", 20) { Fill = OxyColor.FromRgb(239, 68, 68) });
        pieSeries.Slices.Add(new PieSlice("Категория D", 25) { Fill = OxyColor.FromRgb(168, 85, 247) });

        model.Series.Add(pieSeries);
    }

    private void CreateComparisonChart(PlotModel model)
    {
        var barSeries = new BarSeries
        {
            Title = "Сравнение",
            FillColor = OxyColor.FromRgb(59, 130, 246)
        };

        barSeries.Items.Add(new BarItem { Value = 45 });
        barSeries.Items.Add(new BarItem { Value = 38 });
        barSeries.Items.Add(new BarItem { Value = 52 });
        barSeries.Items.Add(new BarItem { Value = 41 });

        model.Series.Add(barSeries);
        model.Axes.Add(new OxyPlot.Axes.CategoryAxis { Position = OxyPlot.Axes.AxisPosition.Left, TextColor = OxyColor.FromRgb(255, 255, 255), ItemsSource = new[] { "Q1", "Q2", "Q3", "Q4" } });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, TextColor = OxyColor.FromRgb(255, 255, 255) });
    }

    private void CreateStockLevelChart(PlotModel model)
    {
        var areaSeries = new AreaSeries
        {
            Title = "Уровень запасов",
            Color = OxyColor.FromRgb(34, 197, 94),
            Fill = OxyColor.FromArgb(100, 34, 197, 94)
        };

        for (int i = 0; i < 12; i++)
        {
            areaSeries.Points.Add(new DataPoint(i, 500 + (i * 50) - (i % 2) * 100));
        }

        model.Series.Add(areaSeries);
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, TextColor = OxyColor.FromRgb(255, 255, 255) });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, TextColor = OxyColor.FromRgb(255, 255, 255) });
    }

    private void CreatePriceTrendChart(PlotModel model)
    {
        var lineSeries = new LineSeries
        {
            Title = "Тренд цен",
            Color = OxyColor.FromRgb(168, 85, 247),
            StrokeThickness = 2
        };

        for (int i = 0; i < 12; i++)
        {
            lineSeries.Points.Add(new DataPoint(i, 100 + (i * 5) + (i % 2) * 3));
        }

        model.Series.Add(lineSeries);
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, TextColor = OxyColor.FromRgb(255, 255, 255) });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, TextColor = OxyColor.FromRgb(255, 255, 255) });
    }

    private void CreateInventoryValueChart(PlotModel model)
    {
        var barSeries = new BarSeries
        {
            Title = "Стоимость инвентаря",
            FillColor = OxyColor.FromRgb(239, 68, 68)
        };

        barSeries.Items.Add(new BarItem { Value = 5000 });
        barSeries.Items.Add(new BarItem { Value = 6200 });
        barSeries.Items.Add(new BarItem { Value = 5800 });
        barSeries.Items.Add(new BarItem { Value = 7100 });

        model.Series.Add(barSeries);
        model.Axes.Add(new OxyPlot.Axes.CategoryAxis { Position = OxyPlot.Axes.AxisPosition.Left, TextColor = OxyColor.FromRgb(255, 255, 255), ItemsSource = new[] { "Янв", "Фев", "Мар", "Апр" } });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, TextColor = OxyColor.FromRgb(255, 255, 255) });
    }

    private void CreateTransactionVolumeChart(PlotModel model)
    {
        var lineSeries = new LineSeries
        {
            Title = "Объем транзакций",
            Color = OxyColor.FromRgb(59, 130, 246),
            StrokeThickness = 2,
            MarkerType = MarkerType.Circle,
            MarkerSize = 5,
            MarkerFill = OxyColor.FromRgb(59, 130, 246)
        };

        for (int i = 0; i < 12; i++)
        {
            lineSeries.Points.Add(new DataPoint(i, 200 + (i * 30)));
        }

        model.Series.Add(lineSeries);
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, TextColor = OxyColor.FromRgb(255, 255, 255) });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, TextColor = OxyColor.FromRgb(255, 255, 255) });
    }

    private void CreateTopProductsChart(PlotModel model)
    {
        var barSeries = new BarSeries
        {
            Title = "Топ товары",
            FillColor = OxyColor.FromRgb(34, 197, 94)
        };

        barSeries.Items.Add(new BarItem { Value = 850 });
        barSeries.Items.Add(new BarItem { Value = 720 });
        barSeries.Items.Add(new BarItem { Value = 680 });
        barSeries.Items.Add(new BarItem { Value = 620 });
        barSeries.Items.Add(new BarItem { Value = 580 });

        model.Series.Add(barSeries);
        model.Axes.Add(new OxyPlot.Axes.CategoryAxis { Position = OxyPlot.Axes.AxisPosition.Left, TextColor = OxyColor.FromRgb(255, 255, 255), ItemsSource = new[] { "Товар 1", "Товар 2", "Товар 3", "Товар 4", "Товар 5" } });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, TextColor = OxyColor.FromRgb(255, 255, 255) });
    }

    private void CreateLowStockChart(PlotModel model)
    {
        var scatterSeries = new ScatterSeries
        {
            Title = "Низкие запасы",
            MarkerType = MarkerType.Diamond,
            MarkerSize = 8,
            MarkerFill = OxyColor.FromRgb(239, 68, 68)
        };

        scatterSeries.Points.Add(new ScatterPoint(1, 50));
        scatterSeries.Points.Add(new ScatterPoint(2, 45));
        scatterSeries.Points.Add(new ScatterPoint(3, 60));
        scatterSeries.Points.Add(new ScatterPoint(4, 40));
        scatterSeries.Points.Add(new ScatterPoint(5, 55));

        model.Series.Add(scatterSeries);
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, TextColor = OxyColor.FromRgb(255, 255, 255) });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, TextColor = OxyColor.FromRgb(255, 255, 255) });
    }

    private void CreateDemandChart(PlotModel model)
    {
        var lineSeries = new LineSeries
        {
            Title = "Спрос",
            Color = OxyColor.FromRgb(168, 85, 247),
            StrokeThickness = 2
        };

        for (int i = 0; i < 12; i++)
        {
            lineSeries.Points.Add(new DataPoint(i, 300 + (i * 40) + (i % 3) * 50));
        }

        model.Series.Add(lineSeries);
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, TextColor = OxyColor.FromRgb(255, 255, 255) });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, TextColor = OxyColor.FromRgb(255, 255, 255) });
    }

    private string GetChartTitle(int chartType)
    {
        return chartType switch
        {
            0 => "📈 Тренд продаж",
            1 => "📊 Распределение по категориям",
            2 => "🔄 Сравнение показателей",
            3 => "📦 Уровни запасов",
            4 => "💰 Тренд цен",
            5 => "💵 Стоимость инвентаря",
            6 => "📋 Объем транзакций",
            7 => "⭐ Топ товары",
            8 => "⚠️ Товары с низким запасом",
            9 => "📉 Прогноз спроса",
            _ => "График"
        };
    }

    private void ExportChart()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "PNG файлы (*.png)|*.png|PDF файлы (*.pdf)|*.pdf",
                DefaultExt = ".png",
                FileName = $"Chart_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("✓ График экспортирован", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
