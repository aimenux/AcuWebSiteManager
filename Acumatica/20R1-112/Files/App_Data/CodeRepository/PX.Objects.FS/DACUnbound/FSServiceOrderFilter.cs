using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public partial class ServiceOrderFilter : IBqlTable
    {
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXInt]
        [PXUIField(DisplayName = "Customer ID")]
        [FSSelectorCustomer]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<ServiceOrderFilter.customerID>>>),
                    DescriptionField = typeof(Location.descr), DisplayName = "Customer Location ID", DirtyRead = true)]
        public virtual int? CustomerLocationID { get; set; }
        #endregion
        #region ServiceID
        public abstract class serviceID : PX.Data.BQL.BqlInt.Field<serviceID> { }

        [PXInt]
        ////Warning: The UIFIeld attribute has to be positioned BEFORE the ServiceInventory lookup because the second one includes an internal UIFIeld attribute and we need to use the one specified in this Filter DAC
        [PXUIField(DisplayName = "Service ID", Visibility = PXUIVisibility.Visible)]
        [Service]
        public virtual int? ServiceID { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        [FSSelectorSrvOrdType]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region FromDate
        public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

        [PXDate]
        [PXUIField(DisplayName = "From Date")]
        public virtual DateTime? FromDate { get; set; }
        #endregion
        #region ToDate
        public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

        [PXDate]
        [PXUIField(DisplayName = "To Date")]
        public virtual DateTime? ToDate { get; set; }
        #endregion
        #region ReportedFlag
        public abstract class reportedFlag : PX.Data.BQL.BqlBool.Field<reportedFlag> { }

        [PXBool]
        [PXUIField(DisplayName = "Reported")]
        public virtual bool? ReportedFlag { get; set; }
        #endregion
        #region CheckOutFlag
        public abstract class checkOutFlag : PX.Data.BQL.BqlBool.Field<checkOutFlag> { }

        [PXBool]
        [PXUIField(DisplayName = "Checked Out")]
        public virtual bool? CheckOutFlag { get; set; }
        #endregion
        #region SignOffFlag
        public abstract class signOffFlag : PX.Data.BQL.BqlBool.Field<signOffFlag> { }

        [PXBool]
        [PXUIField(DisplayName = "Signed Off")]
        public virtual bool? SignOffFlag { get; set; }
        #endregion
        #region SLAFlag
        public abstract class sLAFlag : PX.Data.BQL.BqlBool.Field<sLAFlag> { }

        [PXBool]
        [PXUIField(DisplayName = "Deadline - SLA")]
        public virtual bool? SLAFlag { get; set; }
        #endregion        
        #region CompletedOrders
        public abstract class completedOrder : PX.Data.BQL.BqlBool.Field<completedOrder> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Completed")]
        public virtual bool? CompletedOrder { get; set; }
        #endregion
        #region NotSignedOff
        public abstract class notSignedOff : PX.Data.BQL.BqlBool.Field<notSignedOff> { }

        [PXBool]
        [PXUIField(DisplayName = "Not Signed Off")]
        public virtual bool? NotSignedOff { get; set; }
        #endregion
        #region SignedOff
        public abstract class signedOff : PX.Data.BQL.BqlBool.Field<signedOff> { }

        [PXBool]
        [PXUIField(DisplayName = "Signed Off")]
        public virtual bool? SignedOff { get; set; }
        #endregion
        #region NotCheckedOut
        public abstract class notCheckedOut : PX.Data.BQL.BqlBool.Field<notCheckedOut> { }

        [PXBool]
        [PXUIField(DisplayName = "Not Checked Out")]
        public virtual bool? NotCheckedOut { get; set; }
        #endregion
        #region CheckedOut
        public abstract class checkedOut : PX.Data.BQL.BqlBool.Field<checkedOut> { }

        [PXBool]
        [PXUIField(DisplayName = "Checked Out")]
        public virtual bool? CheckedOut { get; set; }
        #endregion
        #region Posted
        public abstract class posted : PX.Data.BQL.BqlBool.Field<posted> { }

        [PXBool]
        [PXUIField(DisplayName = "Posted")]
        public virtual bool? Posted { get; set; }
        #endregion
        #region NotPosted
        public abstract class notPosted : PX.Data.BQL.BqlBool.Field<notPosted> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Not Posted")]
        public virtual bool? NotPosted { get; set; }
        #endregion
        #region WFStageID
        public abstract class wFStageID : PX.Data.BQL.BqlInt.Field<wFStageID> { }

        [PXInt]
        [PXUIField(DisplayName = "Workflow Stage ID")]
        [PXSelector(typeof(Search2<FSWFStage.wFStageID,
                InnerJoin<FSSrvOrdType,
                    On<
                        FSSrvOrdType.srvOrdTypeID, Equal<FSWFStage.wFID>>>,
                Where<
                    FSSrvOrdType.srvOrdType, Equal<Current<ServiceOrderFilter.srvOrdType>>>,
                OrderBy<
                    Asc<FSWFStage.parentWFStageID,
                    Asc<FSWFStage.sortOrder>>>>), SubstituteKey = typeof(FSWFStage.wFStageCD))]
        public virtual int? WFStageID { get; set; }
        #endregion
        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Service Contract ID")]
        [PXSelector(typeof(
                Search<
                    FSServiceContract.serviceContractID,
                Where2<
                    Where<
                        FSServiceContract.recordType, Equal<FSServiceContract.recordType.ServiceContract>,
                        Or<
                            FSServiceContract.recordType, Equal<FSServiceContract.recordType.RouteServiceContract>>>,                
                And<
                    Where<
                        Current<ServiceOrderFilter.customerID>, IsNull,
                    Or<
                        FSServiceContract.customerID, Equal<Current<ServiceOrderFilter.customerID>>>>>>>),
                SubstituteKey = typeof(FSServiceContract.refNbr))]
        public virtual int? ServiceContractID { get; set; }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Schedule ID")]
        [PXSelector(typeof(
                Search<
                    FSSchedule.scheduleID,
                Where<
                    FSSchedule.entityID, Equal<Current<ServiceOrderFilter.serviceContractID>>,
                And<
                    FSSchedule.entityType, Equal<FSSchedule.entityType.Contract>>>,
                OrderBy<
                    Desc<FSSchedule.refNbr>>>),
                SubstituteKey = typeof(FSSchedule.refNbr))]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region BillingCycleID
        public abstract class billingCycleID : PX.Data.BQL.BqlInt.Field<billingCycleID> { }

        [PXInt]
        [PXUIField(DisplayName = "Billing Cycle ID")]
        [PXSelector(typeof(FSBillingCycle.billingCycleID), SubstituteKey = typeof(FSBillingCycle.billingCycleCD))]
        public virtual int? BillingCycleID { get; set; }
        #endregion
        #region BillingCycle_None
        public abstract class billingCycleNone : PX.Data.BQL.BqlBool.Field<billingCycleNone> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "No Billing Cycle")]
        public virtual bool? BillingCycleNone { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXInt]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXInt]
        [PXDefault(typeof(Search<FSBranchLocation.branchLocationID,
                        Where<FSBranchLocation.branchID, Equal<Current<ServiceContractFilter.branchID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch Location")]
        [FSSelectorBranchLocation]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region SOAction
        public abstract class soAction : ListField_ServiceOrder_Action_Filter
        {
        }

        [PXString(2, IsFixed = true)]
        [soAction.ListAtrribute]
        [PXUIField(DisplayName = "Action")]
        [PXDefault(ID.ServiceOrder_Action_Filter.UNDEFINED)]
        public virtual string SOAction { get; set; }
        #endregion
        #region Status
        public abstract class status : ListField_Status_ServiceOrder
        {
        }

        [PXString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
        [status.ListAtrribute]
        public virtual string Status { get; set; }
        #endregion
    }
}