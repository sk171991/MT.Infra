GO
/****** Object:  StoredProcedure [dbo].[AG_SP_GetPolicyInfo_NSD]    Script Date: 9/22/2017 12:47:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Kushal RV Bajaj
-- Create date: 24th May 2017
-- Description:	Based on Policy Number, Phone Number, VIN searches the policy and returns the information.
			  --This is written solely for the purpose of NSD Integration.
-- =============================================
Create PROCEDURE [dbo].[AG_SP_GetPolicyInfo_NSD]
	-- Add the parameters for the stored procedure here
		@policyNumber varchar(20),
		@phoneNumber varchar(15),
		@vin varchar(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @xmlData xml, 
	@effectiveDate date,
	@expirationDate date,
	@producerCode int,
	@product int, 
	@planCode varchar, 
	@firstName varchar(20),
	@lastName varchar(20),
	@address varchar(MAX),
	@address1 varchar(MAX),
	@address2 varchar(MAX),
	@city varchar(30),
	@state varchar(3),
	@zip varchar(10),
	@email varchar(30),
	@year int,
	@make varchar(10),
	@model varchar(20),
	@riskCount int,
	@counter int,
	@seachByVin bit = 0,
	@searchByPhoneNumber bit = 0,
	@historyId int;

	--Declare a temporary table
	DECLARE @policyData table(
	PolicyNumber varchar(20),
	Message varchar(6),
	EffectiveDate date,
	ExpirationDate date,
	ProducerCode int,
	Product int, 
	PlanCode varchar, 
	FirstName varchar(20),
	LastName varchar(20),
	Address varchar(MAX),
	City varchar(30),
	State varchar(3),
	Zip varchar(10),
	Phone varchar(15),
	Email varchar(30),
	Vin varchar(MAX),
	Year int,
	Make varchar(10),
	Model varchar(20)
	);
	DECLARE @HistoryData table(
	HistoryId int
	);
	--End of table declaration

	IF(@vin='' and @phoneNumber ='' and @policyNumber='' )
			BEGIN
				select 'NSD-2002' as Message,'' as PolicyNumber,'' as EffectiveDate,'' as ExpirationDate,null as ProducerCode, null as Product, '' as PlanCode,
				'' as FirstName,'' as LastName, '' as Address,'' as City, '' as State,'' as Zip,'' as Phone, '' as Email, '' as VIN, Null as Year, '' as Make, '' as Model
				return;
			END

	--Search via VIN. i.e if Phone Number and Policy Number are not provided. 
	if(@phoneNumber ='' and @policyNumber='')
		BEGIN			
			Declare @count int;		
		
			set @seachByVin = 1;

			Insert into @HistoryData 
			select HistoryId from 
			(	select History.XMLData,History.HistoryID, History.PolicyNumber,History.ExpirationDate,History.State,
				History.TransactionStatus,History.TransactionEffectiveDate, Quote.LOB,History.PolicyStatus			 
				from History
				join Quote on History.QuoteID = QUote.QuoteID
				where Quote.LOB = 'PersonalAuto'
				and History.TransactionEffectiveDate <= GETDATE()
				and History.ExpirationDate >= GETDATE()
				and cast(History.XMLData as XML).value('(/session/data/policy/AllVIN)[1]','varchar(max)') like '%'+@vin+'%'					
					
			) as LatestTransaction order by LatestTransaction.TransactionEffectiveDate desc, LatestTransaction.HistoryId desc

			
			set @count = (select count(Distinct HistoryId) from @HistoryData);
			
			if (@count=0) --If no record is found return NSD-2002 as Message
			BEGIN
				select 'NSD-2002' as Message,'' as PolicyNumber,'' as EffectiveDate,'' as ExpirationDate,null as ProducerCode, null as Product, '' as PlanCode,
				'' as FirstName,'' as LastName, '' as Address,'' as City, '' as State,'' as Zip,'' as Phone, '' as Email, '' as VIN, Null as Year, '' as Make, '' as Model
				return;
			END
			if(@count>1)
			BEGIN
				select 'NSD-2001' as Message,'' as PolicyNumber,'' as EffectiveDate,'' as ExpirationDate,null as ProducerCode, null as Product, '' as PlanCode,
				'' as FirstName,'' as LastName, '' as Address,'' as City, '' as State,'' as Zip,'' as Phone, '' as Email, '' as VIN, Null as Year, '' as Make, '' as Model
				return;
			END
			
			set @historyId = (select historyId from @HistoryData);
			--set @XMLData = (select top 1 XMLData from History where HistoryId = @historyId);
			--set @phoneNumber = @xmlData.value('(//session/data/account/PrimaryPhone)[1]','varchar(15)');
		END

	else if(@policyNumber='')
	BEGIN
		Insert into @HistoryData 
			select HistoryID from 
			(	select History.XMLData,History.HistoryID, History.PolicyNumber,History.ExpirationDate,History.State,
				History.TransactionStatus,History.TransactionEffectiveDate, Quote.LOB,History.PolicyStatus			 
				from History
				join Quote on History.QuoteID = QUote.QuoteID
				where Quote.LOB = 'PersonalAuto'
				and History.TransactionEffectiveDate <= GETDATE()
				and History.ExpirationDate >= GETDATE()
				and(
				cast(History.XMLData as XML).value('(//session/data/account/PrimaryPhone)[1]','varchar(11)') like @phoneNumber
				or
				cast(History.XMLData as XML).value('(//session/data/account/MobileNumber)[1]','varchar(11)') like @phoneNumber
				)					
					
			) as LatestTransaction order by LatestTransaction.TransactionEffectiveDate desc, LatestTransaction.HistoryId desc

			
			set @count = (select count(Distinct HistoryId) from @HistoryData);
			
			if (@count=0) --If no record is found return NSD-2002 as Message
			BEGIN
				select 'NSD-2002' as Message,'' as PolicyNumber,'' as EffectiveDate,'' as ExpirationDate,null as ProducerCode, null as Product, '' as PlanCode,
				'' as FirstName,'' as LastName, '' as Address,'' as City, '' as State,'' as Zip,'' as Phone, '' as Email, '' as VIN, Null as Year, '' as Make, '' as Model
				return;
			END
			if(@count>1) --If Multiple Policies are found
			BEGIN
				select 'NSD-2001' as Message,'' as PolicyNumber,'' as EffectiveDate,'' as ExpirationDate,null as ProducerCode, null as Product, '' as PlanCode,
				'' as FirstName,'' as LastName, '' as Address,'' as City, '' as State,'' as Zip,'' as Phone, '' as Email, '' as VIN, Null as Year, '' as Make, '' as Model
				return;
			END
			
			set @historyId = (Select HistoryId from @HistoryData);
			set @searchByPhoneNumber = 1;
	END

	set @xmlData = 
	(select XMLData from 
		(	select top 1 History.XMlData,History.PolicyStatus,History.PolicyNumber,History.TransactionEffectiveDate,History.ExpirationDate,History.State,History.TransactionStatus from History
			join Quote 
			on History.QuoteID=Quote.QuoteID where Quote.LOB = 'PersonalAuto'
			and History.TransactionEffectiveDate <= GETDATE()
			and History.ExpirationDate >=GetDate()
			and (
				(@policyNumber!='' and History.PolicyNumber=@policyNumber) or (History.HistoryID = @historyId)
			)
			 order by History.TransactionEffectiveDate desc,History.HistoryId desc
		)as LatestTransaction where 
			cast(LatestTransaction.XMLData as xml).value('(/session/data/policyAdmin/transactions/transaction[last()]/Type)[1]','varchar(20)') Not IN ('cancel')
	)

	if(@xmlData IS NOT Null)
	BEGIN
		set @policyNumber = @xmlData.value('(/session/data/policy/PolicyNumber)[1]','varchar(20)');
		set @effectiveDate = @xmlData.value('(/session/data/policy/EffectiveDate)[1]','date');
		set @expirationDate = @xmlData.value('(/session/data/policy/ExpirationDate)[1]','date');
		set @producerCode = @xmlData.value('(/session/data/policy/ProducerCode)[1]','int');
		set @product =	@xmlData.value('(/session/data/policy/Product)[1]','int');;
		set @planCode = @xmlData.value('(/session/data/policy/Plan)[1]','varchar');;
		set @firstName = @xmlData.value('(/session/data/account/person/FirstName)[1]','varchar(20)');
		set @lastName = @xmlData.value('(/session/data/account/person/LastName)[1]','varchar(20)');
		set @address1 = @xmlData.value('(/session/data/account/address/Address1)[1]','varchar(MAX)');
		set @address2 = @xmlData.value('(/session/data/account/address/Address2)[1]','varchar(MAX)');
		set @city = @xmlData.value('(/session/data/account/address/City)[1]','varchar(30)');
		set @state = @xmlData.value('(/session/data/account/address/State)[1]','varchar(3)');
		set @zip = @xmlData.value('(/session/data/account/address/ZipCode)[1]','varchar(10)');
		set @email = @xmlData.value('(/session/data/account/Email)[1]','varchar(30)');
		
		if(@searchByPhoneNumber = 0)
		BEGIN
			if(@xmlData.value('(//session/data/account/PrimaryPhone)[1]','varchar(15)') is NULL)
			BEGIN
			set @phoneNumber = @xmlData.value('(//session/data/account/MobileNumber)[1]','varchar(15)');
			END
			else
			BEGIN
			set @phoneNumber = @xmlData.value('(//session/data/account/PrimaryPhone)[1]','varchar(15)');
			END
		END

		if(@address2 !='')
		BEGIN
			set @address = @address1 + ', ' +@address2;
		END
		else
		BEGIN
			set @address = @address1
		END
		set @riskCount = @xmlData.value('count(/session/data/policy/line[Type="PersonalAuto"]/risk)','int');
		set @counter=1;
	
		delete @policyData		

		while(@counter<=@riskCount)
		BEGIN --Incase of VIN search insert only one vehicle details whose VIN matches the Input VIN else insert all the associated vehicles with the policy
			if((@seachByVin=0 and (select @xmlData.value('(/session/data/policy/line[Type="PersonalAuto"]/risk[position()=sql:variable("@counter")]/vehicle/@deleted)[1]','varchar(MAX)')) is NULL) or (@seachByVin=1 and @vin = @xmlData.value('(/session/data/policy/line[Type="PersonalAuto"]/risk[position()=sql:variable("@counter")]/vehicle/VIN)[1]','varchar(MAX)')))
			BEGIN
				set @year = @xmlData.value('(/session/data/policy/line[Type="PersonalAuto"]/risk[position()=sql:variable("@counter")]/vehicle/Year)[1]','int');
				set @make = @xmlData.value('(/session/data/policy/line[Type="PersonalAuto"]/risk[position()=sql:variable("@counter")]/vehicle/remotevinverification/VinResponse/MakeName)[1]','varchar(100)');
				set @model = @xmlData.value('(/session/data/policy/line[Type="PersonalAuto"]/risk[position()=sql:variable("@counter")]/vehicle/remotevinverification/VinResponse/ModelDesc)[1]','varchar(100)');
				set @vin = @xmlData.value('(/session/data/policy/line[Type="PersonalAuto"]/risk[position()=sql:variable("@counter")]/vehicle/VIN)[1]','varchar(MAX)');		
			
				Insert into @policyData values(@policyNumber,'',@effectiveDate,@expirationDate,@producerCode,@product,@planCode,@firstName,@lastName,@address,@city,@state,@zip,@phoneNumber,@email,@vin,@year,@make,@model);
			END
			set @counter+=1;
		END

		select * from @policyData;

	END
	else --If no policy is found return NSD-2002 as Message
	BEGIN
	Select 'NSD-2002' as Message,'' as PolicyNumber,'' as EffectiveDate,'' as ExpirationDate,null as ProducerCode, null as Product, '' as PlanCode,
				'' as FirstName,'' as LastName, '' as Address,'' as City, '' as State,'' as Zip,'' as Phone, '' as Email, '' as VIN, Null as Year, '' as Make, '' as Model
	END
END

