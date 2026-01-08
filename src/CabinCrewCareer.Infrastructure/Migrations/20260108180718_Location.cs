using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Location : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampaignType",
                table: "RoundConfiguration");

            migrationBuilder.DropColumn(
                name: "ConfigurationType",
                table: "RoundConfiguration");

            migrationBuilder.AddColumn<int>(
                name: "RoundTypeId",
                table: "RoundConfiguration",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignType = table.Column<int>(type: "int", nullable: false),
                    Address = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    WardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.LocationId);
                    table.ForeignKey(
                        name: "FK_Location_Ward_WardId",
                        column: x => x.WardId,
                        principalTable: "Ward",
                        principalColumn: "WardId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoundConfiguration_RoundTypeId",
                table: "RoundConfiguration",
                column: "RoundTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_WardId",
                table: "Location",
                column: "WardId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoundConfiguration_RoundType_RoundTypeId",
                table: "RoundConfiguration",
                column: "RoundTypeId",
                principalTable: "RoundType",
                principalColumn: "RoundTypeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoundConfiguration_RoundType_RoundTypeId",
                table: "RoundConfiguration");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropIndex(
                name: "IX_RoundConfiguration_RoundTypeId",
                table: "RoundConfiguration");

            migrationBuilder.DropColumn(
                name: "RoundTypeId",
                table: "RoundConfiguration");

            migrationBuilder.AddColumn<int>(
                name: "CampaignType",
                table: "RoundConfiguration",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConfigurationType",
                table: "RoundConfiguration",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
