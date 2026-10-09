using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.IPGuard.Migrations.EntityBuilders;
using GIBS.Module.IPGuard.Repository;

namespace GIBS.Module.IPGuard.Migrations
{
    [DbContext(typeof(IPGuardContext))]
    [Migration("GIBS.Module.IPGuard.01.00.00.01")]
    public class AddBlockedIpTracking : MultiDatabaseMigration
    {
        public AddBlockedIpTracking(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            new BlockedIpEntityBuilder(migrationBuilder, ActiveDatabase).Create();
            new ErrorAttemptEntityBuilder(migrationBuilder, ActiveDatabase).Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            new ErrorAttemptEntityBuilder(migrationBuilder, ActiveDatabase).Drop();
            new BlockedIpEntityBuilder(migrationBuilder, ActiveDatabase).Drop();
        }
    }
}
