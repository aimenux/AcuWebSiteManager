using System;
using System.Collections;

using PX.Data;

using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.GL;

namespace PX.Objects.DR
{
    public class DRDraftScheduleProc : PXGraph<DRRecognition>
    {
        public PXCancel<SchedulesFilter> Cancel;
        public PXFilter<SchedulesFilter> Filter;
		public PXAction<SchedulesFilter> viewSchedule;
		public PXSetup<DRSetup> Setup;

		[PXFilterable]
        public PXFilteredProcessing<DRScheduleDetail, SchedulesFilter> Items;

        public virtual IEnumerable items()
        {
            SchedulesFilter filter = this.Filter.Current;

	        if (filter != null)
	        {
				PXSelectBase<DRScheduleDetail> select = new PXSelectJoin<
					DRScheduleDetail,
					InnerJoin<DRSchedule,
						On<DRSchedule.scheduleID, Equal<DRScheduleDetail.scheduleID>>,
					InnerJoin<DRDeferredCode,
						On<DRDeferredCode.deferredCodeID, Equal<DRScheduleDetail.defCode>>>>,
					Where<
						DRDeferredCode.accountType, Equal<Current<DRDraftScheduleProc.SchedulesFilter.accountType>>,
						And<DRScheduleDetail.status, Equal<DRScheduleStatus.DraftStatus>,
						And<DRSchedule.isCustom, Equal<True>>>>,
					OrderBy<
						Asc<DRScheduleDetail.scheduleID,
						Asc<DRScheduleDetail.componentID,
						Asc<DRScheduleDetail.detailLineNbr>>>>>
					(this);

				BqlCommand command = select.View.BqlSelect;

				if (!string.IsNullOrEmpty(filter.DeferredCode))
		        {
					command = command.WhereAnd<Where<DRScheduleDetail.defCode, Equal<Current<SchedulesFilter.deferredCode>>>>();
		        }

				if (filter.BranchID != null)
				{
					command = command.WhereAnd<Where<DRScheduleDetail.branchID, Equal<Current<SchedulesFilter.branchID>>>>();
				}

		        if (filter.AccountID != null)
		        {
					command = command.WhereAnd<Where<DRScheduleDetail.defAcctID, Equal<Current<SchedulesFilter.accountID>>>>();
		        }

		        if (filter.SubID != null)
		        {
					command = command.WhereAnd<Where<DRScheduleDetail.defSubID, Equal<Current<SchedulesFilter.subID>>>>();
		        }

		        if (filter.BAccountID != null)
		        {
					command = command.WhereAnd<Where<DRScheduleDetail.bAccountID, Equal<Current<SchedulesFilter.bAccountID>>>>();
		        }

		        if (filter.ComponentID != null)
		        {
					command = command.WhereAnd<Where<DRScheduleDetail.componentID, Equal<Current<SchedulesFilter.componentID>>>>();
		        }

				return command.CreateView(this, mergeCache: !select.View.IsReadOnly).SelectExternalWithPaging();
			}
			else
	        {
		        return null;
	        }
        }
        
        public DRDraftScheduleProc()
		{
			DRSetup setup = Setup.Current;
			Items.SetSelected<DRScheduleDetail.selected>();
		}

		#region Actions

		[PXUIField(DisplayName = "", Visible = false)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewSchedule(PXAdapter adapter)
        {
            if (Items.Current != null)
            {
	            DRRedirectHelper.NavigateToDeferralSchedule(this, Items.Current.ScheduleID);
            }

            return adapter.Get();
        }

	    public PXAction<SchedulesFilter> viewDocument;
		[PXUIField(DisplayName = "")]
	    public virtual IEnumerable ViewDocument(PXAdapter adapter)
	    {
		    if (Items.Current != null)
		    {
			    DRRedirectHelper.NavigateToOriginalDocument(this, Items.Current);
		    }
		    return adapter.Get();
	    }

		#endregion

		#region Cache Attached Handlers

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), false)]
		protected virtual void DRScheduleDetail_DefAmt_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), false)]
		protected virtual void DRScheduleDetail_LineNbr_CacheAttached(PXCache sender) { }

		#endregion

		#region Event Handlers
		protected virtual void SchedulesFilter_AccountType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SchedulesFilter row = e.Row as SchedulesFilter;
            if (row != null)
            {
                row.BAccountID = null;
                row.DeferredCode = null;
                row.AccountID = null;
                row.SubID = null;
            }
        }
                
        protected virtual void SchedulesFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            Items.Cache.Clear();
        }

        protected virtual void SchedulesFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            Items.SetProcessDelegate(ReleaseCustomSchedule);
        }

        protected virtual void DRScheduleDetail_ComponentID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void DRScheduleDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            DR.DRScheduleDetail row = e.Row as DR.DRScheduleDetail;

            if (row != null)
            {
                row.DocumentType = DRScheduleDocumentType.BuildDocumentType(row.Module, row.DocType);
            }
        }
        #endregion

        public static void ReleaseCustomSchedule(DRScheduleDetail item)
        {
            ScheduleMaint maint = PXGraph.CreateInstance<ScheduleMaint>();

			var details = PXSelect<
					DRScheduleDetail,
					Where<DRScheduleDetail.scheduleID, Equal<Required<DRSchedule.scheduleID>>,
						And<DRScheduleDetail.isResidual, Equal<False>>>>
					.Select(maint, item.ScheduleID)
					.RowCast<DRScheduleDetail>();

			maint.FinPeriodUtils.ValidateFinPeriod<DRScheduleDetail>(details, m => item.FinPeriodID, m => m.BranchID.SingleToArray());

            maint.Clear();
            maint.Document.Current = PXSelect<
            	DRScheduleDetail,
            	Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
            		And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>,
            		And<DRScheduleDetail.detailLineNbr, Equal<Required<DRScheduleDetail.detailLineNbr>>>>>>
            	.Select(maint, item.ScheduleID, item.ComponentID, item.DetailLineNbr);

            maint.ReleaseCustomScheduleDetail();
        }

        #region Local Types
        [Serializable]
        public partial class SchedulesFilter : IBqlTable
        {
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
            #region AccountID
            public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
            protected Int32? _AccountID;
            [Account(DisplayName = "Deferral Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
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
            [SubAccount(typeof(SchedulesFilter.accountID), DisplayName = "Deferral Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
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
            #region DeferredCode
            public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
            protected String _DeferredCode;
            [PXDBString(10, InputMask = ">aaaaaaaaaa")]
            [PXUIField(DisplayName = "Deferral Code")]
            [PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.accountType, Equal<Current<SchedulesFilter.accountType>>>>))]
            [PXRestrictor(typeof(Where<DRDeferredCode.active, Equal<True>>), Messages.InactiveDeferralCode, typeof(DRDeferredCode.deferredCodeID))]
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
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			[Branch(PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual int? BranchID { get; set; }
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
					Where<BAccountR.type, Equal<Current<SchedulesFilter.bAccountType>>>>), 
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
            #region ComponentID
            public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
            protected Int32? _ComponentID;

            [Inventory(DisplayName = "Component")]
            public virtual Int32? ComponentID
            {
                get
                {
                    return this._ComponentID;
                }
                set
                {
                    this._ComponentID = value;
                }
            }

            #endregion
        }

        #endregion
    }
}
