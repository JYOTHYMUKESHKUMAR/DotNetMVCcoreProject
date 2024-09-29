using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace DotNetCoreMVCApp.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }        
        public DbSet<Customer> CustomerSet { get; set; }
        public DbSet<CustomerContact> CustomerContactSet { get; set; }
        public DbSet<Supplier> SupplierSet { get; set; }
        public DbSet<SupplierContact> SupplierContactSet { get; set; }
        public DbSet<ProjectDatabase> ProjectDatabaseSet { get; set; }
        public DbSet<BankBalance> BankBalanceSet { get; set; }
        public DbSet<StartingBalance> StartingBalanceSet { get; set; }
        public DbSet<CashIn> CashInSet { get; set; }
        public DbSet<CashInInstallment> CashInInstallmentSet { get; set; }
        public DbSet<CashOut> CashOutSet { get; set; }
        public DbSet<CashOutInstallment> CashOutInstallmentSet { get; set; }
        
        public DbSet<CashFlowSummary> CashFlowSummaries { get; set; }
        

        public DbSet<ProjectCashFlowSummary> ProjectCashFlowSummaries { get; set; }
        public object StartingBalance { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                            .SelectMany(t => t.GetForeignKeys())
                            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            


            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CashFlowSummary>().HasNoKey().ToView("vw_CashFlowSummary");
            modelBuilder.Entity<ProjectCashFlowSummary>()
            .ToView("vw_ProjectCashFlowSummary")
            .HasNoKey();

        }


    }
}
