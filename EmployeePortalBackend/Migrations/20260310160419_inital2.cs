using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeePortalBackend.Migrations
{
    /// <inheritdoc />
    public partial class inital2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstNameHash",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstNameHash",
                table: "Customers");
        }
    }
}
