using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;

namespace HomePage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
            builder.Services.AddExceptionHandler<CustomExceptionHandler>();

            /*builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8001);
                options.ListenAnyIP(8000, listenOptions =>
                {
                    listenOptions.UseHttps("Certs/HomePage.pfx", "admin");
                });
            });*/

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseExceptionHandler("/Error");
            if (!app.Environment.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Pictures")),
                    RequestPath = "/Pictures"
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Backups")),
                    RequestPath = "/Backups"
            });

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
