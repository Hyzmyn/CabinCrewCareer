using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserApplication_Round_RoundId",
                table: "UserApplication");

            migrationBuilder.DropIndex(
                name: "IX_UserApplication_RoundId",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "RoundId",
                table: "UserApplication");

            migrationBuilder.CreateTable(
                name: "RoundActivity",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundActivity", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_RoundActivity_Round_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "RoundId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RoundActivity_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoundActivity_RoundId",
                table: "RoundActivity",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_RoundActivity_UserId",
                table: "RoundActivity",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoundActivity");

            migrationBuilder.AddColumn<int>(
                name: "RoundId",
                table: "UserApplication",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserApplication_RoundId",
                table: "UserApplication",
                column: "RoundId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserApplication_Round_RoundId",
                table: "UserApplication",
                column: "RoundId",
                principalTable: "Round",
                principalColumn: "RoundId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
