using Mercury.Util;

namespace Mercury.Modules.Role;

public class RoleModule : BaseModuleHandler
{
    protected override string MODULE => "/Auth/roles";

    public override void Register(ref RouteGroupBuilder module)
    {
        module.MapGet(
            "/",
            () =>
            {
                return Results.Ok();
            }
        );
    }
}
