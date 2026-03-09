using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patient_mgt.Data.Migrations
{
    /// <inheritdoc />
    public partial class PopulateMRNForExistingPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE patients 
                SET MRN = 'MRN' + RIGHT('000000' + CAST(PatientId AS VARCHAR(6)), 6)
                WHERE MRN = '' OR MRN IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE patients 
                SET MRN = ''
            ");
        }
    }
}
