IF NOT EXISTS(SELECT TOP(1) 1 FROM SYS.TABLES WHERE NAME = 'users')
	CREATE TABLE users(
		id INT IDENTITY(1, 1),
		name varchar(32) NOT NULL,
		password_hash varchar(32) NOT NULL,
		CONSTRAINT pk_users PRIMARY KEY(id)
	)
GO