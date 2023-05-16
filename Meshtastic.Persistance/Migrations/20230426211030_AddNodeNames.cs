using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meshtastic.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Nodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "Nodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "Nodes");
        }
    }
}
