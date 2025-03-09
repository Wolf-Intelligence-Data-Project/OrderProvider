using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderProvider.Migrations.ProductDb
{
    /// <inheritdoc />
    public partial class reservedfromtonullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Modify the ReservedFrom column to be nullable
            migrationBuilder.AlterColumn<DateTime>(
                name: "ReservedFrom",
                table: "Reservations",
                type: "datetime",
                nullable: true, // Set it to nullable
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the ReservedFrom column to not be nullable
            migrationBuilder.AlterColumn<DateTime>(
                name: "ReservedFrom",
                table: "Reservations",
                type: "datetime",
                nullable: false, // Set it back to non-nullable
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);
        }
    }
}
