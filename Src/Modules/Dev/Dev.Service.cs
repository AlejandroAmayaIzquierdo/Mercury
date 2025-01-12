using Mercury.Db;

namespace Mercury.Module.Dev;

public class DevService(MysqlContext dbContext)
{
    private readonly MysqlContext _dbContext = dbContext;

    public async Task<bool> TestMysqlConnection()
    {
        try
        {
            return await _dbContext.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
}
