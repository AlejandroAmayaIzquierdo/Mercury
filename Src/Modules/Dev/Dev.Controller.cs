namespace Mercury.Module.Dev;

public class DevModule : BaseModuleHandler
{
    protected override string MODULE => "/Dev";

    public override void Register(ref RouteGroupBuilder module)
    {
        module.MapGet(
            "/",
            () =>
            {
                return Results.Json("😎");
            }
        );

        module.MapGet(
            "/MysqlConnection",
            () =>
            {
                return Results.Json(DevService.TestMysqlConnection());
            }
        );
    }
}
