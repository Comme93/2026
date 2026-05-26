using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WarehouseManagement.Repositories;

namespace WarehouseManagement.Services;

public class LanguageService
{
    private static string _currentLanguage = "ru";
    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        {
            "ru", new Dictionary<string, string>
            {
                { "Dashboard", "Панель управления" },
                { "Products", "Товары" },
                { "Categories", "Категории" },
                { "Suppliers", "Поставщики" },
                { "Income", "Приход" },
                { "Outcome", "Расход" },
                { "Logs", "Логи" },
                { "Users", "Пользователи" },
                { "Statistics", "Статистика" },
                { "Favorites", "Избранное" },
                { "Cart", "Корзина" },
                { "Settings", "Настройки" },
                { "Logout", "Выход из аккаунта" },
                { "Login", "Вход" },
                { "Password", "Пароль" },
                { "Username", "Имя пользователя" },
                { "Add", "Добавить" },
                { "Edit", "Редактировать" },
                { "Delete", "Удалить" },
                { "Save", "Сохранить" },
                { "Cancel", "Отмена" },
                { "Search", "Поиск" },
                { "Filter", "Фильтр" },
                { "Clear", "Очистить" },
                { "Export", "Экспорт" },
                { "Import", "Импорт" },
                { "Total", "Итого" },
                { "Price", "Цена" },
                { "Quantity", "Количество" },
                { "Description", "Описание" },
                { "Name", "Название" },
                { "Article", "Артикул" },
                { "Category", "Категория" },
                { "Supplier", "Поставщик" },
                { "Date", "Дата" },
                { "Action", "Действие" },
                { "Status", "Статус" },
                { "Active", "Активно" },
                { "Inactive", "Неактивно" },
                { "Success", "Успешно" },
                { "Error", "Ошибка" },
                { "Warning", "Предупреждение" },
                { "Info", "Информация" },
                { "Confirm", "Подтвердить" },
                { "Yes", "Да" },
                { "No", "Нет" },
                { "Close", "Закрыть" },
                { "Back", "Назад" },
                { "Next", "Далее" },
                { "Previous", "Назад" },
                { "Language", "Язык" },
                { "Theme", "Тема" },
                { "DarkTheme", "Тёмная тема" },
                { "LightTheme", "Светлая тема" },
                { "TwoFactorAuth", "Двухфакторная аутентификация" },
                { "Enable2FA", "Включить 2FA" },
                { "Disable2FA", "Отключить 2FA" },
                { "BackupCodes", "Коды восстановления" },
                { "Notifications", "Уведомления" },
                { "NotificationCenter", "Центр уведомлений" },
                { "Backup", "Резервная копия" },
                { "Restore", "Восстановить" },
                { "Recommendations", "Рекомендации" },
                { "Forecast", "Прогноз" },
                { "Demand", "Спрос" },
                { "Confidence", "Уверенность" },
                { "Accuracy", "Точность" },
                { "StockAlert", "Низкий запас" },
                { "PriceAlert", "Изменение цены" },
                { "OrderAlert", "Обновление заказа" },
                { "SystemAlert", "Системное уведомление" },
                { "CriticalStock", "Критический запас" },
                { "LowStock", "Низкий запас" },
                { "InStock", "В наличии" },
                { "OutOfStock", "Нет в наличии" },
                { "AddToCart", "В корзину" },
                { "AddToFavorites", "В избранное" },
                { "RemoveFromCart", "Удалить из корзины" },
                { "RemoveFromFavorites", "Удалить из избранного" },
                { "Checkout", "Оформить заказ" },
                { "ClearCart", "Очистить корзину" },
                { "ClearFavorites", "Очистить избранное" },
                { "SimilarProducts", "Похожие товары" },
                { "FrequentlyBoughtTogether", "Часто покупают вместе" },
                { "RecommendedProducts", "Рекомендуемые товары" },
                { "HighDemand", "Высокий спрос" },
                { "LowDemand", "Низкий спрос" },
                { "TotalProducts", "Всего товаров" },
                { "TotalValue", "Общая стоимость" },
                { "AveragePrice", "Средняя цена" },
                { "TotalCategories", "Всего категорий" },
                { "TotalSuppliers", "Всего поставщиков" },
                { "LastLogin", "Последний вход" },
                { "CreatedAt", "Создано" },
                { "UpdatedAt", "Обновлено" },
                { "DeletedAt", "Удалено" },
                { "ConfirmDelete", "Вы уверены, что хотите удалить?" },
                { "OperationSuccessful", "Операция выполнена успешно" },
                { "OperationFailed", "Операция не выполнена" },
                { "InvalidCredentials", "Неверные учетные данные" },
                { "AccessDenied", "Доступ запрещен" },
                { "RequiredField", "Это поле обязательно" },
                { "InvalidFormat", "Неверный формат" },
                { "AlreadyExists", "Уже существует" },
                { "NotFound", "Не найдено" },
                { "Loading", "Загрузка..." },
                { "Saving", "Сохранение..." },
                { "Deleting", "Удаление..." },
                { "Processing", "Обработка..." },
            }
        },
        {
            "en", new Dictionary<string, string>
            {
                { "Dashboard", "Dashboard" },
                { "Products", "Products" },
                { "Categories", "Categories" },
                { "Suppliers", "Suppliers" },
                { "Income", "Income" },
                { "Outcome", "Outcome" },
                { "Logs", "Logs" },
                { "Users", "Users" },
                { "Statistics", "Statistics" },
                { "Favorites", "Favorites" },
                { "Cart", "Cart" },
                { "Settings", "Settings" },
                { "Logout", "Logout" },
                { "Login", "Login" },
                { "Password", "Password" },
                { "Username", "Username" },
                { "Add", "Add" },
                { "Edit", "Edit" },
                { "Delete", "Delete" },
                { "Save", "Save" },
                { "Cancel", "Cancel" },
                { "Search", "Search" },
                { "Filter", "Filter" },
                { "Clear", "Clear" },
                { "Export", "Export" },
                { "Import", "Import" },
                { "Total", "Total" },
                { "Price", "Price" },
                { "Quantity", "Quantity" },
                { "Description", "Description" },
                { "Name", "Name" },
                { "Article", "Article" },
                { "Category", "Category" },
                { "Supplier", "Supplier" },
                { "Date", "Date" },
                { "Action", "Action" },
                { "Status", "Status" },
                { "Active", "Active" },
                { "Inactive", "Inactive" },
                { "Success", "Success" },
                { "Error", "Error" },
                { "Warning", "Warning" },
                { "Info", "Info" },
                { "Confirm", "Confirm" },
                { "Yes", "Yes" },
                { "No", "No" },
                { "Close", "Close" },
                { "Back", "Back" },
                { "Next", "Next" },
                { "Previous", "Previous" },
                { "Language", "Language" },
                { "Theme", "Theme" },
                { "DarkTheme", "Dark Theme" },
                { "LightTheme", "Light Theme" },
                { "TwoFactorAuth", "Two-Factor Authentication" },
                { "Enable2FA", "Enable 2FA" },
                { "Disable2FA", "Disable 2FA" },
                { "BackupCodes", "Backup Codes" },
                { "Notifications", "Notifications" },
                { "NotificationCenter", "Notification Center" },
                { "Backup", "Backup" },
                { "Restore", "Restore" },
                { "Recommendations", "Recommendations" },
                { "Forecast", "Forecast" },
                { "Demand", "Demand" },
                { "Confidence", "Confidence" },
                { "Accuracy", "Accuracy" },
                { "StockAlert", "Low Stock" },
                { "PriceAlert", "Price Change" },
                { "OrderAlert", "Order Update" },
                { "SystemAlert", "System Alert" },
                { "CriticalStock", "Critical Stock" },
                { "LowStock", "Low Stock" },
                { "InStock", "In Stock" },
                { "OutOfStock", "Out of Stock" },
                { "AddToCart", "Add to Cart" },
                { "AddToFavorites", "Add to Favorites" },
                { "RemoveFromCart", "Remove from Cart" },
                { "RemoveFromFavorites", "Remove from Favorites" },
                { "Checkout", "Checkout" },
                { "ClearCart", "Clear Cart" },
                { "ClearFavorites", "Clear Favorites" },
                { "SimilarProducts", "Similar Products" },
                { "FrequentlyBoughtTogether", "Frequently Bought Together" },
                { "RecommendedProducts", "Recommended Products" },
                { "HighDemand", "High Demand" },
                { "LowDemand", "Low Demand" },
                { "TotalProducts", "Total Products" },
                { "TotalValue", "Total Value" },
                { "AveragePrice", "Average Price" },
                { "TotalCategories", "Total Categories" },
                { "TotalSuppliers", "Total Suppliers" },
                { "LastLogin", "Last Login" },
                { "CreatedAt", "Created" },
                { "UpdatedAt", "Updated" },
                { "DeletedAt", "Deleted" },
                { "ConfirmDelete", "Are you sure you want to delete?" },
                { "OperationSuccessful", "Operation successful" },
                { "OperationFailed", "Operation failed" },
                { "InvalidCredentials", "Invalid credentials" },
                { "AccessDenied", "Access denied" },
                { "RequiredField", "This field is required" },
                { "InvalidFormat", "Invalid format" },
                { "AlreadyExists", "Already exists" },
                { "NotFound", "Not found" },
                { "Loading", "Loading..." },
                { "Saving", "Saving..." },
                { "Deleting", "Deleting..." },
                { "Processing", "Processing..." },
            }
        },
        {
            "es", new Dictionary<string, string>
            {
                { "Dashboard", "Panel de control" },
                { "Products", "Productos" },
                { "Categories", "Categorías" },
                { "Suppliers", "Proveedores" },
                { "Income", "Ingresos" },
                { "Outcome", "Gastos" },
                { "Logs", "Registros" },
                { "Users", "Usuarios" },
                { "Statistics", "Estadísticas" },
                { "Favorites", "Favoritos" },
                { "Cart", "Carrito" },
                { "Settings", "Configuración" },
                { "Logout", "Cerrar sesión" },
                { "Login", "Iniciar sesión" },
                { "Password", "Contraseña" },
                { "Username", "Nombre de usuario" },
                { "Add", "Añadir" },
                { "Edit", "Editar" },
                { "Delete", "Eliminar" },
                { "Save", "Guardar" },
                { "Cancel", "Cancelar" },
                { "Search", "Buscar" },
                { "Filter", "Filtro" },
                { "Clear", "Limpiar" },
                { "Export", "Exportar" },
                { "Import", "Importar" },
                { "Total", "Total" },
                { "Price", "Precio" },
                { "Quantity", "Cantidad" },
                { "Description", "Descripción" },
                { "Name", "Nombre" },
                { "Article", "Artículo" },
                { "Category", "Categoría" },
                { "Supplier", "Proveedor" },
                { "Date", "Fecha" },
                { "Action", "Acción" },
                { "Status", "Estado" },
                { "Active", "Activo" },
                { "Inactive", "Inactivo" },
                { "Success", "Éxito" },
                { "Error", "Error" },
                { "Warning", "Advertencia" },
                { "Info", "Información" },
                { "Confirm", "Confirmar" },
                { "Yes", "Sí" },
                { "No", "No" },
                { "Close", "Cerrar" },
                { "Back", "Atrás" },
                { "Next", "Siguiente" },
                { "Previous", "Anterior" },
                { "Language", "Idioma" },
                { "Theme", "Tema" },
                { "DarkTheme", "Tema oscuro" },
                { "LightTheme", "Tema claro" },
                { "TwoFactorAuth", "Autenticación de dos factores" },
                { "Enable2FA", "Habilitar 2FA" },
                { "Disable2FA", "Deshabilitar 2FA" },
                { "BackupCodes", "Códigos de respaldo" },
                { "Notifications", "Notificaciones" },
                { "NotificationCenter", "Centro de notificaciones" },
                { "Backup", "Copia de seguridad" },
                { "Restore", "Restaurar" },
                { "Recommendations", "Recomendaciones" },
                { "Forecast", "Pronóstico" },
                { "Demand", "Demanda" },
                { "Confidence", "Confianza" },
                { "Accuracy", "Precisión" },
                { "StockAlert", "Stock bajo" },
                { "PriceAlert", "Cambio de precio" },
                { "OrderAlert", "Actualización de pedido" },
                { "SystemAlert", "Alerta del sistema" },
                { "CriticalStock", "Stock crítico" },
                { "LowStock", "Stock bajo" },
                { "InStock", "En stock" },
                { "OutOfStock", "Agotado" },
                { "AddToCart", "Añadir al carrito" },
                { "AddToFavorites", "Añadir a favoritos" },
                { "RemoveFromCart", "Eliminar del carrito" },
                { "RemoveFromFavorites", "Eliminar de favoritos" },
                { "Checkout", "Pagar" },
                { "ClearCart", "Vaciar carrito" },
                { "ClearFavorites", "Limpiar favoritos" },
                { "SimilarProducts", "Productos similares" },
                { "FrequentlyBoughtTogether", "Frecuentemente comprados juntos" },
                { "RecommendedProducts", "Productos recomendados" },
                { "HighDemand", "Alta demanda" },
                { "LowDemand", "Baja demanda" },
                { "TotalProducts", "Total de productos" },
                { "TotalValue", "Valor total" },
                { "AveragePrice", "Precio promedio" },
                { "TotalCategories", "Total de categorías" },
                { "TotalSuppliers", "Total de proveedores" },
                { "LastLogin", "Último inicio de sesión" },
                { "CreatedAt", "Creado" },
                { "UpdatedAt", "Actualizado" },
                { "DeletedAt", "Eliminado" },
                { "ConfirmDelete", "¿Está seguro de que desea eliminar?" },
                { "OperationSuccessful", "Operación exitosa" },
                { "OperationFailed", "Operación fallida" },
                { "InvalidCredentials", "Credenciales inválidas" },
                { "AccessDenied", "Acceso denegado" },
                { "RequiredField", "Este campo es obligatorio" },
                { "InvalidFormat", "Formato inválido" },
                { "AlreadyExists", "Ya existe" },
                { "NotFound", "No encontrado" },
                { "Loading", "Cargando..." },
                { "Saving", "Guardando..." },
                { "Deleting", "Eliminando..." },
                { "Processing", "Procesando..." },
            }
        },
        {
            "de", new Dictionary<string, string>
            {
                { "Dashboard", "Armaturenbrett" },
                { "Products", "Produkte" },
                { "Categories", "Kategorien" },
                { "Suppliers", "Lieferanten" },
                { "Income", "Einkommen" },
                { "Outcome", "Ausgaben" },
                { "Logs", "Protokolle" },
                { "Users", "Benutzer" },
                { "Statistics", "Statistiken" },
                { "Favorites", "Favoriten" },
                { "Cart", "Warenkorb" },
                { "Settings", "Einstellungen" },
                { "Logout", "Abmelden" },
                { "Login", "Anmelden" },
                { "Password", "Passwort" },
                { "Username", "Benutzername" },
                { "Add", "Hinzufügen" },
                { "Edit", "Bearbeiten" },
                { "Delete", "Löschen" },
                { "Save", "Speichern" },
                { "Cancel", "Abbrechen" },
                { "Search", "Suche" },
                { "Filter", "Filter" },
                { "Clear", "Löschen" },
                { "Export", "Exportieren" },
                { "Import", "Importieren" },
                { "Total", "Gesamt" },
                { "Price", "Preis" },
                { "Quantity", "Menge" },
                { "Description", "Beschreibung" },
                { "Name", "Name" },
                { "Article", "Artikel" },
                { "Category", "Kategorie" },
                { "Supplier", "Lieferant" },
                { "Date", "Datum" },
                { "Action", "Aktion" },
                { "Status", "Status" },
                { "Active", "Aktiv" },
                { "Inactive", "Inaktiv" },
                { "Success", "Erfolg" },
                { "Error", "Fehler" },
                { "Warning", "Warnung" },
                { "Info", "Information" },
                { "Confirm", "Bestätigen" },
                { "Yes", "Ja" },
                { "No", "Nein" },
                { "Close", "Schließen" },
                { "Back", "Zurück" },
                { "Next", "Weiter" },
                { "Previous", "Zurück" },
                { "Language", "Sprache" },
                { "Theme", "Design" },
                { "DarkTheme", "Dunkles Design" },
                { "LightTheme", "Helles Design" },
                { "TwoFactorAuth", "Zwei-Faktor-Authentifizierung" },
                { "Enable2FA", "2FA aktivieren" },
                { "Disable2FA", "2FA deaktivieren" },
                { "BackupCodes", "Sicherungscodes" },
                { "Notifications", "Benachrichtigungen" },
                { "NotificationCenter", "Benachrichtigungszentrum" },
                { "Backup", "Sicherung" },
                { "Restore", "Wiederherstellen" },
                { "Recommendations", "Empfehlungen" },
                { "Forecast", "Prognose" },
                { "Demand", "Nachfrage" },
                { "Confidence", "Vertrauen" },
                { "Accuracy", "Genauigkeit" },
                { "StockAlert", "Niedriger Bestand" },
                { "PriceAlert", "Preisänderung" },
                { "OrderAlert", "Bestellaktualisierung" },
                { "SystemAlert", "Systemwarnung" },
                { "CriticalStock", "Kritischer Bestand" },
                { "LowStock", "Niedriger Bestand" },
                { "InStock", "Auf Lager" },
                { "OutOfStock", "Ausverkauft" },
                { "AddToCart", "In den Warenkorb" },
                { "AddToFavorites", "Zu Favoriten hinzufügen" },
                { "RemoveFromCart", "Aus Warenkorb entfernen" },
                { "RemoveFromFavorites", "Aus Favoriten entfernen" },
                { "Checkout", "Kasse" },
                { "ClearCart", "Warenkorb leeren" },
                { "ClearFavorites", "Favoriten löschen" },
                { "SimilarProducts", "Ähnliche Produkte" },
                { "FrequentlyBoughtTogether", "Häufig zusammen gekauft" },
                { "RecommendedProducts", "Empfohlene Produkte" },
                { "HighDemand", "Hohe Nachfrage" },
                { "LowDemand", "Niedrige Nachfrage" },
                { "TotalProducts", "Gesamtprodukte" },
                { "TotalValue", "Gesamtwert" },
                { "AveragePrice", "Durchschnittspreis" },
                { "TotalCategories", "Gesamtkategorien" },
                { "TotalSuppliers", "Gesamtlieferanten" },
                { "LastLogin", "Letzte Anmeldung" },
                { "CreatedAt", "Erstellt" },
                { "UpdatedAt", "Aktualisiert" },
                { "DeletedAt", "Gelöscht" },
                { "ConfirmDelete", "Sind Sie sicher, dass Sie löschen möchten?" },
                { "OperationSuccessful", "Operation erfolgreich" },
                { "OperationFailed", "Operation fehlgeschlagen" },
                { "InvalidCredentials", "Ungültige Anmeldedaten" },
                { "AccessDenied", "Zugriff verweigert" },
                { "RequiredField", "Dieses Feld ist erforderlich" },
                { "InvalidFormat", "Ungültiges Format" },
                { "AlreadyExists", "Existiert bereits" },
                { "NotFound", "Nicht gefunden" },
                { "Loading", "Wird geladen..." },
                { "Saving", "Wird gespeichert..." },
                { "Deleting", "Wird gelöscht..." },
                { "Processing", "Wird verarbeitet..." },
            }
        }
    };

    public static void SetLanguage(string languageCode)
    {
        if (Strings.ContainsKey(languageCode))
        {
            _currentLanguage = languageCode;
            SaveLanguagePreference(languageCode);
        }
    }

    public static string GetString(string key)
    {
        if (Strings.TryGetValue(_currentLanguage, out var languageStrings))
        {
            if (languageStrings.TryGetValue(key, out var value))
                return value;
        }

        if (Strings.TryGetValue("en", out var englishStrings))
        {
            if (englishStrings.TryGetValue(key, out var value))
                return value;
        }

        return key;
    }

    public static List<(string Code, string Name)> GetAvailableLanguages()
    {
        return new List<(string, string)>
        {
            ("ru", "Русский"),
            ("en", "English"),
            ("es", "Español"),
            ("de", "Deutsch")
        };
    }

    public static string GetCurrentLanguage()
    {
        return _currentLanguage;
    }

    public static void LoadLanguagePreference(int userId)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT PreferredLanguage
                FROM AppUsers
                WHERE Id = @userId", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var language = reader.GetString(0);
                if (!string.IsNullOrEmpty(language))
                    SetLanguage(language);
            }
        }
        catch
        {
            _currentLanguage = "ru";
        }
    }

    private static void SaveLanguagePreference(string languageCode)
    {
        try
        {
            if (CurrentSession.CurrentUser == null)
                return;

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                UPDATE AppUsers
                SET PreferredLanguage = @language
                WHERE Id = @userId", conn);

            cmd.Parameters.AddWithValue("@language", languageCode);
            cmd.Parameters.AddWithValue("@userId", CurrentSession.CurrentUser.Id);

            cmd.ExecuteNonQuery();
        }
        catch
        {
        }
    }
}
