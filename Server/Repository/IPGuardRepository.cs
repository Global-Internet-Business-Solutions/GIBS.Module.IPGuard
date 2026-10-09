using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using Oqtane.Repository;

namespace GIBS.Module.IPGuard.Repository
{
	public interface IIPGuardRepository
	{
		IEnumerable<Models.BlockedIp> GetBlockedIps();
		Models.BlockedIp GetBlockedIp(int blockedIpId);
		Models.BlockedIp GetBlockedIp(string ipAddress);
		Models.BlockedIp AddBlockedIp(Models.BlockedIp blockedIp);
		Models.BlockedIp UpdateBlockedIp(Models.BlockedIp blockedIp);
		void DeleteBlockedIp(int blockedIpId);
		void DeleteErrorAttempts(string ipAddress);
		void DeleteAllErrorAttempts();
		IEnumerable<Models.ErrorAttempt> GetErrorAttempts();
		Models.ErrorAttempt AddErrorAttempt(Models.ErrorAttempt errorAttempt);
		int GetRecentErrorAttemptCount(string ipAddress, DateTime since);
		bool IsBlocked(string ipAddress);
	}

	public class IPGuardRepository : IIPGuardRepository, ITransientService
	{
		private readonly IDBContextDependencies _dbContextDependencies;

		public IPGuardRepository(IDBContextDependencies dbContextDependencies)
		{
			_dbContextDependencies = dbContextDependencies;
		}

		public IEnumerable<Models.BlockedIp> GetBlockedIps()
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			return db.BlockedIps
				.AsNoTracking()
				.ToList();
		}

		public Models.BlockedIp GetBlockedIp(int blockedIpId)
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			return db.BlockedIps
				.AsNoTracking()
				.FirstOrDefault(item => item.BlockedIpId == blockedIpId);
		}

		public Models.BlockedIp GetBlockedIp(string ipAddress)
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			return db.BlockedIps
				.FirstOrDefault(item => item.IpAddress == ipAddress);
		}

		public Models.BlockedIp AddBlockedIp(Models.BlockedIp blockedIp)
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			db.BlockedIps.Add(blockedIp);
			db.SaveChanges();
			return blockedIp;
		}

		public Models.BlockedIp UpdateBlockedIp(Models.BlockedIp blockedIp)
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			db.Entry(blockedIp).State = EntityState.Modified;
			db.SaveChanges();
			return blockedIp;
		}

		public void DeleteBlockedIp(int blockedIpId)
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			var blockedIp = db.BlockedIps.FirstOrDefault(item => item.BlockedIpId == blockedIpId);
			if (blockedIp != null)
			{
				db.BlockedIps.Remove(blockedIp);
				db.SaveChanges();
			}
		}

		public void DeleteErrorAttempts(string ipAddress)
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			var errorAttempts = db.ErrorAttempts.Where(item => item.IpAddress == ipAddress).ToList();
			if (errorAttempts.Any())
			{
				db.ErrorAttempts.RemoveRange(errorAttempts);
				db.SaveChanges();
			}
		}

		public void DeleteAllErrorAttempts()
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			var errorAttempts = db.ErrorAttempts.ToList();
			if (errorAttempts.Any())
			{
				db.ErrorAttempts.RemoveRange(errorAttempts);
				db.SaveChanges();
			}
		}

		public IEnumerable<Models.ErrorAttempt> GetErrorAttempts()
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			return db.ErrorAttempts
				.OrderByDescending(item => item.AttemptDate)
				.AsNoTracking()
				.ToList();
		}

		public Models.ErrorAttempt AddErrorAttempt(Models.ErrorAttempt errorAttempt)
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			db.ErrorAttempts.Add(errorAttempt);
			db.SaveChanges();
			return errorAttempt;
		}

		public int GetRecentErrorAttemptCount(string ipAddress, DateTime since)
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			return db.ErrorAttempts.Count(item => item.IpAddress == ipAddress && item.AttemptDate >= since);
		}

		public bool IsBlocked(string ipAddress)
		{
			using var db = new IPGuardContext(_dbContextDependencies);
			return db.BlockedIps.Any(item => item.IpAddress == ipAddress);
		}
	}
}
