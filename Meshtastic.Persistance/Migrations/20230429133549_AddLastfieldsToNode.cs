using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meshtastic.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddLastfieldsToNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastBatteryLevel",
                table: "Nodes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LastLatitude",
                table: "Nodes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LastLongitude",
                table: "Nodes",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastBatteryLevel",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "LastLatitude",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "LastLongitude",
                table: "Nodes");
        }
    }
}
