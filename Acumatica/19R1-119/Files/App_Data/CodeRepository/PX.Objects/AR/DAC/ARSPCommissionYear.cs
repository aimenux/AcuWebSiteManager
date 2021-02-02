namespace PX.Objects.AR
{
	using System;
	using PX.Data;

	/// <summary>
	/// Represents a year in which salesperson commissions are
	/// calculated and to which commission periods (<see 
	/// cref="ARSPCommissionPeriod"/>) belong. The records of 
	/// this type are created during the Calculate Commissions 
	/// (AR505500) process, which corresponds to the <see 
	/// cref="ARSPCommissionProcess"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARSPCommissionYear)]
	public partial class ARSPCommissionYear : PX.Data.IBqlTable
	{
		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		protected String _Year;
		[PXDBString(4, IsKey=true, IsFixed = true)]
		[PXDefault()]
		public virtual String Year
		{
			get
			{
				return this._Year;
			}
			set
			{
				this._Year = value;
			}
		}
		#endregion
		#region Filed
		public abstract class filed : PX.Data.BQL.BqlBool.Field<filed> { }
		protected Boolean? _Filed;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Filed
		{
			get
			{
				return this._Filed;
			}
			set
			{
				this._Filed = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
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
}