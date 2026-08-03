USE HotelManagement2026;
GO

--------------------------------------------------
-- BASIC GUESTS
--------------------------------------------------

INSERT INTO Guests
(
    Name,
    PhoneContact,
    Email,
    DocumentType,
    DocumentNumber
)
VALUES
('João Silva', '912345678', 'joao.silva@email.com', 'Passport', 'PT123456'),
('Maria Costa', '934567890', 'maria.costa@email.com', 'National ID', '12345678'),
('Pedro Santos', '961234567', 'pedro.santos@email.com', 'Driver''s License', 'DL556677'),
('Ana Ferreira', '932112233', 'ana.ferreira@email.com', 'Passport', 'PT998877');


GO


--------------------------------------------------
-- BASIC ROOMS
--------------------------------------------------

INSERT INTO Rooms
(
    RoomNumber,
    RoomType,
    Capacity,
    PricePerNight,
    RoomStatus,
    IsDeleted
)
VALUES
(101, 'Standard', 2, 75.00, 'Available', 0),
(102, 'Standard', 2, 80.00, 'Available', 0),
(201, 'Suite', 4, 150.00, 'Available', 0),
(202, 'Suite', 4, 175.00, 'Available', 0),
(301, 'Luxury', 2, 250.00, 'Available', 0),
(302, 'Luxury', 3, 300.00, 'Maintenance', 0);

GO


--------------------------------------------------
-- BASIC EXTRA SERVICES
--------------------------------------------------

INSERT INTO ExtraServices
(
    Name,
    Price
)
VALUES
('Breakfast', 15.00),
('Spa Access', 50.00),
('Room Service', 25.00),
('Private Parking', 10.00);

GO


--------------------------------------------------
-- VERIFY
--------------------------------------------------

SELECT * FROM Guests;
SELECT * FROM Rooms;
SELECT * FROM ExtraServices;

GO