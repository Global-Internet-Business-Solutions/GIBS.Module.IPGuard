using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.IPGuard.Repository;

namespace GIBS.Module.IPGuard.Migrations
{
    [DbContext(typeof(IPGuardContext))]
    [Migration("GIBS.Module.IPGuard.01.00.09.00")]
    public class CleanupRemainingSiteBlockWindowSetting_01000900 : MultiDatabaseMigration
    {
        public CleanupRemainingSiteBlockWindowSetting_01000900(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var settingTable = ActiveDatabase.RewriteName("Setting");

            migrationBuilder.Sql($@"
DELETE FROM {settingTable}
WHERE EntityName = 'Site'
  AND SettingName = '{IPGuardSettings.BlockWindowHoursSettingName}'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
