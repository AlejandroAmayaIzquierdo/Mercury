using Mercury.DB;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

namespace Mercury;

public class WebService
{
    public static WebApplication? App { get; set; }

    private static Logger? Logger;

    public static void Run()
    {
        Init();
        Build();
    }

    private WebService() { }

    // Initialized data for ws
    private static void Init()
    {
        Logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        Logger.Info("Log Init");
    }

    private static void Build()
    {
        try
        {
            var builder = WebApplication.CreateBuilder();

            // Log System
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Compression
            builder.Services.AddOutputCache();
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;

                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    builder.Configuration.GetSection("Compression:Include").Get<string[]>() ?? []
                );

                options.ExcludedMimeTypes =
                    builder.Configuration.GetSection("Compression:Exclude").Get<string[]>() ?? [];
            });

            var allowedHosts =
                builder
                    .Configuration.GetValue<string>("AllowedHosts")
                    ?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? [];

            // Add CORS services
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    // If allowedHosts is empty or contains "*", allow any origin, otherwise use the specified hosts
                    if (allowedHosts.Contains("*"))
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    else
                        policy.WithOrigins(allowedHosts).AllowAnyMethod().AllowAnyHeader();
                });
            });

            builder.Services.AddRegisterRoutes();

            string? connectionString = builder.Configuration.GetConnectionString("Mysql");
            if (connectionString != null)
                builder.Services.AddDbContext<MySqliteContext>(options => options
                    .UseMySQL(connectionString));
            else
                Logger?.Warn("The connection string is not stablish. Any db Access will fail");


            // TODO add auth (maybe jwt)

            App = builder.Build();

            if (App.Environment.IsDevelopment())
            {
                App.UseSwagger();
                App.UseSwaggerUI();
            }
            else
            {
                App.UseHttpsRedirection();
            }

            // Middlewares
            App.UseMiddleware<ResponseWrapperMiddleware>();

            App.UseRegisterRoutes();

            App.UseOutputCache();

            App.Run();
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "An error occurred while building the application");
        }
        finally
        {
            LogManager.Shutdown();
        }
    }

    public static Logger? GetLogger()
    {
        return Logger;
    }
}
