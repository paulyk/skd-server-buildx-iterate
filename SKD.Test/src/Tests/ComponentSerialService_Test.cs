using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.Dcws;

namespace SKD.Test {


    public class ComponentScanService_Test : TestBase {

        private record SerialTransformTestData(
            string ComponentCode,
            string Serial1,
            string Serial2,
            string Expected_Serial1,
            string Expected_Serial2,
            string Expected_Original_Serial1,
            string Expected_Original_Serial2
        );
        public ComponentScanService_Test() {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();
        }

        [Fact]
        public async Task can_capture_component_serial() {
            // setup
            var kitComponent = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .FirstOrDefaultAsync();

            var input = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: Gen_ComponentSerialNo(),
                Serial2: Gen_ComponentSerialNo()
            );
            var before_count = await context.ComponentSerials.CountAsync();

            // test
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            var payload = await service.CaptureComponentSerial(input);

            // assert
            var after_count = await context.ComponentSerials.CountAsync();
            Assert.Equal(before_count + 1, after_count);

            // assert serial
            var comopnetnSrial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload.Entity.ComponentSerialId);
            Assert.Equal(input.Serial1, comopnetnSrial.Serial1);
            Assert.Equal(input.Serial2, comopnetnSrial.Serial2);
        }

        [Fact]
        public async Task capture_EN_component_serial_transforms_formate_and_saves_origianl() {

            // setup
            var kit = Gen_Kit_Amd_Model_From_Components(new List<(string, string)> {
                ("TR", "STATION_1"),
                ("EN", "STATION_1"),
                ("DS", "STATION_1"),
            });

            var testData = new List<SerialTransformTestData> {
                new SerialTransformTestData("TR",
                    Serial1: "A4321 03092018881360 FB3P 7000  DA A1    ",
                    Serial2: "",
                    Expected_Serial1:"A4321 03092018881360  FB3P 7000 DA  A1 ",
                    Expected_Serial2: "",
                    Expected_Original_Serial1: "A4321 03092018881360 FB3P 7000  DA A1    ",
                    Expected_Original_Serial2: ""
                ),
                new SerialTransformTestData("EN",
                    Serial1: "CSEPA20276110074JB3Q 6007 KB    36304435474544003552423745444400364145374A4643003636474148454200",
                    Serial2: "",
                    Expected_Serial1:"CSEPA20276110074JB3Q 6007 KB",
                    Expected_Serial2: "",
                    Expected_Original_Serial1: "CSEPA20276110074JB3Q 6007 KB    36304435474544003552423745444400364145374A4643003636474148454200",
                    Expected_Original_Serial2: ""
                ),
                new SerialTransformTestData("DS",
                    Serial1: "JB3B-4160005-DG1F63",
                    Serial2: "S0FAF202831970",
                    Expected_Serial1:"JB3B-4160005-DG1F63",
                    Expected_Serial2: "S0FAF202831970",
                    Expected_Original_Serial1: "",
                    Expected_Original_Serial2: ""
                ),
            };

            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            foreach (var entry in testData) {
                var kitComponent = await context.KitComponents
                    .Where(t => t.KitId == kit.Id && t.Component.Code == entry.ComponentCode)
                    .FirstOrDefaultAsync();
                var input = new ComponentSerialInput(
                    KitComponentId: kitComponent.Id,
                    Serial1: entry.Serial1,
                    Serial2: entry.Serial2
                );
                var payload = await service.CaptureComponentSerial(input);
                var componentSerial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload.Entity.ComponentSerialId);

                Assert.Equal(entry.Expected_Serial1, componentSerial.Serial1);
                Assert.Equal(entry.Expected_Serial2, componentSerial.Serial2);
                Assert.Equal(entry.Expected_Original_Serial1, componentSerial.Original_Serial1);
                Assert.Equal(entry.Expected_Original_Serial2, componentSerial.Original_Serial2);
            }
        }


        [Fact]
        public async Task capture_TR_component_transforms_formate_and_saves_origianl() {

            // setup
            var kit = Gen_Kit_Amd_Model_From_Components(new List<(string, string)> {
                ("TR", "STATION_1"),
            });

            var testData = new List<SerialTransformTestData> {
                new SerialTransformTestData("TR",
                    Serial1: "FFTB102020224524",
                    Serial2: "JB3R 7003 SA",
                    Expected_Serial1:"FFTB102020224524      JB3R 7003 SA     ",
                    Expected_Serial2: "",
                    Expected_Original_Serial1: "FFTB102020224524",
                    Expected_Original_Serial2: "JB3R 7003 SA"                    
                ),
            };

            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            foreach (var entry in testData) {
                var kitComponent = await context.KitComponents
                    .Where(t => t.KitId == kit.Id && t.Component.Code == entry.ComponentCode)
                    .FirstOrDefaultAsync();
                var input = new ComponentSerialInput(
                    KitComponentId: kitComponent.Id,
                    Serial1: entry.Serial1,
                    Serial2: entry.Serial2
                );
                var payload = await service.CaptureComponentSerial(input);
                var componentSerial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload.Entity.ComponentSerialId);

                Assert.Equal(entry.Expected_Serial1, componentSerial.Serial1);
                Assert.Equal(entry.Expected_Serial2, componentSerial.Serial2);

                Assert.Equal(entry.Expected_Original_Serial1, componentSerial.Original_Serial1);
                Assert.Equal(entry.Expected_Original_Serial2, componentSerial.Original_Serial2);
            }
        }

        [Fact]
        public async Task capture_component_serial_swaps_if_serial_1_blank() {
            // setup
            var kitComponent = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .FirstOrDefaultAsync();

            var input = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: "",
                Serial2: Gen_ComponentSerialNo()
            );
            // test
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            var payload = await service.CaptureComponentSerial(input);

            // assert
            var componentSerial = await context.ComponentSerials
                .FirstOrDefaultAsync(t => t.Id == payload.Entity.ComponentSerialId);

            Assert.Equal(input.Serial2, componentSerial.Serial1);
            Assert.Equal("", componentSerial.Serial2);
        }

        [Fact]
        public async Task error_capturing_component_serial_if_blank_serial() {
            // setup
            var kitComponent = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .FirstOrDefaultAsync();

            var input = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: ""
            );
            var before_count = await context.ComponentSerials.CountAsync();

            // test
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            var payload = await service.CaptureComponentSerial(input);

            // assert
            var after_count = await context.ComponentSerials.CountAsync();
            Assert.Equal(before_count, after_count);

            var expected_error_message = "no serial numbers provided";
            var actual_error_message = payload.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expected_error_message, actual_error_message.Substring(0, expected_error_message.Length));
        }

        [Fact]
        public async Task error_capturing_component_serial_if_already_captured_for_specified_component() {
            // setup
            var kitComponent = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .FirstOrDefaultAsync();

            var input_1 = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: Gen_ComponentSerialNo()
            );
            var input_2 = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: Gen_ComponentSerialNo()
            );
            var before_count = await context.ComponentSerials.CountAsync();

            // test
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            var payload_1 = await service.CaptureComponentSerial(input_1);
            var payload_2 = await service.CaptureComponentSerial(input_2);

            var expected_error_message = "component serial already captured for this component";
            var actual_error_message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expected_error_message, actual_error_message.Substring(0, expected_error_message.Length));
        }

        [Fact]
        public async Task can_replace_serial_with_new_one_for_specified_component() {
            // setup
            var kitComponent = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .FirstOrDefaultAsync();

            var input_1 = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: Gen_ComponentSerialNo(),
                Serial2: Gen_ComponentSerialNo()
            );
            var input_2 = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: Gen_ComponentSerialNo(),
                Serial2: Gen_ComponentSerialNo(),
                Replace: true
            );

            // test
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            var payload_1 = await service.CaptureComponentSerial(input_1);
            var firstComponentSerial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload_1.Entity.ComponentSerialId);


            var payload_2 = await service.CaptureComponentSerial(input_2);
            var secondComponentSerial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload_2.Entity.ComponentSerialId);

            Assert.Equal(input_1.Serial1, firstComponentSerial.Serial1);
            Assert.Equal(input_1.Serial2, firstComponentSerial.Serial2);

            Assert.Equal(input_2.Serial1, secondComponentSerial.Serial1);
            Assert.Equal(input_2.Serial2, secondComponentSerial.Serial2);

            var total_count = await context.ComponentSerials.CountAsync();
            var removed_count = await context.ComponentSerials.Where(t => t.RemovedAt != null).CountAsync();

            Assert.Equal(2, total_count);
            Assert.Equal(1, removed_count);

        }

        [Fact]
        public async Task error_if_serial_no_already_used_by_another_entry() {
            // setup
            var serialNo = Gen_ComponentSerialNo();

            // first vehcle component
            var kitComponent_1 = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .FirstOrDefaultAsync();

            var input_1 = new ComponentSerialInput(
                KitComponentId: kitComponent_1.Id,
                Serial1: serialNo
            );

            // different vheicle component
            var kitComponent_2 = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .Skip(1)
                .FirstOrDefaultAsync();

            var input_2 = new ComponentSerialInput(
                KitComponentId: kitComponent_2.Id,
                Serial1: serialNo // same serial as input_1
            );

            // test 
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            var payload_1 = await service.CaptureComponentSerial(input_1);
            var payload_2 = await service.CaptureComponentSerial(input_2);

            var expected_error_message = "serial number already in use by aonther entry";
            var actual_message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();

            actual_message = actual_message != null ? actual_message : "";
            Assert.Equal(expected_error_message, actual_message.Substring(0, expected_error_message.Length));
        }

        [Fact]
        public async Task error_If_component_serial_1_and_2_the_same() {
            // setup
            var kitComponent = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .FirstOrDefaultAsync();

            var serialNo = Gen_ComponentSerialNo();
            var input = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: serialNo,
                Serial2: serialNo
            );
            var before_count = await context.ComponentSerials.CountAsync();

            // test
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            var payload = await service.CaptureComponentSerial(input);

            // assert
            var expected_error_message = "serial 1 and 2 are the same";
            var actual_error_message = payload.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expected_error_message, actual_error_message);
        }

        [Fact]
        public async Task error_if_multi_station_component_captured_out_of_sequence() {
            // setup kit 
            var kit = Gen_Kit_Amd_Model_From_Components(new List<(string, string)> {
                ("DS", "STATION_1"),
                ("DA", "STATION_1"),
                ("DS", "STATION_2"),
                ("IK", "STATION_2"),
                ("DS", "STATION_3")
            });

            var kitComponent_1 = await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .FirstOrDefaultAsync();

            var input_1 = new ComponentSerialInput(
                KitComponentId: await context.KitComponents
                    .OrderBy(t => t.ProductionStation.Sequence)
                    .Where(t => t.Component.Code != "DS")
                    .Select(t => t.Id)
                    .FirstOrDefaultAsync(),
                Serial1: Gen_ComponentCode()
            );

            var input_2 = new ComponentSerialInput(
                KitComponentId: await context.KitComponents
                    .OrderByDescending(t => t.ProductionStation.Sequence)
                    .Where(t => t.Component.Code == "DS")
                    .Select(t => t.Id)
                    .FirstOrDefaultAsync(),
                Serial1: Gen_ComponentCode()
            );

            // test
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            await service.CaptureComponentSerial(input_1);

            var component_serial_count = await context.ComponentSerials.CountAsync();
            Assert.Equal(1, component_serial_count);

            var payload = await service.CaptureComponentSerial(input_2);
            var expected_error_message = "serial numbers for prior stations not captured";
            var actual_message = payload.Errors.Select(t => t.Message).FirstOrDefault();
            actual_message = actual_message != null ? actual_message : "";
            Assert.Equal(expected_error_message, actual_message.Substring(0, expected_error_message.Length));
        }

        [Fact]
        public async Task can_capture_full_kit_component_sequence() {

            // setup
            var kit = Gen_Kit_Amd_Model_From_Components(new List<(string, string)> {
                ("DS", "STATION_1"),
                ("DA", "STATION_1"),
                ("DS", "STATION_2"),
                ("IK", "STATION_2"),
                ("DS", "STATION_3")
            });

            var serial_numbers = new List<(string componentCode, string serialNo)> {
                ("DS", "EN-RANDOM-348"),
                ("DA", "DA-RANDOM-995"),
                ("IK", "IK-RANDOM-657"),
            };

            var kitComponents = await context.KitComponents
                .Include(t => t.Component)
                .Include(t => t.ProductionStation)
                .OrderBy(t => t.ProductionStation.Sequence)
                .Where(t => t.KitId == kit.Id).ToListAsync();

            // test
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            foreach (var vc in kitComponents) {
                var code = vc.Component.Code;
                var sortOrder = vc.ProductionStation.Sequence;
                var serialNo = serial_numbers
                        .Where(t => t.componentCode == code)
                        .Select(t => t.serialNo)
                        .First();
                var input = new ComponentSerialInput(
                    KitComponentId: vc.Id,
                    Serial1: serialNo
                );
                await service.CaptureComponentSerial(input);
            }

            // assert
            var component_serial_entry_count = await context.ComponentSerials.CountAsync();
            var expected_count = await context.KitComponents.CountAsync(t => t.KitId == kit.Id);
            Assert.Equal(expected_count, component_serial_entry_count);

        }

        [Fact]
        public async Task error_if_multi_station_component_serial_do_not_match() {

            // setup
            var kit = Gen_Kit_Amd_Model_From_Components(new List<(string, string)> {
                ("EN", "STATION_1"),
                ("DA", "STATION_1"),
                ("EN", "STATION_2"),
            });

            var test_data = new List<(string stationCode, string componentCode, string serialNo)> {
                ("STATION_1", "EN", "CSEPA20276110008JB3Q 6007 AA"),
                ("STATION_1", "DA", "DA-RANDOM-995"),
                ("STATION_2", "EN", "CSEPA20276110008JB3Q 6007 BB"),
            };

            var kitComponents = await context.KitComponents
                .Include(t => t.Component)
                .Include(t => t.ProductionStation)
                .OrderBy(t => t.ProductionStation.Sequence)
                .Where(t => t.KitId == kit.Id).ToListAsync();

            // test
            MutationPayload<ComponentSerialDTO> payload = null;
            var service = new ComponentSerialService(context, new DcwsSerialFormatter());
            foreach (var entry in test_data) {
                var kitComponent = kitComponents.First(t => t.Component.Code == entry.componentCode && t.ProductionStation.Code == entry.stationCode);
                var input = new ComponentSerialInput(
                    KitComponentId: kitComponent.Id,
                    Serial1: entry.serialNo
                );
                payload = await service.CaptureComponentSerial(input);
            }

            var expected_error_mssage = "serial does not match previous station";
            var actual_error_message = payload.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal(expected_error_mssage, actual_error_message.Substring(0, expected_error_mssage.Length));
        }
    }
}