using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Migrations
{
    /// <inheritdoc />
    public partial class _newCarMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Specifications_Cylinders",
                table: "Cars",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specifications_EngineSize",
                table: "Cars",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specifications_FuelType",
                table: "Cars",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Transmission",
                table: "Cars",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specifications_Cylinders",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Specifications_EngineSize",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Specifications_FuelType",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Specifications_Transmission",
                table: "Cars");
        }
    }
}
