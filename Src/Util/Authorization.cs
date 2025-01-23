using Mercury.Util;
using Microsoft.AspNetCore.Authorization;

public static class AuthorizationUtil
{
    /// <summary>
    /// Configures an authorization policy builder to require specific permissions.
    /// </summary>
    /// <param name="builder">The authorization policy builder.</param>
    /// <param name="requiredPermissions">A collection of required permissions.</param>
    public static void RequirePermissions(
        this AuthorizationPolicyBuilder builder,
        params PermissionsTypes[] requiredPermissions
    )
    {
        builder.RequireAssertion(context =>
        {
            // Get the "permissions" claim value
            string userPermissionsString =
                context.User.Claims.FirstOrDefault(claim => claim.Type == "permissions")?.Value
                ?? string.Empty;

            if (string.IsNullOrEmpty(userPermissionsString))
                return false;

            // Parse user's permissions into a list of integers
            var userPermissions = userPermissionsString.Split(',').Select(int.Parse).ToList();

            // Check if all required permissions are present in the user's permissions
            return requiredPermissions.All(p => userPermissions.Contains((int)p));
        });
    }
}
