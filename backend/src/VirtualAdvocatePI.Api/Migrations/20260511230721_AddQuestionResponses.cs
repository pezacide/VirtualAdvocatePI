using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "question_responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionGroup = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QuestionKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AnswerText = table.Column<string>(type: "text", nullable: true),
                    AnswerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_responses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_question_responses_ClaimWorkspaceId",
                table: "question_responses",
                column: "ClaimWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_question_responses_ConditionId",
                table: "question_responses",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_question_responses_ConditionId_QuestionKey",
                table: "question_responses",
                columns: new[] { "ConditionId", "QuestionKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "question_responses");
        }
    }
}
