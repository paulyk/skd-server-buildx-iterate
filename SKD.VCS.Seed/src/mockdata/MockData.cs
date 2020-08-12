using System;
using System.Collections.Generic;
using System.Text.Json;
using SKD.VCS.Model;
using System.IO;
using System.Dynamic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;

namespace SKD.VCS.Seed {
    internal class MockData {

        public ICollection<Vehicle_MockData_DTO> Vehicle_MockData;
        public ICollection<Component_MockData_DTO> Component_MockData;
        public ICollection<VehicleModel_MockData_DTO> VehicleModel_MockData;
        public ICollection<VehicleModelComponent_MockData_DTO> VehicleModelComponent_MockData;
        public ICollection<ProductionStation_Mock_DTO> ProductionStation_MockData;

        public MockData(string dirPath) {

            Vehicle_MockData = JsonSerializer.Deserialize<List<Vehicle_MockData_DTO>>(Vehicles_JSON.Replace("'", "\""));
            Component_MockData = JsonSerializer.Deserialize<List<Component_MockData_DTO>>(Components_JSON.Replace("'", "\""));
            VehicleModel_MockData = JsonSerializer.Deserialize<List<VehicleModel_MockData_DTO>>(VehicleModels_JSON.Replace("'", "\""));
            VehicleModelComponent_MockData = JsonSerializer.Deserialize<List<VehicleModelComponent_MockData_DTO>>(VehicleModelComponents_JSON.Replace("'", "\""));
            ProductionStation_MockData = JsonSerializer.Deserialize<List<ProductionStation_Mock_DTO>>(ProductionStations_JSON.Replace("'", "\""));
        }

        private string Vehicles_JSON = @"
[
    {
      'vin': 'MNCUMNF50JW795262',
      'kitNo': '',
      'lotNo': '',
      'modelId': 'IJBW9E40001',
      'modelName': 'Ranger DC Wildtrak - MY18'
    },
    {
      'vin': 'MNCUMNF50JW795267',
      'kitNo': '',
      'lotNo': '',
      'modelId': 'IJBT9E40002',
      'modelName': 'Ranger DC 3.2 XLT 6AT - MY18'
    },
    {
      'vin': 'MNCUMNF80JW795260',
      'kitNo': '',
      'lotNo': '',
      'modelId': 'IJBT9240002',
      'modelName': 'Ranger DC 2.2 XLT 6AT - MY18'
    },
    {
      'vin': 'MNCBXXMAWBHK48380',
      'kitNo': '',
      'lotNo': '',
      'modelId': 'ZJAV9DC0001',
      'modelName': 'Everest Ambiente 2.2 6AT 2WD - MY18'
    },
    {
      'vin': 'MNCBXXMAWBHD65253',
      'kitNo': '',
      'lotNo': '',
      'modelId': 'ZJAJ9CD0003',
      'modelName': 'Everest Trend 3.2 6AT AWD - MY18'
    },
    {
      'vin': 'MNCBXXMAWBHD63946',
      'kitNo': '',
      'lotNo': '',
      'modelId': 'ZJAE9CD0002',
      'modelName': 'Everest Titanium 3.2 6AT - MY18'
    }
  ]";

        private string Components_JSON = @"
  [
       {'code':'DA','name':'Driver Airbag'}
      ,{'code':'DKA','name':'Driver Knee Airbag'}
      ,{'code':'DS','name':'Driver Side Airbag'}
      ,{'code':'DSC','name':'Driver Side Air Curtain'}
      ,{'code':'EN','name':'Engine'}
      ,{'code':'ENL','name':'Engine Legal Code'}
      ,{'code':'FNL','name':'Frame Number Legal'}
      ,{'code':'FT','name':'Fuel Tank'}
      ,{'code':'IK','name':'Ignition Key Code'}
      ,{'code':'PA','name':'Passenger Airbag'}
      ,{'code':'PS','name':'Passenger Side Airbag'}
      ,{'code':'PSC','name':'Passenger Side Air Curtain'}
      ,{'code':'TC','name':'Transfer Case'}
      ,{'code':'TR','name':'Transmission'}
      ,{'code':'VIN','name':'Marry Body & Frame Check'}
  ]
";

