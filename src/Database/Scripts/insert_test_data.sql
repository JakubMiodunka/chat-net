-- Filling 'users' table.
INSERT INTO
	users(
		name,
		password_hash
	)
VALUES
	('Sylvester Stallone', '1234567890'),
	('Arnold Schwarzenegger', '0987654321'),
	('Bruce Willis', '24680'),
	('Jean-Claude Van Damme', '13579'),
	('Kurt Russell', '2143658709'),
	('Chuck Norris', '3254769801'),
	('Harrison Ford', '321654987'),
	('Dolph Lundgren', '432765098'),
	('Tom Cruise', '0192837465'),
	('Val Kilmer', '5647382910')

-- Filling 'messages' table.
INSERT INTO
	messages(
		timestamp,
		sender_id,
		receiver_id,
		content
	)
VALUES
	('2025-01-01T12:00:00', 1, 2, 'I''m John, John Rambo'),
	('2025-01-01T12:15:00', 2, 1, 'I''ll be back'),
	('2024-07-19T20:10:00', 9, 10, 'Good job Iceman'),
	('2024-07-20T01:00:00', 10, 9, 'I''ve got you Mav'),
	('2025-03-21T13:30:00', 3, 7, 'Zed is dead baby, Zed is dead'),
	('2025-03-21T21:15:00', 7, 3, 'Wrong number - Indy here'),
	('2023-10-10T14:30:00', 5, 6, 'We need to burn it!')
