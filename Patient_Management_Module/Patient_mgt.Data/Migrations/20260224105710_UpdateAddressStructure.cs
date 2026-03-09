using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patient_mgt.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAddressStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "patients");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "patients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "patients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "City",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "State",
                table: "patients");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "patients",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }
    }
}
