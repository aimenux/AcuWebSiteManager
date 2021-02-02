namespace PX.Objects.EP
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
    [EPAccum]
    [PXHidden]
	public partial class EPHistory : PX.Data.IBqlTable
	{
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
			}
		}
		#endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        protected String _FinPeriodID;
        [PXDBString(6, IsKey = true, IsFixed = true)]
        [PXDefault()]
        public virtual String FinPeriodID
        {
            get
            {
                return this._FinPeriodID;
            }
            set
            {
                this._FinPeriodID = value;
            }
        }
        #endregion
		#region FinPtdClaimed
		public abstract class finPtdClaimed : PX.Data.BQL.BqlDecimal.Field<finPtdClaimed> { }
		protected Decimal? _FinPtdClaimed;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? FinPtdClaimed
		{
			get
			{
				return this._FinPtdClaimed;
			}
			set
			{
				this._FinPtdClaimed = value;
			}
		}
		#endregion
		#region TranPtdClaimed
		public abstract class tranPtdClaimed : PX.Data.BQL.BqlDecimal.Field<tranPtdClaimed> { }
		protected Decimal? _TranPtdClaimed;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? TranPtdClaimed
		{
			get
			{
				return this._TranPtdClaimed;
			}
			set
			{
				this._TranPtdClaimed = value;
			}
		}
		#endregion
		#region FinYtdClaimed
		public abstract class finYtdClaimed : PX.Data.BQL.BqlDecimal.Field<finYtdClaimed> { }
		protected Decimal? _FinYtdClaimed;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? FinYtdClaimed
		{
			get
			{
				return this._FinYtdClaimed;
			}
			set
			{
				this._FinYtdClaimed = value;
			}
		}
		#endregion
		#region TranYtdClaimed
		public abstract class tranYtdClaimed : PX.Data.BQL.BqlDecimal.Field<tranYtdClaimed> { }
		protected Decimal? _TranYtdClaimed;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? TranYtdClaimed
		{
			get
			{
				return this._TranYtdClaimed;
			}
			set
			{
				this._TranYtdClaimed = value;
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

    public class EPAccumAttribute : PXAccumulatorAttribute
    {
        public EPAccumAttribute()
            : base(new Type[] {
                typeof(EPHistory.finYtdClaimed),
                typeof(EPHistory.tranYtdClaimed)
            },
            new Type[] {
                typeof(EPHistory.finYtdClaimed),
                typeof(EPHistory.tranYtdClaimed)
            }
            )
        {
        }
        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }
            EPHistory hist = (EPHistory)row;
            columns.RestrictPast<EPHistory.finPeriodID>(PXComp.GE, hist.FinPeriodID.Substring(0, 4) + "01");
            columns.RestrictFuture<EPHistory.finPeriodID>(PXComp.LE, hist.FinPeriodID.Substring(0, 4) + "99");

            return true;
        }
    }
}
