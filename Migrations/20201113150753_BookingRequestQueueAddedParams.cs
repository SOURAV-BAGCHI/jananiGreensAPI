using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JANANIGREENS.API.Migrations
{
    public partial class BookingRequestQueueAddedParams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "BookingRequestQueue",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "VerificationCode",
                table: "BookingRequestQueue",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationLimit",
                table: "BookingRequestQueue",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "BookingRequestQueue");

            migrationBuilder.DropColumn(
                name: "VerificationCode",
                table: "BookingRequestQueue");

            migrationBuilder.DropColumn(
                name: "VerificationLimit",
                table: "BookingRequestQueue");
        }
    }
}
