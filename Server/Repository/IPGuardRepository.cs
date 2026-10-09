using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

namespace GIBS.Module.IPGuard.Repository
{
    public interface IIPGuardRepository
    {
        IEnumerable<Models.IPGuard> GetIPGuards(int ModuleId);
        Models.IPGuard GetIPGuard(int IPGuardId);
        Models.IPGuard GetIPGuard(int IPGuardId, bool tracking);
        Models.IPGuard AddIPGuard(Models.IPGuard IPGuard);
        Models.IPGuard UpdateIPGuard(Models.IPGuard IPGuard);
        void DeleteIPGuard(int IPGuardId);
    }

    public class IPGuardRepository : IIPGuardRepository, ITransientService
    {
        private readonly IDbContextFactory<IPGuardContext> _factory;

        public IPGuardRepository(IDbContextFactory<IPGuardContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Models.IPGuard> GetIPGuards(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return db.IPGuard
		.Where(item => item.ModuleId == ModuleId)
		.AsNoTracking()
		.ToList();
        }

        public Models.IPGuard GetIPGuard(int IPGuardId)
        {
            return GetIPGuard(IPGuardId, false);
        }

        public Models.IPGuard GetIPGuard(int IPGuardId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.IPGuard
		    .Find(IPGuardId);
            }
            else
            {
                return db.IPGuard
		    .AsNoTracking()
		    .FirstOrDefault(item => item.IPGuardId == IPGuardId);
            }
        }

        public Models.IPGuard AddIPGuard(Models.IPGuard IPGuard)
        {
            using var db = _factory.CreateDbContext();
            db.IPGuard.Add(IPGuard);
            db.SaveChanges();
            return IPGuard;
        }

        public Models.IPGuard UpdateIPGuard(Models.IPGuard IPGuard)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(IPGuard).State = EntityState.Modified;
            db.SaveChanges();
            return IPGuard;
        }

        public void DeleteIPGuard(int IPGuardId)
        {
            using var db = _factory.CreateDbContext();
            Models.IPGuard IPGuard = db.IPGuard.Find(IPGuardId);
            db.IPGuard.Remove(IPGuard);
            db.SaveChanges();
        }
    }
}
