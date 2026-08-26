using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseRentMgmt.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserIdUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationCode_UserId",
                table: "EmailVerificationCode");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationCode_UserId",
                table: "EmailVerificationCode",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationCode_UserId",
                table: "EmailVerificationCode");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationCode_UserId",
                table: "EmailVerificationCode",
                column: "UserId");
        }
    }
}
