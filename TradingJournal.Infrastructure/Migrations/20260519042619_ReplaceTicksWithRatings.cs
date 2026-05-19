using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingJournal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTicksWithRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAppropriateDte",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "HasPositionSizing",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "HasProfitTarget",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "HasStopLoss",
                table: "Trades");

            migrationBuilder.AddColumn<int>(
                name: "EntryQuality",
                table: "Trades",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExitQuality",
                table: "Trades",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanAdherence",
                table: "Trades",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskManagement",
                table: "Trades",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntryQuality",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "ExitQuality",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "PlanAdherence",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "RiskManagement",
                table: "Trades");

            migrationBuilder.AddColumn<bool>(
                name: "HasAppropriateDte",
                table: "Trades",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPositionSizing",
                table: "Trades",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasProfitTarget",
                table: "Trades",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasStopLoss",
                table: "Trades",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
