using CommonServiceLocator;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.PM.BudgetControl
{
	public abstract class BudgetControlGraph<TGraph> : PXGraphExtension<TGraph>	where TGraph : PXGraph
	{
		#region Mappings
		protected class DocumentMapping : IBqlMapping
		{
			public Type Extension => typeof(Document);

			protected Type _table;
			public Type Table => _table;
			public DocumentMapping(Type table)
			{
				_table = table;
			}

			public Type CuryID = typeof(Document.curyID);
			public Type Date = typeof(Document.date);
			public Type Hold = typeof(Document.hold);
			public Type WarningAmount = typeof(Document.warningAmount);
		}

		protected class DetailMapping : IBqlMapping
		{
			public Type Extension => typeof(Detail);

			protected Type _table;
			public Type Table => _table;
			public DetailMapping(Type table)
			{
				_table = table;
			}

			public Type ProjectID = typeof(Detail.projectID);
			public Type TaskID = typeof(Detail.taskID);
			public Type InventoryID = typeof(Detail.inventoryID);
			public Type CostCodeID = typeof(Detail.costCodeID);
			public Type WarningAmount = typeof(Detail.warningAmount);
			public Type LineNbr = typeof(Detail.lineNbr);
		}

		protected abstract DocumentMapping GetDocumentMapping();

		protected abstract DetailMapping GetDetailMapping();

		public PXSelectExtension<Document> Documents;

		public PXSelectExtension<Detail> Details;
		#endregion

		[InjectDependency]
		public IBudgetService BudgetService { get; set; }

		IPXCurrencyService _currencyService;
		public IPXCurrencyService CurrencyService
		{
			get
			{
				if (_currencyService == null) _currencyService = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base);
				return _currencyService;
			}
			set
			{
				_currencyService = value;
			}
		}

		private const string WarningTerm = "budgeted";

		public PXFilter<PMBudgetControlLine> BudgetControlLines;

		protected virtual void _(Events.RowSelected<Document> e)
		{
			if (e.Row == null) return;

			if (!IsBudgetControlEnabled()) return;

			var warning = PXUIFieldAttribute.GetWarning<Document.warningAmount>(e.Cache, e.Row);
			if (warning == Messages.BudgetControlDocumentWarning)
				e.Cache.RaiseExceptionHandling<Document.warningAmount>(e.Row, e.Row.WarningAmount, null);

			if (!IsBudgetControlRequiredForDocument()) return;

			InitializeBudgetControlLines();

			foreach (PMBudgetControlLine budgetControlLine in BudgetControlLines.Cache.Inserted)
			{
				if (budgetControlLine.RemainingAmount < 0)
				{
					e.Cache.RaiseExceptionHandling<Document.warningAmount>(e.Row, e.Row.WarningAmount, new PXSetPropertyException(Messages.BudgetControlDocumentWarning, PXErrorLevel.Warning));
					return;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<Document, Document.hold> e)
		{
			if (!IsBudgetControlRequired()) return;

			if (e.Row.Hold == false)
				e.Row.BudgetControlLinesInitialized = false;
		}

		protected virtual void _(Events.RowSelected<Detail> e)
		{
			if (e.Row == null) return;

			if (!IsBudgetControlEnabled()) return;

			var warning = PXUIFieldAttribute.GetWarning<Detail.warningAmount>(e.Cache, e.Row);
			if (!string.IsNullOrEmpty(warning) && warning.ToLower().Contains(WarningTerm))
				e.Cache.RaiseExceptionHandling<Detail.warningAmount>(e.Row, e.Row.WarningAmount, null);

			if (!IsBudgetControlRequiredForDocument()) return;

			InitializeBudgetControlLines();

			var budgetControlLine = GetBudgetControlLine(e.Row);
			if (budgetControlLine != null && budgetControlLine.RemainingAmount < 0)
			{
				var message = GetWarningMessage(budgetControlLine);
				e.Cache.RaiseExceptionHandling<Detail.warningAmount>(e.Row, e.Row.WarningAmount, new PXSetPropertyException(message, PXErrorLevel.Warning));
			}
		}

		private string GetWarningMessage(PMBudgetControlLine budgetControlLine)
		{
			var result = budgetControlLine.BudgetExist == true ?
					string.Format(Messages.BudgetControlWarning, budgetControlLine.BudgetedAmount, budgetControlLine.ConsumedAmount, budgetControlLine.AvailableAmount, budgetControlLine.DocumentAmount, budgetControlLine.RemainingAmount) :
					string.Format(Messages.BudgetControlNotFoundWarning, budgetControlLine.BudgetedAmount, budgetControlLine.DocumentAmount, budgetControlLine.RemainingAmount);
			return result;
		}

		private void InitializeBudgetControlLines()
		{
			if (Documents.Current == null || Documents.Current.BudgetControlLinesInitialized == true) return;

			BudgetControlLines.Cache.Clear();
			foreach (Detail line in Details.Select())
				UpdateBudgetControlLine(line, 1);
			BudgetControlLines.Cache.IsDirty = false;
			Documents.Current.BudgetControlLinesInitialized = true;
		}

		protected virtual void _(Events.RowInserted<Detail> e)
		{
			if (!IsBudgetControlRequired()) return;

			var updated = UpdateBudgetControlLine(e.Row, 1);
			if (updated)
				Details.View.RequestRefresh();
		}

		protected virtual void _(Events.RowUpdated<Detail> e)
		{
			if (!IsBudgetControlRequired()) return;

			if (DetailIsChanged(e.OldRow, e.Row))
			{
				var updated = UpdateBudgetControlLine(e.OldRow, -1);
				updated |= UpdateBudgetControlLine(e.Row, 1);
				if (updated)
					Details.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.RowDeleted<Detail> e)
		{
			if (!IsBudgetControlRequired()) return;

			var updated = UpdateBudgetControlLine(e.Row, -1);
			if (updated)
				Details.View.RequestRefresh();
		}

		private bool UpdateBudgetControlLine(Detail row, int sign)
		{
			var budgetControlLine = GetBudgetControlLine(row);
			if (budgetControlLine != null)
			{
				var amount = GetDetailAmount(row);
				budgetControlLine.DocumentAmount += sign * amount;
				budgetControlLine.LineNumbers += (budgetControlLine.LineNumbers == string.Empty ? string.Empty : ";") + row.LineNbr.ToString();
				return true;
			}
			return false;
		}

		private PMBudgetControlLine GetBudgetControlLine(Detail row)
		{
			if (row == null || 
				row.ProjectID == null || ProjectDefaultAttribute.IsNonProject(row.ProjectID) || 
				row.TaskID == null ||
				!IsBudgetControlRequiredForDetail(row))
				return null;

			var accountGroupID = GetAccountGroup(row);
			if (accountGroupID == null) return null;
			PMAccountGroup accountGroup = PMAccountGroup.PK.Find(Base, accountGroupID);

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, row.ProjectID);

			PMSetup setup = PXSelect<PMSetup>.Select(Base);

			var budgetFilter = new BudgetFilter
			{
				ProjectID = row.ProjectID,
				TaskID = row.TaskID,
				AccountGroupID = accountGroupID,
				CostCodeID = row.CostCodeID,
				InventoryID = row.InventoryID
			};
			
			Lite.PMBudget budget = BudgetService.SelectProjectBalance(budgetFilter, accountGroup, project, out bool isExisting);

			var budgetControlLine = new PMBudgetControlLine
			{
				ProjectID = budget.ProjectID,
				TaskID = budget.TaskID,
				AccountGroupID = budget.AccountGroupID,
				InventoryID = budget.InventoryID,
				CostCodeID = budget.CostCodeID
			};
			var result = BudgetControlLines.Locate(budgetControlLine);
			if (result == null)
			{
				result = BudgetControlLines.Insert(budgetControlLine);
				result.BudgetedAmount = budget.CuryRevisedAmount ?? 0;
				result.DocumentAmount = 0;
				result.BudgetExist = isExisting;
				result.LineNumbers = string.Empty;
			}
			result.ConsumedAmount = GetConsumedAmount(row, budget);
			return result;
		}

		private bool IsBudgetControlRequired()
		{
			var result = IsBudgetControlEnabled() && IsBudgetControlRequiredForDocument();
			return result;
		}

		private bool IsBudgetControlEnabled()
		{
			PMSetup setup = PXSelect<PMSetup>.Select(Base);
			if (setup == null) return false;

			var result = setup.BudgetControl == BudgetControlOption.Warn;
			return result;
		}

		protected virtual decimal? GetDetailAmount(Detail row)
		{
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, row.ProjectID);
			Company company = PXSelect<Company>.Select(Base);
			var doc = Documents.Current;
			var effDate = doc.Date.GetValueOrDefault(DateTime.Now);

			decimal? amount;
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() || (company.BaseCuryID == project.CuryID))
			{
				amount = GetDetailBaseAmount(row);
			}
			else
			{
				amount = GetDetailCuryAmount(row);

				var rateTypeID = project.RateTypeID;
				if (string.IsNullOrEmpty(rateTypeID))
				{
					CMSetup cmsetup = PXSelect<CMSetup>.Select(Base);
					rateTypeID = cmsetup?.PMRateTypeDflt;
				}

				var rate = CurrencyService.GetRate(doc.CuryID, project.CuryID, rateTypeID, effDate);
				if (rate == null)
					Details.Cache.RaiseExceptionHandling<Detail.warningAmount>(row, row.WarningAmount, 
						new PXSetPropertyException(string.Format(Messages.CurrencyRateIsNotDefined, doc.CuryID, project.CuryID, rateTypeID, effDate), PXErrorLevel.Warning));
				else
					amount = PMCommitmentAttribute.CuryConvCury(rate, amount.GetValueOrDefault(), CurrencyService.CuryDecimalPlaces(project.CuryID));
			}
			return amount;
		}

		protected virtual decimal? GetConsumedAmount(Detail row, Lite.PMBudget budget)
		{
			//need to consider dirty CuryCommittedOpenAmount
			var budgetAccum = new PMBudgetAccum
			{
				ProjectID = budget.ProjectID,
				ProjectTaskID = budget.TaskID,
				AccountGroupID = budget.AccountGroupID,
				InventoryID = budget.InventoryID,
				CostCodeID = budget.CostCodeID
			};
			budgetAccum = Base.Caches<PMBudgetAccum>().Locate(budgetAccum) as PMBudgetAccum;

			var result = (budget.CuryActualAmount ?? 0) + (budget.CuryCommittedOpenAmount ?? 0);
			
			if (budgetAccum != null && Base.Caches<PMBudgetAccum>().GetStatus(budgetAccum) == PXEntryStatus.Inserted)
			{
				result += budgetAccum.CuryCommittedOpenAmount.GetValueOrDefault();
			}
			
			return result;
		}

		protected virtual bool IsBudgetControlRequiredForDocument()
		{
			return true;
		}

		protected virtual bool IsBudgetControlRequiredForDetail(Detail row)
		{
			return true;
		}

		protected abstract bool DetailIsChanged(Detail oldRow, Detail row);

		protected abstract int? GetAccountGroup(Detail row);

		protected abstract decimal? GetDetailBaseAmount(Detail row);

		protected abstract decimal? GetDetailCuryAmount(Detail row);
	}
}