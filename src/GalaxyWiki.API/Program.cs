using Microsoft.AspNetCore.Authentication.JwtBearer;
using GalaxyWiki.Application.Services;
using dotenv.net;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using GalaxyWiki.Models;
using GalaxyWiki.Mappings;

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
        .Mappings(m => m.FluentMappings
            .AddFromAssemblyOf<CelestialBodyMap>())
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
