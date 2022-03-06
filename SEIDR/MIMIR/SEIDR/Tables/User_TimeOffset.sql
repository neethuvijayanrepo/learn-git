CREATE TABLE [SEIDR].[User_TimeOffset]
(
	[UserName] VARCHAR(128) NOT NULL PRIMARY KEY default(SUSER_NAME()), 
    [TimeOffset] TIME NOT NULL,
	[CurrentDate] as (GETDATE() + CONVERT(datetime, TimeOffset))
)
