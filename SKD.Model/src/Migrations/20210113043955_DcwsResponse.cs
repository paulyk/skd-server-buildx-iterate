using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class DcwsResponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dcws_response_component_serial_ComponentScanId",
                table: "dcws_response");

            migrationBuilder.RenameColumn(
                name: "ResponseCode",
                table: "dcws_response",
                newName: "ProcessExcptionCode");

            migrationBuilder.RenameColumn(
                name: "ComponentScanId",
                table: "dcws_response",
                newName: "ComponentSerialId");

            migrationBuilder.RenameIndex(
                name: "IX_dcws_response_ComponentScanId",
                table: "dcws_response",
                newName: "IX_dcws_response_ComponentSerialId");

            migrationBuilder.AddForeignKey(
                name: "FK_dcws_response_component_serial_ComponentSerialId",
                table: "dcws_response",
                column: "ComponentSerialId",
                principalTable: "component_serial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dcws_response_component_serial_ComponentSerialId",
                table: "dcws_response");

            migrationBuilder.RenameColumn(
                name: "ProcessExcptionCode",
                table: "dcws_response",
                newName: "ResponseCode");

            migrationBuilder.RenameColumn(
                name: "ComponentSerialId",
                table: "dcws_response",
                newName: "ComponentScanId");

            migrationBuilder.RenameIndex(
                name: "IX_dcws_response_ComponentSerialId",
                table: "dcws_response",
                newName: "IX_dcws_response_ComponentScanId");

            migrationBuilder.AddForeignKey(
                name: "FK_dcws_response_component_serial_ComponentScanId",
                table: "dcws_response",
                column: "ComponentScanId",
                principalTable: "component_serial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
