using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoogleMap.Migrations
{
    public partial class dbmodelupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PolygonPoints_CenterId",
                table: "PolygonPoints",
                column: "CenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_PolygonPoints_PolygonCenters_CenterId",
                table: "PolygonPoints",
                column: "CenterId",
                principalTable: "PolygonCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PolygonPoints_PolygonCenters_CenterId",
                table: "PolygonPoints");

            migrationBuilder.DropIndex(
                name: "IX_PolygonPoints_CenterId",
                table: "PolygonPoints");
        }
    }
}
