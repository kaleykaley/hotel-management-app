DROP DATABASE IF EXISTS HotelManagement;
GO

CREATE DATABASE HotelManagement;
GO

USE HotelManagement;
GO

CREATE TABLE Guests(
	GuestId INT PRIMARY KEY IDENTITY(1,1),
	Name NVARCHAR(100) NOT NULL,
	PhoneContact NVARCHAR(30),
	Email NVARCHAR(100),
	-- Passport, Driver's License, National ID
	DocumentType NVARCHAR(50),
	DocumentNumber NVARCHAR(50)
);

CREATE TABLE Rooms(
	RoomId INT PRIMARY KEY IDENTITY(1,1),
	RoomNumber INT NOT NULL UNIQUE,
	-- enum: Standard, Suite, Luxury
	RoomType NVARCHAR(30) NOT NULL,
	Capacity INT NOT NULL,
	PricePerNight DECIMAL(10,2) NOT NULL,
	-- enum: Available, Occupied, Under_Maintenance, Reserved
	RoomStatus NVARCHAR(30) NOT NULL
);

CREATE TABLE Reservations(
	ReservationId INT PRIMARY KEY IDENTITY(1,1),
	GuestId INT NOT NULL, -- foreign key representing whole object
	RoomId INT NOT NULL, -- foreign key
	CheckInDate DATE NOT NULL,
	CheckOutDate DATE NOT NULL,
	NumberOfGuests INT NOT NULL,
	-- enum:Reserved, Checked_In,Checked_Out,Cancelled
	ReservationStatus NVARCHAR(30) NOT NULL,

	FOREIGN KEY (GuestID) REFERENCES Guests(GuestId),
	FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId)
);

CREATE TABLE Invoices(
	InvoiceId INT PRIMARY KEY IDENTITY(1,1),
	-- unique so each reservation can only have one invoice
	ReservationId INT NOT NULL UNIQUE, -- foreign key for resevation obj.
	--ReservationId INT NOT NULL, 

	--public List<ExtraService> ExtraServices { get; set; } = new List<ExtraService>();
	-- CALCULATED THROUGH: Invoice ? Reservation ? ReservationExtraServices ? ExtraServices

	IssueDate DATE NOT NULL, 
	InvoiceStatus NVARCHAR(30) NOT NULL,

	FOREIGN KEY (ReservationID) REFERENCES Reservations(ReservationID)
);


CREATE TABLE Payments(
	PaymentId INT PRIMARY KEY IDENTITY(1,1),
	-- enum: Cash, Credit_Card, Debit_Card
	PaymentType NVARCHAR(30) NOT NULL,
	AmountPaid DECIMAL(10,2) NOT NULL,
	PaymentDate DATE NOT NULL,
	-- unique so each payment can only have one invoice (must pay in full)
	InvoiceId INT NOT NULL UNIQUE, -- foreign key representing invoice obj.
	--InvoiceId INT NOT NULL, 

	FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId)
);


CREATE TABLE ExtraServices(
	ExtraServiceId INT PRIMARY KEY IDENTITY(1,1),
	Name NVARCHAR(100) NOT NULL UNIQUE,
	Price DECIMAL(10,2) NOT NULL
);

CREATE TABLE ReservationExtraServices(
	ReservationId INT NOT NULL,
	ExtraServiceId INT NOT NULL,
	Quantity INT NOT NULL DEFAULT 1,
	-- primary key consisting of 2 foreign keys
	PRIMARY KEY(ReservationId, ExtraServiceId),

	FOREIGN KEY (ReservationId) REFERENCES Reservations(ReservationId),
	FOREIGN KEY (ExtraServiceId) REFERENCES ExtraServices(ExtraServiceId)
);