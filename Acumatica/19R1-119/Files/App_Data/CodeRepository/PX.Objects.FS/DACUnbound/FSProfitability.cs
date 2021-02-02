using PX.Data;
using PX.Objects.CM;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSProfitability : IBqlTable
    {
        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Line Ref.")]
        public virtual string LineRef { get; set; }
        #endregion
        #region LineType
        public abstract class lineType : ListField_LineType_Profitability
        {
        }

        [PXString(5, IsFixed = true)]
        [PXUIField(DisplayName = "Line Type")]
        [lineType.ListAtrribute]
        [PXDefault]
        public virtual string LineType { get; set; }
        #endregion
        #region ItemID
        public abstract class itemID : PX.Data.BQL.BqlInt.Field<itemID> { }

        [InventoryIDByLineType(typeof(lineType))]
        public virtual int? ItemID { get; set; }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Staff Member")]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region UnitPrice
        public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Unit Price")]
        public virtual decimal? UnitPrice { get; set; }
        #endregion
        #region EstimatedQty
        public abstract class estimatedQty : PX.Data.BQL.BqlDecimal.Field<estimatedQty> { }

        [PXQuantity]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Estimated Quantity")]
        public virtual decimal? EstimatedQty { get; set; }
        #endregion
        #region EstimatedAmount
        public abstract class estimatedAmount : PX.Data.BQL.BqlDecimal.Field<estimatedAmount> { }

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Estimated Amount")]
        public virtual decimal? EstimatedAmount { get; set; }
        #endregion
        #region ActualDuration
        public abstract class actualDuration : PX.Data.BQL.BqlInt.Field<actualDuration> { }

        [PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Actual Duration")]
        public virtual int? ActualDuration { get; set; }
        #endregion
        #region ActualQty
        public abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty> { }

        [PXQuantity]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Actual Quantity")]
        public virtual decimal? ActualQty { get; set; }
        #endregion
        #region ActualAmount
        public abstract class actualAmount : PX.Data.BQL.BqlDecimal.Field<actualAmount> { }

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Actual Amount")]
        public virtual decimal? ActualAmount { get; set; }
        #endregion
        #region BillableQty
        public abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }

        [PXQuantity]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Billable Quantity")]
        public virtual decimal? BillableQty { get; set; }
        #endregion
        #region BillableAmount
        public abstract class billableAmount : PX.Data.BQL.BqlDecimal.Field<billableAmount> { }

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Billable Amount")]
        public virtual decimal? BillableAmount { get; set; }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        [PXPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Unit Cost")]
        public virtual Decimal? UnitCost { get; set; }
        #endregion
        #region CostTotal
        public abstract class costTotal : PX.Data.BQL.BqlDecimal.Field<costTotal> { }

        [PXPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Cost Total")]
        public virtual Decimal? CostTotal { get; set; }
        #endregion
        #region Profit
        public abstract class profit : PX.Data.BQL.BqlDecimal.Field<profit> { }

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Profit")]
        public virtual decimal? Profit { get; set; }
        #endregion
        #region ProfitPercent
        public abstract class profitPercent : PX.Data.BQL.BqlDecimal.Field<profitPercent> { }

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Profit (%)")]
        public virtual decimal? ProfitPercent { get; set; }
        #endregion

        #region Constructors

        public FSProfitability()
        {
        }

        public FSProfitability(FSAppointmentDetPart fsAppointmentDetPartRow)
        {
            this.LineRef = fsAppointmentDetPartRow.LineRef;
            this.LineType = fsAppointmentDetPartRow.LineType;
            this.ItemID = fsAppointmentDetPartRow.InventoryID;
            this.Descr = fsAppointmentDetPartRow.TranDesc;
            this.UnitPrice = fsAppointmentDetPartRow.CuryUnitPrice;
            this.EstimatedQty = fsAppointmentDetPartRow.EstimatedQty;
            this.EstimatedAmount = fsAppointmentDetPartRow.CuryEstimatedTranAmt;
            this.ActualDuration = 0;
            this.ActualQty = fsAppointmentDetPartRow.Qty;
            this.ActualAmount = fsAppointmentDetPartRow.CuryTranAmt;
            this.BillableQty = fsAppointmentDetPartRow.BillableQty;
            this.BillableAmount = fsAppointmentDetPartRow.CuryBillableTranAmt;
            this.UnitCost = fsAppointmentDetPartRow.CuryUnitCost;
            this.CostTotal = this.BillableQty * this.UnitCost;
            this.Profit = this.BillableAmount - this.CostTotal;
            this.ProfitPercent = this.CostTotal == 0.0m ? 0.0m : (this.Profit / this.CostTotal) * 100;
        }

        public FSProfitability(FSAppointmentDetService fsAppointmentDetServiceRow)
        {
            this.LineRef = fsAppointmentDetServiceRow.LineRef;
            this.LineType = fsAppointmentDetServiceRow.LineType;
            this.ItemID = fsAppointmentDetServiceRow.InventoryID;
            this.Descr = fsAppointmentDetServiceRow.TranDesc;
            this.UnitPrice = fsAppointmentDetServiceRow.CuryUnitPrice;
            this.EstimatedQty = fsAppointmentDetServiceRow.EstimatedQty;
            this.EstimatedAmount = fsAppointmentDetServiceRow.CuryEstimatedTranAmt;
            this.ActualDuration = fsAppointmentDetServiceRow.ActualDuration;
            this.ActualQty = fsAppointmentDetServiceRow.Qty;
            this.ActualAmount = fsAppointmentDetServiceRow.CuryTranAmt;
            this.BillableQty = fsAppointmentDetServiceRow.BillableQty;
            this.BillableAmount = fsAppointmentDetServiceRow.CuryBillableTranAmt;
            this.UnitCost = fsAppointmentDetServiceRow.CuryUnitCost;
            this.CostTotal = this.BillableQty * this.UnitCost;
            this.Profit = this.BillableAmount - this.CostTotal;
            this.ProfitPercent = this.CostTotal == 0.0m ? 0.0m : (this.Profit / this.CostTotal) * 100;
        }

        public FSProfitability(FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            this.LineRef = fsAppointmentEmployeeRow.LineRef;
            this.LineType = ID.LineType_Profitability.LABOR_ITEM;
            this.ItemID = fsAppointmentEmployeeRow.LaborItemID;
            this.EmployeeID = fsAppointmentEmployeeRow.EmployeeID;
            this.ActualDuration = fsAppointmentEmployeeRow.ActualDuration;
            this.ActualQty = ((decimal)fsAppointmentEmployeeRow.ActualDuration)/60m;
            this.UnitCost = fsAppointmentEmployeeRow.CuryUnitCost;
            this.CostTotal = fsAppointmentEmployeeRow.CuryExtCost;
        }

        #endregion
    }
}
