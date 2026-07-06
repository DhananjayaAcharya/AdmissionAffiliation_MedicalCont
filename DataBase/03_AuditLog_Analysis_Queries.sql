-- ============================================================================
-- SQL QUERIES FOR AUDIT LOG ANALYSIS
-- Use these queries to analyze your audit logs
-- ============================================================================

-- ============================================================================
-- 1. VIEW ALL RECENT LOGS
-- ============================================================================

-- View last 100 audit log entries
SELECT TOP 100 
    AuditLogId,
    CreatedAt,
    UserName,
    Module,
    Action,
    LogType,
    Status,
    Description,
    IPAddress
FROM AuditLogs
ORDER BY CreatedAt DESC;


-- ============================================================================
-- 2. USER ACTIVITY REPORTS
-- ============================================================================

-- View all activities by a specific user today
DECLARE @UserName NVARCHAR(MAX) = 'college@university.edu'
SELECT 
    CreatedAt,
    Module,
    Action,
    LogType,
    Status,
    Description
FROM AuditLogs
WHERE UserName = @UserName
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- Top 10 most active users
SELECT TOP 10
    UserName,
    COUNT(*) AS TotalActions,
    COUNT(CASE WHEN Status = 'Success' THEN 1 END) AS SuccessCount,
    COUNT(CASE WHEN Status = 'Failure' THEN 1 END) AS FailureCount,
    MIN(CreatedAt) AS FirstAction,
    MAX(CreatedAt) AS LastAction
FROM AuditLogs
WHERE CreatedAt >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))  -- Last 7 days
GROUP BY UserName
ORDER BY TotalActions DESC;


-- User activity by hour
SELECT TOP 24
    CAST(CreatedAt AS DATE) AS [Date],
    DATEPART(HOUR, CreatedAt) AS [Hour],
    COUNT(*) AS ActionCount,
    COUNT(DISTINCT UserName) AS UniqueUsers
FROM AuditLogs
WHERE CreatedAt >= DATEADD(DAY, -1, GETDATE())  -- Last 24 hours
GROUP BY CAST(CreatedAt AS DATE), DATEPART(HOUR, CreatedAt)
ORDER BY CreatedAt DESC;


-- ============================================================================
-- 3. SESSION TRACKING (LOGIN/LOGOUT)
-- ============================================================================

-- All login events today
SELECT
    CreatedAt,
    UserName,
    Module,
    Action,
    IPAddress,
    UserAgent
FROM AuditLogs
WHERE LogType = 'Session'
AND Action = 'Login'
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- All logout events today
SELECT
    CreatedAt,
    UserName,
    Module,
    Action,
    DATEDIFF(MINUTE, 
        (SELECT MAX(CreatedAt) FROM AuditLogs L2 
         WHERE L2.UserName = AuditLogs.UserName 
         AND L2.Action = 'Login' 
         AND L2.CreatedAt < AuditLogs.CreatedAt), 
        CreatedAt) AS SessionDurationMinutes
FROM AuditLogs
WHERE LogType = 'Session'
AND Action = 'Logout'
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- Users currently logged in (no logout after login)
SELECT DISTINCT
    UserName,
    MAX(CreatedAt) AS LastActivity,
    DATEDIFF(MINUTE, MAX(CreatedAt), GETDATE()) AS IdleMinutes,
    COUNT(*) AS ActionsSinceLogin
FROM AuditLogs
WHERE UserName IN (
    SELECT UserName FROM AuditLogs L
    WHERE LogType = 'Session' AND Action = 'Login'
    AND CreatedAt = (
        SELECT MAX(CreatedAt) FROM AuditLogs L2
        WHERE L2.UserName = L.UserName AND L2.LogType = 'Session'
    )
)
GROUP BY UserName
ORDER BY LastActivity DESC;


-- ============================================================================
-- 4. FAILURE & EXCEPTION ANALYSIS
-- ============================================================================

-- All failures in the last 24 hours
SELECT TOP 50
    CreatedAt,
    UserName,
    Module,
    Action,
    LogType,
    Status,
    Description
FROM AuditLogs
WHERE Status = 'Failure'
AND CreatedAt >= DATEADD(DAY, -1, GETDATE())
ORDER BY CreatedAt DESC;


-- All exceptions with details
SELECT TOP 50
    CreatedAt,
    UserName,
    Module,
    ExceptionType,
    ExceptionMessage,
    Source,
    RequestPath,
    IPAddress
FROM AuditLogs
WHERE LogType = 'Exception'
AND CreatedAt >= DATEADD(DAY, -1, GETDATE())
ORDER BY CreatedAt DESC;


-- Most common exceptions
SELECT TOP 20
    ExceptionType,
    COUNT(*) AS Occurrences,
    COUNT(DISTINCT UserName) AS AffectedUsers,
    MIN(CreatedAt) AS FirstOccurrence,
    MAX(CreatedAt) AS LastOccurrence
