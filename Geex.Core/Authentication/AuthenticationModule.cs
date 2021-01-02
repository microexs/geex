﻿using Geex.Shared;
using Geex.Shared._ShouldMigrateToLib.Auth;
using Geex.Shared._ShouldMigrateToLib;

using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

using Volo.Abp.Modularity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using Geex.Core.Authentication.Domain;
using Geex.Core.Authentication.Utils;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.AspNetCore.Security.Claims;
using Volo.Abp.Uow;

namespace Geex.Core.Authentication
{
    [DependsOn(
    )]
    public class AuthenticationModule : GraphQLModule<AuthenticationModule>
    {
        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            base.OnPreApplicationInitialization(context);
        }

        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            base.PostConfigureServices(context);
        }

        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            IdentityModelEventSource.ShowPII = true;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            var services = context.Services;
            var configuration = services.GetConfiguration();
            services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // 签名键必须匹配!
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:JwtBearer:SecurityKey"])),

                        // 验证JWT发行者(iss)的 claim
                        ValidateIssuer = true,
                        ValidIssuer = configuration.GetAppName(),

                        // Validate the JWT Audience (aud) claim
                        ValidateAudience = true,
                        ValidAudience = configuration.GetAppName(),

                        // 验证过期
                        ValidateLifetime = true,

                        // If you want to allow a certain amount of clock drift, set that here
                        ClockSkew = TimeSpan.Zero
                    };
                    options.SecurityTokenValidators.Clear();
                    options.SecurityTokenValidators.Add(new GeexJwtSecurityTokenHandler());
                    if (options.Events != null)
                    {
                        options.Events.OnMessageReceived = receivedContext => { return Task.CompletedTask; };
                        options.Events.OnAuthenticationFailed = receivedContext => { return Task.CompletedTask; };
                        options.Events.OnChallenge = receivedContext => { return Task.CompletedTask; };
                        options.Events.OnForbidden = receivedContext => { return Task.CompletedTask; };
                        options.Events.OnTokenValidated = receivedContext => { return Task.CompletedTask; };
                    };
                });
            services.AddSingleton(new UserTokenGenerateOptions(configuration.GetAppName(), configuration.GetAppName(), configuration["Authentication:JwtBearer:SecurityKey"], TimeSpan.FromMinutes(10000)));
            base.ConfigureServices(context);
        }

        public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            base.OnPostApplicationInitialization(context);
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            app.UseAuthentication();
            base.OnApplicationInitialization(context);
        }
    }
}