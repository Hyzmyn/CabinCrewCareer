using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestResult");

            migrationBuilder.AlterColumn<string>(
                name: "EssayAnswer",
                table: "TestAnswer",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TestName",
                table: "Test",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "Test",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "RoundType",
                table: "Round",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "EnquiryRequest",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "JobDescription",
                table: "CampaignRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobRequirement",
                table: "CampaignRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobDescription",
                table: "Campaign",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobRequirement",
                table: "Campaign",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                table: "Campaign",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoundType",
                table: "Round");

            migrationBuilder.DropColumn(
                name: "JobDescription",
                table: "CampaignRequest");

            migrationBuilder.DropColumn(
                name: "JobRequirement",
                table: "CampaignRequest");

            migrationBuilder.DropColumn(
                name: "JobDescription",
                table: "Campaign");

            migrationBuilder.DropColumn(
                name: "JobRequirement",
                table: "Campaign");

            migrationBuilder.DropColumn(
                name: "RejectedReason",
                table: "Campaign");

            migrationBuilder.AlterColumn<string>(
                name: "EssayAnswer",
                table: "TestAnswer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "TestName",
                table: "Test",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "Test",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "EnquiryRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateTable(
                name: "TestResult",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnquiryId = table.Column<int>(type: "int", nullable: true),
                    TestSessionId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EnquiryBy = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: false),
                    EnquiryNote = ***REMOVED*** "nvarchar(100)", maxLength: 100, nullable: false),
                    EnquiryRequested = table.Column<bool>(type: "bit", nullable: false),
                    ExaminedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExaminedBy = ***REMOVED*** "nvarchar(max)", nullable: false),
                    FinalScore = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    TestDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResult", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_TestResult_EnquiryRequest_EnquiryId",
                        column: x => x.EnquiryId,
                        principalTable: "EnquiryRequest",
                        principalColumn: "EnquiryId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TestResult_TestSession_TestSessionId",
                        column: x => x.TestSessionId,
                        principalTable: "TestSession",
                        principalColumn: "TestSessionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TestResult_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_EnquiryId",
                table: "TestResult",
                column: "EnquiryId",
                unique: true,
                filter: "[EnquiryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_TestSessionId",
                table: "TestResult",
                column: "TestSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_UserId",
                table: "TestResult",
                column: "UserId");
        }
    }
}
