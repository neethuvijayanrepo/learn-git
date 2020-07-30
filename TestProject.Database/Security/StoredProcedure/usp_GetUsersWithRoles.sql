CREATE PROCEDURE [Security].[usp_GetUsersWithRoles]
	 @RoleName varchar(30) 
AS
BEGIN
      --SET NOCOUNT ON added to prevent extra result sets from
	  --interfering with SELECT statements.
	  SET NOCOUNT ON;
	   
	  --- CONVERTING ID STRING TO COLLECTION  
	  DECLARE @RoleNames AS TABLE([RoleName] VARCHAR(30));  
	  INSERT INTO @RoleNames  
	  SELECT CAST([value] AS varchar) FROM STRING_SPLIT(@RoleName,',');  
	
	  SELECT 
			rtrim(ltrim(concat(Users.[FirstName]+' ',Users.[LastName]))) As UserName, Users.UserID
		FROM 
			[Security].[User] Users
			INNER JOIN [Security].[UserRole] UserRole 
			           ON Users.[UserID]=UserRole.[UserID] 
					   AND UserRole.[Status]NOT IN (-1,-2) 
			INNER JOIN [Security].[Role] Roles  
						ON Roles.[RoleID] = UserRole.[RoleID] 
						AND Roles.[Status] NOT IN (-1,-2)  
			INNER JOIN @RoleNames RI 
						ON RI.[RoleName] = Roles.[RoleName] 
			WHERE Users.[Status] NOT IN (-1,-2) AND  UserName != 'System'
	   ORDER BY UserName ASC 
	
END