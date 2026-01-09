using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiredDate",
                table: "RoundConfiguration",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "RoundConfiguration",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiredDate",
                table: "RoundConfiguration");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RoundConfiguration");
        }
    }
}
