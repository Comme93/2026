using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using WarehouseManagement.Models;
using WarehouseManagement.Repositories;

namespace WarehouseManagement.Services;

public class RecommendationService
{
    public List<Product> GetRecommendationsForUser(int userId, int count = 5)
    {
        var recommendations = new List<Product>();

        var purchaseHistory = GetUserPurchaseHistory(userId);
        if (purchaseHistory.Count == 0)
            return GetPopularProducts(count);

        var similarProducts = new Dictionary<int, double>();

        foreach (var purchase in purchaseHistory)
        {
            var similar = GetSimilarProducts(purchase.ProductId, count * 2);
            foreach (var product in similar)
            {
                if (!purchaseHistory.Any(p => p.ProductId == product.Id))
                {
                    if (similarProducts.ContainsKey(product.Id))
                        similarProducts[product.Id] += 0.5;
                    else
                        similarProducts[product.Id] = 0.5;
                }
            }
        }

        var topRecommendations = similarProducts
            .OrderByDescending(x => x.Value)
            .Take(count)
            .Select(x => x.Key)
            .ToList();

        var productRepo = new ProductRepository();
        foreach (var productId in topRecommendations)
        {
            var product = productRepo.GetById(productId);
            if (product != null)
                recommendations.Add(product);
        }

        if (recommendations.Count < count)
        {
            var popular = GetPopularProducts(count - recommendations.Count);
            recommendations.AddRange(popular.Where(p => !recommendations.Any(r => r.Id == p.Id)));
        }

        return recommendations.Take(count).ToList();
    }

    public List<Product> GetSimilarProducts(int productId, int count = 5)
    {
        var similarProducts = new List<Product>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP (@count) ProductId2, SimilarityScore
            FROM ProductSimilarity
            WHERE ProductId1 = @productId
            ORDER BY SimilarityScore DESC", conn);

        cmd.Parameters.AddWithValue("@productId", productId);
        cmd.Parameters.AddWithValue("@count", count);

        using var reader = cmd.ExecuteReader();
        var productIds = new List<int>();
        while (reader.Read())
        {
            productIds.Add(reader.GetInt32(0));
        }

        var productRepo = new ProductRepository();
        foreach (var id in productIds)
        {
            var product = productRepo.GetById(id);
            if (product != null)
                similarProducts.Add(product);
        }

        return similarProducts;
    }

    public List<Product> GetFrequentlyBoughtTogether(int productId, int count = 5)
    {
        var frequentProducts = new List<Product>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP (@count) p2.Id, COUNT(*) as PurchaseCount
            FROM UserPurchaseHistory uph1
            INNER JOIN UserPurchaseHistory uph2 ON uph1.UserId = uph2.UserId
            INNER JOIN Products p2 ON uph2.ProductId = p2.Id
            WHERE uph1.ProductId = @productId
            AND uph2.ProductId != @productId
            GROUP BY p2.Id
            ORDER BY PurchaseCount DESC", conn);

        cmd.Parameters.AddWithValue("@productId", productId);
        cmd.Parameters.AddWithValue("@count", count);

        using var reader = cmd.ExecuteReader();
        var productIds = new List<int>();
        while (reader.Read())
        {
            productIds.Add(reader.GetInt32(0));
        }

        var productRepo = new ProductRepository();
        foreach (var id in productIds)
        {
            var product = productRepo.GetById(id);
            if (product != null)
                frequentProducts.Add(product);
        }

        return frequentProducts;
    }

    public List<Product> GetPopularProducts(int count = 5)
    {
        var popularProducts = new List<Product>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT TOP (@count) ProductId, COUNT(*) as PurchaseCount
            FROM UserPurchaseHistory
            GROUP BY ProductId
            ORDER BY PurchaseCount DESC", conn);

        cmd.Parameters.AddWithValue("@count", count);

        using var reader = cmd.ExecuteReader();
        var productIds = new List<int>();
        while (reader.Read())
        {
            productIds.Add(reader.GetInt32(0));
        }

        var productRepo = new ProductRepository();
        foreach (var id in productIds)
        {
            var product = productRepo.GetById(id);
            if (product != null)
                popularProducts.Add(product);
        }

        return popularProducts;
    }

