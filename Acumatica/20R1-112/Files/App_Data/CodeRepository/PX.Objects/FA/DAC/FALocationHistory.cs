using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.EP;
using PX.Objects.CS;

namespace PX.Objects.FA
{
	/// <summary>
	/// Contains the history of changes of the asset location.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.FALocationHistory)]
	public partial class FALocationHistory : PX.Data.IBqlTable, IFALocation
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		/// <summary>
		/// A reference to <see cref="FixedAsset"/>.
		/// This field is a part of the primary key.
		/// The full primary key contains the <see cref="AssetID"/> and the <see cref="RevisionID"/> fields.		
		/// </summary>
		/// <value>
		/// By default, the value is set to the current fixed asset identifier.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FALocationHistory.assetID>>>>))]
		[PXDBLiteDefault(typeof(FixedAsset.assetID))]
		public virtual Int32? AssetID
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
		#region TransactionType
		public abstract class transactionType : PX.Data.BQL.BqlString.Field<transactionType>
		{
			/// <summary>
			/// The type of the change of the asset location.
			/// </summary>
			/// <value>
			/// The class exposes the following values:
			/// <list type="bullet">
			/// <item> <term><c>"D"</c></term> <description>Displacement (change of the physical location)</description> </item>
			/// <item> <term><c>"R"</c></term> <description>Change of the responsible</description> </item>
			/// </list>
			/// </value>
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Displacement, ChangeResponsible },
					new string[] { Messages.Displacement, Messages.ChangeResponsible }) { ; }
			}

			public const string Displacement = "D";
			public const string ChangeResponsible = "R";

			public class displacement : PX.Data.BQL.BqlString.Constant<displacement>
			{
				public displacement() : base(Displacement) { ;}
			}
			public class changeResponsible : PX.Data.BQL.BqlString.Constant<changeResponsible>
			{
				public changeResponsible() : base(ChangeResponsible) { ;}
			}
		}
		protected String _TransactionType;
		/// <summary>
		/// The type of the change of the asset location.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="transactionType.ListAttribute"/>.
		/// By default, the value is set to <see cref="transactionType.Displacement"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(transactionType.Displacement)]
		[transactionType.List()]
		[PXUIField(DisplayName = "Transaction Type")]
		public virtual String TransactionType
		{
			get
			{
				return this._TransactionType;
			}
			set
			{
				this._TransactionType = value;
			}
		}
		#endregion
		#region TransactionDate
		public abstract class transactionDate : PX.Data.BQL.BqlDateTime.Field<transactionDate> { }
		protected DateTime? _TransactionDate;
		/// <summary>
		/// The date when the asset location was changed.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Transaction Date")]
		[PXDefault()]
		public virtual DateTime? TransactionDate
		{
			get
			{
				return this._TransactionDate;
			}
			set
			{
				this._TransactionDate = value;
			}
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
		protected string _PeriodID;
		/// <summary>
		/// The financial period when the asset location was changed.
		/// </summary>
		[FABookPeriodID(assetSourceType: typeof(assetID))]
		public virtual string PeriodID
		{
			get
			{
				return _PeriodID;
			}
			set
			{
				_PeriodID = value;
			}
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
		[PXDBInt]
		[PXSelector(typeof(Search<FAClass.assetID>),
			SubstituteKey = typeof(FAClass.assetCD),
			DescriptionField = typeof(FAClass.description),
			CacheGlobal = true)]
		[PXDefault(typeof(Search<FixedAsset.classID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>>>))]
		[PXUIField(DisplayName = "Asset Class", Visible = false)]
		public virtual int? ClassID { get; set; }
		#endregion
		#region BuildingID
		public abstract class buildingID : PX.Data.BQL.BqlInt.Field<buildingID> { }
		protected int? _BuildingID;
		/// <summary>
		/// The building in which the fixed asset is physically located.
		/// A reference to <see cref="Building"/>.
		/// Changing this field leads to the creation of a revision of the asset location.
		/// </summary>
		/// <value>
		/// An integer identifier of the building. 
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(Search<Building.buildingID, Where<Building.branchID, Equal<Current<FALocationHistory.locationID>>>>), 
			SubstituteKey=typeof(Building.buildingCD), DescriptionField = typeof(Building.description))]
		[PXUIField(DisplayName = "Building")]
		public virtual int? BuildingID
		{
			get
			{
				return this._BuildingID;
			}
			set
			{
				this._BuildingID = value;
			}
		}
		#endregion
		#region Floor
		public abstract class floor : PX.Data.BQL.BqlString.Field<floor> { }
		protected String _Floor;
		/// <summary>
		/// The floor on which the fixed asset is physically located.
		/// Changing this field leads to the creation of a revision of the asset location.
		/// </summary>
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Floor")]
		public virtual String Floor
		{
			get
			{
				return this._Floor;
			}
			set
			{
				this._Floor = value;
			}
		}
		#endregion
		#region Room
		public abstract class room : PX.Data.BQL.BqlString.Field<room> { }
		protected String _Room;
		/// <summary>
		/// The room in which the fixed asset is physically located.
		/// Changing this field leads to the creation of a revision of the asset location.
		/// </summary>
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Room")]
		public virtual String Room
		{
			get
			{
				return this._Room;
			}
			set
			{
				this._Room = value;
			}
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected int? _EmployeeID;
		/// <summary>
		/// The custodian of the fixed asset.
		/// A reference to <see cref="EPEmployee"/>.
		/// Changing this field leads to the creation of a revision of the asset location.
		/// </summary>
		/// <value>
		/// An integer identifier of the employee. 
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(EPEmployee.bAccountID), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
		[PXUIField(DisplayName = "Custodian")]
		public virtual int? EmployeeID
		{
			get
			{
				return _EmployeeID;
			}
			set
			{
				_EmployeeID = value;
			}
		}
        #endregion
        #region Custodian
        public abstract class custodian : PX.Data.BQL.BqlGuid.Field<custodian> { }
        protected Guid? _Custodian;
        /// <summary>
        /// The user of custodian of the fixed asset.
        /// A reference to <see cref="PX.SM.Users"/>.
        /// </summary>
        /// <value>
        /// A GUID identifier of the user. 
        /// </value>
        [Obsolete(PX.Objects.Common.Messages.FieldIsObsoleteAndWillBeRemoved2019R2)]
        [PXDBField()]
        [PXFormula(typeof(Selector<FALocationHistory.employeeID, EPEmployee.userID>))]
        public virtual Guid? Custodian
        {
            get
            {
                return this._Custodian;
            }
            set
            {
                this._Custodian = value;
            }
        }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		/// <summary>
		/// The branch of the fixed asset.
		/// A reference to <see cref="Branch"/>.
		/// Changing this field leads to the creation of a revision of the asset location.
		/// </summary>
		/// <value>
		/// An integer identifier of the branch.
		/// This is a required field. 
		/// By default, the value is set to the branch of current custodian (if exists) or the current branch. 
		/// </value>
		[Branch(typeof(Coalesce<
			Search2<Location.vBranchID, InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<Location.bAccountID>, And<EPEmployee.defLocationID, Equal<Location.locationID>>>>, Where<EPEmployee.bAccountID, Equal<Current<FALocationHistory.employeeID>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
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
		#region Department
		public abstract class department : PX.Data.BQL.BqlString.Field<department> { }
		protected String _Department;
		/// <summary>
		/// The department of the fixed asset.
		/// A reference to <see cref="EPDepartment"/>.
		/// Changing this field leads to the creation of a revision of the asset location.
		/// </summary>
		/// <value>
		/// An integer identifier of the department. 
		/// This is a required field. 
		/// By default, the value is set to the custodian department.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<EPEmployee.departmentID, Where<EPEmployee.bAccountID, Equal<Current<FALocationHistory.employeeID>>>>))]
		[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
		[PXUIField(DisplayName = "Department")]
		public virtual String Department
		{
			get
			{
				return this._Department;
			}
			set
			{
				this._Department = value;
			}
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;
		/// <summary>
		/// The number of the location revision.
		/// This field is a part of the primary key.
		/// The full primary key contains the <see cref="AssetID"/> and the <see cref="RevisionID"/> fields. 
		/// </summary>
		/// <value>
		/// A unique integer autoincremented number.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		public virtual Int32? RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region PrevRevisionID
		public abstract class prevRevisionID : PX.Data.BQL.BqlInt.Field<prevRevisionID> { }
		protected Int32? _PrevRevisionID;
		/// <summary>
		/// The number of the previous revision of the asset location.
		/// </summary>
		/// <value>
		/// This is the unbound calculated field which contains the decremented number of the active revision (<see cref="RevisionID"/>).
		/// </value>
		[PXInt]
		[PXDBCalced(typeof(Sub<FALocationHistory.revisionID, int1>), typeof(Int32))]
		public virtual Int32? PrevRevisionID
		{
			get
			{
				return this._PrevRevisionID;
			}
			set
			{
				this._PrevRevisionID = value;
			}
		}
		#endregion
		#region FAAccountID
		public abstract class fAAccountID : PX.Data.BQL.BqlInt.Field<fAAccountID> { }
		protected Int32? _FAAccountID;
		/// <summary>
		/// The Fixed Assets account.
		/// A reference to <see cref="Account"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the account. 
		/// This is a required field. 
		/// By default, the value is set to the Fixed Assets account of the fixed asset (<see cref="FixedAsset.FAAccountID"/>).
		/// </value>
		[PXDefault(typeof(Search<FixedAsset.fAAccountID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>>>))]
		[Account(DisplayName = "Fixed Assets Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? FAAccountID
		{
			get
			{
				return this._FAAccountID;
			}
			set
			{
				this._FAAccountID = value;
			}
		}
		#endregion
		#region FASubID
		public abstract class fASubID : PX.Data.BQL.BqlInt.Field<fASubID> { }
		protected Int32? _FASubID;
		/// <summary>
		/// The Fixed Assets subaccount associated with the Fixed Assets account (<see cref="FAAccountID"/>).
		/// A reference to <see cref="Sub"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the subaccount. 
		/// By default, the value is set to the Fixed Assets subaccount of the fixed asset (<see cref="FixedAsset.FASubID"/>).
		/// </value>
		[PXDefault]
		[SubAccount(typeof(FALocationHistory.fAAccountID), DisplayName = "Fixed Assets Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? FASubID
		{
			get
			{
				return this._FASubID;
			}
			set
			{
				this._FASubID = value;
			}
		}
		#endregion
		#region AccumulatedDepreciationAccountID
		public abstract class accumulatedDepreciationAccountID : PX.Data.BQL.BqlInt.Field<accumulatedDepreciationAccountID> { }
		protected Int32? _AccumulatedDepreciationAccountID;
		/// <summary>
		/// The Accumulated Depreciation account.
		/// A reference to <see cref="Account"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the account. 
		/// This is a required field if <see cref="FixedAsset.Depreciable"/> flag is set to <c>true</c>. 
		/// By default, the value is set to the Accumulated Depreciation account of the fixed asset (<see cref="FixedAsset.AccumulatedDepreciationAccountID"/>).
		/// </value>
		[PXDefault(typeof(Search<FixedAsset.accumulatedDepreciationAccountID,
								 Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>>>))]
		[Account(DisplayName = "Accumulated Depreciation Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXUIRequired(typeof(Parent<FixedAsset.depreciable>))]
		public virtual Int32? AccumulatedDepreciationAccountID
		{
			get
			{
				return this._AccumulatedDepreciationAccountID;
			}
			set
			{
				this._AccumulatedDepreciationAccountID = value;
			}
		}
		#endregion
		#region AccumulatedDepreciationSubID
		public abstract class accumulatedDepreciationSubID : PX.Data.BQL.BqlInt.Field<accumulatedDepreciationSubID> { }
		protected Int32? _AccumulatedDepreciationSubID;
		/// <summary>
		/// The Accumulated Depreciation subaccount associated with the Accumulated Depreciation account (<see cref="FAAccountID"/>).
		/// A reference to <see cref="Sub"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the subaccount. 
		/// This is a required field if <see cref="FixedAsset.Depreciable"/> flag is set to <c>true</c>. 
		/// By default, the value is set to the Accumulated Depreciation subaccount of the fixed asset (<see cref="FixedAsset.FASubID"/>).
		/// </value>
		[PXDefault]
		[SubAccount(typeof(FALocationHistory.accumulatedDepreciationAccountID), DisplayName = "Accumulated Depreciation Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXUIRequired(typeof(Parent<FixedAsset.depreciable>))]
		public virtual Int32? AccumulatedDepreciationSubID
		{
			get
			{
				return this._AccumulatedDepreciationSubID;
			}
			set
			{
				this._AccumulatedDepreciationSubID = value;
			}
		}
		#endregion
		#region DepreciatedExpenseAccountID
		public abstract class depreciatedExpenseAccountID : PX.Data.BQL.BqlInt.Field<depreciatedExpenseAccountID> { }
		protected Int32? _DepreciatedExpenseAccountID;
		/// <summary>
		/// The Depreciation Expense account.
		/// A reference to <see cref="Account"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the account. 
		/// This is a required field if <see cref="FixedAsset.Depreciable"/> flag is set to <c>true</c>. 
		/// By default, the value is set to the Depreciation Expense account of the fixed asset (<see cref="FixedAsset.DepreciatedExpenseAccountID"/>).
		/// </value>
		[PXDefault(typeof(Search<FixedAsset.depreciatedExpenseAccountID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>>>))]
		[Account(DisplayName = "Depreciation Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXUIRequired(typeof(Parent<FixedAsset.depreciable>))]
		public virtual Int32? DepreciatedExpenseAccountID
		{
			get
			{
				return this._DepreciatedExpenseAccountID;
			}
			set
			{
				this._DepreciatedExpenseAccountID = value;
			}
		}
		#endregion
		#region DepreciatedExpenseSubID
		public abstract class depreciatedExpenseSubID : PX.Data.BQL.BqlInt.Field<depreciatedExpenseSubID> { }
		protected Int32? _DepreciatedExpenseSubID;
		/// <summary>
		/// The Depreciation Expense subaccount associated with the Depreciation Expense account (<see cref="DepreciatedExpenseAccountID"/>).
		/// A reference to <see cref="Sub"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the subaccount. 
		/// This is a required field if <see cref="FixedAsset.Depreciable"/> flag is set to <c>true</c>. 
		/// By default, the value is set to the Depreciation Expense subaccount of the fixed asset (<see cref="FixedAsset.DepreciatedExpenseSubID"/>).
		/// </value>
		[PXDefault]
		[SubAccount(typeof(FALocationHistory.depreciatedExpenseAccountID), DisplayName = "Depreciation Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXUIRequired(typeof(Parent<FixedAsset.depreciable>))]
		public virtual Int32? DepreciatedExpenseSubID
		{
			get
			{
				return this._DepreciatedExpenseSubID;
			}
			set
			{
				this._DepreciatedExpenseSubID = value;
			}
		}
		#endregion

		#region DisposalAccountID
		public abstract class disposalAccountID : PX.Data.BQL.BqlInt.Field<disposalAccountID> { }
		/// <summary>
		/// The Disposal account.
		/// A reference to <see cref="Account"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the account. 
		/// By default, the value is set to the Disposal account of the fixed asset (<see cref="FixedAsset.DisposalAccountID"/>).
		/// </value>
		[PXDefault(typeof(Search<FixedAsset.disposalAccountID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>>>),
					PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Proceeds Account", DescriptionField = typeof(Account.description))]
		public virtual int? DisposalAccountID { get; set; }
		#endregion
		#region DisposalSubID
		public abstract class disposalSubID : PX.Data.BQL.BqlInt.Field<disposalSubID> { }
		/// <summary>
		/// The Disposal subaccount associated with the Disposal account (<see cref="DisposalAccountID"/>).
		/// A reference to <see cref="Sub"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the subaccount. 
		/// By default, the value is set to the Disposal subaccount of the fixed asset (<see cref="FixedAsset.DisposalSubID"/>).
		/// </value>
		[SubAccount(typeof(FALocationHistory.disposalAccountID), DisplayName = "Proceeds Sub.", DescriptionField = typeof(Sub.description))]
		public virtual int? DisposalSubID { get; set; }
		#endregion

		#region GainAcctID
		public abstract class gainAcctID : PX.Data.BQL.BqlInt.Field<gainAcctID> { }
		/// <summary>
		/// The Gain account.
		/// A reference to <see cref="Account"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the account. 
		/// This is a required field. 
		/// By default, the value is set to the Gain account of the fixed asset (<see cref="FixedAsset.GainAcctID"/>).
		/// </value>
		[PXDefault(typeof(Search<FixedAsset.gainAcctID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>>>))]
		[Account(DisplayName = "Gain Account", DescriptionField = typeof(Account.description))]
		public virtual int? GainAcctID { get; set; }
		#endregion
		#region GainSubID
		public abstract class gainSubID : PX.Data.BQL.BqlInt.Field<gainSubID> { }
		/// <summary>
		/// The Gain subaccount associated with the Gain account (<see cref="GainAccountID"/>).
		/// A reference to <see cref="Sub"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the subaccount. 
		/// This is a required field. 
		/// By default, the value is set to the Gain subaccount of the fixed asset (<see cref="FixedAsset.GainSubID"/>).
		/// </value>
		[PXDefault]
		[SubAccount(typeof(FALocationHistory.gainAcctID), DescriptionField = typeof(Sub.description), DisplayName = "Gain Sub.")]
		public virtual int? GainSubID { get; set; }
		#endregion

		#region LossAcctID
		public abstract class lossAcctID : PX.Data.BQL.BqlInt.Field<lossAcctID> { }
		/// <summary>
		/// The Loss account.
		/// A reference to <see cref="Account"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the account. 
		/// This is a required field. 
		/// By default, the value is set to the Loss account of the fixed asset (<see cref="FixedAsset.LossAcctID"/>).
		/// </value>
		[PXDefault(typeof(Search<FixedAsset.lossAcctID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>>>))]
		[Account(DisplayName = "Loss Account", DescriptionField = typeof(Account.description))]
		public virtual int? LossAcctID { get; set; }
		#endregion
		#region LossSubID
		public abstract class lossSubID : PX.Data.BQL.BqlInt.Field<lossSubID> { }
		/// <summary>
		/// The Loss subaccount associated with the Loss account (<see cref="LossAccountID"/>).
		/// A reference to <see cref="Sub"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the subaccount. 
		/// This is a required field. 
		/// By default, the value is set to the Loss subaccount of the fixed asset (<see cref="FixedAsset.LossSubID"/>).
		/// </value>
		[PXDefault]
		[SubAccount(typeof(FALocationHistory.lossAcctID), DescriptionField = typeof(Sub.description), DisplayName = "Loss Sub.")]
		public virtual int? LossSubID { get; set; }
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		/// <summary>
		/// The reference number of the related transfer document.
		/// A reference to <see cref="FARegister"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the fixed asset document. 
		/// By default, the value is set to the number of the current document.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXDBDefault(typeof(FARegister.refNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<FARegister.refNbr>))]
		[PXUIField(DisplayName = "Transfer Document Nbr.", Visible = false)]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region Reason
		public abstract class reason : PX.Data.BQL.BqlString.Field<reason> { }
		protected String _Reason;
		/// <summary>
		/// The reason of the change of the asset location.
		/// The information field, which value is entered manually.
		/// </summary>
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Reason")]
		public virtual String Reason
		{
			get
			{
				return this._Reason;
			}
			set
			{
				this._Reason = value;
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
		[PXDBLastModifiedByID]
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
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = "Modification Date")]
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
		#region IFALocation Members
		public virtual Int32? BranchID
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
	}
	
}
