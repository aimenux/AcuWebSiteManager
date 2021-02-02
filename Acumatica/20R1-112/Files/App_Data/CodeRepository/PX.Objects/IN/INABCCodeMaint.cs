using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;


namespace PX.Objects.IN
{
	[Serializable]
	public partial class INABCTotal : PX.Data.IBqlTable
	{
		#region TotalABCPct
		public abstract class totalABCPct : PX.Data.BQL.BqlDecimal.Field<totalABCPct> { }
		protected Decimal? _TotalABCPct;
		[PXDBDecimal(2 /*, MinValue = 100.0 , MaxValue = 100.0*/)]
		[PXUIField(DisplayName = "Total ABC Code %", Enabled = false)]
		public virtual Decimal? TotalABCPct
		{
			get
			{
				return this._TotalABCPct;
			}
			set
			{
				this._TotalABCPct = value;
			}
		}
		#endregion
	}

	public class INABCCodeMaint : PXGraph<INABCCodeMaint, INABCCode>
	{
		public PXFilter<INABCTotal> ABCTotals;

		public PXSelect<INABCCode> ABCCodes;
		//public PXSave<INABCCode> Save;

		private decimal CalcTotalABCPct()
		{
			decimal totalABCPct = 0.0m;
			foreach (INABCCode coderec in this.ABCCodes.Select())
			{
				totalABCPct += (coderec.ABCPct ?? 0m);
			}
			return totalABCPct;
		}


		protected virtual IEnumerable aBCTotals()
		{
			PXCache abctotals_cache = this.Caches[typeof(INABCTotal)];
			INABCTotal abctotals = (INABCTotal)abctotals_cache.Current;

			abctotals.TotalABCPct = CalcTotalABCPct();
			abctotals_cache.IsDirty = false;
			yield return abctotals;
		}

		protected virtual void INABCCode_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			decimal total = CalcTotalABCPct();
			if (total != 0 && total != 100m)
			{
				throw new PXRowPersistingException(typeof(INABCTotal.totalABCPct).Name, total, Messages.TotalPctShouldBe100);
			}
		}

		public virtual void INABCCode_CountsPerYear_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null)
			{
				if ((((short)e.NewValue) < 0) || (((short)e.NewValue) > 365))
				{
					throw new PXSetPropertyException(Messages.ThisValueShouldBeBetweenP0AndP1, PXErrorLevel.Error, 0, 365);
				}
			}
		}

		public virtual void INABCCode_MaxCountInaccuracyPct_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if ((((Decimal)e.NewValue) < 0m) || (((Decimal)e.NewValue) > 100m))
			{
				throw new PXSetPropertyException(Messages.PercentageValueShouldBeBetween0And100);
			}
		}

		public virtual void INABCCode_ABCPct_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if ((((Decimal)e.NewValue) < 0m) || (((Decimal)e.NewValue) > 100m))
			{
				throw new PXSetPropertyException(Messages.PercentageValueShouldBeBetween0And100);
			}
		}
	}
}
