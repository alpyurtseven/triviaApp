using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace triviaApp.Migrations
{
    /// <inheritdoc />
    public partial class updateQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Time",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "Questions");
        }
    }
}
