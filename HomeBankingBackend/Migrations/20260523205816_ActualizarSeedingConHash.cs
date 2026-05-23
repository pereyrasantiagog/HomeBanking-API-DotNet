using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBankingBackend.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarSeedingConHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$11$XNB/Y6OpOH0P4z3.egWGQelj09B6IvmaIJ9CQTJYDU1aXNF8Qxf5i");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 102,
                column: "Password",
                value: "$2a$11$Lk0FKuPttS9re2TsChcI1eM5jl2NxdVExQlPGeBSVytbqL2FtEGM6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "password123");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 102,
                column: "Password",
                value: "password456");
        }
    }
}
