
------------------------------------------------------------------------------------------------------------------------------------------------------------------
--TẠO BD VÀ CÁC BẢNG ĐỀ GHI DỮ LIỆU 
------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- Create the database
CREATE DATABASE Project_Store_X_DB;
GO

-- Select the newly created database
USE Project_Store_X_DB;
GO
-- =========================================
--EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'   -- xóa toàn bộ liên kết hiện tại 
--EXEC sp_MSforeachtable 'DROP TABLE ?'   -- xóa toàn bộ bảng hiện tại 
-- =================================================================
-- 1. Create tables that do not have foreign keys
-- =================================================================

-- Table: Suppliers
CREATE TABLE Suppliers (
    SupplierID INT PRIMARY KEY IDENTITY(1,1),
    SupplierName NVARCHAR(255) NOT NULL,
    SupplierEmail NVARCHAR(255) NOT NULL UNIQUE,
    SupplierPhone NVARCHAR(50) NOT NULL UNIQUE,
    SupplierAddress NVARCHAR(500) NULL
);
GO

-- Table: Customers
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(255) NOT NULL,
    CustomerPhone NVARCHAR(50) NOT NULL UNIQUE,
    CustomerAddress NVARCHAR(500) NULL
);
GO

-- Table: Roles
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    RoleDesc NVARCHAR(255) NULL
);
GO

-- Table: Accounts
CREATE TABLE Accounts (
    AccountID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL
);
GO

-- Table: PaymentMethods
CREATE TABLE PaymentMethods (
    MethodID INT PRIMARY KEY IDENTITY(1,1),
    MethodName NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- Table: Categories
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    CategoryDesc NTEXT NULL
);
GO

-- =================================================================
-- 2. Create tables with foreign key dependencies
-- =================================================================

-- Table: Employees (depends on Roles and Accounts)
CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeName NVARCHAR(255) NOT NULL,
    Position NVARCHAR(100) NOT NULL,
    RoleID INT NOT NULL,
    AccountID INT NOT NULL UNIQUE, -- One employee has one account
    CONSTRAINT FK_Employees_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID),
    CONSTRAINT FK_Employees_Accounts FOREIGN KEY (AccountID) REFERENCES Accounts(AccountID)
);
GO

-- Table: Products (depends on Categories and Suppliers)
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(255) NOT NULL,
    ProductDesc NTEXT NULL,
    ImageURL NVARCHAR(2048) NULL,
    Price DECIMAL(10, 2) NOT NULL,
    InventoryQuantity INT NOT NULL DEFAULT 0,
    CategoryID INT NOT NULL,
    SupplierID INT NOT NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT FK_Products_Suppliers FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID),
    CONSTRAINT CHK_Price_Positive CHECK (Price >= 0)
);
GO

-- Table: Orders (depends on Customers, Employees, PaymentMethods)
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    CustomerID INT NOT NULL,
    EmployeeID INT NOT NULL,
    MethodID INT NOT NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    CONSTRAINT FK_Orders_Employees FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),
    CONSTRAINT FK_Orders_PaymentMethods FOREIGN KEY (MethodID) REFERENCES PaymentMethods(MethodID)
);
GO

-- =================================================================
-- 3. Create the final junction table
-- =================================================================

-- Table: OrderDetails (depends on Orders and Products)
CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    CONSTRAINT CHK_Quantity_Positive CHECK (Quantity > 0)
);
GO

PRINT 'Database and tables created successfully!';
------------------------------------------------------------------------------------------------------------------------------------------------------------------
--TẠO CÁC INDEX CHỈ MỤC ĐỂ TỐI ƯU TÔC DỘ TRUY XUẤT 
------------------------------------------------------------------------------------------------------------------------------------------------------------------


-- =====================================================================
-- Tối ưu bảng Products
-- =====================================================================

-- Index trên khóa ngoại CategoryID để tăng tốc các truy vấn JOIN với bảng Categories
-- hoặc lọc sản phẩm theo danh mục.
CREATE INDEX IX_Products_CategoryID ON Products (CategoryID);

