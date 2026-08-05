using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V20002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalCode",
                table: "UserProfiles",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_NationalCode",
                table: "UserProfiles",
                column: "NationalCode",
                unique: true,
                filter: "[NationalCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_NationalCode",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "NationalCode",
                table: "UserProfiles");
        }
    }
}
