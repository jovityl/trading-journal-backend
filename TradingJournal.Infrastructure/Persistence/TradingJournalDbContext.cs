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
        public DbSet<Prompt> Prompts => Set<Prompt>();
        public DbSet<TokenUsage> TokenUsages => Set<TokenUsage>();
        public DbSet<TradeMessage> TradeMessages => Set<TradeMessage>();

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

                entity.Property(t => t.ViolationTags).HasColumnType("text[]");

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Trades)
                    .HasForeignKey(t => t.UserId);
            });

            // Prompt
            modelBuilder.Entity<Prompt>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Key).IsRequired();
                entity.Property(p => p.Content).IsRequired();
                entity.HasIndex(p => p.Key).IsUnique();
            });

            // TokenUsage
            modelBuilder.Entity<TokenUsage>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Provider).IsRequired();
                entity.Property(t => t.Model).IsRequired();
                entity.Property(t => t.Endpoint).IsRequired();
                entity.Property(t => t.Cost).HasColumnType("decimal(18,6)");
                entity.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId);
                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.CreatedAt);
            });

            // TradeMessage
            modelBuilder.Entity<TradeMessage>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Role).IsRequired();
                entity.Property(m => m.Content).IsRequired();
                entity.HasOne(m => m.Trade).WithMany().HasForeignKey(m => m.TradeId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(m => m.TradeId);
                entity.HasIndex(m => m.CreatedAt);
            });
        }
    }
}
