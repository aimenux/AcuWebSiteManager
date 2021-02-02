using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL.DAC;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.Formula;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Common;
using PX.Objects.GL;
using System.Text;
using PX.Data.SQLTree;
using PX.Objects.GL.FinPeriods;
using Type = System.Type;

namespace PX.Objects.Common
{
	public abstract class FinPeriodClosingProcessBase : PXGraph
	{ 
		[Serializable]
		public class FinPeriodClosingProcessParameters : IBqlTable
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
						new string[] { FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Close },
						new string[] { GL.Messages.ActionClose })
					{ }
				}

				public class FullListAttribute : PXStringListAttribute
				{
					public FullListAttribute() : base(
						new string[] { FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Close, FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Reopen },
						new string[] { GL.Messages.ActionClose, GL.Messages.ActionReopen })
					{ }
				}

				public class close : PX.Data.BQL.BqlString.Constant<close> { public close() : base(FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Close) { } }
				public class reopen : PX.Data.BQL.BqlString.Constant<reopen> { public reopen() : base(FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Reopen) { } }

			}

			[PXString(10)]
			[PXUIField(DisplayName = "Action")]
			[FinPeriodClosingProcessParameters.action.FullList]
			[PXDefault(FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Close)]
			public virtual string Action { get; set; }
			#endregion

			#region FirstYear
			public abstract class firstYear : PX.Data.BQL.BqlString.Field<firstYear> { }
			[PXString(4, IsFixed = true)]
			[PXFormula(typeof(Default<filterOrganizationID, action>))]
			public virtual string FirstYear { get; set; }
			#endregion

			#region LastYear
			public abstract class lastYear : PX.Data.BQL.BqlString.Field<lastYear> { }
			[PXString(4, IsFixed = true)]
			[PXFormula(typeof(Default<filterOrganizationID, action>))]
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
		public class UnprocessedObjectsQueryParameters : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			[PXInt]
			public virtual int? OrganizationID { get; set; }
			#endregion

			#region FromFinPeriodID
			public abstract class fromFinPeriodID : PX.Data.BQL.BqlString.Field<fromFinPeriodID> { }
			[PXString(6, IsFixed = true)]
			public virtual string FromFinPeriodID { get; set; }
			#endregion

			#region ToFinPeriodID
			public abstract class toFinPeriodID : PX.Data.BQL.BqlString.Field<toFinPeriodID> { }
			[PXString(6, IsFixed = true)]
			public virtual string ToFinPeriodID { get; set; }
			#endregion

			#region FromFinPeriodStartDate
			public abstract class fromFinPeriodStartDate : PX.Data.BQL.BqlDateTime.Field<fromFinPeriodStartDate> { }
			[PXDate]
			public virtual DateTime? FromFinPeriodStartDate { get; set; }
			#endregion

			#region ToFinPeriodEndDate
			public abstract class toFinPeriodEndDate : PX.Data.BQL.BqlDateTime.Field<toFinPeriodEndDate> { }
			[PXDate]
			public virtual DateTime? ToFinPeriodEndDate { get; set; }
			#endregion
		}

		public class UnprocessedObjectsCheckingRule
		{
			public string ReportID { get; set; }
			public BqlCommand CheckCommand { get; set; }
			public string ErrorMessage { get; set; }
			public Type[] MessageParameters { get; set; } = new Type[] { };
		}

		public class WhereFinPeriodInRange<TFinPeriodID, TOrganizationID> : IBqlWhere
			where TFinPeriodID : IBqlOperand
			where TOrganizationID : IBqlOperand
		{
			private static IBqlCreator Where
			{
				get
				{
					if (PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
						return new Where<TFinPeriodID, GreaterEqual<Current<UnprocessedObjectsQueryParameters.fromFinPeriodID>>,
							 And<TFinPeriodID, LessEqual<Current<UnprocessedObjectsQueryParameters.toFinPeriodID>>>>();
					else
					{
						return new Where<TFinPeriodID, GreaterEqual<Current<UnprocessedObjectsQueryParameters.fromFinPeriodID>>,
					And<TFinPeriodID, LessEqual<Current<UnprocessedObjectsQueryParameters.toFinPeriodID>>,
					And<
						Where<TOrganizationID, Equal<Current<UnprocessedObjectsQueryParameters.organizationID>>,
							Or<Current<UnprocessedObjectsQueryParameters.organizationID>, IsNull>>>>>();
					}
				}
			}

			public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
				=> Where.AppendExpression(ref exp, graph, info, selection);

			public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
				=> Where.Verify(cache, item, pars, ref result, ref value);
		}

		public class WhereDateInRange<TDate, TOrganizationID> : IBqlWhere
			where TDate : IBqlOperand
			where TOrganizationID : IBqlOperand
		{
			private static IBqlCreator Where =>
				new Where<TDate, GreaterEqual<Current<UnprocessedObjectsQueryParameters.fromFinPeriodStartDate>>,
					And<TDate, LessEqual<Current<UnprocessedObjectsQueryParameters.toFinPeriodEndDate>>,
					And<
						Where<TOrganizationID, Equal<Current<UnprocessedObjectsQueryParameters.organizationID>>,
							Or<Current<UnprocessedObjectsQueryParameters.organizationID>, IsNull>>>>>();

			public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
				=> Where.AppendExpression(ref exp, graph, info, selection);

			public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
				=> Where.Verify(cache, item, pars, ref result, ref value);
		}

		public abstract string ClosedFieldName { get; }

		protected virtual string FeatureName => null;
		public virtual bool NeedValidate => string.IsNullOrEmpty(FeatureName) || PXAccess.FeatureInstalled(FeatureName);
		protected abstract UnprocessedObjectsCheckingRule[] CheckingRules { get; }
		public abstract void ClosePeriod(FinPeriod finPeriod);
		public abstract void ClosePeriods(List<FinPeriod> finPeriods);
		public abstract void ReopenPeriod(FinPeriod finPeriod);
		public abstract void ProcessPeriods(List<FinPeriod> finPeriods);
		public abstract List<(string ReportID, IPXResultset ReportData)> GetReportsData(int? organizationiD, string fromPeriodID, string toPeriodID);
		public abstract bool IsUnclosablePeriod(FinPeriod finPeriod);
	}


	public abstract class FinPeriodClosingProcessBase<TGraph, TSubledgerClosedFlagField> : FinPeriodClosingProcessBase
		where TGraph : PXGraph
		where TSubledgerClosedFlagField : IBqlField
	{
		public PXFilter<FinPeriodClosingProcessParameters> Filter;

		public PXCancel<FinPeriodClosingProcessParameters> Cancel;

		public PXFilteredProcessing<
			FinPeriod,
			FinPeriodClosingProcessParameters,
			Where<FinPeriod.organizationID, Equal<Current<FinPeriodClosingProcessParameters.filterOrganizationID>>,
				And<FinPeriod.finYear, GreaterEqual<Current<FinPeriodClosingProcessParameters.fromYear>>,
				And<FinPeriod.finYear, LessEqual<Current<FinPeriodClosingProcessParameters.toYear>>,
				And<Where<Current<FinPeriodClosingProcessParameters.action>, Equal<FinPeriodClosingProcessParameters.action.close>, 
						And<TSubledgerClosedFlagField, NotEqual<True>,
					Or<Current<FinPeriodClosingProcessParameters.action>, Equal<FinPeriodClosingProcessParameters.action.reopen>, 
						And<TSubledgerClosedFlagField, Equal<True>,
						And<FinPeriod.status, NotEqual<FinPeriod.status.locked>>>>>>>>>>>
			FinPeriods;

		public PXSelect<
			OrganizationFinPeriod,
			Where<OrganizationFinPeriod.masterFinPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>>>
			OrganizationFinPeriods;

		public IEnumerable<FinPeriod> SelectedItems => FinPeriods
			.Cache
			.Updated
			.Cast<FinPeriod>()
			.Where(p => p.Selected == true);

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		// Prevent selector verification while the formula chain is not fully completed and first/last years range is temporary invalid
		protected virtual void _(Events.FieldVerifying<FinPeriodClosingProcessParameters.fromYear> e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		// Prevent selector verification while the formula chain is not fully completed and first/last years range is temporary invalid
		protected virtual void _(Events.FieldVerifying<FinPeriodClosingProcessParameters.toYear> e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowSelected<FinPeriodClosingProcessParameters> e)
		{
			FinPeriodClosingProcessBase graph = this.TypelessClone() as FinPeriodClosingProcessBase;
			FinPeriods.SetProcessDelegate(graph.ProcessPeriods);

			ShowDocuments.SetEnabled(SelectedItems.Any());

			bool isReverseActionAvailable = FinPeriodUtils.CanPostToClosedPeriod();

			PXStringListAttribute.SetList<FinPeriodClosingProcessParameters.action>(
				e.Cache,
				e.Row,
				isReverseActionAvailable
					? new FinPeriodClosingProcessParameters.action.FullListAttribute() as PXStringListAttribute
					: new FinPeriodClosingProcessParameters.action.RestrictedListAttribute() as PXStringListAttribute);

		}

		protected virtual void _(Events.RowUpdated<FinPeriodClosingProcessParameters> e)
		{
			if (e.Row.OrganizationID != e.OldRow.OrganizationID
				|| e.Row.FromYear != e.OldRow.FromYear
				|| e.Row.ToYear != e.OldRow.ToYear)
			{
				FinPeriods.Cache.Clear();
			}
		}

		private BqlCommand applicableYearsQuery = null;
		protected BqlCommand ApplicableYearsQuery => applicableYearsQuery = applicableYearsQuery ?? 
			PXSelectJoin<
				FinYear,
				InnerJoin<FinPeriod,
					On<FinYear.organizationID, Equal<FinPeriod.organizationID>,
					And<FinYear.year, Equal<FinPeriod.finYear>>>>,
				Where<FinYear.organizationID, Equal<Current<FinPeriodClosingProcessParameters.filterOrganizationID>>,
					And<Where<Current<FinPeriodClosingProcessParameters.action>, Equal<FinPeriodClosingProcessParameters.action.close>,
							And<TSubledgerClosedFlagField, NotEqual<True>,
						Or<Current<FinPeriodClosingProcessParameters.action>, Equal<FinPeriodClosingProcessParameters.action.reopen>,
							And<TSubledgerClosedFlagField, Equal<True>>>>>>>>.GetCommand();

		protected virtual void _(Events.FieldDefaulting<FinPeriodClosingProcessParameters.firstYear> e)
		{
			e.NewValue = ApplicableYearsQuery
				.OrderByNew<OrderBy<Asc<FinYear.year>>>()
				.SelectSingleReadonly<FinYear>(this, new IBqlTable[] { (FinPeriodClosingProcessParameters)e.Row })
				?.Year;
		}

		protected virtual void _(Events.FieldDefaulting<FinPeriodClosingProcessParameters.lastYear> e)
		{
			e.NewValue = ApplicableYearsQuery
				.OrderByNew<OrderBy<Desc<FinYear.year>>>()
				.SelectSingleReadonly<FinYear>(this, new IBqlTable[] { (FinPeriodClosingProcessParameters)e.Row })
				?.Year;
		}

		public override string ClosedFieldName => typeof(TSubledgerClosedFlagField).Name;

		protected virtual void _(Events.FieldUpdated<FinPeriod, FinPeriod.selected> e)
		{
			FinPeriod currentPeriod = e.Row as FinPeriod;
			if (currentPeriod == null)
				return;

			bool isSelected = currentPeriod.Selected == true;
			bool isDirectAction = FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.GetDirection(Filter.Current.Action) == 
				FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Direction.Direct;

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

		public override void ProcessPeriods(List<FinPeriod> finPeriods)
		{
			switch (Filter.Current.Action)
			{
				case FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Close:
					ClosePeriods(finPeriods);
					break;
				case FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Reopen:
					ReopenPeriods(finPeriods);
					break;
			}
		}

		public override void ClosePeriods(List<FinPeriod> finPeriods)
		{
			PXCache periodsCache = this.Caches<FinPeriod>();
			foreach (FinPeriod finPeriod in finPeriods)
			{
				PXProcessing.SetCurrentItem(finPeriod);
				ClosePeriod(finPeriod);
				PXProcessing.SetProcessed();
			}
		}
		public virtual void ReopenPeriods(List<FinPeriod> finPeriods)
		{
			PXCache periodsCache = this.Caches<FinPeriod>();
			foreach (FinPeriod finPeriod in finPeriods)
			{
				PXProcessing.SetCurrentItem(finPeriod);
				ReopenPeriod(finPeriod);
				PXProcessing.SetProcessed();
			}
		}

		public override void ClosePeriod(FinPeriod finPeriod)
		{
			VerifyOpenDocuments(finPeriod);
			if (PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
			{
				foreach (OrganizationFinPeriod organizationFinPeriod in OrganizationFinPeriods
					.Select(finPeriod.FinPeriodID))
				{
					OrganizationFinPeriods.Cache.SetValue(organizationFinPeriod, ClosedFieldName, true);
					OrganizationFinPeriods.Update(organizationFinPeriod);
				}
			}

			FinPeriods.Cache.SetValue(finPeriod, ClosedFieldName, true);
			FinPeriods.Update(finPeriod);
			Actions.PressSave();
		}

		public override void ReopenPeriod(FinPeriod finPeriod)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
			{
				foreach (OrganizationFinPeriod organizationFinPeriod in OrganizationFinPeriods
					.Select(finPeriod.FinPeriodID))
				{
					OrganizationFinPeriods.Cache.SetValue(organizationFinPeriod, ClosedFieldName, false);
					if (organizationFinPeriod.Status == FinPeriod.status.Closed)
					{
						organizationFinPeriod.Status = FinPeriod.status.Open;
					}
					OrganizationFinPeriods.Update(organizationFinPeriod);
				}
			}

			FinPeriods.Cache.SetValue(finPeriod, ClosedFieldName, false);
			if(finPeriod.Status == FinPeriod.status.Closed)
			{
				finPeriod.Status = FinPeriod.status.Open;
			}
			FinPeriods.Update(finPeriod);
			Actions.PressSave();
		}

		public override bool IsUnclosablePeriod(FinPeriod finPeriod)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
			{
				return OrganizationFinPeriods
					.Select(finPeriod.FinPeriodID)
					.RowCast<OrganizationFinPeriod>()
					.Any(orgFinPeriod => !CheckOpenDocuments(orgFinPeriod).IsSuccess);
			}
			else
			{
				return !CheckOpenDocuments(finPeriod).IsSuccess;
			}
		}

		public PXAction<FinPeriodClosingProcessParameters> ShowDocuments;
		[PXUIField(DisplayName = GL.Messages.ShowDocumentsNonGL, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(VisibleOnProcessingResults = true)]
		public virtual IEnumerable showDocuments(PXAdapter adapter)
		{
			ShowOpenDocuments(SelectedItems);
			return adapter.Get();
		}

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

		public override List<(string ReportID, IPXResultset ReportData)> GetReportsData(int? organizationID, string fromPeriodID, string toPeriodID) =>
			CheckingRules
				.Select(checker => 
				(
					ReportID: checker.ReportID, 
					ReportData: GetResultset(
						checker.CheckCommand, 
						organizationID,
						fromPeriodID, 
						toPeriodID)
				))
				.Where(tuple => (tuple.ReportData?.GetRowCount() ?? 0) > 0)
				.ToList();

        protected virtual string EmptyReportMessage => AP.Messages.NoUnreleasedDocuments;
        protected virtual void ShowOpenDocuments(IEnumerable<FinPeriod> periods)
		{
			ParallelQuery<string> periodIDs = periods.Select(fp => fp.FinPeriodID).AsParallel();

			List<(string ReportID, IPXResultset ReportData)> reports = GetReportsData(Filter.Current.OrganizationID, periodIDs.Min(), periodIDs.Max());

			if (reports.Any())
			{
				(string reportID, IPXResultset reportData) = reports.First();
                PXReportRequiredException report = new PXReportRequiredException( 
                    new PXReportRedirectParameters()
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
					.ForEach(tuple => report.AddSibling(tuple.ReportID, new PXReportRedirectParameters()
				        {
				            ResultSet = tuple.ReportData,
				            ReportParameters = new Dictionary<string, string>
				            {
				                {"OrganizationID",OrganizationMaint.FindOrganizationByID(this, periods.First().OrganizationID)?.OrganizationCD},
				                {"FromPeriodID", FinPeriodIDFormattingAttribute.FormatForDisplay(periodIDs.Min())},
				                {"ToPeriodID", FinPeriodIDFormattingAttribute.FormatForDisplay(periodIDs.Max())}
				            }

                    }));


			    //OrganizationID = finPeriod.OrganizationID,
			    //FromFinPeriodID = finPeriod.FinPeriodID,
			    //ToFinPeriodID = finPeriod.FinPeriodID,
			    //FromFinPeriodStartDate = finPeriod.StartDate,
			    //ToFinPeriodEndDate = finPeriod.EndDate
				throw report;
			}
			else
			{
				Filter.Ask(EmptyReportMessage, MessageButtons.OK);
			}
		}

		protected virtual ProcessingResult CheckOpenDocuments(IFinPeriod finPeriod)
		{
			ProcessingResult errors = new ProcessingResult();

			if (!NeedValidate)
			{
				return errors;
			}

			UnprocessedObjectsQueryParameters parameters = new UnprocessedObjectsQueryParameters
			{
				OrganizationID = finPeriod.OrganizationID==0?null:finPeriod.OrganizationID,
				FromFinPeriodID = finPeriod.FinPeriodID,
				ToFinPeriodID = finPeriod.FinPeriodID,
				FromFinPeriodStartDate = finPeriod.StartDate,
				ToFinPeriodEndDate = finPeriod.EndDate
			};

			foreach (UnprocessedObjectsCheckingRule checker in CheckingRules)
			{
				var command = checker.CheckCommand;
				var view = new PXView(this, true, command);
				PXResult result = null;
				var fields = new List<Type>();
				fields.AddRange(this.Caches[command.GetFirstTable()].BqlKeys);
				fields.AddRange(checker.MessageParameters);
				using (var scope = new PXFieldScope(view, fields.ToArray()))
				{
					result = (PXResult)view.SelectSingleBound(new object[] {parameters});
				}

				if (result != null)
				{
					List<object> messageParameters = checker.MessageParameters
						.Select(param => Caches[BqlCommand.GetItemType(param)]
							.GetStateExt(PXResult.Unwrap(result[BqlCommand.GetItemType(param)], BqlCommand.GetItemType(param)), param.Name))
						.ToList();
					messageParameters.Add(FinPeriodIDAttribute.FormatForError(finPeriod.FinPeriodID));
					errors.AddErrorMessage(checker.ErrorMessage, messageParameters.ToArray());
				}
			}

			return errors;
		}

		protected virtual void VerifyOpenDocuments(IFinPeriod finPeriod)
		{
			FinPeriodStatusProcess.MarkProcessedRowAsErrorAndStop(CheckOpenDocuments(finPeriod));
		}
	}

	public abstract class FinPeriodClosingProcessBase<TGraph, TSubledgerClosedFlagField, TFeatureField> : FinPeriodClosingProcessBase<TGraph, TSubledgerClosedFlagField>
		where TGraph : PXGraph
		where TSubledgerClosedFlagField : IBqlField
		where TFeatureField : IBqlField
	{
		protected override string FeatureName => typeof(TFeatureField).FullName;
	}
}
