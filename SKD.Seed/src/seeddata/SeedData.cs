using System;
using System.Collections.Generic;
using System.Text.Json;
using SKD.Model;
using System.IO;
using System.Dynamic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;

namespace SKD.Seed {
    internal class SeedData {

        public ICollection<Component_MockData_DTO> Component_MockData;
        public ICollection<CmponentStation_McckData_DTO> ComponentStation_MockData;
        public ICollection<ProductionStation_Mock_DTO> ProductionStation_MockData;

        public SeedData() {
            var options = new JsonSerializerOptions {
              PropertyNameCaseInsensitive = true              
            };
            Component_MockData = JsonSerializer.Deserialize<List<Component_MockData_DTO>>(Components_JSON.Replace("'", "\""),options);
            ProductionStation_MockData = JsonSerializer.Deserialize<List<ProductionStation_Mock_DTO>>(ProductionStations_JSON.Replace("'", "\""),options);
            ComponentStation_MockData = JsonSerializer.Deserialize<List<CmponentStation_McckData_DTO>>(ComponentStationMapping_JSON.Replace("'", "\""), options);          
        }

        private readonly string Components_JSON = @"
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
 private readonly string ComponentStationMapping_JSON = @"
 
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


        private readonly string  ProductionStations_JSON = @"
[
  {
    'code': 'FRM10',
    'name': 'Front Suspension and Power steering installation',
    'sortOrder': 1
  },
  {
    'code': 'FRM20',
    'name': 'Rear Suspension installation',
    'sortOrder': 2
  },
  {
    'code': 'FRM30',
    'name': 'Engine,Exhaust system and Fuel tank installation',
    'sortOrder': 3
  },
  {
    'code': 'CAB10',
    'name': 'Cabin Preparation and Decal part',
    'sortOrder': 4
  },
  {
    'code': 'CAB20',
    'name': 'Electric wire routing and Instrument Panel installation into cabin',
    'sortOrder': 5
  },
  {
    'code': 'CAB30',
    'name': 'All the Glass installation',
    'sortOrder': 6
  },
  {
    'code': 'CAB40',
    'name': 'Door Panel component part assembly',
    'sortOrder': 7
  },
  {
    'code': 'FIN10',
    'name': 'Cabin assemble to Chassis',
    'sortOrder': 8
  },
  {
    'code': 'FIN20',
    'name': 'Seat, Floor console and interior part installation.',
    'sortOrder': 9
  },
  {
    'code': 'FIN30',
    'name': 'Front and Rear Bumper installation',
    'sortOrder': 10
  },
  {
    'code': 'FIN40',
    'name': 'Fluid Filling process',
    'sortOrder': 11
  }
]
      
";

    }
}