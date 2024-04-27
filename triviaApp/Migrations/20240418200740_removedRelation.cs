using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace triviaApp.Migrations
{
    /// <inheritdoc />
    public partial class removedRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Competitions_CompetitionId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CompetitionId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompetitionId",
                table: "Categories",
                type: "int",
                nullable: true);

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
        }
    }
}
