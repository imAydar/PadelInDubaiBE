using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelInDubai.Migrations
{
    /// <inheritdoc />
    public partial class navPropertyAdded3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Services_ServiceId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_Events_ActivityId",
                table: "Records");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_Events_EventId",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Records_ActivityId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Records");

            migrationBuilder.AlterColumn<int>(
                name: "EventId",
                table: "Records",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Services_ServiceId",
                table: "Events",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Events_EventId",
                table: "Records",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Services_ServiceId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_Events_EventId",
                table: "Records");

            migrationBuilder.AlterColumn<int>(
                name: "EventId",
                table: "Records",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                table: "Records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Records_ActivityId",
                table: "Records",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Services_ServiceId",
                table: "Events",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Events_ActivityId",
                table: "Records",
                column: "ActivityId",
                principalTable: "Events",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Events_EventId",
                table: "Records",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");
        }
    }
}
