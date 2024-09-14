using System.Reflection;

namespace Mercury;

public static class RouteRegister
{
    public static void AddRegisterRoutes(this IServiceCollection services)
    {
        services.AddSingleton<RouteRegisterService>();
    }

    public static void UseRegisterRoutes(this WebApplication app)
    {
        var routeRegistrar = app.Services.GetRequiredService<RouteRegisterService>();
        routeRegistrar.RegisterRoutes(app);
    }
}

public class RouteRegisterService(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public void RegisterRoutes(WebApplication app)
    {
        var excludeRoutes = _configuration.GetSection("Routes:ExcludeRoute").Get<string[]>();

        // FIXME
        // var clientExpose = _configuration.GetValue<string>("Routes:ClientExpose");

        var routeTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && t.Namespace != null && t.Namespace.Contains("Routes"));

        foreach (var type in routeTypes)
        {
            // Skip excluded routes
            if (excludeRoutes != null && excludeRoutes.Contains(type.Namespace))
                continue;


            var registerMethod = type.GetMethod("Register");
            if (registerMethod != null)
            {
                var instance = Activator.CreateInstance(type);
                registerMethod.Invoke(instance, [app]);
            }
        }
    }
}
