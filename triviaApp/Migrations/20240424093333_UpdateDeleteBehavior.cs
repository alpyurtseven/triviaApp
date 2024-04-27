using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace triviaApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Competitions_CompetitionId",
                table: "Scores");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Participants_ParticipantId",
                table: "Scores");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Participants",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CompetitionId",
                table: "Participants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_CompetitionId",
                table: "Participants",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_Username_CompetitionId",
                table: "Participants",
                columns: new[] { "Username", "CompetitionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Competitions_CompetitionId",
                table: "Participants",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Competitions_CompetitionId",
                table: "Scores",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Participants_ParticipantId",
                table: "Scores",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Competitions_CompetitionId",
                table: "Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Competitions_CompetitionId",
                table: "Scores");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Participants_ParticipantId",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_Participants_CompetitionId",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Participants_Username_CompetitionId",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                table: "Participants");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Competitions_CompetitionId",
                table: "Scores",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Participants_ParticipantId",
                table: "Scores",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
