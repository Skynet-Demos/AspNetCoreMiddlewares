using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiddlewareDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});

            app.UseWhen(context => context.Request.Query.ContainsKey("employee"), HandleEmployee);

            app.MapWhen(context => context.Request.Query.ContainsKey("customer"), HandleCustomer);

            app.Map("/map1", HandleMap1);

            app.Map("/map2/level1", HandleMultiLevelMap2);

            app.Map("/map3", map3App =>
            {
                map3App.Map("/level1", level1App =>
                {
                    level1App.Run(async context =>
                    {
                        await context.Response.WriteAsync("Map 3 - Level 1");
                    });
                });

                map3App.Map("/level2", HandleMap3Level2);
            });

            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("Hello, from Skynet - Middleware 1\n");
                await next.Invoke();
            });

            app.Run(async context =>
            {
                await context.Response.WriteAsync("I'm from Middleware 2");
            });
        }

        private static void HandleMap1(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await context.Response.WriteAsync("I come, whenever 'map1' appears in the url");
            });
        }

        private static void HandleMultiLevelMap2(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await context.Response.WriteAsync("I'm from multilevel Map2");
            });
        }

        private static void HandleMap3Level2(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await context.Response.WriteAsync("I'm from Map3 - Level 2");
            });
        }

        private static void HandleCustomer(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var customer = context.Request.Query["customer"];
                await context.Response.WriteAsync($"Customer is, {customer}");
                await next.Invoke();
            });
        }

        private static void HandleEmployee(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var employee = context.Request.Query["employee"];
                await context.Response.WriteAsync($"Employee is, {employee}\n");
                await next.Invoke();
            });

            app.Run(async context =>
            {
                await context.Response.WriteAsync("UseWhen - Terminating here itself using RUN");
            });
        }
    }
}
