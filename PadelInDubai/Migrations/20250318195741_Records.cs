using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PadelInDubai.Migrations
{
    /// <inheritdoc />
    public partial class Records : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecordId",
                table: "Services",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    SuccessVisitsCount = table.Column<int>(type: "integer", nullable: false),
                    FailVisitsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ClientsCount = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    Online = table.Column<bool>(type: "boolean", nullable: false),
                    Confirmed = table.Column<int>(type: "integer", nullable: false),
                    Notified = table.Column<int>(type: "integer", nullable: false),
                    FromUrl = table.Column<string>(type: "text", nullable: false),
                    VisitId = table.Column<int>(type: "integer", nullable: false),
                    CreatedUserId = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    PaidFull = table.Column<int>(type: "integer", nullable: false),
                    Prepaid = table.Column<bool>(type: "boolean", nullable: false),
                    PrepaidConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    LastChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Records_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Records_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_RecordId",
                table: "Services",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Records_ClientId",
                table: "Records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Records_StaffId",
                table: "Records",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Records_RecordId",
                table: "Services",
                column: "RecordId",
                principalTable: "Records",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Records_RecordId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "Records");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Services_RecordId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "Services");
        }
    }
}
