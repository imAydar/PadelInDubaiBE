using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelInDubai.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintForEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_ServiceId",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ServiceId_StaffId_Date",
                table: "Events",
                columns: new[] { "ServiceId", "StaffId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_ServiceId_StaffId_Date",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ServiceId",
                table: "Events",
                column: "ServiceId");
        }
    }
}
