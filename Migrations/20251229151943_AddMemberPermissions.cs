using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalReservationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanExportReports",
                table: "CompanyMembers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageEvents",
                table: "CompanyMembers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageMembers",
                table: "CompanyMembers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanViewAnalytics",
                table: "CompanyMembers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanExportReports",
                table: "CompanyMembers");

            migrationBuilder.DropColumn(
                name: "CanManageEvents",
                table: "CompanyMembers");

            migrationBuilder.DropColumn(
                name: "CanManageMembers",
                table: "CompanyMembers");

            migrationBuilder.DropColumn(
                name: "CanViewAnalytics",
                table: "CompanyMembers");
        }
    }
}
