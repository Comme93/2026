using WarehouseManagement.Models;
using WarehouseManagement.Repositories;

namespace WarehouseManagement;

public static class CurrentSession
{
    public static AppUser? CurrentUser { get; set; }
    public static bool IsLoggedIn => CurrentUser != null;
    private static PermissionRepository? _permissionRepo;

    public static bool HasPermission(string permissionName)
    {
        if (CurrentUser == null) return false;
        _permissionRepo ??= new PermissionRepository();
        return _permissionRepo.HasPermission(CurrentUser.Id, permissionName);
    }

    public static bool IsAdmin => CurrentUser?.RoleId == 1;

    public static void Logout()
    {
        CurrentUser = null;
    }
}
