using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmersApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Farmers",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Nid = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Center = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Bank = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    BankAcc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankSwift = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AnimalCount = table.Column<int>(type: "int", nullable: false),
                    ExpectedQty = table.Column<int>(type: "int", nullable: false),
                    Maximum = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farmers", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Port = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BaudRate = table.Column<int>(type: "int", nullable: false),
                    SensorMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PortQuantity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BaudRateQuantity = table.Column<int>(type: "int", nullable: false),
                    QuantityMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MilkPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MilkPriceCow = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MilkPriceCamel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "BaudRate", "BaudRateQuantity", "Currency", "MilkPrice", "MilkPriceCamel", "MilkPriceCow", "Port", "PortQuantity", "QuantityMode", "SensorMode", "UpdatedAt" },
                values: new object[] { 1, 2400, 9600, "OMR", 0m, 0.4m, 0.25m, "COM1", null, "manual", "automatic", new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Farmers_Center",
                table: "Farmers",
                column: "Center");

            migrationBuilder.CreateIndex(
                name: "IX_Farmers_Name",
                table: "Farmers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Farmers_Status",
                table: "Farmers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Farmers_Type",
                table: "Farmers",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Farmers");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
