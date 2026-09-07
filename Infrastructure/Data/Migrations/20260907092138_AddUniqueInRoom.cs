using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseRentMgmt.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueInRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Room_UserId",
                table: "Room");

            migrationBuilder.CreateIndex(
                name: "IX_Room_UserId_RoomName",
                table: "Room",
                columns: new[] { "UserId", "RoomName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Room_UserId_RoomName",
                table: "Room");

            migrationBuilder.CreateIndex(
                name: "IX_Room_UserId",
                table: "Room",
                column: "UserId");
        }
    }
}