FROM AuditLogs
WHERE LogType = 'Exception'
AND CreatedAt >= DATEADD(DAY, -7, GETDATE())  -- Last 7 days
GROUP BY ExceptionType
ORDER BY Occurrences DESC;


-- Exception stack traces (for debugging)
SELECT
    CreatedAt,
    UserName,
    ExceptionType,
    ExceptionMessage,
    StackTrace
FROM AuditLogs
WHERE LogType = 'Exception'
AND CreatedAt >= DATEADD(DAY, -1, GETDATE())
ORDER BY CreatedAt DESC;


-- ============================================================================
-- 5. DATABASE OPERATION TRACKING
-- ============================================================================

-- All CREATE operations today
SELECT TOP 100
    CreatedAt,
    UserName,
    TableName,
    Action,
    RecordId,
    Status,
    NewValues
FROM AuditLogs
WHERE LogType = 'DataModification'
AND Action = 'CREATE'
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- All UPDATE operations today
SELECT TOP 100
    CreatedAt,
    UserName,
    TableName,
    RecordId,
    OldValues,
    NewValues,
    Status
FROM AuditLogs
WHERE LogType = 'DataModification'
AND Action = 'UPDATE'
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- All DELETE operations today
SELECT TOP 100
    CreatedAt,
    UserName,
    TableName,
    RecordId,
    OldValues,
    Status
FROM AuditLogs
WHERE LogType = 'DataModification'
AND Action = 'DELETE'
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- Changes to a specific table (e.g., 'Affiliations')
SELECT
    CreatedAt,
    UserName,
    Action,
    RecordId,
    OldValues,
    NewValues,
    Status
FROM AuditLogs
WHERE TableName = 'Affiliations'  -- Change table name
AND LogType = 'DataModification'
ORDER BY CreatedAt DESC;


-- Most modified records
SELECT TOP 20
    TableName,
    RecordId,
    COUNT(*) AS ModificationCount,
    COUNT(DISTINCT UserName) AS ModifiedByUsers,
    MAX(CreatedAt) AS LastModified
FROM AuditLogs
WHERE LogType = 'DataModification'
AND CreatedAt >= DATEADD(DAY, -7, GETDATE())
GROUP BY TableName, RecordId
ORDER BY ModificationCount DESC;


-- ============================================================================
-- 6. PAYMENT TRACKING
-- ============================================================================

-- All payment transactions today
SELECT TOP 100
    CreatedAt,
    UserName,
    EventName,
    Description,
    OldValues AS PaymentDetails,
    Status
FROM AuditLogs
WHERE EventType = 'Payment'  -- Note: adjust column name if different
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- Payment summary by status
SELECT
    Status,
    COUNT(*) AS TransactionCount
FROM AuditLogs
WHERE LogType = 'Event'
AND EventName LIKE '%Payment%'
AND CreatedAt >= DATEADD(DAY, -7, GETDATE())
GROUP BY Status;


-- ============================================================================
-- 7. HTTP REQUEST ANALYSIS
-- ============================================================================

-- Most accessed endpoints
SELECT TOP 50
    RequestPath,
    RequestMethod,
    COUNT(*) AS RequestCount,
    COUNT(DISTINCT UserName) AS UniqueUsers,
    COUNT(CASE WHEN Status = 'Success' THEN 1 END) AS SuccessCount,
    COUNT(CASE WHEN Status = 'Failure' THEN 1 END) AS FailureCount
FROM AuditLogs
WHERE LogType = 'Event'
AND Module = 'HTTP'
AND CreatedAt >= DATEADD(DAY, -1, GETDATE())
GROUP BY RequestPath, RequestMethod
ORDER BY RequestCount DESC;


-- Slow requests (assuming duration is in data)
SELECT TOP 50
    CreatedAt,
    UserName,
    RequestMethod,
    RequestPath,
    Description
FROM AuditLogs
WHERE LogType = 'Event'
AND Module = 'HTTP'
AND CreatedAt >= DATEADD(DAY, -1, GETDATE())
ORDER BY CreatedAt DESC;


-- ============================================================================
-- 8. APPROVAL/REJECTION TRACKING
-- ============================================================================

-- All approvals/rejections
SELECT TOP 100
    CreatedAt,
    UserName,
    Action,
    Description,
    OldValues AS Details
FROM AuditLogs
WHERE EventType = 'Approval'  -- Note: adjust column name if different
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- ============================================================================
-- 9. FILE OPERATION TRACKING
-- ============================================================================

-- All file operations (uploads, downloads)
SELECT TOP 100
    CreatedAt,
    UserName,
    Action,
    Description,
    OldValues AS FileDetails
FROM AuditLogs
WHERE EventType = 'File'  -- Note: adjust column name if different
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- ============================================================================
-- 10. REPORT GENERATION TRACKING
-- ============================================================================

-- All generated reports
SELECT TOP 100
    CreatedAt,
    UserName,
    EventName,
    Description,
    OldValues AS ReportDetails
