CREATE PROCEDURE sp_put_user
	@name VARCHAR(32),
	@password_hash VARCHAR(32)
AS
	INSERT INTO
		users(
			name,
			password_hash
		)
	VALUES(
		@name,
		@password_hash
	)
GO