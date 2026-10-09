using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.IPGuard.Migrations.EntityBuilders
{
    public class BlockedIpEntityBuilder : AuditableBaseEntityBuilder<BlockedIpEntityBuilder>
    {
        private const string _entityTableName = "GIBSIPGuardBlockedIp";
        private readonly PrimaryKey<BlockedIpEntityBuilder> _primaryKey = new("PK_GIBSIPGuardBlockedIp", x => x.BlockedIpId);

        public BlockedIpEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override BlockedIpEntityBuilder BuildTable(ColumnsBuilder table)
        {
            BlockedIpId = AddAutoIncrementColumn(table, "BlockedIpId");
            IpAddress = AddMaxStringColumn(table, "IpAddress");
            CountryLookup = AddMaxStringColumn(table, "CountryLookup");
            ErrorCount = AddIntegerColumn(table, "ErrorCount");
            BlockedDate = AddDateTimeColumn(table, "BlockedDate");
            Reason = AddMaxStringColumn(table, "Reason");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> BlockedIpId { get; set; }
        public OperationBuilder<AddColumnOperation> IpAddress { get; set; }
        public OperationBuilder<AddColumnOperation> CountryLookup { get; set; }
        public OperationBuilder<AddColumnOperation> ErrorCount { get; set; }
        public OperationBuilder<AddColumnOperation> BlockedDate { get; set; }
        public OperationBuilder<AddColumnOperation> Reason { get; set; }
    }
}
