using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;

namespace WarehouseManagement.Services;

public class ChartData
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
    public string Title { get; set; }
    public string ChartType { get; set; }
}

public class ChartService
{
    public ChartData GetSalesTrendData(int daysBack = 30)
    {
        var chartData = new ChartData
        {
            Title = "Тренд продаж",
            ChartType = "Line"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT CAST(TransactionDate AS DATE) as Date, SUM(Quantity * Price) as TotalSales
            FROM OutcomeTransactions
            WHERE TransactionDate >= DATEADD(DAY, -@days, CAST(GETDATE() AS DATE))
            GROUP BY CAST(TransactionDate AS DATE)
            ORDER BY Date ASC", conn);

        cmd.Parameters.AddWithValue("@days", daysBack);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetDateTime(0).ToString("yyyy-MM-dd"));
            chartData.Values.Add(reader.GetDecimal(1));
        }

        return chartData;
    }

    public ChartData GetProductDistributionData()
    {
        var chartData = new ChartData
        {
            Title = "Распределение товаров по категориям",
            ChartType = "Pie"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT c.Name, COUNT(p.Id) as ProductCount
            FROM Categories c
            LEFT JOIN Products p ON c.Id = p.CategoryId
            GROUP BY c.Id, c.Name
            ORDER BY ProductCount DESC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetString(0));
            chartData.Values.Add(reader.GetInt32(1));
        }

        return chartData;
    }

    public ChartData GetSupplierComparisonData()
    {
        var chartData = new ChartData
        {
            Title = "Сравнение поставщиков",
            ChartType = "Bar"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT s.Name, COUNT(p.Id) as ProductCount
            FROM Suppliers s
            LEFT JOIN Products p ON s.Id = p.SupplierId
            GROUP BY s.Id, s.Name
            ORDER BY ProductCount DESC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetString(0));
            chartData.Values.Add(reader.GetInt32(1));
        }

        return chartData;
    }

    public ChartData GetCategoryPerformanceData()
    {
        var chartData = new ChartData
        {
            Title = "Производительность категорий",
            ChartType = "Bar"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT c.Name, SUM(p.Quantity * p.Price) as TotalValue
            FROM Categories c
            LEFT JOIN Products p ON c.Id = p.CategoryId
            GROUP BY c.Id, c.Name
            ORDER BY TotalValue DESC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetString(0));
            chartData.Values.Add(reader.GetDecimal(1));
        }

        return chartData;
    }

    public ChartData GetStockLevelsData()
    {
        var chartData = new ChartData
        {
            Title = "Уровни запасов",
            ChartType = "Line"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP 10 Name, Quantity
            FROM Products
            ORDER BY Quantity DESC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetString(0));
            chartData.Values.Add(reader.GetInt32(1));
        }

        return chartData;
    }

    public ChartData GetPriceTrendsData(int daysBack = 90)
    {
        var chartData = new ChartData
        {
            Title = "Тренды цен",
            ChartType = "Line"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT CAST(ChangedAt AS DATE) as Date, AVG(NewPrice) as AvgPrice
            FROM PriceHistory
            WHERE ChangedAt >= DATEADD(DAY, -@days, CAST(GETDATE() AS DATE))
            GROUP BY CAST(ChangedAt AS DATE)
            ORDER BY Date ASC", conn);

        cmd.Parameters.AddWithValue("@days", daysBack);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetDateTime(0).ToString("yyyy-MM-dd"));
            chartData.Values.Add(reader.GetDecimal(1));
        }

        return chartData;
    }

    public ChartData GetInventoryValueData()
    {
        var chartData = new ChartData
        {
            Title = "Стоимость инвентаря по категориям",
            ChartType = "Pie"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT c.Name, SUM(p.Quantity * p.Price) as TotalValue
            FROM Categories c
            LEFT JOIN Products p ON c.Id = p.CategoryId
            GROUP BY c.Id, c.Name
            ORDER BY TotalValue DESC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetString(0));
            chartData.Values.Add(reader.GetDecimal(1));
        }

        return chartData;
    }

    public ChartData GetTransactionVolumeData(int daysBack = 30)
    {
        var chartData = new ChartData
        {
            Title = "Объем транзакций",
            ChartType = "Bar"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT CAST(TransactionDate AS DATE) as Date, COUNT(*) as TransactionCount
            FROM OutcomeTransactions
            WHERE TransactionDate >= DATEADD(DAY, -@days, CAST(GETDATE() AS DATE))
            GROUP BY CAST(TransactionDate AS DATE)
            ORDER BY Date ASC", conn);

        cmd.Parameters.AddWithValue("@days", daysBack);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetDateTime(0).ToString("yyyy-MM-dd"));
            chartData.Values.Add(reader.GetInt32(1));
        }

        return chartData;
    }

    public ChartData GetTopProductsData(int count = 10)
    {
        var chartData = new ChartData
        {
            Title = "Топ товаров по продажам",
            ChartType = "Bar"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP (@count) p.Name, SUM(ot.Quantity) as TotalSold
            FROM OutcomeTransactions ot
            INNER JOIN Products p ON ot.ProductId = p.Id
            GROUP BY p.Id, p.Name
            ORDER BY TotalSold DESC", conn);

        cmd.Parameters.AddWithValue("@count", count);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetString(0));
            chartData.Values.Add(reader.GetInt32(1));
        }

        return chartData;
    }

    public ChartData GetLowStockProductsData()
    {
        var chartData = new ChartData
        {
            Title = "Товары с низким запасом",
            ChartType = "Bar"
        };

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP 10 Name, Quantity
            FROM Products
            WHERE Quantity <= MinQuantity
            ORDER BY Quantity ASC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            chartData.Labels.Add(reader.GetString(0));
            chartData.Values.Add(reader.GetInt32(1));
        }

        return chartData;
    }
}
