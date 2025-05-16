CREATE PROCEDURE sp_get_user
	@user_id INT
AS
	SELECT 
		id AS Id,
		name AS Name
	FROM
		users
	WHERE
		id = @user_id
GO