using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using System;

namespace PX.Objects.FS
{
    public class FSxEPExpenseClaimDetails : PXCacheExtension<EPExpenseClaimDetails>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region FSEntityTypeUI
        public abstract class fsEntityTypeUI : PX.Data.BQL.BqlString.Field<fsEntityTypeUI>
        {
            public abstract class Values : ListField_FSEntity_Type { }
        }

        [PXString]
        [PXUIField(DisplayName = "Doc. Type")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [fsEntityTypeUI.Values.List]
        public virtual String FSEntityTypeUI { get; set; }
        #endregion
        #region FSEntityType
        public String _FSEntityType;
        public abstract class fsEntityType : PX.Data.BQL.BqlString.Field<fsEntityType> { }

        [PXString()]
        [PXDefault(ID.FSEntityType.ServiceOrder, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Switch<Case<Where<fsEntityTypeUI, IsNotNull>, fsEntityTypeUI>, fsEntityTypeUI.Values.serviceOrder>))]
        public virtual String FSEntityType { get; set; }

        #endregion
        #region FSEntityNoteID
        public abstract class fsEntityNoteID : PX.Data.BQL.BqlGuid.Field<fsEntityNoteID> { }

        [PXUIField(DisplayName = "Reference Nbr.")]
        [FSEntityIDExpenseSelector(typeof(fsEntityTypeUI), typeof(EPExpenseClaimDetails.customerID), typeof(EPExpenseClaimDetails.contractID))]
        [PXDBGuid]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Guid? FSEntityNoteID { get; set; }
        #endregion
        #region IsDocBilledOrClosed
        public abstract class isDocBilledOrClosed : PX.Data.BQL.BqlBool.Field<isDocBilledOrClosed> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsDocBilledOrClosed { get; set; }
        #endregion
        #region IsDocRelatedToProject
        public abstract class isDocRelatedToProject : PX.Data.BQL.BqlBool.Field<isDocRelatedToProject> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsDocRelatedToProject { get; set; }
        #endregion
        #region FSBillable
        public abstract class fsBillable : PX.Data.BQL.BqlBool.Field<fsBillable> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXFormula(typeof(Default<fsEntityNoteID, fsEntityTypeUI>))]
        [PXUIField(DisplayName = "Billable in Field Service Document")]
        public virtual bool? FSBillable { get; set; }
        #endregion
    }
}