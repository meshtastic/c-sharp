using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meshtastic.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeHwModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HardwareModel",
                table: "Nodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HardwareModel",
                table: "Nodes");
        }
    }
}
