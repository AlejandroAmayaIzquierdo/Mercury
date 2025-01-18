namespace Mercury.Modules.Dev;

public class DevModule : BaseModuleHandler
{
    protected override string MODULE => "/Dev";

    public override void Register(ref RouteGroupBuilder module)
    {
        module.MapGet(
            "/MysqlConnection",
            async (DevService service) =>
            {
                bool isConnected = await service.TestMysqlConnection();
                if (!isConnected)
                    return Results.Problem("Error while trying to connect to Mysql Db");
                return Results.Ok(true);
            }
        );
    }
}
