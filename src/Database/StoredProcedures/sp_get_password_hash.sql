CREATE PROCEDURE sp_get_password_hash
	@user_identifier INT
AS
	SELECT
		password_hash
	FROM
		users
	WHERE
		identifier = @user_identifier
GO