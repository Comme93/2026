# Warehouse Management System v9.0 - API Documentation

## Overview
Complete API reference for all services in Warehouse Management System v9.0.

---

## 1. TwoFactorService

### Purpose
Manages two-factor authentication with email delivery and backup codes.

### Methods

#### GenerateCode()
Generates a 6-digit verification code.
```csharp
public string GenerateCode()
```
**Returns:** 6-digit code as string
**Example:** "123456"

#### GenerateBackupCodes(int count = 10)
Generates backup recovery codes.
```csharp
public List<string> GenerateBackupCodes(int count = 10)
```
**Parameters:**
- `count` - Number of codes to generate (default: 10)

**Returns:** List of 8-character backup codes

#### SendCodeViaEmail(int userId, string email, string code)
Sends verification code via email.
```csharp
public bool SendCodeViaEmail(int userId, string email, string code)
```
**Parameters:**
- `userId` - User ID
- `email` - Recipient email address
- `code` - Verification code to send

**Returns:** true if sent successfully, false otherwise

#### VerifyCode(int userId, string code)
Validates a verification code.
```csharp
public bool VerifyCode(int userId, string code)
```
**Parameters:**
- `userId` - User ID
- `code` - Code to verify

**Returns:** true if valid and not expired, false otherwise
**Notes:** Code expires after 10 minutes, max 5 attempts

#### ValidateBackupCode(int userId, string code)
Validates and consumes a backup code.
```csharp
public bool ValidateBackupCode(int userId, string code)
```
**Parameters:**
- `userId` - User ID
- `code` - Backup code to validate

**Returns:** true if valid and unused, false otherwise
**Notes:** Each backup code can only be used once

#### EnableTwoFactor(int userId, string method = "Email", string phoneNumber = null)
Enables 2FA for a user.
```csharp
public void EnableTwoFactor(int userId, string method = "Email", string phoneNumber = null)
```
**Parameters:**
- `userId` - User ID
- `method` - Delivery method: "Email" or "SMS"
- `phoneNumber` - Phone number (required for SMS)

#### DisableTwoFactor(int userId)
Disables 2FA for a user.
```csharp
public void DisableTwoFactor(int userId)
```

#### IsTwoFactorEnabled(int userId)
Checks if 2FA is enabled for a user.
```csharp
public bool IsTwoFactorEnabled(int userId)
```
**Returns:** true if 2FA is enabled, false otherwise

#### GetTwoFactorMethod(int userId)
Gets the 2FA delivery method for a user.
```csharp
public string GetTwoFactorMethod(int userId)
```
**Returns:** "Email" or "SMS"

#### GetBackupCodes(int userId)
Retrieves remaining backup codes for a user.
```csharp
public List<string> GetBackupCodes(int userId)
```
**Returns:** List of unused backup codes

---

## 2. NotificationService

### Purpose
Manages user notifications and preferences.

### Methods

#### SendStockAlert(int productId, string productName, int currentQuantity, int minQuantity)
Sends low stock alert to admins.
```csharp
public void SendStockAlert(int productId, string productName, int currentQuantity, int minQuantity)
```

#### SendPriceChangeAlert(int productId, string productName, decimal oldPrice, decimal newPrice)
Sends price change alert to admins.
```csharp
public void SendPriceChangeAlert(int productId, string productName, decimal oldPrice, decimal newPrice)
```

#### SendOrderAlert(int userId, string orderInfo)
Sends order update alert to user.
```csharp
public void SendOrderAlert(int userId, string orderInfo)
```

#### SendSystemAlert(string title, string message)
Sends system alert to all users.
```csharp
public void SendSystemAlert(string title, string message)
```

#### CreateNotification(int userId, string title, string message, string type, string actionUrl)
Creates a notification for a user.
```csharp
public void CreateNotification(int userId, string title, string message, string type, string actionUrl)
```

#### GetUserNotifications(int userId, bool unreadOnly = false)
Retrieves user notifications.
```csharp
public List<(int Id, string Title, string Message, string Type, bool IsRead, DateTime CreatedAt)> GetUserNotifications(int userId, bool unreadOnly = false)
```
**Returns:** List of notifications

#### GetUnreadNotificationCount(int userId)
Gets count of unread notifications.
```csharp
public int GetUnreadNotificationCount(int userId)
```

#### MarkAsRead(int notificationId)
Marks notification as read.
```csharp
public void MarkAsRead(int notificationId)
```

#### DeleteNotification(int notificationId)
Deletes a notification.
```csharp
public void DeleteNotification(int notificationId)
```

