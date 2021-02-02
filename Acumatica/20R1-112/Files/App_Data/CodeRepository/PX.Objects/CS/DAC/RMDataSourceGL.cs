using System;
using PX.Data;
using PX.Objects.PM;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.FinPeriods;
using PX.CS;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.CS
{
	[Serializable]
	public partial class RMDataSourceGL : PXCacheExtension<RMDataSource>
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(false, null, Required = false, PersistingCheck =PXPersistingCheck.Nothing)]
		public virtual int? OrganizationID { get; set; }
		#endregion
		#region UseMasterCalendar
		public abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }
		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Use Master Calendar", Visibility = PXUIVisibility.Invisible)]		
		public bool? UseMasterCalendar { get; set; }
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;
		[PXDBInt]
		[PXUIField(DisplayName = "Ledger")]
		[PXSelector(typeof(GL.Ledger.ledgerID), SubstituteKey = typeof(GL.Ledger.ledgerCD), DescriptionField = typeof(GL.Ledger.descr), SelectorMode = PXSelectorMode.DisplayModeValue)]
		public virtual Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion		
		#region StartAccount
		public abstract class startAccount : PX.Data.BQL.BqlString.Field<startAccount> { }
		protected string _StartAccount;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Start Account")]
		[PXSelector(typeof(GL.Account.accountCD), DescriptionField = typeof(GL.Account.description), ValidateValue = false, SelectorMode = PXSelectorMode.DisplayModeValue)]
		public virtual string StartAccount
		{
			get
			{
				return this._StartAccount;
			}
			set
			{
				this._StartAccount = value;
			}
		}
		#endregion
		#region StartSub
		public abstract class startSub : PX.Data.BQL.BqlString.Field<startSub> { }
		protected string _StartSub;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Start Sub.")]
		[PXSelector(typeof(GL.Sub.subCD), DescriptionField = typeof(GL.Sub.description), ValidateValue = false, SelectorMode = PXSelectorMode.DisplayModeValue)]
		public virtual string StartSub
		{
			get
			{
				return this._StartSub;
			}
			set
			{
				this._StartSub = value;
			}
		}
		#endregion
		#region StartBranch
		public abstract class startBranch : PX.Data.BQL.BqlString.Field<startBranch> { }

		[BranchCDOfOrganization(typeof(RMDataSourceGL.organizationID), false, DisplayName = "Start Branch", DescriptionField = typeof(CR.BAccount.acctName), SelectorMode = PXSelectorMode.DisplayModeValue)]
		public virtual string StartBranch { get; set; }
		#endregion
		#region StartPeriod
		public abstract class startPeriod : PX.Data.BQL.BqlString.Field<startPeriod> { }
		protected string _StartPeriod;
		[GL.FinPeriodSelector(DescriptionField = typeof(FinPeriod.descr), SelectorMode = PXSelectorMode.DisplayModeValue)]
		[PXUIField(DisplayName = "Start Period")]
		public virtual string StartPeriod
		{
			get
			{
				return this._StartPeriod;
			}
			set
			{
				this._StartPeriod = value;
			}
		}
		#endregion		
		#region EndPeriod
		public abstract class endPeriod : PX.Data.BQL.BqlString.Field<endPeriod> { }
		protected string _EndPeriod;
		[GL.FinPeriodSelector(DescriptionField = typeof(FinPeriod.descr), SelectorMode = PXSelectorMode.DisplayModeValue)]
		[PXUIField(DisplayName = "End Period")]
		public virtual string EndPeriod
		{
			get
			{
				return this._EndPeriod;
			}
			set
			{
				this._EndPeriod = value;
			}
		}
		#endregion		
		#region AccountClassID
		public abstract class accountClassID : PX.Data.BQL.BqlString.Field<accountClassID> { }
		protected string _AccountClassID;
		[PXDBString(20, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Class")]
		[PXSelector(typeof(GL.AccountClass.accountClassID), DescriptionField = typeof(GL.AccountClass.descr), SelectorMode = PXSelectorMode.DisplayModeValue)]
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
		#region EndAccount
		public abstract class endAccount : PX.Data.BQL.BqlString.Field<endAccount> { }
		protected string _EndAccount;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "End Account")]
		[PXSelector(typeof(GL.Account.accountCD), DescriptionField = typeof(GL.Account.description), ValidateValue = false, SelectorMode = PXSelectorMode.DisplayModeValue)]
		public virtual string EndAccount
		{
			get
			{
				return this._EndAccount;
			}
			set
			{
				this._EndAccount = value;
			}
		}
		#endregion				
		#region EndSub
		public abstract class endSub : PX.Data.BQL.BqlString.Field<endSub> { }
		protected string _EndSub;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "End Sub.")]
		[PXSelector(typeof(GL.Sub.subCD), DescriptionField = typeof(GL.Sub.description), ValidateValue = false, SelectorMode = PXSelectorMode.DisplayModeValue)]
		public virtual string EndSub
		{
			get
			{
				return this._EndSub;
			}
			set
			{
				this._EndSub = value;
			}
		}
		#endregion			
		#region EndBranch
		public abstract class endBranch : PX.Data.BQL.BqlString.Field<endBranch> { }

		[BranchCDOfOrganization(typeof(RMDataSourceGL.organizationID), false, DisplayName = "End Branch", DescriptionField = typeof(CR.BAccount.acctName), SelectorMode = PXSelectorMode.DisplayModeValue)]
		public virtual string EndBranch{ get; set; }
		#endregion
		#region StartPeriodYearOffset
		public abstract class startPeriodYearOffset : PX.Data.BQL.BqlShort.Field<startPeriodYearOffset> { }
		protected short? _StartPeriodYearOffset;
		[PXDBShort]
		[PXUIField(DisplayName = "Offset (Year, Period)")]//[PXUIField(DisplayName = "Year Offset")]
		public virtual short? StartPeriodYearOffset
		{
			get
			{
				return this._StartPeriodYearOffset;
			}
			set
			{
				this._StartPeriodYearOffset = value;
			}
		}
		#endregion
		#region StartPeriodOffset
		public abstract class startPeriodOffset : PX.Data.BQL.BqlShort.Field<startPeriodOffset> { }
		protected short? _StartPeriodOffset;
		[PXDBShort]
		[PXUIField(DisplayName = "")] //[PXUIField(DisplayName = "Offset (Year, Period)")]
		public virtual short? StartPeriodOffset
		{
			get
			{
				return this._StartPeriodOffset;
			}
			set
			{
				this._StartPeriodOffset = value;
			}
		}
		#endregion
		#region EndPeriodYearOffset
		public abstract class endPeriodYearOffset : PX.Data.BQL.BqlShort.Field<endPeriodYearOffset> { }
		protected short? _EndPeriodYearOffset;
		[PXDBShort]
		[PXUIField(DisplayName = "Offset (Year, Period)")]//[PXUIField(DisplayName = "Year Offset")]
		public virtual short? EndPeriodYearOffset
		{
			get
			{
				return this._EndPeriodYearOffset;
			}
			set
			{
				this._EndPeriodYearOffset = value;
			}
		}
		#endregion
		#region EndPeriodOffset
		public abstract class endPeriodOffset : PX.Data.BQL.BqlShort.Field<endPeriodOffset> { }
		protected short? _EndPeriodOffset;
		[PXDBShort]
		[PXUIField(DisplayName = "")]//[PXUIField(DisplayName = "Offset (Year, Period)")]		
		public virtual short? EndPeriodOffset
		{
			get
			{
				return this._EndPeriodOffset;
			}
			set
			{
				this._EndPeriodOffset = value;
			}
		}
		#endregion		
	}
}
