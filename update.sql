-- Idempotent SQL script to update the database schema for SuperHeroAPI

BEGIN TRY
    -- Check if the SuperPower column already exists
    IF NOT EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'SuperHeroes' AND COLUMN_NAME = 'SuperPower'
    )
    BEGIN
        -- Add the SuperPower column
        ALTER TABLE SuperHeroes
        ADD SuperPower NVARCHAR(MAX) NOT NULL DEFAULT '';
    END
END TRY
BEGIN CATCH
    PRINT 'Error occurred while updating the database schema.';
END CATCH;