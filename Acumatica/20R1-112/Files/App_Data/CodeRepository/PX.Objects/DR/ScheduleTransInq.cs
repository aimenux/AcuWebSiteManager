using System;
using System.Collections;

using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.Attributes;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.Common.Tools;

namespace PX.Objects.DR
{
	[TableAndChartDashboardType]
	public class ScheduleTransInq : PXGraph<ScheduleTransInq>
	{
		public PXCancel<ScheduleTransFilter> Cancel;
		public PXFilter<ScheduleTransFilter> Filter;
		public PXSelectJoin<
			DRScheduleTran,
			InnerJoin<DRScheduleDetail, 
				On<DRScheduleTran.scheduleID, Equal<DRScheduleDetail.scheduleID>, 
				And<DRScheduleTran.componentID, Equal<DRScheduleDetail.componentID>,
				And<DRScheduleTran.detailLineNbr, Equal<DRScheduleDetail.detailLineNbr>>>>,
			InnerJoin<DRDeferredCode, 
				On<DRDeferredCode.deferredCodeID, Equal<DRScheduleDetail.defCode>>,
			LeftJoin<InventoryItem, 
				On<InventoryItem.inventoryID, Equal<DRScheduleDetail.componentID>>>>>,
					Where<DRDeferredCode.accountType, Equal<Current<ScheduleTransFilter.accountType>>,
					And<DRScheduleTran.status, Equal<DRScheduleTranStatus.PostedStatus>,
				And<DRScheduleTran.finPeriodID, Equal<Current<ScheduleTransFilter.finPeriodID>>>>>>
			Records;

		public virtual IEnumerable records()
		{
			ScheduleTransFilter filter = this.Filter.Current;
			if (filter != null)
			{
				PXSelectBase<DRScheduleTran> select = new PXSelectJoin<DRScheduleTran,
					InnerJoin<DRScheduleDetail, On<DRScheduleTran.scheduleID, Equal<DRScheduleDetail.scheduleID>,
						And<DRScheduleTran.componentID, Equal<DRScheduleDetail.componentID>,
						And<DRScheduleTran.detailLineNbr, Equal<DRScheduleDetail.detailLineNbr>>>>,
					InnerJoin<DRDeferredCode, On<DRDeferredCode.deferredCodeID, Equal<DRScheduleDetail.defCode>>,
					LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<DRScheduleDetail.componentID>>>>>,
					Where<DRDeferredCode.accountType, Equal<Current<ScheduleTransFilter.accountType>>,
						And<DRScheduleTran.status, Equal<DRScheduleTranStatus.PostedStatus>>>>(this);

				if (!string.IsNullOrEmpty(filter.DeferredCode))
				{
					select.WhereAnd<Where<DRScheduleDetail.defCode, Equal<Current<ScheduleTransFilter.deferredCode>>>>();
				}

				if (filter.UseMasterCalendar == true)
				{
					select.WhereAnd<Where<DRScheduleTran.tranPeriodID, Equal<Current<ScheduleTransFilter.finPeriodID>>>>();
				}
				else
				{
					select.WhereAnd<Where<DRScheduleTran.finPeriodID, Equal<Current<ScheduleTransFilter.finPeriodID>>>>();
				}

				if (filter.OrgBAccountID != null)
				{
					select.WhereAnd<Where<DRScheduleTran.branchID, Inside<Current<ScheduleTransFilter.orgBAccountID>>>>(); //MatchWithOrg
				}
				
				if (filter.AccountID != null)
				{
					select.WhereAnd<Where<DRScheduleTran.accountID, Equal<Current<ScheduleTransFilter.accountID>>>>();
				}

				if (filter.SubID != null)
				{
					select.WhereAnd<Where<DRScheduleTran.subID, Equal<Current<ScheduleTransFilter.subID>>>>();
				}

				if (filter.BAccountID != null)
				{
					select.WhereAnd<Where<DRScheduleDetail.bAccountID, Equal<Current<ScheduleTransFilter.bAccountID>>>>();
				}

				foreach (object x in this.QuickSelect(select.View.BqlSelect))
				{
					yield return x;
				}
			}
			else yield break;
		}

		public PXAction<ScheduleTransFilter> previousPeriod;
		public PXAction<ScheduleTransFilter> nextPeriod;
		
		public PXSetup<DRSetup> Setup;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public ScheduleTransInq()
		{
			DRSetup setup = Setup.Current;
		}

		public PXAction<ScheduleTransFilter> viewDoc;
		[PXUIField(DisplayName = "")]
		[PXButton]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			if (Records.Current != null)
			{
				DRRedirectHelper.NavigateToOriginalDocument(this, Records.Current);
			}

			return adapter.Get();
		}

		public PXAction<ScheduleTransFilter> viewBatch;
		[PXUIField(DisplayName = "")]
		[PXButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			JournalEntry target = PXGraph.CreateInstance<JournalEntry>();
			target.Clear();
			Batch batch = PXSelect<Batch, Where<Batch.module, Equal<BatchModule.moduleDR>, And<Batch.batchNbr, Equal<Current<DRScheduleTran.batchNbr>>>>>.Select(this);
			if (batch != null)
			{
				target.BatchModule.Current = batch;
				throw new PXRedirectRequiredException(target, true, "ViewBatch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}
		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable PreviousPeriod(PXAdapter adapter)
		{
			ScheduleTransFilter filter = Filter.Current as ScheduleTransFilter;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);
			FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.FinPeriodID, looped: true);
			filter.FinPeriodID = prevPeriod != null ? prevPeriod.FinPeriodID : null;

			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextPeriod(PXAdapter adapter)
		{
			ScheduleTransFilter filter = Filter.Current as ScheduleTransFilter;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);
			FinPeriod nextPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.FinPeriodID, looped: true);
			filter.FinPeriodID = nextPeriod != null ? nextPeriod.FinPeriodID : null;

			return adapter.Get();
		}

