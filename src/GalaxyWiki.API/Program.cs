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
    // First try to get connection string from environment variables
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
    
    // Fall back to configuration if environment variable is not set
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    }

    return Fluently.Configure()
        .Database(PostgreSQLConfiguration.Standard
            .ConnectionString(connectionString)
            .ShowSql())
        .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UsersMap>())
        // .ExposeConfiguration(cfg => {
        //     // new SchemaUpdate(cfg).Execute(false, true)

        //     // cfg.SetNamingStrategy(new SnakeCaseNamingStrategy());

        //     // print mappings
        //     Console.WriteLine("========== MAPPINGS ==========");
        //     foreach (var persistentClass in cfg.ClassMappings) {
        //         Console.WriteLine($"Entity: {persistentClass.EntityName} => Table: {persistentClass.Table.Name}");
        //         foreach (var property in persistentClass.PropertyIterator) {
        //             var columns = property.ColumnIterator.Cast<NHibernate.Mapping.Column>().Select(c => c.Name).ToList();
        //             Console.WriteLine($"  Property: {property.Name} => Columns: {string.Join(", ", columns)}");
        //         }
        //     }
        // })
        .BuildSessionFactory();
});

builder.Services.AddScoped<NHibernate.ISession>(provider =>
    provider.GetRequiredService<ISessionFactory>().OpenSession());

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<ContentRevisionRepository>();
builder.Services.AddScoped<CommentRepository>();
builder.Services.AddScoped<CelestialBodyRepository>();
builder.Services.AddScoped<BodyTypeRepository>();
builder.Services.AddScoped<StarSystemRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ContentRevisionService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<StarSystemService>();
builder.Services.AddScoped<CelestialBodyService>();


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


class AutomappingConfiguration : DefaultAutomappingConfiguration {

    public override bool ShouldMap(Type type)
    {
        return type.Namespace == "GalaxyWiki.Core.Entities.Mappings";
    }
}