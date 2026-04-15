USE [Admission_Affiliation]
GO

/****** Object:  Table [dbo].[CA_Progress]    Script Date: 13-04-2026 16:13:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CA_Progress](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CollegeCode] [nvarchar](50) NULL,
	[CourseLevel] [nvarchar](10) NULL,
	[StepKey] [nvarchar](100) NULL,
	[IsCompleted] [bit] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_CA] UNIQUE NONCLUSTERED 
(
	[CollegeCode] ASC,
	[CourseLevel] ASC,
	[StepKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CA_Progress] ADD  DEFAULT ((1)) FOR [IsCompleted]
GO

ALTER TABLE [dbo].[CA_Progress] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO

