using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.FA;
using PX.Objects.IN;
using PX.Objects.Common;
using static PX.Objects.Common.FinPeriodClosingProcessBase;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.Formula;
using PX.Objects.GL.Attributes;

namespace PX.Objects.GL
{

	[Serializable]
	public partial class TranBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}

	[Serializable]
	public partial class AdjustingBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}

	public partial class AdjustedBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}

	[Serializable]
	public partial class CashAccountBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}

	[Serializable]
	public partial class CAExpenseBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}
	[Serializable]
	public partial class CASplitBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}

	[Serializable]
	public partial class LineBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}

	[Serializable]
	public partial class INSiteBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}

	[Serializable]
	public partial class INSiteToBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}

	[Serializable]
	public partial class TranINSiteBranch : Branch
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
	}

	[Serializable]
	public partial class INSiteTo : INSite
	{
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
	}

	[Serializable]
	public partial class TranINSite : INSite
	{
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
	}

	public class FinPeriodStatusProcess : PXGraph<FinPeriodStatusProcess>
	{
		#region Internal DACs
		[Serializable]
		public class FinPeriodStatusProcessParameters : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			[Organization(
				true,
				typeof(Switch<Case<Where<Not<FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>>>,
					OrganizationOfBranch<Current<AccessInfo.branchID>>>>))]
			public virtual int? OrganizationID { get; set; }
			#endregion

			#region FilterOrganizationID
			public abstract class filterOrganizationID : PX.Data.BQL.BqlInt.Field<filterOrganizationID> { }

			[PXInt]
			[PXFormula(typeof(IIf<Where<FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>>,
				FinPeriod.organizationID.masterValue,
				organizationID>))]
			public virtual int? FilterOrganizationID { get; set; }
			#endregion

			#region Action
			public abstract class action : PX.Data.BQL.BqlString.Field<action>
			{
				public class RestrictedListAttribute : PXStringListAttribute
				{
					public RestrictedListAttribute() : base(
						new string[] { Undefined, Open, Close, Lock, Deactivate },
						new string[] { PXAutomationMenuAttribute.Undefined, Messages.ActionOpen, Messages.ActionClose, Messages.ActionLock, Messages.ActionDeactivate })
					{ }
				}

				public class FullListAttribute : PXStringListAttribute
				{
					public FullListAttribute() : base(
						new string[] { Undefined, Open, Close, Lock, Deactivate, Reopen, Unlock },
						new string[] { PXAutomationMenuAttribute.Undefined, Messages.ActionOpen, Messages.ActionClose, Messages.ActionLock, Messages.ActionDeactivate, Messages.ActionReopen, Messages.ActionUnlock })
					{ }
				}

				public const string UnknownActionMessage = "Unknown financial period action";

				public const string Undefined = "Undefined";
				public const string Open = "Open";
				public const string Close = "Close";
				public const string Lock = "Lock";
				public const string Deactivate = "Deactivate";
				public const string Reopen = "Reopen";
				public const string Unlock = "Unlock";

				public class undefined : PX.Data.BQL.BqlString.Constant<undefined> { public undefined() : base(Undefined) { } }
				public class open : PX.Data.BQL.BqlString.Constant<open> { public open() : base(Open) { } }
				public class close : PX.Data.BQL.BqlString.Constant<close> { public close() : base(Close) { } }
				public class @lock : PX.Data.BQL.BqlString.Constant<@lock> { public @lock() : base(Lock) { } }
				public class deactivate : PX.Data.BQL.BqlString.Constant<deactivate> { public deactivate() : base(Deactivate) { } }
				public class reopen : PX.Data.BQL.BqlString.Constant<reopen> { public reopen() : base(Reopen) { } }
				public class unlock : PX.Data.BQL.BqlString.Constant<unlock> { public unlock() : base(Unlock) { } }

				public enum Direction { Direct, Reverse }
				public static Direction GetDirection(string action)
				{
					switch (action)
					{
						case Open:
						case Close:
						case Lock:
							return Direction.Direct;
						case Unlock:
						case Reopen:
						case Deactivate:
							return Direction.Reverse;
						default:
							throw new PXException(UnknownActionMessage);
					}
				}

				public static string GetApplicableStatus(string action)
				{
					switch (action)
					{
						case Deactivate:
							return FinPeriod.status.Inactive;
						case Open:
						case Reopen:
							return FinPeriod.status.Open;
						case Close:
						case Unlock:
							return FinPeriod.status.Closed;
						case Lock:
							return FinPeriod.status.Locked;
						default:
							throw new PXException(UnknownActionMessage);
					}

				}
			}

			[PXString(10)]
			[PXUIField(DisplayName = "Action")]
			[action.FullList]
			[PXDefault(action.Undefined)]
			public virtual string Action { get; set; }
			#endregion

			#region AffectedStatus
			public abstract class affectedStatus : PX.Data.BQL.BqlString.Field<affectedStatus> { }
			[PXString(8)]
			[PXFormula(typeof(Switch
				<Case<
					Where<FinPeriodStatusProcessParameters.action, Equal<FinPeriodStatusProcessParameters.action.open>>, 
					FinPeriod.status.inactive,
				Case<
					Where<FinPeriodStatusProcessParameters.action, Equal<FinPeriodStatusProcessParameters.action.deactivate>,
						Or<FinPeriodStatusProcessParameters.action, Equal<FinPeriodStatusProcessParameters.action.close>>>, 
					FinPeriod.status.open,
				Case<
					Where<FinPeriodStatusProcessParameters.action, Equal<FinPeriodStatusProcessParameters.action.reopen>,
						Or<FinPeriodStatusProcessParameters.action, Equal<FinPeriodStatusProcessParameters.action.@lock>>>, 
					FinPeriod.status.closed,
				Case<
					Where<FinPeriodStatusProcessParameters.action, Equal<FinPeriodStatusProcessParameters.action.unlock>>, 
					FinPeriod.status.locked>>>>>))]
			public virtual string AffectedStatus { get; set; }
			#endregion

			#region ReopenInSubledgers
			public abstract class reopenInSubledgers : PX.Data.BQL.BqlBool.Field<reopenInSubledgers> { }
			[PXBool]
			[PXDefault(false)]
			[PXUIVisible(typeof(Where<Current<action>, Equal<action.reopen>>))]
			[PXUIField(DisplayName = "Reopen Financial Periods in All Modules")]
			public virtual Boolean? ReopenInSubledgers
			{
				get;
				set;
			}
			#endregion

			#region FirstYear
			public abstract class firstYear : PX.Data.BQL.BqlString.Field<firstYear> { }
			[PXString(4, IsFixed = true)]
			[PXFormula(typeof(Default<filterOrganizationID, action>))]
			[PXDefault(typeof(Search2<
				FinYear.year,
				InnerJoin<FinPeriod,
					On<FinYear.organizationID, Equal<FinPeriod.organizationID>,
					And<FinYear.year, Equal<FinPeriod.finYear>>>>,
				Where<FinYear.organizationID, Equal<Current<FinPeriodStatusProcessParameters.filterOrganizationID>>,
					And<FinPeriod.status, Equal<Current<FinPeriodStatusProcessParameters.affectedStatus>>>>,
				OrderBy<Asc<FinYear.year>>>))]
			public virtual string FirstYear { get; set; }
			#endregion

			#region LastYear
			public abstract class lastYear : PX.Data.BQL.BqlString.Field<lastYear> { }
			[PXString(4, IsFixed = true)]
			[PXFormula(typeof(Default<filterOrganizationID, action>))]
			[PXDefault(typeof(Search2<
				FinYear.year,
				InnerJoin<FinPeriod,
					On<FinYear.organizationID, Equal<FinPeriod.organizationID>,
					And<FinYear.year, Equal<FinPeriod.finYear>>>>,
				Where<FinYear.organizationID, Equal<Current<FinPeriodStatusProcessParameters.filterOrganizationID>>,
					And<FinPeriod.status, Equal<Current<FinPeriodStatusProcessParameters.affectedStatus>>>>,
				OrderBy<Desc<FinYear.year>>>))]
			public virtual string LastYear { get; set; }
			#endregion

			#region FromYear
			public abstract class fromYear : PX.Data.BQL.BqlString.Field<fromYear> { }
			[PXString(4, IsFixed = true)]
			[PXFormula(typeof(IIf2<Where<IsDirectFinPeriodAction<action>, Equal<True>>, firstYear, lastYear>))]
			[PXUIEnabled(typeof(Where<IsDirectFinPeriodAction<action>, NotEqual<True>>))]
			[PXUIField(DisplayName = "From Year", Required = true)]
			[PXDefault]
			[PXSelector(typeof(Search<
				FinYear.year,
				Where<FinYear.organizationID, Equal<Current<filterOrganizationID>>,
					And2<Where<FinYear.year, GreaterEqual<Current<firstYear>>, Or<Current<firstYear>, IsNull>>,
					And<Where<FinYear.year, LessEqual<Current<lastYear>>, Or<Current<lastYear>, IsNull>>>>>>))]
			public virtual string FromYear { get; set; }
			#endregion

			#region ToYear
			public abstract class toYear : PX.Data.BQL.BqlString.Field<toYear> { }
			[PXString(4, IsFixed = true)]
			[PXFormula(typeof(IIf2<Where<IsDirectFinPeriodAction<action>, Equal<True>>, firstYear, lastYear>))]
			[PXUIEnabled(typeof(Where<IsDirectFinPeriodAction<action>, Equal<True>>))]
			[PXUIField(DisplayName = "To Year", Required = true)]
			[PXDefault]
			[PXSelector(typeof(Search<
				FinYear.year,
				Where<FinYear.organizationID, Equal<Current<filterOrganizationID>>,
					And2<Where<FinYear.year, GreaterEqual<Current<firstYear>>, Or<Current<firstYear>, IsNull>>,
					And<Where<FinYear.year, LessEqual<Current<lastYear>>, Or<Current<lastYear>, IsNull>>>>>>))]
			public virtual string ToYear { get; set; }
			#endregion

		}

		[Serializable]
		[PXHidden]
		public partial class AutoReverseBatch : Batch
		{
			public new abstract class origBatchNbr : PX.Data.BQL.BqlString.Field<origBatchNbr> { }
			public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
			public new abstract class autoReverseCopy : PX.Data.BQL.BqlBool.Field<autoReverseCopy> { }
		}
		#endregion

		public PXCancel<FinPeriodStatusProcessParameters> Cancel;

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		#region Data Members
		public PXFilter<FinPeriodStatusProcessParameters> Filter;
		public PXFilteredProcessing<
			FinPeriod, 
			FinPeriodStatusProcessParameters, 
			Where<FinPeriod.organizationID, Equal<Current<FinPeriodStatusProcessParameters.filterOrganizationID>>,
				And<FinPeriod.finYear, GreaterEqual<Current<FinPeriodStatusProcessParameters.fromYear>>,
				And<FinPeriod.finYear, LessEqual<Current<FinPeriodStatusProcessParameters.toYear>>,
				And<FinPeriod.status, Equal<Current<FinPeriodStatusProcessParameters.affectedStatus>>>>>>> 
			FinPeriods;

		public PXSelect<
			OrganizationFinPeriod,
			Where<OrganizationFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>
			OrganizationFinPeriods;

		public PXSetup<GLSetup> glsetup;

		public FinPeriodStatusProcess()
		{
			GLSetup setup = glsetup.Current;
		}

		public IEnumerable<FinPeriod> SelectedItems => FinPeriods
				.Cache
				.Updated
				.Cast<FinPeriod>()
				.Where(p => p.Selected == true);
		#endregion

		protected virtual List<(string subledgerPrefix, Type closingGraphType)> SubledgerClosingGraphTypes { get; } = new List<(string, Type)>
		{
			( BatchModule.AP, typeof(APClosingProcess) ),
			( BatchModule.AR, typeof(ARClosingProcess) ),
			( BatchModule.CA, typeof(CAClosingProcess) ),
			( BatchModule.FA, typeof(FAClosingProcess) ),
			( BatchModule.IN, typeof(INClosingProcess) ),
		};

		private List<(string subledgerPrefix, Type closingFlag)> _subledgerClosingFlags = null;
		protected virtual List<(string subledgerPrefix, Type closingFlag)> SubledgerClosingFlags
		{
			get
			{
				if(_subledgerClosingFlags == null)
				{
					_subledgerClosingFlags = new List<(string, Type)>
					{
						(BatchModule.AP, typeof(FinPeriod.aPClosed) ),
						(BatchModule.AR, typeof(FinPeriod.aRClosed) ),
						(BatchModule.CA, typeof(FinPeriod.cAClosed) ),
					};

					if(PXAccess.FeatureInstalled<FeaturesSet.fixedAsset>())
					{
						_subledgerClosingFlags.Add((BatchModule.FA, typeof(FinPeriod.fAClosed)));
					}
					if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
					{
						_subledgerClosingFlags.Add((BatchModule.IN, typeof(FinPeriod.iNClosed)));
					}
				}
				return _subledgerClosingFlags;
			}
		}
		
		#region Actions
		public PXAction<FinPeriodStatusProcessParameters> ShowDocuments;
		[PXUIField(DisplayName = Messages.ShowDocuments, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(VisibleOnProcessingResults = true)]
		public virtual IEnumerable showDocuments(PXAdapter adapter)
		{
			ShowOpenDocuments(SelectedItems);
			return adapter.Get();
		}
		#endregion

		protected virtual IPXResultset GetResultset(BqlCommand command, int? organizationID, string fromPeriodID, string toPeriodID) =>
		new PXReportResultset(new PXView(this, true, command).SelectMultiBound(new object[]
		{
			new UnprocessedObjectsQueryParameters
			{
				OrganizationID = organizationID,
				FromFinPeriodID = fromPeriodID,
				ToFinPeriodID = toPeriodID
			}
		}));

		protected virtual List<(string ReportID, IPXResultset ReportData)> GetReportsData(string fromPeriodID, string toPeriodID) =>
			CheckingRules
				.Select(checker => (ReportID: checker.ReportID, ReportData: GetResultset(checker.CheckCommand, Filter.Current.OrganizationID, fromPeriodID, toPeriodID)))
				.Where(tuple => (tuple.ReportData?.GetRowCount() ?? 0) > 0)
				.ToList();

		protected virtual void ShowOpenDocuments(IEnumerable<FinPeriod> periods)
		{
			ParallelQuery<string> periodIDs = periods.Select(fp => fp.FinPeriodID).AsParallel();
			string fromFinPeriodID = periodIDs.Min();
			string toFinPeriodID = periodIDs.Max();

			List<(string ReportID, IPXResultset ReportData)> reports = SubledgerClosingGraphTypes
				.Select(tuple => CreateInstance(tuple.closingGraphType))
				.OfType<FinPeriodClosingProcessBase>()
				.Where(closingGraph => closingGraph.NeedValidate)
				.SelectMany(closingGraph =>
					{
						if (closingGraph is ARClosingProcess arClosingProcess 
							&& Filter.Current.Action == FinPeriodStatusProcessParameters.action.Close)
						{
							arClosingProcess.ExcludePendingProcessingDocs = true;
						}
						return closingGraph.GetReportsData(Filter.Current.OrganizationID, fromFinPeriodID, toFinPeriodID);
					})
				.ToList();

			reports.AddRange(GetReportsData(fromFinPeriodID, toFinPeriodID));

			if (reports.Any())
			{
				(string reportID, IPXResultset reportData) = reports.First();

			    PXReportRequiredException report = new PXReportRequiredException(new PXReportRedirectParameters()
			        {
			            ResultSet = reportData,
			            ReportParameters = new Dictionary<string, string>
			            {
			                {"OrganizationID",OrganizationMaint.FindOrganizationByID(this, periods.First().OrganizationID)?.OrganizationCD},
			                {"FromPeriodID", FinPeriodIDFormattingAttribute.FormatForDisplay(periodIDs.Min())},
			                {"ToPeriodID", FinPeriodIDFormattingAttribute.FormatForDisplay(periodIDs.Max())}
			            }
			        },
			        reportID);

				reports
					.Where(tuple => tuple.ReportID != reportID)
					.ForEach(tuple => report.AddSibling(tuple.ReportID, new PXReportRedirectParameters
				        {
				            ResultSet = tuple.ReportData,
				            ReportParameters = new Dictionary<string, string>
				            {
				                {"OrganizationID",OrganizationMaint.FindOrganizationByID(this, periods.First().OrganizationID)?.OrganizationCD},
				                {"FromPeriodID", FinPeriodIDFormattingAttribute.FormatForDisplay(periodIDs.Min())},
				                {"ToPeriodID", FinPeriodIDFormattingAttribute.FormatForDisplay(periodIDs.Max())}
				            }
                    }));

				throw report;
			}
			else
			{
				Filter.Ask(Messages.NoUnpostedDocumentsForPeriods, MessageButtons.OK);
			}
		}

		#region Events
		// Prevent selector verification while the formula chain is not fully completed and first/last years range is temporary invalid
		protected virtual void _(Events.FieldVerifying<FinPeriodStatusProcessParameters.fromYear> e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		// Prevent selector verification while the formula chain is not fully completed and first/last years range is temporary invalid
		protected virtual void _(Events.FieldVerifying<FinPeriodStatusProcessParameters.toYear> e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowSelected<FinPeriodStatusProcessParameters> e)
		{
			FinPeriodStatusProcess graph = this.Clone();

			FinPeriods.SetProcessDelegate(graph.ProcessStatus);
			FinPeriods.SetParametersDelegate(ConfirmProcessing);

			ShowDocuments.SetEnabled(SelectedItems.Any());

			bool isReverseActionAvailable = FinPeriodUtils.CanPostToClosedPeriod();

			PXStringListAttribute.SetList<FinPeriodStatusProcessParameters.action>(
				e.Cache,
				e.Row,
				isReverseActionAvailable
					? new FinPeriodStatusProcessParameters.action.FullListAttribute() as PXStringListAttribute
					: new FinPeriodStatusProcessParameters.action.RestrictedListAttribute() as PXStringListAttribute);
		}

		protected virtual void _(Events.RowUpdated<FinPeriodStatusProcessParameters> e)
		{
			if (!Filter.Cache.ObjectsEqual<FinPeriodStatusProcessParameters.organizationID,
											FinPeriodStatusProcessParameters.action,
											FinPeriodStatusProcessParameters.fromYear,
											FinPeriodStatusProcessParameters.toYear>(e.Row, e.OldRow))
			{
				FinPeriods.Cache.Clear();
			}
		}

		protected virtual void _(Events.FieldUpdated<FinPeriodStatusProcessParameters, FinPeriodStatusProcessParameters.action> e)
		{
			FinPeriods.Cache.Clear();
		}

		protected virtual void _(Events.FieldUpdated<FinPeriod, FinPeriod.selected> e)
		{
			if (!(e.Row is FinPeriod currentPeriod)) return;

			if (e.ExternalCall)
			{
				bool isSelected = currentPeriod.Selected == true;
				bool isDirectAction = FinPeriodStatusProcessParameters.action.GetDirection(Filter.Current.Action) == FinPeriodStatusProcessParameters.action.Direction.Direct;

				FinPeriods
					.Select()
					.RowCast<FinPeriod>()
					.Where(p => (isSelected && isDirectAction || !isSelected && !isDirectAction) && isSelected != (p.Selected ?? false)
						? string.CompareOrdinal(p.FinPeriodID, currentPeriod.FinPeriodID) < 0
						: string.CompareOrdinal(p.FinPeriodID, currentPeriod.FinPeriodID) > 0)
					.ForEach(p =>
					{
						p.Selected = isSelected;
						FinPeriods.Cache.MarkUpdated(p);
					});

				FinPeriods.View.RequestRefresh();
			}
		}
		#endregion

		#region Functions

		protected virtual void CloseFinPeriodInSubledgers(FinPeriod period)
		{
			HashSet<string> subledgerTitles = new HashSet<string>();
			foreach((string subledgerPrefix, Type closingGraphType) in SubledgerClosingGraphTypes)
			{
				if (CreateInstance(closingGraphType) is FinPeriodClosingProcessBase closingGraph &&
					((bool)FinPeriods.Cache.GetValue(period, closingGraph.ClosedFieldName) != true))
				{
					try
					{
						closingGraph.ClosePeriod(period);
					}
					catch (PXException)
					{
						subledgerTitles.Add(this.GetSubledgerTitle(subledgerPrefix));
					}
				}
			}

			if (subledgerTitles.Any())
			{
				MarkProcessedRowAsErrorAndStop(
					Messages.FinPeriodCanNotBeClosedInSubledgers,
					FinPeriodIDFormattingAttribute.FormatForError(period.FinPeriodID),
					string.Join(", ", subledgerTitles));
			}
		}

		protected virtual void ReopenFinPeriodInSubledgers(FinPeriod period)
		{
			foreach ((string subledgerPrefix, Type closingGraphType) in SubledgerClosingGraphTypes)
			{
				if (CreateInstance(closingGraphType) is FinPeriodClosingProcessBase closingGraph &&
					((bool)FinPeriods.Cache.GetValue(period, closingGraph.ClosedFieldName) == true))
				{
					closingGraph.ReopenPeriod(period);
				}
			}
		}

		protected virtual void VerifyFinPeriodForLock(FinPeriod period)
		{
			HashSet<string> subledgerTitles = new HashSet<string>();
			foreach ((string subledgerPrefix, Type closingGraphType) in SubledgerClosingGraphTypes)
			{
				if (CreateInstance(closingGraphType) is FinPeriodClosingProcessBase closingGraph)
				{
					if(closingGraph.IsUnclosablePeriod(period))
					{
						subledgerTitles.Add(this.GetSubledgerTitle(subledgerPrefix));
					}
				}
			}

			if(IsUnclosablePeriod(period))
			{
				subledgerTitles.Add(this.GetSubledgerTitle<BatchModule.moduleGL>());
			}

			if (subledgerTitles.Any())
			{
				MarkProcessedRowAsErrorAndStop(
					Messages.FinPeriodCanNotBeLockedInSubledgers,
					FinPeriodIDFormattingAttribute.FormatForError(period.FinPeriodID),
					string.Join(", ", subledgerTitles));
			}
		}

		
		private string GetSubledgerTitle(string subledgerPrefix) => BatchModule.GetDisplayName(subledgerPrefix);
		private string GetSubledgerTitle<TSubledgerConst>() where TSubledgerConst : IConstant<string>, IBqlOperand, new() => BatchModule.GetDisplayName<TSubledgerConst>();

		protected virtual HashSet<string> GetSubledgerTitles(List<FinPeriod> finPeriods) => SubledgerClosingFlags
			.Where(tuple => finPeriods.Any(period => (bool?)this.Caches<FinPeriod>().GetValue(period, tuple.closingFlag.Name) != true))
			.Select(tuple => this.GetSubledgerTitle(tuple.subledgerPrefix))
			.ToHashSet();

		protected virtual bool ConfirmProcessing(List<FinPeriod> finPeriods)
		{
			FinPeriodStatusProcessParameters parameters = Filter.Current;
			switch (parameters.Action)
			{
				case FinPeriodStatusProcessParameters.action.Close:

					HashSet<string> subledgerTitles = GetSubledgerTitles(finPeriods);

					if(subledgerTitles.Any())
					{
						string message = string.Format(PXMessages.LocalizeNoPrefix(Messages.ConfirmClosingInSubledgers), string.Join(", ", subledgerTitles), this.GetSubledgerTitle<BatchModule.moduleGL>());
						WebDialogResult result = Filter.Ask(message, MessageButtons.OKCancel);
						return result == WebDialogResult.OK;
					}

					break;
				case FinPeriodStatusProcessParameters.action.Reopen:
					break;
			}
			return true;
		}

		protected virtual void ProcessStatus(List<FinPeriod> finPeriods)
		{
			FinPeriodStatusProcessParameters parameters = Filter.Current;

			if (parameters.Action == FinPeriodStatusProcessParameters.action.Reopen
				&& parameters.ReopenInSubledgers == true)
			{
				foreach(FinPeriod finPeriod in PXSelect<FinPeriod, 
					Where<FinPeriod.organizationID, Equal<Current<FinPeriodStatusProcessParameters.filterOrganizationID>>,
						And<FinPeriod.finPeriodID, Greater<Required<FinPeriod.finPeriodID>>,
						And<Where<FinPeriod.aPClosed, Equal<True>,
							Or<FinPeriod.aRClosed, Equal<True>,
							Or<FinPeriod.cAClosed, Equal<True>,
							Or<FinPeriod.fAClosed, Equal<True>,
							Or<FinPeriod.iNClosed, Equal<True>>>>>>>>>>.Select(this, finPeriods.Last().FinPeriodID))
				{
					ReopenFinPeriodInSubledgers(finPeriod);
				}
			}

			string applicableStatus = FinPeriodStatusProcessParameters.action.GetApplicableStatus(parameters.Action);
			foreach (FinPeriod finPeriod in finPeriods)
			{
				PXProcessing.SetCurrentItem(finPeriod);

				switch (parameters.Action)
				{
					case FinPeriodStatusProcessParameters.action.Close:
						CloseFinPeriodInSubledgers(finPeriod);
						break;
					case FinPeriodStatusProcessParameters.action.Lock:
						VerifyFinPeriodForLock(finPeriod);
						break;
					case FinPeriodStatusProcessParameters.action.Reopen:
						if (parameters.ReopenInSubledgers == true)
						{
							ReopenFinPeriodInSubledgers(finPeriod);
						}
						break;
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
				{
					foreach (OrganizationFinPeriod organizationFinPeriod in OrganizationFinPeriods
						.Select(finPeriod.FinPeriodID))
					{
						AdditionalOrganizationFinPeriodProcessing(parameters.Action, organizationFinPeriod);

						organizationFinPeriod.Status = applicableStatus;
						OrganizationFinPeriods.Update(organizationFinPeriod);
					}
				}
				else
				{
					AdditionalOrganizationFinPeriodProcessing(parameters.Action, finPeriod);
				}

				finPeriod.Status = applicableStatus;
				FinPeriods.Update(finPeriod);

				PXProcessing.SetProcessed();
				Actions.PressSave();
			}
		}

		protected virtual void CreateAutoreverseBatches(IFinPeriod period)
		{
			PostGraph pg = CreateInstance<PostGraph>();

			foreach (Batch batch in PXSelectJoin<
				Batch,
				InnerJoin<Branch,
					On<Batch.branchID, Equal<Branch.branchID>>,
				LeftJoin<AutoReverseBatch,
					On<AutoReverseBatch.origModule, Equal<Batch.module>,
					And<AutoReverseBatch.origBatchNbr, Equal<Batch.batchNbr>,
					And<AutoReverseBatch.autoReverseCopy, Equal<True>>>>>>,
				Where<Batch.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>,
					And<Branch.organizationID, Equal<Required<FinPeriod.organizationID>>,
					And<Batch.autoReverse, Equal<True>,
					And<Batch.released, Equal<True>,
					And<AutoReverseBatch.origBatchNbr, IsNull>>>>>>
				.Select(this, period.FinPeriodID, period.OrganizationID))
			{
				pg.Clear();
				Batch copy = pg.ReverseBatchProc(batch);
				pg.ReleaseBatchProc(copy);

				if (glsetup.Current.AutoPostOption == true)
				{
					pg.PostBatchProc(copy);
				}
			}
		}

		protected static BqlCommand OpenBatchesQuery { get; } =
			PXSelectJoin<
				Batch,
				InnerJoin<Branch,
					On<Batch.branchID, Equal<Branch.branchID>>,
				LeftJoin<GLTran,
					On<Batch.module, Equal<GLTran.module>,
					And<Batch.batchNbr, Equal<GLTran.batchNbr>>>,
				LeftJoin<TranBranch,
					On<GLTran.branchID, Equal<TranBranch.branchID>>,
				LeftJoin<GLTranDoc, On<Batch.batchNbr, Equal<GLTranDoc.refNbr>,
					And<GLTranDoc.tranModule, Equal<BatchModule.moduleGL>>>>>>>,
				Where2<
					Where2<WhereFinPeriodInRange<Batch.finPeriodID, Branch.organizationID>,
						Or<WhereFinPeriodInRange<GLTran.finPeriodID, TranBranch.organizationID>>>,
					And<Where<Batch.posted, NotEqual<True>, // unposted
							And<Batch.released, Equal<True>,
						Or<Batch.released, NotEqual<True>, // unreleased
							And<Batch.scheduled, NotEqual<True>,
							And<Batch.voided, NotEqual<True>>>>>>>>>
			.GetCommand();

		protected virtual UnprocessedObjectsCheckingRule[] CheckingRules { get; } = new UnprocessedObjectsCheckingRule[]
		{
			new UnprocessedObjectsCheckingRule
			{
				ReportID = "GL656100",
				ErrorMessage = Messages.PeriodHasUnpostedBatches,
				CheckCommand = OpenBatchesQuery
			},
		};

		protected virtual void VerifyOpenBatches(IFinPeriod finPeriod)
		{
			FinPeriodStatusProcess.MarkProcessedRowAsErrorAndStop(CheckOpenBatches(finPeriod));
		}

		protected virtual ProcessingResult CheckOpenBatches(IFinPeriod finPeriod)
		{
			ProcessingResult errors = new ProcessingResult();

			UnprocessedObjectsQueryParameters parameters = new UnprocessedObjectsQueryParameters
			{
				OrganizationID = finPeriod.OrganizationID,
				FromFinPeriodID = finPeriod.FinPeriodID,
				ToFinPeriodID = finPeriod.FinPeriodID
			};

			foreach (UnprocessedObjectsCheckingRule checker in CheckingRules)
			{
				PXResult result = (PXResult)new PXView(this, true, checker.CheckCommand).SelectSingleBound(new object[] { parameters });
				if (result != null)
				{
					List<object> messageParameters = checker.MessageParameters
						.Select(param => Caches[BqlCommand.GetItemType(param)].GetStateExt(PXResult.Unwrap(result[BqlCommand.GetItemType(param)], BqlCommand.GetItemType(param)), param.Name))
						.ToList();
					messageParameters.Add(FinPeriodIDAttribute.FormatForError(finPeriod.FinPeriodID));
					errors.AddErrorMessage(checker.ErrorMessage, messageParameters.ToArray());
				}
			}

			return errors;
		}

		protected virtual bool IsUnclosablePeriod(FinPeriod finPeriod)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
			{
				return OrganizationFinPeriods
					.Select(finPeriod.FinPeriodID)
					.RowCast<OrganizationFinPeriod>()
					.Any(orgFinPeriod => !CheckOpenBatches(orgFinPeriod).IsSuccess);
			}
			else
			{
				return !CheckOpenBatches(finPeriod).IsSuccess;
			}
		}

		protected void AdditionalOrganizationFinPeriodProcessing(string action, IFinPeriod period)
		{
			switch (action)
			{
				case FinPeriodStatusProcessParameters.action.Close:
					CreateAutoreverseBatches(period);
					VerifyOpenBatches(period);
					break;
				case FinPeriodStatusProcessParameters.action.Open:
					break;
				case FinPeriodStatusProcessParameters.action.Lock:
					break;
				case FinPeriodStatusProcessParameters.action.Deactivate:
					break;
				case FinPeriodStatusProcessParameters.action.Reopen:
					break;
				case FinPeriodStatusProcessParameters.action.Unlock:
					break;
			}
		}

		public static void MarkProcessedRowAsErrorAndStop(string errorMessage, params object[] parameters)
		{
			PXException exception = new PXException(errorMessage, parameters);
			PXProcessing.SetError(exception);
			throw exception;
		}

		public static void MarkProcessedRowAsErrorAndStop(ProcessingResult errors)
		{
			if (!errors.IsSuccess)
			{
				MarkProcessedRowAsErrorAndStop(errors.GeneralMessage);
			}
		}
		#endregion
	}
}
