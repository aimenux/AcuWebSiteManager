using System;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXBreakInheritance]
    [PXProjection(typeof(Select<FSScheduleDet,
                        Where<
                            FSScheduleDet.lineType, Equal<ListField_LineType_Part_ALL.Inventory_Item>,
                            Or<FSScheduleDet.lineType, Equal<ListField_LineType_Part_ALL.Instruction_Part>,
                            Or<FSScheduleDet.lineType, Equal<ListField_LineType_Part_ALL.Comment_Part>>>>>), Persistent = true)]
    public class FSScheduleDetPart : FSScheduleDet
	{
        #region ScheduleID
        public new abstract class scheduleID : PX.Data.IBqlField
        {
        }

        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "ScheduleID")]
        [PXParent(typeof(Select<FSSchedule, Where<FSSchedule.scheduleID, Equal<Current<FSScheduleDetPart.scheduleID>>>>))]
        [PXDBLiteDefault(typeof(FSSchedule.scheduleID))]
        public override int? ScheduleID { get; set; }
        #endregion
        #region LineNbr
        public new abstract class lineNbr : PX.Data.IBqlField
        {
        }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSSchedule.lineCntr))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false)]
        public override int? LineNbr { get; set; }
        #endregion
        #region ScheduleDetID
        public new abstract class scheduleDetID : PX.Data.IBqlField
        {
        }

        [PXDBIdentity]
        [PXUIField(Enabled = false)]
        public override int? ScheduleDetID { get; set; }
        #endregion
        #region LineType
        public new abstract class lineType : ListField_LineType_Part_ALL
        {
        }

        [PXDBString(5, IsFixed = true)]
        [PXUIField(DisplayName = "Line Type")]
        [lineType.ListAtrribute]
        [PXDefault(ID.LineType_ServiceContract.INVENTORY_ITEM)]
        public override string LineType { get; set; }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.IBqlField
        {
        }

        [StockItem]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Inventory ID")]
        public override int? InventoryID { get; set; }
        #endregion
        #region Qty
        public new abstract class qty : PX.Data.IBqlField
        {
        }

        [PXDBQuantity(MinValue = 0.00)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Quantity")]
        public override decimal? Qty { get; set; }
        #endregion
        #region SMEquipmentID
        public new abstract class SMequipmentID : PX.Data.IBqlField
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorMaintenanceEquipment(typeof(FSSchedule.srvOrdType),
                                        typeof(FSSchedule.billCustomerID),
                                        typeof(FSSchedule.customerID),
                                        typeof(FSSchedule.customerLocationID),
                                        typeof(FSSchedule.branchLocationID),
                                        DescriptionField = typeof(FSEquipment.serialNumber))]
        [PXRestrictor(typeof(Where<FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                TX.Messages.EQUIPMENT_IS_INSTATUS, typeof(FSEquipment.status))]
        public override int? SMEquipmentID { get; set; }
        #endregion
        #region ServiceTemplateID
        public new abstract class serviceTemplateID : PX.Data.IBqlField
        {
        }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Service Template ID")]
        [PXSelector(typeof(Search<FSServiceTemplate.serviceTemplateID,
                            Where<FSServiceTemplate.srvOrdType,
                                Equal<Current<FSSchedule.srvOrdType>>>>),
                SubstituteKey = typeof(FSServiceTemplate.serviceTemplateCD),
                DescriptionField = typeof(FSServiceTemplate.descr))]
        public override int? ServiceTemplateID { get; set; }
        #endregion
        #region TranDesc
        public new abstract class tranDesc : PX.Data.IBqlField
        {
        }

        [PXFormula(typeof(Selector<inventoryID, InventoryItem.descr>))]
        [PXFormula(typeof(Selector<serviceTemplateID, FSServiceTemplate.descr>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Transaction Description")]
        public override string TranDesc { get; set; }
        #endregion
        #region BillingRule
        public new abstract class billingRule : ListField_BillingRule
        {
        }

        [PXDBString(4, IsFixed = true)]
        [billingRule.List]
        [PXDefault(ID.BillingRule.NONE)]
        [PXFormula(typeof(Switch<
                            Case<Where<FSScheduleDetPart.lineType, Equal<ListField_LineType_ALL.Service>>,
                                Selector<inventoryID, FSxService.billingRule>>,
                            //default case
                            FSScheduleDetPart.billingRule>))]
        [PXUIField(DisplayName = "Billing Rule")]
        public override string BillingRule { get; set; }
        #endregion
        #region NoteID
        public new abstract class noteID : PX.Data.IBqlField
        {
        }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public override Guid? NoteID { get; set; }
        #endregion
        #region ComponentID
        public new abstract class componentID : PX.Data.IBqlField
        {
        }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorComponentIDByFSEquipmentComponent(typeof(SMequipmentID))]
        public override int? ComponentID { get; set; }
        #endregion
        #region EquipmentLineRef
        public new abstract class equipmentLineRef : PX.Data.IBqlField
        {
        }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component Line Ref.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorEquipmentLineRef(typeof(SMequipmentID), typeof(componentID))]
        public override int? EquipmentLineRef { get; set; }
        #endregion
        #region CreatedByID
        public new abstract class createdByID : PX.Data.IBqlField
        {
        }

        [PXDBCreatedByID]
        public override Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public new abstract class createdByScreenID : PX.Data.IBqlField
        {
        }

        [PXDBCreatedByScreenID]
        public override string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public new abstract class createdDateTime : PX.Data.IBqlField
        {
        }

        [PXDBCreatedDateTime]
        public override DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public new abstract class lastModifiedByID : PX.Data.IBqlField
        {
        }

        [PXDBLastModifiedByID]
        public override Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public new abstract class lastModifiedByScreenID : PX.Data.IBqlField
        {
        }

        [PXDBLastModifiedByScreenID]
        public override string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public new abstract class lastModifiedDateTime : PX.Data.IBqlField
        {
        }

        [PXDBLastModifiedDateTime]
        public override DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public new abstract class Tstamp : PX.Data.IBqlField
        {
        }

        [PXDBTimestamp]
        public override byte[] tstamp { get; set; }
        #endregion

        #region EquipmentItemClass
        public new abstract class equipmentItemClass : PX.Data.IBqlField
        {
        }

        [PXString(2, IsFixed = true)]
        public override string EquipmentItemClass { get; set; }
        #endregion
    }
}