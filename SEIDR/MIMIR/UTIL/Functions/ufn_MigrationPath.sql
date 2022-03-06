
CREATE FUNCTION UTIL.ufn_MigrationPath(@Path varchar(500))
RETURNS varchar(500)
AS
BEGIN
       DECLARE @env varchar(3) = 'UAT'
       SET @env = SUBSTRING(@@SERVERNAME,7,3)
       DECLARE @fileshareServer varchar(25) = 'ncimtxfls01' -- same for DEV and UAT

       IF UPPER(@env) = 'PRD' 
       BEGIN
              SET @fileshareServer = 'ncimtxfls02.nci.local'
       END

       IF @Path is null
              RETURN NULL
       RETURN        
              replace(
              replace(
              replace(
              replace(
              replace(
              replace(
              replace(
              replace(
              replace(
              replace(
              replace(
              replace(
              replace(
        replace(
                     @Path,'\is\DATA\','\')
                     ,'\_SourceFiles\',REPLACE('\sourcefiles_##\','##',@env))
                     ,'\data\sourcefiles','\sourcefiles')
                     ,'sdsrv031.cymetrix.com',@fileshareServer)
                     ,'sdsrv015.cymetrix.com\andromedafiles\',@fileshareserver + REPLACE('\andromeda_##\','##',@env))
                     ,'\FTP\','\SFTPMTX\')
                     ,'\andromedafiles\',REPLACE('\andromeda_##\','##',@env))
                     ,'\Andromeda_Exports_Phase2\',REPLACE('\exports_##\','##',@env))
                     ,'\\sdsrv015.cymetrix.com\','\\' + @fileshareserver + '\')
                     ,'\\sdsrv031\','\\' + @fileshareserver + '\')
                     ,'\\sdsrv015\','\\' + @fileshareserver + '\')
                     ,'\\LBDC03', 'CUBS_SOURCE')
                     , '\\irv2cubs01', 'CUBS')
                     ,'\andromeda_exports','\exports_' + @env) 
END