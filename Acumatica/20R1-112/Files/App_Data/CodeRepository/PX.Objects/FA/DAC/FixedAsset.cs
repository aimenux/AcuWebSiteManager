using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.FA
{
	#region FARecordType Attribute
	public class FARecordType
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues => _AllowedValues;
			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels): base(AllowedValues, AllowedLabels) {}
		}

		public class MethodListAttribute : CustomListAttribute
		{
			public MethodListAttribute(): base(
				new string[] { ClassType, AssetType, BothType},
				new string[] { Messages.ClassType, Messages.AssetType, Messages.BothType}) {}
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute(): base(
				new string[] { ClassType, AssetType, ElementType},
				new string[] { Messages.ClassType, Messages.AssetType, Messages.ElementType}) {}
		}

		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute(): base(typeof(FixedAsset.recordType), typeof(FixedAsset.createdDateTime),
				new string[] { ClassType, AssetType, ElementType },
				new Type[] { null, typeof(Search<FASetup.assetNumberingID>), typeof(Search<FASetup.assetNumberingID>) })
			{
				NullMode = NullNumberingMode.UserNumbering;
			}
		}

		public const string ClassType = "C";
		public const string AssetType = "A";
		public const string ElementType = "E";
		public const string BothType = "B";

		public class classType : PX.Data.BQL.BqlString.Constant<classType>
		{
			public classType() : base(ClassType) {}
		}

		public class assetType : PX.Data.BQL.BqlString.Constant<assetType>
		{
			public assetType() : base(AssetType) {}
		}

		public class elementType : PX.Data.BQL.BqlString.Constant<elementType>
		{
			public elementType() : base(ElementType) {}
		}

		public class bothType : PX.Data.BQL.BqlString.Constant<bothType>
		{
			public bothType() : base(BothType) {}
		}
	}
   #endregion

   #region FixedAssetStatus Attribute
   public class FixedAssetStatus
   {
      /// <summary>
      /// The list of the fixed asset statuses.
      /// </summary>
      /// <value>
      /// The allowed values are:
      /// <list type="bullet">
      /// <item> <term><c>H</c></term> <description>On hold</description> </item>
      /// <item> <term><c>A</c></term> <description>Active</description> </item>
      /// <item> <term><c>S</c></term> <description>Suspended</description> </item>
      /// <item> <term><c>F</c></term> <description>Fully depreciated</description> </item>
      /// <item> <term><c>D</c></term> <description>Disposed</description> </item>
      /// <item> <term><c>R</c></term> <description>Reversed</description> </item>
      ///</list>
      /// </value>
      public class ListAttribute : PXStringListAttribute
      {
         public ListAttribute() : base(
            new string[] { Active, Hold, Suspended, FullyDepreciated, Disposed, Reversed },
            new string[] { Messages.Active, Messages.Hold, Messages.Suspended, Messages.FullyDepreciated, Messages.Disposed, Messages.Reversed })
         { }
      }

      public const string Active = "A";
      public const string Hold = "H";
      public const string Suspended = "S";
      public const string FullyDepreciated = "F";
      public const string Disposed = "D";
      [Obsolete(PX.Objects.Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public const string UnderConstruction = "C";
      [Obsolete(PX.Objects.Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
      public const string Dekitting = "K";
		public const string Reversed = "R";

		public class active : PX.Data.BQL.BqlString.Constant<active>
		{
			public active() : base(Active) {}
		}

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) {}
		}

		public class suspended : PX.Data.BQL.BqlString.Constant<suspended>
		{
			public suspended() : base(Suspended) {}
		}

		public class fullyDepreciated : PX.Data.BQL.BqlString.Constant<fullyDepreciated>
		{
			public fullyDepreciated() : base(FullyDepreciated) {}
		}

		public class disposed : PX.Data.BQL.BqlString.Constant<disposed>
		{
			public disposed() : base(Disposed) {}
		}

      [Obsolete(PX.Objects.Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
      public class underConstruction : PX.Data.BQL.BqlString.Constant<underConstruction>
		{
         public underConstruction() : base(UnderConstruction) { }
      }

      [Obsolete(PX.Objects.Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
      public class dekitting : PX.Data.BQL.BqlString.Constant<dekitting>
		{
			public dekitting() : base(Dekitting) {}
		}

		public class reversed : PX.Data.BQL.BqlString.Constant<reversed>
		{
			public reversed() : base(Reversed) {}
		}
	}
	#endregion

	/// <summary>
	/// Contains the main properties of fixed assets and their classes.
	/// Fixed assets are edited on the Fixed Assets (FA.30.30.00) form (which corresponds to the <see cref="AssetMaint"/> graph).
	/// The fixed asset classes edited through the Fixed Asset Classes (FA.20.10.00) screen (corresponds to the <see cref="AssetClassMaint"/> graph).
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(new Type[]{
		typeof(AssetClassMaint),
		typeof(AssetClassMaint),
		typeof(AssetMaint),
		typeof(AssetMaint)},
		new Type[]{
			typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>, And<FixedAsset.recordType, Equal<FARecordType.classType>>>>),
			typeof(Where<FAClass.assetID, Less<Zero>, And<FAClass.recordType, Equal<FARecordType.classType>>>),
			typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>, And<FixedAsset.recordType, Equal<FARecordType.assetType>>>>),
			typeof(Where<FixedAsset.assetID, Less<Zero>, And<FixedAsset.recordType, Equal<FARecordType.assetType>>>)

		})]
	[PXCacheName(Messages.FixedAsset)]
	public partial class FixedAsset : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		/// <summary>
		/// An unbound service field, which indicates that the fixed asset is marked for processing.
		/// </summary>
		/// <value>
		/// If the value of the field is <c>true</c>, the asset will be processed; otherwise, the asset will not be processed.
		/// </value>
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion

		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		/// <summary>
		/// The identifier of the fixed asset. The identifier is used for foreign references; it can be negative for newly inserted records.
		/// </summary>
		/// <value>A unique integer number.</value>
		[PXDBIdentity]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual int? AssetID { get; set; }
		#endregion

		#region RecordType
		public abstract class recordType : PX.Data.BQL.BqlString.Field<recordType> { }
		/// <summary>
		/// A type of the entity: fixed asset, class, or element.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <list type="bullet">
		/// <item> <term><c>"C"</c></term> <description>indicates a fixed asset class.</description> </item>
		/// <item> <term><c>"A"</c></term> <description>indicates a fixed asset.</description> </item>
		/// <item> <term><c>"E"</c></term> <description>indicates a fixed asset element (also called component).</description> </item>
		///</list>
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Record Type", TabOrder = 0)]
		[FARecordType.List]
		public virtual string RecordType { get; set; }
		#endregion

		#region AssetCD
		public abstract class assetCD : PX.Data.BQL.BqlString.Field<assetCD> { }
		/// <summary>
		/// A string identifier, which contains a key value. This field is also a selector for navigation.
		/// </summary>
		/// <value>The value can be entered manually or can be auto-numbered.</value>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Asset ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search2<assetCD,
			LeftJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>,
			LeftJoin<FALocationHistory, On<FALocationHistory.assetID, Equal<FixedAsset.assetID>, 
				And<FALocationHistory.revisionID, Equal<FADetails.locationRevID>>>,
			LeftJoin<Branch, On<Branch.branchID, Equal<FALocationHistory.locationID>>,
			LeftJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<FALocationHistory.employeeID>>,
			LeftJoin<FAClass, On<FAClass.assetID, Equal<FixedAsset.classID>>>>>>>, 
			Where<recordType, Equal<Current<recordType>>>>),
			typeof(assetCD),
			typeof(description),
			typeof(classID),
			typeof(FAClass.description),
			typeof(depreciable),
			typeof(usefulLife),
			typeof(assetTypeID),
			typeof(FADetails.status),
			typeof(Branch.branchCD),
			typeof(EPEmployee.acctName),
			typeof(FALocationHistory.department), 
			Filterable = true)]
		[FARecordType.Numbering]
		[PXFieldDescription]
		public virtual string AssetCD { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// A reference to the branch, <see cref="Branch"/>.
		/// </summary>
		/// <value>An integer identifier of the branch. It is a required value. By default, the value is set to the identifier of the current branch.</value>
		[Branch]
		public virtual int? BranchID { get; set; }
		#endregion

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
		/// <summary>
		/// A reference to the fixed asset class.
		/// </summary>
		/// <value>An integer identifier of the fixed asset class. It is a required value.</value>
		[PXDBInt]
		[PXRestrictor(typeof(Where<FAClass.active, Equal<True>>), Messages.InactiveFAClass, typeof(FAClass.assetCD))]
		[PXSelector(typeof(Search<FAClass.assetID, Where<FAClass.recordType, Equal<FARecordType.classType>>>),
			typeof(FAClass.assetCD),
			typeof(FAClass.assetTypeID),
			typeof(FAClass.description),
			typeof(FAClass.usefulLife),
			SubstituteKey = typeof(FAClass.assetCD),
			DescriptionField = typeof(FAClass.description),
			CacheGlobal = true)]
		[PXUIField(DisplayName = "Asset Class", Visibility = PXUIVisibility.Visible, TabOrder = 3)]
		public virtual int? ClassID { get; set; }
		#endregion

		#region OldClassID
		public abstract class oldClassID : PX.Data.BQL.BqlInt.Field<oldClassID> { }
		/// <summary>
		/// The service reference to the previous class, which is used in the transfer process only.
		/// </summary>
		/// <value>An integer identifier of the fixed asset class.</value>
		[PXInt]
		[PXDBCalced(typeof(FixedAsset.classID), typeof(int), Persistent = true)]
		public virtual int? OldClassID { get; set; }
		#endregion

		#region ParentAssetID
		public abstract class parentAssetID : PX.Data.BQL.BqlInt.Field<parentAssetID> { }
		/// <summary>
		/// The reference to the parent fixed asset, which is generally used in fixed asset components.
		/// </summary>
		/// <value>An integer identifier of the fixed asset. It is a required value for the fixed asset component.</value>
		[PXDBInt]
		[PXParent(typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.parentAssetID>>>>), UseCurrent = true, LeaveChildren = true)]
		[PXSelector(typeof(Search<FixedAsset.assetID, Where<FixedAsset.assetID, NotEqual<Current<FixedAsset.assetID>>,
			And<Where<FixedAsset.recordType, Equal<Current<FixedAsset.recordType>>,
				And<Current<FixedAsset.recordType>, NotEqual<FARecordType.elementType>,
				Or<Current<FixedAsset.recordType>, Equal<FARecordType.elementType>,
				And<FixedAsset.recordType, Equal<FARecordType.assetType>>>>>>>>),
			typeof(FixedAsset.assetCD), 
			typeof(FixedAsset.assetTypeID), 
			typeof(FixedAsset.description), 
			typeof(FixedAsset.usefulLife),
			SubstituteKey = typeof(assetCD),
			DescriptionField = typeof(description))]
		[PXUIField(DisplayName = "Parent Asset", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 2)]
		public virtual int? ParentAssetID { get; set; }
		#endregion

		#region AssetTypeID
		public abstract class assetTypeID : PX.Data.BQL.BqlString.Field<assetTypeID> { }
		/// <summary>
		/// The reference to the fixed asset type, <see cref="FAType"/>
		/// </summary>
		/// <value>An integer identifier of the fixed asset type. It is a required value. By default, the value is inserted from the fixed asset class.</value>
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<FAType.assetTypeID>), DescriptionField = typeof(FAType.description))]
		[PXUIField(DisplayName = "Asset Type", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 4, Required = true)]
		[PXDefault(typeof(Search<FixedAsset.assetTypeID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[PXFormula(typeof(Switch<Case<Where<FixedAsset.classID, IsNotNull>, Selector<FixedAsset.classID, FAClass.assetTypeID>>, Null>))]
		public virtual string AssetTypeID { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		/// <summary>
		/// The status of the fixed asset.
		/// </summary>
		/// <value>
		/// The allowed values of the asset status described in <see cref="FixedAssetStatus.ListAttribute"/>
		/// </value>
		[PXString(1, IsFixed = true)]
		[PXDBScalar(typeof(Search<FADetails.status, Where<FADetails.assetID, Equal<FixedAsset.assetID>>>))]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[FixedAssetStatus.List]
		[PXDefault(FixedAssetStatus.Active, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Status { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		/// <summary>
		/// The description of fixed asset.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 2)]
		[PXFieldDescription]
		public virtual string Description { get; set; }
		#endregion


		#region ConstructionAccountID
		public abstract class constructionAccountID : PX.Data.BQL.BqlInt.Field<constructionAccountID> { }
		/// <summary>
		/// The reference to the construction account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the construction account. By default, the value is inserted from the fixed asset class.</value>
		[Account(DisplayName = "Construction Account", Visibility = PXUIVisibility.Invisible, Visible = false, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<FixedAsset.constructionAccountID, 
			Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? ConstructionAccountID { get; set; }
		#endregion
		#region ConstructionSubID
		public abstract class constructionSubID : PX.Data.BQL.BqlInt.Field<constructionSubID> { }
		/// <summary>
		/// The reference to the construction subaccount associated with the construction account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the construction subaccount. By default, the value is inserted from the fixed asset class.</value>
		[SubAccount(typeof(FixedAsset.constructionAccountID), DisplayName = "Construction Sub.", Visibility = PXUIVisibility.Invisible, Visible = false, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<FixedAsset.constructionSubID,
			Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? ConstructionSubID { get; set; }
		#endregion

		#region FAAccountID
		public abstract class fAAccountID : PX.Data.BQL.BqlInt.Field<fAAccountID> { }
		/// <summary>
		/// The reference to the Fixed Assets account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the account. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(Search<FixedAsset.fAAccountID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[Account(DisplayName = "Fixed Assets Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.FA)]
		public virtual int? FAAccountID { get; set; }
		#endregion
		#region FASubID
		public abstract class fASubID : PX.Data.BQL.BqlInt.Field<fASubID> { }
		/// <summary>
		/// The reference to the Fixed Assets subaccount associated with the Fixed Assets account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the subaccount. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault]
		[SubAccount(typeof(FixedAsset.fAAccountID), DisplayName = "Fixed Assets Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual int? FASubID { get; set; }
		#endregion

		#region FAAccrualAcctID
		public abstract class fAAccrualAcctID : PX.Data.BQL.BqlInt.Field<fAAccrualAcctID> { }
		/// <summary>
		/// The reference to the FA Accrual account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the account. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(FASetup.fAAccrualAcctID))]
		[Account(DisplayName = "FA Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual int? FAAccrualAcctID { get; set; }
		#endregion
		#region FAAccrualSubID
		public abstract class fAAccrualSubID : PX.Data.BQL.BqlInt.Field<fAAccrualSubID> { }
		/// <summary>
		/// The reference to the FA Accrual subaccount associated with the FA Accrual account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the subaccount. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(FASetup.fAAccrualSubID))]
		[SubAccount(typeof(FixedAsset.fAAccrualAcctID), Visibility = PXUIVisibility.Visible, DisplayName = "FA Accrual Sub.", DescriptionField = typeof(Sub.description))]
		public virtual int? FAAccrualSubID { get; set; }
		#endregion

		#region AccumulatedDepreciationAccountID
		public abstract class accumulatedDepreciationAccountID : PX.Data.BQL.BqlInt.Field<accumulatedDepreciationAccountID> { }
		/// <summary>
		/// The reference to the Accumulated Depreciation account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the account. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(Search<FixedAsset.accumulatedDepreciationAccountID, 
				Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[Account(DisplayName = "Accumulated Depreciation Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.FA)]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		public virtual int? AccumulatedDepreciationAccountID { get; set; }
		#endregion
		#region AccumulatedDepreciationSubID
		public abstract class accumulatedDepreciationSubID : PX.Data.BQL.BqlInt.Field<accumulatedDepreciationSubID> { }
		/// <summary>
		/// The reference to the Accumulated Depreciation subaccount associated with the Accumulated Depreciation account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the subaccount. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault]
		[SubAccount(typeof(FixedAsset.accumulatedDepreciationAccountID), DisplayName = "Accumulated Depreciation Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		public virtual int? AccumulatedDepreciationSubID { get; set; }
		#endregion

		#region DepreciatedExpenseAccountID
		public abstract class depreciatedExpenseAccountID : PX.Data.BQL.BqlInt.Field<depreciatedExpenseAccountID> { }
		/// <summary>
		/// The reference to the Depreciation Expense account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the account. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(Search<FixedAsset.depreciatedExpenseAccountID,
			 Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[Account(DisplayName = "Depreciation Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		public virtual int? DepreciatedExpenseAccountID { get; set; }
		#endregion
		#region DepreciatedExpenseSubID
		public abstract class depreciatedExpenseSubID : PX.Data.BQL.BqlInt.Field<depreciatedExpenseSubID> { }
		/// <summary>
		/// The reference to the Depreciation Expense subaccount associated with the Depreciation Expense account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the subaccount. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault]
		[SubAccount(typeof(FixedAsset.depreciatedExpenseAccountID), DisplayName = "Depreciation Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		public virtual int? DepreciatedExpenseSubID { get; set; }
		#endregion

		#region DisposalAccountID
		public abstract class disposalAccountID : PX.Data.BQL.BqlInt.Field<disposalAccountID> { }
		/// <summary>
		/// The reference to the Proceeds account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the account. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(Search<FixedAsset.disposalAccountID, 
			Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Proceeds Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual int? DisposalAccountID { get; set; }
		#endregion
		#region DisposalSubID
		public abstract class disposalSubID : PX.Data.BQL.BqlInt.Field<disposalSubID> { }
		/// <summary>
		/// The reference to the Proceeds subaccount associated with the Proceeds account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the subaccount. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(Search<FixedAsset.disposalSubID, 
			Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(FixedAsset.disposalAccountID), DisplayName = "Proceeds Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual int? DisposalSubID { get; set; }
		#endregion

		#region RentAccountID
		public abstract class rentAccountID : PX.Data.BQL.BqlInt.Field<rentAccountID> { }
		/// <summary>
		/// The reference to the Rent account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the account</value>
		[Account(DisplayName = "Rent Account", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Account.description), Visible = false)]
		public virtual int? RentAccountID { get; set; }
		#endregion
		#region RentSubID
		public abstract class rentSubID : PX.Data.BQL.BqlInt.Field<rentSubID> { }
		/// <summary>
		/// The reference to the Rent subaccount associated with the Rent account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the subaccount.</value>
		[SubAccount(typeof(FixedAsset.rentAccountID), DisplayName = "Rent Sub.", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description), Visible = false)]
		public virtual int? RentSubID { get; set; }
		#endregion

		#region LeaseAccountID
		public abstract class leaseAccountID : PX.Data.BQL.BqlInt.Field<leaseAccountID> { }
		/// <summary>
		/// The reference to the Lease account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the account</value>
		[Account(DisplayName = "Lease Account", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Account.description), Visible = false)]
		public virtual int? LeaseAccountID { get; set; }
		#endregion
		#region LeaseSubID
		public abstract class leaseSubID : PX.Data.BQL.BqlInt.Field<leaseSubID> { }
		/// <summary>
		/// The reference to the Lease subaccount associated with the Lease account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the subaccount.</value>
		[SubAccount(typeof(FixedAsset.leaseAccountID), DisplayName = "Lease Sub.", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description), Visible = false)]
		public virtual int? LeaseSubID { get; set; }
		#endregion

		#region GainAcctID
		public abstract class gainAcctID : PX.Data.BQL.BqlInt.Field<gainAcctID> { }
		/// <summary>
		/// The reference to the Gain account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the account. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(Search<FixedAsset.gainAcctID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[Account(DisplayName = "Gain Account", DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual int? GainAcctID { get; set; }
		#endregion
		#region GainSubID
		public abstract class gainSubID : PX.Data.BQL.BqlInt.Field<gainSubID> { }
		/// <summary>
		/// The reference to the Gain subaccount associated with the Gain account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the subaccount. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(Search<FixedAsset.gainSubID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[SubAccount(typeof(FixedAsset.gainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Gain Sub.")]
		public virtual int? GainSubID { get; set; }
		#endregion

		#region LossAcctID
		public abstract class lossAcctID : PX.Data.BQL.BqlInt.Field<lossAcctID> { }
		/// <summary>
		/// The reference to the Loss account, <see cref="Account"/>
		/// </summary>
		/// <value>An integer identifier of the account. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(Search<FixedAsset.lossAcctID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[Account(DisplayName = "Loss Account", DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual int? LossAcctID { get; set; }
		#endregion
		#region LossSubID
		public abstract class lossSubID : PX.Data.BQL.BqlInt.Field<lossSubID> { }
		/// <summary>
		/// The reference to the Loss subaccount associated with the Loss account, <see cref="Sub"/>
		/// </summary>
		/// <value>An integer identifier of the subaccount. By default, the value is inserted from the fixed asset class.</value>
		[PXDefault(typeof(Search<FixedAsset.lossSubID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[SubAccount(typeof(FixedAsset.lossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Loss Sub.")]
		public virtual int? LossSubID { get; set; }
		#endregion


		#region FASubMask
		public abstract class fASubMask : PX.Data.BQL.BqlString.Field<fASubMask> { }
		/// <summary>
		/// A subaccount mask that is used to generate the Fixed Assets subaccount.
		/// </summary>
		/// <value>
		/// The allowed symbols of the subaccount mask described in <see cref="SubAccountMaskAttribute"/>
		/// </value>
		[PXDBString(30, IsUnicode = true, InputMask = "")]
		public virtual string FASubMask { get; set; }
		#endregion

		#region AccumDeprSubMask
		public abstract class accumDeprSubMask : PX.Data.BQL.BqlString.Field<accumDeprSubMask> { }
		/// <summary>
		/// A subaccount mask that is used to generate the Accumulated Depreciation subaccount.
		/// </summary>
		/// <value>
		/// The allowed symbols of the subaccount mask described in <see cref="SubAccountMaskAttribute"/>
		/// </value>
		[PXDBString(30, IsUnicode = true, InputMask = "")]
		public virtual string AccumDeprSubMask { get; set; }
		#endregion

		#region DeprExpenceSubMask
		public abstract class deprExpenceSubMask : PX.Data.BQL.BqlString.Field<deprExpenceSubMask> { }
		/// <summary>
		/// A subaccount mask that is used to generate the Depreciation Expence subaccount.
		/// </summary>
		/// <value>
		/// The allowed symbols of the subaccount mask described in <see cref="SubAccountMaskAttribute"/>
		/// </value>
		[PXDBString(30, IsUnicode = true, InputMask = "")]
		public virtual string DeprExpenceSubMask { get; set; }
		#endregion

		#region UseFASubMask
		public abstract class useFASubMask : PX.Data.BQL.BqlBool.Field<useFASubMask> { }
		/// <summary>
		/// A flag that determines whether the Fixed Assets subaccount mask has to be used for the Accumulated Depreciation subaccount.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Fixed Asset Sub. Mask", FieldClass = SubAccountAttribute.DimensionName)]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		[PXUIEnabled(typeof(FixedAsset.depreciable))]
		public virtual bool? UseFASubMask { get; set; }
		#endregion

		#region ProceedsSubMask
		public abstract class proceedsSubMask : PX.Data.BQL.BqlString.Field<proceedsSubMask> { }
		/// <summary>
		/// A subaccount mask that is used to generate the Proceed subaccount.
		/// </summary>
		/// <value>
		/// The allowed symbols of the subaccount mask described in <see cref="SubAccountMaskAttribute"/>
		/// </value>
		[PXDBString(30, IsUnicode = true, InputMask = "")]
		public virtual string ProceedsSubMask { get; set; }
		#endregion

		#region GainLossSubMask
		public abstract class gainLossSubMask : PX.Data.BQL.BqlString.Field<gainLossSubMask> { }
		/// <summary>
		/// A subaccount mask that is used to generate the Gain and Loss subaccounts.
		/// </summary>
		/// <value>
		/// The allowed symbols of the subaccount mask described in <see cref="SubAccountMaskAttribute"/>
		/// </value>
		[PXDBString(30, IsUnicode = true, InputMask = "")]
		public virtual string GainLossSubMask { get; set; }
		#endregion


		#region Depreciable
		public abstract class depreciable : PX.Data.BQL.BqlBool.Field<depreciable> { }
		/// <summary>
		/// A flag that determines whether the fixed asset should be depreciated.
		/// </summary>
		/// <value>By default, the value is inserted from the appropriate fixed asset class.</value>
		[PXDBBool]
		[PXFormula(typeof(Switch<
			Case<Where<EntryStatus, Equal<EntryStatus.inserted>, And<FixedAsset.classID, IsNotNull>>, Selector<FixedAsset.classID, FixedAsset.depreciable>,
			Case<Where<EntryStatus, Equal<EntryStatus.inserted>, And<FixedAsset.classID, IsNull>>, True>>, 
			FixedAsset.depreciable>))]
		[PXUIField(DisplayName = "Depreciate", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIEnabled(typeof(Where<FixedAsset.recordType, NotEqual<FARecordType.assetType>, 
			Or<FixedAsset.isAcquired, NotEqual<True>, 
			Or<EntryStatus, Equal<EntryStatus.inserted>>>>))]
		public virtual bool? Depreciable { get; set; }
		#endregion

		#region UsefulLife
		public abstract class usefulLife : PX.Data.BQL.BqlDecimal.Field<usefulLife> { }
		/// <summary>
		/// The useful life of the fixed asset measured in years.
		/// </summary>
		/// <value> An integer number of years of useful life. By default, the value is inserted from the appropriate fixed asset class.</value>
		[PXDBDecimal(4)]
		[PXDefault(typeof(Search<FixedAsset.usefulLife, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[PXUIField(DisplayName = "Useful Life, Years", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		[PXUIEnabled(typeof(FixedAsset.depreciable))]
		public virtual decimal? UsefulLife { get; set; }
		#endregion

		#region IsTangible
		public abstract class isTangible : PX.Data.BQL.BqlBool.Field<isTangible> { }
		/// <summary>
		/// A flag that indicates whether the fixed asset is tangible.
		/// </summary>
		/// <value>If a type is associated with the fixed asset, the default value is inserted from the fixed asset type (<see cref="FAType"/>).
		/// If no type is associated with the fixed asset, the default value is <c>true</c>.</value>
		[PXDBBool]
		[PXDefault(true, typeof(Search<FixedAsset.isTangible, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[PXFormula(typeof(Switch<Case<Where<FixedAsset.assetTypeID, IsNotNull>, Selector<FixedAsset.assetTypeID, FAType.isTangible>>, True>))]
		[PXUIField(DisplayName = "Tangible", Enabled = false)]
		public virtual bool? IsTangible { get; set; }
		#endregion

		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		/// <summary>
		/// A flag that indicates whether the fixed asset is active.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active { get; set; }
		#endregion

		#region IsAcquired
		public abstract class isAcquired : PX.Data.BQL.BqlBool.Field<isAcquired> { }
		/// <summary>
		/// A flag that indicates whether the fixed asset was acquired.
		/// </summary>
		[PXDBBool]
		public virtual bool? IsAcquired { get; set; }
		#endregion

		#region Suspended
		public abstract class suspended : PX.Data.BQL.BqlBool.Field<suspended> { }
		/// <summary>
		/// A flag that indicates whether the fixed asset is suspended.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Suspended")]
		public virtual bool? Suspended { get; set; }
		#endregion

		#region HoldEntry
		public abstract class holdEntry : PX.Data.BQL.BqlBool.Field<holdEntry> { }
		/// <summary>
		/// A flag that determines whether new fixed assets should have the On Hold status by default. 
		/// This field makes sense only for fixed asset classes.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hold on Entry")]
		public virtual bool? HoldEntry { get; set; }
		#endregion

		#region AcceleratedDepreciation
		public abstract class acceleratedDepreciation : PX.Data.BQL.BqlBool.Field<acceleratedDepreciation> { }
		/// <summary>
		/// A flag that determines whether the Straight Line depreciation method is equivalent to the Remaining Value depreciation method.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Accelerated Depreciation for SL Depr. Method")]
		[PXDefault(false)]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		[PXUIEnabled(typeof(FixedAsset.depreciable))]
		public virtual bool? AcceleratedDepreciation { get; set; }
		#endregion

		#region ServiceScheduleID
		public abstract class serviceScheduleID : PX.Data.BQL.BqlInt.Field<serviceScheduleID> { }
		/// <summary>
		/// The reference to the service schedule, <see cref="FAServiceSchedule"/>. This field is reserved for future use.
		/// </summary>
		/// <value>An integer identifier of the service schedule. 
		/// By default, the value is inserted from the appropriate fixed asset class.</value>
		[PXDBInt]
		[PXUIField(DisplayName = "Service Schedule", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXSelector(typeof(Search<FAServiceSchedule.scheduleID>), 
			typeof(FAServiceSchedule.scheduleCD),
			typeof(FAServiceSchedule.serviceEveryPeriod),
			typeof(FAServiceSchedule.serviceEveryValue),
			typeof(FAServiceSchedule.serviceAfterUsageValue),
			typeof(FAServiceSchedule.serviceAfterUsageUOM),
			typeof(FAServiceSchedule.description),
			SubstituteKey = typeof(FAServiceSchedule.scheduleCD), 
			DescriptionField = typeof(FAServiceSchedule.description))]
		[PXDefault(typeof(Search<FixedAsset.serviceScheduleID, 
			Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? ServiceScheduleID { get; set; }
		#endregion

		#region UsageScheduleID
		public abstract class usageScheduleID : PX.Data.BQL.BqlInt.Field<usageScheduleID> { }
		/// <summary>
		/// The reference to the usage schedule, <see cref="FAUsageSchedule"/>. This field is reserved for future use.
		/// </summary>
		/// <value>An integer identifier of the usage schedule. 
		/// By default, the value is inserted from the appropriate fixed asset class.</value>
		[PXDBInt]
		[PXUIField(DisplayName = "Usage Measurement Schedule", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXSelector(typeof(Search<FAUsageSchedule.scheduleID>), 
			typeof(FAUsageSchedule.scheduleCD),
			typeof(FAUsageSchedule.readUsageEveryPeriod),
			typeof(FAUsageSchedule.readUsageEveryValue),
			typeof(FAUsageSchedule.usageUOM),
			typeof(FAUsageSchedule.description),
			SubstituteKey = typeof(FAUsageSchedule.scheduleCD), 
			DescriptionField = typeof(FAUsageSchedule.usageUOM))]
		[PXDefault(typeof(Search<FixedAsset.usageScheduleID,
			Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? UsageScheduleID { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.FA, 
			Messages.FixedAssetSearchTitle, 
			new Type[] { typeof(FixedAsset.assetCD) },
			new Type[]
			{
				typeof(FixedAsset.description),
				typeof(FixedAsset.assetTypeID),
				typeof(FixedAsset.branchID),
				typeof(FixedAsset.recordType),
				typeof(FixedAsset.classID),
				typeof(FAClass.description)
			},
			Line1Format = "{1}{3}{4}", 
			Line1Fields = new Type[]
			{
				typeof(FixedAsset.parentAssetID),
				typeof(FixedAsset.assetCD),
				typeof(FixedAsset.classID),
				typeof(FAClass.description),
				typeof(FixedAsset.assetTypeID)
			},
			Line2Format = "{0}", 
			Line2Fields = new Type[] { typeof(FixedAsset.description) }
		)]
		[PXNote(DescriptionField = typeof(FixedAsset.assetCD), Selector = typeof(FixedAsset.assetCD))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		/// <summary>
		/// The number of objects in the fixed asset.
		/// </summary>
		/// <value>A decimal number (which can be a fractional number). The default value is <c>1.0</c>.</value>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "1.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual decimal? Qty { get; set; }
		#endregion

		#region SplittedFrom
		public abstract class splittedFrom : PX.Data.BQL.BqlInt.Field<splittedFrom> { }
		/// <summary>
		/// An identifier of the source fixed asset if the current fixed asset has been splitted.
		/// </summary>
		[PXDBInt]
		public virtual int? SplittedFrom { get; set; }
		#endregion

		#region DisposalAmt
		public abstract class disposalAmt : PX.Data.BQL.BqlDecimal.Field<disposalAmt> { }
		/// <summary>
		/// The disposal amount of the asset.
		/// </summary>
		[PXBaseCury]
		[PXUIField(DisplayName = "Proceeds Amount")]
		public virtual decimal? DisposalAmt { get; set; }
		#endregion

		#region SalvageAmtAfterSplit
		public abstract class salvageAmtAfterSplit : PX.Data.BQL.BqlDecimal.Field<salvageAmtAfterSplit> { }
		/// <exclude/>
		[PXBaseCury]
		public virtual decimal? SalvageAmtAfterSplit { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual byte[] tstamp
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
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedDateTime]
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
		
		#region FADetails
		[Serializable()]
		public class FADetails : IBqlTable
		{
			#region AssetID
			public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
			protected int? _AssetID;
			[PXDBInt(IsKey = true)]
			public virtual int? AssetID
			{
				get
				{
					return this._AssetID;
				}
				set
				{
					this._AssetID = value;
				}
			}
			#endregion
			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			protected string _Status;
			[PXDBString(1, IsFixed = true)]
			[FixedAssetStatus.List]
			public virtual string Status
			{
				get
				{
					return this._Status;
				}
				set
				{
					this._Status = value;
				}
			}
			#endregion
			#region LocationRevID
			public abstract class locationRevID : PX.Data.BQL.BqlInt.Field<locationRevID> { }
			protected int? _LocationRevID;
			[PXDBInt]
			public virtual int? LocationRevID
			{
				get
				{
					return _LocationRevID;
				}
				set
				{
					_LocationRevID = value;
				}
			}
			#endregion
		}
		#endregion
	}

	[Serializable]
	[PXCacheName(Messages.AssetClass)]
	public partial class FAClass : FixedAsset
	{
		public new abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		public new abstract class recordType : PX.Data.BQL.BqlString.Field<recordType> { }
		public new abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

		public new abstract class assetCD : PX.Data.BQL.BqlString.Field<assetCD> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Asset Class ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search2<assetCD,
			LeftJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>,
			LeftJoin<FALocationHistory, On<FALocationHistory.assetID, Equal<FixedAsset.assetID>,
				And<FALocationHistory.revisionID, Equal<FADetails.locationRevID>>>,
			LeftJoin<Branch, On<Branch.branchID, Equal<FALocationHistory.locationID>>,
			LeftJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<FALocationHistory.employeeID>>,
			LeftJoin<FAClass, On<FAClass.assetID, Equal<FixedAsset.classID>>>>>>>,
			Where<recordType, Equal<Current<recordType>>>>),
			typeof(assetCD),
			typeof(description),
			typeof(classID),
			typeof(description),
			typeof(depreciable),
			typeof(usefulLife),
			typeof(assetTypeID),
			typeof(FADetails.status),
			typeof(Branch.branchCD),
			typeof(EPEmployee.acctName),
			typeof(FALocationHistory.department),
			Filterable = true)]
		[FARecordType.Numbering]
		[PXFieldDescription]
		public override string AssetCD { get; set; }

		public new abstract class assetTypeID : PX.Data.BQL.BqlString.Field<assetTypeID> { }
		public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		public new abstract class usefulLife : PX.Data.BQL.BqlDecimal.Field<usefulLife> { }

		#region FASubMask
		public new abstract class fASubMask : PX.Data.BQL.BqlString.Field<fASubMask> { }
		[PXDefault]
		[SubAccountMask(DisplayName = "Combine Fixed Asset Sub. from")]
		public override string FASubMask { get; set; }
		#endregion
		#region AccumDeprSubMask
		public new abstract class accumDeprSubMask : PX.Data.BQL.BqlString.Field<accumDeprSubMask> { }
		[PXDefault]
		[SubAccountMask(DisplayName = "Combine Accumulated Depreciation Sub. from")]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		public override string AccumDeprSubMask { get; set; }
		#endregion
		#region DeprExpenceSubMask
		public new abstract class deprExpenceSubMask : PX.Data.BQL.BqlString.Field<deprExpenceSubMask> { }
		[PXDefault]
		[SubAccountMask(DisplayName = "Combine Depreciation Expense Sub. from")]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		public override string DeprExpenceSubMask { get; set; }
		#endregion
		#region ProceedsSubMask
		public new abstract class proceedsSubMask : PX.Data.BQL.BqlString.Field<proceedsSubMask> { }
		[SubAccountMask(DisplayName = "Combine Proceeds Sub. from")]
		public override string ProceedsSubMask { get; set; }
		#endregion
		#region GainLossSubMask
		public new abstract class gainLossSubMask : PX.Data.BQL.BqlString.Field<gainLossSubMask> { }
		[PXDefault]
		[SubAccountMask(DisplayName = "Combine Gain/Loss Sub. from")]
		public override string GainLossSubMask { get; set; }
		#endregion
	}

	[Serializable]
	[PXCacheName(Messages.FAComponent)]
	public partial class FAComponent : FixedAsset
	{
		#region RecordType
		public new abstract class recordType : PX.Data.BQL.BqlString.Field<recordType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FARecordType.ElementType)]
		[PXUIField(DisplayName = "Record Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, TabOrder = 0)]
		[FARecordType.List]
		public override string RecordType { get; set; }
		#endregion

		#region AssetCD
		public new abstract class assetCD : PX.Data.BQL.BqlString.Field<assetCD> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Asset ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder=1)]
		[PXSelector(typeof(Search<FAComponent.assetCD, 
			Where<FAComponent.recordType, Equal<FARecordType.elementType>, 
				And<Where<FAComponent.parentAssetID, IsNull, 
					Or<FAComponent.assetCD, Equal<Current<FAComponent.assetCD>>>>>>>))]
		public override string AssetCD { get; set; }
		#endregion
	}
}
