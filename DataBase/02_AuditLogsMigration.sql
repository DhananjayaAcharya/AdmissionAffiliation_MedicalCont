-- SQL Migration: Verify and Create AuditLogs Table
-- Execute this script to ensure the AuditLogs table exists with the correct structure

-- Check if table exists, if not create it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AuditLogs')
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [AuditLogId] [bigint] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](max) NULL,
        [UserName] [nvarchar](max) NULL,
        [Module] [nvarchar](max) NULL,
        [Action] [nvarchar](max) NOT NULL,
        [LogType] [nvarchar](max) NOT NULL,
        [Status] [nvarchar](max) NOT NULL,
        [TableName] [nvarchar](max) NULL,
        [RecordId] [nvarchar](max) NULL,
        [OldValues] [nvarchar](max) NULL,
        [NewValues] [nvarchar](max) NULL,
        [ExceptionType] [nvarchar](max) NULL,
        [ExceptionMessage] [nvarchar](max) NULL,
        [StackTrace] [nvarchar](max) NULL,
        [Source] [nvarchar](max) NULL,
        [RequestPath] [nvarchar](max) NULL,
        [RequestMethod] [nvarchar](max) NULL,
        [Description] [nvarchar](max) NULL,
        [IPAddress] [nvarchar](max) NULL,
        [UserAgent] [nvarchar](max) NULL,
        [CreatedAt] [datetime2] NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([AuditLogId] ASC)
    );

    PRINT 'AuditLogs table created successfully.';
END
ELSE
BEGIN
    PRINT 'AuditLogs table already exists.';
END

-- Add indexes for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_UserName_CreatedAt')
    CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserName_CreatedAt] 
    ON [dbo].[AuditLogs] ([UserName], [CreatedAt] DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_LogType_CreatedAt')
    CREATE NONCLUSTERED INDEX [IX_AuditLogs_LogType_CreatedAt] 
    ON [dbo].[AuditLogs] ([LogType], [CreatedAt] DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Status_CreatedAt')
    CREATE NONCLUSTERED INDEX [IX_AuditLogs_Status_CreatedAt] 
    ON [dbo].[AuditLogs] ([Status], [CreatedAt] DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_CreatedAt')
    CREATE NONCLUSTERED INDEX [IX_AuditLogs_CreatedAt] 
    ON [dbo].[AuditLogs] ([CreatedAt] DESC);

PRINT 'Indexes created successfully.';

-- Verify table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AuditLogs' 
ORDER BY ORDINAL_POSITION;
