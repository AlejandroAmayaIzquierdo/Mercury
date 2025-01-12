using Mercury.Db;
using Mercury.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace Mercury.Module.Movies;

public class MoviesService(MysqlContext dbContext)
{
    private readonly MysqlContext _dbContext = dbContext;

    public List<Movie> GetAll()
    {
        return [.. _dbContext.Movies.Include(movie => movie.Genre)];
    }

    public async Task<Movie?> GetById(Guid id)
    {
        return await _dbContext.Movies.SingleOrDefaultAsync(x => x.Id == id);
    }
}
