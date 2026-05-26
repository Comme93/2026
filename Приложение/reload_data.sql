-- Insert Products with correct IDs
INSERT INTO Products (Article, Name, CategoryId, Unit, Price, Quantity, MinQuantity, Description, SupplierId, IsActive) VALUES
('ELEC-001', 'Laptop Computer', 6, 'pcs', 450.00, 50, 10, 'High-performance laptop', 4, 1),
('ELEC-002', 'USB Cable', 6, 'pcs', 120.00, 200, 50, 'Standard USB 3.0 cable', 4, 1),
('FURN-001', 'Office Chair', 7, 'pcs', 5000.00, 15, 5, 'Ergonomic office chair', 5, 1),
('FURN-002', 'Desk', 7, 'pcs', 2500.00, 30, 10, 'Wooden office desk', 5, 1),
('PACK-001', 'Cardboard Box', 8, 'box', 800.00, 100, 20, 'Standard shipping box', 6, 1),
('PACK-002', 'Bubble Wrap', 8, 'roll', 150.00, 80, 15, 'Protective bubble wrap', 6, 1),
('CONS-001', 'Paper Clips', 10, 'box', 250.00, 500, 100, 'Standard paper clips', 6, 1),
('TOOL-001', 'Screwdriver Set', 9, 'set', 3500.00, 25, 5, 'Professional screwdriver set', 4, 1);

-- Insert Income Transactions
INSERT INTO IncomeTransactions (ProductId, Quantity, Price, SupplierId, TransactionDate, Notes, CreatedByUserId) VALUES
(1, 50, 450.00, 4, GETDATE(), 'Initial stock', 1),
(3, 15, 5000.00, 5, GETDATE(), 'Office furniture delivery', 1),
(5, 100, 800.00, 6, GETDATE(), 'Packaging materials', 1);

-- Insert Outcome Transactions
INSERT INTO OutcomeTransactions (ProductId, Quantity, Price, TransactionDate, Reason, Notes, CreatedByUserId) VALUES
(1, 5, 450.00, GETDATE(), 'Sale', 'Customer order #001', 1),
(3, 2, 5000.00, GETDATE(), 'Sale', 'Customer order #002', 1),
(5, 20, 800.00, GETDATE(), 'Damage', 'Damaged during transport', 1);
