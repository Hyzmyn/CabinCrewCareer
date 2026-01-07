using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurationEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfigurationId",
                table: "EvaluationForm",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationForm_ConfigurationId",
                table: "EvaluationForm",
                column: "ConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationForm_RoundConfiguration_ConfigurationId",
                table: "EvaluationForm",
                column: "ConfigurationId",
                principalTable: "RoundConfiguration",
                principalColumn: "RoundConfigurationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationForm_RoundConfiguration_ConfigurationId",
                table: "EvaluationForm");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationForm_ConfigurationId",
                table: "EvaluationForm");

            migrationBuilder.DropColumn(
                name: "ConfigurationId",
                table: "EvaluationForm");
        }
    }
}
