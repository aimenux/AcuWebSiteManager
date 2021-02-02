using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CR;

namespace PX.Objects.RQ
{	
	[PXProjection(typeof(Select<RQRequisitionLine>), Persistent=false)]
    [Serializable]
	public partial class RQRequisitionLineBidding : IBqlTable
	{
		#region ReqNbr
		public abstract class reqNbr : PX.Data.BQL.BqlString.Field<reqNbr> { }
		protected String _ReqNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(RQRequisitionLine.reqNbr))]						
		public virtual String ReqNbr
		{
			get
			{
				return this._ReqNbr;
			}
			set
			{
				this._ReqNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected int? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(RQRequisitionLine.lineNbr))]				
		public virtual int? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion			
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[RQRequisitionInventoryItem(Filterable = true, BqlField = typeof(RQRequisitionLine.inventoryID), Enabled=false)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(BqlField = typeof(RQRequisitionLine.subItemID), Enabled = false)]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion		
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(RQRequisitionLine.description))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]		
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion		
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXDBString(BqlField = typeof(RQRequisitionLine.lineNbr), InputMask = "", IsUnicode = true)]
		[PXUIField(DisplayName = "Alternate ID", Enabled = false)]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(RQRequisitionLine.curyInfoID))]
		[PXDBDefault(typeof(RQRequisition.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion

		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(RQRequisitionLineBidding.inventoryID), DisplayName = "UOM", BqlField = typeof(RQRequisitionLine.uOM), Enabled = false)]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBQuantity(typeof(RQRequisitionLineBidding.uOM), typeof(RQRequisitionLineBidding.baseOrderQty), HandleEmptyKey = true, BqlField = typeof(RQRequisitionLine.orderQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Qty.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region BaseOrderQty
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
		protected Decimal? _BaseOrderQty;
		[PXDBDecimal(6, BqlField = typeof(RQRequisitionLine.baseOrderQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseOrderQty
		{
			get
			{
				return this._BaseOrderQty;
			}
			set
			{
				this._BaseOrderQty = value;
			}
		}
		#endregion
		#region QuoteNumber
		public abstract class quoteNumber : PX.Data.BQL.BqlString.Field<quoteNumber> { }
		protected String _QuoteNumber;
		[PXString(20, IsUnicode = true)]
		[PXUIField(DisplayName = "Bid Number")]
		public virtual String QuoteNumber
		{
			get
			{
				return this._QuoteNumber;
			}
			set
			{
				this._QuoteNumber = value;
			}
		}
		#endregion
		#region QuoteQty
		public abstract class quoteQty : PX.Data.BQL.BqlDecimal.Field<quoteQty> { }
		protected Decimal? _QuoteQty;
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Bid Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? QuoteQty
		{
			get
			{
				return this._QuoteQty;
			}
			set
			{
				this._QuoteQty = value;
			}
		}
		#endregion		
		#region CuryQuoteUnitCost
		public abstract class curyQuoteUnitCost : PX.Data.BQL.BqlDecimal.Field<curyQuoteUnitCost> { }
		protected Decimal? _CuryQuoteUnitCost;

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(RQRequisitionLineBidding.curyInfoID), typeof(RQRequisitionLineBidding.quoteUnitCost))]
		[PXUIField(DisplayName = "Bid Unit Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryQuoteUnitCost
		{
			get
			{
				return this._CuryQuoteUnitCost;
			}
			set
			{
				this._CuryQuoteUnitCost = value;
			}
		}
		#endregion
		#region QuoteUnitCost
		public abstract class quoteUnitCost : PX.Data.BQL.BqlDecimal.Field<quoteUnitCost> { }
		protected Decimal? _QuoteUnitCost;
		[PXPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QuoteUnitCost
		{
			get
			{
				return this._QuoteUnitCost;
			}
			set
			{
				this._QuoteUnitCost = value;
			}
		}
		#endregion
		#region CuryQuoteExtCost
		public abstract class curyQuoteExtCost : PX.Data.BQL.BqlDecimal.Field<curyQuoteExtCost> { }
		protected Decimal? _CuryQuoteExtCost;
		[PXCurrency(typeof(RQRequisitionLineBidding.curyInfoID), typeof(RQRequisitionLineBidding.quoteExtCost))]
		[PXUIField(DisplayName = "Bid Extended Cost", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Mult<RQRequisitionLineBidding.quoteQty, RQRequisitionLineBidding.curyQuoteUnitCost>))]		
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryQuoteExtCost
		{
			get
			{
				return this._CuryQuoteExtCost;
			}
			set
			{
				this._CuryQuoteExtCost = value;
			}
		}
		#endregion
		#region QuoteExtCost
		public abstract class quoteExtCost : PX.Data.BQL.BqlDecimal.Field<quoteExtCost> { }
		protected Decimal? _QuoteExtCost;

		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QuoteExtCost
		{
			get
			{
				return this._QuoteExtCost;
			}
			set
			{
				this._QuoteExtCost = value;
			}
		}
		#endregion
		#region MinQty
		public abstract class minQty : PX.Data.BQL.BqlDecimal.Field<minQty> { }
		protected Decimal? _MinQty;
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Min. Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? MinQty
		{
			get
			{
				return this._MinQty;
			}
			set
			{
				this._MinQty = value;
			}
		}
		#endregion
	}
}