#### SetNotificationPreferences(int userId, bool stockAlerts, bool priceAlerts, bool orderAlerts, bool systemAlerts, string deliveryMethod)
Sets user notification preferences.
```csharp
public void SetNotificationPreferences(int userId, bool stockAlerts, bool priceAlerts, bool orderAlerts, bool systemAlerts, string deliveryMethod)
```

#### GetNotificationPreferences(int userId)
Gets user notification preferences.
```csharp
public (bool StockAlerts, bool PriceAlerts, bool OrderAlerts, bool SystemAlerts, string DeliveryMethod) GetNotificationPreferences(int userId)
```

---

## 3. BackupService

### Purpose
Manages database backups and restoration.

### Methods

#### CreateBackup(bool isAutomatic = false)
Creates a database backup.
```csharp
public string CreateBackup(bool isAutomatic = false)
```
**Returns:** Path to backup file
**Throws:** Exception if backup fails

#### RestoreBackup(int backupId)
Restores database from backup.
```csharp
public bool RestoreBackup(int backupId)
```
**Returns:** true if successful, false otherwise

#### GetAllBackups()
Retrieves all backups.
```csharp
public List<(int Id, string BackupName, DateTime BackupDate, long BackupSize, string BackupType, bool IsAutomatic)> GetAllBackups()
```

#### DeleteBackup(int backupId)
Deletes a backup.
```csharp
public void DeleteBackup(int backupId)
```

---

## 4. AutoSaveService

### Purpose
Manages automatic database backups.

### Methods

#### StartAutoSave(int intervalMinutes = 30)
Starts automatic backup timer.
```csharp
public void StartAutoSave(int intervalMinutes = 30)
```

#### StopAutoSave()
Stops automatic backup timer.
```csharp
public void StopAutoSave()
```

#### SetBackupInterval(int intervalMinutes)
Changes backup interval.
```csharp
public void SetBackupInterval(int intervalMinutes)
```

#### GetBackupInterval()
Gets current backup interval.
```csharp
public int GetBackupInterval()
```

---

## 5. RecommendationService

### Purpose
Provides product recommendations using multiple algorithms.

### Methods

#### GetRecommendationsForUser(int userId, int count = 5)
Gets personalized recommendations for user.
```csharp
public List<Product> GetRecommendationsForUser(int userId, int count = 5)
```

#### GetSimilarProducts(int productId, int count = 5)
Gets similar products.
```csharp
public List<Product> GetSimilarProducts(int productId, int count = 5)
```

#### GetFrequentlyBoughtTogether(int productId, int count = 5)
Gets products frequently bought with this product.
```csharp
public List<Product> GetFrequentlyBoughtTogether(int productId, int count = 5)
```

#### GetPopularProducts(int count = 5)
Gets trending/popular products.
```csharp
public List<Product> GetPopularProducts(int count = 5)
```

#### RecordPurchase(int userId, int productId, int quantity, decimal price)
Records a purchase for recommendation analysis.
```csharp
public void RecordPurchase(int userId, int productId, int quantity, decimal price)
```

#### UpdateProductSimilarity()
Recalculates product similarity scores.
```csharp
public void UpdateProductSimilarity()
```

---

## 6. DemandForecastingService

### Purpose
Predicts product demand using statistical algorithms.

### Methods

#### GenerateForecast(int productId, int daysAhead = 30)
Generates demand forecast for a product.
```csharp
public void GenerateForecast(int productId, int daysAhead = 30)
```

#### UpdateAllForecasts(int daysAhead = 30)
Generates forecasts for all products.
```csharp
public void UpdateAllForecasts(int daysAhead = 30)
```

#### GetForecast(int productId, int daysAhead = 30)
Retrieves forecast for a product.
```csharp
public List<(DateTime Date, int ForecastedQuantity, decimal Confidence)> GetForecast(int productId, int daysAhead = 30)
```

#### GetForecastAccuracy(int productId)
Gets forecast accuracy percentage.
```csharp
public decimal GetForecastAccuracy(int productId)
```
**Returns:** Accuracy percentage (0-100)

#### GetHighDemandProducts(int threshold = 100)
Gets products with high forecasted demand.
```csharp
public List<(int ProductId, string ProductName, int ForecastedDemand, decimal Confidence)> GetHighDemandProducts(int threshold = 100)
```

#### GetLowDemandProducts(int threshold = 10)
Gets products with low forecasted demand.
```csharp
public List<(int ProductId, string ProductName, int ForecastedDemand, decimal Confidence)> GetLowDemandProducts(int threshold = 10)
```

#### RecordActualDemand(int productId, int quantity)
Records actual demand for accuracy tracking.
```csharp
public void RecordActualDemand(int productId, int quantity)
```

---

## 7. LanguageService

### Purpose
Manages multi-language support.

### Methods

