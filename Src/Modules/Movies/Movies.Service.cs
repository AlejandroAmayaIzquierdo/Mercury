using Mercury.Db;
using Mercury.Models.Movies;
using Microsoft.EntityFrameworkCore;

namespace Mercury.Modules.Movies;

public record InputCreateMovie(string Tittle, int? Genre = 1);

public class MoviesService(MysqlContext dbContext)
{
    private readonly MysqlContext _dbContext = dbContext;

    public List<Movie> GetAll()
    {
        return [.. _dbContext.Movies.Include(movie => movie.Genre)];
    }

    public async Task<Movie?> GetById(Guid id)
    {
        return await _dbContext
            .Movies.Include(movie => movie.Genre)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<(string?, Movie?)> Add(InputCreateMovie input)
    {
        try
        {
            Movie movie =
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = input.Tittle,
                    GenreId = input.Genre
                };

            _dbContext.Movies.Add(movie);
            await _dbContext.SaveChangesAsync();

            return (null, movie);
        }
        catch (Exception ex)
        {
            return (ex.Message, null);
        }
    }

    public async Task<(string?, Movie?)> Update(Movie input)
    {
        try
        {
            Movie? movie = await GetById(input.Id);

            if (movie == null)
                return ("Could not find the movie", null);

            movie.Title = input.Title;
            movie.GenreId = input.GenreId;

            _dbContext.Movies.Update(movie);

            await _dbContext.SaveChangesAsync();

            return (null, movie);
        }
        catch (Exception ex)
        {
            return (ex.Message, null);
        }
    }

    public async Task<bool> DeleteById(Guid id)
    {
        var result = await _dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM `Movies` WHERE `Id` = {0}",
            id
        );
        return result > 0;
    }
}
