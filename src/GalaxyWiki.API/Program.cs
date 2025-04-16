using Microsoft.AspNetCore.Authentication.JwtBearer;
using dotenv.net;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using FluentNHibernate.Automapping;
using GalaxyWiki.API.Services;
using GalaxyWiki.Api.Repositories;
using GalaxyWiki.Api.Services;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configure NHibernate
builder.Services.AddSingleton<ISessionFactory>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    return Fluently.Configure()
        .Database(PostgreSQLConfiguration.Standard
            .ConnectionString(connectionString)
            .ShowSql())
        .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UsersMap>())
        .BuildSessionFactory();
});

builder.Services.AddScoped<NHibernate.ISession>(provider =>
    provider.GetRequiredService<ISessionFactory>().OpenSession());

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();  // Interface-based
builder.Services.AddScoped<UserRepository>();  // Concrete class registration
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<IContentRevisionRepository, ContentRevisionRepository>();
builder.Services.AddScoped<CommentRepository>();
builder.Services.AddScoped<ICelestialBodyRepository, CelestialBodyRepository>();
builder.Services.AddScoped<BodyTypeRepository>();
builder.Services.AddScoped<StarSystemRepository>();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContentRevisionService, ContentRevisionService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IStarSystemService, StarSystemService>();
builder.Services.AddScoped<ICelestialBodyService, CelestialBodyService>();

// JWT Authentication Configuration
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://accounts.google.com",
            ValidateAudience = true,
            ValidAudiences = [Environment.GetEnvironmentVariable("CLIENT_ID")],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Remove eventually
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

class AutomappingConfiguration : DefaultAutomappingConfiguration
{
    public override bool ShouldMap(Type type)
    {
        return type.Namespace == "GalaxyWiki.Core.Entities.Mappings";
    }
}
