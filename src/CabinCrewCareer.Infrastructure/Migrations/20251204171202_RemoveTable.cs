using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "UserApplication");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                table: "CampaignRequest",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "CampaignRequest");

            migrationBuilder.CreateTable(
                name: "UserApplication",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ApplicationStatus = table.Column<int>(type: "int", nullable: false),
                    CitizenId = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EnglishDegreeNumber = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    EnglishTestDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Experience = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    ListeningScore = table.Column<int>(type: "int", nullable: false),
                    ReadingScore = table.Column<int>(type: "int", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApplication", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_UserApplication_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: true),
                    DocumentURL = ***REMOVED*** "nvarchar(max)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UploadDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Document_UserApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "UserApplication",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Document_ApplicationId",
                table: "Document",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplication_UserId",
                table: "UserApplication",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }
    }
}
