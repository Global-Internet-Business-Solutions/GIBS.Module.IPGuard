using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.IPGuard.Repository;

namespace GIBS.Module.IPGuard.Migrations
{
    [DbContext(typeof(IPGuardContext))]
    [Migration("GIBS.Module.IPGuard.01.00.02.00")]

    public class EnforceUniqueBlockedIp : MultiDatabaseMigration
    {
        public EnforceUniqueBlockedIp(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var blockedIpTable = ActiveDatabase.RewriteName("GIBSIPGuardBlockedIp");
            var errorAttemptTable = ActiveDatabase.RewriteName("GIBSIPGuardErrorAttempt");

            migrationBuilder.Sql($"UPDATE {blockedIpTable} SET IpAddress = LOWER(IpAddress)");
            migrationBuilder.Sql($"DELETE FROM {blockedIpTable} WHERE BlockedIpId IN (SELECT BlockedIpId FROM (SELECT BlockedIpId, ROW_NUMBER() OVER (PARTITION BY IpAddress ORDER BY ErrorCount DESC, BlockedDate DESC, BlockedIpId DESC) AS rn FROM {blockedIpTable}) AS RankedBlockedIps WHERE rn > 1)");
            migrationBuilder.Sql($"ALTER TABLE {blockedIpTable} ALTER COLUMN IpAddress NVARCHAR(45) NOT NULL");
            migrationBuilder.Sql($"UPDATE {errorAttemptTable} SET IpAddress = LOWER(IpAddress)");
            migrationBuilder.Sql($"ALTER TABLE {errorAttemptTable} ALTER COLUMN IpAddress NVARCHAR(45) NOT NULL");
            migrationBuilder.CreateIndex(
                name: "IX_GIBSIPGuardBlockedIp_IpAddress",
                table: blockedIpTable,
                column: "IpAddress",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var blockedIpTable = ActiveDatabase.RewriteName("GIBSIPGuardBlockedIp");
            var errorAttemptTable = ActiveDatabase.RewriteName("GIBSIPGuardErrorAttempt");

            migrationBuilder.DropIndex(
                name: "IX_GIBSIPGuardBlockedIp_IpAddress",
                table: blockedIpTable);
            migrationBuilder.Sql($"ALTER TABLE {blockedIpTable} ALTER COLUMN IpAddress NVARCHAR(MAX) NOT NULL");
            migrationBuilder.Sql($"ALTER TABLE {errorAttemptTable} ALTER COLUMN IpAddress NVARCHAR(MAX) NOT NULL");
        }
    }
}
