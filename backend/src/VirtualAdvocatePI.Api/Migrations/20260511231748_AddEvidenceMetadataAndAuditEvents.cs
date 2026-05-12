using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceMetadataAndAuditEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EventDetail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClientType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "evidence_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EvidenceStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FileType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    DocumentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ProviderName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    UserNotes = table.Column<string>(type: "text", nullable: true),
                    AiSummary = table.Column<string>(type: "text", nullable: true),
                    UserConfirmedSummary = table.Column<string>(type: "text", nullable: true),
                    UsedInGeneratedPack = table.Column<bool>(type: "boolean", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_items", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_ClaimWorkspaceId",
                table: "audit_events",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_CreatedAt",
                table: "audit_events",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_EventType",
                table: "audit_events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_UserId",
                table: "audit_events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_items_ClaimWorkspaceId",
                table: "evidence_items",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_items_ConditionId",
                table: "evidence_items",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_items_EvidenceStatus",
                table: "evidence_items",
                column: "EvidenceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_items_EvidenceType",
                table: "evidence_items",
                column: "EvidenceType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "evidence_items");
        }
    }
}
