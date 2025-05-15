IF EXISTS (SELECT TOP(1) 1 FROM SYSOBJECTS WHERE id = OBJECT_ID('sp_get_password_hash') AND OBJECTPROPERTY(id, 'IsProcedure') = 1)
	DROP PROCEDURE sp_get_password_hash
GO

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