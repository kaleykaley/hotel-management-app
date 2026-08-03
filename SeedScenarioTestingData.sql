USE HotelManagement2026;
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
('Pedro Santos', '961234567', 'pedro.santos@email.com', 'Driver''s License', 'DL556677'),
('Ana Ferreira', '932112233', 'ana.ferreira@email.com', 'Passport', 'PT998877'),
('Carlos Oliveira', '965443322', 'carlos.oliveira@email.com', 'National ID', '87654321');

GO


--------------------------------------------------
-- ROOMS
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
(102, 'Standard', 2, 80.00, 'Reserved', 0),
(201, 'Suite', 4, 150.00, 'Available', 0),
(202, 'Suite', 4, 175.00, 'Occupied', 0),
(301, 'Luxury', 2, 250.00, 'Available', 0),
(302, 'Luxury', 3, 300.00, 'Maintenance', 0);

GO


--------------------------------------------------
-- RESERVATIONS
--------------------------------------------------

-- Future reservation
-- Tests:
-- delete restrictions
-- double booking prevention

INSERT INTO Reservations
(
    GuestId,
    RoomId,
    CheckInDate,
    CheckOutDate,
    NumberOfGuests,
    ReservationStatus
)
VALUES
(
    1,
    2,
    '2026-09-10',
    '2026-09-15',
    2,
    'Reserved'
);


-- Current guest checked in
-- Tests:
-- checkout process
-- room status changes

INSERT INTO Reservations
(
    GuestId,
    RoomId,
    CheckInDate,
    CheckOutDate,
    NumberOfGuests,
    ReservationStatus
)
VALUES
(
    2,
    4,
    '2026-08-01',
    '2026-08-05',
    2,
    'Checked_In'
);


-- Completed stay
-- Tests:
-- history
-- invoices
-- payments

INSERT INTO Reservations
(
    GuestId,
    RoomId,
    CheckInDate,
    CheckOutDate,
    NumberOfGuests,
    ReservationStatus
)
VALUES
(
    3,
    3,
    '2026-07-01',
    '2026-07-04',
    3,
    'Checked_Out'
);


-- Cancelled reservation

INSERT INTO Reservations
(
    GuestId,
    RoomId,
    CheckInDate,
    CheckOutDate,
    NumberOfGuests,
    ReservationStatus
)
VALUES
(
    4,
    1,
    '2026-08-20',
    '2026-08-25',
    1,
    'Cancelled'
);

GO


--------------------------------------------------
-- EXTRA SERVICES
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
-- SERVICES USED
--------------------------------------------------

INSERT INTO ReservationExtraServices
(
    ReservationId,
    ExtraServiceId,
    Quantity
)
VALUES
(3, 1, 3),
(3, 4, 1);

GO


--------------------------------------------------
-- INVOICES
--------------------------------------------------

INSERT INTO Invoices
(
    ReservationId,
    IssueDate,
    InvoiceStatus
)
VALUES
(3, '2026-07-04', 'Paid'),
(2, '2026-08-05', 'Unpaid');

GO


--------------------------------------------------
-- PAYMENTS
--------------------------------------------------

INSERT INTO Payments
(
    PaymentType,
    AmountPaid,
    PaymentDate,
    InvoiceId
)
VALUES
(
    'Credit_Card',
    520.00,
    '2026-07-04',
    1
);

GO


--------------------------------------------------
-- VERIFY
--------------------------------------------------

SELECT * FROM Guests;
SELECT * FROM Rooms;
SELECT * FROM Reservations;
SELECT * FROM ExtraServices;
SELECT * FROM ReservationExtraServices;
SELECT * FROM Invoices;
SELECT * FROM Payments;

GO