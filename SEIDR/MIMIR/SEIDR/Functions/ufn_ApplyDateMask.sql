 CREATE FUNCTION [SEIDR].[ufn_ApplyDateMask](@Path varchar(500), @ProcessingDate date)
 RETURNS varchar(500)
 AS
 BEGIN
	IF @Path NOT LIKE '%<%'
		RETURN @Path
	DECLARE @Path2 varchar(500)
	DECLARE @d_Offset int = 0, @m_Offset int = 0, @y_Offset int = 0
	DECLARE @V int, @W int, @X int	
	SET @V = PATINDEX('%<[-+0123456789]%YYYY[^>]%M[^>]%D>%', @Path)
	IF @V > 0
	BEGIN -- File Registration date mask (backwards)
		SELECT @W = CHARINDEX('M', @Path, @V)
		SELECT @X = CHARINDEX('D', @Path, @W)
		SELECT @y_Offset = -1 * CONVERT(int,  SUBSTRING(@Path, @V + 1, CHARINDEX('Y', @Path, @V) - @V -1)),
				@m_Offset = -1 *  CONVERT(int, SUBSTRING(@Path, CHARINDEX('Y', @Path, @V) + 4, @W - @V - 7)),
				@d_Offset = -1 *  CONVERT(int, SUBSTRING(@Path, CHARINDEX('M', @Path, @W) + 2, @X - @W - 2))		
		SELECT @Path = SUBSTRING(@Path, 0, @V) + SUBSTRING(@Path, CHARINDEX('>', @Path, @X) + 1, LEN(@Path))
	END
	ELSE
	BEGIN
		SELECT @V = PATINDEX('%<[-+0123456789]%Y>%', @Path)
		IF @V > 0
		BEGIN
			SELECT @y_Offset = CONVERT(int, SUBSTRING(@Path, @V+ 1, CHARINDEX('Y', @Path, @V) - @V - 1))			
			SELECT @Path = SUBSTRING(@Path, 0, @V) + SUBSTRING(@Path, CHARINDEX('>', @Path, @V) +1, LEN(@Path))
		END
		SELECT @V = PATINDEX('%<[-+0123456789]%M>%', @Path)
		IF @V > 0
		BEGIN
			SELECT @m_Offset = CONVERT(int, SUBSTRING(@Path, @V+1, CHARINDEX('M', @path, @V) - @V - 1))
			SELECT @Path = SUBSTRING(@Path, 0, @V) + SUBSTRING(@Path, CHARINDEX('>', @Path, @V) +1, LEN(@Path))
		END
		SELECT @V = PATINDEX('%<[-+0123456789]%D>%', @Path)
		IF @V > 0
		BEGIN
			SELECT @d_Offset = CONVERT(int, SUBSTRING(@Path, @V+1, CHARINDEX('D', @Path, @V) - @V - 1))
			SELECT @Path = SUBSTRING(@Path, 0, @V) + SUBSTRING(@Path, CHARINDEX('>', @Path, @V) +1, LEN(@Path))
		END
	END
	SELECT @ProcessingDate = DATEADD(year, @y_Offset,
								DATEADD(month, @m_Offset,
									DATEADD(day, @d_Offset, @ProcessingDate)))


	DECLARE @YYYY varchar(4) = CONVERT(varchar, DATEPART(year, @ProcessingDate))
	DECLARE @YY varchar(2) = RIGHT(@YYYY, 2)
	DECLARE @CC varchar(2) = LEFT(@YYYY, 2)
	DECLARE @M varchar(2) = CONVERT(varchar, DATEPART(month, @ProcessingDate))
	DECLARE @MM varchar(2) = RIGHT('0' + @M, 2)
	DECLARE @D varchar(2) = CONVERT(varchar, DATEPART(day, @ProcessingDate))
	DECLARE @DD varchar(2) = RIGHT('0' + @D, 2)
	SET @Path2 = REPLACE(REPLACE(REPLACE(REPLACE(
					REPLACE(REPLACE(
						REPLACE(REPLACE(@Path, '<D>', @D), '<DD>', @DD)
						, '<M>', @M), '<MM>', @MM)
					, '<YY>', @YY), '<YYYY>', @YYYY), '<CCYY>', @YYYY), '<CC>', @CC)
	RETURN @Path2
 END