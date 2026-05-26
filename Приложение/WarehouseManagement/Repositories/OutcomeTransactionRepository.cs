using Microsoft.Data.SqlClient;
using WarehouseManagement.Models;

namespace WarehouseManagement.Repositories;

public class OutcomeTransactionRepository
{
    public List<OutcomeTransaction> GetAll()
    {
        var transactions = new List<OutcomeTransaction>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            SELECT ot.Id, ot.ProductId, p.Name as ProductName, ot.Quantity, ot.Price,
                   ot.TransactionDate, ot.Reason, ot.Notes, ot.CreatedByUserId
            FROM OutcomeTransactions ot
            LEFT JOIN Products p ON ot.ProductId = p.Id
            ORDER BY ot.TransactionDate DESC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            transactions.Add(new OutcomeTransaction
            {
                Id = (int)reader["Id"],
                ProductId = (int)reader["ProductId"],
                ProductName = reader["ProductName"].ToString() ?? "",
                Quantity = (int)reader["Quantity"],
                Price = (decimal)reader["Price"],
                TransactionDate = (DateTime)reader["TransactionDate"],
                Reason = reader["Reason"].ToString() ?? "",
                Notes = reader["Notes"].ToString() ?? "",
                CreatedByUserId = reader["CreatedByUserId"] != DBNull.Value ? (int)reader["CreatedByUserId"] : 0
            });
        }
        return transactions;
    }

    public void Insert(OutcomeTransaction transaction)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            INSERT INTO OutcomeTransactions (ProductId, Quantity, Price, Reason, Notes, CreatedByUserId)
            VALUES (@productId, @quantity, @price, @reason, @notes, @createdByUserId)", conn);

        cmd.Parameters.AddWithValue("@productId", transaction.ProductId);
        cmd.Parameters.AddWithValue("@quantity", transaction.Quantity);
        cmd.Parameters.AddWithValue("@price", transaction.Price);
        cmd.Parameters.AddWithValue("@reason", transaction.Reason ?? "");
        cmd.Parameters.AddWithValue("@notes", transaction.Notes ?? "");
        cmd.Parameters.AddWithValue("@createdByUserId", transaction.CreatedByUserId);

        cmd.ExecuteNonQuery();

        using var updateCmd = new SqlCommand("UPDATE Products SET Quantity = Quantity - @qty WHERE Id = @id", conn);
        updateCmd.Parameters.AddWithValue("@qty", transaction.Quantity);
        updateCmd.Parameters.AddWithValue("@id", transaction.ProductId);
        updateCmd.ExecuteNonQuery();
    }
}
