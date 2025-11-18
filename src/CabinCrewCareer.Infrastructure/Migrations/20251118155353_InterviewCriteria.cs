using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InterviewCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewCriteria",
                columns: table => new
                {
                    InterviewCriteriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewCriteria", x => x.InterviewCriteriaId);
                });

            migrationBuilder.CreateTable(
                name: "InterviewCriteriaItem",
                columns: table => new
                {
                    InterviewCriteriaItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Criteria = ***REMOVED*** "nvarchar(250)", maxLength: 250, nullable: false),
                    InterviewCriteriaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewCriteriaItem", x => x.InterviewCriteriaItemId);
                    table.ForeignKey(
                        name: "FK_InterviewCriteriaItem_InterviewCriteria_InterviewCriteriaId",
                        column: x => x.InterviewCriteriaId,
                        principalTable: "InterviewCriteria",
                        principalColumn: "InterviewCriteriaId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewCriteriaItem_InterviewCriteriaId",
                table: "InterviewCriteriaItem",
                column: "InterviewCriteriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewCriteriaItem");

            migrationBuilder.DropTable(
                name: "InterviewCriteria");
        }
    }
}
