-- Insert Categories with Russian names
INSERT INTO Categories (Name, Description, IsActive) VALUES
('Электроника', 'Электронные устройства и компоненты', 1),
('Мебель', 'Офисная и складская мебель', 1),
('Упаковка', 'Упаковочные материалы и расходники', 1),
('Инструменты', 'Ручные инструменты и оборудование', 1),
('Расходники', 'Общие офисные и складские расходники', 1);

-- Insert Suppliers with Russian names
INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, Address, IsActive) VALUES
('ТехСнабжение ООО', 'Иван Петров', '+7-495-123-4567', 'ivan@techsnab.ru', 'Москва, ул. Технологическая, 10', 1),
('МебельПро ООО', 'Мария Сидорова', '+7-495-234-5678', 'maria@mebelpro.ru', 'Санкт-Петербург, пр. Мебельный, 25', 1),
('УпаковкаПлюс ООО', 'Сергей Иванов', '+7-495-345-6789', 'sergey@upakovka.ru', 'Екатеринбург, ул. Упаковочная, 15', 1);

-- Insert Products with Russian names
INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES
('ELEC-001', 'Ноутбук профессиональный', 1, 'шт', 450.00, 50, 10, 'Высокопроизводительный ноутбук', 1, 1),
('ELEC-002', 'Кабель USB 3.0', 1, 'шт', 120.00, 200, 50, 'Стандартный кабель USB 3.0', 1, 1),
('FURN-001', 'Кресло офисное', 2, 'шт', 5000.00, 15, 5, 'Эргономичное офисное кресло', 2, 1),
('FURN-002', 'Стол письменный', 2, 'шт', 2500.00, 30, 10, 'Деревянный офисный стол', 2, 1),
('PACK-001', 'Коробка картонная', 3, 'шт', 800.00, 100, 20, 'Стандартная картонная коробка', 3, 1),
('PACK-002', 'Пленка воздушно-пузырчатая', 3, 'рулон', 150.00, 80, 15, 'Защитная пленка для упаковки', 3, 1),
('CONS-001', 'Скрепки канцелярские', 5, 'коробка', 250.00, 500, 100, 'Стандартные скрепки', 3, 1),
('TOOL-001', 'Набор отверток', 4, 'набор', 3500.00, 25, 5, 'Профессиональный набор отверток', 1, 1);

-- Insert Income Transactions
INSERT INTO IncomeTransactions (ProductId, Quantity, Price, SupplierId, TransactionDate, Notes, CreatedByUserId) VALUES
(1, 50, 450.00, 1, GETDATE(), 'Начальный запас', 1),
(3, 15, 5000.00, 2, GETDATE(), 'Поставка офисной мебели', 1),
(5, 100, 800.00, 3, GETDATE(), 'Упаковочные материалы', 1);

-- Insert Outcome Transactions
INSERT INTO OutcomeTransactions (ProductId, Quantity, Price, TransactionDate, Reason, Notes, CreatedByUserId) VALUES
(1, 5, 450.00, GETDATE(), 'Продажа', 'Заказ клиента #001', 1),
(3, 2, 5000.00, GETDATE(), 'Продажа', 'Заказ клиента #002', 1),
(5, 20, 800.00, GETDATE(), 'Повреждение', 'Повреждено при транспортировке', 1);
