using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meshtastic.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilizationFieldsToNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "LastAirUtilTx",
                table: "Nodes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LastChannelUtilTx",
                table: "Nodes",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastAirUtilTx",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "LastChannelUtilTx",
                table: "Nodes");
        }
    }
}
