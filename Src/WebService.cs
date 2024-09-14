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
                    [
                        // TODO Add MIME types that should be compressed
                        "text/plain",
                        "application/json",
                        "text/css",
                        "application/javascript"
                    ]
                );

                options.ExcludedMimeTypes =
                [
                    "application/zip",
                    "application/x-rar-compressed",
                    "application/x-7z-compressed",
                    "application/x-gzip",
                    "video/mp4",
                    "video/x-matroska"
                ];
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
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    }
                    else
                    {
                        policy.WithOrigins(allowedHosts).AllowAnyMethod().AllowAnyHeader();
                    }
                });
            });

            builder.Services.AddRegisterRoutes();

            builder.Services.AddDbContext<MySqliteContext>(options =>
            {
                options.UseMySQL("server=127.0.0.1;port=3306;uid=root;pwd=;database=mercury");
            });


            // TODO add auth (maybe jwt)

            App = builder.Build();

            if (App.Environment.IsDevelopment())
            {
                App.UseSwagger();
                App.UseSwaggerUI();
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
