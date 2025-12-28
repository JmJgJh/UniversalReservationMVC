using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalReservationMVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResourceCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Companies_CompanyId",
                table: "Resources");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Companies_CompanyId",
                table: "Resources",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Companies_CompanyId",
                table: "Resources");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Companies_CompanyId",
                table: "Resources",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
