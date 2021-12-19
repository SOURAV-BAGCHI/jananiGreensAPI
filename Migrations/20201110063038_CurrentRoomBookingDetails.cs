using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JANANIGREENS.API.Migrations
{
    public partial class CurrentRoomBookingDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrentRoomBookingDetails",
                columns: table => new
                {
                    RoomBookingDetailsId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FkRoomId = table.Column<long>(nullable: false),
                    FkBookingId = table.Column<long>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentRoomBookingDetails", x => x.RoomBookingDetailsId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentRoomBookingDetails");
        }
    }
}
