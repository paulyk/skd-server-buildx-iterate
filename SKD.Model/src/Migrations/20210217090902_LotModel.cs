using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class LotModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lot_plant_PlantId",
                table: "lot");

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "lot",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(@"
                update 
                    lot
                set 
                    ModelId = TMP.ModelId
                FROM (
                    select distinct  
                        l.LotNo as LotNo,
                        m.Id as ModelId
                    from kit as k
                    join lot as l on l.Id = k.LotId
                    join vehicle_model as m on m.Id = k.ModelId
                ) AS TMP
                WHERE lot.LotNo = TMP.LotNo            
            ");

            migrationBuilder.CreateIndex(
                name: "IX_lot_ModelId",
                table: "lot",
                column: "ModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_lot_plant_PlantId",
                table: "lot",
                column: "PlantId",
                principalTable: "plant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_lot_vehicle_model_ModelId",
                table: "lot",
                column: "ModelId",
                principalTable: "vehicle_model",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lot_plant_PlantId",
                table: "lot");

            migrationBuilder.DropForeignKey(
                name: "FK_lot_vehicle_model_ModelId",
                table: "lot");

            migrationBuilder.DropIndex(
                name: "IX_lot_ModelId",
                table: "lot");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "lot");

            migrationBuilder.AddForeignKey(
                name: "FK_lot_plant_PlantId",
                table: "lot",
                column: "PlantId",
                principalTable: "plant",
                principalColumn: "Id");
        }
    }
}
