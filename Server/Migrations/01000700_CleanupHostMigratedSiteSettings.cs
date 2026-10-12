using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.IPGuard.Repository;

namespace GIBS.Module.IPGuard.Migrations
{
    [DbContext(typeof(IPGuardContext))]
    [Migration("GIBS.Module.IPGuard.01.00.07.00")]
    public class CleanupHostMigratedSiteSettings_01000700 : MultiDatabaseMigration
    {
        public CleanupHostMigratedSiteSettings_01000700(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var settingTable = ActiveDatabase.RewriteName("Setting");

            migrationBuilder.Sql($@"
DELETE FROM {settingTable}
WHERE EntityName = 'Site'
  AND SettingName IN (
      '{IPGuardSettings.EnabledSettingName}',
      '{IPGuardSettings.ThresholdSettingName}',
      '{IPGuardSettings.AllowedIpAddressSettingName}',
      '{IPGuardSettings.DiagnosticModeSettingName}',
      '{IPGuardSettings.ExcludeReservedRoutesSettingName}'
  )");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
