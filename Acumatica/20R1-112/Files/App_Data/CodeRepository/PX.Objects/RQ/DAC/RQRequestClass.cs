namespace PX.Objects.RQ
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.GL;	

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(RQRequestClassMaint))]
	[PXCacheName(Messages.RequestClass)]
	public partial class RQRequestClass : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<RQRequestClass>.By<reqClassID>
		{
			public static RQRequestClass Find(PXGraph graph, string reqClassID) => FindBy(graph, reqClassID);
		}
		#endregion

		#region ReqClassID
		public abstract class reqClassID : PX.Data.BQL.BqlString.Field<reqClassID> { }
		protected String _ReqClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(RQRequestClass.reqClassID), DescriptionField=typeof(RQRequestClass.descr))]
		[PX.Data.EP.PXFieldDescription]
		public virtual String ReqClassID
		{
			get
			{
				return this._ReqClassID;
			}
			set
			{
				this._ReqClassID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDefault]
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		
		#region VendorNotRequest
		public abstract class vendorNotRequest : PX.Data.BQL.BqlBool.Field<vendorNotRequest> { }
		protected Boolean? _VendorNotRequest;
		[PXDBBool()]
		[PXUIField(DisplayName = "Vendor Information is Not Required", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? VendorNotRequest
		{
			get
			{
				return this._VendorNotRequest;
			}
			set
			{
				this._VendorNotRequest = value;
			}
		}
		#endregion		
		#region VendorMultiply
		public abstract class vendorMultiply : PX.Data.BQL.BqlBool.Field<vendorMultiply> { }
		protected Boolean? _VendorMultiply;
		[PXDBBool()]
		[PXUIField(DisplayName = "Allow Multiple Vendors per Request", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? VendorMultiply
		{
			get
			{
				return this._VendorMultiply;
			}
			set
			{
				this._VendorMultiply = value;
			}
		}
		#endregion		
		#region RestrictItemList
		public abstract class restrictItemList : PX.Data.BQL.BqlBool.Field<restrictItemList> { }
		protected Boolean? _RestrictItemList;
		[PXDBBool()]
		[PXUIField(DisplayName = "Restrict Requested Items to the Specified List", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? RestrictItemList
		{
			get
			{
				return this._RestrictItemList;
			}
			set
			{
				this._RestrictItemList = value;
			}
		}
		#endregion		
		#region HideInventoryID
		public abstract class hideInventoryID : PX.Data.BQL.BqlBool.Field<hideInventoryID> { }
		protected Boolean? _HideInventoryID;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hide Inventory Item", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? HideInventoryID
		{
			get
			{
				return this._HideInventoryID;
			}
			set
			{
				this._HideInventoryID = value;
			}
		}
		#endregion		
		#region IssueRequestor
		public abstract class issueRequestor : PX.Data.BQL.BqlBool.Field<issueRequestor> { }
		protected Boolean? _IssueRequestor;
		[PXDBBool()]
		[PXUIField(DisplayName = "Issue to Requester", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? IssueRequestor
		{
			get
			{
				return this._IssueRequestor;
			}
			set
			{
				this._IssueRequestor = value;
			}
		}
		#endregion		
		#region CustomerRequest
		public abstract class customerRequest : PX.Data.BQL.BqlBool.Field<customerRequest> { }
		protected Boolean? _CustomerRequest;
		[PXDBBool()]
		[PXUIField(DisplayName = "Customer Request", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? CustomerRequest
		{
			get
			{
				return this._CustomerRequest;
			}
			set
			{
				this._CustomerRequest = value;
			}
		}
		#endregion		
		#region ExpenseAccountDefault
		public abstract class expenseAccountDefault : PX.Data.BQL.BqlString.Field<expenseAccountDefault> { }
		protected string _ExpenseAccountDefault;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(RQAccountSource.None)]
		[PXUIField(DisplayName = "Use Expense Account From")]
		[RQAccountSource.List]
		public virtual string ExpenseAccountDefault
		{
			get
			{
				return this._ExpenseAccountDefault;
			}
			set
			{
				this._ExpenseAccountDefault = value;
			}
		}
		#endregion		
		#region ExpenseSubMask
		public abstract class expenseSubMask : PX.Data.BQL.BqlString.Field<expenseSubMask> { }
		protected String _ExpenseSubMask;
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[SubAccountMask(DisplayName = "Combine Expense Sub. From")]
		public virtual String ExpenseSubMask
		{
			get
			{
				return this._ExpenseSubMask;
			}
			set
			{
				this._ExpenseSubMask = value;
			}
		}
		#endregion
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		protected Int32? _ExpenseAcctID;
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? ExpenseAcctID
		{
			get
			{
				return this._ExpenseAcctID;
			}
			set
			{
				this._ExpenseAcctID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		protected Int32? _ExpenseSubID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(RQRequestClass.expenseAcctID), DisplayName = "Expense Sub", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
			}
		}
		#endregion
		#region PromisedLeadTime
		public abstract class promisedLeadTime : PX.Data.BQL.BqlShort.Field<promisedLeadTime> { }
		protected Int16? _PromisedLeadTime;
		[PXDBShort(MinValue = 0, MaxValue = 3660)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Promised Lead Time (Days)")]
		public Int16? PromisedLeadTime
		{
			get
			{
				return this._PromisedLeadTime;
			}
			set
			{
				this._PromisedLeadTime = value;
			}
		}
		#endregion
		#region BudgetValidation
		public abstract class budgetValidation : PX.Data.BQL.BqlInt.Field<budgetValidation> { }
		protected int? _BudgetValidation;
		[PXDBInt()]
		[PXUIField(DisplayName = "Budget Validation", Visibility = PXUIVisibility.Visible)]
		[PXDefault(RQRequestClassBudget.None)]
		[RQRequestClassBudget.List]
		public virtual int? BudgetValidation
		{
			get
			{
				return this._BudgetValidation;
			}
			set
			{
				this._BudgetValidation = value;
			}
		}
		#endregion
		
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(RQRequestClass.reqClassID),
			Selector = typeof(Search<RQRequestClass.reqClassID>))]
		public virtual Guid? NoteID { get; set; }
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
	}

	public class RQRequestClassBudget
	{
		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(None, Messages.None),
					Pair(Warning, Messages.Warning),
					Pair(Error, Messages.Error),
				}) {}
		}

		public const int None = 0;
		public const int Warning = 1;
		public const int Error = 2;


		public class none : PX.Data.BQL.BqlInt.Constant<none>
		{
			public none() : base(None) { ;}
		}
		public class warning : PX.Data.BQL.BqlInt.Constant<warning>
		{
			public warning() : base(Warning) { ;}
		}
		public class error : PX.Data.BQL.BqlInt.Constant<error>
		{
			public error() : base(Error) { ;}
		}
	}
}