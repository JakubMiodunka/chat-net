CREATE PROCEDURE sp_get_password_hash
	@user_id INT
AS
	SELECT
		password_hash
	FROM
		users
	WHERE
		id = @user_id
GO