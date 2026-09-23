using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFunctionalLoadEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "functional_impact_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityDomain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GoodDayDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    BadDayDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    BadDayFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AidsOrHelpUsed = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_functional_impact_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "load_exposure_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActivityDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BodyAreaAffected = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TypicalWeightKg = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    MaxWeightKg = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    Frequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DurationPerOccasion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RepetitionsDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ServicePeriodFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ServicePeriodTo = table.Column<DateOnly>(type: "date", nullable: true),
                    YearsExposed = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    EquipmentOrContext = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HazardType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_load_exposure_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "load_reference_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TypicalWeightKg = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    WeightRangeKg = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ServiceContext = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SourceLabel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_load_reference_items", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_functional_impact_entries_ClaimWorkspaceId",
                table: "functional_impact_entries",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_functional_impact_entries_ConditionId",
                table: "functional_impact_entries",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_functional_impact_entries_Status",
                table: "functional_impact_entries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_load_exposure_records_ClaimWorkspaceId",
                table: "load_exposure_records",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_load_exposure_records_ConditionId",
                table: "load_exposure_records",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_load_exposure_records_RecordType",
                table: "load_exposure_records",
                column: "RecordType");

            migrationBuilder.CreateIndex(
                name: "IX_load_exposure_records_Status",
                table: "load_exposure_records",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_load_reference_items_Category",
                table: "load_reference_items",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_load_reference_items_ItemName",
                table: "load_reference_items",
                column: "ItemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_load_reference_items_Status",
                table: "load_reference_items",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "functional_impact_entries");

            migrationBuilder.DropTable(
                name: "load_exposure_records");

            migrationBuilder.DropTable(
                name: "load_reference_items");
        }
    }
}
