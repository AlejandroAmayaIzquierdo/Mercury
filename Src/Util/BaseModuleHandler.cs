namespace Mercury.Util;

public abstract class BaseModuleHandler
{
    protected abstract string MODULE { get; }
    protected virtual bool IS_AUTH_MODULE { get; } = false;
    protected virtual string AUTH_MODULE_ROLE { get; } = string.Empty;

    public void Invoke(ref WebApplication app)
    {
        var module = app.MapGroup(MODULE);
        module.WithTags(MODULE);

        if (IS_AUTH_MODULE)
        {
            if (!string.IsNullOrWhiteSpace(AUTH_MODULE_ROLE))
            {
                // Require specific role
                module.RequireAuthorization(options =>
                {
                    options.RequireRole(AUTH_MODULE_ROLE);
                });
            }
            else
            {
                // General authorization with no specific role
                module.RequireAuthorization();
            }
        }

        Register(ref module);
    }

    public abstract void Register(ref RouteGroupBuilder module);
}
