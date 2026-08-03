USE HotelManagement2026;
GO

-- Disable foreign key checks temporarily
EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Delete all data
DELETE FROM Payments;
DELETE FROM Invoices;
DELETE FROM ReservationExtraServices;
DELETE FROM Reservations;
DELETE FROM ExtraServices;
DELETE FROM Guests;
DELETE FROM Rooms;
GO

-- Reset identity counters
DBCC CHECKIDENT ('Payments', RESEED, 0);
DBCC CHECKIDENT ('Invoices', RESEED, 0);
DBCC CHECKIDENT ('ReservationExtraServices', RESEED, 0);
DBCC CHECKIDENT ('Reservations', RESEED, 0);
DBCC CHECKIDENT ('ExtraServices', RESEED, 0);
DBCC CHECKIDENT ('Guests', RESEED, 0);
DBCC CHECKIDENT ('Rooms', RESEED, 0);
GO

-- Re-enable foreign keys
EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
GO