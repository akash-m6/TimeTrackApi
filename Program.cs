using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TimeTrack.API.Data;
using TimeTrack.API.Repository.IRepository;
using TimeTrack.API.Repository;
using TimeTrack.API.Service;
using TimeTrack.API.Middleware;
using TimeTrack.API.Service.ServiceInterface;

// ENTRY POINT: Program.cs
// PURPOSE: Configures and starts the TimeTrack API backend application.

var builder = WebApplication.CreateBuilder(args);

// CONFIGURATION: Database, Dependency Injection, Authentication, Authorization, CORS, Swagger

// Database Configuration with resiliency
builder.Services.AddDbContext<TimeTrackDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("TimeTrackConnection");
    options.UseSqlServer(connStr, sql =>
    {
        sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        );
        sql.CommandTimeout(120);
    });
});

// REPOSITORY REGISTRATION: Registers all repository interfaces and implementations for DI.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITimeLogRepository, TimeLogRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskTimeRepository, TaskTimeRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPendingRegistrationRepository, PendingRegistrationRepository>();
builder.Services.AddScoped<IBreakRepository, BreakRepository>();

// SERVICE REGISTRATION: Registers all service interfaces and implementations for DI.
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITimeLoggingService, TimeLoggingService>();
builder.Services.AddScoped<ITaskManagementService, TaskManagementService>();
builder.Services.AddScoped<IProductivityAnalyticsService, ProductivityAnalyticsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IBreakService, BreakService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IProductivityService, ProductivityService>();

// Memory cache for caching productivity per-user
builder.Services.AddMemoryCache();

// AUTHENTICATION: Configures JWT authentication and token validation.
var jwtKey = builder.Configuration["JwtSettings:SecretKey"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json");
}

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

// AUTHORIZATION: Adds role-based authorization policies.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
});

// CONTROLLERS: Adds MVC controllers to the application.
builder.Services.AddControllers();

// SWAGGER: Configures Swagger/OpenAPI for API documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TimeTrack API",
        Version = "v1",
        Description = "Internal Time Logging & Productivity Monitoring System"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS: Configures Cross-Origin Resource Sharing for frontend integration.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// APPLICATION PIPELINE: Builds and configures the HTTP request pipeline.

// DATABASE MIGRATION & SEEDING: Applies migrations and seeds the database on startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<TimeTrackDbContext>();
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Check if database already has tables (created outside EF migrations)
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'";
            var tableExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

            if (tableExists)
            {
                logger.LogInformation("Existing database detected. Ensuring migration history is synchronized...");
                
                // Ensure migration history table exists and mark migrations as applied
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory')
                    BEGIN
                        CREATE TABLE [__EFMigrationsHistory] (
                            [MigrationId] nvarchar(150) NOT NULL,
                            [ProductVersion] nvarchar(32) NOT NULL,
                            CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                        );
                    END;

                    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260211093650_Initial')
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20260211093650_Initial', '9.0.0');

                    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260211124100_UpdateNullableFields')
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20260211124100_UpdateNullableFields', '9.0.0');
                ");
                
                logger.LogInformation("Migration history synchronized.");
            }
            else
            {
                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
            }

            logger.LogInformation("Seeding database...");
            await DatabaseSeeder.SeedAsync(context);
            logger.LogInformation("Database is ready!");
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// DEVELOPMENT: Enables Swagger UI in development environment.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// MIDDLEWARE: Adds global exception handler middleware.
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// SECURITY: Enforces HTTPS redirection.
app.UseHttpsRedirection();

// CORS: Enables CORS policy for frontend.
app.UseCors("AllowFrontend");

// AUTHENTICATION: Enables JWT authentication middleware.
app.UseAuthentication();

// AUTHORIZATION: Enables authorization middleware.
app.UseAuthorization();

// ROUTING: Maps controller endpoints.
app.MapControllers();

// RUN: Starts the web application.
app.Run();

