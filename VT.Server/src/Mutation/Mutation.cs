
using System;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VT.Model;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VT.Server {
    public class Mutation {

        public async Task<UpdateVehiclePayload> CreateVehicle(
            [Service] VehicleService service,
            [Service] AppDbContext ctx,
            VehicleInput input
        ) {
            var vehicle = new Vehicle() {
                VIN = input.Vin,
                ModelId =  ToGuid(input.ModelId),
                KitNo = input.KitNo,
                LotNo = input.LotNo
            };
            return await service.CreateVehicle(vehicle);
        }

        private Guid ToGuid(string str) {
            Guid gOut;
            Guid.TryParse(str, out gOut);
            return gOut;
        }        
    }
}
