namespace Mercury;

public abstract class BaseModuleHandler
{
    protected abstract string MODULE { get; }

    // TODO if this is true add .RequireAuthorization to the module.
    // Maybe add some kind of role auth too.
    // protected abstract bool IS_AUTH_MODULE { get; }

    public void Invoke(ref WebApplication app)
    {
        var module = app.MapGroup(MODULE);
        module.WithTags(MODULE);
        Register(ref module);
    }

    public abstract void Register(ref RouteGroupBuilder module);
}
