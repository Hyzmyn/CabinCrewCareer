using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinCrewCareer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurationsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveDate",
                table: "ScoringCriteriaItem",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "ScoringCriteriaItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveDate",
                table: "ScoringCriteriaDetail",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "ScoringCriteriaDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveDate",
                table: "ScoringCriteria",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "ScoringCriteria",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveDate",
                table: "InterviewCriteriaItem",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "InterviewCriteriaItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveDate",
                table: "InterviewCriteria",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "InterviewCriteria",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RoundConfiguration",
                columns: table => new
                {
                    RoundConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationType = table.Column<int>(type: "int", nullable: false),
                    CampaignType = table.Column<int>(type: "int", nullable: false),
                    Benchmark = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundConfiguration", x => x.RoundConfigurationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoundConfiguration");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "ScoringCriteriaItem");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ScoringCriteriaItem");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "ScoringCriteriaDetail");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ScoringCriteriaDetail");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "ScoringCriteria");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ScoringCriteria");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "InterviewCriteriaItem");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InterviewCriteriaItem");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "InterviewCriteria");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InterviewCriteria");

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
        }
    }
}
