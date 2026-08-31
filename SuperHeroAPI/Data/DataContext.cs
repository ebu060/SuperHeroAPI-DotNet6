using Microsoft.EntityFrameworkCore;

namespace SuperHeroAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<SuperHero> SuperHeroes { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Mission> Missions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SuperHero>()
                .HasOne(h => h.Team)
                .WithMany(t => t.Heroes)
                .HasForeignKey(h => h.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Mission>()
                .HasMany(m => m.AssignedHeroes)
                .WithMany(h => h.Missions)
                .UsingEntity<Dictionary<string, object>>(
                    "MissionHero",
                    j => j.HasOne<SuperHero>().WithMany().HasForeignKey("SuperHeroId"),
                    j => j.HasOne<Mission>().WithMany().HasForeignKey("MissionId"),
                    j =>
                    {
                        j.HasKey("MissionId", "SuperHeroId");
                    });
        }
    }
}
