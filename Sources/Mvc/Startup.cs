using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using LinkedInPoc.Mvc.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mmu.Mlh.ServiceProvisioning.Areas.Initialization.Models;
using Mmu.Mlh.ServiceProvisioning.Areas.Initialization.Services;
using Mmu.Mlh.ApplicationExtensions.Areas.Dropbox.Services;
using LinkedInPoc.Mvc.Services;

namespace LinkedInPoc.Mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureContainer(ServiceRegistry registry)
        {
            var containerConfig = ContainerConfiguration.CreateFromAssembly(typeof(Startup).Assembly);
            ServiceProvisioningInitializer.PopulateRegistry(containerConfig, registry);

            var dropboxLocator = new Container(registry).GetService<IDropboxLocator>();
            var dropboxPath = dropboxLocator.LocateDropboxPath();

            var completePath = Path.Combine(dropboxPath, "Apps", "LinkedInPoc", "Secrets.txt");
            var textLines = File.ReadAllLines(completePath);

            registry.AddAuthentication(
                    options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = "LinkedIn";
                    })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddLinkedIn("LinkedIn", options =>
                {
                    options.ClientId = textLines[0];
                    options.ClientSecret = textLines[1];
                    options.CallbackPath = new PathString("/signin-linkedin");

                    options.SaveTokens = true;
                    options.Scope.Clear();
                    options.Scope.Add("r_liteprofile");
                    options.Scope.Add("r_emailaddress");
                    options.Scope.Add("w_member_social");

                    options.Events.OnCreatingTicket = ticket =>
                    {
                        // For some reason, HttpContext.GetTokenAsync("access_token") doesn't work;
                        LinkedInAccessTokenSingleton.Value = ticket.AccessToken;
                        return Task.CompletedTask;
                    };
                });

            registry.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            registry.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            registry.AddControllersWithViews();
            registry.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
