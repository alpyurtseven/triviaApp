using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace triviaApp.Migrations
{
    /// <inheritdoc />
    public partial class competitionRelationUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "CompetitionCategory",
                columns: table => new
                {
                    CompetitionId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionCategory", x => new { x.CompetitionId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_CompetitionCategory_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitionCategory_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetitionQuestion",
                columns: table => new
                {
                    CompetitionId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionQuestion", x => new { x.CompetitionId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_CompetitionQuestion_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitionQuestion_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionCategory_CategoryId",
                table: "CompetitionCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionQuestion_QuestionId",
                table: "CompetitionQuestion",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitionCategory");

            migrationBuilder.DropTable(
                name: "CompetitionQuestion");

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
    }
}
