using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAiDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_drafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionId = table.Column<Guid>(type: "uuid", nullable: true),
                    DraftType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PromptVersion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SourceReferences = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DraftText = table.Column<string>(type: "text", nullable: false),
                    UserEditedText = table.Column<string>(type: "text", nullable: true),
                    ReviewStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_drafts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_drafts_ClaimWorkspaceId",
                table: "ai_drafts",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_drafts_ConditionId",
                table: "ai_drafts",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_drafts_DraftType",
                table: "ai_drafts",
                column: "DraftType");

            migrationBuilder.CreateIndex(
                name: "IX_ai_drafts_ReviewStatus",
                table: "ai_drafts",
                column: "ReviewStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_drafts");
        }
    }
}