-- Index trên khóa ngoại SupplierID để tăng tốc các truy vấn JOIN với bảng Suppliers
-- hoặc lọc sản phẩm theo nhà cung cấp.
CREATE INDEX IX_Products_SupplierID ON Products (SupplierID);

-- Index trên cột ProductName vì đây là cột thường được dùng để tìm kiếm sản phẩm.
CREATE INDEX IX_Products_ProductName ON Products (ProductName);



-- =====================================================================
-- Tối ưu bảng Orders
-- =====================================================================

-- Index trên khóa ngoại EmployeeID để tìm kiếm nhanh các đơn hàng do một nhân viên xử lý.
CREATE INDEX IX_Orders_EmployeeID ON Orders (EmployeeID);

-- Index trên khóa ngoại CustomerID, rất quan trọng để lấy lịch sử đơn hàng của khách hàng.
CREATE INDEX IX_Orders_CustomerID ON Orders (CustomerID);

-- Index trên khóa ngoại MethodID để tăng tốc các báo cáo, thống kê theo phương thức thanh toán.
CREATE INDEX IX_Orders_MethodID ON Orders (MethodID);

-- Index trên cột OrderDate vì các truy vấn báo cáo thường lọc và sắp xếp theo ngày đặt hàng.
CREATE INDEX IX_Orders_OrderDate ON Orders (OrderDate);


-- =====================================================================
-- Tối ưu bảng OrderDetails
-- =====================================================================

-- Index trên khóa ngoại OrderID để lấy tất cả các chi tiết của một đơn hàng một cách nhanh chóng.
CREATE INDEX IX_OrderDetails_OrderID ON OrderDetails (OrderID);

-- Index trên khóa ngoại ProductID để tìm tất cả các đơn hàng có chứa một sản phẩm cụ thể.
CREATE INDEX IX_OrderDetails_ProductID ON OrderDetails (ProductID);


-- =====================================================================
-- Tối ưu bảng Employees
-- =====================================================================

-- Index trên khóa ngoại RoleID để tìm kiếm các nhân viên theo vai trò.
CREATE INDEX IX_Employees_RoleID ON Employees (RoleID);

-- Index trên khóa ngoại AccountID để tăng tốc JOIN với bảng Accounts.
CREATE INDEX IX_Employees_AccountID ON Employees (AccountID);


-- =====================================================================
-- Tối ưu bảng Accounts
-- =====================================================================

-- UNIQUE INDEX trên cột Username để đảm bảo tên đăng nhập là duy nhất và tăng tốc
-- tối đa cho quá trình xác thực đăng nhập (tìm kiếm theo username).
CREATE UNIQUE INDEX IX_Accounts_Username ON Accounts (Username);


-- =====================================================================
-- Tối ưu bảng Customers
-- =====================================================================

-- Index trên cột CustomerName để hỗ trợ tìm kiếm khách hàng theo tên.
CREATE INDEX IX_Customers_CustomerName ON Customers (CustomerName);

-- Index trên cột CustomerPhone để hỗ trợ tìm kiếm khách hàng theo số điện thoại.
CREATE INDEX IX_Customers_CustomerPhone ON Customers (CustomerPhone);


------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- kiểm tra toàn bộ các bảng: 
------------------------------------------------------------------------------------------------------------------------------------------------------------------
select * from Products
select * from Suppliers
select * from Categories
select * from Customers
select * from Employees
select * from Orders
select * from OrderDetails
select * from Accounts
select * from Roles
select * from PaymentMethods

------------------------------------------------------------------------------------------------------------------------------------------------------------
-- TRIỂN KHAI THÊM DỮ LIỆU VÀO BẢNG 
------------------------------------------------------------------------------------------------------------------------------------------------------------

-- Insert data for table Roles
INSERT INTO Roles (RoleName, RoleDesc) VALUES
('Admin', 'Has full access to the system.'),
('Sale Staff', 'Handles sales, orders, and customer interactions.'),
('Warehouse Staff', 'Manages inventory, stock, and product shipments.');
GO
USE Project_Store_X_DB;
GO
DECLARE @Pass NVARCHAR(255);
SET @Pass = '$2a$11$u9vZJQmqG1uxXt9GxF1EROO.qw7tzuR/PDX45Cq6r/5HW89Ky0K4u'; 


