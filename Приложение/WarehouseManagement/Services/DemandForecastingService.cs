using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;

namespace WarehouseManagement.Services;

public class DemandForecastingService
{
    public void GenerateForecast(int productId, int daysAhead = 30)
    {
        var historicalData = GetHistoricalDemand(productId, 365);

        if (historicalData.Count < 7)
            return;

        var forecast = CalculateExponentialSmoothing(historicalData, daysAhead);

        SaveForecast(productId, forecast, "ExponentialSmoothing");
    }

    public void UpdateAllForecasts(int daysAhead = 30)
    {
        var productRepo = new ProductRepository();
        var allProducts = productRepo.GetAll();

        foreach (var product in allProducts)
        {
            GenerateForecast(product.Id, daysAhead);
        }
    }

    public List<(DateTime Date, int ForecastedQuantity, decimal Confidence)> GetForecast(int productId, int daysAhead = 30)
    {
        var forecasts = new List<(DateTime, int, decimal)>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP (@days) ForecastDate, ForecastedQuantity, Confidence
            FROM DemandForecasts
            WHERE ProductId = @productId
            ORDER BY ForecastDate ASC", conn);

        cmd.Parameters.AddWithValue("@productId", productId);
        cmd.Parameters.AddWithValue("@days", daysAhead);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            forecasts.Add((
                reader.GetDateTime(0),
                reader.GetInt32(1),
                reader.GetDecimal(2)
            ));
        }

