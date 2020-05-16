
using System.Collections.Generic;

namespace VT.Model {

    public class UpdateVehiclePayload : MutationPayloadBase {
        public Vehicle Vehicle { get; set; }
    }
}