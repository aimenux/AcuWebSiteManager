using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    [PXTable(typeof(EPEquipment.equipmentID), IsOptional = true)]
    public class FSxEquipment : PXCacheExtension<EPEquipment>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Branch location ID", Enabled = false)]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID,
                            Where<FSBranchLocation.branchID, Equal<Current<FSServiceOrder.branchID>>>>),
                            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
                            DescriptionField = typeof(FSBranchLocation.descr))]
        [PXFormula(typeof(Default<FSxEquipment.branchID>))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Staff Member ID", Enabled = false)]
        [FSSelector_StaffMember_All]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region RoomID
        public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }

        [PXDBString(10)]
        [PXUIField(DisplayName = "Room", Enabled = false)]
        [PXSelector(typeof(Search<FSRoom.roomID, 
                                    Where<FSRoom.branchLocationID, 
                                    Equal<Current<FSxEquipment.branchLocationID>>>>), 
                            DescriptionField = typeof(FSRoom.descr))]
        [PXFormula(typeof(Default<FSxEquipment.branchLocationID>))]
        public virtual string RoomID { get; set; }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Warehouse ID", Enabled = false)]
        [PXSelector(typeof(INSite.siteID), 
                    SubstituteKey = typeof(INSite.siteCD))]
        public virtual int? SiteID { get; set; }
        #endregion
        #region AllowSchedule
        public abstract class allowSchedule : PX.Data.BQL.BqlBool.Field<allowSchedule> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Show in Calendars and Route Maps")]
        public virtual bool? AllowSchedule { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXDefault(typeof(AccessInfo.branchID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch ID", Enabled = false)]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
        #endregion
    }
}