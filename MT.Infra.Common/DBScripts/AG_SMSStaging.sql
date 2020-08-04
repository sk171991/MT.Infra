USE [AG_DEV09_CON]
GO

/****** Object:  Table [dbo].[AG_SMSStaging]    Script Date: 7/24/2017 6:50:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AG_SMSStaging](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyNumber] [varchar](30) NULL,
	[PolicyStatus] [varchar](20) NULL,
	[EffectiveDate] [date] NULL,
	[ReceiverNumber] [varchar](13) NULL,
	[SMSStatus] [varchar](10) NULL,
	[CreatedBy] [varchar](20) NULL,
	[CreatedOn] [datetime] NULL,
	[UpdatedBy] [varchar](20) NULL,
	[UpdatedOn] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

