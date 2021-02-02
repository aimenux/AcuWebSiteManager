using System;
using System.Linq;
using PX.Data;
using System.Collections.Generic;
using PX.Objects.CS;
using PX.Objects.AP;

namespace PX.Objects.TX
{
	public abstract class ManualVATAttribute : TaxAttribute
	{
		#region DocCuryTaxAmt
		protected string _DocCuryTaxAmt = "CuryTaxAmt";
		public Type DocCuryTaxAmt
		{
			set
			{
				_DocCuryTaxAmt = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryOrigDocAmt
		protected string _CuryOrigDocAmt = "CuryOrigDocAmt";
		public Type CuryOrigDocAmt
		{
			set
			{
				_CuryOrigDocAmt = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion		

		protected bool isManualVAT(PXCache sender)
		{
			string taxzone = GetTaxZone(sender, null);
			TaxZone zone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.SelectWindowed(sender.Graph, 0, 1, taxzone);
			return zone != null && zone.IsManualVATZone == true && PXAccess.FeatureInstalled<FeaturesSet.manualVATEntryMode>();
		}

		protected abstract bool isControlTaxTotalRequired(PXCache sender);

		protected abstract bool isControlTotalRequired(PXCache sender);

		public ManualVATAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type CalcMode = null, Type parentBranchIDField = null)
			: base(ParentType, TaxType, TaxSumType, CalcMode, parentBranchIDField)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (PXAccess.FeatureInstalled<FeaturesSet.manualVATEntryMode>())
			{
				sender.Graph.FieldUpdated.AddHandler(_ParentType, _DocCuryTaxAmt, TaxTotalFieldUpdated);
				sender.Graph.FieldUpdated.AddHandler(_ParentType, _CuryOrigDocAmt, DocTotalFieldUpdated);
			}
		}

		protected override void CalcTaxes(PXCache sender, object row, PXTaxCheck taxchk)
		{
			if (isManualVAT(sender))
			{
				CalcTotals(sender, row, true);
				return;
			}

			base.CalcTaxes(sender, row, taxchk);
		}

		protected override void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			if (isManualVAT(sender))
			{
				string taxzone = GetTaxZone(sender, null);
				TaxZone zone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.SelectWindowed(sender.Graph, 0, 1, taxzone);
				//it is possible to brake this by overriding ManualTaxes - consider changing
				if (!ManualTaxes(sender, row).Any(det => det.TaxID == zone.TaxID))
				{
					TaxDetail newdet = (TaxDetail)Activator.CreateInstance(_TaxSumType);
					newdet = DefaultSimplifiedVAT(sender, row, zone, newdet);
					if (newdet != null)
					{
						PXCache sumCache = sender.Graph.Caches[_TaxSumType];
						Insert(sumCache, newdet);
					}
				}
				return;
			}

			base.DefaultTaxes(sender, row, DefaultExisting);
		}

		protected override TaxDetail CalculateTaxSum(PXCache sender, object taxrow, object row)
		{
			if (isManualVAT(sender))
			{
				PXCache sumcache = sender.Graph.Caches[_TaxSumType];
				string taxzone = GetTaxZone(sender, null);
				TaxZone zone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.SelectWindowed(sender.Graph, 0, 1, taxzone);
				TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
				TaxDetail taxSum = null;				
				if (zone.TaxID == taxdet.TaxID)
				{
					taxSum = DefaultSimplifiedVAT(sender, row, zone, taxdet);
				}
				return taxSum;				
			}

			return base.CalculateTaxSum(sender, taxrow, row);
		}

		protected virtual void DocTotalFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (isControlTotalRequired(sender) && isManualVAT(sender))
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				ReDefaultTaxes(cache, null, null);
				base.ParentFieldUpdated(sender, e);
			}
		}

		protected virtual void TaxTotalFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (isControlTaxTotalRequired(sender) && isManualVAT(sender))
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				ReDefaultTaxes(cache, null, null);
				base.ParentFieldUpdated(sender, e);
			}	
		}

		protected override void ParentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (isManualVAT(sender))
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				ReDefaultTaxes(cache, null, null);
			}
			base.ParentFieldUpdated(sender, e);
		}

		protected override void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.manualVATEntryMode>())
			{
				TaxZone newTaxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
				if (newTaxZone != null && newTaxZone.IsManualVATZone == true)
				{
					ReDefaultTaxes(sender, null, null);
					_ParentRow = e.Row;
					CalcTaxes(sender, null);
					_ParentRow = null;
					return;
				}
			}
			base.ZoneUpdated(sender, e);
		}

		protected TaxDetail DefaultSimplifiedVAT(PXCache sender, object row, TaxZone zone, TaxDetail newdet)
		{
			PXCache cache = sender.Graph.Caches[_TaxSumType];
			decimal? docCuryTaxAmt = (decimal?)ParentGetValue(sender.Graph, _DocCuryTaxAmt) ?? 0m;
			decimal? docCuryOrigDocAmt = (decimal?)ParentGetValue(sender.Graph, _CuryOrigDocAmt) ?? 0m;
			if (docCuryTaxAmt == 0m)
			{
				return null;
			}
			newdet.TaxID = zone.TaxID;
			if (docCuryOrigDocAmt - docCuryTaxAmt != 0)
			{
				newdet.TaxRate = (docCuryTaxAmt / (docCuryOrigDocAmt - docCuryTaxAmt)) * 100m;
			}
			else
			{
				newdet.TaxRate = 0m;
			}
			newdet.NonDeductibleTaxRate = 0m;
			cache.SetValue(newdet, _CuryTaxableAmt, Math.Abs(docCuryOrigDocAmt.Value) > Math.Abs(docCuryTaxAmt.Value) ? docCuryOrigDocAmt - docCuryTaxAmt : 0m);
			cache.SetValue(newdet, _CuryTaxAmt, docCuryTaxAmt);
			newdet.CuryExpenseAmt = 0m;
			return newdet;
		}
	}
}
