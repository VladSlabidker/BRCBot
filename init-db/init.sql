CREATE DATABASE ReceiptBot;
GO

USE ReceiptBot;
GO

CREATE LOGIN myuser WITH PASSWORD = 'mypassword';
GO

CREATE USER myuser FOR LOGIN myuser;
GO

ALTER ROLE db_owner ADD MEMBER myuser;
GO