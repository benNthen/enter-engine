using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterEngine.Api.Migrations
{
  /// <inheritdoc />
  public partial class AddCategories : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "Categories",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            MinWeightKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
            MaxWeightKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
            IconKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Categories", x => x.Id);
            table.CheckConstraint("CK_Categories_ValidRange", "[MinWeightKg] >= 0 AND ([MaxWeightKg] IS NULL OR [MaxWeightKg] > [MinWeightKg])");
          });

      migrationBuilder.InsertData(
        table: "Categories",
        columns: new[]
        {
                "Name",
                "MinWeightKg",
                "MaxWeightKg",
                "IconKey"
        },
        values: new object[,]
        {
                { "Light", 0m, 500m, "car" },
                { "Medium", 500m, 2500m, "van" },
                { "Heavy", 2500m, null!, "truck" }
        });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "Categories");
    }
  }
}
