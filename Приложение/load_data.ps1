$connectionString = "Server=(localdb)\mssqllocaldb;Database=WarehouseManagementDB;Integrated Security=true;"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()

$queries = @(
    "INSERT INTO Categories (Name, Description, IsActive) VALUES (N'Электроника', N'Электронные устройства и компоненты', 1)",
    "INSERT INTO Categories (Name, Description, IsActive) VALUES (N'Мебель', N'Офисная и складская мебель', 1)",
    "INSERT INTO Categories (Name, Description, IsActive) VALUES (N'Упаковка', N'Упаковочные материалы и расходники', 1)",
    "INSERT INTO Categories (Name, Description, IsActive) VALUES (N'Инструменты', N'Ручные инструменты и оборудование', 1)",
    "INSERT INTO Categories (Name, Description, IsActive) VALUES (N'Расходники', N'Общие офисные и складские расходники', 1)",
    
    "INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, Address, IsActive) VALUES (N'ТехСнабжение ООО', N'Иван Петров', N'+7-495-123-4567', N'ivan@techsnab.ru', N'Москва, ул. Технологическая, 10', 1)",
    "INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, Address, IsActive) VALUES (N'МебельПро ООО', N'Мария Сидорова', N'+7-495-234-5678', N'maria@mebelpro.ru', N'Санкт-Петербург, пр. Мебельный, 25', 1)",
    "INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, Address, IsActive) VALUES (N'УпаковкаПлюс ООО', N'Сергей Иванов', N'+7-495-345-6789', N'sergey@upakovka.ru', N'Екатеринбург, ул. Упаковочная, 15', 1)",
    
    "INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES (N'ELEC-001', N'Ноутбук профессиональный', 6, N'шт', 450.00, 50, 10, N'Высокопроизводительный ноутбук', 4, 1)",
    "INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES (N'ELEC-002', N'Кабель USB 3.0', 6, N'шт', 120.00, 200, 50, N'Стандартный кабель USB 3.0', 4, 1)",
    "INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES (N'FURN-001', N'Кресло офисное', 7, N'шт', 5000.00, 15, 5, N'Эргономичное офисное кресло', 5, 1)",
    "INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES (N'FURN-002', N'Стол письменный', 7, N'шт', 2500.00, 30, 10, N'Деревянный офисный стол', 5, 1)",
    "INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES (N'PACK-001', N'Коробка картонная', 8, N'шт', 800.00, 100, 20, N'Стандартная картонная коробка', 6, 1)",
    "INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES (N'PACK-002', N'Пленка воздушно-пузырчатая', 8, N'рулон', 150.00, 80, 15, N'Защитная пленка для упаковки', 6, 1)",
    "INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES (N'CONS-001', N'Скрепки канцелярские', 10, N'коробка', 250.00, 500, 100, N'Стандартные скрепки', 6, 1)",
    "INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES (N'TOOL-001', N'Набор отверток', 9, N'набор', 3500.00, 25, 5, N'Профессиональный набор отверток', 4, 1)",
    
    "INSERT INTO IncomeTransactions (ProductId, Quantity, Price, SupplierId, TransactionDate, Notes, CreatedByUserId) VALUES (11, 50, 450.00, 4, GETDATE(), N'Начальный запас', 1)",
    "INSERT INTO IncomeTransactions (ProductId, Quantity, Price, SupplierId, TransactionDate, Notes, CreatedByUserId) VALUES (13, 15, 5000.00, 5, GETDATE(), N'Поставка офисной мебели', 1)",
    "INSERT INTO IncomeTransactions (ProductId, Quantity, Price, SupplierId, TransactionDate, Notes, CreatedByUserId) VALUES (15, 100, 800.00, 6, GETDATE(), N'Упаковочные материалы', 1)",
    
    "INSERT INTO OutcomeTransactions (ProductId, Quantity, Price, TransactionDate, Reason, Notes, CreatedByUserId) VALUES (11, 5, 450.00, GETDATE(), N'Продажа', N'Заказ клиента #001', 1)",
    "INSERT INTO OutcomeTransactions (ProductId, Quantity, Price, TransactionDate, Reason, Notes, CreatedByUserId) VALUES (13, 2, 5000.00, GETDATE(), N'Продажа', N'Заказ клиента #002', 1)",
    "INSERT INTO OutcomeTransactions (ProductId, Quantity, Price, TransactionDate, Reason, Notes, CreatedByUserId) VALUES (15, 20, 800.00, GETDATE(), N'Повреждение', N'Повреждено при транспортировке', 1)"
)

foreach ($query in $queries) {
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $command.ExecuteNonQuery()
}

$connection.Close()
Write-Host "Data loaded successfully"
