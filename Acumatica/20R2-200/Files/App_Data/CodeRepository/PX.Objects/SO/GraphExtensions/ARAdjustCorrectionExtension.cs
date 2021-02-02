using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.SO.GraphExtensions
{
	public abstract class ARAdjustCorrectionExtension<TGraph> : ARAdjustCorrectionExtension<TGraph, object>
		where TGraph : PXGraph
	{
	}

	public abstract class ARAdjustCorrectionExtension<TGraph, TCancellationField> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<ARAdjust.ARInvoice.isUnderCorrection, NotEqual<True>>),
			Messages.CantCreateApplicationToInvoiceUnderCorrection, typeof(ARAdjust.ARInvoice.refNbr))]
		public virtual void _(Events.CacheAttached<ARAdjust.adjdRefNbr> e)
		{
		}

		public virtual void _(Events.FieldVerifying<ARAdjust.adjdRefNbr> e)
		{
			if (this.GetCancellationFieldValue() == true)
			{
				e.Cancel = true;
			}
		}

		public virtual void _(Events.RowPersisting<ARAdjust> e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert
				&& this.GetCancellationFieldValue() != true)
			{
				var origInv = AR.Standalone.ARRegister.PK.Find(Base, e.Row.AdjdDocType, e.Row.AdjdRefNbr);
				if (origInv?.IsUnderCorrection == true)
				{
					e.Cache.RaiseExceptionHandling<ARAdjust.adjdRefNbr>(e.Row, e.Row.AdjdRefNbr,
						new PXSetPropertyException(Messages.CantCreateApplicationToInvoiceUnderCorrection, origInv.RefNbr));
				}
			}
		}

		public virtual bool? GetCancellationFieldValue()
		{
			if (!typeof(IBqlField).IsAssignableFrom(typeof(TCancellationField)))
				return false;

			PXCache parentCache = Base.Caches[typeof(TCancellationField).DeclaringType];
			return (bool?)parentCache.GetValue(parentCache.Current, typeof(TCancellationField).Name);
		}
	}
}
