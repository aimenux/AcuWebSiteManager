using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSServiceTemplateDetPart : FSServiceTemplateDet
	{
		#region LineType
		public new abstract class lineType : ListField_LineType_Part_ALL
		{
		}

		[PXDBString(5, IsFixed = true)]
        [lineType.ListAtrribute]
		[PXDefault(ID.LineType_ServiceTemplate.INVENTORY_ITEM)]
		[PXUIField(DisplayName = "Line Type")]
        public override string LineType { get; set; }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<lineType>))]
        [InventoryIDByLineType(typeof(lineType), Filterable = true)]
        [PXRestrictor(typeof(
                        Where<
                            InventoryItem.itemType, NotEqual<INItemTypes.serviceItem>,
                            Or<FSxServiceClass.requireRoute, Equal<True>,
                            Or<Current<FSSrvOrdType.requireRoute>, Equal<False>>>>),
                TX.Error.NONROUTE_SERVICE_CANNOT_BE_HANDLED_WITH_ROUTE_SRVORDTYPE)]
        [PXRestrictor(typeof(
                        Where<
                            InventoryItem.itemType, NotEqual<INItemTypes.serviceItem>,
                            Or<FSxServiceClass.requireRoute, Equal<False>,
                            Or<Current<FSSrvOrdType.requireRoute>, Equal<True>>>>),
                TX.Error.ROUTE_SERVICE_CANNOT_BE_HANDLED_WITH_NONROUTE_SRVORDTYPE)]
        public override int? InventoryID { get; set; }
        #endregion
    }
}