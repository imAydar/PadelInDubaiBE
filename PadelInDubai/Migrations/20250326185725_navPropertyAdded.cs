using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelInDubai.Migrations
{
    /// <inheritdoc />
    public partial class navPropertyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Records_ActivityId",
                table: "Records",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Events_ActivityId",
                table: "Records",
                column: "ActivityId",
                principalTable: "Events",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_Events_ActivityId",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Records_ActivityId",
                table: "Records");
        }
    }
}
