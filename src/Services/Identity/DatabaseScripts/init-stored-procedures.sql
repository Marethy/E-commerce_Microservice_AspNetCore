-- Wait for database to be ready
USE IdentityDb;
GO

-- Create Get_Permission_By_RoleId
IF EXISTS (
	SELECT * FROM sysobjects
	WHERE id = object_id(N'[Get_Permission_By_RoleId]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE [Get_Permission_By_RoleId]
END
GO

CREATE PROCEDURE [Get_Permission_By_RoleId] @roleId varchar(50) null
AS
BEGIN
	SET NOCOUNT ON;
	SELECT * FROM [Identity].Permissions
	WHERE RoleId = @roleId
END
GO

-- Create Create_Permission
IF EXISTS (
	SELECT * FROM sysobjects
	WHERE id = object_id(N'[Create_Permission]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE [Create_Permission]
END
GO

CREATE PROCEDURE [Create_Permission] 
	@roleId VARCHAR(50) NULL,
	@function VARCHAR(50) NULL,
	@command VARCHAR(50) NULL,
	@newId BIGINT OUTPUT
AS
BEGIN
	SET XACT_ABORT ON;
	BEGIN TRAN
		BEGIN TRY 
			IF NOT EXISTS (
				SELECT * FROM [Identity].Permissions
				WHERE [RoleId] = @roleId AND [Function] = @function AND [Command] = @command)
			BEGIN
				INSERT INTO [Identity].Permissions ([RoleId], [Function], [Command])
				VALUES (@roleId, @function, @command)
				SET @newId = SCOPE_IDENTITY();
			END
	COMMIT
	END TRY
	
	BEGIN CATCH 
	ROLLBACK
		DECLARE @ErrorMessage VARCHAR(2000)
			SELECT @ErrorMessage = 'ERROR: ' + ERROR_MESSAGE() 
			RAISERROR(@ErrorMessage, 16, 1)
	END CATCH
END
GO

-- Create Delete_Permission
IF EXISTS (
	SELECT * FROM sysobjects
	WHERE id = object_id(N'[Delete_Permission]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE [Delete_Permission]
END
GO

CREATE PROCEDURE [Delete_Permission] 
	@roleId VARCHAR(50) NULL,
	@function VARCHAR(50) NULL,
	@command VARCHAR(50) NULL
AS
BEGIN
	DELETE FROM [Identity].Permissions 
	WHERE [RoleId] = @roleId AND [Function] = @function AND [Command] = @command
END
GO

-- Create Permission table type for Update_Permissions_By_RoleId
IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'Permission')
BEGIN
	DROP TYPE [dbo].[Permission]
END
GO

CREATE TYPE [dbo].[Permission] AS TABLE(
	[Function] VARCHAR(50) NOT NULL,
	[Command] VARCHAR(50) NOT NULL,
	[RoleId] VARCHAR(50) NOT NULL
)
GO

-- Create Update_Permissions_By_RoleId
IF EXISTS (
	SELECT * FROM sysobjects
	WHERE id = object_id(N'[Update_Permissions_By_RoleId]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE [Update_Permissions_By_RoleId]
END
GO

CREATE PROCEDURE [Update_Permissions_By_RoleId] 
	@roleId VARCHAR(50) NULL,
	@permissions Permission READONLY
AS
BEGIN
	SET XACT_ABORT ON;
	BEGIN TRAN
		BEGIN TRY
			DELETE FROM [Identity].Permissions WHERE RoleId = @roleId

			INSERT INTO [Identity].Permissions
			SELECT [Function], [Command], [RoleId] FROM @permissions
	COMMIT
	END TRY

	BEGIN CATCH
		ROLLBACK
			DECLARE @ErrorMessage VARCHAR(2000)
			SELECT @ErrorMessage = 'ERROR: ' + ERROR_MESSAGE()
			RAISERROR(@ErrorMessage, 16, 1)
	END CATCH
END
GO

PRINT 'Stored procedures created successfully';
