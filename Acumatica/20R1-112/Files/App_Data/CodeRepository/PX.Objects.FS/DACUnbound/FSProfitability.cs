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

        public FSProfitability(FSAppointmentDet fsAppointmentDetRow)
        {
            this.LineRef = fsAppointmentDetRow.LineRef;
            this.LineType = fsAppointmentDetRow.LineType;
            this.ItemID = fsAppointmentDetRow.InventoryID;
            this.Descr = fsAppointmentDetRow.TranDesc;
            this.UnitPrice = fsAppointmentDetRow.CuryUnitPrice;
            this.EstimatedQty = fsAppointmentDetRow.EstimatedQty;
            this.EstimatedAmount = fsAppointmentDetRow.CuryEstimatedTranAmt;
            this.ActualDuration = fsAppointmentDetRow.IsService ? fsAppointmentDetRow.ActualDuration : 0;
            this.ActualQty = fsAppointmentDetRow.Qty;
            this.ActualAmount = fsAppointmentDetRow.CuryTranAmt;
            this.BillableQty = fsAppointmentDetRow.BillableQty;
            this.BillableAmount = fsAppointmentDetRow.CuryBillableTranAmt;
            this.UnitCost = fsAppointmentDetRow.CuryUnitCost;
            this.CostTotal = this.BillableQty * this.UnitCost;
            this.Profit = Math.Round((decimal)this.BillableAmount, 2) - Math.Round((decimal)this.CostTotal, 2);
            this.ProfitPercent = this.CostTotal == 0.0m ? 0.0m : (this.Profit / this.CostTotal) * 100;
        }

        public FSProfitability(FSLog fsLogRow)
        {
            this.LineRef = fsLogRow.LineRef;
            this.LineType = ID.LineType_Profitability.LABOR_ITEM;
            this.ItemID = fsLogRow.LaborItemID;
            this.EmployeeID = fsLogRow.BAccountID;
            this.ActualDuration = fsLogRow.TimeDuration;
            this.UnitCost = fsLogRow.CuryUnitCost;
            this.ActualQty = ((decimal)fsLogRow.TimeDuration) / 60m;
            this.ActualAmount = this.ActualQty * this.UnitCost;
            this.CostTotal = fsLogRow.CuryExtCost;
            this.BillableQty = 0m;
            this.BillableAmount = 0m;

            if (fsLogRow.IsBillable == true)
            {
                this.BillableQty = ((decimal)fsLogRow.BillableTimeDuration) / 60m;
                this.BillableAmount = this.BillableQty * this.UnitCost;
            }

            this.Profit = Math.Round((decimal)this.BillableAmount, 2) - Math.Round((decimal)this.CostTotal, 2);
            this.ProfitPercent = this.CostTotal == 0.0m ? 0.0m : (this.Profit / this.CostTotal) * 100;
        }

        #endregion
    }
}
