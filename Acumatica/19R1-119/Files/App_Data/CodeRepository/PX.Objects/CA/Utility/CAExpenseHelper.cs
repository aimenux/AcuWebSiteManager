using System;
using PX.Data;

namespace PX.Objects.CA
{
	[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
	public static class CAExpenseHelper
	{
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public class one : PX.Data.BQL.BqlInt.Constant<one>
		{
			public one() : base(1) { }
		}

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public const string CA_30_40_00 = "CA.30.40.00";

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		private static void CAExpense_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var expense = (CAExpense)e.Row;

			if (string.IsNullOrEmpty(expense.AdjRefNbr))
			{
				return;
			}

			CAAdj adj = PXSelect<CAAdj, Where<CAAdj.adjTranType, Equal<CATranType.cATransferExp>, And<CAAdj.adjRefNbr, Equal<Required<CAExpense.adjRefNbr>>>>>.Select(sender.Graph, expense.AdjRefNbr);
			CASplit split = PXSelect<CASplit, Where<CASplit.adjTranType, Equal<CATranType.cATransferExp>, And<CASplit.adjRefNbr, Equal<Required<CAExpense.adjRefNbr>>,
				And<CASplit.lineNbr, Equal<one>>>>>.Select(sender.Graph, expense.AdjRefNbr);

			adj.EntryTypeID = expense.EntryTypeID;
			split.AccountID = expense.AccountID;
			split.SubID = expense.SubID;
			split.CuryUnitPrice = expense.CuryTranAmt;
			split.CuryTranAmt = expense.CuryTranAmt;
			split.TranAmt = expense.TranAmt;
			adj.DrCr = expense.DrCr;
			adj.CuryTranAmt = expense.CuryTranAmt;
			adj.TranAmt = expense.TranAmt;
			adj.CuryControlAmt = expense.CuryTranAmt;
			adj.ControlAmt = expense.TranAmt;
			adj.TranDate = expense.TranDate;
			adj.TranPeriodID = expense.TranPeriodID;
			adj.TranDesc = expense.TranDesc;
			adj.ExtRefNbr = expense.ExtRefNbr;

			sender.Graph.Caches[typeof(CASplit)].Update(split);
			sender.Graph.Caches[typeof(CAAdj)].Update(adj);
		}

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		private static void CAAdj_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var adj = (CAAdj)e.Row;

			if (adj.AdjTranType != CATranType.CATransferExp)
			{
				return;
			}

			CAExpense expense = PXSelect<CAExpense, Where<CAExpense.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>>>.Select(sender.Graph, adj.AdjRefNbr);

			expense.EntryTypeID = adj.EntryTypeID;
			expense.DrCr = adj.DrCr;
			expense.CuryTranAmt = adj.CuryTranAmt;
			expense.TranAmt = adj.TranAmt;
			expense.TranDate = adj.TranDate;
			expense.TranPeriodID = adj.TranPeriodID;
			expense.TranDesc = adj.TranDesc;
			expense.ExtRefNbr = adj.ExtRefNbr;

			sender.Graph.Caches[typeof(CAExpense)].Update(expense);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete && 
				sender.Graph.Accessinfo.ScreenID == CA_30_40_00 && 
				adj.AdjTranType == CATranType.CATransferExp)
			{
				e.Cancel = true;
				throw new PXException(Messages.TransferDocCanNotBeDel);
			}
		}

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		private static void CASplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var split = (CASplit)e.Row;

			if (split.AdjTranType != CATranType.CATransferExp)
			{
				return;
			}

			CAExpense expense = PXSelect<CAExpense, Where<CAExpense.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>>>.Select(sender.Graph, split.AdjRefNbr);

			expense.AccountID = split.AccountID;
			expense.SubID = split.SubID;
			expense.CuryTranAmt = split.CuryTranAmt;

			sender.Graph.Caches[typeof(CAExpense)].Update(expense);
		}

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public static void InitBackwardEditorHandlers(CATranEntry graph)
		{
			graph.RowPersisting.AddHandler<CAAdj>(CAExpenseHelper.CAAdj_RowPersisting);
			graph.RowPersisting.AddHandler<CASplit>(CAExpenseHelper.CASplit_RowPersisting);
		}

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public static void InitBackwardEditorHandlers(CashTransferEntry graph)
		{
			graph.RowPersisting.AddHandler<CAExpense>(CAExpenseHelper.CAExpense_RowPersisting);
		}
	}
}