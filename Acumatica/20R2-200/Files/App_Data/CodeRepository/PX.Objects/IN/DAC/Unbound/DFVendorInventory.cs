using System;
using PX.Data;

namespace PX.Objects.IN
{
	[Obsolete]
	[Serializable]
	[PXHidden]
	public partial class DFVendorInventory : IBqlTable
	{
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region AddLeadTimeDays
		public abstract class addLeadTimeDays : PX.Data.BQL.BqlShort.Field<addLeadTimeDays> { }
		protected Int16? _AddLeadTimeDays;
		[PXDefault((short)0)]
		[PXDBShort()]
		[PXUIField(DisplayName = "Add. Lead Time (Days)")]
		public virtual Int16? AddLeadTimeDays
		{
			get
			{
				return this._AddLeadTimeDays;
			}
			set
			{
				this._AddLeadTimeDays = value;
			}
		}
		#endregion
		#region VLeadTime
		public abstract class vLeadTime : PX.Data.BQL.BqlShort.Field<vLeadTime> { }
		protected Int16? _VLeadTime;
		[PXShort(MinValue = 0, MaxValue = 100000)]
		[PXUIField(DisplayName = "Vendor Lead Time (Days)", Enabled = false)]
		public virtual Int16? VLeadTime
		{
			get
			{
				return this._VLeadTime;
			}
			set
			{
				this._VLeadTime = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected Boolean? _Active;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region MinOrdFreq
		public abstract class minOrdFreq : PX.Data.BQL.BqlInt.Field<minOrdFreq> { }
		protected Int32? _MinOrdFreq;
		[PXDBInt()]
		[PXUIField(DisplayName = "Min. Order Freq.(Days)")]
		[PXDefault(0)]
		public virtual Int32? MinOrdFreq
		{
			get
			{
				return this._MinOrdFreq;
			}
			set
			{
				this._MinOrdFreq = value;
			}
		}
		#endregion
		#region MinOrdQty
		public abstract class minOrdQty : PX.Data.BQL.BqlDecimal.Field<minOrdQty> { }
		protected Decimal? _MinOrdQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Min. Order Qty.")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? MinOrdQty
		{
			get
			{
				return this._MinOrdQty;
			}
			set
			{
				this._MinOrdQty = value;
			}
		}
		#endregion
		#region MaxOrdQty
		public abstract class maxOrdQty : PX.Data.BQL.BqlDecimal.Field<maxOrdQty> { }
		protected Decimal? _MaxOrdQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Max Order Qty.")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? MaxOrdQty
		{
			get
			{
				return this._MaxOrdQty;
			}
			set
			{
				this._MaxOrdQty = value;
			}
		}
		#endregion
		#region LotSize
		public abstract class lotSize : PX.Data.BQL.BqlDecimal.Field<lotSize> { }
		protected Decimal? _LotSize;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Lot Size")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LotSize
		{
			get
			{
				return this._LotSize;
			}
			set
			{
				this._LotSize = value;
			}
		}
		#endregion
		#region ERQ
		public abstract class eRQ : PX.Data.BQL.BqlDecimal.Field<eRQ> { }
		protected Decimal? _ERQ;
		[PXDBQuantity]
		[PXUIField(DisplayName = "ERQ")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ERQ
		{
			get
			{
				return this._ERQ;
			}
			set
			{
				this._ERQ = value;
			}
		}
		#endregion
	}
}
