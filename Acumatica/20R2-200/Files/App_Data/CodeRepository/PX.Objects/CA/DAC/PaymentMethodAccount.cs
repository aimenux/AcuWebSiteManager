using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	[Serializable]
	[PXProjection(typeof(Select2<PaymentMethodAccount,
			InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>>),
		new Type[] { typeof(PaymentMethodAccount) })]
	[PXCacheName(Messages.PaymentMethodAccount)]
	public partial class PaymentMethodAccount : IBqlTable
	{
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(typeof(PaymentMethod.paymentMethodID))]
		[PXParent(typeof(Select<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PaymentMethodAccount.paymentMethodID>>>>))]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
		[PXUIField(DisplayName = "Payment Method")]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, IsKey = true, DescriptionField = typeof(CashAccount.descr))]
		[PXDefault]
        [PXRestrictor(typeof(Where<CashAccount.active, Equal<True>>), Messages.CashAccountInactive, typeof(CashAccount.cashAccountCD))]
        public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch(BqlField = typeof(CashAccount.branchID))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region UseForAP
		public abstract class useForAP : PX.Data.BQL.BqlBool.Field<useForAP> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use in AP")]
		public virtual bool? UseForAP
		{
			get;
			set;
		}
		#endregion
		#region APIsDefault
		public abstract class aPIsDefault : PX.Data.BQL.BqlBool.Field<aPIsDefault> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AP Default")]
		[Common.UniqueBool(typeof(PaymentMethodAccount.paymentMethodID), typeof(PaymentMethodAccount.branchID))]
		public virtual bool? APIsDefault
		{
			get;
			set;
		}
		#endregion
		#region APAutoNextNbr
		public abstract class aPAutoNextNbr : PX.Data.BQL.BqlBool.Field<aPAutoNextNbr> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AP - Suggest Next Number")]
		public virtual bool? APAutoNextNbr
		{
			get;
			set;
		}
		#endregion
		#region APLastRefNbr
		public abstract class aPLastRefNbr : PX.Data.BQL.BqlString.Field<aPLastRefNbr> { }

		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "AP Last Reference Number")]
		public virtual string APLastRefNbr
		{
			get;
			set;
		}
		#endregion
		#region APLastRefNbrIsNull
		protected bool? _APLastRefNbrIsNull = false;
		public virtual bool? APLastRefNbrIsNull
		{
			get
			{
				return this._APLastRefNbrIsNull;
			}
			set
			{
				this._APLastRefNbrIsNull = value;
			}
		}
		#endregion
		#region APBatchLastRefNbr
		public abstract class aPBatchLastRefNbr : PX.Data.BQL.BqlString.Field<aPBatchLastRefNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Last Reference Number")]
		public virtual string APBatchLastRefNbr
		{
			get;
			set;
		}
		#endregion
		#region UseForAR
		public abstract class useForAR : PX.Data.BQL.BqlBool.Field<useForAR> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use in AR")]
		public virtual bool? UseForAR
		{
			get;
			set;
		}
		#endregion
		#region ARIsDefault
		public abstract class aRIsDefault : PX.Data.BQL.BqlBool.Field<aRIsDefault> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AR Default")]
		[Common.UniqueBool(typeof(PaymentMethodAccount.paymentMethodID), typeof(PaymentMethodAccount.branchID))]
		public virtual bool? ARIsDefault
		{
			get;
			set;
		}
		#endregion
		#region ARIsDefaultForRefund
		public abstract class aRIsDefaultForRefund : PX.Data.BQL.BqlBool.Field<aRIsDefaultForRefund> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AR Default For Refund")]
		[Common.UniqueBool(typeof(PaymentMethodAccount.paymentMethodID), typeof(PaymentMethodAccount.branchID))]
		public virtual bool? ARIsDefaultForRefund
		{
			get;
			set;
		}
		#endregion
		#region ARAutoNextNbr
		public abstract class aRAutoNextNbr : PX.Data.BQL.BqlBool.Field<aRAutoNextNbr> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AR - Suggest Next Number")]
		public virtual bool? ARAutoNextNbr
		{
			get;
			set;
		}
		#endregion
		#region ARLastRefNbr
		public abstract class aRLastRefNbr : PX.Data.BQL.BqlString.Field<aRLastRefNbr> { }

		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "AR Last Reference Number")]
		public virtual string ARLastRefNbr
		{
			get;
			set;
		}
		#endregion
		#region APLastRefNbrIsNull
		protected bool? _ARLastRefNbrIsNull = false;
		public virtual bool? ARLastRefNbrIsNull
		{
			get
			{
				return this._ARLastRefNbrIsNull;
			}
			set
			{
				this._ARLastRefNbrIsNull = value;
			}
		}
		#endregion
	}


	public class PaymentMethodAccountUsage
	{
		public const string UseForAP = "P";
		public const string UseForAR = "R";

		public class useForAP : PX.Data.BQL.BqlString.Constant<useForAP>
		{
			public useForAP() : base(UseForAP) { }
		}

		public class useForAR : PX.Data.BQL.BqlString.Constant<useForAR>
		{
			public useForAR() : base(UseForAR) { }
		}
	}
}