#### SetLanguage(string languageCode)
Sets active language.
```csharp
public static void SetLanguage(string languageCode)
```
**Parameters:**
- `languageCode` - "ru", "en", "es", or "de"

#### GetString(string key)
Gets localized string.
```csharp
public static string GetString(string key)
```
**Returns:** Localized string or key if not found

#### GetAvailableLanguages()
Gets list of supported languages.
```csharp
public static List<(string Code, string Name)> GetAvailableLanguages()
```

#### GetCurrentLanguage()
Gets active language code.
```csharp
public static string GetCurrentLanguage()
```

#### LoadLanguagePreference(int userId)
Loads user's language preference.
```csharp
public static void LoadLanguagePreference(int userId)
```

---

## 8. ChartService

### Purpose
Generates chart data for analytics.

### Methods

#### GetSalesTrendData(int daysBack = 30)
Gets sales trend data.
```csharp
public ChartData GetSalesTrendData(int daysBack = 30)
```
**Returns:** ChartData with line chart data

#### GetProductDistributionData()
Gets product distribution by category.
```csharp
public ChartData GetProductDistributionData()
```
**Returns:** ChartData with pie chart data

#### GetSupplierComparisonData()
Gets supplier comparison data.
```csharp
public ChartData GetSupplierComparisonData()
```
**Returns:** ChartData with bar chart data

#### GetCategoryPerformanceData()
Gets category performance data.
```csharp
public ChartData GetCategoryPerformanceData()
```

#### GetStockLevelsData()
Gets stock levels for top products.
```csharp
public ChartData GetStockLevelsData()
```

#### GetPriceTrendsData(int daysBack = 90)
Gets price trend data.
```csharp
public ChartData GetPriceTrendsData(int daysBack = 90)
```

#### GetInventoryValueData()
Gets inventory value by category.
```csharp
public ChartData GetInventoryValueData()
```

#### GetTransactionVolumeData(int daysBack = 30)
Gets transaction volume data.
```csharp
public ChartData GetTransactionVolumeData(int daysBack = 30)
```

#### GetTopProductsData(int count = 10)
Gets top selling products.
```csharp
public ChartData GetTopProductsData(int count = 10)
```

#### GetLowStockProductsData()
Gets low stock products.
```csharp
public ChartData GetLowStockProductsData()
```

---

## 9. EncryptionService

### Purpose
Provides encryption and hashing utilities.

### Methods

#### Encrypt(string plainText)
Encrypts text using AES.
```csharp
public static string Encrypt(string plainText)
```
**Returns:** Base64-encoded encrypted text

#### Decrypt(string cipherText)
Decrypts AES-encrypted text.
```csharp
public static string Decrypt(string cipherText)
```

#### HashPassword(string password)
Hashes password using SHA256.
```csharp
public static string HashPassword(string password)
```

#### VerifyPassword(string password, string hash)
Verifies password against hash.
```csharp
public static bool VerifyPassword(string password, string hash)
```

---

## 10. TimezoneService

### Purpose
Manages timezone conversions and preferences.

### Methods

#### GetAvailableTimezones()
Gets list of system timezones.
```csharp
public static List<(string Id, string DisplayName)> GetAvailableTimezones()
```

#### SetUserTimezone(int userId, string timezoneId)
Sets user's timezone preference.
```csharp
public static void SetUserTimezone(int userId, string timezoneId)
```

#### GetUserTimezone(int userId)
Gets user's timezone preference.
```csharp
public static string GetUserTimezone(int userId)
```

#### ConvertToUserTimezone(DateTime utcDateTime, int userId)
Converts UTC time to user's timezone.
```csharp
public static DateTime ConvertToUserTimezone(DateTime utcDateTime, int userId)
```

#### ConvertFromUserTimezone(DateTime localDateTime, int userId)
Converts local time to UTC.
```csharp
public static DateTime ConvertFromUserTimezone(DateTime localDateTime, int userId)
```

#### FormatDateTimeForUser(DateTime dateTime, int userId, string format = "yyyy-MM-dd HH:mm:ss")
Formats datetime for user's timezone.
```csharp
public static string FormatDateTimeForUser(DateTime dateTime, int userId, string format = "yyyy-MM-dd HH:mm:ss")
```

#### GetTimezoneOffset(string timezoneId)
Gets timezone offset from UTC.
```csharp
public static string GetTimezoneOffset(string timezoneId)
```
**Returns:** String like "UTC+02:00"

---

## 11. TwoFactorAuditService

### Purpose
Logs and audits 2FA operations.

### Methods

#### LogTwoFactorEvent(int userId, TwoFactorEventType eventType, string details = null, bool success = true)
Logs a 2FA event.
```csharp
public void LogTwoFactorEvent(int userId, TwoFactorEventType eventType, string details = null, bool success = true)
```

