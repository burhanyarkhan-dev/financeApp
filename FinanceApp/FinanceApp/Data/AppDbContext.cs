using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            modelBuilder.Entity<OtpCode>()
                .HasIndex(o => new { o.PhoneNumber, o.Purpose, o.IsUsed });
        }

        public override int SaveChanges()
        {
            StampUpdatedAt();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            StampUpdatedAt();
            return base.SaveChangesAsync(ct);
        }

        private void StampUpdatedAt()
        {
            foreach (var e in ChangeTracker.Entries<User>())
                if (e.State == EntityState.Modified)
                    e.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
