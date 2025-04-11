using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelInDubai.Migrations
{
    /// <inheritdoc />
    public partial class levelAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Clients",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Clients");
        }
    }
}
