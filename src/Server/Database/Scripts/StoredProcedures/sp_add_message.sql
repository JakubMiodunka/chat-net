IF EXISTS(SELECT TOP(1) 1 FROM SYSOBJECTS WHERE id = OBJECT_ID('sp_add_message') AND OBJECTPROPERTY(id, 'IsProcedure') = 1)
	DROP PROCEDURE sp_add_message
GO

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