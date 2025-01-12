namespace Mercury;

public abstract class BaseModuleHandler
{
    protected abstract string MODULE { get; }

    public void Invoke(ref WebApplication app)
    {
        var module = app.MapGroup(MODULE);
        Register(ref module);
    }

    public abstract void Register(ref RouteGroupBuilder module);
}
