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