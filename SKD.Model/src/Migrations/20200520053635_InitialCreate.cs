using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "component",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Type = table.Column<string>(maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    Email = table.Column<string>(maxLength: 320, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_model",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    Code = table.Column<string>(maxLength: 11, nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_model", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    VIN = table.Column<string>(maxLength: 17, nullable: false),
                    KitNo = table.Column<string>(nullable: true),
                    LotNo = table.Column<string>(nullable: true),
                    ModelId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_vehicle_model_ModelId",
                        column: x => x.ModelId,
                        principalTable: "vehicle_model",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_model_component",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    VehicleModelId = table.Column<Guid>(nullable: false),
                    ComponentId = table.Column<Guid>(nullable: false),
                    Sequence = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_model_component", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_model_component_component_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_model_component_vehicle_model_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "vehicle_model",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_component",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    VehicleId = table.Column<Guid>(nullable: false),
                    ComponentId = table.Column<Guid>(nullable: false),
                    SerialNumber = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_component", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_component_component_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_component_vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_component_Code",
                table: "component",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_component_Name",
                table: "component",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_ModelId",
                table: "vehicle",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_VIN",
                table: "vehicle",
                column: "VIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_component_ComponentId",
                table: "vehicle_component",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_component_VehicleId_ComponentId",
                table: "vehicle_component",
                columns: new[] { "VehicleId", "ComponentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_Code",
                table: "vehicle_model",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_Name",
                table: "vehicle_model",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_component_ComponentId",
                table: "vehicle_model_component",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_component_VehicleModelId_ComponentId",
                table: "vehicle_model_component",
                columns: new[] { "VehicleModelId", "ComponentId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "vehicle_component");

            migrationBuilder.DropTable(
                name: "vehicle_model_component");

            migrationBuilder.DropTable(
                name: "vehicle");

            migrationBuilder.DropTable(
                name: "component");

            migrationBuilder.DropTable(
                name: "vehicle_model");
        }
    }
}
