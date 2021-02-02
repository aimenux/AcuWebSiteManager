using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.EQUIPMENT_SETUP)]
    [PXPrimaryGraph(typeof(EquipmentSetupMaint))]
	public class FSEquipmentSetup : FSSetup
    {
    }
}
