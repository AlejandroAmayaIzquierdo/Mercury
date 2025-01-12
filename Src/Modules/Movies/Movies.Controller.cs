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
    }
}
