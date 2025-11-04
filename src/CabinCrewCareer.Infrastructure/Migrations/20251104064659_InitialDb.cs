using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    CityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = ***REMOVED*** "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.CityId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = ***REMOVED*** "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Ward",
                columns: table => new
                {
                    WardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WardName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ward", x => x.WardId);
                    table.ForeignKey(
                        name: "FK_Ward_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AirlinePartner",
                columns: table => new
                {
                    PartnerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    WardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirlinePartner", x => x.PartnerId);
                    table.ForeignKey(
                        name: "FK_AirlinePartner_Ward_WardId",
                        column: x => x.WardId,
                        principalTable: "Ward",
                        principalColumn: "WardId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = ***REMOVED*** "nvarchar(11)", maxLength: 11, nullable: false),
                    FullName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    ImgURL = ***REMOVED*** "nvarchar(max)", nullable: true),
                    Address = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: true),
                    ResetPasswordOtp = ***REMOVED*** "nvarchar(6)", maxLength: 6, nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    WardId = table.Column<int>(type: "int", nullable: true),
                    PartnerId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = ***REMOVED*** "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = ***REMOVED*** "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_User_AirlinePartner_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "AirlinePartner",
                        principalColumn: "PartnerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_User_Ward_WardId",
                        column: x => x.WardId,
                        principalTable: "Ward",
                        principalColumn: "WardId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AirlinePartner_WardId",
                table: "AirlinePartner",
                column: "WardId");

            migrationBuilder.CreateIndex(
                name: "IX_User_PartnerId",
                table: "User",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_User_WardId",
                table: "User",
                column: "WardId");

            migrationBuilder.CreateIndex(
                name: "IX_Ward_CityId",
                table: "Ward",
                column: "CityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "AirlinePartner");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Ward");

            migrationBuilder.DropTable(
                name: "City");
        }
    }
}
