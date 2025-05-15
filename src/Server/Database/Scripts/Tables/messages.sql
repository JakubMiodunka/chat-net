IF NOT EXISTS(SELECT TOP(1) 1 FROM SYS.TABLES WHERE NAME = 'messages')
	CREATE TABLE messages(
		id INT IDENTITY(1, 1),
		timestamp DATETIME NOT NULL,
		sender_id INT NOT NULL,
		receiver_id INT NOT NULL,
		content VARCHAR(1024) NOT NULL,
		CONSTRAINT pk_messages PRIMARY KEY(Id),
		CONSTRAINT fk_messages_sender_id FOREIGN KEY(sender_id) REFERENCES users(id),
		CONSTRAINT fk_messages_receiver_id FOREIGN KEY(receiver_id) REFERENCES users(id)
	)
GO