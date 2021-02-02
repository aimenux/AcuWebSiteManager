using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;

namespace PX.Objects.FA
{
    public class DisposalProcess : PXGraph<DisposalProcess>
    {
        public PXCancel<DisposalFilter> Cancel;
        public PXFilter<DisposalFilter> Filter;
        [PXFilterable]
        [PX.SM.PXViewDetailsButton(typeof(FixedAsset.assetCD), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXFilteredProcessingJoin<FixedAsset, DisposalFilter, 
			InnerJoin<FADetails, On<FixedAsset.assetID, Equal<FADetails.assetID>>,
			LeftJoin<Account, On<FixedAsset.fAAccountID, Equal<Account.accountID>>>>> Assets;
        public PXSelect<FABookBalance> bookbalances;
        public PXSetup<FASetup> fasetup;
 
        public DisposalProcess()
        {
            fasetup.Current = null;
            FASetup setup = fasetup.Current;

            if (fasetup.Current.UpdateGL != true)
            {
				throw new PXSetupNotEnteredException<FASetup>(Messages.OperationNotWorkInInitMode, PXUIFieldAttribute.GetDisplayName<FASetup.updateGL>(fasetup.Cache));
			}

            PXUIFieldAttribute.SetDisplayName<Account.accountClassID>(Caches[typeof(Account)], Messages.FixedAssetsAccountClass);

            if(fasetup.Current.AutoReleaseDisp != true)
            {
                Assets.SetProcessCaption(Messages.Prepare);
                Assets.SetProcessAllCaption(Messages.PrepareAll);
            }
        }

        protected virtual IEnumerable assets()
        {
            DisposalFilter filter = Filter.Current;

            PXSelectBase<FixedAsset> cmd = new PXSelectJoin<FixedAsset, 
					InnerJoin<FADetails, On<FixedAsset.assetID, Equal<FADetails.assetID>>,
					LeftJoin<Account, On<FixedAsset.fAAccountID, Equal<Account.accountID>>>>, 
					Where<FADetails.status, NotEqual<FixedAssetStatus.disposed>, 
						And<FADetails.status, NotEqual<FixedAssetStatus.hold>, 
						And<FADetails.status, NotEqual<FixedAssetStatus.suspended>>>>>(this);

            if (filter.BookID != null)
            {
                cmd.Join<InnerJoin<FABookBalance, On<FABookBalance.assetID, Equal<FixedAsset.assetID>>>>();
                cmd.WhereAnd<Where<FABookBalance.bookID, Equal<Current<DisposalFilter.bookID>>>>();
            }

            if (filter.ClassID != null)
            {
                cmd.WhereAnd<Where<FixedAsset.classID, Equal<Current<DisposalFilter.classID>>>>();
            }
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>() || filter.OrgBAccountID != null)
			{
				cmd.WhereAnd<Where<FixedAsset.branchID, Inside<Current<DisposalFilter.orgBAccountID>>>>();
			}
			if (filter.ParentAssetID != null)
            {
                cmd.WhereAnd<Where<FixedAsset.parentAssetID, Equal<Current<DisposalFilter.parentAssetID>>>>();
            }

            int startRow = PXView.StartRow;
            int totalRows = 0;

            List<PXFilterRow> newFilters = new List<PXFilterRow>();
            foreach (PXFilterRow f in PXView.Filters)
            {
                if (f.DataField.ToLower() == "status")
                {
                    f.DataField = "FADetails__Status";
                }
                newFilters.Add(f);
            }
            List<object> list = cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, newFilters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows);
            PXView.StartRow = 0;
            return list;
        }

        protected virtual void DisposalFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            DisposalFilter filter = (DisposalFilter)e.Row;
            if (filter == null) return;

