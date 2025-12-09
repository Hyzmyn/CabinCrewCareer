using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EntityAdding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestTypeId",
                table: "Test",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoundTypeId",
                table: "Round",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequirementId",
                table: "CampaignRequest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Requirement",
                columns: table => new
                {
                    RequirementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requirement", x => x.RequirementId);
                });

            migrationBuilder.CreateTable(
                name: "RoundType",
                columns: table => new
                {
                    RoundTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundTypeName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    CampaignType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundType", x => x.RoundTypeId);
                });

            migrationBuilder.CreateTable(
                name: "TestType",
                columns: table => new
                {
                    TestTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestTypeName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestType", x => x.TestTypeId);
                });

            migrationBuilder.CreateTable(
                name: "RequirementItem",
                columns: table => new
                {
                    RequirementItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = ***REMOVED*** "nvarchar(150)", maxLength: 150, nullable: false),
                    RequirementId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementItem", x => x.RequirementItemId);
                    table.ForeignKey(
                        name: "FK_RequirementItem_Requirement_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "Requirement",
                        principalColumn: "RequirementId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Test_TestTypeId",
                table: "Test",
                column: "TestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Round_RoundTypeId",
                table: "Round",
                column: "RoundTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequest_RequirementId",
                table: "CampaignRequest",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementItem_RequirementId",
                table: "RequirementItem",
                column: "RequirementId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignRequest_Requirement_RequirementId",
                table: "CampaignRequest",
                column: "RequirementId",
                principalTable: "Requirement",
                principalColumn: "RequirementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Round_RoundType_RoundTypeId",
                table: "Round",
                column: "RoundTypeId",
                principalTable: "RoundType",
                principalColumn: "RoundTypeId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Test_TestType_TestTypeId",
                table: "Test",
                column: "TestTypeId",
                principalTable: "TestType",
                principalColumn: "TestTypeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignRequest_Requirement_RequirementId",
                table: "CampaignRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Round_RoundType_RoundTypeId",
                table: "Round");

            migrationBuilder.DropForeignKey(
                name: "FK_Test_TestType_TestTypeId",
                table: "Test");

            migrationBuilder.DropTable(
                name: "RequirementItem");

            migrationBuilder.DropTable(
                name: "RoundType");

            migrationBuilder.DropTable(
                name: "TestType");

            migrationBuilder.DropTable(
                name: "Requirement");

            migrationBuilder.DropIndex(
                name: "IX_Test_TestTypeId",
                table: "Test");

            migrationBuilder.DropIndex(
                name: "IX_Round_RoundTypeId",
                table: "Round");

            migrationBuilder.DropIndex(
                name: "IX_CampaignRequest_RequirementId",
                table: "CampaignRequest");

            migrationBuilder.DropColumn(
                name: "TestTypeId",
                table: "Test");

            migrationBuilder.DropColumn(
                name: "RoundTypeId",
                table: "Round");

            migrationBuilder.DropColumn(
                name: "RequirementId",
                table: "CampaignRequest");
        }
    }
}
