using PX.Common;
using PX.Data;
using System;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.AR;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXPrimaryGraph(typeof(ServiceContractScheduleEntry))]
    public class FSContractSchedule : FSSchedule
	{
        #region EntityID
        public new abstract class entityID : PX.Data.IBqlField
        {
        }

        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Service Contract ID")]
        [PXSelector(typeof(Search<FSServiceContract.serviceContractID,
                                    Where<
                                        FSServiceContract.recordType, Equal<FSServiceContract.recordType.ServiceContract>>>),
                           SubstituteKey = typeof(FSServiceContract.refNbr))]
        public override int? EntityID { get; set; }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.IBqlField
        {
        }

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXSelector(typeof(Search<FSContractSchedule.refNbr,
                           Where<FSContractSchedule.entityID, Equal<Current<FSContractSchedule.entityID>>,
                               And<FSContractSchedule.entityType, Equal<FSContractSchedule.entityType.Contract>>>,
                           OrderBy<Desc<FSContractSchedule.refNbr>>>))]
        [AutoNumber(typeof(Search<FSSetup.scheduleNumberingID>), typeof(AccessInfo.businessDate))]
        public override string RefNbr { get; set; }
        #endregion
        #region CustomerID
        public abstract new class customerID : PX.Data.IBqlField
        {
        }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Customer", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Selector<FSContractSchedule.entityID, FSServiceContract.customerID>))]
        [FSSelectorContractScheduleCustomer(typeof(Where<FSServiceContract.recordType, Equal<FSServiceContract.recordType.ServiceContract>>))]
        [PXRestrictor(typeof(Where<Customer.status, IsNull,
               Or<Customer.status, Equal<BAccount.status.active>,
               Or<Customer.status, Equal<BAccount.status.oneTime>>>>),
               PX.Objects.AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
        public override int? CustomerID { get; set; }
        #endregion
        #region SrvOrdType
        public new abstract class srvOrdType : PX.Data.IBqlField
        {
        }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault(typeof(Coalesce<
            Search2<FSxUserPreferences.dfltSrvOrdType,
            InnerJoin<
                FSSrvOrdType, On<FSSrvOrdType.srvOrdType, Equal<FSxUserPreferences.dfltSrvOrdType>>>,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                And<FSSrvOrdType.behavior, NotEqual<ListField_Behavior_SrvOrdType.RouteAppointment>>>>,
            Search<FSSetup.dfltSrvOrdType>>))]
        [FSSelectorContractSrvOrdType]
        public override string SrvOrdType { get; set; }
        #endregion
        #region ScheduleGenType
        public new abstract class scheduleGenType : ListField_ScheduleGenType_ContractSchedule
        {
        }

        [PXDBString(2, IsUnicode = true)]
        [scheduleGenType.ListAtrribute]
        [PXUIField(DisplayName = "Schedule Generation Type")]
        [PXDefault(typeof(Search<FSServiceContract.scheduleGenType,
                                  Where<
                                       FSServiceContract.customerID, Equal<Current<FSContractSchedule.customerID>>,
                                       And<FSServiceContract.serviceContractID, Equal<Current<FSContractSchedule.entityID>>>>>))]
        public override string ScheduleGenType { get; set; }
        #endregion
        #region EntityType
        public new abstract class entityType : ListField_Schedule_EntityType
        {
        }
        #endregion
        #region ProjectID
        public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDefault(typeof(Search<FSServiceContract.projectID,
                          Where<
                              FSServiceContract.serviceContractID, Equal<Current<FSContractSchedule.entityID>>,
                          And<
                              Current<FSContractSchedule.entityType>, Equal<FSSchedule.entityType.Contract>>>>))]
        [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        [ProjectBase(typeof(customerID), Enabled = false)]
        public override int? ProjectID { get; set; }
        #endregion
        #region DfltProjectTaskID
        public new abstract class dfltProjectTaskID : PX.Data.BQL.BqlInt.Field<dfltProjectTaskID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Default Project Task", Enabled = false, FieldClass = ProjectAttribute.DimensionName)]
        [PXDefault(typeof(Search2<FSServiceContract.dfltProjectTaskID,
                            InnerJoin<FSSrvOrdType, On<FSSrvOrdType.srvOrdType, Equal<Current<srvOrdType>>>,
                            InnerJoin<PMTask, On<PMTask.taskID, Equal<FSServiceContract.dfltProjectTaskID>,
                                        And<PMTask.projectID, Equal<Current<projectID>>>>>>,
                          Where<
                              FSServiceContract.serviceContractID, Equal<Current<entityID>>,
                          And<
                              Current<entityType>, Equal<FSSchedule.entityType.Contract>,
                            And2<
                                Where<FSSrvOrdType.enableINPosting, Equal<False>, Or<PMTask.visibleInIN, Equal<True>>>,
                                And<
                                    Where2<
                                        Where<
                                            FSSrvOrdType.postTo, Equal<FSPostTo.None>>,
                                        Or<
                                            Where2<
                                                Where<
                                                    FSSrvOrdType.postTo, Equal<FSPostTo.Accounts_Receivable_Module>,
                                                    And<
                                                        Where<
                                                            PMTask.visibleInAR, Equal<True>>>>,
                                            Or<
                                                Where2<
                                                    Where<
                                                        FSSrvOrdType.postTo, Equal<FSPostTo.Sales_Order_Module>,
                                                            Or<FSSrvOrdType.postTo, Equal<FSPostTo.Sales_Order_Invoice>>>,
                                                    And<
                                                        Where<
                                                            PMTask.visibleInSO, Equal<True>>>>>>>>>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]

        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<projectID>>>))]
        public override int? DfltProjectTaskID { get; set; }
        #endregion
    }
}