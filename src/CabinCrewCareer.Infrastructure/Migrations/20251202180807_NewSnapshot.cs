using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryRequest_User_UserId",
                table: "EnquiryRequest");

            migrationBuilder.AddColumn<int>(
                name: "ApplicationSnapshotId",
                table: "RoundActivity",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserApplicationSnapshot",
                columns: table => new
                {
                    ApplicationSnapshotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationStatus = table.Column<int>(type: "int", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Experience = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = ***REMOVED*** "nvarchar(11)", maxLength: 11, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    EnglishDegreeNumber = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CitizenId = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: false),
                    EnglishTestDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ListeningScore = table.Column<int>(type: "int", nullable: false),
                    ReadingScore = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApplicationSnapshot", x => x.ApplicationSnapshotId);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSnapshot",
                columns: table => new
                {
                    DocumentSnapshotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentURL = ***REMOVED*** "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UploadDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationSnapshotId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSnapshot", x => x.DocumentSnapshotId);
                    table.ForeignKey(
                        name: "FK_DocumentSnapshot_UserApplicationSnapshot_ApplicationSnapshotId",
                        column: x => x.ApplicationSnapshotId,
                        principalTable: "UserApplicationSnapshot",
                        principalColumn: "ApplicationSnapshotId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoundActivity_ApplicationSnapshotId",
                table: "RoundActivity",
                column: "ApplicationSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSnapshot_ApplicationSnapshotId",
                table: "DocumentSnapshot",
                column: "ApplicationSnapshotId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnquiryRequest_User_UserId",
                table: "EnquiryRequest",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RoundActivity_UserApplicationSnapshot_ApplicationSnapshotId",
                table: "RoundActivity",
                column: "ApplicationSnapshotId",
                principalTable: "UserApplicationSnapshot",
                principalColumn: "ApplicationSnapshotId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryRequest_User_UserId",
                table: "EnquiryRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_RoundActivity_UserApplicationSnapshot_ApplicationSnapshotId",
                table: "RoundActivity");

            migrationBuilder.DropTable(
                name: "DocumentSnapshot");

            migrationBuilder.DropTable(
                name: "UserApplicationSnapshot");

            migrationBuilder.DropIndex(
                name: "IX_RoundActivity_ApplicationSnapshotId",
                table: "RoundActivity");

            migrationBuilder.DropColumn(
                name: "ApplicationSnapshotId",
                table: "RoundActivity");

            migrationBuilder.AddForeignKey(
                name: "FK_EnquiryRequest_User_UserId",
                table: "EnquiryRequest",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId");
        }
    }
}
