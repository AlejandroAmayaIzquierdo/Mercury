using System.Text;
using Mercury.Db;
using Mercury.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Mercury API", Version = "v1" }
            );

            // Add JWT Authentication to Swagger
            options.AddSecurityDefinition(
                "Bearer",
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description =
                        "Enter 'your valid token in the text input below.\nExample: \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
                }
            );

            options.AddSecurityRequirement(
                new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            );
        });

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
        builder.Services.AddScoped<JWTHandler>();

        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWTSecurity:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWTSecurity:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JWTSecurity:Token"]!)
                    )
                };
            });
        builder.Services.AddAuthorization();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Middlewares
        app.UseMiddleware<ResponseWrapperMiddleware>();
        app.UseRegisterRoutes();

        app.UseAuthorization();

        app.UseOutputCache();

        app.Run();
    }
}
