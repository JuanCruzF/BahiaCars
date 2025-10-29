using System.Text;

using Catalog.Application.Contracts.Persistence;

using Catalog.Infrastructure.Persistence;

using Catalog.Infrastructure.Persistence.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;

using StackExchange.Redis;



var builder = WebApplication.CreateBuilder(args);

// Configurar logging para debug
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddCors(options =>

{

    options.AddPolicy("AllowAll", policyBuilder =>

    {

        policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();

    });

});

builder.Services.AddDbContext<CatalogDbContext>(options =>

    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

    .AddJwtBearer(options =>

    {

        options.TokenValidationParameters = new TokenValidationParameters

        {

            ValidateIssuer = true,

            ValidateAudience = true,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],

            ValidAudience = builder.Configuration["JwtSettings:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),

            ClockSkew = TimeSpan.FromSeconds(30)

        };

        // DEBUG: Log de eventos de autenticación JWT

        options.Events = new JwtBearerEvents

        {

            OnAuthenticationFailed = context =>

            {

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogError($"Auth Failed: {context.Exception.Message}");

                logger.LogError($"Exception: {context.Exception}");

                logger.LogError($"Token received: {context.Request.Headers["Authorization"]}");

                return Task.CompletedTask;

            },

            OnTokenValidated = context =>

            {

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Token validado correctamente");

                return Task.CompletedTask;

            },

            OnChallenge = context =>

            {

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogWarning($"Challenge: {context.ErrorDescription}");

                logger.LogWarning($"Challenge Details: {context.ErrorUri}");

                return Task.CompletedTask;

            },

            OnForbidden = context =>

            {

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogWarning($"Forbidden: {context.Result?.Failure?.Message}");

                return Task.CompletedTask;

            }

        };

    });

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>

    ConnectionMultiplexer.Connect("redis:6379, abortConnect=false"));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

builder.Services.AddEndpointsApiExplorer();




var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();


// MIDDLEWARE DE DEBUG - Loguear TODAS las requests
app.Use(async (context, next) =>

{

    var log = context.RequestServices.GetRequiredService<ILogger<Program>>();

    log.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");



    if (context.Request.Headers.ContainsKey("Authorization"))

    {

        log.LogInformation($"Authorization header found: {context.Request.Headers["Authorization"]}");

    }

    else

    {

        log.LogWarning("NO Authorization header found");

    }



    await next();

});

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();
foreach (var endpoint in endpointDataSource.Endpoints)
{
    if (endpoint is RouteEndpoint routeEndpoint)
    {
        logger.LogInformation($"Registered endpoint: {routeEndpoint.RoutePattern.RawText}");
    }
}

using (var scope = app.Services.CreateScope())

{

    var services = scope.ServiceProvider;

    try

    {

        var context = services.GetRequiredService<CatalogDbContext>();

        context.Database.Migrate();

        logger.LogInformation("Database migrations applied successfully");

    }

    catch (Exception ex)

    {

        var dbLogger = services.GetRequiredService<ILogger<Program>>();

        dbLogger.LogError(ex, "Error applying migrations");

    }

}

logger.LogInformation("Application started");

app.Run();