using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyRestful.Infrastructure;
using Serilog;

namespace MyRestful.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "My Restful API";
            var host = CreateWebHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var salesContext = services.GetRequiredService<MyContext>();
                    MyContextSeed.SeedAsync(salesContext, loggerFactory).Wait();
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .UseUrls("http://localhost:5000")
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
