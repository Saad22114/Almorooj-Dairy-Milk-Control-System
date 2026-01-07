using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmersApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMilkEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MilkEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FarmerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MilkType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "cow"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Temperature = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Density = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Quality = table.Column<int>(type: "int", nullable: true),
                    CalculatedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    Device = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntryDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnteredBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkEntries", x => x.Id);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_MilkEntries_FarmerCode",
                table: "MilkEntries",
                column: "FarmerCode");

            migrationBuilder.CreateIndex(
                name: "IX_MilkEntries_EntryDateTime",
                table: "MilkEntries",
                column: "EntryDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_MilkEntries_Status",
                table: "MilkEntries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MilkEntries_MilkType",
                table: "MilkEntries",
                column: "MilkType");

            migrationBuilder.CreateIndex(
                name: "IX_MilkEntries_EntryDateTime_FarmerCode",
                table: "MilkEntries",
                columns: new[] { "EntryDateTime", "FarmerCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MilkEntries");
        }
    }
}
