CREATE TABLE messages(
		identifier INT IDENTITY(1, 1),
		timestamp DATETIME NOT NULL,
		sender_identifier INT NOT NULL,
		receiver_identifier INT NOT NULL,
		content VARCHAR(MAX) NOT NULL,
		CONSTRAINT pk_messages PRIMARY KEY(identifier),
		CONSTRAINT fk_messages_sender_id FOREIGN KEY(sender_identifier) REFERENCES users(identifier),
		CONSTRAINT fk_messages_receiver_id FOREIGN KEY(receiver_identifier) REFERENCES users(identifier)
	)