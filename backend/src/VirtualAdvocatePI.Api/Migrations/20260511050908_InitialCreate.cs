using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "claim_conditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DiagnosisStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateDiagnosed = table.Column<DateOnly>(type: "date", nullable: true),
                    CurrentSymptoms = table.Column<string>(type: "text", nullable: true),
                    TreatmentSummary = table.Column<string>(type: "text", nullable: true),
                    MedicationSummary = table.Column<string>(type: "text", nullable: true),
                    MedicationSideEffects = table.Column<string>(type: "text", nullable: true),
                    FunctionalImpactSummary = table.Column<string>(type: "text", nullable: true),
                    LifestyleImpactSummary = table.Column<string>(type: "text", nullable: true),
                    WorkImpactSummary = table.Column<string>(type: "text", nullable: true),
                    StabilityNotes = table.Column<string>(type: "text", nullable: true),
                    WorseningNotes = table.Column<string>(type: "text", nullable: true),
                    IsPrimaryCondition = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_conditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "claim_workspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimFramework = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClaimScenario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WorkspaceTitle = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastOpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GeneratedPackStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_workspaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirebaseUid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AccountStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_claim_conditions_ClaimWorkspaceId",
                table: "claim_conditions",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_claim_workspaces_UserId",
                table: "claim_workspaces",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_FirebaseUid",
                table: "users",
                column: "FirebaseUid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "claim_conditions");

            migrationBuilder.DropTable(
                name: "claim_workspaces");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
