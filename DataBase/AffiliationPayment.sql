USE [Admission_Affiliation]
GO

-- ===================== CREATE TABLE =====================
CREATE TABLE [dbo].[Affiliation_Payment]
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CollegeCode NVARCHAR(100) NOT NULL,
    FacultyCode INT NOT NULL,

    AffiliationTypeId INT NOT NULL,

    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),

    Amount DECIMAL(10,2) NOT NULL,

    TransactionReferenceNo VARCHAR(100) NOT NULL,

    SupportingDocument NVARCHAR(255) NULL,

    RegistrationNumber VARCHAR(100) NULL,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
)
GO

-- ===================== FOREIGN KEYS =====================

-- College FK
ALTER TABLE [dbo].[Affiliation_Payment]
ADD CONSTRAINT FK_Payment_College
FOREIGN KEY (CollegeCode)
REFERENCES [dbo].[Affiliation_College_Master](CollegeCode)
GO

-- Faculty FK
ALTER TABLE [dbo].[Affiliation_Payment]
ADD CONSTRAINT FK_Payment_Faculty
FOREIGN KEY (FacultyCode)
REFERENCES [dbo].[Faculty](FacultyId)
GO

-- Affiliation Type FK
ALTER TABLE [dbo].[Affiliation_Payment]
ADD CONSTRAINT FK_Payment_AffiliationType
FOREIGN KEY (AffiliationTypeId)
REFERENCES [dbo].[TypeOfAffiliation](TypeId)
GO

-- ===================== UNIQUE CONSTRAINT =====================
ALTER TABLE [dbo].[Affiliation_Payment]
ADD CONSTRAINT UQ_Payment_TransactionReference
UNIQUE (TransactionReferenceNo)
GO

-- ===================== INDEX =====================
CREATE INDEX IX_Payment_College_Faculty
ON [dbo].[Affiliation_Payment] (CollegeCode, FacultyCode)
GO