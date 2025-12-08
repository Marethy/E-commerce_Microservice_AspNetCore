IF EXISTS (
	SELECT * FROM sysobjects
	WHERE id = object_id(N'[Update_Permissions_By_RoleId]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE [Update_Permissions_By_RoleId]
END

DROP TYPE IF EXISTS [dbo].[Permission]
CREATE TYPE [dbo].[Permission] AS TABLE(
	[Function] VARCHAR(50) NOT NULL,
	[Command] VARCHAR(50) NOT NULL,
	[RoleId] VARCHAR(50) NOT NULL)
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