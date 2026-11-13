

USE master;
GO

IF DB_ID(N'RaceDayDB') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

/* -----------------------------
   1. Users
   ----------------------------- */
CREATE TABLE dbo.Users
(
    UserID INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Phone NVARCHAR(30) NULL,
    ProfilePictureUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Users PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Organiser', N'Participant'))
);
GO

/* -----------------------------
   2. Events
   ----------------------------- */
CREATE TABLE dbo.Events
(
    EventID INT IDENTITY(1,1) NOT NULL,
    OrganiserID INT NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EventType NVARCHAR(10) NOT NULL,
    BannerImageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Events PRIMARY KEY (EventID),
    CONSTRAINT FK_Events_Users_Organiser
        FOREIGN KEY (OrganiserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT CK_Events_Distance CHECK (DistanceKm > 0),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN (N'Run', N'Walk', N'Cycle'))
);
GO

/* -----------------------------
   3. Categories
   ----------------------------- */
CREATE TABLE dbo.Categories
(
    CategoryID INT IDENTITY(1,1) NOT NULL,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    MinAge INT NULL,
    MaxAge INT NULL,
    CategoryDistanceKm DECIMAL(6,2) NULL,

    CONSTRAINT PK_Categories PRIMARY KEY (CategoryID),
    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID) ON DELETE CASCADE,
    CONSTRAINT CK_Categories_AgeRange CHECK
        (MinAge IS NULL OR MaxAge IS NULL OR (MinAge >= 0 AND MaxAge >= MinAge)),
    CONSTRAINT CK_Categories_Distance CHECK
        (CategoryDistanceKm IS NULL OR CategoryDistanceKm > 0),
    CONSTRAINT UQ_Categories_Event_Name UNIQUE (EventID, CategoryName)
);
GO

/* -----------------------------
   4. Enrolments
   ----------------------------- */
CREATE TABLE dbo.Enrolments
(
    EnrolmentID INT IDENTITY(1,1) NOT NULL,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME2(0) NOT NULL CONSTRAINT DF_Enrolments_Date DEFAULT SYSUTCDATETIME(),
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Enrolments_Status DEFAULT N'Confirmed',

    CONSTRAINT PK_Enrolments PRIMARY KEY (EnrolmentID),
    CONSTRAINT FK_Enrolments_Users_Participant
        FOREIGN KEY (ParticipantID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Enrolments_Events
        FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID),
    CONSTRAINT FK_Enrolments_Categories
        FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID),
    CONSTRAINT CK_Enrolments_Status CHECK (Status IN (N'Pending', N'Confirmed', N'Cancelled')),
    CONSTRAINT UQ_Enrolments_Participant_Event UNIQUE (ParticipantID, EventID)
);
GO

/* -----------------------------
   5. Results
   ----------------------------- */
CREATE TABLE dbo.Results
(
    ResultID INT IDENTITY(1,1) NOT NULL,
    EnrolmentID INT NOT NULL,
    FinishTimeSeconds INT NOT NULL,
    FinishingPosition INT NOT NULL,
    PublishedAt DATETIME2(0) NULL,

    CONSTRAINT PK_Results PRIMARY KEY (ResultID),
    CONSTRAINT UQ_Results_Enrolment UNIQUE (EnrolmentID),
    CONSTRAINT FK_Results_Enrolments
        FOREIGN KEY (EnrolmentID) REFERENCES dbo.Enrolments(EnrolmentID) ON DELETE CASCADE,
    CONSTRAINT CK_Results_FinishTime CHECK (FinishTimeSeconds > 0),
    CONSTRAINT CK_Results_Position CHECK (FinishingPosition > 0)
);
GO

/* Indexes on common foreign-key lookup columns */
CREATE INDEX IX_Events_OrganiserID ON dbo.Events(OrganiserID);
CREATE INDEX IX_Categories_EventID ON dbo.Categories(EventID);
CREATE INDEX IX_Enrolments_ParticipantID ON dbo.Enrolments(ParticipantID);
CREATE INDEX IX_Enrolments_EventID ON dbo.Enrolments(EventID);
CREATE INDEX IX_Enrolments_CategoryID ON dbo.Enrolments(CategoryID);
GO

/* -----------------------------
   Seed Users
   PasswordHash values are placeholders for demonstration.
   Part 2 must hash real passwords before storing them.
   ----------------------------- */
INSERT INTO dbo.Users
    (FirstName, LastName, Email, PasswordHash, Role, Phone)
VALUES
    (N'Naledi', N'Mokoena', N'naledi.organiser@raceday.co.za', N'PART2_HASH_PLACEHOLDER_1', N'Organiser', N'0825551001'),
    (N'Johan', N'van Wyk', N'johan.organiser@raceday.co.za', N'PART2_HASH_PLACEHOLDER_2', N'Organiser', N'0825551002'),
    (N'Ayanda', N'Dlamini', N'ayanda.participant@raceday.co.za', N'PART2_HASH_PLACEHOLDER_3', N'Participant', N'0825552001'),
    (N'Liam', N'Naidoo', N'liam.participant@raceday.co.za', N'PART2_HASH_PLACEHOLDER_4', N'Participant', N'0825552002');
GO

/* -----------------------------
   Seed Events
   ----------------------------- */
INSERT INTO dbo.Events
    (OrganiserID, Name, Description, EventDate, Location, DistanceKm, EventType)
VALUES
    (1, N'Durban Golden Run 10K', N'A fast 10 km road race along the Durban beachfront.', '2026-10-18', N'Durban Beachfront, KwaZulu-Natal', 10.00, N'Run'),
    (1, N'Limpopo Community Walk', N'A family-friendly community walk supporting active living.', '2026-11-07', N'Polokwane, Limpopo', 5.00, N'Walk'),
    (2, N'Cape Cycle Challenge', N'A road cycling event for recreational and competitive cyclists.', '2026-12-06', N'Cape Town, Western Cape', 42.00, N'Cycle');
GO

/* -----------------------------
   Categories for every event
   ----------------------------- */
INSERT INTO dbo.Categories
    (EventID, CategoryName, MinAge, MaxAge, CategoryDistanceKm)
VALUES
    (1, N'Under 20', 13, 19, 10.00),
    (1, N'Senior', 20, 39, 10.00),
    (1, N'40 Plus', 40, NULL, 10.00),
    (2, N'Junior Walk', 10, 17, 5.00),
    (2, N'Open Walk', 18, 49, 5.00),
    (2, N'50 Plus Walk', 50, NULL, 5.00),
    (3, N'Open Cycle', 18, 39, 42.00),
    (3, N'40 Plus Cycle', 40, NULL, 42.00);
GO

/* -----------------------------
   Sample enrolments
   ----------------------------- */
INSERT INTO dbo.Enrolments
    (ParticipantID, EventID, CategoryID, Status)
VALUES
    (3, 1, 2, N'Confirmed'),
    (4, 1, 2, N'Confirmed'),
    (3, 2, 5, N'Confirmed'),
    (4, 3, 7, N'Pending');
GO

/* Sample result records for completed-style testing */
INSERT INTO dbo.Results
    (EnrolmentID, FinishTimeSeconds, FinishingPosition, PublishedAt)
VALUES
    (1, 3125, 47, SYSUTCDATETIME()),
    (2, 3290, 82, SYSUTCDATETIME());
GO

/* Verification queries */
SELECT * FROM dbo.Users;
SELECT * FROM dbo.Events;
SELECT * FROM dbo.Categories;
SELECT * FROM dbo.Enrolments;
SELECT * FROM dbo.Results;
GO
