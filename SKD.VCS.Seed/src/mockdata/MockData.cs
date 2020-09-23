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

        public ICollection<Component_MockData_DTO> Component_MockData;
        public ICollection<CmponentStation_McckData_DTO> ComponentStation_MockData;
        public ICollection<ProductionStation_Mock_DTO> ProductionStation_MockData;

        public MockData(string dirPath) {
            Component_MockData = JsonSerializer.Deserialize<List<Component_MockData_DTO>>(Components_JSON.Replace("'", "\""));
            ProductionStation_MockData = JsonSerializer.Deserialize<List<ProductionStation_Mock_DTO>>(ProductionStations_JSON.Replace("'", "\""));
            ComponentStation_MockData = JsonSerializer.Deserialize<List<CmponentStation_McckData_DTO>>(ComponentStationMapping_JSON.Replace("'", "\""));          
        }

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