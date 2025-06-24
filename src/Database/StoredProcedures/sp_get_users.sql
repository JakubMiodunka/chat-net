CREATE PROCEDURE sp_get_users
	@user_identifier_csv_list varchar(MAX)
AS
	SELECT 
		identifier AS Identifier,
		name AS Name
	FROM
		users
	WHERE
		identifier IN(SELECT value FROM STRING_SPLIT(@user_identifier_csv_list, ','))
GO
