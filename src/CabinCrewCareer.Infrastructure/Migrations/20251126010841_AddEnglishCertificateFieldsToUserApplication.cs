using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishCertificateFieldsToUserApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CitizenId",
                table: "UserApplication",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EnglishTestDate",
                table: "UserApplication",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ListeningScore",
                table: "UserApplication",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReadingScore",
                table: "UserApplication",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalScore",
                table: "UserApplication",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CitizenId",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "EnglishTestDate",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "ListeningScore",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "ReadingScore",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "UserApplication");
        }
    }
}
