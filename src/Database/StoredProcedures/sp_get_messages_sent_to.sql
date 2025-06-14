﻿CREATE PROCEDURE sp_get_messages_sent_to
	@receiver_id INT,
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
		JOIN users AS senders ON messages.sender_id=senders.id
		JOIN users AS receivers ON messages.receiver_id=receivers.id
	WHERE
		receivers.id=@receiver_id AND
		messages.timestamp >= @start_timestamp AND
		messages.timestamp <= @end_timestamp
GO