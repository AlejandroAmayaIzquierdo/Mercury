using Mercury.Models.Db;
using Mercury.Models.Exceptions;

namespace Mercury.Module.Movies;

public class MoviesModule : BaseModuleHandler
{
    protected override string MODULE => "/Movies";

    public override void Register(ref RouteGroupBuilder module)
    {
        module.MapGet(
            "/",
            (MoviesService service) =>
            {
                var movies = service.GetAll();
                return Results.Ok(movies);
            }
        );
        module.MapGet(
            "/{id}",
            async (Guid id, MoviesService service) =>
            {
                Movie? movie =
                    await service.GetById(id)
                    ?? throw new HttpException("Movie not found with that id", 404);

                return Results.Ok(movie);
            }
        );
    }
}