            Assets.SetProcessEnabled(filter.DisposalMethodID != null);
            Assets.SetProcessAllEnabled(filter.DisposalMethodID != null);
            PXProcessingStep[] targets = PXAutomation.GetProcessingSteps(this);
            if (targets.Length > 0)
            {
                Assets.SetProcessTarget(targets[0].GraphName,
                                        targets.Length > 1 ? null : targets[0].Name,
                                        targets[0].Actions[0].Name,
                                        targets[0].Actions[0].Menus[0],
                                        filter.DisposalDate,
                                        filter.DisposalPeriodID,
                                        filter.DisposalAmt,
                                        filter.DisposalMethodID,
                                        filter.DisposalAccountID,
                                        filter.DisposalSubID,
                                        filter.DisposalAmtMode,
                                        filter.DeprBeforeDisposal,
                                        filter.Reason);
            }
            else
            {
                throw new PXScreenMisconfigurationException(SO.Messages.MissingMassProcessWorkFlow);
            }

            PXUIFieldAttribute.SetEnabled<DisposalFilter.disposalAmt>(sender, e.Row, filter.DisposalAmtMode == DisposalFilter.disposalAmtMode.Automatic);
            PXUIFieldAttribute.SetEnabled<FixedAsset.disposalAmt>(Assets.Cache, null, Filter.Current.DisposalAmtMode == DisposalFilter.disposalAmtMode.Manual);
        }

