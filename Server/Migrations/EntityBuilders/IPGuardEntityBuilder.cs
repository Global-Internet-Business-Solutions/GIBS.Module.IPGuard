using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.IPGuard.Migrations.EntityBuilders
{
    public class IPGuardEntityBuilder : AuditableBaseEntityBuilder<IPGuardEntityBuilder>
    {
        private const string _entityTableName = "GIBSIPGuard";
        private readonly PrimaryKey<IPGuardEntityBuilder> _primaryKey = new("PK_GIBSIPGuard", x => x.IPGuardId);
        private readonly ForeignKey<IPGuardEntityBuilder> _moduleForeignKey = new("FK_GIBSIPGuard_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public IPGuardEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override IPGuardEntityBuilder BuildTable(ColumnsBuilder table)
        {
            IPGuardId = AddAutoIncrementColumn(table,"IPGuardId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Name = AddMaxStringColumn(table,"Name");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> IPGuardId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
    }
}
