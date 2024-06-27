using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoogleMap.Migrations
{
    public partial class updatedbmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "PolygonCenters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "PolygonCenters");
        }
    }
}
