CREATE PROCEDURE sp_add_message
	@timestamp DATETIME,
	@sender_id INT,
	@receiver_id INT,
	@content VARCHAR(1024)
AS
	INSERT INTO
		messages(
			timestamp,
			sender_id,
			receiver_id,
			content
		)
	VALUES(
		@timestamp,
		@sender_id,
		@receiver_id,
		@content
	)
GO