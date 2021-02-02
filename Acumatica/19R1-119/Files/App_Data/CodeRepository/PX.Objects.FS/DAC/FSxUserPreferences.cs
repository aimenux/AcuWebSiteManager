using PX.Data;
using PX.Objects.CS;
using PX.SM;
using System;

namespace PX.Objects.FS
{
    [PXTable(typeof(UserPreferences.userID), IsOptional = true)]
    public class FSxUserPreferences : PXCacheExtension<UserPreferences>
	{
        #region DfltBranchLocationID
        public abstract class dfltBranchLocationID : PX.Data.BQL.BqlInt.Field<dfltBranchLocationID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Default Branch Location", FieldClass = "SERVICEMANAGEMENT")]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID, Where<FSBranchLocation.branchID, Equal<Current<UserPreferences.defBranchID>>>>), SubstituteKey = typeof(FSBranchLocation.branchLocationCD), DescriptionField = typeof(FSBranchLocation.descr))]
        [PXFormula(typeof(Default<UserPreferences.defBranchID>))]
        public virtual int? DfltBranchLocationID { get; set; }
        #endregion
        #region TrackLocation
        public abstract class trackLocation : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Track Location")]
        public virtual bool? TrackLocation { get; set; }
        #endregion
        #region Interval
        public abstract class interval : PX.Data.IBqlField
        {
        }

        [PXDBShort(MinValue = 1)]
        [PXDefault((short)5, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Tracking Frequency", Enabled = false)]
        public virtual Int16? Interval { get; set; }
        #endregion
        #region Distance
        public abstract class distance : PX.Data.IBqlField
        {
        }

        [PXDBShort(MinValue = 1)]
        [PXDefault((short)250, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Distance Frequency", Enabled = false)]
        public virtual Int16? Distance { get; set; }
        #endregion
    }
}
