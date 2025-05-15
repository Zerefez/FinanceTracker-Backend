using FinanceTracker.DataAccess;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FinanceTrackerContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        (x) => $"The value '{x}' is invalid.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        (x) => $"The field {x} must be a number.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => $"A value is required.");

    options.CacheProfiles.Add("NoCache",
        new CacheProfile() { NoStore = true });
    options.CacheProfiles.Add("Any-60",
        new CacheProfile() { Location = ResponseCacheLocation.Any, Duration = 60 });
    options.CacheProfiles.Add("Any-1hour",
        new CacheProfile() { Location = ResponseCacheLocation.Any, Duration = 3600 });
    options.CacheProfiles.Add("Client-1day",
        new CacheProfile() { Location = ResponseCacheLocation.Client, Duration = 86400 });
});

builder.Services.AddScoped(typeof(IDataAccessService<>), typeof(DataAccessService<>));
builder.Services.AddIdentity<FinanceUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<FinanceTrackerContext>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});



builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        options.DefaultChallengeScheme =
            options.DefaultForbidScheme =
                options.DefaultScheme =
                    options.DefaultSignInScheme =
                        options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(
                builder.Configuration["JWT:SigningKey"]))
    };
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var context = serviceProvider.GetRequiredService<FinanceTrackerContext>();
    
    if (app.Environment.IsProduction())
    {
        try
        {
            // First check if the database exists, if not create it
            context.Database.EnsureCreated();
            
            // Now verify if tables exist by checking each required table
            bool tablesExist = true;
            
            try
            {
                // Try to query each of the main tables to check if they exist
                var aspNetUsersExist = context.Database.ExecuteSqlRaw("SELECT COUNT(*) FROM \"AspNetUsers\" LIMIT 1");
                var jobsExist = context.Database.ExecuteSqlRaw("SELECT COUNT(*) FROM \"Jobs\" LIMIT 1");
                var workshiftsExist = context.Database.ExecuteSqlRaw("SELECT COUNT(*) FROM \"WorkShifts\" LIMIT 1");
                var supplementsExist = context.Database.ExecuteSqlRaw("SELECT COUNT(*) FROM \"SupplementDetails\" LIMIT 1");
            }
            catch (Exception)
            {
                // If any query fails, some table doesn't exist
                tablesExist = false;
            }
            
            if (!tablesExist)
            {
                // If tables don't exist, recreate the database from scratch
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }
        catch (Exception ex)
        {
            // If any unexpected error occurs, log it and try one more time
            Console.WriteLine($"Database initialization error: {ex.Message}");
            
            // Last resort - just ensure the database is created
            context.Database.EnsureCreated();
        }
    }
    else
    {
        // In development, apply migrations as usual
        context.Database.Migrate();
    }
    
    try
    {
        // Only initialize with seed data after we've ensured the database is properly created
        Dbseeder.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seed data error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoint for Render
app.MapGet("/healthz", () => "Healthy");

app.Run();