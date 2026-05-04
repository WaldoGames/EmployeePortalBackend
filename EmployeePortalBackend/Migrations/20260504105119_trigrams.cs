using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeePortalBackend.Migrations
{
    /// <inheritdoc />
    public partial class trigrams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrigramHashes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    FullnamePartHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrigramHashes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerTrigramHashes",
                columns: table => new
                {
                    CustomersId = table.Column<string>(type: "text", nullable: false),
                    TrigramHashesid = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTrigramHashes", x => new { x.CustomersId, x.TrigramHashesid });
                    table.ForeignKey(
                        name: "FK_CustomerTrigramHashes_Customers_CustomersId",
                        column: x => x.CustomersId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerTrigramHashes_TrigramHashes_TrigramHashesid",
                        column: x => x.TrigramHashesid,
                        principalTable: "TrigramHashes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTrigramHashes_TrigramHashesid",
                table: "CustomerTrigramHashes",
                column: "TrigramHashesid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerTrigramHashes");

            migrationBuilder.DropTable(
                name: "TrigramHashes");
        }
    }
}
