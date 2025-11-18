using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScoringCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoringCriteria",
                columns: table => new
                {
                    ScoringCriteriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringCriteria", x => x.ScoringCriteriaId);
                });

            migrationBuilder.CreateTable(
                name: "ScoringCriteriaItem",
                columns: table => new
                {
                    ScoringCriteriaItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemType = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    Text = ***REMOVED*** "nvarchar(250)", maxLength: 250, nullable: false),
                    EnglishText = ***REMOVED*** "nvarchar(250)", maxLength: 250, nullable: false),
                    ScoringCriteriaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringCriteriaItem", x => x.ScoringCriteriaItemId);
                    table.ForeignKey(
                        name: "FK_ScoringCriteriaItem_ScoringCriteria_ScoringCriteriaId",
                        column: x => x.ScoringCriteriaId,
                        principalTable: "ScoringCriteria",
                        principalColumn: "ScoringCriteriaId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ScoringCriteriaDetail",
                columns: table => new
                {
                    ScoringCriteriaDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetailText = ***REMOVED*** "nvarchar(100)", maxLength: 100, nullable: false),
                    ScoringCriteriaItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringCriteriaDetail", x => x.ScoringCriteriaDetailId);
                    table.ForeignKey(
                        name: "FK_ScoringCriteriaDetail_ScoringCriteriaItem_ScoringCriteriaItemId",
                        column: x => x.ScoringCriteriaItemId,
                        principalTable: "ScoringCriteriaItem",
                        principalColumn: "ScoringCriteriaItemId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScoringCriteriaDetail_ScoringCriteriaItemId",
                table: "ScoringCriteriaDetail",
                column: "ScoringCriteriaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringCriteriaItem_ScoringCriteriaId",
                table: "ScoringCriteriaItem",
                column: "ScoringCriteriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScoringCriteriaDetail");

            migrationBuilder.DropTable(
                name: "ScoringCriteriaItem");

            migrationBuilder.DropTable(
                name: "ScoringCriteria");
        }
    }
}
