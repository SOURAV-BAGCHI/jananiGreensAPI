using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JANANIGREENS.API.Migrations
{
    public partial class CancellationRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CancellationRequests",
                columns: table => new
                {
                    BookingRequestId = table.Column<string>(maxLength: 200, nullable: false),
                    CustomerName = table.Column<string>(maxLength: 200, nullable: false),
                    RequestDateTime = table.Column<DateTime>(nullable: false),
                    BookingStartDate = table.Column<DateTime>(nullable: false),
                    Reason = table.Column<string>(maxLength: 1000, nullable: true),
                    CancellationAccepted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancellationRequests", x => x.BookingRequestId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CancellationRequests");
        }
    }
}
