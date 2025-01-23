namespace Mercury.Util;

public abstract class BaseModuleHandler
{
    protected abstract string MODULE { get; }
    protected virtual bool IS_AUTH_MODULE { get; } = false;
    protected virtual ICollection<PermissionsTypes> Permissions { get; } = [];

    public void Invoke(ref WebApplication app)
    {
        var module = app.MapGroup(MODULE);
        module.WithTags(MODULE);

        if (IS_AUTH_MODULE || Permissions.Count > 0)
        {
            if (Permissions.Count > 0)
            {
                module.RequireAuthorization(options =>
                {
                    options.RequireAssertion(context =>
                    {
                        string userPermissionsString =
                            context
                                .User.Claims.FirstOrDefault(claim => claim.Type == "permissions")
                                ?.Value ?? string.Empty;

                        if (string.IsNullOrEmpty(userPermissionsString))
                            return true;

                        var userPermissions =
                            userPermissionsString?.Split(',').Select(int.Parse).ToList() ?? [];

                        return Permissions.All(p => userPermissions.Contains((int)p));
                    });
                });
            }
            else
            {
                module.RequireAuthorization();
            }
        }

        Register(ref module);
    }

    public abstract void Register(ref RouteGroupBuilder module);
}
