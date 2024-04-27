using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace triviaApp.Migrations
{
    /// <inheritdoc />
    public partial class competitionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompetitionId",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompetitionId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CompetitionId",
                table: "Questions",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CompetitionId",
                table: "Categories",
                column: "CompetitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Competitions_CompetitionId",
                table: "Categories",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Competitions_CompetitionId",
                table: "Questions",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Competitions_CompetitionId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Competitions_CompetitionId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CompetitionId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CompetitionId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                table: "Categories");
        }
    }
}
