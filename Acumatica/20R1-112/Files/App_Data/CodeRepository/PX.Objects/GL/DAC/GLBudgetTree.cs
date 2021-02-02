using System;
using PX.Data;﻿

namespace PX.Objects.GL
 {
	/// <summary>
	/// Represents a node in the template budget structure. The structure defines the tree of budget articles and is used
	/// when articles are preloaded into a new budget (see <see cref="GLBudgetLine"/>).
	/// Records of this type don't maintain any budget amounts and are used only to define the structure of a budget,
	/// which, however, can be altered in any budget for a particular ledger, branch and year.
	/// 
	/// The class is used for both group (see <see cref="IsGroup"/>) and leaf nodes.
	/// To maintain tree structure, the class stores a link to the parent node in the<see cref="ParentGroupID"/> field.
	/// 
	/// Records of this type are created on the Budget Configuration (GL205000) form (see the <see cref="GLBudgetTreeMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
	 [PXPrimaryGraph(typeof(GLBudgetTreeMaint))]
	[PXCacheName(Messages.GLBudgetTree)]
	public partial class GLBudgetTree : PX.Data.IBqlTable, PX.SM.IIncludable
	 {
		 #region GroupID
		 public abstract class groupID : PX.Data.BQL.BqlGuid.Field<groupID> { }
		 protected Guid? _GroupID;

		/// <summary>
		/// The unique identifier of the budget tree node.
		/// </summary>
		 [PXDBGuid(IsKey=true)]
		 [PXDefault()]
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
		/// The identifier of the parent budget tree node.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="GroupID"/> field.
		/// The value is equal to <see cref="Guid.Empty"/> for the nodes on the first level of the tree.
		/// </value>
		 [PXDBGuid()]
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
		 #region SortOrder
		 public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		 protected int? _SortOrder;

		/// <summary>
		/// An integer field that defines the order in which budget tree nodes that belong to one <see cref="ParentGroupID">parent</see>
		/// appear relative to each other.
		/// The value of this field is changed by users when they move budget tree nodes by using the corresponding actions on the Budget Configuration (GL205000) form.
		/// </summary>
		 [PXDBInt()]
		 [PXDefault(0)]
		 [PXUIField(DisplayName = "Sort Order", Visibility = PXUIVisibility.Invisible)]
		 public virtual int? SortOrder
		 {
			 get
			 {
				 return this._SortOrder;
			 }
			 set
			 {
				 this._SortOrder = value;
			 }
		 }
		 #endregion
		 #region IsGroup
		 public abstract class isGroup : PX.Data.BQL.BqlBool.Field<isGroup> { }
		 protected bool? _IsGroup;

		/// <summary>
		/// The identifier of the GL <see cref="Account">account</see> of the budget tree node.
		/// </summary>
		/// <value>
		/// The value of the field can be empty for the group nodes(see<see cref="IsGroup"/>).
		/// </value>
		 [PXDBBool()]
		 [PXUIField(DisplayName = "Node")]
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
		 #region Rollup
		 public abstract class rollup : PX.Data.BQL.BqlBool.Field<rollup> { }
		 protected bool? _Rollup;
		 [PXDBBool()]
		 [PXUIField(DisplayName = "Rollup", Enabled = false)]
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
		 #region AccountID
		 public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		 protected Int32? _AccountID;

		/// <summary>
		/// Identifier of the GL <see cref="Account"/> of the budget tree node.
		/// May be empty for the group (see <see cref="IsGroup"/>) nodes.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		 [PXDBInt()]
         [PXDimensionSelector(AccountAttribute.DimensionName, (typeof(Search<Account.accountID, Where<Account.accountCD, Like<Current<SelectedNode.accountMaskWildcard>>>, OrderBy<Asc<Account.accountCD>>>)), typeof(Account.accountCD), DescriptionField = typeof(Account.description))]
         [PXRestrictor(typeof(Where<Account.active, Equal<True>>), Messages.AccountInactive)]
		 [PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
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
		 #region Description
		 public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		 protected string _Description;

		/// <summary>
		/// The description of the budget tree node.
		/// </summary>
		/// <value>
		/// Defaults to the description of the <see cref="AccountID">account</see>, but can be overwritten by a user.
		/// </value>
		[PXDBString(150, IsUnicode = true)]
		 [PXUIField(DisplayName = "Description", Required = true)]
		 [PXDefault(typeof(Search<Account.description, Where<Account.accountID, Equal<Current<GLBudgetTree.accountID>>>>))]
		 public virtual string Description
		 {
			 get
			 {
				 return this._Description;
			 }
			 set
			 {
				 this._Description = value;
			 }
		 }
		 #endregion
		 #region SubID
		 public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		 protected int? _SubID;

		/// <summary>
		/// The identifier of the GL <see cref="Sub">subaccount</see> of the budget tree node.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// The value of the field can be empty for the group (see <see cref="IsGroup"/>) nodes.
		/// </value>
		 [SubAccount]
		 public virtual int? SubID
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
		 #region AccountMask
		 public abstract class accountMask : PX.Data.BQL.BqlString.Field<accountMask> { }
		 protected string _AccountMask;

		/// <summary>
		/// For a group node (see <see cref="IsGroup"/>), defines the mask for selection of the child budget tree nodes by their accounts.
		/// The selector on the <see cref="AccountID"/> field of the child nodes allows only accounts,
		/// whose <see cref="Account.AccountCD"/> matches the specified mask.
		/// </summary>
		 [PXDBString(10, IsUnicode = true)]
		 [PXUIField(DisplayName = "Account Mask", Required = false)]
		 [PXDefault(typeof(Search<Account.accountCD, Where<Account.accountID, Equal<Current<GLBudgetTree.accountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		 public virtual string AccountMask
		 {
			 get
			 {
				 return this._AccountMask;
			 }
			 set
			 {
				 this._AccountMask = value;
			 }
		 }
		 #endregion
		 #region SubMask
		 public abstract class subMask : PX.Data.BQL.BqlString.Field<subMask> { }
		 protected string _SubMask;

		/// <summary>
		/// For a group node (see <see cref="IsGroup"/>), defines the mask for selection of the child budget tree nodes by their subaccounts.
		/// The selector on the <see cref="SubID"/> field of the child nodes allows only subaccounts,
		/// whose <see cref="Sub.SubCD"/> matches the specified mask.
		/// </summary>
		 [PXDBString(30, IsUnicode = true)]
		 [PXUIField(DisplayName = "Subaccount Mask", Required = false)]
		 public virtual string SubMask
		 {
			 get
			 {
				 return this._SubMask;
			 }
			 set
			 {
				 this._SubMask = value;
			 }
		 }
		 #endregion
		 #region Secured
		 public abstract class secured : PX.Data.BQL.BqlBool.Field<secured> { }
		 protected bool? _Secured;

		/// <summary>
		/// An unbound field that indicates (if set to <c>true</c>) that access to the budget tree node is restricted, because the node is included into a restriction group or groups.
		/// </summary>
		/// <value>
		/// This field is populated only in the context of the Budget Configuration (GL205000) form (<see cref="GLBudgetTreeMaint"/>).
		/// </value>
		 [PXBool]
		 [PXUIField(DisplayName = "Secured", Enabled=false)]
		 [PXUnboundDefault(false)]
		 public virtual bool? Secured
		 {
			 get
			 {
				 return this._Secured;
			 }
			 set
			 {
				 this._Secured = value;
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
		 protected string _CreatedByScreenID;
		 [PXDBCreatedByScreenID()]
		 public virtual string CreatedByScreenID
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
		 protected string _LastModifiedByScreenID;
		 [PXDBLastModifiedByScreenID()]
		 public virtual string LastModifiedByScreenID
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
		 #region TStamp
		 public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		 protected byte[] _TStamp;
		 [PXDBTimestamp()]
		 public virtual byte[] TStamp
		 {
			 get
			 {
				 return this._TStamp;
			 }
			 set
			 {
				 this._TStamp = value;
			 }
		 }
		 #endregion
		 #region GroupMask
		 public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }

		/// <summary>
		/// The group mask showing which <see cref="PX.SM.RelationGroup">restriction groups</see> the budget tree node belongs to.
		/// To learn more about the way restriction groups are managed, see the documentation for the GL Account Access (GL104000) form
		/// (which corresponds to the <see cref="GLAccess"/> graph).
		/// </summary>
		 [PXDBGroupMask]
		 public virtual byte[] GroupMask { get; set; }
		 #endregion
		 #region Included
		 public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		 protected bool? _Included;

		/// <summary>
		/// An unbound field that is used in the user interface to include the budget tree node into a <see cref="PX.SM.RelationGroup">restriction group</see>.
		/// Also see <see cref="GLBudgetTree.GroupMask"/>.
		/// </summary>
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
	 }
 }
