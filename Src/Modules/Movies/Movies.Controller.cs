using Mercury.Models.Db;
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
                Movie? movie = await service.GetById(id);

                if (movie == null)
                    return Results.NotFound("There is no movie with that id");

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

                // XXX When using Problem a object its added to the detail of the global response.
                // So if you want to access the message you have to go Detail.detail and that doesn't feel right.
                // Use badRequest could be a solution but it also feels wrong because in this case is not a problem of a bad request its a internal error
                if (!string.IsNullOrEmpty(err))
                    return Results.Problem(err);
                return Results.Ok(movie);
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
                    return Results.NotFound(err);
                return Results.Ok(movie);
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
