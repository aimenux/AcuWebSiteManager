using PX.Data;

namespace PX.Objects.FS
{
    public class ContractInvoiceLine
    {
        #region Methods
        public ContractInvoiceLine() { }

        public ContractInvoiceLine(IDocLine row)
        {
            InventoryID = row.InventoryID;
            UOM = row.UOM;
            SMEquipmentID = row.SMEquipmentID;
            CuryUnitPrice = row.CuryUnitPrice;
            CuryBillableExtPrice = row.CuryBillableExtPrice;
            DiscPct = row.DiscPct;
            SubItemID = row.SubItemID;
            SiteID = row.SiteID;
            SiteLocationID = row.SiteLocationID;
            IsBillable = row.IsBillable;
            AcctID = row.AcctID;
            SubID = row.SubID;
            EquipmentAction = row.EquipmentAction;
            EquipmentLineRef = row.EquipmentLineRef;
            NewTargetEquipmentLineNbr = row.NewTargetEquipmentLineNbr;
            ComponentID = row.ComponentID;
            LineRef = row.LineRef;
            TranDescPrefix = string.Empty;
            ProjectTaskID = row.ProjectTaskID;
            CostCodeID = row.CostCodeID;
            Processed = false;
        }

        public ContractInvoiceLine(PXResult<FSContractPeriodDet, FSContractPeriod, FSServiceContract, FSBranchLocation> row)
        {
            FSServiceContract fsServiceContractRow = (FSServiceContract)row;
            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)row;
            FSBranchLocation fsBranchLocationRow = (FSBranchLocation)row;

            ServiceContractID = fsServiceContractRow.ServiceContractID;
            ContractType = fsServiceContractRow.RecordType;
            ContractPeriodID = fsContractPeriodDetRow.ContractPeriodDetID;
            ContractPeriodDetID = fsContractPeriodDetRow.ContractPeriodID;

            BillingRule = fsContractPeriodDetRow.BillingRule;
            InventoryID = fsContractPeriodDetRow.InventoryID;
            UOM = fsContractPeriodDetRow.UOM;
            SMEquipmentID = fsContractPeriodDetRow.SMEquipmentID;
            CuryUnitPrice = fsContractPeriodDetRow.RecurringUnitPrice;
            
            ContractRelated = false;
            SubItemID = fsBranchLocationRow?.DfltSubItemID;
            SiteID = fsBranchLocationRow?.DfltSiteID;
            SiteLocationID = null;
            IsBillable = true;

            if (BillingRule == ID.BillingRule.TIME)
            {
                Qty = decimal.Divide((decimal)(fsContractPeriodDetRow.Time ?? 0), 60);
            }
            else
            {
                Qty = fsContractPeriodDetRow.Qty;
            }
            
            OverageItemPrice = fsContractPeriodDetRow.OverageItemPrice;
            AcctID = null;
            SubID = null;
            EquipmentAction = ID.Equipment_Action.NONE;
            EquipmentLineRef = null;
            NewTargetEquipmentLineNbr = null;
            ComponentID = null;
            LineRef = string.Empty;
            SalesPersonID = fsServiceContractRow.SalesPersonID;
            Commissionable = fsServiceContractRow.Commissionable;

            TranDescPrefix = string.Empty;

            ProjectTaskID = fsContractPeriodDetRow.ProjectTaskID;
            CostCodeID = fsContractPeriodDetRow.CostCodeID;

            Processed = false;
        }

        public ContractInvoiceLine(PXResult<FSAppointmentDet, FSSODet, FSAppointment> row) : this((IDocLine)(FSAppointmentDet)row)
        { 
            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)row;
            FSAppointment fsAppointmentRow = (FSAppointment)row;
            FSSODet fsSODetRow = (FSSODet)row;

            AppointmentID = fsAppointmentDetRow.AppointmentID;
            AppDetID = fsAppointmentDetRow.AppDetID;

