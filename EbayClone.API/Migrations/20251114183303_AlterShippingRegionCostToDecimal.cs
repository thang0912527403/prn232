using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EbayClone.API.Migrations
{
    /// <inheritdoc />
    public partial class AlterShippingRegionCostToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "38cc9973-5a06-49af-ac88-675f3d416e90");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "ShippingRegions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "PasswordHash", "Rating", "Role", "TotalSales", "TrustLevel", "UserName" },
                values: new object[] { "91492a58-55cc-46e0-9c79-077dd0e79961", "admin@ebayclone.com", "admin123", 5.0m, "Admin", 0, 0, "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "91492a58-55cc-46e0-9c79-077dd0e79961");

            migrationBuilder.AlterColumn<string>(
                name: "Cost",
                table: "ShippingRegions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "PasswordHash", "Rating", "Role", "TotalSales", "TrustLevel", "UserName" },
                values: new object[] { "38cc9973-5a06-49af-ac88-675f3d416e90", "admin@ebayclone.com", "admin123", 5.0m, "Admin", 0, 0, "Admin" });
        }
    }
}
