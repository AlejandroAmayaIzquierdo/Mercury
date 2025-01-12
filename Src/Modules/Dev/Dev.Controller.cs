using Mercury.Models.Exceptions;

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
            async (DevService service) =>
            {
                bool isConnected = await service.TestMysqlConnection();
                if (!isConnected)
                    throw new HttpException("Error while trying to connect to Mysql Db", 408);
                return true;
            }
        );
    }
}
