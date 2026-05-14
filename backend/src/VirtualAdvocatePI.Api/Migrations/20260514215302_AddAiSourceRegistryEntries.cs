using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAiSourceRegistryEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_source_registry_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Jurisdiction = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CitationLabel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContentHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_source_registry_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_source_registry_entries_ApprovalStatus",
                table: "ai_source_registry_entries",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ai_source_registry_entries_Category",
                table: "ai_source_registry_entries",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ai_source_registry_entries_IsActive",
                table: "ai_source_registry_entries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ai_source_registry_entries_SourceKey",
                table: "ai_source_registry_entries",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_source_registry_entries_SourceType",
                table: "ai_source_registry_entries",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_ai_source_registry_entries_Status",
                table: "ai_source_registry_entries",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_source_registry_entries");
        }
    }
}
