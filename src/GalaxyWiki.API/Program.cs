using Microsoft.AspNetCore.Authentication.JwtBearer;
using dotenv.net;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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
