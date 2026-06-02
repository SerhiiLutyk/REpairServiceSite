using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GadgetFix.Users.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramLinkCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramLinkCode",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramLinkCode",
                table: "Users");
        }
    }
}
