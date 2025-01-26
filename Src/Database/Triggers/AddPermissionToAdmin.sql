DELIMITER $$

CREATE TRIGGER AddPermissionToAdmin
AFTER INSERT ON Permissions
FOR EACH ROW
BEGIN
    -- Declare a variable to store the Admin role ID
    DECLARE adminRoleId INT;

    -- Fetch the Admin role ID
    SELECT Id INTO adminRoleId
    FROM Roles
    WHERE Name = 'Admin'
    LIMIT 1;

    -- Insert the new RolePermission if the Admin role exists
    IF adminRoleId IS NOT NULL THEN
        INSERT INTO RolePermissions (RoleId, PermissionId)
        VALUES (adminRoleId, NEW.Id);
    END IF;
END$$

DELIMITER ;