#### LogTwoFactorCodeGeneration(int userId, string method)
Logs code generation.
```csharp
public void LogTwoFactorCodeGeneration(int userId, string method)
```

#### LogTwoFactorCodeSent(int userId, string destination)
Logs code delivery.
```csharp
public void LogTwoFactorCodeSent(int userId, string destination)
```

#### LogTwoFactorCodeVerification(int userId, bool success)
Logs code verification attempt.
```csharp
public void LogTwoFactorCodeVerification(int userId, bool success)
```

#### LogBackupCodeUsage(int userId, int remainingCodes)
Logs backup code usage.
```csharp
public void LogBackupCodeUsage(int userId, int remainingCodes)
```

#### LogTwoFactorEnabled(int userId, string method)
Logs 2FA enablement.
```csharp
public void LogTwoFactorEnabled(int userId, string method)
```

#### LogTwoFactorDisabled(int userId)
Logs 2FA disablement.
```csharp
public void LogTwoFactorDisabled(int userId)
```

#### GetTwoFactorEventCount(int userId, int daysBack = 30)
Gets count of 2FA events.
```csharp
public int GetTwoFactorEventCount(int userId, int daysBack = 30)
```

#### GetFailedVerificationAttempts(int userId, int minutesBack = 60)
Gets failed verification attempts.
```csharp
public int GetFailedVerificationAttempts(int userId, int minutesBack = 60)
```

#### IsUserLockedOut(int userId, int maxAttempts = 5, int minutesBack = 60)
Checks if user is locked out due to failed attempts.
```csharp
public bool IsUserLockedOut(int userId, int maxAttempts = 5, int minutesBack = 60)
```

---

## Database Schema

### New Tables

#### TwoFactorSettings
- UserId (INT, FK)
- IsEnabled (BIT)
- Method (NVARCHAR)
- PhoneNumber (NVARCHAR)
- SecretKey (NVARCHAR)
- BackupCodes (NVARCHAR(MAX))

#### TwoFactorAttempts
- UserId (INT, FK)
- Code (NVARCHAR)
- CreatedAt (DATETIME)
- ExpiresAt (DATETIME)
- IsUsed (BIT)
- AttemptCount (INT)

#### Notifications
- UserId (INT, FK)
- Title (NVARCHAR)
- Message (NVARCHAR)
- Type (NVARCHAR)
- IsRead (BIT)
- CreatedAt (DATETIME)
- ActionUrl (NVARCHAR)

#### NotificationPreferences
- UserId (INT, FK)
- StockAlerts (BIT)
- PriceAlerts (BIT)
- OrderAlerts (BIT)
- SystemAlerts (BIT)
- DeliveryMethod (NVARCHAR)

#### UserPurchaseHistory
- UserId (INT, FK)
- ProductId (INT, FK)
- PurchaseDate (DATETIME)
- Quantity (INT)
- Price (DECIMAL)

#### ProductSimilarity
- ProductId1 (INT, FK)
- ProductId2 (INT, FK)
- SimilarityScore (DECIMAL)
- CreatedAt (DATETIME)

#### DemandForecasts
- ProductId (INT, FK)
- ForecastDate (DATE)
- ForecastedQuantity (INT)
- Confidence (DECIMAL)
- Method (NVARCHAR)
- CreatedAt (DATETIME)

#### HistoricalDemand
- ProductId (INT, FK)
- Date (DATE)
- ActualQuantity (INT)
- ForecastedQuantity (INT)
- Error (DECIMAL)

#### Backups
- BackupName (NVARCHAR)
- BackupPath (NVARCHAR)
- BackupDate (DATETIME)
- BackupSize (BIGINT)
- BackupType (NVARCHAR)
- IsAutomatic (BIT)
- IsRestored (BIT)

### Modified Tables

#### AppUsers
- Added: PreferredLanguage (NVARCHAR(10), default 'ru')
- Added: PreferredTimezone (NVARCHAR(100))

---

## Error Handling

All services implement try-catch blocks and return safe defaults:
- Encryption errors return original text
- Database errors are logged silently
- Invalid parameters return null or empty collections

---

## Performance Considerations

- Product similarity is cached in database
- Forecasts are pre-calculated and stored
- Notifications use indexed queries
- Backups run asynchronously
- Chart data is aggregated at query time

---

## Security

- All passwords hashed with SHA256
- Sensitive data encrypted with AES
- SQL injection prevented with parameterized queries
- 2FA attempts limited to 5 per 10 minutes
- Backup codes are one-time use only
- All operations logged for audit trail

---

Version: 9.0.0
Last Updated: 2026-05-05
