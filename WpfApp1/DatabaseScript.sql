-- ============================================================
-- СКРИПТ СОЗДАНИЯ БАЗЫ ДАННЫХ ДЛЯ РЕСТОРАНА "МОРСКОЙ ВКУС"
-- ============================================================

CREATE DATABASE Restoran;
GO

USE Restoran;
GO

-- 1. ТАБЛИЦА ПОЛЬЗОВАТЕЛИ
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    login NVARCHAR(20) NOT NULL UNIQUE,
    parol NVARCHAR(20) NOT NULL,
    FIO NVARCHAR(50) NOT NULL,
    Rol NVARCHAR(25) NOT NULL CHECK (Rol IN ('admin', 'ofitsiant')),
    phone NVARCHAR(50) NULL,
    DataSosdania DATE NULL
);

-- 2. ТАБЛИЦА СТОЛЫ
CREATE TABLE Tables (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Number INT NOT NULL UNIQUE,
    Vmestimost INT NOT NULL CHECK (Vmestimost > 0),
    Status NVARCHAR(20) NOT NULL DEFAULT 'svoboden' CHECK (Status IN ('svoboden', 'bron', 'zaniat', 'uborka'))
);

-- 3. ТАБЛИЦА КАТЕГОРИИ МЕНЮ
CREATE TABLE KategoriiMenu (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

-- 4. ТАБЛИЦА БЛЮДА
CREATE TABLE Blyda (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IDKategoriiMenu INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Opisanie NVARCHAR(MAX) NULL,
    prise MONEY NOT NULL CHECK (prise >= 0),
    Ves NVARCHAR(50) NULL,
    Foto NVARCHAR(255) NULL,
    CONSTRAINT FK_Blyda_KategoriiMenu FOREIGN KEY (IDKategoriiMenu) REFERENCES KategoriiMenu(Id)
);

-- 5. ТАБЛИЦА БРОНИ
CREATE TABLE Broni (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IDTables INT NOT NULL,
    IDUsers INT NOT NULL,
    FIO NVARCHAR(150) NOT NULL,
    phone NVARCHAR(30) NOT NULL,
    Nachalo DATETIME NOT NULL,
    [End] DATETIME NULL,
    Status NVARCHAR(30) NOT NULL CHECK (Status IN ('podtverzhdena', 'posazhen', 'zavershena', 'otmenena')),
    Komentari NVARCHAR(500) NULL,
    CONSTRAINT FK_Broni_Tables FOREIGN KEY (IDTables) REFERENCES Tables(ID),
    CONSTRAINT FK_Broni_Users FOREIGN KEY (IDUsers) REFERENCES Users(Id)
);

-- 6. ТАБЛИЦА ЗАКАЗЫ
CREATE TABLE Zakazy (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdTables INT NOT NULL,
    IdUsers INT NOT NULL,
    KolVo INT NOT NULL CHECK (KolVo > 0),
    Nachalo DATETIME2 NOT NULL,
    [End] DATETIME2 NULL,
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('otkryt', 'zakryt', 'otmenen')),
    Summa_Itogo MONEY NOT NULL CHECK (Summa_Itogo >= 0),
    CONSTRAINT FK_Zakazy_Tables FOREIGN KEY (IdTables) REFERENCES Tables(ID),
    CONSTRAINT FK_Zakazy_Users FOREIGN KEY (IdUsers) REFERENCES Users(Id)
);

-- 7. ТАБЛИЦА ПОЗИЦИИ ЗАКАЗА
CREATE TABLE PozitsiaZakaza (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdZakazy INT NOT NULL,
    IdBlyda INT NOT NULL,
    Kolvo INT NOT NULL CHECK (Kolvo > 0),
    Prise MONEY NOT NULL CHECK (Prise >= 0),
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('otpravleno', 'gotovo', 'podano')),
    Komentari NVARCHAR(MAX) NULL,
    CONSTRAINT FK_PozitsiaZakaza_Zakazy FOREIGN KEY (IdZakazy) REFERENCES Zakazy(Id),
    CONSTRAINT FK_PozitsiaZakaza_Blyda FOREIGN KEY (IdBlyda) REFERENCES Blyda(Id)
);

