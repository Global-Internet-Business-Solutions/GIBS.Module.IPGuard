using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Oqtane.Repository.Databases.Interfaces;

namespace GIBS.Module.IPGuard.Repository
{
    public class IPGuardContext : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Models.IPGuard> IPGuard { get; set; }

        public IPGuardContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.IPGuard>().ToTable(ActiveDatabase.RewriteName("GIBSIPGuard"));
        }
    }
}