		#region Cache Attached
		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), nameof(PXDefaultAttribute.PersistingCheck), PXPersistingCheck.Nothing)]
		protected virtual void DRScheduleTran_RecDate_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), nameof(PXDefaultAttribute.PersistingCheck), PXPersistingCheck.Nothing)]
		protected virtual void DRScheduleTran_AccountID_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), nameof(PXDefaultAttribute.PersistingCheck), PXPersistingCheck.Nothing)]
		protected virtual void DRScheduleTran_SubID_CacheAttached(PXCache sender) { }
		#endregion

		protected virtual void ScheduleTransFilter_AccountType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ScheduleTransFilter row = e.Row as ScheduleTransFilter;
			if (row != null)
			{
				row.BAccountID = null;
				row.DeferredCode = null;
				row.AccountID = null;
				row.SubID = null;
			}
		}


		#region Local Types
		[Serializable]
		public partial class ScheduleTransFilter : IBqlTable
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

			[BranchOfOrganization(typeof(ScheduleTransFilter.organizationID), false, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual int? BranchID { get; set; }
			#endregion
			#region OrgBAccountID
			public abstract class orgBAccountID : IBqlField { }

			[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
			public int? OrgBAccountID { get; set; }
			#endregion
			#region AccountType
			public abstract class accountType : PX.Data.BQL.BqlString.Field<accountType> { }
			protected string _AccountType;
			[PXDBString(1)]
			[PXDefault(DeferredAccountType.Income)]
			[LabelList(typeof(DeferredAccountType))]
			[PXUIField(DisplayName = "Code Type", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string AccountType
			{
				get
				{
					return this._AccountType;
				}
				set
				{
					this._AccountType = value;
					switch (value)
					{
						case DeferredAccountType.Expense:
							_BAccountType = CR.BAccountType.VendorType;
							break;
						default:
							_BAccountType = CR.BAccountType.CustomerType;
							break;
					}
				}
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			protected String _FinPeriodID;
			[PXDefault]
			[FinPeriodSelector(null,
				null,
				branchSourceType: typeof(ScheduleTransFilter.branchID),
				organizationSourceType: typeof(ScheduleTransFilter.organizationID),
				useMasterCalendarSourceType: typeof(ScheduleTransFilter.useMasterCalendar),
				redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
			[PXUIField(DisplayName = "Fin. Period")]
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
			#region DeferredCode
			public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
			protected String _DeferredCode;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Deferral Code")]
            [PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.accountType, Equal<Current<ScheduleTransFilter.accountType>>>>),
                        typeof(DRDeferredCode.deferredCodeID),
                        typeof(DRDeferredCode.description),
                        typeof(DRDeferredCode.accountType),
                        typeof(DRDeferredCode.accountID),
                        typeof(DRDeferredCode.subID),
                        typeof(DRDeferredCode.method),
                        typeof(DRDeferredCode.active))]
            public virtual String DeferredCode
			{
				get
				{
					return this._DeferredCode;
				}
				set
				{
					this._DeferredCode = value;
				}
			}
			#endregion

			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			protected Int32? _AccountID;
			[Account(DisplayName = "Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
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
			[SubAccount(typeof(ScheduleTransFilter.accountID), DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
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
			#region BAccountType
			public abstract class bAccountType : PX.Data.BQL.BqlString.Field<bAccountType> { }
			protected String _BAccountType;
			[PXDefault(CR.BAccountType.CustomerType)]
			[PXString(2, IsFixed = true)]
			[PXStringList(new string[] { CR.BAccountType.VendorType, CR.BAccountType.CustomerType },
					new string[] { CR.Messages.VendorType, CR.Messages.CustomerType })]
			public virtual String BAccountType
			{
				get
				{
					return this._BAccountType;
				}
				set
				{
					this._BAccountType = value;
				}
			}
			#endregion
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBInt]
			[PXUIField(DisplayName = Messages.BusinessAccount)]
			[PXSelector(
				typeof(Search<
					BAccountR.bAccountID, 
					Where<BAccountR.type, Equal<Current<ScheduleTransFilter.bAccountType>>>>), 
				new Type[] 
				{
					typeof(BAccountR.acctCD),
					typeof(BAccountR.acctName),
					typeof(BAccountR.type)
				}, 
				SubstituteKey = typeof(BAccountR.acctCD))]
			public virtual int? BAccountID
			{
				get;
				set;
			}
			#endregion

			#region UseMasterCalendar
			public abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }

			[PXDBBool]
			[PXUIField(DisplayName = Common.Messages.UseMasterCalendar, FieldClass = "MultipleCalendarsSupport")]
			public bool? UseMasterCalendar { get; set; }
			#endregion
		}

		#endregion
	}
}
