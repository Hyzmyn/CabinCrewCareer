using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "Blog");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropIndex(
                name: "IX_TestAnswer_SelectedOptionId",
                table: "TestAnswer");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "EvaluationForm",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserHistory",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipatedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CampaignRoundId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistory", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_UserHistory_CampaignRound_CampaignRoundId",
                        column: x => x.CampaignRoundId,
                        principalTable: "CampaignRound",
                        principalColumn: "CampaignRoundId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserHistory_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_SelectedOptionId",
                table: "TestAnswer",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_CampaignRoundId",
                table: "UserHistory",
                column: "CampaignRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_UserId",
                table: "UserHistory",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHistory");

            migrationBuilder.DropIndex(
                name: "IX_TestAnswer_SelectedOptionId",
                table: "TestAnswer");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "EvaluationForm");

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Blog",
                columns: table => new
                {
                    BlogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Body = ***REMOVED*** "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tags = ***REMOVED*** "nvarchar(max)", nullable: true),
                    Title = ***REMOVED*** "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog", x => x.BlogId);
                    table.ForeignKey(
                        name: "FK_Blog_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Blog_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CommentBody = ***REMOVED*** "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comment_Blog_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blog",
                        principalColumn: "BlogId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Comment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_SelectedOptionId",
                table: "TestAnswer",
                column: "SelectedOptionId",
                unique: true,
                filter: "[SelectedOptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_CategoryId",
                table: "Blog",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_UserId",
                table: "Blog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_BlogId",
                table: "Comment",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_UserId",
                table: "Comment",
                column: "UserId");
        }
    }
}