-- Insert data for table Accounts
INSERT INTO Accounts (Username, PasswordHash) VALUES
('hieushipperadmin', @Pass),  -- ID: 1
('sale01',           @Pass), -- ID: 2
('sale02',           @Pass), -- ID: 3
('warehouse01',      @Pass), -- ID: 4
('warehouse02',      @Pass); -- ID: 5
GO



-- Insert data for table Suppliers
INSERT INTO Suppliers (SupplierName, SupplierEmail, SupplierPhone, SupplierAddress) VALUES
('Good Smile Company', 'contact@goodsmile.jp', '03-5209-3111', 'Tokyo, Japan'),
('Aniplex+', 'support@aniplexplus.com', '03-5211-7555', 'Tokyo, Japan'),
('Bandai Spirits', 'info@bandai.co.jp', '03-6731-2525', 'Tokyo, Japan'),
('Alter', 'info@alter-web.jp', '03-3527-1877', 'Tokyo, Japan'),
('Kotobukiya', 'support@kotobukiya.co.jp', '042-522-3500', 'Tachikawa, Japan'),
('MegaHouse', 'support@megahobby.jp', '04-7146-0651', 'Chiba, Japan');
GO

-- Insert data for table Customers
INSERT INTO Customers (CustomerName, CustomerPhone, CustomerAddress) VALUES
('An Van Nguyen', '0901234567', '123 Le Loi, District 1, Ho Chi Minh City'),
('Bich Thi Tran', '0912345678', '456 Hai Ba Trung, Hanoi'),
('Cuong Minh Le', '0987654321', '789 Nguyen Van Linh, Da Nang'),
('Dung Thuy Pham', '0934567890', '101 Vo Van Tan, District 3, Ho Chi Minh City'),
('Em Van Hoang', '0978123456', '212 Ly Thuong Kiet, Hanoi'),
('Ha Thi Vo', '0945678123', '313 Tran Phu, Can Tho');
GO

-- Insert data for table PaymentMethods
INSERT INTO PaymentMethods (MethodName) VALUES
('Cash'),
('Credit Card'),
('MoMo Wallet'),
('Bank Transfer'),
('ZaloPay'),
('Cash on Delivery (COD)');
GO

-- Insert data for table Categories
INSERT INTO Categories (CategoryName, CategoryDesc) VALUES
('Scale Figure', 'Highly detailed 1/7, 1/8, etc. scale figures.'),
('Nendoroid', 'Chibi-style figures with interchangeable faceplates and body parts.'),
('Figma', 'Action figures with a flexible joint system.'),
('Prize Figure', 'Affordable figures often found as prizes in crane games.'),
('Statue', 'Static, non-articulated figures focused on sculpture.'),
('Chibi Figure', 'Cute figures with a large head and small body style.');
GO

-- Insert data for table Employees
INSERT INTO Employees (EmployeeName, Position, RoleID, AccountID) VALUES
-- 1. Admin
(N'Pham Quang Hieu', 'System Admin',    1, 1), 
-- 2. Sale (RoleID = 2)
(N'sale02', 'Salesman',        2, 2),
(N'sale02', 'Salesman',        2, 3), 
-- 3. Warehouse (RoleID = 3)
(N'warehouse01',       'Inventory Keeper',3, 4), 
(N'warehouse02',       'Inventory Keeper',3, 5); 
GO

-- Insert data for table Products
INSERT INTO Products (ProductName, ProductDesc, ImageURL, Price, InventoryQuantity, CategoryID, SupplierID) VALUES
('Tanjiro Kamado 1/8 Scale Figure', 'Tanjiro figure in Hinokami Kagura pose.', '/images/tanjiro_scale.jpg', 1500000, 15, 1, 2),
('Nendoroid Nezuko Kamado', 'Nendoroid Nezuko with her wooden box and various expressions.', '/images/nezuko_nendo.jpg', 850000, 25, 2, 1),
('Figma Zenitsu Agatsuma', 'Figma Zenitsu with Thunder Breathing effects.', '/images/zenitsu_figma.jpg', 1200000, 10, 3, 1),
('Inosuke Hashibira Prize Figure', 'Affordable Inosuke figure from Bandai.', '/images/inosuke_prize.jpg', 500000, 50, 4, 3),
('Giyu Tomioka 1/8 Scale Figure', 'Water Hashira Giyu figure with water effects.', '/images/giyu_scale.jpg', 1800000, 12, 1, 4),
('Shinobu Kocho 1/7 Scale Figure', 'Graceful Insect Hashira Shinobu figure.', '/images/shinobu_scale.jpg', 1750000, 18, 1, 4);
GO

