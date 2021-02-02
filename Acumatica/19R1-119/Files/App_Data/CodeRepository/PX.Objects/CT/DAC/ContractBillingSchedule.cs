using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.FA;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CT
{
	[PXPrimaryGraph(typeof(ContractMaint))]
	[Serializable]
	[PXCacheName(Messages.ContractBillingSchedule)]
	public partial class ContractBillingSchedule : IBqlTable
	{
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		protected Int32? _ContractID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(Contract.contractID))]
		[PXParent(typeof(Select<Contract, Where<Contract.contractID, Equal<Current<ContractBillingSchedule.contractID>>>>))]
		public virtual Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected String _Type;
		[PXDefault(BillingType.Monthly)]
		[PXDBString(1, IsFixed = true)]
		[BillingType.ListForContract]
		[PXUIField(DisplayName = "Billing Period", Required=true)]
		public virtual String Type
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
		#region NextDate
		public abstract class nextDate : PX.Data.BQL.BqlDateTime.Field<nextDate> { }
		protected DateTime? _NextDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Next Billing Date", Visibility=PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual DateTime? NextDate
		{
			get
			{
				return this._NextDate;
			}
			set
			{
				this._NextDate = value;
			}
		}
		#endregion
		#region LastDate
		public abstract class lastDate : PX.Data.BQL.BqlDateTime.Field<lastDate> { }
		protected DateTime? _LastDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Billing Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual DateTime? LastDate
		{
			get
			{
				return this._LastDate;
			}
			set
			{
				this._LastDate = value;
			}
		}
		#endregion
		#region BillTo
		public abstract class billTo : PX.Data.BQL.BqlString.Field<billTo>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() 
					: base(
						new string[] { ParentAccount, CustomerAccount, SpecificAccount },
						new string[] { Messages.ParentAccount, Messages.CustomerAccount, Messages.SpecificAccount }) {}
			}

			public const string ParentAccount = "P";
			public const string CustomerAccount = "M";
			public const string SpecificAccount = "S";

			public class parentAccount : PX.Data.BQL.BqlString.Constant<parentAccount>
			{
				public parentAccount() : base(billTo.ParentAccount) { ;}
			}
			public class customerAccount : PX.Data.BQL.BqlString.Constant<customerAccount>
			{
				public customerAccount() : base(billTo.CustomerAccount) { ;}
			}
			public class specificAccount : PX.Data.BQL.BqlString.Constant<specificAccount>
			{
				public specificAccount() : base(billTo.SpecificAccount) { ;}
			}
		}
		protected String _BillTo;
		[PXDefault(billTo.CustomerAccount, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(1, IsFixed = true)]
		[billTo.List]
		[PXUIField(DisplayName = "Bill To")]
		public virtual String BillTo
		{
			get
			{
				return this._BillTo;
			}
			set
			{
				this._BillTo = value;
			}
		}
		#endregion
		#region StartBilling
		public abstract class startBilling : PX.Data.BQL.BqlDateTime.Field<startBilling> { }
		protected DateTime? _StartBilling;
		[PXDBDate]
		[PXUIField(DisplayName = "Billing Schedule Starts On", Enabled=false)]
		public virtual DateTime? StartBilling
		{
			get
			{
				return this._StartBilling;
			}
			set
			{
				this._StartBilling = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[CustomerActive(DescriptionField = typeof(Customer.acctName), Visibility=PXUIVisibility.SelectorVisible, DisplayName="Account")]
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
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[PXDefault(typeof(Search<BAccount.defLocationID, Where<BAccount.bAccountID, Equal<Current<ContractBillingSchedule.accountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[CS.LocationID(typeof(Where<Location.bAccountID, Equal<Current<ContractBillingSchedule.accountID>>>), DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<ContractBillingSchedule.accountID>.IsRelatedTo<Location.bAccountID>,
				Field<ContractBillingSchedule.locationID>.IsRelatedTo<Location.locationID>
			>))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region InvoiceFormula
		public abstract class invoiceFormula : PX.Data.BQL.BqlString.Field<invoiceFormula> { }
		[PXDBString(4000, IsUnicode = true)]
		[PXDefault("=@ActionInvoice+' '+[Contract.ContractCD]+': '+[Contract.Description]+'.'")]
		[PXUIField(DisplayName = "Invoice Description")]
		public virtual string InvoiceFormula
		{
			get;
			set;
		}
		#endregion
		#region TranFormula
		public abstract class tranFormula : PX.Data.BQL.BqlString.Field<tranFormula> { }
		[PXDBString(4000, IsUnicode = true)]
		[PXDefault("=IIf( @Prefix=Null, '', @Prefix+': ')+ IIf( @ActionItem=Null,'',@ActionItem+': ')+[UsageData.Description]")]
		[PXUIField(DisplayName = "Line Description")]
		public virtual string TranFormula
		{
			get;
			set;
		}
		#endregion

		#region System Columns
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}

	public static class BillingType
	{


		public class ListForProjectAttribute : PXStringListAttribute
		{
			public ListForProjectAttribute()
				: base(
				new string[] { Weekly, Monthly, Quarterly, Annual, OnDemand },
				new string[] { Messages.Weekly, Messages.Monthly, Messages.Quarterly, Messages.Annual, Messages.OnDemand }) { ; }
		 }

		public class ListForContractAttribute : PXStringListAttribute
		{
			public ListForContractAttribute()
				: base(
				new string[] { Weekly, Monthly, Quarterly, SemiAnnual, Annual, OnDemand, Statement },
				new string[] { Messages.Weekly, Messages.Monthly, Messages.Quarterly, Messages.SemiAnnual, Messages.Annual, Messages.OnDemand, Messages.StatementBased }) { ; }
		}
		public const string Statement = "S";
		public const string Quarterly = "Q";
		public const string Monthly = "M";
		public const string Annual = "A";
		public const string SemiAnnual = "6";
		public const string Weekly = "W";
		public const string OnDemand = "D";

		public class BillingStatement : PX.Data.BQL.BqlString.Constant<BillingStatement>
		{
			public BillingStatement() : base(BillingType.Statement) { ;}
		}
		public class BillingQuarterly : PX.Data.BQL.BqlString.Constant<BillingQuarterly>
		{
			public BillingQuarterly() : base(BillingType.Quarterly) { ;}
		}
		public class BillingMonthly : PX.Data.BQL.BqlString.Constant<BillingMonthly>
		{
			public BillingMonthly() : base(BillingType.Monthly) { ;}
		}
		public class BillingAnnual : PX.Data.BQL.BqlString.Constant<BillingAnnual>
		{
			public BillingAnnual() : base(BillingType.Annual) { ;}
		}
		public class BillingSemiAnnual : PX.Data.BQL.BqlString.Constant<BillingSemiAnnual>
		{
			public BillingSemiAnnual() : base(BillingType.SemiAnnual) { ;}
		}
		public class BillingWeekly : PX.Data.BQL.BqlString.Constant<BillingWeekly>
		{
			public BillingWeekly() : base(BillingType.Weekly) { ;}
		}
		public class BillingOnDemand : PX.Data.BQL.BqlString.Constant<BillingOnDemand>
		{
			public BillingOnDemand() : base(BillingType.OnDemand) { ;}
		}
	}
}