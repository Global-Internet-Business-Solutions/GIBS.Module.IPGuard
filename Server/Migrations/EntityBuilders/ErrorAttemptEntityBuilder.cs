using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.IPGuard.Migrations.EntityBuilders
{
    public class ErrorAttemptEntityBuilder : AuditableBaseEntityBuilder<ErrorAttemptEntityBuilder>
    {
        private const string _entityTableName = "GIBSIPGuardErrorAttempt";
        private readonly PrimaryKey<ErrorAttemptEntityBuilder> _primaryKey = new("PK_GIBSIPGuardErrorAttempt", x => x.ErrorAttemptId);

        public ErrorAttemptEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override ErrorAttemptEntityBuilder BuildTable(ColumnsBuilder table)
        {
            ErrorAttemptId = AddAutoIncrementColumn(table, "ErrorAttemptId");
            IpAddress = AddMaxStringColumn(table, "IpAddress");
            RequestedUrl = AddMaxStringColumn(table, "RequestedUrl");
            AttemptDate = AddDateTimeColumn(table, "AttemptDate");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> ErrorAttemptId { get; set; }
        public OperationBuilder<AddColumnOperation> IpAddress { get; set; }
        public OperationBuilder<AddColumnOperation> RequestedUrl { get; set; }
        public OperationBuilder<AddColumnOperation> AttemptDate { get; set; }
    }
}