-- Insert data for table ordder
INSERT INTO Orders (CustomerID, EmployeeID, MethodID, OrderDate) VALUES
(1, 2, 2, '2025-10-20 10:30:00'),
(2, 3, 3, '2025-10-21 14:00:00'),
(1, 2, 1, '2025-10-22 11:00:00'),
(3, 3, 4, '2025-10-22 18:45:00'),
(4, 2, 6, '2025-10-23 09:15:00'),
(5, 3, 2, '2025-10-24 16:20:00');
GO

-- Insert data for table OrderDetails
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, TotalAmount) VALUES
(1, 1, 1, 1500000),  
(1, 2, 1, 850000),
(2, 4, 2, 1000000),
(3, 5, 1, 1800000),
(4, 6, 1, 1750000),
(5, 3, 1, 1200000);
GO
-------------------------------------------------------------------------------------
INSERT INTO Suppliers (SupplierName, SupplierEmail, SupplierPhone, SupplierAddress)
	VALUES (N'Ufotable Studio', 'contact@ufotable.com', '0999888777', N'Tokyo, Japan ');


Update Suppliers
set SupplierAddress = N'Tokyo, Japan'
WHERE SupplierPhone = N'0999888777'
SELECT *
FROM Suppliers
WHERE SupplierName = N'Ufotable Studio';


INSERT INTO Products (ProductName, ProductDesc, ImageURL, Price, InventoryQuantity, CategoryID, SupplierID) VALUES
('Tanjiro Kamado chibi', 'Tanjiro figure in Hinokami Kagura pose.', '/images/tanjiro_chibi.jpg', 1400000, 4, 1, 2)



USE Project_Store_X_DB;
GO

-- ====================================================================================
-- 🛑 KHU VỰC CẤU HÌNH MẬT KHẨU
-- Hãy dán chuỗi Hash bạn vừa copy từ C# vào giữa 2 dấu nháy đơn '...' bên dưới
-- ====================================================================================
DECLARE @HashChung NVARCHAR(255);
SET @HashChung = '$2a$11$tStXtCACRJBp.cCydSuu4UCeSemAox0ek76ywsv8N4PWqxE3/gPIQRy
'; 

-- Ví dụ: SET @HashChung = '$2a$11$ZqK...';
-- Nếu chưa có hash, chạy tạm dòng dưới để tránh lỗi (nhưng sẽ ko đăng nhập được):
IF @HashChung = '$2a$11$tStXtCACRJBp.cCydSuu4UCeSemAox0ek76ywsv8N4PWqxE3/gPIQRy
' SET @HashChung = 'TEMP_HASH'; 



------------------------------------------------------------------------------------------------------------------------------------------------------------
-- TRIỂN KHAI TẠO PROCEDURE VÀ STRIGGER CHO DỰ ÁN 
------------------------------------------------------------------------------------------------------------------------------------------------------------
USE Project_Store_X_DB;
GO

-- =============================================
-- Tạo SP: Chỉ lấy thông tin theo Username, kèm theo PasswordHash để BLL kiểm tra
-- =============================================
CREATE OR ALTER PROCEDURE usp_GetAccountByUsername
    @Username NVARCHAR(50)
AS
BEGIN
    SELECT 
        e.EmployeeID,
        e.EmployeeName,
        r.RoleName,
        a.PasswordHash -- Cần lấy cột này để mang về C# so sánh
    FROM Employees e
    JOIN Accounts a ON e.AccountID = a.AccountID
    JOIN Roles r ON e.RoleID = r.RoleID
    WHERE a.Username = @Username;
END;
GO
USE Project_Store_X_DB;
GO

