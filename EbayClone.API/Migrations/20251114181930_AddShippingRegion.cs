using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EbayClone.API.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingRegion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "92a2ef13-725b-4549-9ab2-10bd090ddecc");

            migrationBuilder.CreateTable(
                name: "ShippingRegions",
                columns: table => new
                {
                    ShippingRegionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cost = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingRegions", x => x.ShippingRegionId);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "PasswordHash", "Rating", "Role", "TotalSales", "TrustLevel", "UserName" },
                values: new object[] { "38cc9973-5a06-49af-ac88-675f3d416e90", "admin@ebayclone.com", "admin123", 5.0m, "Admin", 0, 0, "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingRegions");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "38cc9973-5a06-49af-ac88-675f3d416e90");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "PasswordHash", "Rating", "Role", "TotalSales", "TrustLevel", "UserName" },
                values: new object[] { "92a2ef13-725b-4549-9ab2-10bd090ddecc", "admin@ebayclone.com", "admin123", 5.0m, "Admin", 0, 0, "Admin" });
        }
    }
}