        private string VehicleModels_JSON = @"
[
  {
    'code': 'IJBW9E40001',
    'name': 'Ranger DC Wildtrak - MY18'
  },
  {
    'code': 'IJBT9E40002',
    'name': 'Ranger DC 3.2 XLT 6AT - MY18'
  },
  {
    'code': 'IJBT9240002',
    'name': 'Ranger DC 2.2 XLT 6AT - MY18'
  },
  {
    'code': 'ZJAV9DC0001',
    'name': 'Everest Ambiente 2.2 6AT 2WD - MY18'
  },
  {
    'code': 'ZJAJ9CD0003',
    'name': 'Everest Trend 3.2 6AT AWD - MY18'
  },
  {
    'code': 'ZJAE9CD0002',
    'name': 'Everest Titanium 3.2 6AT - MY18'
  }
]";

        private string VehicleModelComponents_JSON = @"
[
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'DA',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'DS',
    'sequence': 2,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'DSC',
    'sequence': 3,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'EN',
    'sequence': 4,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'ENL',
    'sequence': 5,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'FNL',
    'sequence': 6,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'FT',
    'sequence': 7,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'IK',
    'sequence': 7,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'PA',
    'sequence': 8,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'PS',
    'sequence': 8,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'PSC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'TC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'TR',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBW9E40001',
    'componentCode': 'EN',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'DA',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'DS',
    'sequence': 2,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'DSC',
    'sequence': 3,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'EN',
    'sequence': 4,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'ENL',
    'sequence': 5,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'FNL',
    'sequence': 6,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'FT',
    'sequence': 7,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'IK',
    'sequence': 7,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'PA',
    'sequence': 8,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'PS',
    'sequence': 8,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'PSC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'TC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'TR',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9E40002',
    'componentCode': 'EN',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'DA',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'DS',
    'sequence': 2,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'DSC',
    'sequence': 3,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'EN',
    'sequence': 4,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'ENL',
    'sequence': 5,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'FNL',
    'sequence': 6,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'FT',
    'sequence': 7,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'IK',
    'sequence': 7,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'PA',
    'sequence': 8,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'PS',
    'sequence': 8,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'PSC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'TC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'TR',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'IJBT9240002',
    'componentCode': 'EN',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'DA',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'DKA',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'DS',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'DSC',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'EN',
    'sequence': 2,
    'prerequisite': '1'
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'ENL',
    'sequence': 3,
    'prerequisite': '2'
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'FNL',
    'sequence': 4,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'FT',
    'sequence': 5,
    'prerequisite': '3,5'
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'IK',
    'sequence': 6,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'PA',
    'sequence': 7,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'PS',
    'sequence': 8,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'PSC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'TC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'TR',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAV9DC0001',
    'componentCode': 'EN',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'DA',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'DKA',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'DS',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'DSC',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'EN',
    'sequence': 2,
    'prerequisite': '1'
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'ENL',
    'sequence': 3,
    'prerequisite': '2'
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'FNL',
    'sequence': 4,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'FT',
    'sequence': 5,
    'prerequisite': '3,5'
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'IK',
    'sequence': 6,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'PA',
    'sequence': 7,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'PS',
    'sequence': 8,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'PSC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'TC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'TR',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAJ9CD0003',
    'componentCode': 'EN',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'DA',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'DKA',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'DS',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'DSC',
    'sequence': 1,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'EN',
    'sequence': 2,
    'prerequisite': '1'
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'ENL',
    'sequence': 3,
    'prerequisite': '2'
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'FNL',
    'sequence': 4,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'FT',
    'sequence': 5,
    'prerequisite': '3,5'
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'IK',
    'sequence': 6,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'PA',
    'sequence': 7,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'PS',
    'sequence': 8,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'PSC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'TC',
    'sequence': 9,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'TR',
    'sequence': 10,
    'prerequisite': ''
  },
  {
    'modelCode': 'ZJAE9CD0002',
    'componentCode': 'EN',
    'sequence': 10,
    'prerequisite': ''
  }
]";


        private string ProductionStations_JSON = @"
[
  {
    'code': 'FRM03',
    'name': 'FRM03',
    'sortOrder': 1
  },
  {
    'code': 'CHS01',
    'name': 'CHS01',
    'sortOrder': 3
  },
  {
    'code': 'CAB02',
    'name': 'CAB02',
    'sortOrder': 2
  },
  {
    'code': 'CHS02',
    'name': 'CHS02',
    'sortOrder': 4
  },
  {
    'code': 'CHS03',
    'name': 'CHS03',
    'sortOrder': 5
  }
]
      
";

    }
}