CREATE PROCEDURE sp_put_message
	@timestamp DATETIME,
	@sender_identifier INT,
	@receiver_identifier INT,
	@content VARCHAR(1024)
AS
	INSERT INTO
		messages(
			timestamp,
			sender_identifier,
			receiver_identifier,
			content
		)
	VALUES(
		@timestamp,
		@sender_identifier,
		@receiver_identifier,
		@content
	)
GO