namespace WarehouseManagement.Models;

public class UserActionLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}
