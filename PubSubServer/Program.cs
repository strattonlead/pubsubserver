using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using PubSubServer.Hubs;
using PubSubServer.Services;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddSignalR();
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

bool.TryParse(Environment.GetEnvironmentVariable("DISABLE_AUTHENTICATION") ?? "false", out var disableAuthentication);

if (!disableAuthentication)
{
    var multiAuthScheme = "MULTI_AUTH_SCHEME";
    // JwtBearerDefaults.AuthenticationScheme
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = multiAuthScheme;
        options.DefaultChallengeScheme = multiAuthScheme;
    }).AddPolicyScheme(multiAuthScheme, "Multi Auth Scheme", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            if (context.Request.Cookies.Any(x => x.Key == Constants.Authentication.PIN_COOKIE_NAME))
            {
                return Constants.Authentication.PIN_AUTHENTICATION_SCHEME;
            }

            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    });

    //.AddJwtBearer("Bearer", options =>
    //{
    //    options.Authority = "https://keycloak.createif-ds.de/auth/realms/ds-test";
    //    options.Audience = "app_client";
    //    options.RequireHttpsMetadata = false;
    //    options.TokenValidationParameters = new TokenValidationParameters
    //    {
    //        ValidateIssuer = false,
    //        ValidateAudience = false,
    //        ValidateLifetime = false,
    //        ValidateIssuerSigningKey = false,
    //        ValidIssuer = "https://keycloak.createif-ds.de/auth/realms/ds-test",
    //        ValidAudience = "app_client",
    //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("UuPnPrndyI57dKZs0p9vZvXa8dFjeJVu"))
    //    };

    //    //options.Events = new JwtBearerEvents
    //    //{
    //    //    OnMessageReceived = context =>
    //    //    {
    //    //        var accessToken = context.Request.Query["access_token"];

    //    //        // If the request is for our hub...
    //    //        var path = context.HttpContext.Request.Path;
    //    //        if (!string.IsNullOrEmpty(accessToken) &&
    //    //            (path.StartsWithSegments("/hubs/chat")))
    //    //        {
    //    //            // Read the token out of the query string
    //    //            context.Token = accessToken;
    //    //        }
    //    //        return Task.CompletedTask;
    //    //    }
    //    //};
    //});
}


var app = builder.Build();

app.UseHttpsRedirection();
if (!disableAuthentication)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapGet("/", () => "Hello World!");
app.MapGet("/public", () => "Public Hello World!")
    .AllowAnonymous();
app.MapGet("/private", () => "Private Hello World!")
    .RequireAuthorization();

app.MapPost("/tokens/connect", (HttpContext ctx, JwtOptions jwtOptions)
    => TokenEndpoint.Connect(ctx, jwtOptions));

app.MapGet("/", async (ctx) =>
{
    var authResult = await ctx.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
    if (authResult?.Succeeded != true)
    {

    }

    // Get the access token and refresh token
    var accessToken = authResult.Properties.GetTokenValue("access_token");
    var refreshToken = authResult.Properties.GetTokenValue("refresh_token");
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<PubSubHub>("/pubsub");

app.Run(async (context) =>
{
    await context.Response.WriteAsync("PubSubServer running...");
});

app.Run();
