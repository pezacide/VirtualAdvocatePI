using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualAdvocatePI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLoadExposureQuantities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DaysPerYear",
                table: "load_exposure_records",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HoursLifted",
                table: "load_exposure_records",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinutesPerOccasion",
                table: "load_exposure_records",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RepetitionsPerDay",
                table: "load_exposure_records",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StepsPerOccasion",
                table: "load_exposure_records",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDaysExposed",
                table: "load_exposure_records",
                type: "numeric(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysPerYear",
                table: "load_exposure_records");

            migrationBuilder.DropColumn(
                name: "HoursLifted",
                table: "load_exposure_records");

            migrationBuilder.DropColumn(
                name: "MinutesPerOccasion",
                table: "load_exposure_records");

            migrationBuilder.DropColumn(
                name: "RepetitionsPerDay",
                table: "load_exposure_records");

            migrationBuilder.DropColumn(
                name: "StepsPerOccasion",
                table: "load_exposure_records");

            migrationBuilder.DropColumn(
                name: "TotalDaysExposed",
                table: "load_exposure_records");
        }
    }
}
