CREATE PROCEDURE sp_get_messages_sent_by
	@sender_identifier INT,
	@start_timestamp DATETIME,
	@end_timestamp DATETIME
AS
	SELECT 
		identifier AS id,
		timestamp AS Timestamp,
		sender_identifier AS SenderIdentifier,
		receiver_identifier AS ReceiverIdentifier,
		content AS Content
	FROM
		messages
	WHERE
		@sender_identifier=@sender_identifier AND
		messages.timestamp >= @start_timestamp AND
		messages.timestamp <= @end_timestamp
GO
