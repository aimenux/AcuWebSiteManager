using PX.Data;
using PX.Objects.CM;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.PMForecastHistory)]
	[Serializable]
	public class PMForecastHistory : IBqlTable, IProjectFilter
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}

		/// <summary>
		/// Get or set Project TaskID
		/// </summary>
		public int? TaskID => ProjectTaskID;

		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion

		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}
		[PXDBInt(IsKey = true)]
		public virtual Int32? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		[PXDBInt(IsKey = true)]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		[PXDBInt(IsKey = true)]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID>
		{
		}

		[GL.FinPeriodID(IsKey = true)]
		public virtual String PeriodID
		{
			get;
			set;
		}
		#endregion
				
		#region ActualQty
		public abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty>
		{
		}
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Quantity", Enabled = false)]
		public virtual Decimal? ActualQty
		{
			get;
			set;
		}
		#endregion
		#region CuryActualAmount
		public abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount", Enabled = false)]
		public virtual Decimal? CuryActualAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualAmount
		public abstract class actualAmount : PX.Data.BQL.BqlDecimal.Field<actualAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount in Base Currency", Enabled = false)]
		public virtual Decimal? ActualAmount
		{
			get;
			set;
		}
		#endregion
		#region DraftChangeOrderQty
		public abstract class draftChangeOrderQty : PX.Data.BQL.BqlDecimal.Field<draftChangeOrderQty>
		{
		}
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Potential CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? DraftChangeOrderQty
		{
			get;
			set;
		}
		#endregion
		#region CuryDraftChangeOrderAmount
		public abstract class curyDraftChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyDraftChangeOrderAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Potential CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryDraftChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderQty
		public abstract class changeOrderQty : PX.Data.BQL.BqlDecimal.Field<changeOrderQty>
		{
		}
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? ChangeOrderQty
		{
			get;
			set;
		}
		#endregion
		#region CuryChangeOrderAmount
		public abstract class curyChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyChangeOrderAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryChangeOrderAmount
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}

	[PXBreakInheritance]
	[PMForecastHistoryAccum]
	[Serializable]
	[PXHidden]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMForecastHistoryAccum : PMForecastHistory
	{
		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}

		[PXDBInt(IsKey = true)]
		public override Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}

		[PXDBInt(IsKey = true)]
		public override Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion

		#region AccountGroupID
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}

		[PXDBInt(IsKey = true)]
		public override Int32? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		[PXDBInt(IsKey = true)]
		public override Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		[PXDBInt(IsKey = true)]
		public override Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region PeriodID
		public new abstract class periodID : PX.Data.BQL.BqlString.Field<periodID>
		{
		}

		[GL.FinPeriodID(IsKey = true)]
		public override String PeriodID
		{
			get;
			set;
		}
		#endregion
	}
}
