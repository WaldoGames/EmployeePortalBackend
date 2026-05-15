using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeePortalBackend.Migrations
{
    /// <inheritdoc />
    public partial class changeddatatypecreatedat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "link",
                table: "IdRequests",
                newName: "ObjectKey");

            migrationBuilder.Sql(@"
                ALTER TABLE ""IdRequests""
                ALTER COLUMN ""ValidUntilDate"" TYPE timestamp with time zone
                USING ""ValidUntilDate""::timestamp with time zone;
            ");

                    migrationBuilder.Sql(@"
                ALTER TABLE ""IdRequests""
                ALTER COLUMN ""CreatedDate"" TYPE timestamp with time zone
                USING ""CreatedDate""::timestamp with time zone;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ObjectKey",
                table: "IdRequests",
                newName: "link");

            migrationBuilder.AlterColumn<string>(
                name: "ValidUntilDate",
                table: "IdRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedDate",
                table: "IdRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
