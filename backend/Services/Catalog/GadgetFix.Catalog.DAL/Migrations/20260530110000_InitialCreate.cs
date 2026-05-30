using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GadgetFix.Catalog.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Slug = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepairServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    EstimatedDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairServices_DeviceTypes_DeviceTypeId",
                        column: x => x.DeviceTypeId,
                        principalTable: "DeviceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DeviceTypes",
                columns: new[] { "Id", "Icon", "Name", "Slug" },
                values: new object[,]
                {
                    { 1, "smartphone", "Смартфон", "smartphone" },
                    { 2, "laptop", "Ноутбук", "laptop" },
                    { 3, "tablet", "Планшет", "tablet" },
                    { 4, "watch", "Смарт-годинник", "watch" }
                });

            migrationBuilder.InsertData(
                table: "RepairServices",
                columns: new[] { "Id", "BasePrice", "DeviceTypeId", "EstimatedDays", "Name" },
                values: new object[,]
                {
                    { 1, 1800m, 1, 1, "Заміна екрана" },
                    { 2, 900m, 1, 1, "Заміна акумулятора" },
                    { 3, 1500m, 1, 3, "Ремонт після потрапляння води" },
                    { 4, 700m, 2, 1, "Чистка та заміна термопасти" },
                    { 5, 1200m, 2, 1, "Апгрейд SSD / RAM" },
                    { 6, 1400m, 3, 2, "Заміна тачскріна" },
                    { 7, 1100m, 4, 2, "Заміна скла дисплея" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTypes_Slug",
                table: "DeviceTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairServices_DeviceTypeId",
                table: "RepairServices",
                column: "DeviceTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairServices");

            migrationBuilder.DropTable(
                name: "DeviceTypes");
        }
    }
}
