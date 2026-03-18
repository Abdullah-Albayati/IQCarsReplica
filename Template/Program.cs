using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Template.Data;
using Template.Profiles;
using Template.Services;

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
    builder.Logging.AddConsole();
}

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<ITokenServices, TokenServices>();
builder.Services.AddScoped<IUserServices, UserServices>();

// AutoMapper
builder.Services.AddAutoMapper(_ => { }, typeof(MappingProfile).Assembly);

// Swagger (Dev only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Base API Template", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        }
    });
});

// Get database connection string (prioritize DATABASE_URL from Dokploy/Docker)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

// Database (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// CORS
builder.Services.AddCors(options =>
{
    var cors = builder.Configuration.GetSection("Cors");
    var allowedOrigins = cors.GetSection("AllowedOrigins").Get<string[]>();
    var allowedMethods = cors.GetSection("AllowedMethods").Get<string[]>();
    var allowedHeaders = cors.GetSection("AllowedHeaders").Get<string[]>();

    options.AddPolicy(
        "DefaultCors",
        policy =>
        {
            if (allowedOrigins is { Length: > 0 })
            {
                policy.WithOrigins(allowedOrigins);
            }
            else
            {
                policy.AllowAnyOrigin();
            }

            if (allowedMethods is { Length: > 0 })
            {
                policy.WithMethods(allowedMethods);
            }
            else
            {
                policy.AllowAnyMethod();
            }

            if (allowedHeaders is { Length: > 0 })
            {
                policy.WithHeaders(allowedHeaders);
            }
            else
            {
                policy.AllowAnyHeader();
            }
        }
    );
});

// Utilities
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

var jwtToken = builder.Configuration["AppSettings:Token"];
if (string.IsNullOrWhiteSpace(jwtToken))
{
    throw new InvalidOperationException("JWT token signing key (AppSettings:Token) is not configured.");
}

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtToken)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        app.Logger.LogInformation("Applying database migrations...");
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to apply database migrations.");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Base API Template v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Logger.LogInformation("Base API template started ({env})", app.Environment.EnvironmentName);

app.Run();
