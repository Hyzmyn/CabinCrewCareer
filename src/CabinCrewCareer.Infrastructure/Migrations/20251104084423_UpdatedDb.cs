using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignRequest",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectReason = ***REMOVED*** "nvarchar(max)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PartnerId = table.Column<int>(type: "int", nullable: true),
                    DirectorId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = ***REMOVED*** "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = ***REMOVED*** "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRequest", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_CampaignRequest_AirlinePartner_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "AirlinePartner",
                        principalColumn: "PartnerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignRequest_User_DirectorId",
                        column: x => x.DirectorId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NotificationType",
                columns: table => new
                {
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTypeName = ***REMOVED*** "nvarchar(25)", maxLength: 25, nullable: false),
                    Description = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationType", x => x.NotificationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Campaign",
                columns: table => new
                {
                    CampaignId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    CampaignType = table.Column<int>(type: "int", nullable: false),
                    Description = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetQuantity = table.Column<int>(type: "int", nullable: false),
                    ActualQuantiy = table.Column<int>(type: "int", nullable: false),
                    CampaignStatus = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SeniorId = table.Column<int>(type: "int", nullable: true),
                    RequestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaign", x => x.CampaignId);
                    table.ForeignKey(
                        name: "FK_Campaign_CampaignRequest_RequestId",
                        column: x => x.RequestId,
                        principalTable: "CampaignRequest",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Campaign_User_SeniorId",
                        column: x => x.SeniorId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = ***REMOVED*** "nvarchar(25)", maxLength: 25, nullable: false),
                    Body = ***REMOVED*** "nvarchar(100)", maxLength: 100, nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    NotificationTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notification_NotificationType_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalTable: "NotificationType",
                        principalColumn: "NotificationTypeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notification_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CampaignRound",
                columns: table => new
                {
                    CampaignRoundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundName = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetQuantity = table.Column<int>(type: "int", nullable: false),
                    ActualQuantiy = table.Column<int>(type: "int", nullable: false),
                    RoundStatus = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRound", x => x.CampaignRoundId);
                    table.ForeignKey(
                        name: "FK_CampaignRound_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Round",
                columns: table => new
                {
                    RoundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Notes = ***REMOVED*** "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CampaignRoundId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Round", x => x.RoundId);
                    table.ForeignKey(
                        name: "FK_Round_CampaignRound_CampaignRoundId",
                        column: x => x.CampaignRoundId,
                        principalTable: "CampaignRound",
                        principalColumn: "CampaignRoundId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserApplication",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationStatus = table.Column<int>(type: "int", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RoundId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApplication", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_UserApplication_Round_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "RoundId",
                        onDelete: ReferentialAction.SetNull);
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
                    DocumentURL = ***REMOVED*** "nvarchar(max)", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: true)
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
                name: "IX_Campaign_RequestId",
                table: "Campaign",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaign_SeniorId",
                table: "Campaign",
                column: "SeniorId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequest_DirectorId",
                table: "CampaignRequest",
                column: "DirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequest_PartnerId",
                table: "CampaignRequest",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRound_CampaignId",
                table: "CampaignRound",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_ApplicationId",
                table: "Document",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_NotificationTypeId",
                table: "Notification",
                column: "NotificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Round_CampaignRoundId",
                table: "Round",
                column: "CampaignRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplication_RoundId",
                table: "UserApplication",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplication_UserId",
                table: "UserApplication",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "UserApplication");

            migrationBuilder.DropTable(
                name: "NotificationType");

            migrationBuilder.DropTable(
                name: "Round");

            migrationBuilder.DropTable(
                name: "CampaignRound");

            migrationBuilder.DropTable(
                name: "Campaign");

            migrationBuilder.DropTable(
                name: "CampaignRequest");
        }
    }
}
