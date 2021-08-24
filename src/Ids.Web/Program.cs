using EscapeDungeonIdentityWeb;
using IdentityServer4.EntityFramework.DbContexts;
using Ids.Web.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace EscapeDungeonIdentity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{currentEnv}.json", optional: true, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", configuration["ApplicationName"] ?? "IdentityServer")
                .Enrich.WithProperty("Environment", currentEnv)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Warning("Starting up");
                var host = CreateHostBuilder(args).Build();

                using (var serviceScope = host.Services.CreateScope())
                {
                    serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                    var configurationContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                    configurationContext.Database.Migrate();
                    SeedDb.EnsureSeededConfigurationData(configurationContext);

                    var appContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
                    appContext.Database.Migrate();
                    SeedDb.EnsureSeededUsersData(serviceScope);
                }

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseSerilog(Log.Logger)
                        .UseStartup<Startup>();
                });
    }
}
