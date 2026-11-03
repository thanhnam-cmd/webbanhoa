-- Script tạo CSDL website bán hoa Floria (SQL Server)
-- Sinh tự động từ EF Core: dotnet run --project WebBanHoa -- --export-sql Database/WebBanHoa_SqlServer.sql
-- Ứng dụng tự tạo CSDL + dữ liệu mẫu khi chạy lần đầu; script này dùng để tham khảo hoặc tạo thủ công.

IF DB_ID(N'WebBanHoa') IS NULL CREATE DATABASE [WebBanHoa];
GO
USE [WebBanHoa];
GO

CREATE TABLE [AdminUsers] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(200) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AdminUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    [Description] nvarchar(300) NULL,
    [Image] nvarchar(300) NULL,
    [SortOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ContactMessages] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [Phone] nvarchar(20) NULL,
    [Message] nvarchar(2000) NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ContactMessages] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [Phone] nvarchar(20) NULL,
    [Address] nvarchar(300) NULL,
    [PasswordHash] nvarchar(200) NOT NULL,
    [IsLocked] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [DataProtectionKeys] (
    [Id] int NOT NULL IDENTITY,
    [FriendlyName] nvarchar(max) NULL,
    [Xml] nvarchar(max) NULL,
    CONSTRAINT [PK_DataProtectionKeys] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [MediaFiles] (
    [Id] int NOT NULL IDENTITY,
    [FileName] nvarchar(200) NOT NULL,
    [ContentType] nvarchar(100) NOT NULL,
    [Data] varbinary(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_MediaFiles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Occasions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    [Description] nvarchar(300) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Occasions] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [PostCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PostCategories] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Sliders] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(150) NOT NULL,
    [Subtitle] nvarchar(300) NULL,
    [Image] nvarchar(300) NULL,
    [Link] nvarchar(300) NULL,
    [SortOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Sliders] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(20) NOT NULL,
    [CustomerId] int NULL,
    [SenderName] nvarchar(100) NOT NULL,
    [SenderPhone] nvarchar(20) NOT NULL,
    [SenderEmail] nvarchar(150) NULL,
    [ReceiverName] nvarchar(100) NOT NULL,
    [ReceiverPhone] nvarchar(20) NOT NULL,
    [Address] nvarchar(300) NOT NULL,
    [DeliveryDate] datetime2 NOT NULL,
    [DeliverySlot] nvarchar(30) NOT NULL,
    [CardMessage] nvarchar(300) NULL,
    [Note] nvarchar(500) NULL,
    [Subtotal] decimal(18,0) NOT NULL,
    [ShippingFee] decimal(18,0) NOT NULL,
    [Total] decimal(18,0) NOT NULL,
    [PaymentMethod] int NOT NULL,
    [PaymentStatus] int NOT NULL,
    [Status] int NOT NULL,
    [CancelReason] nvarchar(300) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [Slug] nvarchar(170) NOT NULL,
    [Sku] nvarchar(30) NULL,
    [CategoryId] int NOT NULL,
    [OccasionId] int NULL,
    [Price] decimal(18,0) NOT NULL,
    [SalePrice] decimal(18,0) NULL,
    [Image] nvarchar(300) NULL,
    [ShortDescription] nvarchar(400) NULL,
    [Description] nvarchar(max) NULL,
    [Meaning] nvarchar(400) NULL,
    [Stock] int NOT NULL,
    [Sold] int NOT NULL,
    [IsFeatured] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_Occasions_OccasionId] FOREIGN KEY ([OccasionId]) REFERENCES [Occasions] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [Posts] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Slug] nvarchar(220) NOT NULL,
    [Summary] nvarchar(500) NULL,
    [Content] nvarchar(max) NULL,
    [Image] nvarchar(300) NULL,
    [PostCategoryId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [Views] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Posts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Posts_PostCategories_PostCategoryId] FOREIGN KEY ([PostCategoryId]) REFERENCES [PostCategories] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [PaymentTransactions] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [Provider] nvarchar(30) NOT NULL,
    [TxnRef] nvarchar(50) NOT NULL,
    [TransactionNo] nvarchar(50) NULL,
    [Amount] decimal(18,0) NOT NULL,
    [BankCode] nvarchar(20) NULL,
    [ResponseCode] nvarchar(10) NULL,
    [Message] nvarchar(300) NULL,
    [IsSuccess] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentTransactions_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [OrderDetails] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [ProductId] int NULL,
    [ProductName] nvarchar(150) NOT NULL,
    [Image] nvarchar(300) NULL,
    [Price] decimal(18,0) NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderDetails_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE SET NULL
);
GO


CREATE UNIQUE INDEX [IX_AdminUsers_Username] ON [AdminUsers] ([Username]);
GO


CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);
GO


CREATE UNIQUE INDEX [IX_Customers_Email] ON [Customers] ([Email]);
GO


CREATE UNIQUE INDEX [IX_Occasions_Slug] ON [Occasions] ([Slug]);
GO


CREATE INDEX [IX_OrderDetails_OrderId] ON [OrderDetails] ([OrderId]);
GO


CREATE INDEX [IX_OrderDetails_ProductId] ON [OrderDetails] ([ProductId]);
GO


CREATE UNIQUE INDEX [IX_Orders_Code] ON [Orders] ([Code]);
GO


CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
GO


CREATE INDEX [IX_PaymentTransactions_OrderId] ON [PaymentTransactions] ([OrderId]);
GO


CREATE UNIQUE INDEX [IX_PostCategories_Slug] ON [PostCategories] ([Slug]);
GO


CREATE INDEX [IX_Posts_PostCategoryId] ON [Posts] ([PostCategoryId]);
GO


CREATE UNIQUE INDEX [IX_Posts_Slug] ON [Posts] ([Slug]);
GO


CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
GO


CREATE INDEX [IX_Products_OccasionId] ON [Products] ([OccasionId]);
GO


CREATE UNIQUE INDEX [IX_Products_Slug] ON [Products] ([Slug]);
GO


