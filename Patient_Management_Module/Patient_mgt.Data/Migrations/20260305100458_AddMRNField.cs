using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patient_mgt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMRNField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MRN",
                table: "patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MRN",
                table: "patients");
        }
    }
}
