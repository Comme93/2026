using WarehouseManagement.Forms;
using WarehouseManagement.Repositories;
using WarehouseManagement.Services;

namespace WarehouseManagement;

static class Program
{
    private static AutoSaveService? _autoSaveService;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            DatabaseHelper.InitializeDatabase();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка инициализации БД: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _autoSaveService = new AutoSaveService();
        _autoSaveService.StartAutoSave(30);

        while (true)
        {
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                LanguageService.LoadLanguagePreference(CurrentSession.CurrentUser?.Id ?? 1);

                var mainForm = new MainForm();
                Application.Run(mainForm);
            }
            else
            {
                break;
            }
        }

        _autoSaveService?.StopAutoSave();
    }
}