FROM AuditLogs
WHERE EventType = 'Report'  -- Note: adjust column name if different
AND CreatedAt >= CAST(GETDATE() AS DATE)
ORDER BY CreatedAt DESC;


-- ============================================================================
-- 11. IP ADDRESS & LOCATION ANALYSIS
-- ============================================================================

-- Login attempts from different IPs
SELECT
    UserName,
    IPAddress,
    COUNT(*) AS LoginCount,
    MIN(CreatedAt) AS FirstLogin,
    MAX(CreatedAt) AS LastLogin
FROM AuditLogs
WHERE LogType = 'Session'
AND Action = 'Login'
AND CreatedAt >= DATEADD(DAY, -7, GETDATE())
GROUP BY UserName, IPAddress
ORDER BY LoginCount DESC;


-- Suspicious activity: Multiple users from same IP
SELECT
    IPAddress,
    COUNT(DISTINCT UserName) AS UserCount,
    COUNT(*) AS ActionCount
FROM AuditLogs
WHERE CreatedAt >= DATEADD(DAY, -7, GETDATE())
AND IPAddress IS NOT NULL
GROUP BY IPAddress
HAVING COUNT(DISTINCT UserName) > 5  -- More than 5 users from same IP
ORDER BY ActionCount DESC;


-- ============================================================================
-- 12. DAILY STATISTICS
-- ============================================================================

-- Daily activity summary
SELECT
    CAST(CreatedAt AS DATE) AS [Date],
    COUNT(*) AS TotalActions,
    COUNT(DISTINCT UserName) AS UniqueUsers,
    COUNT(DISTINCT IPAddress) AS UniqueIPs,
    COUNT(CASE WHEN Status = 'Success' THEN 1 END) AS SuccessCount,
    COUNT(CASE WHEN Status = 'Failure' THEN 1 END) AS FailureCount,
    COUNT(CASE WHEN LogType = 'Exception' THEN 1 END) AS ExceptionCount,
    COUNT(CASE WHEN LogType = 'Session' AND Action = 'Login' THEN 1 END) AS LoginCount
FROM AuditLogs
WHERE CreatedAt >= DATEADD(DAY, -30, GETDATE())
GROUP BY CAST(CreatedAt AS DATE)
ORDER BY [Date] DESC;


-- ============================================================================
-- 13. AUDIT LOG MAINTENANCE
-- ============================================================================

-- Count total logs
SELECT 
    COUNT(*) AS TotalLogs,
    MIN(CreatedAt) AS OldestLog,
    MAX(CreatedAt) AS NewestLog,
    DATEDIFF(DAY, MIN(CreatedAt), MAX(CreatedAt)) AS DaysOfLogs
FROM AuditLogs;


-- Database size
SELECT
    SUM(ps.reserved_page_count) * 8 / 1024 AS ReservedMB,
    SUM(ps.used_page_count) * 8 / 1024 AS UsedMB,
    (SUM(ps.reserved_page_count) - SUM(ps.used_page_count)) * 8 / 1024 AS UnusedMB
FROM sys.dm_db_partition_stats AS ps
WHERE ps.object_id = OBJECT_ID('AuditLogs');


-- Logs older than 30 days
SELECT COUNT(*) AS OldLogsCount
FROM AuditLogs
WHERE CreatedAt < DATEADD(DAY, -30, GETDATE());


-- Archive old logs (create new table first, then run this)
-- SELECT * INTO AuditLogs_Archive_2025 FROM AuditLogs
-- WHERE CreatedAt < DATEADD(DAY, -30, GETDATE());


-- ============================================================================
-- 14. COMPLIANCE REPORTING
-- ============================================================================

-- User access timeline (when they accessed, from where)
DECLARE @UserName NVARCHAR(MAX) = 'college@university.edu'

SELECT
    CreatedAt,
    Action,
    Module,
    RequestPath,
    IPAddress,
    UserAgent,
    Status
FROM AuditLogs
WHERE UserName = @UserName
ORDER BY CreatedAt DESC;


-- Complete audit trail for a specific record
DECLARE @TableName NVARCHAR(MAX) = 'Affiliations'
DECLARE @RecordId NVARCHAR(MAX) = '123'

SELECT
    AuditLogId,
    CreatedAt,
    UserName,
    Action,
    OldValues,
    NewValues,
    Status,
    Description
FROM AuditLogs
WHERE TableName = @TableName
AND RecordId = @RecordId
ORDER BY CreatedAt ASC;


-- ============================================================================
-- 15. PERFORMANCE INDEXES
-- ============================================================================

-- Check if indexes are being used
SELECT
    name AS IndexName,
    user_updates,
    user_seeks,
    user_scans,
    user_lookups,
    (user_seeks + user_scans + user_lookups) AS TotalReads
FROM sys.dm_db_index_usage_stats
WHERE database_id = DB_ID()
AND object_id = OBJECT_ID('AuditLogs')
ORDER BY TotalReads DESC;