        return forecasts;
    }

    public decimal GetForecastAccuracy(int productId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT AVG(ABS(Error))
            FROM HistoricalDemand
            WHERE ProductId = @productId AND Error IS NOT NULL", conn);

        cmd.Parameters.AddWithValue("@productId", productId);

        var result = cmd.ExecuteScalar();
        if (result == DBNull.Value)
            return 0;

        var avgError = (decimal)result;
        return Math.Max(0, 100 - avgError);
    }

    public List<(int ProductId, string ProductName, int ForecastedDemand, decimal Confidence)> GetHighDemandProducts(int threshold = 100)
    {
        var highDemandProducts = new List<(int, string, int, decimal)>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP 10 df.ProductId, p.Name, df.ForecastedQuantity, df.Confidence
            FROM DemandForecasts df
            INNER JOIN Products p ON df.ProductId = p.Id
            WHERE df.ForecastedQuantity > @threshold
            AND df.ForecastDate = CAST(GETDATE() AS DATE)
            ORDER BY df.ForecastedQuantity DESC", conn);

        cmd.Parameters.AddWithValue("@threshold", threshold);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            highDemandProducts.Add((
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetDecimal(3)
            ));
        }

        return highDemandProducts;
    }

    public List<(int ProductId, string ProductName, int ForecastedDemand, decimal Confidence)> GetLowDemandProducts(int threshold = 10)
    {
        var lowDemandProducts = new List<(int, string, int, decimal)>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP 10 df.ProductId, p.Name, df.ForecastedQuantity, df.Confidence
            FROM DemandForecasts df
            INNER JOIN Products p ON df.ProductId = p.Id
            WHERE df.ForecastedQuantity < @threshold
            AND df.ForecastDate = CAST(GETDATE() AS DATE)
            ORDER BY df.ForecastedQuantity ASC", conn);

        cmd.Parameters.AddWithValue("@threshold", threshold);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lowDemandProducts.Add((
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetDecimal(3)
            ));
        }

        return lowDemandProducts;
    }

    private List<(DateTime Date, int Quantity)> GetHistoricalDemand(int productId, int daysBack)
    {
        var historicalData = new List<(DateTime, int)>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT Date, ActualQuantity
            FROM HistoricalDemand
            WHERE ProductId = @productId
            AND Date >= DATEADD(DAY, -@days, CAST(GETDATE() AS DATE))
            ORDER BY Date ASC", conn);

        cmd.Parameters.AddWithValue("@productId", productId);
        cmd.Parameters.AddWithValue("@days", daysBack);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            historicalData.Add((
                reader.GetDateTime(0),
                reader.GetInt32(1)
            ));
        }

        return historicalData;
    }

    private List<(DateTime Date, int ForecastedQuantity, decimal Confidence)> CalculateExponentialSmoothing(
        List<(DateTime Date, int Quantity)> historicalData, int daysAhead)
    {
        var forecasts = new List<(DateTime, int, decimal)>();

        if (historicalData.Count == 0)
            return forecasts;

        const double alpha = 0.3;
        double smoothed = historicalData[0].Quantity;

        foreach (var (date, quantity) in historicalData.Skip(1))
        {
            smoothed = alpha * quantity + (1 - alpha) * smoothed;
        }

        var lastDate = historicalData.Last().Date;
        var variance = CalculateVariance(historicalData.Select(x => (double)x.Quantity).ToList());
        var stdDev = Math.Sqrt(variance);

        for (int i = 1; i <= daysAhead; i++)
        {
            var forecastDate = lastDate.AddDays(i);
            var forecastedQuantity = (int)Math.Round(smoothed);
            var confidence = (decimal)Math.Max(0.5, 1 - (stdDev / (smoothed + 1)) * 0.1);

            forecasts.Add((forecastDate, forecastedQuantity, confidence));
        }

        return forecasts;
    }

    private double CalculateVariance(List<double> values)
    {
        if (values.Count == 0)
            return 0;

        var mean = values.Average();
        var variance = values.Sum(x => Math.Pow(x - mean, 2)) / values.Count;

        return variance;
    }

    private void SaveForecast(int productId, List<(DateTime Date, int ForecastedQuantity, decimal Confidence)> forecasts, string method)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var deleteCmd = new SqlCommand(@"
            DELETE FROM DemandForecasts
            WHERE ProductId = @productId", conn);

        deleteCmd.Parameters.AddWithValue("@productId", productId);
        deleteCmd.ExecuteNonQuery();

        foreach (var (date, quantity, confidence) in forecasts)
        {
            using var insertCmd = new SqlCommand(@"
                INSERT INTO DemandForecasts (ProductId, ForecastDate, ForecastedQuantity, Confidence, Method)
                VALUES (@productId, @date, @quantity, @confidence, @method)", conn);

            insertCmd.Parameters.AddWithValue("@productId", productId);
            insertCmd.Parameters.AddWithValue("@date", date);
            insertCmd.Parameters.AddWithValue("@quantity", quantity);
            insertCmd.Parameters.AddWithValue("@confidence", confidence);
            insertCmd.Parameters.AddWithValue("@method", method);

            insertCmd.ExecuteNonQuery();
        }
    }

    public void RecordActualDemand(int productId, int quantity)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        var today = DateTime.Now.Date;

        using var cmd = new SqlCommand(@"
            IF EXISTS (SELECT 1 FROM HistoricalDemand WHERE ProductId = @productId AND Date = @date)
                UPDATE HistoricalDemand
                SET ActualQuantity = ActualQuantity + @quantity
                WHERE ProductId = @productId AND Date = @date
            ELSE
                INSERT INTO HistoricalDemand (ProductId, Date, ActualQuantity)
                VALUES (@productId, @date, @quantity)", conn);

        cmd.Parameters.AddWithValue("@productId", productId);
        cmd.Parameters.AddWithValue("@date", today);
        cmd.Parameters.AddWithValue("@quantity", quantity);

        cmd.ExecuteNonQuery();

        var forecast = GetForecast(productId, 1);
        if (forecast.Count > 0)
        {
            var error = Math.Abs(forecast[0].ForecastedQuantity - quantity);

            using var errorCmd = new SqlCommand(@"
                UPDATE HistoricalDemand
                SET ForecastedQuantity = @forecasted, Error = @error
                WHERE ProductId = @productId AND Date = @date", conn);

            errorCmd.Parameters.AddWithValue("@productId", productId);
            errorCmd.Parameters.AddWithValue("@date", today);
            errorCmd.Parameters.AddWithValue("@forecasted", forecast[0].ForecastedQuantity);
            errorCmd.Parameters.AddWithValue("@error", error);

            errorCmd.ExecuteNonQuery();
        }
    }
}
