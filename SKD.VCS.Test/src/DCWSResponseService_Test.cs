using System;
using System.Collections.Generic;
using SKD.VCS.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.VCS.Test {
    public class DCWSResponseService_Test : TestBase {

        private SkdContext ctx;
        public DCWSResponseService_Test() {
            ctx = GetAppDbContext();
        }

        [Fact]
        public async Task can_create_dcws_response() {
            // setup
            var vehicle = Gen_VehicleModel_With_Vehicle(
                ctx,
                vin: Util.RandomString(EntityFieldLen.Vehicle_VIN),

                lotNo: Util.RandomString(EntityFieldLen.Vehicle_LotNo),
                modelCode: Util.RandomString(EntityFieldLen.VehicleModel_Code),
                component_stations_maps: new List<(string, string)> {
                ("component_1", "station_1")
            });

            var vehicleComponent = vehicle.VehicleComponents.First();
            var componentScan = Gen_ComponentScan(ctx, vehicleComponent.Id);

            // act
            var service = new DCWSResponseService(ctx);
            var dto = new DCWWResponseDTO {
                ComponentScanId = componentScan.Id,
                ResponseCode = "NONE",
                ErrorMessage = ""
            };
            var payload = await service.CreateDCWSResponse(dto);
            // assert
            Assert.True(payload.Errors.Count() == 0, "error count should be 0");
            var responseCoount = ctx.DCWSResponses.Count();
            Assert.True(responseCoount == 1, "should have 1 DCWSResponse entry");

            var response = ctx.DCWSResponses
                .Include(t => t.ComponentScan).ThenInclude(t => t.VehicleComponent)
                .FirstOrDefault(t => t.Id == payload.Entity.Id);

            Assert.True(response.ComponentScan.AcceptedAt != null, "component scan AcceptedAt should be set");
            Assert.True(response.ComponentScan.VehicleComponent.ScanVerifiedAt != null, "vehicle component ScanVerifiedAt should be set");
        }

        [Fact]
        public async Task cannot_create_duplicate_dcws_response_code() {

            var vehicle = Gen_VehicleModel_With_Vehicle(
                ctx,
                vin: Util.RandomString(EntityFieldLen.Vehicle_VIN),

                lotNo: Util.RandomString(EntityFieldLen.Vehicle_LotNo),
                modelCode: Util.RandomString(EntityFieldLen.VehicleModel_Code),
                component_stations_maps: new List<(string, string)> {
                            ("component_1", "station_1")
            });

            var vehicleComponent = vehicle.VehicleComponents.First();
            var componentScan = Gen_ComponentScan(ctx, vehicleComponent.Id);

            var service = new DCWSResponseService(ctx);
            var dto = new DCWWResponseDTO {
                ComponentScanId = componentScan.Id,
                ResponseCode = "NONE",
                ErrorMessage = ""
            };
            var payload = await service.CreateDCWSResponse(dto);
            Assert.True(payload.Errors.Count() == 0, "error count should be 0");
            // dpulicate
            var payload_2 = await service.CreateDCWSResponse(dto);
            Assert.True(payload_2.Errors.Count() == 1, "should have one error");
            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.True(errorMessage == "duplicate");
        }
    }
}