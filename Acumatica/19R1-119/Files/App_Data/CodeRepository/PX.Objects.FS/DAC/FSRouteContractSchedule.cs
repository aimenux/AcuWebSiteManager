using PX.Data;
using System;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.AR;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXPrimaryGraph(typeof(RouteServiceContractScheduleEntry))]
    public class FSRouteContractSchedule : FSSchedule
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
                                        FSServiceContract.recordType, Equal<FSServiceContract.recordType.RouteServiceContract>>>),
                           SubstituteKey = typeof(FSServiceContract.refNbr))]
        public override int? EntityID { get; set; }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.IBqlField
        {
        }

        //Included in FSRouteContractScheduleFSServiceContract projection
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXSelector(typeof(Search<FSRouteContractSchedule.refNbr,
                           Where<FSRouteContractSchedule.entityID, Equal<Current<FSRouteContractSchedule.entityID>>,
                               And<FSRouteContractSchedule.entityType, Equal<FSRouteContractSchedule.entityType.Contract>>>,
                           OrderBy<Desc<FSRouteContractSchedule.refNbr>>>))]
        [AutoNumber(typeof(Search<FSSetup.scheduleNumberingID>), typeof(AccessInfo.businessDate))]
        public override string RefNbr { get; set; }
        #endregion
        #region CustomerID
        public abstract new class customerID : PX.Data.IBqlField
        {
        }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Selector<FSContractSchedule.entityID, FSServiceContract.customerID>))]
        [FSSelectorContractScheduleCustomer(typeof(Where<FSServiceContract.recordType, Equal<FSServiceContract.recordType.RouteServiceContract>>))]
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
        [PXDefault]
        [FSSelectorRouteContractSrvOrdTypeAttribute]
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
                                       FSServiceContract.customerID, Equal<Current<FSRouteContractSchedule.customerID>>,
                                       And<FSServiceContract.serviceContractID, Equal<Current<FSRouteContractSchedule.entityID>>>>>))]
        public override string ScheduleGenType { get; set; }
        #endregion
        #region EntityType
        public new abstract class entityType : ListField_Schedule_EntityType
        {
        }
        #endregion
    }
}