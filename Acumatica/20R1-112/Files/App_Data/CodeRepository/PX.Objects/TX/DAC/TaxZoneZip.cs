namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxZoneZip)]
	public partial class TaxZoneZip : PX.Data.IBqlTable
	{
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXParent(typeof(Select<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<TaxZoneZip.taxZoneID>>>>))]
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXDefault(typeof(TaxZone.taxZoneID))]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region ZipCode
		public abstract class zipCode : PX.Data.BQL.BqlString.Field<zipCode> { }
		protected String _ZipCode;
		[PXUIField(DisplayName="Zip Code")]
		[PXDBString(9, IsKey = true)]
		[PXDefault()]
		public virtual String ZipCode
		{
			get
			{
				return this._ZipCode;
			}
			set
			{
				this._ZipCode = value;
			}
		}
		#endregion
		#region ZipMin
		public abstract class zipMin : PX.Data.BQL.BqlInt.Field<zipMin> { }
		protected Int32? _ZipMin;
		[PXUIField(DisplayName = "Zip Code+4 (Min.)")]
		[PXDBInt(IsKey = true, MinValue=0, MaxValue=9999)]
		[PXDefault(1)]
		public virtual Int32? ZipMin
		{
			get
			{
				return this._ZipMin;
			}
			set
			{
				this._ZipMin = value;
			}
		}
		#endregion
		#region ZipMax
		public abstract class zipMax : PX.Data.BQL.BqlInt.Field<zipMax> { }
		protected Int32? _ZipMax;
		[PXUIField(DisplayName = "Zip Code+4 (Max.)")]
		[PXDBInt(MinValue = 0, MaxValue = 9999)]
		[PXDefault(9999)]
		public virtual Int32? ZipMax
		{
			get
			{
				return this._ZipMax;
			}
			set
			{
				this._ZipMax = value;
			}
		}
		#endregion
	}
}
