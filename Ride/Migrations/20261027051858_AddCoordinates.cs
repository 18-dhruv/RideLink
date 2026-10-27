using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideService.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DropoffCoordinates_Latitude",
                table: "Rides",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DropoffCoordinates_Longitude",
                table: "Rides",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PickupCoordinates_Latitude",
                table: "Rides",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PickupCoordinates_Longitude",
                table: "Rides",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropoffCoordinates_Latitude",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "DropoffCoordinates_Longitude",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "PickupCoordinates_Latitude",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "PickupCoordinates_Longitude",
                table: "Rides");
        }
    }
}
