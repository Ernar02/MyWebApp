using System.ComponentModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApp.Abstract;
using WebApp.Service;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication
              (CookieAuthenticationDefaults.AuthenticationScheme)
                  .AddCookie(options =>
                  {
                      options.LoginPath = "/Auth/Login";
                      options.LogoutPath = "/Auth/Logout";
                      options.ExpireTimeSpan = TimeSpan.FromHours(2);
                      options.SlidingExpiration = true;
                  });


            // Add services to the container.

            builder.Services.AddScoped<IUser, UserService>();
            builder.Services.AddScoped<CheckUserStatusFilter>();

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.AddService<CheckUserStatusFilter>();
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (!app.Environment.IsDevelopment())
            //{
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            //}

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}");

            app.Run();
        }
    }
}


