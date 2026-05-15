using Microsoft.EntityFrameworkCore;
using TradingJournal.Domain.Entities;

namespace TradingJournal.Infrastructure.Persistence
{
    public class TradingJournalDbContext : DbContext
    {
        public TradingJournalDbContext(DbContextOptions<TradingJournalDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Trade> Trades => Set<Trade>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Auth0Id).IsRequired();
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.DisplayName).IsRequired();
                entity.HasIndex(u => u.Auth0Id).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Trade
            modelBuilder.Entity<Trade>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Ticker).IsRequired();
                entity.Property(t => t.OptionType).IsRequired();
                entity.Property(t => t.Strategy).IsRequired();
                entity.Property(t => t.EntryPrice).HasColumnType("decimal(18,2)");
                entity.Property(t => t.ExitPrice).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Pnl).HasColumnType("decimal(18,2)");

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Trades)
                    .HasForeignKey(t => t.UserId);
            });
        }
    }
}
