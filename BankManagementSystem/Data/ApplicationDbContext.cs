using BankManagementSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Branch> Branches { get; set; }

        public DbSet<Manager> Managers { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<BankTransaction> Transactions { get; set; }

        public DbSet<CustomerAccount> CustomerAccounts { get; set; }

        protected override void OnConfiguring(
     DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
    "Server=.;Database=BankManagementDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Branch
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(b => b.Code);

                entity.Property(b => b.Code)
                    .HasMaxLength(20);

                entity.Property(b => b.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(b => b.Address)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(b => b.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(b => b.Manager)
                    .WithOne(m => m.Branch)
                    .HasForeignKey<Branch>(b => b.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Manager
            modelBuilder.Entity<Manager>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(m => m.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(m => m.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.NationalId)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasIndex(c => c.NationalId)
                    .IsUnique();

                entity.Property(c => c.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(c => c.Address)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(c => c.CustomerType)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            // Account
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.AccountNumber);

                entity.Property(a => a.AccountNumber)
                    .HasMaxLength(30);

                entity.Property(a => a.CurrentBalance)
                    .HasPrecision(18, 2);

                entity.Property(a => a.AccountType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(a => a.Branch)
                    .WithMany(b => b.Accounts)
                    .HasForeignKey(a => a.BranchCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BankTransaction
            modelBuilder.Entity<BankTransaction>(entity =>
            {
                entity.HasKey(t => t.TransactionNumber);

                entity.Property(t => t.Amount)
                    .HasPrecision(18, 2);

                entity.Property(t => t.Note)
                    .HasMaxLength(300);

                entity.Property(t => t.TransactionType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(t => t.Account)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey(t => t.AccountNumber)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CustomerAccount
            modelBuilder.Entity<CustomerAccount>(entity =>
            {
                entity.HasKey(ca => new
                {
                    ca.CustomerId,
                    ca.AccountNumber
                });

                entity.Property(ca => ca.OwnershipType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(ca => ca.AccountStatus)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(ca => ca.Customer)
                    .WithMany(c => c.CustomerAccounts)
                    .HasForeignKey(ca => ca.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ca => ca.Account)
                    .WithMany(a => a.CustomerAccounts)
                    .HasForeignKey(ca => ca.AccountNumber)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}