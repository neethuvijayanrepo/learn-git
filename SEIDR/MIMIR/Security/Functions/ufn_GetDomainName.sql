﻿CREATE FUNCTION SECURITY.ufn_GetDomainName(@value varchar(500))
RETURNS VARCHAR(130)
AS
BEGIN
	IF @Value LIKE '%\%'
		RETURN SUBSTRING(@Value, 1, CHARINDEX('\', @Value) -1)
	IF @value LIKE '%@%'
		RETURN SUBSTRING(@Value, CHARINDEX('@', @Value) + 1, LEN(@Value))
	RETURN DEFAULT_DOMAIN()
END