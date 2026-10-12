using Microsoft.EntityFrameworkCore;
using Oqtane.Infrastructure;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Repository.Databases.Interfaces;

namespace GIBS.Module.IPGuard.Repository
{
    public class IPGuardContext : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Models.IPGuard> IPGuard { get; set; }
        public virtual DbSet<Models.BlockedIp> BlockedIps { get; set; }
        public virtual DbSet<Models.ErrorAttempt> ErrorAttempts { get; set; }

        public IPGuardContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.IPGuard>().ToTable(ActiveDatabase.RewriteName("GIBSIPGuard"));
            builder.Entity<Models.BlockedIp>().ToTable(ActiveDatabase.RewriteName("GIBSIPGuardBlockedIp"));
            builder.Entity<Models.BlockedIp>().HasIndex(item => item.IpAddress).IsUnique();
            builder.Entity<Models.ErrorAttempt>().ToTable(ActiveDatabase.RewriteName("GIBSIPGuardErrorAttempt"));
        }
    }
}
