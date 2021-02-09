#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SKD.Model {

    public class KitService {

        private readonly SkdContext context;
        private readonly DateTime currentDate;
        public readonly int planBuildLeadTimeDays = 6;

        public KitService(SkdContext ctx, DateTime currentDate, int planBuildLeadTimeDays) {
            this.context = ctx;
            this.currentDate = currentDate;
            this.planBuildLeadTimeDays = planBuildLeadTimeDays;
        }

        public async Task<MutationPayload<Lot>> AssingKitVin(AssignKitVinInput input) {
            var payload = new MutationPayload<Lot>(null);
            payload.Errors = await ValidateAssignKitVinInput(input);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            // assign vin
            var vehicleLot = await context.Lots
                .Include(t => t.Kits)
                .FirstOrDefaultAsync(t => t.LotNo == input.LotNo);
            payload.Entity = vehicleLot;

            vehicleLot.Kits.ToList().ForEach(vehicle => {
                var vin = input.Kits.First(t => t.KitNo == vehicle.KitNo).VIN;
                vehicle.VIN = vin;
            });
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<MutationPayload<KitTimelineEvent>> CreateKitTimelineEvent(KitTimelineEventInput dto) {
            var payload = new MutationPayload<KitTimelineEvent>(null);
            payload.Errors = await ValidateCreateVehicleTimelineEvent(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var vehicle = await context.Kits
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.KitNo == dto.KitNo);

            // mark other timeline events of the same type as removed for this vehicle
            vehicle.TimelineEvents
                .Where(t => t.EventType.Code == dto.EventType.ToString())
                .ToList().ForEach(timelieEvent => {
                    if (timelieEvent.RemovedAt == null) {
                        timelieEvent.RemovedAt = DateTime.UtcNow;
                    }
                });

            // create timeline event and add to vehicle
            var newTimelineEvent = new KitTimelineEvent {
                EventType = await context.VehicleTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == dto.EventType.ToString()),
                EventDate = dto.EventDate,
                EventNote = dto.EventNote
            };

            vehicle.TimelineEvents.Add(newTimelineEvent);

            // save
            payload.Entity = newTimelineEvent;
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<MutationPayload<Lot>> CreateLotTimelineEvent(LotTimelineEventInput dto) {
            var payload = new MutationPayload<Lot>(null);
            payload.Errors = await ValidateCreateVehicleLotTimelineEvent(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var vehicleLot = await context.Lots
                .Include(t => t.Kits)
                    .ThenInclude(t => t.TimelineEvents)
                    .ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);

            foreach (var vehicle in vehicleLot.Kits) {

                // mark other timeline events of the same type as removed for this vehicle
                vehicle.TimelineEvents
                    .Where(t => t.EventType.Code == dto.EventType.ToString())
                    .ToList().ForEach(timelieEvent => {
                        if (timelieEvent.RemovedAt == null) {
                            timelieEvent.RemovedAt = DateTime.UtcNow;
                        }
                    });

                // create timeline event and add to vehicle
                var newTimelineEvent = new KitTimelineEvent {
                    EventType = await context.VehicleTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == dto.EventType.ToString()),
                    EventDate = dto.EventDate,
                    EventNote = dto.EventNote
                };

                vehicle.TimelineEvents.Add(newTimelineEvent);

            }

            // // save
            payload.Entity = vehicleLot;
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateAssignKitVinInput(AssignKitVinInput dto) {
            var errors = new List<Error>();

            var vehicleLot = await context.Lots.AsNoTracking()
                .Include(t => t.Kits)
                .FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);

            // lotNo
            if (vehicleLot == null) {
                errors.Add(new Error("lotNo", $"vehicle lot not found: {dto.LotNo}"));
                return errors;
            }

            if (vehicleLot.RemovedAt != null) {
                errors.Add(new Error("lotNo", "vehicle lot marked removed"));
                return errors;
            }

            // invalid VIN(s)
            var validator = new Validator();
            var invalidVins = dto.Kits
                .Select(t => t.VIN)
                .Where(vin => !validator.Valid_KitNo(vin))
                .ToList();

            if (invalidVins.Any()) {
                errors.Add(new Error("", $"invalid VINs found in lot {String.Join(", ", invalidVins)}"));
                return errors;
            }

            // duplicatev vins
            var duplicate_vins = new List<string>();
            foreach (var kit in dto.Kits) {
                var existing = await context.Kits.AsNoTracking().AnyAsync(t => t.VIN == kit.VIN);
                if (existing) {
                    duplicate_vins.Add(kit.VIN);
                }
            }

            if (duplicate_vins.Any()) {
                errors.Add(new Error("", $"duplicate VIN(s) found {String.Join(", ", duplicate_vins)}"));
                return errors;
            }

            // Wehicles with matching kit numbers not found
            var kit_numbers_not_found = new List<string>();
            foreach (var kit in dto.Kits) {
                var exists = await context.Kits.AsNoTracking().AnyAsync(t => t.KitNo == kit.KitNo);
                if (!exists) {
                    kit_numbers_not_found.Add(kit.KitNo);
                }
            }
            if (kit_numbers_not_found.Any()) {
                errors.Add(new Error("", $"kit numbers not found {String.Join(", ", kit_numbers_not_found)}"));
                return errors;
            }

            // kit count
            if (vehicleLot.Kits.Count() != dto.Kits.Count) {
                errors.Add(new Error("lotNo", $"number of kits {dto.Kits.Count} doesn't match number of kits in lot {vehicleLot.Kits.Count}"));
                return errors;
            }

            // duplicate kitNos in payload
            var duplicateKitNos = dto.Kits
                .GroupBy(t => t.KitNo)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.ToList())
                .Select(t => t.KitNo)
                .Distinct();

            if (duplicateKitNos.Count() > 0) {
                errors.Add(new Error("lotNo", $"duplicate kitNo(s) in payload: {String.Join(", ", duplicateKitNos)}"));
                return errors;
            }

            return errors;
        }

        public async Task<List<Error>> ValidateCreateVehicleTimelineEvent(KitTimelineEventInput input) {
            var errors = new List<Error>();

            // kitNo
            var kit = await context.Kits.AsNoTracking()
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.KitNo == input.KitNo);
            if (kit == null) {
                errors.Add(new Error("KitNo", $"kit not found for kitNo: {input.KitNo}"));
                return errors;
            }

            // duplicate kit timeline event
            var duplicate = kit.TimelineEvents
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == input.EventType.ToString())
                .Where(t => t.EventDate == input.EventDate)
                .Where(t => t.EventNote == input.EventNote)
                .FirstOrDefault();

            if (duplicate != null) {
                var dateStr = input.EventDate.ToShortDateString();
                errors.Add(new Error("", $"duplicate kit timeline event: {input.EventType.ToString()} {dateStr} "));
                return errors;
            }

            // missing prerequisite timeline events
            var currentTimelineEventType = await context.VehicleTimelineEventTypes
                .FirstOrDefaultAsync(t => t.Code == input.EventType.ToString());

            var missingTimlineSequences = Enumerable.Range(1, currentTimelineEventType.Sequecne - 1)
                .Where(seq => !kit.TimelineEvents
                .Any(t => t.EventType.Sequecne == seq)).ToList();


            if (missingTimlineSequences.Count > 0) {
                var mssingTimelineEventCodes = await context.VehicleTimelineEventTypes
                    .Where(t => missingTimlineSequences.Any(missingSeq => t.Sequecne == missingSeq))
                    .Select(t => t.Code).ToListAsync();

                var text = mssingTimelineEventCodes.Aggregate((a, b) => a + ", " + b);
                errors.Add(new Error("", $"prior timeline event(s) mssing {text}"));
                return errors;
            }

            // CUSTOM_RECEIVED 
            if (input.EventType == TimeLineEventType.CUSTOM_RECEIVED) {
                if (currentDate <= input.EventDate) {
                    errors.Add(new Error("", $"custom received date must be before current date"));
                    return errors;
                }
            }

            // PLAN_BUILD 
            if (input.EventType == TimeLineEventType.PLAN_BUILD) {
                var custom_receive_date = kit.TimelineEvents
                    .Where(t => t.RemovedAt == null)
                    .Where(t => t.EventType.Code == TimeLineEventType.CUSTOM_RECEIVED.ToString())
                    .Select(t => t.EventDate).First();

                var custom_receive_plus_lead_time_date = custom_receive_date.AddDays(planBuildLeadTimeDays);

                var plan_build_date = input.EventDate;
                if (plan_build_date <= custom_receive_plus_lead_time_date) {
                    errors.Add(new Error("", $"plan build must greater custom receive by {planBuildLeadTimeDays} days"));
                    return errors;
                }
            }

            return errors;
        }

        public async Task<List<Error>> ValidateCreateVehicleLotTimelineEvent(LotTimelineEventInput input) {
            var errors = new List<Error>();

            var lot = await context.Lots.AsNoTracking()
                .Include(t => t.Kits).ThenInclude(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.LotNo == input.LotNo);

            // vehicle lot 
            if (lot == null) {
                errors.Add(new Error("VIN", $"lot not found for lotNo: {input.LotNo}"));
                return errors;
            }

            // duplicate 
            var duplicateTimelineEventsFound = lot.Kits.SelectMany(t => t.TimelineEvents)
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == input.EventType.ToString())
                .Where(t => t.EventDate == input.EventDate)
                .ToList();

            if (duplicateTimelineEventsFound.Count > 0) {
                var dateStr = input.EventDate.ToShortDateString();
                errors.Add(new Error("", $"duplicate kit timeline event: {input.LotNo}, Type: {input.EventType.ToString()} Date: {dateStr} "));
                return errors;
            }

            // CUSTOM_RECEIVED 
            if (input.EventType == TimeLineEventType.CUSTOM_RECEIVED) {
                if (input.EventDate >= currentDate) {
                    errors.Add(new Error("VIN", $"custom received date must be before current date"));
                    return errors;
                }
            }

            return errors;
        }
    }
}