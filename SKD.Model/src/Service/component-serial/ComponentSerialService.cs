#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SKD.Dcws;

namespace SKD.Model {

    public class ComponentSerialService {
        private readonly SkdContext context;
        private readonly DcwsSerialFormatter serialFormatter;

        public ComponentSerialService(SkdContext ctx, DcwsSerialFormatter serialFormatter) {
            this.context = ctx;
            this.serialFormatter = serialFormatter;
        }

        public async Task<MutationPayload<ComponentSerialDTO>> CaptureComponentSerial(ComponentSerialInput input) {
            input = SwapSerial(input);

            var payload = new MutationPayload<ComponentSerialDTO>(null);

            payload.Errors = await ValidateCaptureComponentSerial<ComponentSerialInput>(input with { });
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            var kitComponent = await context.KitComponents
                .Include(t => t.Component)
                .FirstOrDefaultAsync(t => t.Id == input.KitComponentId);

            // Some serial1 must be formatted acording to DCWS rules
            // The following formats EN / TR serials if they need adjusting
            // Other compoent serials are returned unchanged.
            var formatSerialResult = serialFormatter.FormatSerial(kitComponent.Component.Code, input.Serial1);

            // create
            var componentSerial = new ComponentSerial {
                KitComponentId = input.KitComponentId,
                Serial1 = formatSerialResult.Serial,
                Serial2 = input.Serial2,
                // only set Origian_Serial1 if it is different
                Original_Serial1 = formatSerialResult.Serial != input.Serial1
                    ? input.Serial1
                    : "",
            };

            // Deactivate existing scan if Replace == true
            if (input.Replace) {
                var existintScans = await context.ComponentSerials
                    .Where(t => t.KitComponentId == input.KitComponentId && t.RemovedAt == null).ToListAsync();
                existintScans.ForEach(t => t.RemovedAt = DateTime.UtcNow);
            }

            // add             
            context.ComponentSerials.Add(componentSerial);

            // save
            await context.SaveChangesAsync();

            payload.Entity = await context.ComponentSerials
                .Where(t => t.Id == componentSerial.Id)
                .Select(t => new ComponentSerialDTO {
                    ComponentSerialId = t.Id,
                    VIN = t.KitComponent.Kit.VIN,
                    LotNo = t.KitComponent.Kit.Lot.LotNo,
                    ComponentCode = t.KitComponent.Component.Code,
                    ComponentName = t.KitComponent.Component.Name,
                    ProductionStationCode = t.KitComponent.ProductionStation.Code,
                    ProductionStationName = t.KitComponent.ProductionStation.Name,
                    Serial1 = t.Serial1,
                    Serial2 = t.Serial2,
                    VerifiedAt = t.VerifiedAt,
                    CreatedAt = t.CreatedAt
                }).FirstOrDefaultAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateCaptureComponentSerial<T>(ComponentSerialInput input) where T : ComponentSerialInput {
            var errors = new List<Error>();

            var targetKitCmponent = await context.KitComponents
                .Include(t => t.ProductionStation)
                .FirstOrDefaultAsync(t => t.Id == input.KitComponentId);

            if (targetKitCmponent == null) {
                errors.Add(new Error("KitComponentId", $"kit component not found: {input.KitComponentId}"));
                return errors;
            }

            if (targetKitCmponent.RemovedAt != null) {
                errors.Add(new Error("KitComponentId", $"kit component removed: {input.KitComponentId}"));
                return errors;
            }

            // serial numbers blank
            if (input.Serial1 is null or "" && input.Serial2 is null or "") {
                errors.Add(new Error("", "no serial numbers provided"));
                return errors;
            }

            // serial not null and numbers identical 
            if (input.Serial1 is not null or "" && input.Serial1 == input.Serial2) {
                errors.Add(new Error("", "serial 1 and 2 are the same"));
                return errors;
            }

            // Transform input.Serial1 before continuing the validateion
            var kitComponent = await context.KitComponents
                .Include(t => t.Component)
                .FirstOrDefaultAsync(t => t.Id == input.KitComponentId);
            var formatResult = serialFormatter.FormatSerial(kitComponent.Component.Code, input.Serial1);
            // set input.Serial1 to the formatted result before proceeding with the validation
            input = input with { Serial1 = formatResult.Serial };

            if (!formatResult.Success) {
                errors.Add(new Error("", formatResult.Message));
                return errors;
            }

            // component serial entry for this kit component 
            var componentSerialForKitComponent = await context.ComponentSerials
                .Include(t => t.KitComponent).ThenInclude(t => t.Kit)
                .Include(t => t.KitComponent).ThenInclude(t => t.Component)
                .Include(t => t.KitComponent).ThenInclude(t => t.ProductionStation)
                .Where(t => t.KitComponent.Id == input.KitComponentId)
                .Where(t => t.RemovedAt == null)
                .FirstOrDefaultAsync();

            if (componentSerialForKitComponent != null && !input.Replace) {
                var vin = componentSerialForKitComponent.KitComponent.Kit.VIN;
                var stationCode = componentSerialForKitComponent.KitComponent.ProductionStation.Code;
                var componentCode = componentSerialForKitComponent.KitComponent.Component.Code;
                errors.Add(new Error("", $"component serial already captured for this component: {vin}-{stationCode}-{componentCode}"));
                return errors;
            }

            // serial no already in use by different kit component
            var componentSerials_with_same_serialNo = await context.ComponentSerials
                .Include(t => t.KitComponent).ThenInclude(t => t.Component)
                .Where(t => t.RemovedAt == null)
                // different component
                .Where(t => t.KitComponent.Id != targetKitCmponent.Id)
                // exclude if same kit and same component code   
                // ....... Engine component code will be scanned muliple times)
                .Where(t => !(
                    // same kit
                    t.KitComponent.KitId == targetKitCmponent.KitId
                    &&
                    // same component
                    t.KitComponent.ComponentId == targetKitCmponent.ComponentId
                    )
                )
                // user could point scan serial 2 before serial 1, so we check for both
                .Where(t =>
                    t.Serial1 == input.Serial1 && t.Serial2 == input.Serial2
                    ||
                    t.Serial1 == input.Serial2 && t.Serial2 == input.Serial1
                )
                .ToListAsync();


            if (componentSerials_with_same_serialNo.Count() > 0) {
                errors.Add(new Error("", $"serial number already in use by aonther entry"));
                return errors;
            }

            /* MULTI STATION COMPONENT
            *  Some components serials should be captured repeatedly in consequitive production sations.
            *  They should not be captured out of order
            */

            var preeceedingRequiredComponentEntriesNotCaptured = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                // save kit         
                .Where(t => t.Kit.Id == targetKitCmponent.KitId)
                // same component id
                .Where(t => t.ComponentId == targetKitCmponent.ComponentId)
                // preceeding target kit component
                .Where(t => t.ProductionStation.Sequence < targetKitCmponent.ProductionStation.Sequence)
                // no captured serial entries
                .Where(t => !t.ComponentSerials.Any(u => u.RemovedAt == null))
                .Select(t => new {
                    ProductionStationCode = t.ProductionStation.Code,
                })
                .ToListAsync();

            if (preeceedingRequiredComponentEntriesNotCaptured.Any()) {
                var statonCodes = preeceedingRequiredComponentEntriesNotCaptured
                    .Select(t => t.ProductionStationCode)
                    .Aggregate((a, b) => a + ", " + b);

                errors.Add(new Error("", $"serial numbers for prior stations not captured: {statonCodes}"));
                return errors;
            }

            /* MULTI STATION COMPONENT
            *  Must have matching serial numbers
            */

            var prior_kit_components_with_same_compoent_code_different_serial_numbers = await context.KitComponents
                .Include(t => t.ComponentSerials)
                .OrderBy(t => t.ProductionStation.Sequence)
                // save kit         
                .Where(t => t.Kit.Id == targetKitCmponent.KitId)
                // same component id
                .Where(t => t.ComponentId == targetKitCmponent.ComponentId)
                // preceeding target kit component
                .Where(t => t.ProductionStation.Sequence < targetKitCmponent.ProductionStation.Sequence)
                // any active component serials that do not match input.Serial1, input.Serial2
                .Where(t =>
                    t.ComponentSerials
                        .Where(t => t.RemovedAt == null)
                        .Any(u => u.Serial1 != input.Serial1 || u.Serial2 != input.Serial2)
                )
                .Select(t => new {
                    Sequence = t.ProductionStation.Sequence,
                    StationCode = t.ProductionStation.Code,
                    Serial1 = t.ComponentSerials.Where(u => u.RemovedAt == null).First().Serial1,
                    Serial2 = t.ComponentSerials.Where(u => u.RemovedAt == null).First().Serial2,
                })
                .ToListAsync();

            if (prior_kit_components_with_same_compoent_code_different_serial_numbers.Any()) {
                var entry = prior_kit_components_with_same_compoent_code_different_serial_numbers
                    .OrderByDescending(t => t.Sequence)
                    .Last();
                var text = $"serial does not match previous station: {entry.StationCode}, {entry.Serial1}, {entry.Serial2}";
                errors.Add(new Error("", text.Trim(' ', ',')));
                return errors;
            }

            return errors;
        }

        private ComponentSerialInput SwapSerial(ComponentSerialInput input) {
            input = input with {
                Serial1 = input.Serial1 is null or "" ? "" : input.Serial1,
                Serial2 = input.Serial2 is null or "" ? "" : input.Serial2
            };

            if (input.Serial1.Trim().Length == 0) {
                return input with {
                    KitComponentId = input.KitComponentId,
                    Serial1 = input.Serial2,
                    Serial2 = ""
                };
            }
            return input;
        }

        public async Task<ComponentSerial> ApplyComponentSerialFormat(Guid componentSerialId) {
            var cs = await context.ComponentSerials
                .Include(t => t.KitComponent).ThenInclude(t => t.Component)
                .FirstOrDefaultAsync(t => t.Id == componentSerialId);

            if (cs.KitComponent.Component.Code != "EN") {
                throw new Exception("EN Component only");
            }

            var formatResult = serialFormatter.FormatSerial(cs.KitComponent.Component.Code, cs.Serial1);
            if (!formatResult.Success) {
                throw new Exception("Could not trasform: " + formatResult.Message);
            }
            cs.Original_Serial1 = cs.Serial1;
            cs.Serial1 = formatResult.Serial;

            await context.SaveChangesAsync();
            return cs;
        }

        public async Task<BasicKitInfo?> GetBasicKitInfo(string vin) {
            var result = await context.Kits
                .Where(t => t.VIN == vin)
                .Select(t => new BasicKitInfo {
                    KitNo = t.KitNo,
                    VIN = t.VIN,
                    LotNo = t.Lot.LotNo,
                    ModelCode = t.Lot.Model.Code,
                    ModelName = t.Lot.Model.Name
                }).FirstOrDefaultAsync();

            return result;
        }

        public async Task<KitComponentSerialInfo?> GetKitComponentSerialInfo(string kitNo, string componentCode) {
            var data = await context.KitComponents
                .Where(t => t.Kit.KitNo == kitNo && t.Component.Code == componentCode)
                .Select(t => new {
                    KitComponentId = t.Id,
                    RemvoedAt = t.RemovedAt,
                    ComponentCode = t.Component.Code,
                    ComponentName = t.Component.Name,
                    StationSequence = t.ProductionStation.Sequence,
                    StationCode = t.ProductionStation.Code,
                    StationName = t.ProductionStation.Name,
                    SerialCapture = t.ComponentSerials
                        .Where(k => k.RemovedAt == null)
                        .FirstOrDefault()
                }).ToListAsync();

            if (data.Count == 0) {
                return (KitComponentSerialInfo?)null;
            }

            var result = new KitComponentSerialInfo {            
                ComponentCode = data[0].ComponentCode,
                ComponentName = data[0].ComponentCode,
                RemovedAt = data[0].RemvoedAt,
                Stations = data.OrderBy(t => t.StationSequence).Select(t => new StatcionSerialInfo {
                    KitComponentId = t.KitComponentId,
                    StationSequence = t.StationSequence,
                    StationCode = t.StationCode,
                    StationName = t.StationName,
                    Serial1 = t.SerialCapture?.Serial1,
                    Serial2 = t.SerialCapture?.Serial2,
                    CreatedAt = t.SerialCapture?.CreatedAt,
                    VerifiedAt = t.SerialCapture?.VerifiedAt
                }).ToList()
            };

            return result;
        }
    }
}
