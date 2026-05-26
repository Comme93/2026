namespace WarehouseManagement.Models;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public static class PermissionConstants
{
    public const string VIEW_PRODUCTS = "VIEW_PRODUCTS";
    public const string EDIT_PRODUCTS = "EDIT_PRODUCTS";
    public const string DELETE_PRODUCTS = "DELETE_PRODUCTS";
    public const string VIEW_CATEGORIES = "VIEW_CATEGORIES";
    public const string EDIT_CATEGORIES = "EDIT_CATEGORIES";
    public const string DELETE_CATEGORIES = "DELETE_CATEGORIES";
    public const string VIEW_SUPPLIERS = "VIEW_SUPPLIERS";
    public const string EDIT_SUPPLIERS = "EDIT_SUPPLIERS";
    public const string DELETE_SUPPLIERS = "DELETE_SUPPLIERS";
    public const string VIEW_INCOME = "VIEW_INCOME";
    public const string CREATE_INCOME = "CREATE_INCOME";
    public const string VIEW_OUTCOME = "VIEW_OUTCOME";
    public const string CREATE_OUTCOME = "CREATE_OUTCOME";
    public const string VIEW_LOGS = "VIEW_LOGS";
    public const string VIEW_USERS = "VIEW_USERS";
    public const string MANAGE_USERS = "MANAGE_USERS";
}
