namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLAllocationAccountHistory)]
	public partial class GLAllocationAccountHistory : PX.Data.IBqlTable
	{
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(Batch))]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(Batch))]
		[PXParent(typeof(Select<Batch, Where<Batch.batchNbr, Equal<Current<GLAllocationAccountHistory.batchNbr>>, And<Batch.module, Equal<Current<GLAllocationAccountHistory.module>>>>>))]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;
        [Branch(IsKey = true)]
        public virtual Int32? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region AllocatedAmount
		public abstract class allocatedAmount : PX.Data.BQL.BqlDecimal.Field<allocatedAmount> { }
		protected Decimal? _AllocatedAmount;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? AllocatedAmount
		{
			get
			{
				return this._AllocatedAmount;
			}
			set
			{
				this._AllocatedAmount = value;
			}
		}
		#endregion
		#region PriorPeriodsAllocAmount
		public abstract class priorPeriodsAllocAmount : PX.Data.BQL.BqlDecimal.Field<priorPeriodsAllocAmount> { }
		protected Decimal? _PriorPeriodsAllocAmount;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? PriorPeriodsAllocAmount
		{
			get
			{
				return this._PriorPeriodsAllocAmount;
			}
			set
			{
				this._PriorPeriodsAllocAmount = value;
			}
		}
		#endregion
		#region ContrAccontID
		public abstract class contrAccontID : PX.Data.BQL.BqlInt.Field<contrAccontID> { }
		protected Int32? _ContrAccontID;
		[PXDBInt()]
		public virtual Int32? ContrAccontID
		{
			get
			{
				return this._ContrAccontID;
			}
			set
			{
				this._ContrAccontID = value;
			}
		}
		#endregion
		#region ContrSubID
		public abstract class contrSubID : PX.Data.BQL.BqlInt.Field<contrSubID> { }
		protected Int32? _ContrSubID;
		[PXDBInt()]
		public virtual Int32? ContrSubID
		{
			get
			{
				return this._ContrSubID;
			}
			set
			{
				this._ContrSubID = value;
			}
		}
		#endregion
		#region SourceLedgerID
		public abstract class sourceLedgerID : PX.Data.BQL.BqlInt.Field<sourceLedgerID> { }
		protected Int32? _SourceLedgerID;
		[PXDBInt()]
		public virtual Int32? SourceLedgerID
		{
			get
			{
				return this._SourceLedgerID;
			}
			set
			{
				this._SourceLedgerID = value;
			}
		}
		#endregion
	}
}
