using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JANANIGREENS.API.Migrations
{
    public partial class BookingDetailsAndStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingDetailsAndStatus",
                columns: table => new
                {
                    BookingId = table.Column<string>(nullable: false),
                    Checkin = table.Column<DateTime>(nullable: false),
                    Checkout = table.Column<DateTime>(nullable: false),
                    Status = table.Column<short>(nullable: false),
                    RoomCheckinCheckoutDetails = table.Column<string>(nullable: true),
                    IsProcessComplete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingDetailsAndStatus", x => x.BookingId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingDetailsAndStatus");
        }
    }
}
