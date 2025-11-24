using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvaluationForm",
                columns: table => new
                {
                    EvaluationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EvaluatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    ExaminerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationForm", x => x.EvaluationId);
                    table.ForeignKey(
                        name: "FK_EvaluationForm_RoundActivity_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "RoundActivity",
                        principalColumn: "ActivityId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EvaluationForm_User_ExaminerId",
                        column: x => x.ExaminerId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AppearanceResult",
                columns: table => new
                {
                    AppearanceResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    EvaluationId = table.Column<int>(type: "int", nullable: true),
                    ScoringCriteriaDetailId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppearanceResult", x => x.AppearanceResultId);
                    table.ForeignKey(
                        name: "FK_AppearanceResult_EvaluationForm_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "EvaluationForm",
                        principalColumn: "EvaluationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppearanceResult_ScoringCriteriaDetail_ScoringCriteriaDetailId",
                        column: x => x.ScoringCriteriaDetailId,
                        principalTable: "ScoringCriteriaDetail",
                        principalColumn: "ScoringCriteriaDetailId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InterviewResult",
                columns: table => new
                {
                    InterviewResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Comment = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    EvaluationId = table.Column<int>(type: "int", nullable: true),
                    InterviewCriteriaItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewResult", x => x.InterviewResultId);
                    table.ForeignKey(
                        name: "FK_InterviewResult_EvaluationForm_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "EvaluationForm",
                        principalColumn: "EvaluationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InterviewResult_InterviewCriteriaItem_InterviewCriteriaItemId",
                        column: x => x.InterviewCriteriaItemId,
                        principalTable: "InterviewCriteriaItem",
                        principalColumn: "InterviewCriteriaItemId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppearanceResult_EvaluationId",
                table: "AppearanceResult",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppearanceResult_ScoringCriteriaDetailId",
                table: "AppearanceResult",
                column: "ScoringCriteriaDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationForm_ActivityId",
                table: "EvaluationForm",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationForm_ExaminerId",
                table: "EvaluationForm",
                column: "ExaminerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewResult_EvaluationId",
                table: "InterviewResult",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewResult_InterviewCriteriaItemId",
                table: "InterviewResult",
                column: "InterviewCriteriaItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppearanceResult");

            migrationBuilder.DropTable(
                name: "InterviewResult");

            migrationBuilder.DropTable(
                name: "EvaluationForm");
        }
    }
}
