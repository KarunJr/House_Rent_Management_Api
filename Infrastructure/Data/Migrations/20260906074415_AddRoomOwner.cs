using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseRentMgmt.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Room",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Room_UserId",
                table: "Room",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Room_AspNetUsers_UserId",
                table: "Room",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Room_AspNetUsers_UserId",
                table: "Room");

            migrationBuilder.DropIndex(
                name: "IX_Room_UserId",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Room");
        }
    }
}
