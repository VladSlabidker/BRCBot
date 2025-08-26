using System.Data.SqlClient;
using Common.Enums;
using Data.SQL.Configs;
using Data.SQL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Data.SQL;

public class BotContext: DbContext
{
    public DbSet<Bank> Banks { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<Subscription> Subscriptions { get; set; }
    
    public DbSet<Payment> Payments { get; set; }
    
    public DbSet<Receipt> Receipts { get; set; }
    
    private readonly SqlExpressConfig _sqlExpressConfig;
    
    public BotContext(IOptions<SqlExpressConfig> sqlExpressConfig)
    {
        _sqlExpressConfig = sqlExpressConfig.Value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_sqlExpressConfig != null)
        {
            var connectionString = new SqlConnectionStringBuilder()
            {
                DataSource = _sqlExpressConfig.DataSource,
                InitialCatalog = _sqlExpressConfig.InitialCatalog,
                IntegratedSecurity = false,
                UserID = _sqlExpressConfig.UserId,
                Password = _sqlExpressConfig.Password,
                TrustServerCertificate = _sqlExpressConfig.TrustServerCertificate
            };
            
            optionsBuilder.UseSqlServer(connectionString.ConnectionString);
        }
        base.OnConfiguring(optionsBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Receipt>(e =>
        {
            e.HasOne(r => r.Bank)
                .WithMany()
                .HasForeignKey(r => r.BankId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(s => s.Payment)
                .WithOne(p => p.Subscription)
                .HasForeignKey<Subscription>(s => s.PaymentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Bank>().HasData(Enum.GetValues<BankType>().Select(e => new Bank()
        {
            Id = (int)e, 
            Name = e.ToString()
        }));
    }
}