using Microsoft.Data.SqlClient;
using WarehouseManagement.Models;

namespace WarehouseManagement.Repositories;

public class IncomeTransactionRepository
{
    public List<IncomeTransaction> GetAll()
    {
        var transactions = new List<IncomeTransaction>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            SELECT it.Id, it.ProductId, p.Name as ProductName, it.Quantity, it.Price,
                   it.SupplierId, s.Name as SupplierName, it.TransactionDate, it.Notes, it.CreatedByUserId
            FROM IncomeTransactions it
            LEFT JOIN Products p ON it.ProductId = p.Id
            LEFT JOIN Suppliers s ON it.SupplierId = s.Id
            ORDER BY it.TransactionDate DESC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            transactions.Add(new IncomeTransaction
            {
                Id = (int)reader["Id"],
                ProductId = (int)reader["ProductId"],
                ProductName = reader["ProductName"].ToString() ?? "",
                Quantity = (int)reader["Quantity"],
                Price = (decimal)reader["Price"],
                SupplierId = reader["SupplierId"] != DBNull.Value ? (int)reader["SupplierId"] : 0,
                SupplierName = reader["SupplierName"].ToString() ?? "",
                TransactionDate = (DateTime)reader["TransactionDate"],
                Notes = reader["Notes"].ToString() ?? "",
                CreatedByUserId = reader["CreatedByUserId"] != DBNull.Value ? (int)reader["CreatedByUserId"] : 0
            });
        }
        return transactions;
    }

    public void Insert(IncomeTransaction transaction)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            INSERT INTO IncomeTransactions (ProductId, Quantity, Price, SupplierId, Notes, CreatedByUserId)
            VALUES (@productId, @quantity, @price, @supplierId, @notes, @createdByUserId)", conn);

        cmd.Parameters.AddWithValue("@productId", transaction.ProductId);
        cmd.Parameters.AddWithValue("@quantity", transaction.Quantity);
        cmd.Parameters.AddWithValue("@price", transaction.Price);
        cmd.Parameters.AddWithValue("@supplierId", transaction.SupplierId > 0 ? transaction.SupplierId : DBNull.Value);
        cmd.Parameters.AddWithValue("@notes", transaction.Notes ?? "");
        cmd.Parameters.AddWithValue("@createdByUserId", transaction.CreatedByUserId);

        cmd.ExecuteNonQuery();

        using var updateCmd = new SqlCommand("UPDATE Products SET Quantity = Quantity + @qty WHERE Id = @id", conn);
        updateCmd.Parameters.AddWithValue("@qty", transaction.Quantity);
        updateCmd.Parameters.AddWithValue("@id", transaction.ProductId);
        updateCmd.ExecuteNonQuery();
    }
}
