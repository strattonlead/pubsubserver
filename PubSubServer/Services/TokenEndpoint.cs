﻿using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PubSubServer.Services
{
    public static class TokenEndpoint
    {
        public static async Task<IResult> Connect(HttpContext ctx, JwtOptions jwtOptions)
        {
            if (ctx.Request.ContentType != "application/x-www-form-urlencoded")
            {
                return Results.BadRequest(new { Error = "Invalid Request" });
            }

            var formCollection = await ctx.Request.ReadFormAsync();

            // pulls information from the form
            if (formCollection.TryGetValue("grant_type", out var grantType) == false)
            {
                return Results.BadRequest(new { Error = "Invalid Request" });
            }

            if (formCollection.TryGetValue("username", out var userName) == false)
            {
                return Results.BadRequest(new { Error = "Invalid Request" });
            }

            if (formCollection.TryGetValue("password", out var password) == false)
            {
                return Results.BadRequest(new { Error = "Invalid Request" });
            }

            var tokenExpiration = TimeSpan.FromSeconds(jwtOptions.ExpirationSeconds);
            var accessToken = TokenEndpoint.CreateAccessToken(
                jwtOptions,
                userName,
                TimeSpan.FromMinutes(60),
                new[] { "read_todo", "create_todo" });

            return Results.Ok(new
            {
                access_token = accessToken,
                expiration = (int)tokenExpiration.TotalSeconds,
                type = "bearer"
            });
        }

        public static string CreateAccessToken(JwtOptions jwtOptions, string username, TimeSpan expiration, string[] permissions)
        {
            var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);
            var symmetricKey = new SymmetricSecurityKey(keyBytes);

            var signingCredentials = new SigningCredentials(
                symmetricKey,
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim("sub", username),
                new Claim("name", username),
                new Claim("aud", jwtOptions.Audience)
            };

            var roleClaims = permissions.Select(x => new Claim("role", x));
            claims.AddRange(roleClaims);

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                expires: DateTime.Now.Add(expiration),
                signingCredentials: signingCredentials);

            var rawToken = new JwtSecurityTokenHandler().WriteToken(token);
            return rawToken;
        }
    }
}
