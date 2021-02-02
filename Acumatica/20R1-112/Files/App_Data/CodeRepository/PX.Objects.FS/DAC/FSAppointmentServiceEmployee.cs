using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXBreakInheritance]
    [PXProjection(typeof(Select<FSAppointmentDet>), Persistent = false)]
    public class FSAppointmentServiceEmployee : FSAppointmentDet
    {
        public new abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        public new abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        public new abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }

        #region LineRef
        public new abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public override string LineRef { get; set; }
        #endregion

        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        
        #endregion
    }
}