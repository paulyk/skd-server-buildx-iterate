
using System;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SKD.VCS.Model;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SKD.VCS.Server {
    public class Mutation {

        /// <summary>
        /// Create a vehicle entry
        /// </summary>
        public async Task<MutationPayload<Vehicle>> CreateVehicle(
            [Service] VehicleService service,
            [Service] SkdContext ctx,
            VehicleInput input
        ) {
            var vehicle = new Vehicle {
                VIN = input.Vin,
                ModelId = ToGuid(input.ModelId),
                KitNo = input.KitNo,
                LotNo = input.LotNo
            };
            return await service.CreateVehicle(vehicle);
        }

        /// <summary>
        /// Create or update a component
        /// </summary>
        public async Task<MutationPayload<Component>> SaveComponent(
            [Service] ComponentService service1,
            [Service] SkdContext ctx,
            ComponentInput input
        ) {
            var component = new Component {
                Id = input.Id != null ? new Guid(input.Id) : Guid.NewGuid(),
                Code = input.Code,
                Name = input.Name
            };
            return await service1.SaveComponent(component);
        }

        

        private Guid ToGuid(string str) {
            Guid gOut;
            Guid.TryParse(str, out gOut);
            return gOut;
        }
    }
}
