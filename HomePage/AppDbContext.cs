using HomePage.Model;
using HomePage.Spending;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ToDoItem> ToDo { get; set; }

        public DbSet<CalendarActivity> CalendarActivity { get; set; }

        public DbSet<Category> Category { get; set; }

        public DbSet<ExtraWord> ExtraWord { get; set; }

        public DbSet<CurrentWordMix> CurrentWordMix { get; set; }

        public DbSet<ThemeDay> ThemeDay { get; set; }

        public DbSet<RedDay> RedDay { get; set; }

        public DbSet<WordMixResult> WordMixResult { get; set; }

        public DbSet<Movie> Movie { get; set; }

        public DbSet<Ingredient> Ingredient { get; set; }

        public DbSet<FoodStorageItem> FoodStorage { get; set; }

        public DbSet<FoodRanking> FoodRanking { get; set; }

        public DbSet<SpendingItem> SpendingItem { get; set; }

        public DbSet<SpendingGroup> SpendingGroup { get; set; }

        public DbSet<FoodIngredient> FoodIngredient { get; set; }

        public DbSet<Food> Food { get; set; }

        public DbSet<DayFood> DayFood { get; set; }

        public DbSet<DayFoodDishConnection> DayFoodDishConnection { get; set; }

        public DbSet<LogRow> LogRow { get; set; }

        public DbSet<SignInCookie> SignInCookie { get; set; }

        public DbSet<UserInfo> UserInfo { get; set; }

        public DbSet<Settings> Settings { get; set; }

        public DbSet<ChoreModel> ChoreModel { get; set; }

        public DbSet<ChoreStreak> ChoreStreak { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogRow>()
                .Property(x => x.LogRowSeverity)
                .HasConversion<string>();

            modelBuilder.Entity<DayFoodDishConnection>()
                .HasKey(dc => new { dc.DayFoodId, dc.FoodId });

            modelBuilder.Entity<DayFoodDishConnection>()
                .HasOne(dc => dc.DayFood)
                .WithMany(df => df.FoodConnections)
                .HasForeignKey(dc => dc.DayFoodId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DayFoodDishConnection>()
                .HasOne(dc => dc.Food)
                .WithMany(f => f.FoodConnections)
                .HasForeignKey(dc => dc.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Food>()
                .HasMany(s => s.Categories)
                .WithMany(c => c.Food)
                .UsingEntity(j => j.ToTable("FoodCategories"));

            modelBuilder.Entity<FoodIngredient>()
                .HasKey(fi => new { fi.FoodId, fi.IngredientId});

            modelBuilder.Entity<ChoreStreak>()
            .HasKey(c => new { c.ChoreId, c.Person});
        }
    }
}
