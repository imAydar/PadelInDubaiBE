using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelInDubai.Migrations
{
    /// <inheritdoc />
    public partial class RecordsToEvents_removed_navProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_Events_ActivityId",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Records_ActivityId",
                table: "Records");

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Records",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Records_EventId",
                table: "Records",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Events_EventId",
                table: "Records",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_Events_EventId",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Records_EventId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Records");

            migrationBuilder.CreateIndex(
                name: "IX_Records_ActivityId",
                table: "Records",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Events_ActivityId",
                table: "Records",
                column: "ActivityId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
