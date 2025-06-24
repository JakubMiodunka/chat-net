CREATE TABLE users(
	identifier INT IDENTITY(1, 1),
	name varchar(32) NOT NULL,
	password_hash varchar(32) NOT NULL,
	CONSTRAINT pk_users PRIMARY KEY(identifier)
)