-- 8. ТАБЛИЦА ОПЛАТЫ
CREATE TABLE Oplaty (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdZakazy INT NOT NULL,
    IdUsers INT NOT NULL,
    Summa MONEY NOT NULL CHECK (Summa >= 0),
    Chaevye MONEY NULL CHECK (Chaevye >= 0),
    Sposob_oplate NVARCHAR(50) NOT NULL CHECK (Sposob_oplate IN (N'Наличные', N'Карта', N'QR-код')),
    DataOplat DATETIME NOT NULL,
    CONSTRAINT FK_Oplaty_Zakazy FOREIGN KEY (IdZakazy) REFERENCES Zakazy(Id),
    CONSTRAINT FK_Oplaty_Users FOREIGN KEY (IdUsers) REFERENCES Users(Id)
);
GO

-- ============================================================
-- СОЗДАНИЕ ТРИГГЕРОВ
-- ============================================================

-- Триггер 1: При создании заказа стол становится занятым
CREATE TRIGGER TR_Zakaz_Insert_UpdateTable
ON Zakazy
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Tables
    SET Status = N'zaniat'
    WHERE ID IN (SELECT IdTables FROM inserted);
END;
GO

-- Триггер 2: При оплате заказа стол освобождается
CREATE TRIGGER TR_Zakaz_Update_Status
ON Zakazy
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Tables
    SET Status = N'svoboden'
    WHERE ID IN (
        SELECT i.IdTables
        FROM inserted i
        JOIN deleted d ON i.Id = d.Id
        WHERE i.Status = N'zakryt' AND d.Status != N'zakryt'
    );
END;
GO

-- ============================================================
-- ЗАПОЛНЕНИЕ ТЕСТОВЫМИ ДАННЫМИ
-- ============================================================

-- Добавляем администратора и официанта
INSERT INTO Users (login, parol, FIO, Rol, phone) VALUES 
(N'admin', N'admin123', N'Иванова Анна', N'admin', N'+79990000000'),
(N'waiter1', N'waiter123', N'Петров Сергей', N'ofitsiant', N'+79990000001');

-- Добавляем 20 столов
INSERT INTO Tables (Number, Vmestimost, Status) VALUES
(1, 2, 'svoboden'), (2, 2, 'svoboden'), (3, 4, 'svoboden'), (4, 4, 'svoboden'), (5, 2, 'svoboden'),
(6, 4, 'svoboden'), (7, 4, 'svoboden'), (8, 2, 'svoboden'), (9, 2, 'svoboden'), (10, 2, 'svoboden'),
(11, 4, 'svoboden'), (12, 4, 'svoboden'), (13, 2, 'svoboden'), (14, 2, 'svoboden'), (15, 4, 'svoboden'),
(16, 4, 'svoboden'), (17, 2, 'svoboden'), (18, 2, 'svoboden'), (19, 6, 'svoboden'), (20, 6, 'svoboden');

-- Добавляем категории меню
INSERT INTO KategoriiMenu (Name) VALUES 
(N'Напитки'), (N'Горячее'), (N'Салаты'), (N'Закуски'), (N'Десерты');

-- Добавляем блюда (примеры для меню)
INSERT INTO Blyda (IDKategoriiMenu, Name, Opisanie, prise, Ves, Foto) VALUES 
(2, N'Лосось на гриле', N'Стейк лосося, овощи гриль', 1190.00, N'280 г', N'/Images/losos.jpg'),
(2, N'Мидии в сливочном соусе', N'Мидии, сливки, чеснок', 690.00, N'300 г', N'/Images/midii.jpg'),
(2, N'Кальмары на гриле', N'Кольца кальмара, лимон', 790.00, N'250 г', N'/Images/kalmar.jpg'),
(1, N'Белое вино', N'Сухое белое вино', 450.00, N'150 мл', N'/Images/vino.jpg'),
(4, N'Устрицы Фин де Клер', N'Свежие устрицы, лимон', 390.00, N'6 шт', N'/Images/ustricy.jpg'),
(5, N'Чизкейк', N'Классический с ягодным соусом', 350.00, N'150 г', N'/Images/cheesecake.jpg');
GO