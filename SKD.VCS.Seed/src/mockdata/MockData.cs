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
        public ICollection<CmponentStation_McckData_DTO> ComponentStation_MockData;
        public ICollection<ProductionStation_Mock_DTO> ProductionStation_MockData;

        public MockData(string dirPath) {

            Component_MockData = JsonSerializer.Deserialize<List<Component_MockData_DTO>>(Components_JSON.Replace("'", "\""));
            ProductionStation_MockData = JsonSerializer.Deserialize<List<ProductionStation_Mock_DTO>>(ProductionStations_JSON.Replace("'", "\""));
            ComponentStation_MockData = JsonSerializer.Deserialize<List<CmponentStation_McckData_DTO>>(ComponentStationMapping_JSON.Replace("'", "\""));          
            VehicleModel_MockData = JsonSerializer.Deserialize<List<VehicleModel_MockData_DTO>>(VehicleModels_JSON.Replace("'", "\""));
            Vehicle_MockData = JsonSerializer.Deserialize<List<Vehicle_MockData_DTO>>(Vehicles_JSON.Replace("'", "\""));
        }

        private string Vehicles_JSON = @"
[
  {
    'vin': 'MNCUMNF50JW795262',
    'modelCode': 'ZRAE9GD0010',
    'lotNo': '001',
    'kitNo': '001'
  },
  {
    'vin': 'MNCUMNF50JW795267',
    'modelCode': 'ZRAE9GD9999',
    'lotNo': '002',
    'kitNo': '001'
  },
  {
    'vin': 'MNCUMNF80JW795260',
    'modelCode': 'ZRAE9PQ0010',
    'lotNo': '003',
    'kitNo': '001'
  },
  {
    'vin': 'MNCBXXMAWBHK48380',
    'modelCode': 'ZRAE9GD5010',
    'lotNo': '004',
    'kitNo': '001'
  },
  {
    'vin': 'MNCBXXMAWBHD65253',
    'modelCode': 'ARLQ93D0001',
    'lotNo': '005',
    'kitNo': '001'
  },
  {
    'vin': 'MNCBXXMAWBHD63946',
    'modelCode': 'ARLQ93D9999',
    'lotNo': '006',
    'kitNo': '001'
  },
  {
    'vin': 'MNCBXXMAWBHD65454',
    'modelCode': 'ARLQ99E0001',
    'lotNo': '007',
    'kitNo': '001'

  },
  {
    'vin': 'MNCBXXMAWBHD65873',
    'modelCode': 'ARLQ93D5555',
    'lotNo': '008',
    'kitNo': '001'
  },
  {
    'vin': 'MNCBXXMAWBHDK4871',
    'modelCode': 'ARLQ93A2222',
    'lotNo': '009',
    'kitNo': '001'
  }
]
";

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
    'code': 'ZRAE9GD0010',
    'name': 'U375 XLT 2.0L SG DSL PANTHER-B 2WHD AUTO HONEY GOLD'
  },
  {
    'code': 'ZRAE9GD9999',
    'name': 'U375 LTD 2.0L BI DSL PANTHER-C 4WHD AUTO COGNAC'
  },
  {
    'code': 'ZRAE9PQ0010',
    'name': 'U375 XLT 2.0L SG DSL PANTHER-B 2WHD AUTO EBONY'
  },
  {
    'code': 'ZRAE9GD5010',
    'name': 'U375 LTD 2.0L BI DSL PANTHER-C 4WHD AUTO EBONY'
  },
  {
    'code': 'ARLQ93D0001',
    'name': 'P375 XL 2.2L I4 DSL PUMA 4WHD MAN'
  },
  {
    'code': 'ARLQ93D9999',
    'name': 'P375 XLT 2.2L I4 DSL PUMA 4WHD AUTO'
  },
  {
    'code': 'ARLQ99E0001',
    'name': 'P375 XLT 3.2L I5 DSL PUMA 4WHD AUTOA9SAA'
  },
  {
    'code': 'ARLQ93D5555',
    'name': 'P375 XLT 3.2L I5 DSL PUMA 4WHD AUTOA9SAB'
  },
  {
    'code': 'ARLQ93A2222',
    'name': 'P375 WILDTRAK 3.2L I5 DSL PUMA 4WHD AUTO'
  }
]
";

 private string ComponentStationMapping_JSON = @"
 
 [
  {
    'componentCode': 'EN',
    'stationCode': 'FRM03'
  },
  {
    'componentCode': 'ENL',
    'stationCode': 'FRM03'
  },
  {
    'componentCode': 'FT',
    'stationCode': 'FRM03'
  },
  {
    'componentCode': 'TC',
    'stationCode': 'FRM03'
  },
  {
    'componentCode': 'TR',
    'stationCode': 'FRM03'
  },
  {
    'componentCode': 'EN',
    'stationCode': 'CHS01'
  },
  {
    'componentCode': 'VIN',
    'stationCode': 'CHS01'
  },
  {
    'componentCode': 'DA',
    'stationCode': 'CAB02'
  },
  {
    'componentCode': 'DKA',
    'stationCode': 'CAB02'
  },
  {
    'componentCode': 'DSC',
    'stationCode': 'CAB02'
  },
  {
    'componentCode': 'PA',
    'stationCode': 'CAB02'
  },
  {
    'componentCode': 'PS',
    'stationCode': 'CAB02'
  },
  {
    'componentCode': 'DS',
    'stationCode': 'CHS02'
  },
  {
    'componentCode': 'IK',
    'stationCode': 'CHS02'
  },
  {
    'componentCode': 'PSC',
    'stationCode': 'CHS02'
  },
  {
    'componentCode': 'EN',
    'stationCode': 'CHS03'
  }
]
 
 ";


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