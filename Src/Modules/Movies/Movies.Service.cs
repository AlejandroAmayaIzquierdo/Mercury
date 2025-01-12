using Mercury.Db;
using Mercury.Models.Db;

namespace Mercury.Module.Movies;

public class MoviesService(MysqlContext dbContext)
{
    private readonly MysqlContext _dbContext = dbContext;

    public List<Movie> GetAll()
    {
        return [.. _dbContext.Movies];
    }
}
