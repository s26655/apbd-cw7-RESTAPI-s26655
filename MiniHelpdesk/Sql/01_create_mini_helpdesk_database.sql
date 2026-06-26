IF DB_ID(N'MiniHelpdeskDb') IS NULL
BEGIN
    CREATE DATABASE [MiniHelpdeskDb];
END
GO

USE [MiniHelpdeskDb];
GO

IF OBJECT_ID(N'dbo.Tickets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tickets
    (
        Id INT IDENTITY(1,1) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Status NVARCHAR(20) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,

        CONSTRAINT PK_Tickets PRIMARY KEY (Id),
        CONSTRAINT CK_Tickets_Status CHECK (Status IN (N'Open', N'Closed')),
        CONSTRAINT CK_Tickets_Title_NotEmpty CHECK (LEN(LTRIM(RTRIM(Title))) > 0)
    );
END
GO

IF OBJECT_ID(N'dbo.TicketComments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TicketComments
    (
        Id INT IDENTITY(1,1) NOT NULL,
        TicketId INT NOT NULL,
        Author NVARCHAR(100) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,

        CONSTRAINT PK_TicketComments PRIMARY KEY (Id),
        CONSTRAINT FK_TicketComments_Tickets
            FOREIGN KEY (TicketId) REFERENCES dbo.Tickets(Id),
        CONSTRAINT CK_TicketComments_Author_NotEmpty CHECK (LEN(LTRIM(RTRIM(Author))) > 0),
        CONSTRAINT CK_TicketComments_Content_NotEmpty CHECK (LEN(LTRIM(RTRIM(Content))) > 0)
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_TicketComments_TicketId'
      AND object_id = OBJECT_ID(N'dbo.TicketComments')
)
BEGIN
    CREATE INDEX IX_TicketComments_TicketId
    ON dbo.TicketComments(TicketId);
END
GO