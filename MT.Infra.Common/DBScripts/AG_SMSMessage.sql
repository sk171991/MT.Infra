USE [AG_DEV09_CON]
GO

/****** Object:  Table [dbo].[AG_SMSMessage]    Script Date: 7/24/2017 6:50:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AG_SMSMessage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AccountSid] [varchar](100) NOT NULL,
	[ApiVersion] [date] NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[ErrorCode] [int] NULL,
	[ErrorMessage] [varchar](100) NULL,
	[FromNumber] [varchar](100) NOT NULL,
	[ToNumber] [varchar](100) NOT NULL,
	[Status] [varchar](50) NOT NULL,
	[Uri] [nvarchar](max) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[DateUpdated] [datetime] NOT NULL,
	[MessageInput] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

