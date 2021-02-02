using PX.Common;
using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Extensions
{
	public abstract class NonDecimalUnitsNoVerifyOnHoldExt<TGraph, TDocument, TLine, TLineQty, TLineSplit, TLineSplitQty> : PXGraphExtension<TGraph> 
		where TGraph : PXGraph
		where TDocument: class, IBqlTable, new()
		where TLine: class, IBqlTable, new()
		where TLineQty: class, IBqlField
		where TLineSplit: class, IBqlTable, new()
		where TLineSplitQty : class, IBqlField
	{
		public abstract bool HaveHoldStatus(TDocument doc);
		public abstract int? GetLineNbr(TLine line);
		public abstract int? GetLineNbr(TLineSplit split);

		protected abstract TLine LocateLine(TLineSplit split);

		public abstract IEnumerable<TLine> GetLines();

		public abstract IEnumerable<TLineSplit> GetSplits();

		protected virtual void _(Events.RowSelected<TDocument> e)
		{
			if (e.Row != null)
				UpdateDecimalVerifyMode(e.Row);
		}

		protected virtual void _(Events.RowUpdated<TDocument> e)
		{
			if (HaveHoldStatus(e.OldRow) == HaveHoldStatus(e.Row))
				return;
			UpdateDecimalVerifyMode(e.Row);
			var lineCache = Base.Caches<TLine>();
			var lineSplitCache = Base.Caches<TLineSplit>();

			var splits = GetSplits().ToLookup(GetLineNbr);
			foreach (TLine line in GetLines())
			{
				VerifyLine(lineCache, line);
				foreach (TLineSplit split in splits[GetLineNbr(line)])
				{
					using (new NotDecimalUnitErrorRedirectorScope<TLineSplitQty, TLineQty>(lineCache, line))
						VerifySplit(lineSplitCache, split);
				}
			}
		}

		protected virtual void VerifyLine(PXCache lineCache, TLine line) => VerifyRow(lineCache, line);

		protected virtual void VerifySplit(PXCache lineSplitCache, TLineSplit split) => VerifyRow(lineSplitCache, split);

		private void VerifyRow<TRow>(PXCache cache, TRow row) where TRow : class, IBqlTable, new()
		{
			var ex = PXDBQuantityAttribute.VerifyForDecimal(cache, row);
			if (ex != null && ex.ErrorLevel >= PXErrorLevel.Error)
				cache.MarkUpdated(row);
		}

		protected virtual void UpdateDecimalVerifyMode(TDocument doc)
		{
			var verifyMode = HaveHoldStatus(doc) ? DecimalVerifyMode.Warning : DecimalVerifyMode.Error;
			Base.Caches<TLine>().Adjust<PXDBQuantityAttribute>().ForAllFields(a => a.DecimalVerifyMode = verifyMode);
			Base.Caches<TLineSplit>().Adjust<PXDBQuantityAttribute>().ForAllFields(a => a.DecimalVerifyMode = verifyMode);
		}

		[PXOverride]
		public int Persist(Type cacheType, PXDBOperation operation, Func<Type, PXDBOperation, int> basePersist)
		{
			if (operation.IsIn(PXDBOperation.Insert, PXDBOperation.Update) && cacheType == typeof(TLineSplit))
			{
				using (new NotDecimalUnitErrorRedirectorScope<TLineSplit, TLineSplitQty, TLine, TLineQty>(Base.Caches<TLine>(), LocateLine))
					return basePersist(cacheType, operation);
			}
			return basePersist(cacheType, operation);
		}
	}
}
