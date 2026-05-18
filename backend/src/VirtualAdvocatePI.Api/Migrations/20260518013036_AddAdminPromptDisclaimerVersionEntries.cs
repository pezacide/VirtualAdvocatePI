using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPromptDisclaimerVersionEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_prompt_disclaimer_version_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    VersionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VersionLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AppliesTo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_prompt_disclaimer_version_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_prompt_disclaimer_version_entries_AppliesTo",
                table: "admin_prompt_disclaimer_version_entries",
                column: "AppliesTo");

            migrationBuilder.CreateIndex(
                name: "IX_admin_prompt_disclaimer_version_entries_ApprovalStatus",
                table: "admin_prompt_disclaimer_version_entries",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_admin_prompt_disclaimer_version_entries_Category",
                table: "admin_prompt_disclaimer_version_entries",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_admin_prompt_disclaimer_version_entries_IsActive",
                table: "admin_prompt_disclaimer_version_entries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_admin_prompt_disclaimer_version_entries_Status",
                table: "admin_prompt_disclaimer_version_entries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_admin_prompt_disclaimer_version_entries_VersionKey",
                table: "admin_prompt_disclaimer_version_entries",
                column: "VersionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_prompt_disclaimer_version_entries_VersionType",
                table: "admin_prompt_disclaimer_version_entries",
                column: "VersionType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_prompt_disclaimer_version_entries");
        }
    }
}
