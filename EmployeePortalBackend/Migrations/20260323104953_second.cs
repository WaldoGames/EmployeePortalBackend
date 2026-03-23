using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EmployeePortalBackend.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CustomerSenstives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSenstives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSenstives_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<string>(type: "text", nullable: false),
                    EmployeeId = table.Column<string>(type: "text", nullable: false),
                    EmployeeName = table.Column<string>(type: "text", nullable: false),
                    link = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<string>(type: "text", nullable: false),
                    ValidUntilDate = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdRequests_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomerId = table.Column<string>(type: "text", nullable: false),
                    CreatorEmployeeId = table.Column<string>(type: "text", nullable: false),
                    CreatorEmployeeName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSenstives_CustomerId",
                table: "CustomerSenstives",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdRequests_CustomerId",
                table: "IdRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId",
                table: "Tickets",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerSenstives");

            migrationBuilder.DropTable(
                name: "IdRequests");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropColumn(
                name: "email",
                table: "Customers");
        }
    }
}
