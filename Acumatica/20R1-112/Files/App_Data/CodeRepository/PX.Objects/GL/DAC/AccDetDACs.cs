using System;
using PX.Data;
using PX.Objects.CM;
using PX.Data.EP;

/// <summary>
/// Account Details Light DACs
/// </summary>
namespace PX.Objects.GL.ADL
{
	public partial class Batch : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;

		[Branch()]
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
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;

		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		[BatchModule.List()]
		[PXFieldDescription]
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

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
		[BatchModule.Numbering()]
		[PXFieldDescription]
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
		#region DateEntered
		public abstract class dateEntered : PX.Data.BQL.BqlDateTime.Field<dateEntered> { }
		protected DateTime? _DateEntered;

		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? DateEntered
		{
			get
			{
				return this._DateEntered;
			}
			set
			{
				this._DateEntered = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;

		[OpenPeriod(typeof(ADL.Batch.dateEntered))]
		[PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
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
		#region BatchType
		public abstract class batchType : PX.Data.BQL.BqlString.Field<batchType> { }
		protected String _BatchType;

		[PXDBString(3)]
		[PXDefault(BatchTypeCode.Normal)]
		[PXUIField(DisplayName = "Type")]
		[BatchTypeCode.List()]
		public virtual String BatchType
		{
			get
			{
				return this._BatchType;
			}
			set
			{
				this._BatchType = value;
			}
		}
		#endregion
		#region Scheduled
		public abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		protected Boolean? _Scheduled;

		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Scheduled
		{
			get
			{
				return this._Scheduled;
			}
			set
			{
				this._Scheduled = value;
			}
		}
		#endregion
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		protected Boolean? _Voided;

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Voided")]
		public virtual Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
	}

	public partial class Account : PX.Data.IBqlTable, PX.SM.IIncludable
	{
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;

		[PXDBIdentity]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
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
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected string _Type;

		[PXDBString(1, IsFixed = true)]
		[AccountType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected string _CuryID;

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;

		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion
		#region TransactionsForGivenCurrencyExists
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public abstract class transactionsForGivenCurrencyExists : PX.Data.BQL.BqlBool.Field<transactionsForGivenCurrencyExists> { }

		[PXBool]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public virtual bool? TransactionsForGivenCurrencyExists { get; set; }
		#endregion
		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		protected bool? _Included;

		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBool]
		[PXUIField(DisplayName = "Included")]
		public virtual bool? Included
		{
			get
			{
				return this._Included;
			}
			set
			{
				this._Included = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TX.TaxCategory.taxCategoryID), DescriptionField = typeof(TX.TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TX.TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TX.TaxCategory.taxCategoryID))]
		public virtual String TaxCategoryID { get; set; }
		#endregion

		public const string Default = "0";
	}

	public partial class Sub : PX.Data.IBqlTable
	{
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;

		[PXDBIdentity()]
		[PXUIField(DisplayName = "Sub. ID", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
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
		#region SubCD
		public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }
		protected String _SubCD;

		[PXDefault()]
		[SubAccountRaw(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String SubCD
		{
			get
			{
				return this._SubCD;
			}
			set
			{
				this._SubCD = value;
			}
		}
		#endregion
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;

		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion
	}
}
