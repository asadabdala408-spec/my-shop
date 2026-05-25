using System.Text.Json.Serialization;
using Jewelryshop.Api.Data;
using Jewelryshop.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var connectionString = ResolveConnectionString(builder.Configuration);
var databaseConfigured = !string.IsNullOrWhiteSpace(connectionString);

Console.WriteLine(
    $"[startup] PORT={port}, DATABASE_URL={EnvSet("DATABASE_URL")}, PGHOST={EnvSet("PGHOST")}, databaseConfigured={databaseConfigured}");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var configuredOrigins = builder.Configuration["Cors:AllowedOrigins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin))
                {
                    return false;
                }

                if (configuredOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                    && uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

if (databaseConfigured)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString!));
}

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<CloudinaryService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"]
            ?? "replace-this-with-a-long-secure-secret-key-that-is-at-least-32-characters";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.Logger.LogInformation(
    "Listening on http://0.0.0.0:{Port}, database configured: {DatabaseConfigured}, CORS: {Origins}",
    port,
    databaseConfigured,
    string.Join(", ", configuredOrigins));

if (!databaseConfigured)
{
    app.Logger.LogWarning(
        "DATABASE_URL is missing or invalid. Railway: API service → Variables → Add Reference → PostgreSQL → DATABASE_URL");
}

app.UseCors("Frontend");

app.MapGet("/health", () => Results.Ok(new { status = "ok", databaseConfigured }));

app.MapGet("/health/config", () => Results.Ok(new
{
    databaseConfigured,
    port,
    env = new
    {
        DATABASE_URL = EnvSet("DATABASE_URL"),
        DATABASE_PRIVATE_URL = EnvSet("DATABASE_PRIVATE_URL"),
        DATABASE_PUBLIC_URL = EnvSet("DATABASE_PUBLIC_URL"),
        PGHOST = EnvSet("PGHOST"),
        PGDATABASE = EnvSet("PGDATABASE"),
        connectionStringsDefault = EnvSet("ConnectionStrings__DefaultConnection")
    }
}));

app.MapGet("/health/db", async (IServiceProvider services) =>
{
    if (!databaseConfigured)
    {
        return Results.Json(
            new
            {
                status = "error",
                message = "DATABASE_URL or ConnectionStrings__DefaultConnection is not set. Railway: Variables → Add Reference → PostgreSQL → DATABASE_URL"
            },
            statusCode: 503);
    }

    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.OpenConnectionAsync();
        await db.Database.CloseConnectionAsync();

        var categories = await db.Categories.CountAsync();
        var products = await db.Products.CountAsync();
        return Results.Ok(new { status = "ok", categories, products });
    }
    catch (Exception ex)
    {
        var detail = ex.InnerException?.Message ?? ex.Message;
        return Results.Json(new { status = "error", message = detail }, statusCode: 503);
    }
});

if (databaseConfigured)
{
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
                await DatabaseSeeder.SeedAsync(app.Services);
                app.Logger.LogInformation("Database migrated and seeded.");
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Database migration or seeding failed.");
            }
        });
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static string EnvSet(string name) =>
    string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)) ? "missing" : "set";

static string? ResolveConnectionString(IConfiguration configuration)
{
    var candidates = new[]
    {
        configuration.GetConnectionString("DefaultConnection"),
        configuration["DATABASE_URL"],
        configuration["DATABASE_PRIVATE_URL"],
        configuration["DATABASE_PUBLIC_URL"],
        configuration["POSTGRES_URL"],
        Environment.GetEnvironmentVariable("DATABASE_URL"),
        Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL"),
        Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL"),
        Environment.GetEnvironmentVariable("POSTGRES_URL"),
        BuildFromPgParts(configuration)
    };

    foreach (var candidate in candidates)
    {
        var normalized = NormalizeConnectionString(candidate);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            return normalized;
        }
    }

    return null;
}

static string? BuildFromPgParts(IConfiguration configuration)
{
    var host = configuration["PGHOST"] ?? Environment.GetEnvironmentVariable("PGHOST");
    var database = configuration["PGDATABASE"] ?? Environment.GetEnvironmentVariable("PGDATABASE");
    var user = configuration["PGUSER"] ?? Environment.GetEnvironmentVariable("PGUSER");
    var password = configuration["PGPASSWORD"] ?? Environment.GetEnvironmentVariable("PGPASSWORD");
    var port = configuration["PGPORT"] ?? Environment.GetEnvironmentVariable("PGPORT") ?? "5432";

    if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(database) || string.IsNullOrWhiteSpace(user))
    {
        return null;
    }

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require";
}

static string? NormalizeConnectionString(string? raw)
{
    if (string.IsNullOrWhiteSpace(raw))
    {
        return null;
    }

    // Npgsql does not need channel_binding; it can break Railway/Neon connections.
    raw = System.Text.RegularExpressions.Regex.Replace(
        raw,
        @"[&?]channel_binding=[^&]*",
        string.Empty,
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    if (raw.Contains("your-neon", StringComparison.OrdinalIgnoreCase)
        || raw.Contains("your_user", StringComparison.OrdinalIgnoreCase)
        || raw.Contains("your_password", StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return TryBuildFromPostgresUri(raw);
    }

    try
    {
        var csb = new NpgsqlConnectionStringBuilder(raw)
        {
            SslMode = SslMode.Require
        };

        if (string.IsNullOrWhiteSpace(csb.Host))
        {
            return null;
        }

        return csb.ConnectionString;
    }
    catch
    {
        return null;
    }
}

static string? TryBuildFromPostgresUri(string uri)
{
    try
    {
        var parsed = new Uri(uri);
        var userInfo = parsed.UserInfo.Split(':', 2);
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = parsed.AbsolutePath.TrimStart('/');
        if (string.IsNullOrWhiteSpace(database))
        {
            database = "railway";
        }

        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = parsed.Host,
            Port = parsed.Port > 0 ? parsed.Port : 5432,
            Database = database,
            Username = username,
            Password = password,
            SslMode = SslMode.Require
        };

        if (string.IsNullOrWhiteSpace(csb.Host))
        {
            return null;
        }

        return csb.ConnectionString;
    }
    catch
    {
        return null;
    }
}
