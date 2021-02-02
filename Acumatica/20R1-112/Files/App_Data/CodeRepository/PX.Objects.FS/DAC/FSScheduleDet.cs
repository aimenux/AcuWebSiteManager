using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.FS
{	
	[System.SerializableAttribute]
	public class FSScheduleDet : PX.Data.IBqlTable, ISortOrder
    {
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "ScheduleID")]
        [PXParent(typeof(Select<FSSchedule, Where<FSSchedule.scheduleID, Equal<Current<FSScheduleDet.scheduleID>>>>))]
        [PXDBLiteDefault(typeof(FSSchedule.scheduleID))]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
        {
        }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSSchedule.lineCntr))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false)]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region ScheduleDetID
        public abstract class scheduleDetID : PX.Data.BQL.BqlInt.Field<scheduleDetID> { }

        [PXDBIdentity]
        [PXUIField(Enabled = false)]
        public virtual int? ScheduleDetID { get; set; }
        #endregion
        #region SMEquipmentID
        public abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorMaintenanceEquipment(typeof(FSSchedule.srvOrdType),
                                        typeof(FSSchedule.billCustomerID),
                                        typeof(FSSchedule.customerID),
                                        typeof(FSSchedule.customerLocationID),
                                        typeof(FSSchedule.branchID),
                                        typeof(FSSchedule.branchLocationID),
                                        DescriptionField = typeof(FSEquipment.serialNumber))]
        [PXRestrictor(typeof(Where<FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                TX.Messages.EQUIPMENT_IS_INSTATUS, typeof(FSEquipment.status))]
        public virtual int? SMEquipmentID { get; set; }
        #endregion
        #region LineType
        public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType>
        {
        }

        private string _LineType;

        [PXDBString(5, IsFixed = true)]
        [PXDefault(ID.LineType_Schedule.SERVICE)]
        [PXUIField(DisplayName = "Line Type")]
        [FSLineType.List]
        public virtual string LineType
        {
            get
            {
                return this._LineType;
            }
            set
            {
                this._LineType = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXFormula(typeof(Default<lineType>))]
        [InventoryIDByLineType(typeof(lineType), Filterable = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Inventory ID")]
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

        public virtual int? InventoryID { get; set; }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        [PXDBQuantity(MinValue = 0.00)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Quantity")]
        public virtual decimal? Qty { get; set; }
        #endregion
        #region ServiceTemplateID
        public abstract class serviceTemplateID : PX.Data.BQL.BqlInt.Field<serviceTemplateID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Service Template ID")]
        [PXSelector(typeof(Search<FSServiceTemplate.serviceTemplateID,
                            Where<FSServiceTemplate.srvOrdType,
                                Equal<Current<FSSchedule.srvOrdType>>>>),
                SubstituteKey = typeof(FSServiceTemplate.serviceTemplateCD),
                DescriptionField = typeof(FSServiceTemplate.descr))]
        public virtual int? ServiceTemplateID { get; set; }
        #endregion
        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Sort Order", Visible = false, Enabled = false)]
        public virtual Int32? SortOrder { get; set; }
        #endregion
        #region TranDesc
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        [PXFormula(typeof(Selector<inventoryID, InventoryItem.descr>))]
        [PXFormula(typeof(Selector<serviceTemplateID, FSServiceTemplate.descr>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Transaction Description")]
        public virtual string TranDesc { get; set; }
        #endregion
        #region BillingRule
        public abstract class billingRule : ListField_BillingRule
        {
        }

        [PXDBString(4, IsFixed = true)]
        [billingRule.List]
        [PXFormula(typeof(Default<inventoryID, lineType>))]
        [PXDefault(typeof(Switch<
                            Case<Where<FSScheduleDet.lineType, Equal<ListField_LineType_ALL.Service>>,
                                Selector<inventoryID, FSxService.billingRule>>,
                            ListField_BillingRule.FlatRate>))]
        [PXUIField(DisplayName = "Billing Rule")]
        public virtual string BillingRule { get; set; }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region ComponentID
        public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorComponentIDByFSEquipmentComponent(typeof(SMequipmentID))]
        public virtual int? ComponentID { get; set; }
        #endregion
        #region EquipmentAction
        public abstract class equipmentAction : ListField_Schedule_EquipmentAction
        {
        }

        [PXDBString(2, IsFixed = true)]
        [equipmentAction.ListAtrribute]
        [PXDefault(ID.Equipment_Action.NONE)]
        [PXUIField(DisplayName = "Equipment Action", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public virtual string EquipmentAction { get; set; }
        #endregion
        #region EquipmentLineRef
        public abstract class equipmentLineRef : PX.Data.BQL.BqlInt.Field<equipmentLineRef> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component Line Ref.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorEquipmentLineRef(typeof(SMequipmentID), typeof(componentID))]
        public virtual int? EquipmentLineRef { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion

        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDBInt]
        [PXDBDefault(typeof(FSSchedule.projectID))]
        [PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Project Task", FieldClass = ProjectAttribute.DimensionName)]
        [PXFormula(typeof(Default<inventoryID, lineType>))]
        [PXDefault(typeof(Search<FSSchedule.dfltProjectTaskID, Where<FSSchedule.scheduleID, Equal<Current<scheduleID>>,
                            And<Current<lineType>, NotEqual<ListField_LineType_UnifyTabs.Comment>,
                            And<Current<lineType>, NotEqual<ListField_LineType_UnifyTabs.Instruction>>>>>),
                            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(Where<lineType, NotEqual<ListField_LineType_UnifyTabs.Comment>,
                            And<lineType, NotEqual<ListField_LineType_UnifyTabs.Instruction>>>))]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<projectID>>>))]
        [PXForeignReference(typeof(Field<projectTaskID>.IsRelatedTo<PMTask.taskID>))]
        public virtual int? ProjectTaskID { get; set; }
        #endregion
        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        [PXFormula(typeof(Default<inventoryID, serviceTemplateID, lineType>))]
        [SMCostCode(typeof(skipCostCodeValidation), null, typeof(projectTaskID), DisplayName = "Cost Code", Filterable = false, Enabled = false)]
        [PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
        public virtual int? CostCodeID { get; set; }
        #endregion

        #region SkipCostCodeValidation
        public abstract class skipCostCodeValidation : PX.Data.BQL.BqlBool.Field<skipCostCodeValidation> { }

        [PXBool]
        [PXFormula(typeof(IIf<
                                Where<lineType, Equal<ListField_LineType_UnifyTabs.Service>,
                                   Or<lineType, Equal<ListField_LineType_UnifyTabs.NonStockItem>,
                                   Or<lineType, Equal<ListField_LineType_UnifyTabs.Inventory_Item>,
                                   Or<lineType, Equal<ListField_LineType_ALL.Service_Template>>>>>, False, True>))]
        public virtual bool? SkipCostCodeValidation { get; set; }
        #endregion

        #region EquipmentItemClass
        public abstract class equipmentItemClass : PX.Data.BQL.BqlString.Field<equipmentItemClass>
        {
        }

        [PXString(2, IsFixed = true)]
        public virtual string EquipmentItemClass { get; set; }
        #endregion


        #region Unbound properties
        public bool IsService
        {
            get
            {
                return LineType == ID.LineType_AppSrvOrd.SERVICE || LineType == ID.LineType_AppSrvOrd.NONSTOCKITEM;
            }
        }

        public bool IsInventoryItem
        {
            get
            {
                return LineType == ID.LineType_AppSrvOrd.INVENTORY_ITEM;
            }
        }
        #endregion
    }
}
