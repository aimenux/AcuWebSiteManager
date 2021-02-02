using System;
using PX.Data;

namespace PX.Objects.GL
{
	/// <summary>
	/// Represents a budget article. The class is used for both group (see <see cref="IsGroup"/>) and leaf articles.
	/// To maintain tree structure, the class stores a link to the parent budget article in the <see cref="ParentGroupID"/> field.
	/// 
	/// The budget article holds the amount allocated to a particular account-subaccount pair
	/// (or a group of accounts and subaccounts, such as <see cref="AccountMask"/>) for a particular year.
	/// Distribution of this amount between the periods of the year is stored in the corresponding <see cref="GLBudgetLineDetail"/> records.
	/// 
	/// Records of this type are created on the Budgets (GL302010) form (see the <see cref="GLBudgetEntry"/> graph)
	/// either manually or by the preload mechanism that creates GLBudgetLine from <see cref="GLBudgetTree"/> records."/>
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(GLBudgetEntry), Filter = typeof(BudgetFilter))]
	[PXCacheName(Messages.BudgetArticle)]
	public partial class GLBudgetLine : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region GroupID
		public abstract class groupID : PX.Data.BQL.BqlGuid.Field<groupID> { }
		protected Guid? _GroupID;

		/// <summary>
		/// The unique identifier of the budget article.
		/// This field is a part of the compound key.
		/// </summary>
		[PXDBGuid(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "GroupID", Visibility = PXUIVisibility.Invisible)]
		public virtual Guid? GroupID
		{
			get
			{
				return this._GroupID;
			}
			set
			{
				this._GroupID = value;
			}
		}
		#endregion
		#region ParentGroupID
		public abstract class parentGroupID : PX.Data.BQL.BqlGuid.Field<parentGroupID> { }
		protected Guid? _ParentGroupID;

		/// <summary>
		/// The identifier of the parent <see cref="GLBudgetLine">budget article</see>.
		/// This field is a part of the compound key.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="GLBudgetLine.GroupID"/> field of the parent.
		/// The value is equal to <see cref="Guid.Empty"/> for the nodes on the first level of the tree.
		/// </value>
		[PXDBGuid]
		[PXDefault]
		[PXUIField(DisplayName = "ParentGroupID", Visibility = PXUIVisibility.Invisible)]
		public virtual Guid? ParentGroupID
		{
			get
			{
				return this._ParentGroupID;
			}
			set
			{
				this._ParentGroupID = value;
			}
		}
		#endregion
		#region Rollup
		public abstract class rollup : PX.Data.BQL.BqlBool.Field<rollup> { }
		protected bool? _Rollup;
		[PXDBBool()]
		[PXUIField(DisplayName = "Rollup", Visible = false, Enabled=false, Visibility=PXUIVisibility.Invisible)]
		[PXDefault(false)]
		public virtual bool? Rollup
		{
			get
			{
				return this._Rollup;
			}
			set
			{
				this._Rollup = value;
			}
		}
		#endregion
		#region IsGroup
		public abstract class isGroup : PX.Data.BQL.BqlBool.Field<isGroup> { }
		protected bool? _IsGroup;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the budget article represents a group of other articles.
		/// That is, the article has child articles, which are linked to this article by their <see cref="ParentGroupID"/> field.
		/// </summary>
		[PXDBBool()]
        [PXUIField(DisplayName = "Node", Enabled = false)]
		[PXDefault(false)]
		public virtual bool? IsGroup
		{
			get
			{
				return this._IsGroup;
			}
			set
			{
				this._IsGroup = value;
			}
		}
		#endregion
		#region IsPreloaded
		public abstract class isPreloaded : PX.Data.BQL.BqlBool.Field<isPreloaded> { }
		protected bool? _IsPreloaded;

		/// Specifies (if set to <c>true</c>) that the budget article was created from a
		/// <see cref="GLBudgetTree">budget tree configuration node</see> by the budget tree
		/// preload process. (The preload process is invoked when a user creates a budget for a new year (see <see cref="GLBudgetEntry.BudgetFilter_RowUpdated"/>)
		/// or converts an existing budget (see <see cref="GLBudgetEntry.ConvertBudget"/>).)
		[PXDBBool()]
		[PXUIField(DisplayName = "Preloaded", Visible = false, Enabled = false)]
		[PXDefault(false)]
		public virtual bool? IsPreloaded
		{
			get
			{
				return this._IsPreloaded;
			}
			set
			{
				this._IsPreloaded = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// The identifier of the <see cref="Branch">branch</see> to which the budget article belongs.
		/// This field is a part of the compound key.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch(typeof(BudgetFilter.branchID), IsKey = true, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual int? BranchID { get; set; }
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }

		/// <summary>
		/// The identifier of the <see cref="Ledger">ledger</see> to which the budget article belongs.
		/// This field is a part of the compound key.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Ledger.LedgerID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(BudgetFilter.ledgerID))]
		[PXUIField(Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual int? LedgerID { get; set; }
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }

		/// <summary>
		/// The <see cref="FinYear">financial year</see> to which the budget article belongs.
		/// This field is a part of the compound key.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="FinYear.year"/> field.
		/// </value>
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXUIField(Visible = false, DisplayName = "Financial Year")]
		[PXDefault(typeof(BudgetFilter.finYear))]
		public virtual string FinYear { get; set; }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		/// <summary>
		/// The identifier of the GL <see cref="Account">account</see> of the budget article.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// The value of the field can be empty for the group articles (see <see cref="IsGroup"/>).
		/// </value>
		[PXDBInt]
		[PXDimensionSelector(AccountAttribute.DimensionName, 
			typeof(Search<Account.accountID, Where<Account.accountCD, Like<Current<SelectedGroup.accountMaskWildcard>>>, OrderBy<Asc<Account.accountCD>>>),
			typeof(Account.accountCD),
			DescriptionField=typeof(Account.description))]
		[PXRestrictor(typeof(Where<Account.active, Equal<True>>), Messages.AccountInactive)]
		[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Dynamic)]
		[PXDefault]
		public virtual int? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		/// <summary>
		/// The identifier of the GL <see cref="Sub">subaccount</see> of the budget article.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// Can be empty for the group articles (see <see cref="IsGroup"/>).
		/// </value>
		[SubAccount(Visibility = PXUIVisibility.Dynamic)]
		[PXRestrictor(typeof(Where<Sub.active, Equal<True>>), Messages.SubaccountInactive, typeof(Sub.subCD))]
		[PXDefault]
		public virtual int? SubID { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The description of the budget article.
		/// </summary>
		/// <value>
		/// Defaults to the description of the <see cref="AccountID">account</see>, but can be overwritten by user.
		/// </value>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		[PXDefault(typeof(Search<Account.description, Where<Account.accountID, Equal<Current<GLBudgetLine.accountID>>>>))]
		public virtual string Description { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

		/// <summary>
		/// The amount that is budgeted for the article for a particular <see cref="FinYear">year</see>.
		/// </summary>
		/// <value>
		/// The value can be edited by a user and indicates the decision to allocate the specified amount
		/// to the particular budget article.
		/// After the article is released, the amount can still be edited. If the amount is changed, the article will be marked as unreleased (see <see cref="Released"/>)
		/// and can be released again, which will result in updating the budget ledger figures to match the current state of the article.
		/// </value>
		[PXDBDecimal(typeof(Search2<CM.Currency.decimalPlaces,
			InnerJoin<Ledger, On<Ledger.baseCuryID, Equal<CM.Currency.curyID>>>,
			Where<Ledger.ledgerID, Equal<Current<GLBudgetLine.ledgerID>>>>))]
		[PXUIField(DisplayName = "Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? Amount { get; set; }
		#endregion
		#region AllocatedAmount
		public abstract class allocatedAmount : PX.Data.BQL.BqlDecimal.Field<allocatedAmount> { }
		protected Decimal? _AllocatedAmount;

		/// <summary>
		/// The total amount of the budget article distributed between the periods of the year.
		/// </summary>
		/// <value>
		/// The value of this field is calculated as a sum of amounts of the detail lines of the budget article (see <see cref="GLBudgetLineDetail.Amount"/>)
		/// and cannot be edited by a user.
		/// A budget article cannot be released until its <see cref="Amount"/> and <see cref="AllocatedAmount"/> are equal to each other.
		/// </value>
		[PXDBDecimal(typeof(Search2<CM.Currency.decimalPlaces,
			InnerJoin<Ledger, On<Ledger.baseCuryID, Equal<CM.Currency.curyID>>>,
			Where<Ledger.ledgerID, Equal<Current<GLBudgetLine.ledgerID>>>>))]
		[PXUIField(DisplayName = "Distributed Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AllocatedAmount
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
		#region ReleasedAmount
		public abstract class releasedAmount : PX.Data.BQL.BqlDecimal.Field<releasedAmount> { }
		protected Decimal? _ReleasedAmount;

		/// <summary>
		/// The currently released amount of the article, which matches the figures in the budget ledger.
		/// </summary>
		/// <value>
		/// This field is updated with the value of the <see cref="Amount"/> field upon release of the budget article.
		/// The difference between the values of this field and the <see cref="Amount"/> field shows the difference
		/// between the current state of the article and the corresponding figures in the budget ledger.
		/// </value>
		[PXDBDecimal(typeof(Search2<CM.Currency.decimalPlaces,
			InnerJoin<Ledger, On<Ledger.baseCuryID, Equal<CM.Currency.curyID>>>,
			Where<Ledger.ledgerID, Equal<Current<GLBudgetLine.ledgerID>>>>))]
		[PXUIField(DisplayName = "Released Amount", Enabled = false, Visible= false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ReleasedAmount
		{
			get
			{
				return this._ReleasedAmount;
			}
			set
			{
				this._ReleasedAmount = value;
			}
		}
		#endregion
		#region AccountMask
		public abstract class accountMask : PX.Data.BQL.BqlString.Field<accountMask> { }
		protected string _AccountMask;

		/// <summary>
		/// For a group article (see <see cref="IsGroup"/>), defines the mask for selection of the child budget articles by their accounts.
		/// The selector on the <see cref="AccountID"/> field of the child articles allows only accounts
		/// whose <see cref="Account.AccountCD"/> matches the specified mask.
		/// </summary>
		[PXUIField(DisplayName = "Account Mask", Enabled = false, Visible = false)]
		[PXDefault("", PersistingCheck=PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true)]
		public virtual string AccountMask { get; set; }
		#endregion
		#region SubMask
		public abstract class subMask : PX.Data.BQL.BqlString.Field<subMask> { }
		protected string _SubMask;

		/// <summary>
		/// For a group article (see <see cref="IsGroup"/>), defines the mask for selection of the child budget articles by their subaccounts.
		/// The selector on the <see cref="SubID"/> field of the child articles allows only subaccounts
		/// whose <see cref="Sub.SubCD"/> matches the specified mask.
		/// </summary>
		[PXUIField(DisplayName = "Subaccount Mask", Enabled = false, Visible = false)]
		[PXDefault("", PersistingCheck=PXPersistingCheck.Nothing)]
		[PXDBString(30, IsUnicode = true)]
		public virtual string SubMask { get; set; }
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the budget article has been released at least once.
		/// A released article can be released again if its <see cref="Amount"/> is not equal to its <see cref="ReleasedAmount"/>.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		public virtual bool? Released { get; set; }
		#endregion
		#region WasReleased
		public abstract class wasReleased : PX.Data.BQL.BqlBool.Field<wasReleased> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the article was released at least once.
		/// Different from <see cref="Released"/> in that the latter will be reset to <c>false</c> upon edits of
		/// a released article, while this field will still be <c>true</c> even if the article can be released again.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? WasReleased { get; set; }
		#endregion
		#region Comparison
		public abstract class comparison : PX.Data.BQL.BqlBool.Field<comparison> { }
		protected bool? _Comparison;
		[PXBool]
		public virtual bool? Comparison {
			get
			{
				return this._Comparison;
			}
			set
			{
				this._Comparison = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;

		/// <summary>
		/// The group mask showing which <see cref="PX.SM.RelationGroup">restriction groups</see> the budget article belongs to.
		/// The value of this field is inherited from the <see cref="GLBudgetTree.GroupMask">group mask</see> of the corresponding budget tree node,
		/// because access to budgets is managed through the budget tree configuration.
		/// </summary>
		[PXDBGroupMask]
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID(Visible = false)]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID(Visible = false)]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		public decimal[] Allocated;
		public decimal[] _Compared;
		public virtual decimal[] Compared
		{
			get
			{
				return this._Compared;
			}
			set
			{
				this._Compared = value;
			}
		}
		#region TreeSortOrder
		public abstract class treeSortOrder : PX.Data.BQL.BqlInt.Field<treeSortOrder> { }
		protected int? _TreeSortOrder;

		/// <summary>
		/// An integer field that defines the order in which group budget articles (see <see cref="IsGroup"/>) that belong to one <see cref="ParentGroupID">parent</see>
		/// appear relative to each other in the tree view.
		/// </summary>
		/// <value>
		/// The value of this field is automatically assigned during budget tree preload from
		/// <see cref="GLBudgetTree.SortOrder"/> of the corresponding tree node.
		/// </value>
		[PXDBInt()]
		[PXDefault(0)]
		[PXUIField(DisplayName = "TreeSortOrder", Visibility = PXUIVisibility.Invisible)]
		public virtual int? TreeSortOrder
		{
			get
			{
				return this._TreeSortOrder;
			}
			set
			{
				this._TreeSortOrder = value;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

		/// <summary>
		/// The internal field that defines the order in which non-group budget articles appear relative to each other.
		/// </summary>
		/// <value>
		/// This field is populated only in the <see cref="GLBudgetEntry"/> graph and is not stored in the database.
		/// </value>
		[PXInt]
		public virtual int? SortOrder { get; set; }
		#endregion
		#region IsUploaded
		public abstract class isUploaded : PX.Data.BQL.BqlBool.Field<isUploaded> { }
		public virtual bool? IsUploaded { get; set; }
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }

		/// <summary>
		/// The field is reserved for internal use.
		/// </summary>
		public virtual bool? Cleared { get; set; }
		#endregion
		#region IsRolledUp
		public abstract class isRolledUp : PX.Data.BQL.BqlBool.Field<isRolledUp> { }
		[PXBool]
		public virtual bool? IsRolledUp { get; set; }
		#endregion
	}
}
