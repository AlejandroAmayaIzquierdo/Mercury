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
        var excludeModules = _configuration.GetSection("Routes:ExcludeModules").Get<string[]>();

        var routeTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseModuleHandler)));

        foreach (var type in routeTypes)
        {
            // Skip excluded routes based on the MODULE property
            var moduleField = type.GetProperty(
                "MODULE",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
            );
            if (excludeModules != null && moduleField != null)
            {
                var instance = Activator.CreateInstance(type);
                var moduleValue = moduleField.GetValue(instance)?.ToString();

                if (excludeModules.Contains(moduleValue))
                    continue;
            }

            // Find the Register method
            var registerMethod = type.GetMethod("Invoke");
            if (registerMethod != null)
            {
                var instance = Activator.CreateInstance(type);
                registerMethod.Invoke(instance, [app]);
            }
        }
    }
}
