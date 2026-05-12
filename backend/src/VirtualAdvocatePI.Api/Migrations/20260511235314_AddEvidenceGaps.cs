using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceGaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evidence_gaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GapType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    GapStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlainEnglishExplanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SuggestedNextStep = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_gaps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_gaps_ClaimWorkspaceId",
                table: "evidence_gaps",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_gaps_ConditionId",
                table: "evidence_gaps",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_gaps_GapStatus",
                table: "evidence_gaps",
                column: "GapStatus");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_gaps_GapType",
                table: "evidence_gaps",
                column: "GapType");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_gaps_Severity",
                table: "evidence_gaps",
                column: "Severity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evidence_gaps");
        }
    }
}
