-- Idempotent migration script to add SuperPower field to SuperHero table

SET NOCOUNT ON;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'SuperPower' AND Object_ID = Object_ID(N'dbo.SuperHeroes'))
BEGIN
    ALTER TABLE dbo.SuperHeroes
    ADD SuperPower NVARCHAR(MAX) NOT NULL DEFAULT (N'');
END
