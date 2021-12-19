using Microsoft.EntityFrameworkCore.Migrations;

namespace JANANIGREENS.API.Migrations
{
    public partial class BookingDetailsAndStatusUpdatedStatusList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusList",
                table: "BookingDetailsAndStatus",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusList",
                table: "BookingDetailsAndStatus");
        }
    }
}
