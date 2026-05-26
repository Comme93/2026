namespace WarehouseManagement.Models;

public class IncomeTransaction
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
}
