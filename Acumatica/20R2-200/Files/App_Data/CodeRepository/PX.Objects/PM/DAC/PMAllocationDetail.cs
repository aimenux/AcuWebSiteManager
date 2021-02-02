namespace PX.Objects.PM
{
	using System;
	using PX.Data;
	using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.CS;
using System.Collections.Generic;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMAllocationDetail)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMAllocationDetail : PX.Data.IBqlTable
	{
		#region AllocationID
		public abstract class allocationID : PX.Data.BQL.BqlString.Field<allocationID> { }
		protected String _AllocationID;
		[PXDBString(PMAllocation.allocationID.Length, IsKey = true, IsUnicode = true)]
		[PXDefault(typeof(PMAllocation.allocationID))]
		[PXParent(typeof(Select<PMAllocation, Where<PMAllocation.allocationID, Equal<Current<PMAllocationDetail.allocationID>>>>))]
		public virtual String AllocationID
		{
			get
			{
				return this._AllocationID;
			}
			set
			{
				this._AllocationID = value;
			}
		}
		#endregion
		#region StepID
		public abstract class stepID : PX.Data.BQL.BqlInt.Field<stepID> { }
		protected Int32? _StepID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Step ID")]
		public virtual Int32? StepID
		{
			get
			{
				return this._StepID;
			}
			set
			{
				this._StepID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
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
		#region SelectOption
		public abstract class selectOption : PX.Data.BQL.BqlString.Field<selectOption> { }
		protected String _SelectOption;
		[PMSelectOption.List]
		[PXDefault(PMSelectOption.Transaction)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Select Transactions")]
		public virtual String SelectOption
		{
			get
			{
				return this._SelectOption;
			}
			set
			{
				this._SelectOption = value;
			}
		}
		#endregion
		#region Post
		public abstract class post : PX.Data.BQL.BqlBool.Field<post> { }
		protected Boolean? _Post;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Create Allocation Transaction")]
		public virtual Boolean? Post
		{
			get
			{
				return this._Post;
			}
			set
			{
				this._Post = value;
			}
		}
		#endregion
		#region QtyFormula
		public abstract class qtyFormula : PX.Data.BQL.BqlString.Field<qtyFormula> { }
		protected String _QtyFormula;
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Quantity Formula")]
		public virtual String QtyFormula
		{
			get
			{
				return this._QtyFormula;
			}
			set
			{
				this._QtyFormula = value;
			}
		}
		#endregion
		#region BillableQtyFormula
		public abstract class billableQtyFormula : PX.Data.BQL.BqlString.Field<billableQtyFormula> { }
		protected String _BillableQtyFormula;
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Billable Qty. Formula")]
		public virtual String BillableQtyFormula
		{
			get
			{
				return this._BillableQtyFormula;
			}
			set
			{
				this._BillableQtyFormula = value;
			}
		}
		#endregion
		#region AmountFormula
		public abstract class amountFormula : PX.Data.BQL.BqlString.Field<amountFormula> { }
		protected String _AmountFormula;
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Amount Formula")]
		public virtual String AmountFormula
		{
			get
			{
				return this._AmountFormula;
			}
			set
			{
				this._AmountFormula = value;
			}
		}
		#endregion
		#region DescriptionFormula
		public abstract class descriptionFormula : PX.Data.BQL.BqlString.Field<descriptionFormula> { }
		protected String _DescriptionFormula;
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Description Formula")]
		public virtual String DescriptionFormula
		{
			get
			{
				return this._DescriptionFormula;
			}
			set
			{
				this._DescriptionFormula = value;
			}
		}
		#endregion
		#region RangeStart
		public abstract class rangeStart : PX.Data.BQL.BqlInt.Field<rangeStart> { }
		protected Int32? _RangeStart;
		[PXDBInt()]
		[PXUIField(DisplayName = "Range Start")]
		public virtual Int32? RangeStart
		{
			get
			{
				return this._RangeStart;
			}
			set
			{
				this._RangeStart = value;
			}
		}
		#endregion
		#region RangeEnd
		public abstract class rangeEnd : PX.Data.BQL.BqlInt.Field<rangeEnd> { }
		protected Int32? _RangeEnd;
		[PXDBInt()]
		[PXUIField(DisplayName = "Range End")]
		public virtual Int32? RangeEnd
		{
			get
			{
				return this._RangeEnd;
			}
			set
			{
				this._RangeEnd = value;
			}
		}
		#endregion
        #region RateTypeID
        public abstract class rateTypeID : PX.Data.BQL.BqlString.Field<rateTypeID> { }
		protected String _RateTypeID;
		[PXDBString(PMRateType.rateTypeID.Length, IsUnicode = true)]
		[PXSelector(typeof(PMRateType.rateTypeID), DescriptionField = typeof(PMRateType.description))]
		[PXUIField(DisplayName="Rate Type")]
        public virtual String RateTypeID
		{
			get
			{
                return this._RateTypeID;
			}
			set
			{
                this._RateTypeID = value;
			}
		}
		#endregion
		#region AccountGroupFrom
		public abstract class accountGroupFrom : PX.Data.BQL.BqlInt.Field<accountGroupFrom> { }
		protected Int32? _AccountGroupFrom;
		[AccountGroup(DisplayName="Account Group From")]
		public virtual Int32? AccountGroupFrom
		{
			get
			{
				return this._AccountGroupFrom;
			}
			set
			{
				this._AccountGroupFrom = value;
			}
		}
		#endregion
		#region AccountGroupTo
		public abstract class accountGroupTo : PX.Data.BQL.BqlInt.Field<accountGroupTo> { }
		protected Int32? _AccountGroupTo;
		[AccountGroup(DisplayName = "Account Group To")]
		public virtual Int32? AccountGroupTo
		{
			get
			{
				return this._AccountGroupTo;
			}
			set
			{
				this._AccountGroupTo = value;
			}
		}
		#endregion
        #region Method
        public abstract class method : PX.Data.BQL.BqlString.Field<method> { }
        protected String _Method;
        [PMMethod.List]
        [PXDefault(PMMethod.Transaction)]
        [PXDBString(1)]
        [PXUIField(DisplayName = "Allocation Method")]
        public virtual String Method
        {
            get
            {
                return this._Method;
            }
            set
            {
                this._Method = value;
            }
        }
        #endregion
		#region UpdateGL
		public abstract class updateGL : PX.Data.BQL.BqlBool.Field<updateGL> { }
		protected Boolean? _UpdateGL;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Post Transaction to GL")]
		public virtual Boolean? UpdateGL
		{
			get
			{
				return this._UpdateGL;
			}
			set
			{
				this._UpdateGL = value;
			}
		}
		#endregion

		#region BranchOrigin
		public abstract class branchOrigin : PX.Data.BQL.BqlString.Field<branchOrigin> { }
		protected String _BranchOrigin;
		[PMOrigin.BranchFilterList]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Filter", FieldClass = "BRANCH")]
		public virtual String BranchOrigin
		{
			get
			{
				return this._BranchOrigin;
			}
			set
			{
				this._BranchOrigin = value;
			}
		}
		#endregion
		#region SourceBranchID
		public abstract class sourceBranchID : PX.Data.BQL.BqlInt.Field<sourceBranchID> { }
		protected Int32? _SourceBranchID;		
		[Branch(DisplayName = "Source Branch ID", IsDetail = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SourceBranchID
		{
			get
			{
				return this._SourceBranchID;
			}
			set
			{
				this._SourceBranchID = value;
			}
		}
		#endregion
		#region OffsetBranchOrigin
		public abstract class offsetBranchOrigin : PX.Data.BQL.BqlString.Field<offsetBranchOrigin> { }
		protected String _OffsetBranchOrigin;
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Branch", FieldClass = "BRANCH")]
		public virtual String OffsetBranchOrigin
		{
			get
			{
				return this._OffsetBranchOrigin;
			}
			set
			{
				this._OffsetBranchOrigin = value;
			}
		}
		#endregion
		#region TargetBranchID
		public abstract class targetBranchID : PX.Data.BQL.BqlInt.Field<targetBranchID> { }
		protected Int32? _TargetBranchID;		
		[Branch(DisplayName = "Target Branch ID", IsDetail = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? TargetBranchID
		{
			get
			{
				return this._TargetBranchID;
			}
			set
			{
				this._TargetBranchID = value;
			}
		}
		#endregion
		#region ProjectOrigin
		public abstract class projectOrigin : PX.Data.BQL.BqlString.Field<projectOrigin> { }
		protected String _ProjectOrigin;
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Project")]
		public virtual String ProjectOrigin
		{
			get
			{
				return this._ProjectOrigin;
			}
			set
			{
				this._ProjectOrigin = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[Project]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskOrigin
		public abstract class taskOrigin : PX.Data.BQL.BqlString.Field<taskOrigin> { }
		protected String _TaskOrigin;
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Task")]
		public virtual String TaskOrigin
		{
			get
			{
				return this._TaskOrigin;
			}
			set
			{
				this._TaskOrigin = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[ProjectTask(typeof(PMAllocationDetail.projectID), AllowNull=true )]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region TaskCD
		public abstract class taskCD : PX.Data.BQL.BqlString.Field<taskCD> { }
		protected String _TaskCD;
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TaskCD
		{
			get
			{
				return this._TaskCD;
			}
			set
			{
				this._TaskCD = value;
			}
		}
		#endregion
		#region AccountGroupOrigin
		public abstract class accountGroupOrigin : PX.Data.BQL.BqlString.Field<accountGroupOrigin> { }
		protected String _AccountGroupOrigin;
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Account Group")]
		public virtual String AccountGroupOrigin
		{
			get
			{
				return this._AccountGroupOrigin;
			}
			set
			{
				this._AccountGroupOrigin = value;
			}
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		[AccountGroup(typeof(Where<Current<PMAllocationDetail.updateGL>, Equal<True>,
		And<PMAccountGroup.type, NotEqual<PMAccountType.offBalance>,
		Or<Current<PMAllocationDetail.updateGL>, Equal<False>>>>), DisplayName = "Account Group")]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region AccountOrigin
		public abstract class accountOrigin : PX.Data.BQL.BqlString.Field<accountOrigin> { }
		protected String _AccountOrigin;
		[PMOrigin.DebitAccountListAttribute]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Account Origin")]
		public virtual String AccountOrigin
		{
			get
			{
				return this._AccountOrigin;
			}
			set
			{
				this._AccountOrigin = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(null, typeof(Search<Account.accountID, Where<Account.accountGroupID, IsNotNull>>), AvoidControlAccounts = true)]
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
		#region SubMask
		public abstract class subMask : PX.Data.BQL.BqlString.Field<subMask> { }
		protected String _SubMask;
		[PMSubAccountMask(DisplayName = "Subaccount")]
		public virtual String SubMask
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
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(typeof(PMAllocationDetail.accountID))]
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
		
		#region OffsetProjectOrigin
		public abstract class offsetProjectOrigin : PX.Data.BQL.BqlString.Field<offsetProjectOrigin> { }
		protected String _OffsetProjectOrigin;
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Project")]
		public virtual String OffsetProjectOrigin
		{
			get
			{
				return this._OffsetProjectOrigin;
			}
			set
			{
				this._OffsetProjectOrigin = value;
			}
		}
		#endregion
		#region OffsetProjectID
		public abstract class offsetProjectID : PX.Data.BQL.BqlInt.Field<offsetProjectID> { }
		protected Int32? _OffsetProjectID;
		[Project(DisplayName="Project")]
		public virtual Int32? OffsetProjectID
		{
			get
			{
				return this._OffsetProjectID;
			}
			set
			{
				this._OffsetProjectID = value;
			}
		}
		#endregion
		#region OffsetTaskOrigin
		public abstract class offsetTaskOrigin : PX.Data.BQL.BqlString.Field<offsetTaskOrigin> { }
		protected String _OffsetTaskOrigin;
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Task")]
		public virtual String OffsetTaskOrigin
		{
			get
			{
				return this._OffsetTaskOrigin;
			}
			set
			{
				this._OffsetTaskOrigin = value;
			}
		}
		#endregion
		#region OffsetTaskID
		public abstract class offsetTaskID : PX.Data.BQL.BqlInt.Field<offsetTaskID> { }
		protected Int32? _OffsetTaskID;
		[ProjectTask(typeof(PMAllocationDetail.offsetProjectID), AllowNull=true, DisplayName="Project Task")]
		public virtual Int32? OffsetTaskID
		{
			get
			{
				return this._OffsetTaskID;
			}
			set
			{
				this._OffsetTaskID = value;
			}
		}
		#endregion
		#region OffsetTaskCD
		public abstract class offsetTaskCD : PX.Data.BQL.BqlString.Field<offsetTaskCD> { }
		protected String _OffsetTaskCD;
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Project Task", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OffsetTaskCD
		{
			get
			{
				return this._OffsetTaskCD;
			}
			set
			{
				this._OffsetTaskCD = value;
			}
		}
		#endregion
		#region OffsetAccountGroupOrigin
		public abstract class offsetAccountGroupOrigin : PX.Data.BQL.BqlString.Field<offsetAccountGroupOrigin> { }
		protected String _OffsetAccountGroupOrigin;
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Account Group")]
		public virtual String OffsetAccountGroupOrigin
		{
			get
			{
				return this._OffsetAccountGroupOrigin;
			}
			set
			{
				this._OffsetAccountGroupOrigin = value;
			}
		}
		#endregion
		#region OffsetAccountGroupID
		public abstract class offsetAccountGroupID : PX.Data.BQL.BqlInt.Field<offsetAccountGroupID> { }
		protected Int32? _OffsetAccountGroupID;
		[AccountGroup(typeof(Where<Current<PMAllocationDetail.updateGL>, Equal<True>,
		And<PMAccountGroup.type, NotEqual<PMAccountType.offBalance>,
		Or<Current<PMAllocationDetail.updateGL>, Equal<False>>>>), DisplayName = "Account Group")]
		public virtual Int32? OffsetAccountGroupID
		{
			get
			{
				return this._OffsetAccountGroupID;
			}
			set
			{
				this._OffsetAccountGroupID = value;
			}
		}
		#endregion
		#region OffsetAccountOrigin
		public abstract class offsetAccountOrigin : PX.Data.BQL.BqlString.Field<offsetAccountOrigin> { }
		protected String _OffsetAccountOrigin;
		[PMOrigin.CreditAccountListAttribute]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Account Origin")]
		public virtual String OffsetAccountOrigin
		{
			get
			{
				return this._OffsetAccountOrigin;
			}
			set
			{
				this._OffsetAccountOrigin = value;
			}
		}
		#endregion
		#region OffsetAccountID
		public abstract class offsetAccountID : PX.Data.BQL.BqlInt.Field<offsetAccountID> { }
		protected Int32? _OffsetAccountID;
		[Account(null, typeof(Search<Account.accountID, Where<Account.accountGroupID, IsNotNull>>), DisplayName = "Account", AvoidControlAccounts = true)]
		public virtual Int32? OffsetAccountID
		{
			get
			{
				return this._OffsetAccountID;
			}
			set
			{
				this._OffsetAccountID = value;
			}
		}
		#endregion
		#region OffsetSubMask
		public abstract class offsetSubMask : PX.Data.BQL.BqlString.Field<offsetSubMask> { }
		protected String _OffsetSubMask;
		[PMSubAccountMask(DisplayName = "Subaccount")]
		public virtual String OffsetSubMask
		{
			get
			{
				return this._OffsetSubMask;
			}
			set
			{
				this._OffsetSubMask = value;
			}
		}
		#endregion
		#region OffsetSubID
		public abstract class offsetSubID : PX.Data.BQL.BqlInt.Field<offsetSubID> { }
		protected Int32? _OffsetSubID;
		[SubAccount(typeof(PMAllocationDetail.offsetAccountID), DisplayName="Subaccount")]
		public virtual Int32? OffsetSubID
		{
			get
			{
				return this._OffsetSubID;
			}
			set
			{
				this._OffsetSubID = value;
			}
		}
		#endregion

		#region Reverse
		public abstract class reverse : PX.Data.BQL.BqlString.Field<reverse> { }
		protected String _Reverse;
		[PMReverse.List]
		[PXDefault(PMReverse.OnInvoiceRelease)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Reverse Allocation")]
		public virtual String Reverse
		{
			get
			{
				return this._Reverse;
			}
			set
			{
				this._Reverse = value;
			}
		}
		#endregion
		#region NoRateOption
		public abstract class noRateOption : PX.Data.BQL.BqlString.Field<noRateOption> { }
		protected String _NoRateOption;
		[PMNoRateOption.AllocationList]
		[PXDefault(PMNoRateOption.SetOne)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "If @Rate is not defined")]
		public virtual String NoRateOption
		{
			get
			{
				return this._NoRateOption;
			}
			set
			{
				this._NoRateOption = value;
			}
		}
		#endregion
		#region DateSource
		public abstract class dateSource : PX.Data.BQL.BqlString.Field<dateSource> { }
		protected String _DateSource;
		[PMDateSource.List]
		[PXDefault(PMDateSource.Transaction)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Date Source")]
		public virtual String DateSource
		{
			get
			{
				return this._DateSource;
			}
			set
			{
				this._DateSource = value;
			}
		}
		#endregion
		
		#region GroupByItem
		public abstract class groupByItem : PX.Data.BQL.BqlBool.Field<groupByItem> { }
		protected Boolean? _GroupByItem;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Item")]
		public virtual Boolean? GroupByItem
		{
			get
			{
				return this._GroupByItem;
			}
			set
			{
				this._GroupByItem = value;
			}
		}
		#endregion
		#region GroupByEmployee
		public abstract class groupByEmployee : PX.Data.BQL.BqlBool.Field<groupByEmployee> { }
		protected Boolean? _GroupByEmployee;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Employee")]
		public virtual Boolean? GroupByEmployee
		{
			get
			{
				return this._GroupByEmployee;
			}
			set
			{
				this._GroupByEmployee = value;
			}
		}
		#endregion
		#region GroupByDate
		public abstract class groupByDate : PX.Data.BQL.BqlBool.Field<groupByDate> { }
		protected Boolean? _GroupByDate;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Date")]
		public virtual Boolean? GroupByDate
		{
			get
			{
				return this._GroupByDate;
			}
			set
			{
				this._GroupByDate = value;
			}
		}
		#endregion
		#region GroupByVendor
		public abstract class groupByVendor : PX.Data.BQL.BqlBool.Field<groupByVendor> { }
		protected Boolean? _GroupByVendor;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Vendor")]
		public virtual Boolean? GroupByVendor
		{
			get
			{
				return this._GroupByVendor;
			}
			set
			{
				this._GroupByVendor = value;
			}
		}
		#endregion
		#region FullDetail
		public abstract class fullDetail : PX.Data.BQL.BqlBool.Field<fullDetail> { }
		
		[PXBool()]
		public virtual Boolean? FullDetail
		{
			get
			{
				return GroupByItem != true && GroupByEmployee != true && GroupByDate != true && GroupByVendor != true;
			}
		}
		#endregion

		#region Allocation
		public abstract class allocation : PX.Data.BQL.BqlInt.Field<allocation> { }
		protected int? _Allocation;
		[PXInt]
		[PXUIField(DisplayName = "Allocation")]
		public virtual int? Allocation
		{
			get
			{
				return StepID;
			}
			set
			{
			}
		}
		#endregion
		#region AllocationText
		public abstract class allocationText : PX.Data.BQL.BqlString.Field<allocationText> { }
		protected String _AllocationText;
		[PXString(10)]
		public virtual String AllocationText
		{
			get
			{
				return this._AllocationText;
			}
			set
			{
				this._AllocationText = value;
			}
		}
		#endregion
		
		#region AllocateZeroAmount
		public abstract class allocateZeroAmount : PX.Data.BQL.BqlBool.Field<allocateZeroAmount> { }
		protected Boolean? _AllocateZeroAmount;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Create Transaction with Zero Amount")]
		public virtual Boolean? AllocateZeroAmount
		{
			get
			{
				return this._AllocateZeroAmount;
			}
			set
			{
				this._AllocateZeroAmount = value;
			}
		}
		#endregion
		#region AllocateZeroQty
		public abstract class allocateZeroQty : PX.Data.BQL.BqlBool.Field<allocateZeroQty> { }
		protected Boolean? _AllocateZeroQty;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Create Transaction with Zero Qty.")]
		public virtual Boolean? AllocateZeroQty
		{
			get
			{
				return this._AllocateZeroQty;
			}
			set
			{
				this._AllocateZeroQty = value;
			}
		}
		#endregion
		#region AllocateNonBillable
		public abstract class allocateNonBillable : PX.Data.BQL.BqlBool.Field<allocateNonBillable> { }
		protected Boolean? _AllocateNonBillable;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Allocate Non-Billable Transactions")]
		public virtual Boolean? AllocateNonBillable
		{
			get
			{
				return this._AllocateNonBillable;
			}
			set
			{
				this._AllocateNonBillable = value;
			}
		}
		#endregion

		#region MarkAsNotAllocated
		public abstract class markAsNotAllocated : PX.Data.BQL.BqlBool.Field<markAsNotAllocated> { }
		protected Boolean? _MarkAsNotAllocated;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Can be used as source in another allocation.")]
		public virtual Boolean? MarkAsNotAllocated
		{
			get
			{
				return this._MarkAsNotAllocated;
			}
			set
			{
				this._MarkAsNotAllocated = value;
			}
		}
		#endregion

		#region CopyNotes
		public abstract class copyNotes : PX.Data.BQL.BqlBool.Field<copyNotes> { }
		protected Boolean? _CopyNotes;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Notes", Visibility=PXUIVisibility.Visible, Visible=false)]
		public virtual Boolean? CopyNotes
		{
			get
			{
				return this._CopyNotes;
			}
			set
			{
				this._CopyNotes = value;
			}
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
        [PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
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
		[PXDBCreatedByID]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
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

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMOrigin
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Source, Messages.Origin_Source),
					Pair(Change, Messages.Origin_Change),
				}) {}
		}

		public class DebitAccountListAttribute : PXStringListAttribute
		{
			public DebitAccountListAttribute() : base(
				new[]
				{
					Pair(Source, Messages.Origin_Source),
					Pair(Change, Messages.Origin_Change),
					Pair(OtherSource, Messages.Origin_CreditSource),
				}) {}
		}

		public class CreditAccountListAttribute : PXStringListAttribute
		{
			public CreditAccountListAttribute() : base(
				new[]
				{
					Pair(Source, Messages.Origin_Source),
					Pair(Change, Messages.Origin_Change),
					Pair(OtherSource, Messages.Origin_DebitSource),
				}) {}
		}

		/// <summary>
		/// List of available Account Group sources. 
		/// Account Group can be taken either from Source object, from Account or specified directly.
		/// </summary>
		public class AccountGroupListAttribute : PXStringListAttribute
		{
			public AccountGroupListAttribute() : base(
				new[]
				{
					Pair(Source, Messages.Origin_Source),
					Pair(Change, Messages.Origin_Change),
					Pair(FromAccount, Messages.Origin_FromAccount),
				}) {}
		}

        public class BranchFilterListAttribute : PXStringListAttribute
        {
            public BranchFilterListAttribute() : base(
                new[]
                {
                    Pair(Source, Messages.Origin_None),
                    Pair(Change, Messages.Origin_Branch),
                })
            { }
        }

        public const string Source = "S";
		public const string Change = "C";
		public const string FromAccount = "F";
		public const string None = "N";
		public const string OtherSource = "X";
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMMethod
    {
	    public class ListAttribute : PXStringListAttribute
	    {
		    public ListAttribute() : base(
			    new[]
				{
					Pair(Transaction, Messages.PMMethod_Transaction),
					Pair(Budget, Messages.PMMethod_Budget),
				}) {}
	    }

	    public const string Transaction = "T";
        public const string Budget = "B";

        public class transaction : PX.Data.BQL.BqlString.Constant<transaction>
		{
            public transaction() : base(Transaction) { ;}
        }

        public class budget : PX.Data.BQL.BqlString.Constant<budget>
		{
            public budget() : base(Budget) { ;}
        }

    }


	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMReverse
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(OnInvoiceRelease, Messages.PMReverse_OnARInvoiceRelease),
					Pair(OnInvoiceGeneration, Messages.PMReverse_OnARInvoiceGeneration),
					Pair(Never, Messages.PMReverse_Never),
				}) {}
		}

		public const string OnInvoiceRelease = "I";
		public const string OnInvoiceGeneration = "B";
		public const string Never = "N";
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMNoRateOption
	{
		public class AllocationListAttribute : PXStringListAttribute
		{
			public AllocationListAttribute():base(
				new[]
				{
					Pair(SetOne, Messages.PMNoRateOption_SetOne),
					Pair(SetZero, Messages.PMNoRateOption_SetZero),
					Pair(RaiseError, Messages.PMNoRateOption_RaiseError),
					Pair(DontAllocate, Messages.PMNoRateOption_NoAllocate),
				}){}
		}

		public class BillingListAttribute : PXStringListAttribute
		{
			public BillingListAttribute() : base(
				new[]
				{
					Pair(SetOne, Messages.PMNoRateOption_SetOne),
					Pair(SetZero, Messages.PMNoRateOption_SetZero),
					Pair(RaiseError, Messages.PMNoRateOption_RaiseError),
					Pair(DontAllocate, Messages.PMNoRateOption_NoBill),
				})
			{ }
		}


		public const string SetOne = "1";
		public const string SetZero = "0";
		public const string RaiseError = "E";
		public const string DontAllocate = "N";

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMDateSource
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Transaction, Messages.PMDateSource_Transaction),
					Pair(Allocation, Messages.PMDateSource_Allocation),
				}) {}
		}

		public const string Transaction = "T";
		public const string Allocation = "A";
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMSelectOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Transaction, Messages.PMSelectOption_Transaction),
					Pair(Step, Messages.PMSelectOption_Step),
				}) {}
		}

		public const string Transaction = "T";
		public const string Step = "S";

		public class transaction : PX.Data.BQL.BqlString.Constant<transaction>
		{
			public transaction() : base(Transaction) { ;}
		}

		public class step : PX.Data.BQL.BqlString.Constant<step>
		{
			public step() : base(Step) { ;}
		}

	}
}
