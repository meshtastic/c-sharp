using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meshtastic.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddAltitudeToNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastAltitude",
                table: "Nodes",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastAltitude",
                table: "Nodes");
        }
    }
}
