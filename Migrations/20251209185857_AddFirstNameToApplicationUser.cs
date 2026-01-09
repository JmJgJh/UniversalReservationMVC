using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalReservationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddFirstNameToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "SeatX",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "SeatY",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "Purchased",
                table: "Tickets",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "Reservations",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "End",
                table: "Reservations",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "Events",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "End",
                table: "Events",
                newName: "EndTime");

            migrationBuilder.AddColumn<int>(
                name: "Column",
                table: "Seats",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Seats",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Row",
                table: "Seats",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Resources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResourceType",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SeatId",
                table: "Reservations",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_UserId",
                table: "Reservations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Seats_SeatId",
                table: "Reservations",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_UserId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Seats_SeatId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_SeatId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Column",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "Row",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ResourceType",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Tickets",
                newName: "Purchased");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Reservations",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Reservations",
                newName: "End");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Events",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Events",
                newName: "End");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Resources",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SeatX",
                table: "Reservations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeatY",
                table: "Reservations",
                type: "INTEGER",
                nullable: true);
        }
    }
}