        protected virtual void DisposalFilter_DisposalMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<DisposalFilter.disposalAccountID>(e.Row);
            sender.SetDefaultExt<DisposalFilter.disposalSubID>(e.Row);
        }

        [PXDBInt(IsKey = true)]
        [PXSelector(typeof(Search<FixedAsset.assetID>),
            SubstituteKey = typeof(FixedAsset.assetCD), CacheGlobal = true, DescriptionField = typeof(FixedAsset.description))]
        [PXUIField(DisplayName = "Asset ID", Enabled = false)]
        public virtual void FABookBalance_AssetID_CacheAttached(PXCache sender)
        {
        }

        protected virtual void FixedAsset_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            FixedAsset asset = (FixedAsset) e.Row;

			// AssetID can be <0 when the datasource inserts a temporary record on redirect from selector
			if (asset == null || asset.AssetID < 0) return;

            FADetails assetdet = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FADetails.assetID>>>>.Select(this, asset.AssetID);

            try
            {
                AssetProcess.ThrowDisabled_Dispose(this, asset, assetdet, fasetup.Current, (DateTime)Filter.Current.DisposalDate, Filter.Current.DisposalPeriodID, Filter.Current.DeprBeforeDisposal == true);
            }
            catch (PXException exc)
            {
                PXUIFieldAttribute.SetEnabled<FixedAsset.selected>(sender, asset, false);
                sender.RaiseExceptionHandling<FixedAsset.selected>(asset, null, new PXSetPropertyException(exc.MessageNoNumber, PXErrorLevel.RowWarning));
            }
            if (Filter.Current.DisposalAmtMode == DisposalFilter.disposalAmtMode.Manual && asset.Selected == true && asset.DisposalAmt == null)
            {
                sender.RaiseExceptionHandling<FixedAsset.disposalAmt>(asset, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<FixedAsset.disposalAmt>(sender)));
            }
        }

        [Serializable]
        public partial class DisposalFilter : ProcessAssetFilter
        {
			#region OrganizationID
			public new abstract class organizationID : IBqlField { }

			[Organization]
			public override int? OrganizationID
			{
				get;
				set;
			}
			#endregion
			#region BranchID
			public new abstract class branchID : IBqlField { }

			[BranchOfOrganization(
				organizationFieldType: typeof(DisposalFilter.organizationID),
				onlyActive: true,
				featureFieldType: typeof(FeaturesSet.multipleCalendarsSupport),
				IsDetail = false,
				PersistingCheck = PXPersistingCheck.Nothing)]
			public override int? BranchID
			{
				get;
				set;
			}
			#endregion

			#region OrgBAccountID
			public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }

			[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
			[PXUIRequired(typeof(Where<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>>))]
			public int? OrgBAccountID { get; set; }
			#endregion

			#region DisposalDate
			public abstract class disposalDate : PX.Data.BQL.BqlDateTime.Field<disposalDate> { }
            protected DateTime? _DisposalDate;
            [PXDBDate]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Disposal Date")]
            public virtual DateTime? DisposalDate
            {
                get
                {
                    return _DisposalDate;
                }
                set
                {
                    _DisposalDate = value;
                }
            }
            #endregion
            #region DisposalPeriodID
            public abstract class disposalPeriodID : PX.Data.BQL.BqlString.Field<disposalPeriodID> { }
            protected string _DisposalPeriodID;
            [PXUIField(DisplayName = "Disposal Period")]
			[FABookPeriodOpenInGLSelector(
				dateType: typeof(DisposalFilter.disposalDate),
				branchSourceType: typeof(DisposalFilter.branchID),
				organizationSourceType: typeof(DisposalFilter.organizationID),
				isBookRequired: false)]
            public virtual string DisposalPeriodID
            {
                get
                {
                    return _DisposalPeriodID;
                }
                set
                {
                    _DisposalPeriodID = value;
                }
            }
            #endregion
            #region DisposalAmt
            public abstract class disposalAmt : PX.Data.BQL.BqlDecimal.Field<disposalAmt> { }
            protected Decimal? _DisposalAmt;
            [PXDBBaseCury]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Proceeds Amount")]
            public virtual Decimal? DisposalAmt
            {
                get
                {
                    return _DisposalAmt;
                }
                set
                {
                    _DisposalAmt = value;
                }
            }
            #endregion
            #region DisposalMethodID
            public abstract class disposalMethodID : PX.Data.BQL.BqlInt.Field<disposalMethodID> { }
            protected Int32? _DisposalMethodID;
            [PXDBInt]
            [PXDefault]
            [PXSelector(typeof(Search<FADisposalMethod.disposalMethodID>),
                        SubstituteKey = typeof(FADisposalMethod.disposalMethodCD),
                        DescriptionField = typeof(FADisposalMethod.description))]
            [PXUIField(DisplayName = "Disposal Method", Required = true)]
            public virtual Int32? DisposalMethodID
            {
                get
                {
                    return _DisposalMethodID;
                }
                set
                {
                    _DisposalMethodID = value;
                }
            }
            #endregion
            #region DisposalAccountID
            public abstract class disposalAccountID : PX.Data.BQL.BqlInt.Field<disposalAccountID> { }
            protected Int32? _DisposalAccountID;
            [PXDefault(typeof(Coalesce<Search<FADisposalMethod.proceedsAcctID, Where<FADisposalMethod.disposalMethodID, Equal<Current<DisposalFilter.disposalMethodID>>>>,
                Search<FASetup.proceedsAcctID>>))]
            [Account(DisplayName = "Proceeds Account", DescriptionField = typeof(Account.description))]
            public virtual Int32? DisposalAccountID
            {
                get
                {
                    return _DisposalAccountID;
                }
                set
                {
                    _DisposalAccountID = value;
                }
            }
            #endregion
            #region DisposalSubID
            public abstract class disposalSubID : PX.Data.BQL.BqlInt.Field<disposalSubID> { }
            protected Int32? _DisposalSubID;
            [PXDefault(typeof(Coalesce<Search<FADisposalMethod.proceedsSubID, Where<FADisposalMethod.disposalMethodID, Equal<Current<DisposalFilter.disposalMethodID>>>>,
                Search<FASetup.proceedsSubID>>))]
            [SubAccount(typeof(DisposalFilter.disposalAccountID), typeof(DisposalFilter.branchID), DisplayName = "Proceeds Sub.", DescriptionField = typeof(Sub.description))]
            public virtual Int32? DisposalSubID
            {
                get
                {
                    return _DisposalSubID;
                }
                set
                {
                    _DisposalSubID = value;
                }
            }
            #endregion
            #region DisposalAmtMode
            public abstract class disposalAmtMode : PX.Data.BQL.BqlString.Field<disposalAmtMode>
            {
                public class ListAttribute : PXStringListAttribute
                {
                    public ListAttribute()
                        : base(
                        new[] { Automatic, Manual },
                        new[] { Messages.Automatic, Messages.Manual }) { }
                }

                public const string Automatic = "A";
                public const string Manual = "M";

                public class automatic : PX.Data.BQL.BqlString.Constant<automatic>
                {
                    public automatic() : base(Automatic) { }
                }
                public class manual : PX.Data.BQL.BqlString.Constant<manual>
                {
                    public manual() : base(Manual) { }
                }
            }
            protected String _DisposalAmtMode;
            [PXDBString(1, IsFixed = true)]
            [PXUIField(DisplayName = "Proceeds Allocation")]
            [disposalAmtMode.List]
            [PXDefault(disposalAmtMode.Automatic, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual String DisposalAmtMode
            {
                get
                {
                    return _DisposalAmtMode;
                }
                set
                {
                    _DisposalAmtMode = value;
                }
            }
            #endregion
            #region DeprBeforeDisposal
            public abstract class deprBeforeDisposal : PX.Data.BQL.BqlBool.Field<deprBeforeDisposal> { }
            protected bool? _DeprBeforeDisposal;
            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Depreciate Before Disposal")]
            public virtual bool? DeprBeforeDisposal
            {
                get
                {
                    return _DeprBeforeDisposal;
                }
                set
                {
                    _DeprBeforeDisposal = value;
                }
            }
            #endregion
            #region Reason
            public abstract class reason : PX.Data.BQL.BqlString.Field<reason> { }
            protected String _Reason;
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
        }
    }

    [Serializable]
    [PXHidden]
    public partial class ProcessAssetFilter : IBqlTable
    {
		#region OrganizationID
		public abstract class organizationID : IBqlField {}

		[Organization]
		public virtual int? OrganizationID
		{
			get;
			set;
		}
		#endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[BranchOfOrganization(
			organizationFieldType: typeof(ProcessAssetFilter.organizationID),
			onlyActive: true,
			featureFieldType: typeof(FeaturesSet.multipleCalendarsSupport),
			IsDetail = false,
			PersistingCheck = PXPersistingCheck.Nothing)]

		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected int? _BookID;
		[PXDBInt]
		[PXSelector(typeof(FABook.bookID),
					SubstituteKey = typeof(FABook.bookCode),
					DescriptionField = typeof(FABook.description))]
		[PXUIField(DisplayName = "Book")]
		public virtual int? BookID
        {
            get
            {
				return _BookID;
            }
            set
            {
				_BookID = value;
            }
        }
        #endregion
        #region ParentAssetID
        public abstract class parentAssetID : PX.Data.BQL.BqlInt.Field<parentAssetID> { }
        protected int? _ParentAssetID;
        [PXDBInt]
        [PXSelector(typeof(Search<FixedAsset.assetID, Where<FixedAsset.recordType, Equal<FARecordType.assetType>>>),
                    SubstituteKey = typeof(FixedAsset.assetCD),
                    DescriptionField = typeof(FixedAsset.description))]
        [PXUIField(DisplayName = "Parent Asset", Visibility = PXUIVisibility.Visible)]
        public virtual int? ParentAssetID
        {
            get
            {
                return _ParentAssetID;
            }
            set
            {
                _ParentAssetID = value;
            }
        }
        #endregion
        #region ClassID
        public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
        protected int? _ClassID;
        [PXDBInt]
        [PXSelector(typeof(Search<FixedAsset.assetID, Where<FixedAsset.recordType, Equal<FARecordType.classType>, And<FixedAsset.depreciable, Equal<True>>>>),
                    SubstituteKey = typeof(FixedAsset.assetCD),
                    DescriptionField = typeof(FixedAsset.description))]
        [PXUIField(DisplayName = "Asset Class", Visibility = PXUIVisibility.Visible, TabOrder = 3)]
        public virtual int? ClassID
        {
            get
            {
                return _ClassID;
            }
            set
            {
                _ClassID = value;
            }
        }
        #endregion
    }
}