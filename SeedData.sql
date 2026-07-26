USE HotelManagement;
GO

--------------------------------------------------
-- GUESTS
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
('Pedro Santos', '961234567', 'pedro.santos@email.com', 'Driver License', 'DL556677'),
('Ana Ferreira', '932112233', 'ana.ferreira@email.com', 'Passport', 'PT998877'),
('Carlos Oliveira', '965443322', 'carlos.oliveira@email.com', 'National ID', '87654321');

--------------------------------------------------
-- ROOMS
--------------------------------------------------

INSERT INTO Rooms
(
    RoomNumber,
    RoomType,
    Capacity,
    PricePerNight,
    RoomStatus
)
VALUES
(101, 'Standard', 2, 75.00, 'Available'),
(102, 'Standard', 2, 80.00, 'Available'),
(201, 'Suite', 4, 150.00, 'Available'),
(202, 'Suite', 4, 175.00, 'Available'),
(301, 'Luxury', 2, 250.00, 'Available'),
(302, 'Luxury', 3, 300.00, 'Under_Maintenance');

GO