    public void RecordPurchase(int userId, int productId, int quantity, decimal price)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            INSERT INTO UserPurchaseHistory (UserId, ProductId, Quantity, Price)
            VALUES (@userId, @productId, @quantity, @price)", conn);

        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@productId", productId);
        cmd.Parameters.AddWithValue("@quantity", quantity);
        cmd.Parameters.AddWithValue("@price", price);

        cmd.ExecuteNonQuery();
    }

    public void UpdateProductSimilarity()
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            DELETE FROM ProductSimilarity", conn);
        cmd.ExecuteNonQuery();

        using var selectCmd = new SqlCommand(@"
            SELECT DISTINCT p1.Id, p2.Id
            FROM Products p1
            CROSS JOIN Products p2
            WHERE p1.Id < p2.Id
            AND p1.CategoryId = p2.CategoryId", conn);

        using var reader = selectCmd.ExecuteReader();
        var pairs = new List<(int, int)>();
        while (reader.Read())
        {
            pairs.Add((reader.GetInt32(0), reader.GetInt32(1)));
        }

        foreach (var (productId1, productId2) in pairs)
        {
            var similarity = CalculateSimilarity(productId1, productId2);

            using var insertCmd = new SqlCommand(@"
                INSERT INTO ProductSimilarity (ProductId1, ProductId2, SimilarityScore)
                VALUES (@p1, @p2, @score)", conn);

            insertCmd.Parameters.AddWithValue("@p1", productId1);
            insertCmd.Parameters.AddWithValue("@p2", productId2);
            insertCmd.Parameters.AddWithValue("@score", similarity);

            insertCmd.ExecuteNonQuery();
        }
    }

    private decimal CalculateSimilarity(int productId1, int productId2)
    {
        var productRepo = new ProductRepository();
        var p1 = productRepo.GetById(productId1);
        var p2 = productRepo.GetById(productId2);

        if (p1 == null || p2 == null)
            return 0;

        decimal similarity = 0;

        if (p1.CategoryId == p2.CategoryId)
            similarity += 0.3m;

        if (p1.SupplierId == p2.SupplierId)
            similarity += 0.2m;

        var priceDiff = Math.Abs(p1.Price - p2.Price) / Math.Max(p1.Price, p2.Price);
        similarity += (decimal)(1 - Math.Min(priceDiff, 1)) * 0.2m;

        var purchaseCorrelation = GetPurchaseCorrelation(productId1, productId2);
        similarity += purchaseCorrelation * 0.3m;

        return Math.Min(similarity, 1);
    }

    private decimal GetPurchaseCorrelation(int productId1, int productId2)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT COUNT(DISTINCT uph1.UserId)
            FROM UserPurchaseHistory uph1
            INNER JOIN UserPurchaseHistory uph2 ON uph1.UserId = uph2.UserId
            WHERE uph1.ProductId = @p1 AND uph2.ProductId = @p2", conn);

        cmd.Parameters.AddWithValue("@p1", productId1);
        cmd.Parameters.AddWithValue("@p2", productId2);

        var correlation = (int)cmd.ExecuteScalar();
        return Math.Min(correlation / 10m, 1);
    }

    private List<(int ProductId, int Quantity, decimal Price)> GetUserPurchaseHistory(int userId)
    {
        var history = new List<(int, int, decimal)>();

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(@"
            SELECT ProductId, Quantity, Price
            FROM UserPurchaseHistory
            WHERE UserId = @userId
            ORDER BY PurchaseDate DESC", conn);

        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            history.Add((
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetDecimal(2)
            ));
        }

        return history;
    }
}
