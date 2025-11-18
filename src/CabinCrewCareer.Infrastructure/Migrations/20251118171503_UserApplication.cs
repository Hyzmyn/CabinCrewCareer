using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "UserApplication",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishDegreeNumber",
                table: "UserApplication",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Experience",
                table: "UserApplication",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "UserApplication",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "UserApplication",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Document",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Document",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "UploadDate",
                table: "Document",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "EnglishDegreeNumber",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "UserApplication");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "UploadDate",
                table: "Document");
        }
    }
}
