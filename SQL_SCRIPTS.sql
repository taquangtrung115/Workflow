-- =============================================
-- SQL Scripts for Workflow Database
-- =============================================

-- Drop existing tables if needed (for clean setup)
/*
DROP TABLE IF EXISTS ApprovalRecords;
DROP TABLE IF EXISTS WorkflowInstances;
DROP TABLE IF EXISTS WorkflowLevels;
DROP TABLE IF EXISTS WorkflowTemplates;
DROP TABLE IF EXISTS Documents;
DROP TABLE IF EXISTS UserFileTypePermissions;
DROP TABLE IF EXISTS FileTypes;
*/

-- =============================================
-- Create Tables
-- =============================================

-- FileTypes table
CREATE TABLE FileTypes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Mime NVARCHAR(200) NOT NULL,
    ExtensionsJson NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_FileTypes_Mime ON FileTypes(Mime);
CREATE INDEX IX_FileTypes_Name ON FileTypes(Name);

-- UserFileTypePermissions table
CREATE TABLE UserFileTypePermissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    FileTypeId UNIQUEIDENTIFIER NOT NULL,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_UserFileTypePermissions_FileTypes FOREIGN KEY (FileTypeId) REFERENCES FileTypes(Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IX_UserFileTypePermissions_UserId_FileTypeId ON UserFileTypePermissions(UserId, FileTypeId);

-- Documents table
CREATE TABLE Documents (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Filename NVARCHAR(500) NOT NULL,
    MimeType NVARCHAR(200) NULL,
    BlobUrl NVARCHAR(1000) NOT NULL,
    FileSize BIGINT NOT NULL,
    UploadedBy UNIQUEIDENTIFIER NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Documents_UploadedBy ON Documents(UploadedBy);
CREATE INDEX IX_Documents_UploadedAt ON Documents(UploadedAt);

-- WorkflowTemplates table
CREATE TABLE WorkflowTemplates (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE INDEX IX_WorkflowTemplates_IsActive ON WorkflowTemplates(IsActive);
CREATE INDEX IX_WorkflowTemplates_CreatedBy ON WorkflowTemplates(CreatedBy);

-- WorkflowLevels table
CREATE TABLE WorkflowLevels (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    TemplateId UNIQUEIDENTIFIER NOT NULL,
    [Order] INT NOT NULL,
    ApproverType NVARCHAR(50) NOT NULL,
    DepartmentId UNIQUEIDENTIFIER NULL,
    UserIdsJson NVARCHAR(MAX) NULL,
    RequiredApprovals INT NOT NULL DEFAULT 1,
    AllowedFileTypesJson NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_WorkflowLevels_Templates FOREIGN KEY (TemplateId) REFERENCES WorkflowTemplates(Id) ON DELETE CASCADE
);

CREATE INDEX IX_WorkflowLevels_TemplateId_Order ON WorkflowLevels(TemplateId, [Order]);

-- WorkflowInstances table
CREATE TABLE WorkflowInstances (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    TemplateId UNIQUEIDENTIFIER NOT NULL,
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    CurrentLevelOrder INT NOT NULL DEFAULT 1,
    Status NVARCHAR(50) NOT NULL DEFAULT 'InProgress',
    RequestedBy UNIQUEIDENTIFIER NOT NULL,
    RequestedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ClosedAt DATETIME2 NULL,
    CONSTRAINT FK_WorkflowInstances_Templates FOREIGN KEY (TemplateId) REFERENCES WorkflowTemplates(Id),
    CONSTRAINT FK_WorkflowInstances_Documents FOREIGN KEY (DocumentId) REFERENCES Documents(Id)
);

CREATE INDEX IX_WorkflowInstances_Status ON WorkflowInstances(Status);
CREATE INDEX IX_WorkflowInstances_RequestedBy ON WorkflowInstances(RequestedBy);
CREATE INDEX IX_WorkflowInstances_RequestedAt ON WorkflowInstances(RequestedAt);

-- ApprovalRecords table
CREATE TABLE ApprovalRecords (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    InstanceId UNIQUEIDENTIFIER NOT NULL,
    LevelOrder INT NOT NULL,
    ApproverUserId UNIQUEIDENTIFIER NOT NULL,
    Approved BIT NOT NULL,
    Comment NVARCHAR(2000) NULL,
    SignatureBlob NVARCHAR(MAX) NULL,
    SignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ApprovalRecords_Instances FOREIGN KEY (InstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ApprovalRecords_InstanceId_LevelOrder ON ApprovalRecords(InstanceId, LevelOrder);
CREATE INDEX IX_ApprovalRecords_ApproverUserId ON ApprovalRecords(ApproverUserId);

-- =============================================
-- Seed Data
-- =============================================

-- Insert FileTypes
INSERT INTO FileTypes (Id, Name, Mime, ExtensionsJson) VALUES
    ('00000000-0000-0000-0000-0000000000F1', 'PDF', 'application/pdf', '[".pdf"]'),
    ('00000000-0000-0000-0000-0000000000F2', 'Word Document', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', '[".docx"]'),
    ('00000000-0000-0000-0000-0000000000F3', 'Excel Spreadsheet', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', '[".xlsx"]'),
    ('00000000-0000-0000-0000-0000000000F4', 'Image JPEG', 'image/jpeg', '[".jpg",".jpeg"]'),
    ('00000000-0000-0000-0000-0000000000F5', 'Image PNG', 'image/png', '[".png"]');

-- Insert Sample Template
INSERT INTO WorkflowTemplates (Id, Name, Description, CreatedBy)
VALUES ('11111111-1111-1111-1111-111111111111', 'Purchase Approval Workflow', 'Template cho quy trình duyệt hóa đơn mua hàng', '00000000-0000-0000-0000-000000000001');

-- Insert Template Levels
INSERT INTO WorkflowLevels (TemplateId, [Order], ApproverType, DepartmentId, RequiredApprovals, AllowedFileTypesJson)
VALUES ('11111111-1111-1111-1111-111111111111', 1, 'Department', '22222222-2222-2222-2222-222222222222', 1, '["application/pdf",".pdf"]');

INSERT INTO WorkflowLevels (TemplateId, [Order], ApproverType, UserIdsJson, RequiredApprovals, AllowedFileTypesJson)
VALUES ('11111111-1111-1111-1111-111111111111', 2, 'Users', '["33333333-3333-3333-3333-333333333333","44444444-4444-4444-4444-444444444444"]', 2, '["application/pdf",".pdf",".docx"]');

-- =============================================
-- Useful Queries
-- =============================================

-- Lấy tất cả templates với số lượng levels
SELECT 
    t.Id,
    t.Name,
    t.Description,
    t.IsActive,
    COUNT(l.Id) AS LevelCount
FROM WorkflowTemplates t
LEFT JOIN WorkflowLevels l ON t.Id = l.TemplateId
GROUP BY t.Id, t.Name, t.Description, t.IsActive
ORDER BY t.CreatedAt DESC;

-- Lấy workflow instances đang pending
SELECT 
    i.Id AS InstanceId,
    t.Name AS TemplateName,
    d.Filename,
    i.CurrentLevelOrder,
    i.Status,
    i.RequestedAt
FROM WorkflowInstances i
JOIN WorkflowTemplates t ON i.TemplateId = t.Id
JOIN Documents d ON i.DocumentId = d.Id
WHERE i.Status = 'InProgress'
ORDER BY i.RequestedAt DESC;

-- Lấy approval history của một instance
SELECT 
    ar.LevelOrder,
    ar.ApproverUserId,
    ar.Approved,
    ar.Comment,
    ar.SignedAt
FROM ApprovalRecords ar
WHERE ar.InstanceId = 'YOUR_INSTANCE_ID'
ORDER BY ar.LevelOrder, ar.SignedAt;

-- Lấy user permissions
SELECT 
    ufp.UserId,
    ft.Name AS FileTypeName,
    ft.Mime,
    ufp.GrantedAt
FROM UserFileTypePermissions ufp
JOIN FileTypes ft ON ufp.FileTypeId = ft.Id
WHERE ufp.UserId = 'YOUR_USER_ID'
ORDER BY ft.Name;

-- Statistics: Workflow instances by status
SELECT 
    Status,
    COUNT(*) AS Count
FROM WorkflowInstances
GROUP BY Status;

-- Statistics: Average approval time per level
SELECT 
    LevelOrder,
    AVG(DATEDIFF(HOUR, i.RequestedAt, ar.SignedAt)) AS AvgHoursToApprove
FROM ApprovalRecords ar
JOIN WorkflowInstances i ON ar.InstanceId = i.Id
WHERE ar.Approved = 1
GROUP BY LevelOrder
ORDER BY LevelOrder;

-- Find documents waiting for specific user approval
SELECT DISTINCT
    i.Id AS InstanceId,
    d.Filename,
    t.Name AS TemplateName,
    i.CurrentLevelOrder,
    i.RequestedAt
FROM WorkflowInstances i
JOIN WorkflowTemplates t ON i.TemplateId = t.Id
JOIN Documents d ON i.DocumentId = d.Id
JOIN WorkflowLevels l ON l.TemplateId = t.Id AND l.[Order] = i.CurrentLevelOrder
WHERE i.Status = 'InProgress'
  AND (
    (l.ApproverType = 'Users' AND l.UserIdsJson LIKE '%YOUR_USER_ID%')
    OR (l.ApproverType = 'Department' AND l.DepartmentId = 'YOUR_DEPARTMENT_ID')
  )
  AND NOT EXISTS (
    SELECT 1 FROM ApprovalRecords ar 
    WHERE ar.InstanceId = i.Id 
    AND ar.LevelOrder = i.CurrentLevelOrder 
    AND ar.ApproverUserId = 'YOUR_USER_ID'
  )
ORDER BY i.RequestedAt;

-- =============================================
-- Cleanup Queries (for testing)
-- =============================================

-- Delete all approval records
-- DELETE FROM ApprovalRecords;

-- Delete all instances
-- DELETE FROM WorkflowInstances;

-- Delete all documents
-- DELETE FROM Documents;

-- Reset specific instance
-- DELETE FROM ApprovalRecords WHERE InstanceId = 'YOUR_INSTANCE_ID';
-- UPDATE WorkflowInstances SET Status = 'InProgress', CurrentLevelOrder = 1, ClosedAt = NULL WHERE Id = 'YOUR_INSTANCE_ID';

-- =============================================
-- Maintenance Queries
-- =============================================

-- Archive old completed workflows (older than 1 year)
-- Nên move sang bảng archive thay vì delete
/*
SELECT * INTO WorkflowInstances_Archive
FROM WorkflowInstances
WHERE Status IN ('Approved', 'Rejected', 'Cancelled')
  AND ClosedAt < DATEADD(YEAR, -1, GETUTCDATE());

DELETE FROM WorkflowInstances
WHERE Status IN ('Approved', 'Rejected', 'Cancelled')
  AND ClosedAt < DATEADD(YEAR, -1, GETUTCDATE());
*/

-- Update statistics
-- UPDATE STATISTICS WorkflowInstances;
-- UPDATE STATISTICS ApprovalRecords;

-- Check table sizes
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB, 
    SUM(a.used_pages) * 8 AS UsedSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.NAME IN ('FileTypes', 'UserFileTypePermissions', 'Documents', 'WorkflowTemplates', 'WorkflowLevels', 'WorkflowInstances', 'ApprovalRecords')
GROUP BY t.Name, p.Rows
ORDER BY TotalSpaceKB DESC;
