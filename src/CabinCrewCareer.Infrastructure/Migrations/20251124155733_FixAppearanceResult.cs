using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAppearanceResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppearanceResult_ScoringCriteriaDetail_ScoringCriteriaDetailId",
                table: "AppearanceResult");

            migrationBuilder.RenameColumn(
                name: "ScoringCriteriaDetailId",
                table: "AppearanceResult",
                newName: "ScoringCriteriaItemId");

            migrationBuilder.RenameIndex(
                name: "IX_AppearanceResult_ScoringCriteriaDetailId",
                table: "AppearanceResult",
                newName: "IX_AppearanceResult_ScoringCriteriaItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppearanceResult_ScoringCriteriaItem_ScoringCriteriaItemId",
                table: "AppearanceResult",
                column: "ScoringCriteriaItemId",
                principalTable: "ScoringCriteriaItem",
                principalColumn: "ScoringCriteriaItemId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppearanceResult_ScoringCriteriaItem_ScoringCriteriaItemId",
                table: "AppearanceResult");

            migrationBuilder.RenameColumn(
                name: "ScoringCriteriaItemId",
                table: "AppearanceResult",
                newName: "ScoringCriteriaDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_AppearanceResult_ScoringCriteriaItemId",
                table: "AppearanceResult",
                newName: "IX_AppearanceResult_ScoringCriteriaDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppearanceResult_ScoringCriteriaDetail_ScoringCriteriaDetailId",
                table: "AppearanceResult",
                column: "ScoringCriteriaDetailId",
                principalTable: "ScoringCriteriaDetail",
                principalColumn: "ScoringCriteriaDetailId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
