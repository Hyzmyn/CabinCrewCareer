using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Test",
                columns: table => new
                {
                    TestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    Purpose = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    TestType = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    AudioFileURL = ***REMOVED*** "nvarchar(max)", nullable: false),
                    JoinCode = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = ***REMOVED*** "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = ***REMOVED*** "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Test", x => x.TestId);
                });

            migrationBuilder.CreateTable(
                name: "TestQuestion",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionContent = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    TestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestQuestion", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_TestQuestion_Test_TestId",
                        column: x => x.TestId,
                        principalTable: "Test",
                        principalColumn: "TestId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TestSession",
                columns: table => new
                {
                    TestSessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    TestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSession", x => x.TestSessionId);
                    table.ForeignKey(
                        name: "FK_TestSession_Test_TestId",
                        column: x => x.TestId,
                        principalTable: "Test",
                        principalColumn: "TestId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TestSession_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TestOption",
                columns: table => new
                {
                    OptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OptionContent = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestOption", x => x.OptionId);
                    table.ForeignKey(
                        name: "FK_TestOption_TestQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "TestQuestion",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryRequest",
                columns: table => new
                {
                    EnquiryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reason = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    NewScore = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TestSessionId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryRequest", x => x.EnquiryId);
                    table.ForeignKey(
                        name: "FK_EnquiryRequest_TestSession_TestSessionId",
                        column: x => x.TestSessionId,
                        principalTable: "TestSession",
                        principalColumn: "TestSessionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EnquiryRequest_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TestAnswer",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Score = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    EssayAnswer = ***REMOVED*** "nvarchar(100)", maxLength: 100, nullable: false),
                    TestSessionId = table.Column<int>(type: "int", nullable: true),
                    SelectedOptionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAnswer", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_TestAnswer_TestOption_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "TestOption",
                        principalColumn: "OptionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TestAnswer_TestSession_TestSessionId",
                        column: x => x.TestSessionId,
                        principalTable: "TestSession",
                        principalColumn: "TestSessionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TestResult",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Score = table.Column<int>(type: "int", nullable: false),
                    TestDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FinalScore = table.Column<int>(type: "int", nullable: false),
                    EnquiryRequested = table.Column<bool>(type: "bit", nullable: false),
                    EnquiryBy = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: false),
                    EnquiryNote = ***REMOVED*** "nvarchar(100)", maxLength: 100, nullable: false),
                    ExaminedBy = ***REMOVED*** "nvarchar(max)", nullable: false),
                    ExaminedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EnquiryId = table.Column<int>(type: "int", nullable: true),
                    TestSessionId = table.Column<int>(type: "int", nullable: true)
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
                name: "IX_EnquiryRequest_TestSessionId",
                table: "EnquiryRequest",
                column: "TestSessionId",
                unique: true,
                filter: "[TestSessionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryRequest_UserId",
                table: "EnquiryRequest",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_SelectedOptionId",
                table: "TestAnswer",
                column: "SelectedOptionId",
                unique: true,
                filter: "[SelectedOptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_TestSessionId",
                table: "TestAnswer",
                column: "TestSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestOption_QuestionId",
                table: "TestOption",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestQuestion_TestId",
                table: "TestQuestion",
                column: "TestId");

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

            migrationBuilder.CreateIndex(
                name: "IX_TestSession_TestId",
                table: "TestSession",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSession_UserId",
                table: "TestSession",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestAnswer");

            migrationBuilder.DropTable(
                name: "TestResult");

            migrationBuilder.DropTable(
                name: "TestOption");

            migrationBuilder.DropTable(
                name: "EnquiryRequest");

            migrationBuilder.DropTable(
                name: "TestQuestion");

            migrationBuilder.DropTable(
                name: "TestSession");

            migrationBuilder.DropTable(
                name: "Test");
        }
    }
}
