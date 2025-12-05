using HomePage.Chores;
using HomePage.Data;
using HomePage.Repositories;
using HomePage.Spending;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace HomePage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<BruteForceProtector>();
            builder.Services.AddTransient<CurrentWordMixRepository>();
            builder.Services.AddTransient<WordMixResultRepository>();
            builder.Services.AddTransient<FoodStorageRepository>();
            builder.Services.AddTransient<IngredientRepository>();
            builder.Services.AddTransient<FoodRepository>();
            builder.Services.AddTransient<DayFoodRepository>();
            builder.Services.AddTransient<RedDayRepository>();
            builder.Services.AddTransient<SpendingGroupRepository>();
            builder.Services.AddTransient<ThemeDayRepository>();
            builder.Services.AddTransient<SignInRepository>();
            builder.Services.AddTransient<SettingsRepository>();
            builder.Services.AddTransient<DatabaseLogger>();
            builder.Services.AddTransient<ChoreRepository>();
            builder.Services.AddRazorPages();

            builder.Services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
            builder.Services.AddExceptionHandler<CustomExceptionHandler>();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

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

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
