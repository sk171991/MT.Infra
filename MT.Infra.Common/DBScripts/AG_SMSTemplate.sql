USE [AG_DEV09_CON]
GO

/****** Object:  Table [dbo].[AG_SMSTemplate]    Script Date: 7/24/2017 6:51:19 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AG_SMSTemplate](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Module] [varchar](100) NOT NULL,
	[Body] [varchar](500) NULL
) ON [PRIMARY]

GO

