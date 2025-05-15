IF EXISTS (SELECT TOP(1) 1 FROM SYSOBJECTS WHERE id = OBJECT_ID('sp_get_messages_sent_by') AND OBJECTPROPERTY(id, 'IsProcedure') = 1)
	DROP PROCEDURE sp_get_messages_sent_by
GO

CREATE PROCEDURE sp_get_messages_sent_by
	@sender_id INT,
	@start_timestamp DATETIME,
	@end_timestamp DATETIME
AS
	SELECT 
		messages.id AS id,
		messages.timestamp AS Timestamp,
		senders.id AS SenderId,
		receivers.id AS ReceiverId,
		messages.content AS Content
	FROM messages 
		JOIN users senders ON messages.sender_id=senders.id
		JOIN users receivers ON messages.receiver_id=receivers.id
	WHERE
		senders.id=@sender_id AND
		messages.timestamp >= @start_timestamp AND
		messages.timestamp <= @end_timestamp
GO
