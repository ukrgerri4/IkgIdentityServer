using EscapeDungeonIdentityWeb;
using IdentityServer4.Models;
using Ids.Web.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EscapeDungeonIdentity
{
    public class Startup
    {
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.environment = environment;
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // ===== Manage Connection String For SqLite ========
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            string executable = Assembly.GetExecutingAssembly().Location;
            string dbPath = Path.Combine(Path.GetDirectoryName(executable), "db_data");
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }
            connectionString = connectionString.Replace("{db_path}", dbPath);
            // ===== Manage Connection String For SqLite ========

            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services
                .AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(connectionString, b => b.MigrationsAssembly(migrationAssembly));
                    options.EnableSensitiveDataLogging(environment.IsDevelopment());
                });

            services
                .AddIdentity<IdentityUser, IdentityRole>(config =>
                {

                    /* TODO: remove in production*/
                    /* FOR DEVELOPMENT ONLY */
                    config.Password.RequiredLength = 3;
                    config.Password.RequireNonAlphanumeric = false;
                    config.Password.RequireLowercase = false;
                    config.Password.RequireDigit = false;
                    config.Password.RequireUppercase = false;
                    /* TODO: remove in production*/

                    config.User.RequireUniqueEmail = true;
                    config.SignIn.RequireConfirmedEmail = false;
                    config.Lockout.MaxFailedAccessAttempts = int.MaxValue;
                    config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                })
                .AddEntityFrameworkStores<AppDbContext>();

            services
                .AddIdentityServer()
                .AddAspNetIdentity<IdentityUser>()
                .AddConfigurationStore(options =>
                    options.ConfigureDbContext = 
                        builder => builder.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationAssembly))
                )
                .AddOperationalStore(options =>
                    options.ConfigureDbContext =
                        builder => builder.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationAssembly))
                )
                .AddDeveloperSigningCredential();

            services
                .AddControllersWithViews();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
