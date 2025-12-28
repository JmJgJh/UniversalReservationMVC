using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalReservationMVC.Migrations
{
    /// <inheritdoc />
    public partial class ImprovedModelsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_UserId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Events_EventId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Resources_ResourceId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Seats_SeatId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Seats_ResourceId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ResourceId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Events_ResourceId",
                table: "Events");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Tickets",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchasedAt",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Resources",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Reservations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status",
                table: "Tickets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_ResourceId_X_Y",
                table: "Seats",
                columns: new[] { "ResourceId", "X", "Y" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourceType",
                table: "Resources",
                column: "ResourceType");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ResourceId_StartTime_EndTime",
                table: "Reservations",
                columns: new[] { "ResourceId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                table: "Reservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ResourceId_StartTime",
                table: "Events",
                columns: new[] { "ResourceId", "StartTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_UserId",
                table: "Reservations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Events_EventId",
                table: "Reservations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Resources_ResourceId",
                table: "Reservations",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Seats_SeatId",
                table: "Reservations",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_UserId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Events_EventId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Resources_ResourceId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Seats_SeatId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Seats_ResourceId_X_Y",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Resources_ResourceType",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ResourceId_StartTime_EndTime",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_Status",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Events_ResourceId_StartTime",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PurchasedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Reservations");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_ResourceId",
                table: "Seats",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ResourceId",
                table: "Reservations",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ResourceId",
                table: "Events",
                column: "ResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_UserId",
                table: "Reservations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Events_EventId",
                table: "Reservations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Resources_ResourceId",
                table: "Reservations",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Seats_SeatId",
                table: "Reservations",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id");
        }
    }
}
