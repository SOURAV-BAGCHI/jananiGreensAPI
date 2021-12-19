using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JANANIGREENS.API.Migrations
{
    public partial class BookingRequestQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingRequestQueue",
                columns: table => new
                {
                    BookingRequestId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Phone = table.Column<string>(maxLength: 20, nullable: true),
                    Email = table.Column<string>(maxLength: 500, nullable: true),
                    BookingStartDate = table.Column<DateTime>(nullable: false),
                    BookingEndDate = table.Column<DateTime>(nullable: false),
                    RoomOrderDetails = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRequestQueue", x => x.BookingRequestId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingRequestQueue");
        }
    }
}
