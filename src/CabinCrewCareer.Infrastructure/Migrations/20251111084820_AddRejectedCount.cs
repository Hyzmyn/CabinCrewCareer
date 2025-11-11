using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectedCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationInMinutes",
                table: "Test",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TestId",
                table: "Round",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedCount",
                table: "CampaignRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RejectedCount",
                table: "Campaign",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Round_TestId",
                table: "Round",
                column: "TestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Round_Test_TestId",
                table: "Round",
                column: "TestId",
                principalTable: "Test",
                principalColumn: "TestId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Round_Test_TestId",
                table: "Round");

            migrationBuilder.DropIndex(
                name: "IX_Round_TestId",
                table: "Round");

            migrationBuilder.DropColumn(
                name: "DurationInMinutes",
                table: "Test");

            migrationBuilder.DropColumn(
                name: "TestId",
                table: "Round");

            migrationBuilder.DropColumn(
                name: "RejectedCount",
                table: "CampaignRequest");

            migrationBuilder.DropColumn(
                name: "RejectedCount",
                table: "Campaign");
        }
    }
}
