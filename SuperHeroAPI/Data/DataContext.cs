using Microsoft.EntityFrameworkCore;

namespace SuperHeroAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<SuperHero> SuperHeroes { get; set; }
        public DbSet<Battle> Battles { get; set; }
        public DbSet<BattleLog> BattleLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Battle>()
                .HasOne(b => b.AttackerHero)
                .WithMany()
                .HasForeignKey(b => b.AttackerHeroId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Battle>()
                .HasOne(b => b.DefenderHero)
                .WithMany()
                .HasForeignKey(b => b.DefenderHeroId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Battle>()
                .HasOne(b => b.WinnerHero)
                .WithMany()
                .HasForeignKey(b => b.WinnerHeroId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BattleLog>()
                .HasOne(b => b.Battle)
                .WithMany(b => b.CombatLogs)
                .HasForeignKey(bl => bl.BattleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
