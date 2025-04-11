using Microsoft.AspNetCore.Authentication.JwtBearer;
using GalaxyWiki.Application.Services;
using dotenv.net;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using FluentNHibernate.Automapping;
using System.Net.Security;
using GalaxyWiki.Core.Entities;

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
        .Mappings(m => m.AutoMappings
            .Add(AutoMap.AssemblyOf<BodyType>())                        // body_types
            .Add(AutoMap.AssemblyOf<CelestialBody>())                   // celestial_bodies
            .Add(AutoMap.AssemblyOf<Comment>())                         // comments
            .Add(AutoMap.AssemblyOf<ContentRevision>())                 // content_revisions
            .Add(AutoMap.AssemblyOf<Role>())                            // roles
            .Add(AutoMap.AssemblyOf<StarSystem>())                      // star_systems
            .Add(AutoMap.AssemblyOf<User>())                            // users
        )
        .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
        .BuildSessionFactory();
});

builder.Services.AddScoped<NHibernate.ISession>(provider =>
    provider.GetRequiredService<ISessionFactory>().OpenSession());

builder.Services.AddScoped<ContentRevisionService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://accounts.google.com",
            ValidateAudience = true,
            ValidAudiences = new[] { Environment.GetEnvironmentVariable("CLIENT_ID") },
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

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