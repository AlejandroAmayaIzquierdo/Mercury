using Mercury.Util;

namespace Mercury.Modules.Dev;

public class DevModule : BaseModuleHandler
{
    public override void Register(ref RouteGroupBuilder module)
    {
        module.MapGet(
            "/mysqlConnection",
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
