using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseRentMgmt.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueUserAndTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Tenant",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_UserId_Phone",
                table: "Tenant",
                columns: new[] { "UserId", "Phone" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenant_AspNetUsers_UserId",
                table: "Tenant",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenant_AspNetUsers_UserId",
                table: "Tenant");

            migrationBuilder.DropIndex(
                name: "IX_Tenant_UserId_Phone",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tenant");
        }
    }
}