            BillingRule = fsSODetRow.BillingRule;
            ContractRelated = fsAppointmentDetRow.ContractRelated;
            Qty = fsAppointmentDetRow.ContractRelated == true ? fsAppointmentDetRow.Qty : fsAppointmentDetRow.BillableQty;
            OverageItemPrice = fsAppointmentDetRow.OverageItemPrice;
            SalesPersonID = fsAppointmentRow.SalesPersonID;
            Commissionable = fsAppointmentRow.Commissionable;
        }

        public ContractInvoiceLine(PXResult<FSSODet, FSServiceOrder> row) : this((IDocLine)(FSSODet)row)
        {
            FSSODet fsSODetRow = (FSSODet)row;
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)row;

            SOID = fsSODetRow.SOID;
            SODetID = fsSODetRow.SODetID;

            BillingRule = fsSODetRow.BillingRule;
            ContractRelated = fsSODetRow.ContractRelated;
            Qty = fsSODetRow.ContractRelated == true ? fsSODetRow.EstimatedQty : fsSODetRow.BillableQty;
            OverageItemPrice = fsSODetRow.CuryExtraUsageUnitPrice;
            SalesPersonID = fsServiceOrderRow.SalesPersonID;
            Commissionable = fsServiceOrderRow.Commissionable;
        }

        public ContractInvoiceLine(ContractInvoiceLine row, decimal? qty) : this(row)
        {
            Qty = qty;
        }

        public ContractInvoiceLine(ContractInvoiceLine row)
        {
            ServiceContractID = row.ServiceContractID;
            ContractType = row.ContractType;
            ContractPeriodID = row.ContractPeriodDetID;
            ContractPeriodDetID = row.ContractPeriodID;

            AppointmentID = row.AppointmentID;
            AppDetID = row.AppDetID;

            SOID = row.SOID;
            SODetID = row.SODetID;

            BillingRule = row.BillingRule;
            InventoryID = row.InventoryID;
            UOM = row.UOM;
            SMEquipmentID = row.SMEquipmentID;
            CuryUnitPrice = row.CuryUnitPrice;
            DiscPct = row.DiscPct;
            ContractRelated = row.ContractRelated;
            SubItemID = row.SubItemID;
            SiteID = row.SiteID;
            SiteLocationID = row.SiteLocationID;
            IsBillable = row.IsBillable;

            Qty = row.Qty;

            CuryBillableExtPrice = row.CuryBillableExtPrice;
            OverageItemPrice = row.OverageItemPrice;
            AcctID = row.AcctID;
            SubID = row.SubID;
            EquipmentAction = row.EquipmentAction;
            EquipmentLineRef = row.EquipmentLineRef;
            NewTargetEquipmentLineNbr = row.NewTargetEquipmentLineNbr;
            ComponentID = row.ComponentID;
            LineRef = row.LineRef;
            SalesPersonID = row.SalesPersonID;
            Commissionable = row.Commissionable;

            TranDescPrefix = string.Empty;

            ProjectTaskID = row.ProjectTaskID;
            CostCodeID = row.CostCodeID;

            Processed = false;
        }
        #endregion

        #region Contract Fields
        public int? ServiceContractID { get; set; }

        public int? ContractPeriodID { get; set; }

        public int? ContractPeriodDetID { get; set; }

        public string ContractType { get; set; }
        #endregion

        #region Appointment Fields
        public int? AppointmentID { get; set; }

        public int? AppDetID { get; set; }
        #endregion

        #region Service Order Fields
        public int? SOID { get; set; }

        public int? SODetID { get; set; }
        #endregion

        #region Group Fields

        public string BillingRule { get; set; }

        public int? InventoryID { get; set; }

        public string UOM { get; set; }

        public int? SMEquipmentID { get; set; }

        public decimal? CuryUnitPrice { get; set; }

        public decimal? CuryBillableExtPrice { get; set; }

        public decimal? DiscPct { get; set; }

        public bool? ContractRelated { get; set; }

        public int? SubItemID { get; set; }

        public int? SiteID { get; set; }

        public int? SiteLocationID { get; set; }

        public bool? IsBillable { get; set; }
        #endregion

        #region BoundFields
        public decimal? OverageItemPrice { get; set; }

        public decimal? Qty { get; set; }

        public int? AcctID { get; set; }

        public int? SubID { get; set; }

        public string EquipmentAction { get; set; }

        public int? EquipmentLineRef { get; set; }

        public string NewTargetEquipmentLineNbr { get; set; }

        public int? ComponentID { get; set; }

        public string LineRef { get; set; }

        public int? SalesPersonID { get; set; }

        public bool? Commissionable { get; set; }

        public int? ProjectTaskID { get; set; }

        public int? CostCodeID { get; set; }
        #endregion

        #region UnboundFields
        public string TranDescPrefix { get; set; }

        public bool? Processed { get; set; }
        #endregion
    }
}