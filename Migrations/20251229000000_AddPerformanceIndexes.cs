using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalReservationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add indexes for frequently queried columns
            
            // Reservations - frequently filtered by dates and status
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_StartTime",
                table: "Reservations",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_EndTime",
                table: "Reservations",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                table: "Reservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CreatedAt",
                table: "Reservations",
                column: "CreatedAt");

            // Events - frequently filtered by dates
            migrationBuilder.CreateIndex(
                name: "IX_Events_StartTime",
                table: "Events",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EndTime",
                table: "Events",
                column: "EndTime");

            // Payments - frequently filtered by status
            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAt",
                table: "Payments",
                column: "CreatedAt");

            // Tickets - frequently filtered by status
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status",
                table: "Tickets",
                column: "Status");

            // Companies - frequently filtered by active status
            migrationBuilder.CreateIndex(
                name: "IX_Companies_IsActive",
                table: "Companies",
                column: "IsActive");

            // CompanyMembers - frequently filtered by active status
            migrationBuilder.CreateIndex(
                name: "IX_CompanyMembers_IsActive",
                table: "CompanyMembers",
                column: "IsActive");

            // Composite indexes for common query patterns
            
            // Reservations by resource and date range
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ResourceId_StartTime_EndTime",
                table: "Reservations",
                columns: new[] { "ResourceId", "StartTime", "EndTime" });

            // Reservations by user and status
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId_Status",
                table: "Reservations",
                columns: new[] { "UserId", "Status" });

            // Events by resource and date
            migrationBuilder.CreateIndex(
                name: "IX_Events_ResourceId_StartTime",
                table: "Events",
                columns: new[] { "ResourceId", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop composite indexes
            migrationBuilder.DropIndex(
                name: "IX_Events_ResourceId_StartTime",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_UserId_Status",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ResourceId_StartTime_EndTime",
                table: "Reservations");

            // Drop single column indexes
            migrationBuilder.DropIndex(
                name: "IX_CompanyMembers_IsActive",
                table: "CompanyMembers");

            migrationBuilder.DropIndex(
                name: "IX_Companies_IsActive",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CreatedAt",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Events_EndTime",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_StartTime",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CreatedAt",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_Status",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_EndTime",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_StartTime",
                table: "Reservations");
        }
    }
}
