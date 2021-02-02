using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.GL
{
	[Serializable]
	public partial class GLHistoryEnqFilter : IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(
			onlyActive: false, 
			Required = false)]
		public int? OrganizationID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[BranchOfOrganization(
			typeof(GLHistoryEnqFilter.organizationID), 
			onlyActive: false,
			Required = false)]
		public int? BranchID { get; set; }
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : IBqlField { }

		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
		public int? OrgBAccountID { get; set; }
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[PXDefault()]
        [FinPeriodSelector(null,
		//[AnyPeriodFilterable(null, 
			typeof(AccessInfo.businessDate),
			branchSourceType: typeof(GLHistoryEnqFilter.branchID),
			organizationSourceType: typeof(GLHistoryEnqFilter.organizationID),
			useMasterCalendarSourceType: typeof(GLHistoryEnqFilter.useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
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

		#region UseMasterCalendar
		public abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }

		[PXBool]
		[PXUIField(DisplayName = Common.Messages.UseMasterCalendar)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.multipleCalendarsSupport>))]
		public bool? UseMasterCalendar { get; set; }
		#endregion

		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }

		[PXDBInt]
		[PXDefault(
			typeof(Coalesce<Coalesce<
				Search<Organization.actualLedgerID, 
					Where<Organization.organizationID, Equal<Current2<GLHistoryEnqFilter.organizationID>>>>,
				Search<Branch.ledgerID, 
					Where<Branch.branchID, Equal<Current2<GLHistoryEnqFilter.branchID>>>>>,
				Search<Branch.ledgerID,
					Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Ledger.ledgerID), DescriptionField = typeof(Ledger.descr), SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual int? LedgerID { get; set; }
		#endregion

		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[AccountAny] //Allow YtdNetIncome be visible 
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
		//[PXDBInt()]
		[SubAccount(Visibility = PXUIVisibility.Visible)]
		[PXDefault()]
		//[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.Invisible)]
		//[PXSelector(typeof(Sub.subID), DescriptionField = typeof(Sub.description), SubstituteKey = typeof(Sub.subCD))]
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
		[SubAccountRestrictedRaw(DisplayName = "Subaccount",SuppressValidation=true)]
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
		
		#region BegFinPeriod
		public abstract class begFinPeriod : PX.Data.BQL.BqlString.Field<begFinPeriod> { }
		public virtual String BegFinPeriod
		{
			get
			{
				if (this._FinPeriodID != null)
					return FirstPeriodOfYear(FinPeriodUtils.FiscalYear(this._FinPeriodID));
				else
					return null;
			}
		}
		#endregion BegFinPeriod
		#region SubCD Wildcard 
		public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubCDWildcard
		{
			[PXDependsOnFields(typeof(subCD))]
			get
			{
				return SubCDUtils.CreateSubCDWildcard(this._SubCD, SubAccountAttribute.DimensionName);
			}
		}

		
	
		#endregion
		#region ShowCuryDetails
		public abstract class showCuryDetail : PX.Data.BQL.BqlBool.Field<showCuryDetail> { }
		protected bool? _ShowCuryDetail;
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "Show Currency Details", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ShowCuryDetail
		{
			get
			{
				return this._ShowCuryDetail;
			}
			set
			{
				this._ShowCuryDetail = value;
			}
		}
		#endregion
		#region AccountClassID
		public abstract class accountClassID : PX.Data.BQL.BqlString.Field<accountClassID> { }
		protected string _AccountClassID;
		[PXDBString(20, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Class", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(AccountClass.accountClassID))]
		public virtual string AccountClassID
		{
			get
			{
				return this._AccountClassID;
			}
			set
			{
				this._AccountClassID = value;
			}
		}
		#endregion
		#region Internal Functions 
		protected static string FirstPeriodOfYear(string year) 
		{
			return string.Concat(year, CS_FIRST_PERIOD);
		}
		private const string CS_FIRST_PERIOD = "01";
		#endregion 
	}
}
