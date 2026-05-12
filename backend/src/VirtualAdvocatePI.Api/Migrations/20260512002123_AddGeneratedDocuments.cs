using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "generated_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DocumentStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocxStoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PdfStoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TemplateVersion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IncludedAiDraftIds = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DownloadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generated_documents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_generated_documents_ClaimWorkspaceId",
                table: "generated_documents",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_generated_documents_DocumentStatus",
                table: "generated_documents",
                column: "DocumentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_generated_documents_DocumentType",
                table: "generated_documents",
                column: "DocumentType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "generated_documents");
        }
    }
}
