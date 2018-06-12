using System;
using System.IO;
using System.Linq;
using System.Text;
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyRestful.Api.Validators;
using MyRestful.Core.Interfaces;
using MyRestful.Infrastructure;
using MyRestful.Infrastructure.Extensions;
using MyRestful.Infrastructure.Repositories;
using MyRestful.Infrastructure.Resources;
using MyRestful.Infrastructure.Services;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;

namespace MyRestful.Api
{
    public class Startup
    {
        private readonly ILoggerFactory _loggerFactory;
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", @"log.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper();
            services.AddDbContext<MyContext>(options =>
            {
                options.UseInMemoryDatabase("MyDatabase");
                options.UseLoggerFactory(_loggerFactory);
            });
            services.AddMvc(options =>
            {
                options.ReturnHttpNotAcceptable = true;
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                var jsonInputFormatter = options.OutputFormatters.OfType<JsonInputFormatter>().FirstOrDefault();
                jsonInputFormatter?.SupportedMediaTypes.Add("application/vnd.mycompany.country.create+json");
                jsonInputFormatter?.SupportedMediaTypes.Add("application/vnd.mycompany.countrywithcontinent.create+json");

                var jsonOutputFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                jsonOutputFormatter?.SupportedMediaTypes.Add("application/vnd.mycompany.hateoas+json");
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            })
            .AddFluentValidation();

            services.AddTransient<IValidator<CityAddResource>, CityAddOrUpdateResourceValidator<CityAddResource>>();
            services.AddTransient<IValidator<CityUpdateResource>, CityUpdateResourceValidator>();
            services.AddTransient<IValidator<CountryAddResource>, CountryAddResourceValidator>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>()
                    .ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddPropertyMappings();
            services.AddTransient<ITypeHelperService, TypeHelperService>();

            services.AddHttpCacheHeaders(
                expirationModelOptions =>
                {
                    expirationModelOptions.MaxAge = 600;

                },
                validationModelOptions =>
                {
                    validationModelOptions.AddMustRevalidate = true;
                });

            services.AddResponseCaching();

            // services.AddMvc()
            //     .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        var serverSecret = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(Configuration["JWT:ServerSecret"]));

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = serverSecret,
                            ValidIssuer = Configuration["JWT:Issuer"],
                            ValidAudience = Configuration["JWT:Audience"]
                        };
                    });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
                options.ExcludedHosts.Add("example.com");
                options.ExcludedHosts.Add("www.example.com");
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
                options.HttpsPort = 5001;
            });

            services.AddCors();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://example.com"));
            });

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowSpecificOrigin"));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(builder => builder
                .WithOrigins("http://example.com")
                .AllowAnyHeader());

            app.UseCors("AllowSpecificOrigin");

            app.UseStatusCodePages();

            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandlerFeature != null)
                    {
                        var logger = _loggerFactory.CreateLogger("Global Exception Logger");
                        logger.LogError(500, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                    }
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(exceptionHandlerFeature?.Error?.Message ?? "An Error Occurred.");
                });
            });

            app.UseHsts();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseResponseCaching();
            app.UseHttpCacheHeaders();

            app.UseMvc();
        }
    }
}
