using System;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class DCWSResponseService_Test : TestBase {

        public DCWSResponseService_Test() {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();
        }

        [Fact]
        public async Task can_create_dcws_response() {
            // setup
            var vehicle = context.Kits.First();
            var vehicleComponent = vehicle.KitComponents.First();
            var componentScan = Gen_ComponentScan(vehicleComponent.Id);

            // act
            var service = new DCWSResponseService(context);
            var input = new DcwsComponentResponseInput {
                VehicleComponentId = vehicleComponent.Id,
                ResponseCode = "NONE",
                ErrorMessage = ""
            };
            var payload = await service.SaveDcwsComponentResponse(input);
            // assert
            Assert.True(payload.Errors.Count() == 0, "error count should be 0");
            var responseCoount = context.DCWSResponses.Count();
            Assert.True(responseCoount == 1, "should have 1 DCWSResponse entry");

            var response = context.DCWSResponses
                .Include(t => t.ComponentSerial).ThenInclude(t => t.KitComponent)
                .FirstOrDefault(t => t.Id == payload.Entity.Id);

            Assert.True(response.ComponentSerial.VerifiedAt != null, "component scan AcceptedAt should be set");
            Assert.True(response.ComponentSerial.KitComponent.VerifiedAt != null, "vehicle component ScanVerifiedAt should be set");
        }

        [Fact]
        public async Task previous_dcws_response_codes_marked_removed_when_new_one_submitted() {
            // setup
            var vehicle = context.Kits.First();
            var vehicleComponent = vehicle.KitComponents.First();
            var comonentSerial = Gen_ComponentScan(vehicleComponent.Id);

            var service = new DCWSResponseService(context);
            var input = new DcwsComponentResponseInput {
                VehicleComponentId = vehicleComponent.Id,
                ResponseCode = "INVALIDSCAN",
                ErrorMessage = ""
            };

            var input_2 = new DcwsComponentResponseInput {
                VehicleComponentId = vehicleComponent.Id,
                ResponseCode = "NONE",
                ErrorMessage = ""
            };

            // act
            await service.SaveDcwsComponentResponse(input);
            await service.SaveDcwsComponentResponse(input_2);

            var csr = await context.ComponentSerials.Include(t => t.DcwsResponses)
                .FirstOrDefaultAsync(t => t.Id == comonentSerial.Id);

            var dcws_resposne_count = csr.DcwsResponses.Count();
            Assert.Equal(2, dcws_resposne_count);

            var by_date = csr.DcwsResponses.OrderByDescending(t => t.CreatedAt).ToList();

            var latestOne = by_date[0];
            var firstOne = by_date[1];
            // latest one not removed
            Assert.Null(latestOne.RemovedAt);
            Assert.NotNull(firstOne.RemovedAt);
        }

        [Fact]
        public async Task ignores_dcws_response_if_matches_latest_entry() {
            var vehicle = context.Kits.First();
            var vehicleComponent = vehicle.KitComponents.First();
            var comonentSerial = Gen_ComponentScan(vehicleComponent.Id);

            var service = new DCWSResponseService(context);
            var input = new DcwsComponentResponseInput {
                VehicleComponentId = vehicleComponent.Id,
                ResponseCode = "INVALIDSCAN",
                ErrorMessage = ""
            };

            // act
            await service.SaveDcwsComponentResponse(input);
            await service.SaveDcwsComponentResponse(input);

            var csr = await context.ComponentSerials.Include(t => t.DcwsResponses)
              .FirstOrDefaultAsync(t => t.Id == comonentSerial.Id);

            var dcws_resposne_count = csr.DcwsResponses.Count();
            Assert.Equal(1, dcws_resposne_count);
        }
    }
}