using Mercury.Util;

namespace Mercury.Modules;

public class AppModule : BaseModuleHandler
{
    protected override string MODULE => "/";

    public override void Register(ref RouteGroupBuilder module)
    {
        module
            .MapGet(
                "/",
                (IHostEnvironment environment) =>
                {
                    if (environment.IsDevelopment())
                        return Results.Redirect("/swagger");
                    return Results.NotFound();
                }
            )
            .ExcludeFromDescription();
    }
}