-- ================================================================
-- Trigger: Ngăn chặn xóa tài khoản Admin (trg_PreventDeleteAdmin)
-- ================================================================
CREATE OR ALTER TRIGGER trg_PreventDeleteAdmin
ON Employees
FOR DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra xem trong danh sách bị xóa có ai là Admin không
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN Roles r ON d.RoleID = r.RoleID
        WHERE r.RoleName = 'Admin'
    )
    BEGIN
        -- Nếu có, báo lỗi và hoàn tác (Rollback)
        RAISERROR (N'Security Error: Cannot delete Administrator (Admin) account.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -- Tùy chọn: Xóa luôn Account tương ứng với nhân viên bị xóa
    DELETE FROM Accounts
    WHERE AccountID IN (SELECT AccountID FROM deleted);
END;
GO
-- ================================================================
-- 1. TRIGGER: Chặn XÓA tài khoản ID = 1 (Super Admin)
-- ================================================================
CREATE OR ALTER TRIGGER trg_PreventDeleteSuperAdmin
ON Employees
FOR DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra nếu trong danh sách bị xóa có ID = 1
    IF EXISTS (SELECT 1 FROM deleted WHERE EmployeeID = 1)
    BEGIN
        RAISERROR (N'Lỗi bảo mật: Không thể xóa tài khoản Super Admin (ID: 1).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Vẫn giữ logic cũ: Xóa Account tương ứng nếu xóa nhân viên thường thành công
    DELETE FROM Accounts 
    WHERE AccountID IN (SELECT AccountID FROM deleted);
END;
GO
------------------------------------------------------
--SCRIPT SQL CHUYỂN ID 2 VỀ 1 (Chạy trong SQL Server)
------------------------------------------------------

USE Project_Store_X_DB;
GO

BEGIN TRANSACTION;

    PRINT '>>> Bắt đầu quy trình chuyển đổi ID...';

    -- BƯỚC 1: Tạm tắt các ràng buộc (Foreign Keys) để tránh lỗi khi xóa/sửa
    -- Tắt kiểm tra khóa ngoại ở bảng Orders (nơi tham chiếu đến Employee)
    ALTER TABLE Orders NOCHECK CONSTRAINT FK_Orders_Employees;
    
    -- Tắt tạm Trigger bảo vệ (để cho phép xóa Admin cũ là ID 2)
    DISABLE TRIGGER ALL ON Employees;

    -- BƯỚC 2: Cho phép chèn ID thủ công vào bảng Employees
    SET IDENTITY_INSERT Employees ON;

    -- BƯỚC 3: Lưu thông tin của ID 2 vào biến tạm
    DECLARE @OldID INT = 2;
    DECLARE @NewID INT = 1;
    
    DECLARE @Name NVARCHAR(255);
    DECLARE @Pos NVARCHAR(100);
    DECLARE @Role INT;
    DECLARE @Acc INT;

    SELECT 
        @Name = EmployeeName, 
        @Pos = Position, 
        @Role = RoleID, 
        @Acc = AccountID
    FROM Employees WHERE EmployeeID = @OldID;

    -- Kiểm tra xem có tìm thấy ID 2 không
    IF @Name IS NOT NULL
    BEGIN
        -- BƯỚC 4: Xóa ID 2 (Để nhả AccountID ra, vì AccountID là Unique)
        -- Nếu không xóa trước, bước Insert sẽ lỗi trùng AccountID
        DELETE FROM Employees WHERE EmployeeID = @OldID;

        -- BƯỚC 5: Chèn lại với ID mới là 1
        INSERT INTO Employees (EmployeeID, EmployeeName, Position, RoleID, AccountID)
        VALUES (@NewID, @Name, @Pos, @Role, @Acc);

        -- BƯỚC 6: Cập nhật dữ liệu liên quan (Nếu lỡ có đơn hàng nào gắn với ID 2)
        UPDATE Orders SET EmployeeID = @NewID WHERE EmployeeID = @OldID;
        
        PRINT '>>> Đã chuyển đổi thành công EmployeeID từ 2 sang 1.';
    END
    ELSE
    BEGIN
        PRINT '>>> KHÔNG TÌM THẤY EmployeeID = 2. Vui lòng kiểm tra lại.';
    END

    -- BƯỚC 7: Tắt chế độ chèn ID thủ công
    SET IDENTITY_INSERxcT Employees OFF;

    -- BƯỚC 8: Bật lại ràng buộc và Trigger
    ALTER TABLE Orders CHECK CONSTRAINT FK_Orders_Employees;
    ENABLE TRIGGER ALL ON Employees;

COMMIT TRANSACTION;
GO

-- Kiểm tra lại kết quả
SELECT * FROM Employees;
-- ================================================================
-- 2. TRIGGER: Chặn SỬA Quyền của ID = 1
-- (Ngăn không cho ai hạ quyền Admin của ID 1 xuống Staff)
-- ================================================================
CREATE OR ALTER TRIGGER trg_PreventDemoteSuperAdmin
ON Employees
FOR UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra: Nếu đang update dòng ID = 1
    IF EXISTS (SELECT 1 FROM inserted WHERE EmployeeID = 1)
    BEGIN
        -- Kiểm tra xem RoleID có bị thay đổi không
        -- Giả sử RoleID = 1 là Admin (Dựa theo script data cũ của bạn)
        -- Logic: Nếu ID=1 mà RoleID trong bảng Inserted khác 1 -> Chặn ngay
        
        -- Lấy RoleID của Admin (để chắc chắn)
        DECLARE @AdminRoleID INT = (SELECT RoleID FROM Roles WHERE RoleName = 'Admin');

        IF EXISTS (SELECT 1 FROM inserted WHERE EmployeeID = 1 AND RoleID != @AdminRoleID)
        BEGIN
             RAISERROR (N'Lỗi bảo mật: Không được phép thay đổi quyền hạn của Super Admin.', 16, 1);
             ROLLBACK TRANSACTION;
             RETURN;
        END
    END
END;
GO
-- ================================================================
-- 2. TRIGGER: tự cập nhập kho hàng sau mỗi khi ordeDetail mới được tạo( tự trừ )
-- ================================================================
USE Project_Store_X_DB;
GO



CREATE TRIGGER trg_ReduceStock_AfterOrder
ON OrderDetails
AFTER INSERT
AS
BEGIN
    -- SET NOCOUNT ON để ngăn SQL trả về thông báo "X rows affected" làm nhiễu ứng dụng
    SET NOCOUNT ON;

    -- Cập nhật bảng Products
    -- Lấy số lượng tồn kho cũ TRỪ ĐI số lượng vừa bán (lấy từ bảng ảo 'inserted')
    UPDATE p
    SET p.InventoryQuantity = p.InventoryQuantity - i.Quantity
    FROM Products p
    INNER JOIN inserted i ON p.ProductID = i.ProductID;
END;
GO






------------------------------------------------------------------------------------------------------------------------------------------------------------
--TEST TÔC ĐỘ TRUY XUẤT 
------------------------------------------------------------------------------------------------------------------------------------------------------------
----Quá trình triển khai test tốc độc truy xuất như sau :  
----B1: bật thống kê time 
--SET STATISTICS TIME ON;
----B2 tiến hành trruy xuất trừng trường hợp.
---- Truy vấn 1 (Inefficient)
--SELECT *
--FROM Products
--WHERE SellingPrice < 2000000;

---- Truy vấn 2 (More efficient)
--SELECT ProductName, SellingPrice, ProductImageURL
--FROM Products
--WHERE SellingPrice < 2000000;

----B3: xem kết quá tại bảng thông báo 



------------------------------------------------------------------------------------------------------------------------------------------------------------
-- CÁC LỆNH TRUY VẤN THỬ NGHIỆM 
------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 3. GetEmployeeSalesPerformance
SELECT 
    E.EmployeeID,
    E.EmployeeName,
    COUNT(O.OrderID) AS OrdersHandled,
    SUM(OD.TotalAmount) AS TotalSales
FROM Employees E
JOIN Orders O ON E.EmployeeID = O.EmployeeID
JOIN OrderDetails OD ON O.OrderID = OD.OrderID
GROUP BY E.EmployeeID
ORDER BY TotalSales DESC;
--2. GetCustomerPurchaseHistory
SELECT 
    C.CustomerID,
    C.CustomerName,
    COUNT(O.OrderID) AS TotalOrders,
    SUM(OD.TotalAmount) AS TotalSpent
FROM Customers C
LEFT JOIN Orders O ON C.CustomerID = O.CustomerID
JOIN OrderDetails OD ON O.OrderID = OD.OrderID
GROUP BY C.CustomerID, C.CustomerName
ORDER BY TotalSpent DESC;
--1. GetTop5SellingProducts

SELECT TOP 5 p.ProductName, ISNULL(SUM(od.Quantity),0) AS QuantitySold
                FROM Orders o
                INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
                INNER JOIN Products p ON od.ProductID = p.ProductID
                WHERE o.OrderDate BETWEEN @start AND @end
                GROUP BY p.ProductName
                ORDER BY QuantitySold DESC
--4. tìm những sản phẩm có số lượng săp hết hàng 
SELECT ProductID, ProductName, InventoryQuantity
FROM Products
WHERE InventoryQuantity <= 5
ORDER BY InventoryQuantity ASC;
-- lệnh xem tông thu nhập theo thời gian 
SELECT c.CustomerID, c.CustomerName, ISNULL(SUM(od.TotalAmount),0) AS TotalSpent
 FROM Orders o
INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
INNER JOIN Customers c ON o.CustomerID = c.CustomerID
WHERE o.OrderDate BETWEEN @start AND @end
GROUP BY c.CustomerID, c.CustomerName
ORDER BY TotalSpent DESC
-- lênh xem thông tin đơn hàng cho nhân viên
SELECT 
    C.CustomerID,
    C.CustomerName,
    C.CustomerPhone,
    C.CustomerAddress,
    COUNT(O.OrderID) AS TotalOrders,
    SUM(Od.TotalAmount) AS TotalSpent,
    MAX(O.OrderDate) AS LastOrderDate
FROM Customers C
LEFT JOIN Orders O ON C.CustomerID = O.CustomerID
JOIN OrderDetails OD ON C.CustomerID = O.CustomerID
GROUP BY C.CustomerID, C.CustomerName, C.CustomerPhone, C.CustomerAddress
ORDER BY TotalSpent DESC;


-------------------------------------------------------------------------------------------------------------------------------------------------------

-------------------------------------------------------------------------------------------------------------
--kiểm tra khóa hiện tại của bảng 
EXEC sp_helpconstraint 'Transactions';
-- đổi sang tự động cập nhập ngày giờ 
ALTER TABLE Transactions
ADD CONSTRAINT DF_Transactions_TransactionDate DEFAULT GETDATE() FOR TransactionDate;
-- chuyển cột TransactionDate thành NOT NULL
ALTER TABLE Transactions
ALTER COLUMN TransactionDate DATETIME NOT NULL;


---------------------------------------------------------------------------------------------------------------
-- chiến lược backup full-diff-log
-- chuột phải vào DB và backup :)))





-- chú ý: khi khôi phục phải ở master
USE master;
GO
USE Project_Store_X_DB;
GO

-- 1️⃣ Tắt tạm constraint để tránh lỗi xóa
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- 2️⃣ Xóa toàn bộ dữ liệu trong các bảng
EXEC sp_MSforeachtable 'DELETE FROM ?';

-- 3️⃣ Bật lại constraint
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

-- 4️⃣ Reset toàn bộ IDENTITY về 0 (khi chèn bản ghi mới, ID sẽ bắt đầu từ 1)
DECLARE @TableName NVARCHAR(255);
DECLARE TableCursor CURSOR FOR
SELECT name FROM sys.tables
WHERE OBJECTPROPERTY(OBJECT_ID, 'TableHasIdentity') = 1;

OPEN TableCursor;
FETCH NEXT FROM TableCursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @SQL NVARCHAR(400);
    SET @SQL = 'DBCC CHECKIDENT (''' + @TableName + ''', RESEED, 0)';
    EXEC (@SQL);

    FETCH NEXT FROM TableCursor INTO @TableName;
END;

CLOSE TableCursor;
DEALLOCATE TableCursor;

PRINT '✅ Toàn bộ dữ liệu đã được xóa và IDENTITY reset về 1.';
