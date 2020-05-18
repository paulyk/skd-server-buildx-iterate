using System;
using System.Collections.Generic;
using System.Text.Json;
using VT.Model;
using System.IO;
using System.Dynamic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;

namespace VT.Seed {
    internal class SeedData {

        public ICollection<Vehicle_Seed_DTO> Vehicle_SeedData;
        public ICollection<Component_Seed_DTO> Component_SeedData;
        public ICollection<VehicleModel_Seed_DTO> VehicleModel_SeedData;
        public ICollection<VehicleModelComponent_Seed_DTO> VehicleModelComponent_SeedData;

        public SeedData(string dirPath) {

            Vehicle_SeedData = JsonSerializer.Deserialize<List<Vehicle_Seed_DTO>>(Vehicles_JSON.Replace("'", "\""));
            Component_SeedData = JsonSerializer.Deserialize<List<Component_Seed_DTO>>(Components_JSON.Replace("'", "\""));
            VehicleModel_SeedData = JsonSerializer.Deserialize<List<VehicleModel_Seed_DTO>>(VehicleModels_JSON.Replace("'", "\""));
            VehicleModelComponent_SeedData = JsonSerializer.Deserialize<List<VehicleModelComponent_Seed_DTO>>(VehicleModelComponents_JSON.Replace("'", "\""));

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
    {
      'code': 'EB3B-41043B13-AF3ZHE',
      'name': 'MOD ASY DRV AIR BAG RESTNT',
      'type': 'DA'
    },
    {
      'code': 'JB3B-4160005-AFD1GRY',
      'name': 'ST ASY FRT TR LH',
      'type': 'DS'
    },
    {
      'code': 'EB3B-41042D95-AH',
      'name': 'BAG ASY AIR RF LH',
      'type': 'DSC'
    },
    {
      'code': 'FB3Q-6007-DA3D',
      'name': 'ENG ASY(FFRD-T6 200PS AUTO EU2-4)',
      'type': 'EN'
    },
    {
      'code': 'FB3Q-6007-DA3DL',
      'name': 'ENG ASY(FFRD-T6 200PS AUTO EU2-4) (DUP)',
      'type': 'ENL'
    },
    {
      'code': 'JB3G-9K007-EA',
      'name': 'TNK & SDR ASY FUL',
      'type': 'FT'
    },
    {
      'code': 'EB3C-3F880-AA',
      'name': 'LK STNG COL',
      'type': 'IK'
    },
    {
      'code': 'JB3B-4104304-BC1F63',
      'name': 'PNL ASY I/PNL',
      'type': 'PA'
    },
    {
      'code': 'JB3B-4160004-AGD1GRY',
      'name': 'ST ASY FRT TR',
      'type': 'PS'
    },
    {
      'code': 'EB3B-41042D94-AK',
      'name': 'BAG ASY AIR RF',
      'type': 'PSC'
    },
    {
      'code': 'JB3P-7A195-CD',
      'name': 'CSE ASY-TRNSF',
      'type': 'TC'
    },
    {
      'code': 'FB3P-7000-DA',
      'name': 'TRANS & CONV ASY (6R80)',
      'type': 'TR'
    },
    {
      'code': 'AB39-21045J76-BJ3ZHE',
      'name': 'BLSTR I/PNL KNEE',
      'type': 'DKA'
    },
    {
      'code': 'JB3B-4160005-DG1F63',
      'name': 'ST ASY FRT TR LH (DUP)',
      'type': ''
    },
    {
      'code': 'JB3B-4160004-EG1F63',
      'name': 'Passenger Side Airbag ST ASY FRT TR',
      'type': 'PS'
    },
    {
      'code': 'EB3B-41143B13-AF3ZHE',
      'name': 'Driver Airbag',
      'type': ''
    },
    {
      'code': 'AB39-21145J76-BJ3ZHE',
      'name': 'Driver Knee Airbag',
      'type': ''
    },
    {
      'code': 'JB3B-4161115-JG1F63',
      'name': 'Driver Side Airbag',
      'type': ''
    },
    {
      'code': 'EB3B-41142D95-AH',
      'name': 'Driver Side Air Curtain',
      'type': ''
    },
    {
      'code': 'FB3Q-6667-DA3D',
      'name': 'Engine',
      'type': 'EN'
    },
    {
      'code': 'JB3G-9K117-EA',
      'name': 'Fuel Tank',
      'type': 'FT'
    },
    {
      'code': 'EB3C-3F881-AA',
      'name': 'Ignition Key Code',
      'type': ''
    },
    {
      'code': 'JB3B-4104314-BC1F63',
      'name': 'Passenger Airbag',
      'type': ''
    },
    {
      'code': 'JB3B-4160004-CG1F63',
      'name': 'Passenger Side Airbag',
      'type': ''
    },
    {
      'code': 'EB3B-41142D94-AK',
      'name': 'Passenger Side Air Curtain',
      'type': ''
    },
    {
      'code': 'JB3P-7A199-CD',
      'name': 'Transfer Case',
      'type': ''
    },
    {
      'code': 'FB3P-7123-DA',
      'name': 'Transmission',
      'type': ''
    }
  ]";

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
      'componentCode': 'EB3B-41043B13-AF3ZHE',
      'sequence': 1
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'JB3B-4160005-AFD1GRY',
      'sequence': 2
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'EB3B-41042D95-AH',
      'sequence': 3
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'FB3Q-6007-DA3D',
      'sequence': 4
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'FB3Q-6007-DA3DL',
      'sequence': 4
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'JB3G-9K007-EA',
      'sequence': 5
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'EB3C-3F880-AA',
      'sequence': 7
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'JB3B-4104304-BC1F63',
      'sequence': 8
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'JB3B-4160004-AGD1GRY',
      'sequence': 8
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'EB3B-41042D94-AK',
      'sequence': 9
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'JB3P-7A195-CD',
      'sequence': 9
    },
    {
      'modelCode': 'IJBW9E40001',
      'componentCode': 'FB3P-7000-DA',
      'sequence': 10
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'EB3B-41043B13-AF3ZHE',
      'sequence': 1
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'AB39-21045J76-BJ3ZHE',
      'sequence': 2
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'JB3B-4160005-DG1F63',
      'sequence': 3
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'EB3B-41042D95-AH',
      'sequence': 4
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'FB3Q-6007-DA3D',
      'sequence': 4
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'JB3G-9K007-EA',
      'sequence': 5
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'EB3C-3F880-AA',
      'sequence': 7
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'JB3B-4104304-BC1F63',
      'sequence': 8
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'JB3B-4160004-EG1F63',
      'sequence': 8
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'EB3B-41042D94-AK',
      'sequence': 9
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'JB3P-7A195-CD',
      'sequence': 9
    },
    {
      'modelCode': 'IJBT9E40002',
      'componentCode': 'FB3P-7000-DA',
      'sequence': 10
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'EB3B-41043B13-AF3ZHE',
      'sequence': 1
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'JB3B-4160005-AFD1GRY',
      'sequence': 2
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'EB3B-41042D95-AH',
      'sequence': 3
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'FB3Q-6007-DA3D',
      'sequence': 4
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'FB3Q-6007-DA3D',
      'sequence': 4
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'JB3G-9K007-EA',
      'sequence': 5
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'EB3C-3F880-AA',
      'sequence': 7
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'JB3B-4104304-BC1F63',
      'sequence': 8
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'JB3B-4160004-AGD1GRY',
      'sequence': 8
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'EB3B-41042D94-AK',
      'sequence': 9
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'JB3P-7A195-CD',
      'sequence': 9
    },
    {
      'modelCode': 'IJBT9240002',
      'componentCode': 'FB3P-7000-DA',
      'sequence': 10
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'EB3B-41143B13-AF3ZHE',
      'sequence': 1
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'AB39-21145J76-BJ3ZHE',
      'sequence': 1
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'JB3B-4161115-JG1F63',
      'sequence': 2
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'EB3B-41142D95-AH',
      'sequence': 3
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'FB3Q-6667-DA3D',
      'sequence': 4
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'JB3G-9K117-EA',
      'sequence': 5
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'EB3C-3F881-AA',
      'sequence': 5
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'JB3B-4104314-BC1F63',
      'sequence': 7
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'JB3B-4160004-CG1F63',
      'sequence': 8
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'EB3B-41142D94-AK',
      'sequence': 8
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'JB3P-7A199-CD',
      'sequence': 9
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'FB3P-7123-DA',
      'sequence': 9
    },
    {
      'modelCode': 'ZJAV9DC0001',
      'componentCode': 'FB3P-7000-DA',
      'sequence': 10
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'EB3B-41043B13-AF3ZHE',
      'sequence': 1
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'AB39-21045J76-BJ3ZHE',
      'sequence': 1
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'JB3B-4160005-AFD1GRY',
      'sequence': 2
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'EB3B-41042D95-AH',
      'sequence': 3
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'FB3Q-6007-DA3D',
      'sequence': 4
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'FB3Q-6007-DA3D',
      'sequence': 5
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'JB3G-9K007-EA',
      'sequence': 5
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'EB3C-3F880-AA',
      'sequence': 7
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'JB3B-4104304-BC1F63',
      'sequence': 8
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'JB3B-4160004-AGD1GRY',
      'sequence': 8
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'EB3B-41042D94-AK',
      'sequence': 9
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'JB3P-7A195-CD',
      'sequence': 9
    },
    {
      'modelCode': 'ZJAJ9CD0003',
      'componentCode': 'FB3P-7000-DA',
      'sequence': 10
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'EB3B-41043B13-AF3ZHE',
      'sequence': 1
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'AB39-21045J76-BJ3ZHE',
      'sequence': 1
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'JB3B-4160005-AFD1GRY',
      'sequence': 2
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'EB3B-41042D95-AH',
      'sequence': 3
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'FB3Q-6007-DA3D',
      'sequence': 4
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'FB3Q-6007-DA3D',
      'sequence': 5
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'JB3G-9K007-EA',
      'sequence': 5
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'EB3C-3F880-AA',
      'sequence': 7
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'JB3B-4104304-BC1F63',
      'sequence': 8
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'JB3B-4160004-AGD1GRY',
      'sequence': 8
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'EB3B-41042D94-AK',
      'sequence': 9
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'JB3P-7A195-CD',
      'sequence': 9
    },
    {
      'modelCode': 'ZJAE9CD0002',
      'componentCode': 'FB3P-7000-DA',
      'sequence': 10
    }
  ]";
    }

}