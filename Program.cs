using Mercury.Db;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

namespace Mercury;

public class Program
{
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();

        // Log System
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        LogService.Instance = LogManager
            .Setup()
            .LoadConfigurationFromAppSettings()
            .GetCurrentClassLogger();
        LogService.Get()?.Info("Log Init");

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

        string? connectionString = builder.Configuration.GetConnectionString("Mysql");
        if (connectionString != null)
            builder.Services.AddDbContext<MysqlContext>(
                options => options.UseMySQL(connectionString),
                ServiceLifetime.Scoped
            );
        else
            LogService
                .Get()
                ?.Error("The connection string is not stablish. Any db Access will fail");

        builder.Services.AddRegisterRoutes();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Middlewares
        app.UseMiddleware<ResponseWrapperMiddleware>();
        app.UseRegisterRoutes();

        app.UseOutputCache();

        app.Run();
    }
}
