using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelInDubai.Migrations
{
    /// <inheritdoc />
    public partial class AddClientTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientTag_Clients_ClientId",
                table: "ClientTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientTag",
                table: "ClientTag");

            migrationBuilder.RenameTable(
                name: "ClientTag",
                newName: "ClientTags");

            migrationBuilder.RenameIndex(
                name: "IX_ClientTag_ClientId",
                table: "ClientTags",
                newName: "IX_ClientTags_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientTags",
                table: "ClientTags",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientTags_Clients_ClientId",
                table: "ClientTags",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientTags_Clients_ClientId",
                table: "ClientTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientTags",
                table: "ClientTags");

            migrationBuilder.RenameTable(
                name: "ClientTags",
                newName: "ClientTag");

            migrationBuilder.RenameIndex(
                name: "IX_ClientTags_ClientId",
                table: "ClientTag",
                newName: "IX_ClientTag_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientTag",
                table: "ClientTag",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientTag_Clients_ClientId",
                table: "ClientTag",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }
    }
}
