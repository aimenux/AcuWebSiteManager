using System;

using PX.Data;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents the fee rate of late payments as an annual percentage
	/// rate effective on the specified date. The records of this
	/// type are used to specify rate details for <see cref="ARFinCharge">
	/// overdue charge codes</see> that use percentage charging methods
	/// (see <see cref="ARFinCharge.ChargingMethod">). The records of this
	/// type are edited on the Overdue Charges (AR204500) form, which
	/// corresponds to the <see cref="ARFinChargesMaint"/> graph.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ARFinChargePercent)]
	public partial class ARFinChargePercent : PX.Data.IBqlTable
	{
		public const string DefaultStartDate = "01/01/1900";
		#region FinChargeID
		public abstract class finChargeID : PX.Data.BQL.BqlString.Field<finChargeID> { }
		protected String _FinChargeID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDBDefault(typeof(ARFinCharge.finChargeID))]
		[PXParent(typeof(Select<ARFinCharge, Where<ARFinCharge.finChargeID, Equal<Current<ARFinChargePercent.finChargeID>>>>))]

		public virtual String FinChargeID
		{
			get
			{
				return this._FinChargeID;
			}
			set
			{
				this._FinChargeID = value;
			}
		}
		#endregion
		#region FinChargePercent
		public abstract class finChargePercent : PX.Data.BQL.BqlDecimal.Field<finChargePercent> { }
		protected Decimal? _FinChargePercent;
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Percent Rate")]
		public virtual Decimal? FinChargePercent
		{
			get
			{
				return this._FinChargePercent;
			}
			set
			{
				this._FinChargePercent = value;
			}
		}
		#endregion
		#region BeginDate
		public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }
		protected DateTime? _BeginDate;
		[PXDBDate(IsKey = true)]
		[PXDefault(TypeCode.DateTime, DefaultStartDate)]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
		[PXCheckUnique(new Type[] { typeof(finChargeID) })]
		public virtual DateTime? BeginDate
		{
			get
			{
				return this._BeginDate;
			}
			set
			{
				this._BeginDate = value;
			}
		}
		#endregion
		#region PercentID
		public abstract class percentID : PX.Data.BQL.BqlInt.Field<percentID> { }
		protected Int32? _PercentID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? PercentID
		{
			get
			{
				return this._PercentID;
			}
			set
			{
				this._PercentID = value;
			}
		}
		#endregion
	}
}
