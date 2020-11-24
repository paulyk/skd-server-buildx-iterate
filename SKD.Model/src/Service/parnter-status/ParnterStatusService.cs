using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SKD.Model {

    public class PartnerStatusService {

        private readonly SkdContext context;

        public PartnerStatusService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<QueryPayload<PartnerStatusDTO>> GetPartnerStatus(PartnerStatusInput input) {
            var payload = new QueryPayload<PartnerStatusDTO>(null);
            payload.Errors = await ValidateGetPartnerStatus(input);
            if (payload.Errors.Any()) {
                return payload;
            }
            //
            var wholesaleCutoff = input.RunDate.AddDays(7);

            // filter by plant code
            var query_plantCode = context.Vehicles.Where(t => t.Lot.Plant.Code == input.PlantCode).AsQueryable();

            // filter by custome recived
            var query_customReceived = context.Vehicles
                .Where(t => t.TimelineEvents.Any(ev => ev.RemovedAt == null && ev.EventType.Code == TimeLineEventType.CUSTOM_RECEIVED.ToString()))
                .AsQueryable();

            // filter by wholesale null or whilesalte < runDate + 7
            var query_wholeSale = query_customReceived
                .Where(t =>
                    // no wholesale time line event
                    !t.TimelineEvents.Any(ev => ev.RemovedAt == null && ev.EventType.Code == TimeLineEventType.WHOLE_SALE.ToString())

                    ||

                    // wholesale timeline event before cut-off date
                    t.TimelineEvents.Any(ev =>
                        ev.RemovedAt == null &&
                        ev.EventType.Code == TimeLineEventType.WHOLE_SALE.ToString() &&
                        ev.EventDate <= wholesaleCutoff
                    )
                ).AsQueryable();

            var entries = new List<PartnerStatusDTO>();
            var vehicles = await query_wholeSale
                .Include(t => t.Lot)
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .ToListAsync();

            var dto = new PartnerStatusDTO {
                RunDate = input.RunDate,
                PlantCode = input.PlantCode,
            };

            foreach (var vehicle in vehicles) {
                var vehicleStatusEntry = new PartnerStatusDTO.VehicleStatus {
                    TxType = GetVehicle_TxSatus(vehicle, input.RunDate),
                    CurrentStatusType = GetCurrentSatusType(vehicle),
                    LotNo = vehicle.Lot.LotNo,
                    KitNo = vehicle.KitNo,
                    VIN = GetVehicleVIN(vehicle),
                    DealerCode = GetDealerCode(vehicle),
                    EngineSerialNumber = await GetEngineSerialNumber(vehicle, input.EngineComponentCode),
                    CustomReceived = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.CUSTOM_RECEIVED),
                    PlanBuild = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.PLAN_BUILD),
                    BuildCompleted = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.BULD_COMPLETED),
                    GateRelease = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.GATE_RELEASED),
                    Wholesale = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.WHOLE_SALE),
                };
                dto.VehicleStatusEntries.Add(vehicleStatusEntry);
            }
            payload.Entity = dto;
            return payload;
        }

        public string GetVehicleVIN(Vehicle vehicle) {
            var buildCompletedEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventType.BULD_COMPLETED.ToString());

            if (buildCompletedEvent == null) {
                return "";
            }
            return vehicle.VIN;
        }

        private async Task<string> GetEngineSerialNumber(Vehicle vehicle, string engineComponentCode) {
            if (engineComponentCode == null) {
                throw new Exception("GetEngineSerialNumber: Engine component code required");
            }

            var buildCompletedEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventType.BULD_COMPLETED.ToString());

            if (buildCompletedEvent == null) {
                return "";
            }

            var componentScan = await context.ComponentScans
                .Where(t => t.VehicleComponent.Vehicle.KitNo == vehicle.KitNo)
                .Where(t => t.VehicleComponent.Component.Code == engineComponentCode)
                .Where(t => t.AcceptedAt != null && t.RemovedAt == null)
                .FirstOrDefaultAsync();

            return (componentScan?.Scan1 + " " + componentScan?.Scan2).Trim();
        }

        private DateTime? GetVehicleTimelineEventDate(Vehicle vehicle, TimeLineEventType eventType) {
            var timeLineEvnet = vehicle.TimelineEvents.FirstOrDefault(t => t.RemovedAt == null && t.EventType.Code == eventType.ToString());
            return timeLineEvnet != null
                ? timeLineEvnet.EventDate
                : (DateTime?)null;
        }

        private string GetDealerCode(Vehicle vehicle) {
            var timeLineEvnet = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == TimeLineEventType.WHOLE_SALE.ToString())
                .FirstOrDefault();

            return timeLineEvnet != null
                ? timeLineEvnet.EventNote
                : null;
        }

        private PartnerStatus_CurrentStatusType GetCurrentSatusType(Vehicle vehicle) {
            var timeLineEvnet = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.CreatedAt).FirstOrDefault();
            if (timeLineEvnet == null) {
                throw new Exception("Should have at least custom received event");
            }

            if (timeLineEvnet.EventType.Code == TimeLineEventType.CUSTOM_RECEIVED.ToString()) {
                return PartnerStatus_CurrentStatusType.FPCR;
            } else if (timeLineEvnet.EventType.Code == TimeLineEventType.PLAN_BUILD.ToString()) {
                return PartnerStatus_CurrentStatusType.FPBP;
            } else if (timeLineEvnet.EventType.Code == TimeLineEventType.BULD_COMPLETED.ToString()) {
                return PartnerStatus_CurrentStatusType.FPBC;
            } else if (timeLineEvnet.EventType.Code == TimeLineEventType.GATE_RELEASED.ToString()) {
                return PartnerStatus_CurrentStatusType.FPGR;
            }
            return PartnerStatus_CurrentStatusType.FPWS;
        }

        private PartnerStatus_TxType GetVehicle_TxSatus(Vehicle vehicle, DateTime runDate) {
            var latest_event = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault(t => t.RemovedAt == null);

            if (latest_event.EventType.Code == TimeLineEventType.CUSTOM_RECEIVED.ToString() && latest_event.CreatedAt.Date == runDate) {
                return PartnerStatus_TxType.Added;
            } else if (latest_event.EventType.Code != TimeLineEventType.WHOLE_SALE.ToString() && latest_event.CreatedAt.Date == runDate) {
                return PartnerStatus_TxType.Changed;
            } else if (latest_event.EventType.Code == TimeLineEventType.WHOLE_SALE.ToString()) {
                return PartnerStatus_TxType.Final;
            }
            return PartnerStatus_TxType.NoChange;
        }

        public async Task<List<Error>> ValidateGetPartnerStatus(PartnerStatusInput input) {
            var errors = new List<Error>();

            var plantExists = await context.Plants.AnyAsync(t => t.Code == input.PlantCode);
            if (!plantExists) {
                errors.Add(new Error("plantCode", "plant code not found"));
            }
        
            var component = await context.Components.FirstOrDefaultAsync(t => t.Code == input.EngineComponentCode);
            if (component == null) {
                errors.Add(new Error("EngineComponentCode", $"engine component not found for {input.EngineComponentCode}"));
            }

            return errors;
        }
    }
}