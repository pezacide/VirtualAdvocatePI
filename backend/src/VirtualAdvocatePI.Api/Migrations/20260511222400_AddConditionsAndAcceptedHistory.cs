using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddConditionsAndAcceptedHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "claim_conditions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "accepted_condition_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviouslyAcceptedByDva = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OriginalAct = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreviousCompensationReceived = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreviousDvaDecisionLetterAvailable = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreviousAssessmentLetterAvailable = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreviousDecisionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreviousAssessmentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    WorseningClaimed = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorseningSummary = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accepted_condition_history", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accepted_condition_history_ClaimWorkspaceId",
                table: "accepted_condition_history",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_accepted_condition_history_ConditionId",
                table: "accepted_condition_history",
                column: "ConditionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accepted_condition_history");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "claim_conditions");
        }
    }
}
