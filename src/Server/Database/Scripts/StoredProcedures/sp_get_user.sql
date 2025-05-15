IF EXISTS (SELECT TOP(1) 1 FROM SYSOBJECTS WHERE id = OBJECT_ID('sp_get_user') AND OBJECTPROPERTY(id, 'IsProcedure') = 1)
	DROP PROCEDURE sp_get_user
GO

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