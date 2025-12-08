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