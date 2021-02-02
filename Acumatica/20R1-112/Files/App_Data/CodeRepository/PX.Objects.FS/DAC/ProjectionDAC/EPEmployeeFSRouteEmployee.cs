using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    [Serializable]
    [PXPrimaryGraph(typeof(EPEmployeeMaintBridge))]
    [PXProjection(typeof(
        Select2<EPEmployee,
            InnerJoin<BAccount,
                On<BAccount.bAccountID, Equal<EPEmployee.bAccountID>>,
            InnerJoin<FSRouteEmployee,
                On<FSRouteEmployee.employeeID, Equal<BAccount.bAccountID>>>>>))]
    public class EPEmployeeFSRouteEmployee : EPEmployee
    {
        #region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        [PXDBInt(IsKey = true, BqlField = typeof(BAccount.bAccountID))]
        [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
        public override int? BAccountID { get; set; }
        #endregion

        #region AcctCD
        public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

        [EmployeeRaw]
        [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(BAccount.acctCD))]
        [PXDefault]
        [PXFieldDescription]
        [PXUIField(DisplayName = "Driver ID", Visibility = PXUIVisibility.SelectorVisible)]
        public override string AcctCD { get; set; }
        #endregion

        #region AcctName
        public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

        [PXDBString(60, IsUnicode = true, BqlField = typeof(BAccount.acctName))]
        [PXDefault]
        [PXUIField(DisplayName = "Driver Name", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        public override string AcctName { get; set; }
        #endregion

        #region RouteID
        public abstract class routeID : PX.Data.BQL.BqlInt.Field<routeID> { }

        [PXDefault]
        [PXDBInt(BqlField = typeof(FSRouteEmployee.routeID))]
        public virtual int? RouteID { get; set; }
        #endregion

        #region PriorityPreference
        public abstract class priorityPreference : PX.Data.BQL.BqlInt.Field<priorityPreference> { }

        [PXDBInt(BqlField = typeof(FSRouteEmployee.priorityPreference))]
        [PXDefault(1)]
        [PXUIField(DisplayName = "Priority Preference", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? PriorityPreference { get; set; }
        #endregion

        #region Status
        public new abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
        }

        [PXDBString(1, IsFixed = true, BqlField = typeof(BAccount.status))]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
        [EPEmployee.status.List]
        public override string Status { get; set; }
        #endregion

        #region DepartmentID
        public new abstract class departmentID : PX.Data.BQL.BqlString.Field<departmentID> { }

        [PXDBString(10, IsUnicode = true, BqlField = typeof(EPEmployee.departmentID))]
        [PXDefault]
        [PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
        [PXUIField(DisplayName = "Department", Visibility = PXUIVisibility.SelectorVisible)]
        public override string DepartmentID { get; set; }
        #endregion


        #region Memory Helper
        #region MemDriverName
        public abstract class memDriverName : PX.Data.BQL.BqlString.Field<memDriverName>
        {
        }

        [PXString]
        [PXUIField(DisplayName = "Driver Name", Enabled = false)]
        public virtual string MemDriverName { get; set; }
        #endregion
        #endregion
    }
}
