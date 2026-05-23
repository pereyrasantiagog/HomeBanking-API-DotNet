using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HomeBankingBackend.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsersAndAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "Password" },
                values: new object[,]
                {
                    { 101, "santiago@ejemplo.com", "Santiago", "Perez", "password123" },
                    { 102, "maria@ejemplo.com", "Maria", "Gomez", "password456" }
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Balance", "CreationDate", "Number", "UserId" },
                values: new object[,]
                {
                    { 101, 150000m, new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "VIN-00000101", 101 },
                    { 102, 50000m, new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "VIN-00000102", 102 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 102);
        }
    }
}
