using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoogleMap.Migrations
{
    public partial class updatedbmodels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PolygonCenter",
                table: "PolygonCenter");

            migrationBuilder.RenameTable(
                name: "PolygonCenter",
                newName: "PolygonCenters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PolygonCenters",
                table: "PolygonCenters",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PolygonCenters",
                table: "PolygonCenters");

            migrationBuilder.RenameTable(
                name: "PolygonCenters",
                newName: "PolygonCenter");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PolygonCenter",
                table: "PolygonCenter",
                column: "Id");
        }
    }
}
