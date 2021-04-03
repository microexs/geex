﻿using System.Threading.Tasks;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.Modularity;
using Autofac.Extensions.DependencyInjection;
using Geex.Core.Authentication;
using Geex.Core.Authorization;
using Geex.Core.UserManagement;
using Microsoft.AspNetCore.Http;
using Geex.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Geex.Core.Users;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Entities;
using Volo.Abp.Auditing;
using Volo.Abp.Authorization;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Http;
using Volo.Abp.Security;
using Volo.Abp.Uow;
using Volo.Abp.Validation;
using Volo.Abp.VirtualFileSystem;

namespace Geex.Core
{
    [DependsOn(
        //typeof(AbpAuditingModule),
        //typeof(AbpSecurityModule),
        //typeof(AbpVirtualFileSystemModule),
        typeof(AbpUnitOfWorkModule),
        typeof(AbpHttpModule),
        //typeof(AbpAuthorizationModule),
        //typeof(AbpValidationModule),
        typeof(AbpExceptionHandlingModule),
        typeof(AuthenticationModule),
        typeof(AuthorizationModule),
        typeof(UserManagementModule)
        )]
    public class AppModule : GraphQLEntryModule<AppModule>
    {
        private IWebHostEnvironment _env;
        private IConfiguration _configuration;


        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            //this.Configure<AbpAuditingOptions>((Action<AbpAuditingOptions>)(options => options.Contributors.Add((AuditLogContributor)new AspNetCoreAuditLogContributor())));
            context.Services.AddMediatR(typeof(AppModule));
            context.Services.AddSingleton<IMediator>(x => new Mediator(x.GetService));
            context.Services.AddHttpContextAccessor();
            context.Services.AddObjectAccessor<IApplicationBuilder>();
            //context.Services.Replace(ServiceDescriptor.Transient<IOptionsFactory<RequestLocalizationOptions>, AbpRequestLocalizationOptionsFactory>());
            this._env = context.Services.GetSingletonInstance<IWebHostEnvironment>();
            this._configuration = context.Services.GetConfiguration();

            context.Services.AddHealthChecks();
            context.Services.AddCors(options =>
            {
                if (_env.IsDevelopment())
                {
                    options.AddDefaultPolicy(x => x.SetIsOriginAllowed(x => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
                }
            });
            base.ConfigureServices(context);
        }

        public override void OnApplicationInitialization(
            ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            this._env = context.GetEnvironment();
            this._configuration = context.GetConfiguration();

            app.UseCors();

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            });


            app.UseHealthChecks("/health-check");

            app.UseGeexGraphQL();
        }
    }
}