using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PubSubServer.Hubs;
using PubSubServer.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddSignalR();

var applicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME");
bool.TryParse(Environment.GetEnvironmentVariable("USE_DATABASE"), out var useDatabase);
bool.TryParse(Environment.GetEnvironmentVariable("USE_DATA_PROTECTION"), out var useDataProtection);
bool.TryParse(Environment.GetEnvironmentVariable("USE_MSSQL"), out var useMsSql);

if (useDatabase)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (useMsSql)
        {
            var msSqlConnectionString = Environment.GetEnvironmentVariable("MSSQL_CONNECTION_STRING");
            options.UseSqlServer(msSqlConnectionString);
        }
    });
}

if (useDataProtection)
{
    builder.Services.AddDataProtection()
        .SetApplicationName(applicationName)
        .PersistKeysToDbContext<ApplicationDbContext>();
}


bool.TryParse(Environment.GetEnvironmentVariable("USE_AUTHENTICATION"), out var useAuthentication);
if (useAuthentication)
{
    var authenticationScheme = Environment.GetEnvironmentVariable("AUTHENTICATION_SCHEME");
    var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = authenticationScheme;
        options.DefaultChallengeScheme = authenticationScheme;
        options.DefaultScheme = authenticationScheme;
    });

    bool.TryParse(Environment.GetEnvironmentVariable("USE_COOKIE_AUTHENTICATION"), out var useCookieAuthentication);
    if (useCookieAuthentication)
    {
        authBuilder.AddCookie(authenticationScheme, options =>
        {
            bool.TryParse(Environment.GetEnvironmentVariable("SLIDING_EXPIRATION"), out var slidingExpiration);
            int.TryParse(Environment.GetEnvironmentVariable("SAME_SITE"), out var sameSite);
            var sameSiteMode = (SameSiteMode)sameSite;
            bool.TryParse(Environment.GetEnvironmentVariable("HTTP_ONLY"), out var httpOnly);
            int.TryParse(Environment.GetEnvironmentVariable("SECURE_POLICY"), out var securePolicy);
            var securePolicyMode = (CookieSecurePolicy)securePolicy;
            TimeSpan.TryParse(Environment.GetEnvironmentVariable("EXPIRE_TIME_SPAN"), out var expireTimeSpan);

            options.SlidingExpiration = slidingExpiration;
            options.Cookie.Name = Environment.GetEnvironmentVariable("COOKIE_NAME");
            options.Cookie.Domain = Environment.GetEnvironmentVariable("DOMAIN");
            options.Cookie.Path = Environment.GetEnvironmentVariable("PATH");
            options.Cookie.SameSite = sameSiteMode;
            options.Cookie.HttpOnly = httpOnly;
            options.Cookie.SecurePolicy = securePolicyMode;
            options.ExpireTimeSpan = expireTimeSpan;
        });
    }
}

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
//        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY"))),
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = false,
//        ValidateIssuerSigningKey = true
//    };
//});


//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//}).AddOpenIdConnect(options =>
//{
//    options.Authority = "https://keycloak.createif-ds.de/auth/realms/ds-test";
//    options.ClientId = "pubsubtest";
//    options.ClientSecret = "UuPnPrndyI57dKZs0p9vZvXa8dFjeJVu";
//    options.ResponseType = "code";
//    options.SaveTokens = true;
//    options.Scope.Add("openid");
//    options.Scope.Add("profile");
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        NameClaimType = "preferred_username",
//        RoleClaimType = "roles"
//    };
//});


var app = builder.Build();

app.UseHttpsRedirection();
if (useAuthentication)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapGet("/public", () => "Public Hello World!")
    .AllowAnonymous();
app.MapGet("/private", () => "Private Hello World!")
    .RequireAuthorization();

app.MapPost("/tokens/connect", (HttpContext ctx, JwtOptions jwtOptions)
    => TokenEndpoint.Connect(ctx, jwtOptions));

//app.MapGet("/", async (context) =>
//{
//    //var authResult = await ctx.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
//    //if (authResult?.Succeeded != true)
//    //{

//    //}

//    //// Get the access token and refresh token
//    //var accessToken = authResult.Properties.GetTokenValue("access_token");
//    //var refreshToken = authResult.Properties.GetTokenValue("refresh_token");
//    await context.Response.WriteAsync("PubSubServer running...");
//});

app.UseDefaultFiles();
app.UseStaticFiles();

if (useAuthentication)
{
    app.MapHub<PubSubHub>("/pubsub").RequireAuthorization();
}
else
{
    app.MapHub<PubSubHub>("/pubsub").AllowAnonymous();
}

//app.Run(async (context) =>
//{
//    await context.Response.WriteAsync("PubSubServer running...");
//});

app.Run();
