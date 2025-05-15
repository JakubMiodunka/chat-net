IF EXISTS(SELECT TOP(1) 1 FROM SYSOBJECTS WHERE id = OBJECT_ID('sp_add_user') AND OBJECTPROPERTY(id, 'IsProcedure') = 1)
	DROP PROCEDURE sp_add_user
GO

CREATE PROCEDURE sp_add_user
	@user_name VARCHAR(32),
	@password_hash VARCHAR(32)
AS
	INSERT INTO
		users(
			name,
			password_hash
		)
	VALUES(
		@user_name,
		@password_hash
	)
GO