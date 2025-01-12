using Mercury.Models.Db;
using Mercury.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;

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

        module.MapPost(
            "/",
            async ([FromBody] InputCreateMovie input, MoviesService service) =>
            {
                (string?, Movie?) resp = await service.Add(input);

                string? err = resp.Item1;
                Movie? movie = resp.Item2;

                if (!string.IsNullOrEmpty(err))
                    throw new HttpException(err);
                return movie;
            }
        );

        module.MapPut(
            "/",
            async ([FromBody] Movie input, MoviesService service) =>
            {
                (string?, Movie?) resp = await service.Update(input);

                string? err = resp.Item1;
                Movie? movie = resp.Item2;

                if (!string.IsNullOrEmpty(err))
                    throw new HttpException(err, 404);
                return movie;
            }
        );

        module.MapDelete(
            "/{id}",
            async (Guid id, MoviesService service) =>
            {
                return await service.DeleteById(id);
            }
        );
    }
}
