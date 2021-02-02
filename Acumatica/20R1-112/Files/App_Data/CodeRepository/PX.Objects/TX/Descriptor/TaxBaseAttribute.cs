using PX.Data;
using PX.Common;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PX.Api;
using System.Collections;
using PX.Data.SQLTree;
using System.Reflection;

namespace PX.Objects.TX
{
	public abstract partial class TaxBaseAttribute : PXAggregateAttribute,
													 IPXRowInsertedSubscriber,
													 IPXRowUpdatedSubscriber,
													 IPXRowDeletedSubscriber,
													 IPXRowPersistedSubscriber,
													 IComparable
	{
		protected string _LineNbr = "LineNbr";
		protected string _CuryOrigTaxableAmt = "CuryOrigTaxableAmt";
		protected string _CuryTaxAmt = "CuryTaxAmt";
		protected string _CuryTaxAmtSumm = "CuryTaxAmtSumm";
		protected string _CuryTaxDiscountAmt = "CuryTaxDiscountAmt";
		protected string _CuryTaxableAmt = "CuryTaxableAmt";
		protected string _CuryExemptedAmt = "CuryExemptedAmt";
		protected string _CuryTaxableDiscountAmt = "CuryTaxableDiscountAmt";
		protected string _CuryExpenseAmt = "CuryExpenseAmt";
		protected string _CuryRateTypeID = "CuryRateTypeID";
		protected string _CuryEffDate = "CuryEffDate";
		protected string _CuryRate = "SampleCuryRate";
		protected string _RecipRate = "SampleRecipRate";
		protected string _IsTaxSaved = "IsTaxSaved";
		protected string _RecordID = "RecordID";

		protected Type _ParentType;
		protected Type _ChildType;
		protected Type _TaxType;
		protected Type _TaxSumType;
		protected Type _CuryKeyField = null;

		protected Dictionary<object, object> inserted = null;
		protected Dictionary<object, object> updated = null;

		#region TaxID
		protected string _TaxID = "TaxID";
		public Type TaxID
		{
			set
			{
				_TaxID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region TaxCategoryID
		protected string _TaxCategoryID = "TaxCategoryID";
		public Type TaxCategoryID
		{
			set
			{
				_TaxCategoryID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region TaxZoneID
		protected string _TaxZoneID = "TaxZoneID";
		public Type TaxZoneID
		{
			set
			{
				_TaxZoneID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region DocDate
		protected string _DocDate = "DocDate";
		public Type DocDate
		{
			set
			{
				_DocDate = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		public Type ParentBranchIDField { get; set; }
		#region FinPeriodID
		protected string _FinPeriodID = "FinPeriodID";
		public Type FinPeriodID
		{
			set
			{
				_FinPeriodID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion

		#region CuryTranAmt
		protected abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }
		protected Type CuryTranAmt = typeof(curyTranAmt);
		protected string _CuryTranAmt
		{
			get
			{
				return CuryTranAmt.Name;
			}
		}
		#endregion

		#region OrigGroupDiscountRate
		protected abstract class origGroupDiscountRate : PX.Data.BQL.BqlDecimal.Field<origGroupDiscountRate> { }
		protected Type OrigGroupDiscountRate = typeof(origGroupDiscountRate);
		protected string _OrigGroupDiscountRate
		{
			get
			{
				return OrigGroupDiscountRate.Name;
			}
		}
		#endregion
		#region OrigDocumentDiscountRate
		protected abstract class origDocumentDiscountRate : PX.Data.BQL.BqlDecimal.Field<origDocumentDiscountRate> { }
		protected Type OrigDocumentDiscountRate = typeof(origDocumentDiscountRate);
		protected string _OrigDocumentDiscountRate
		{
			get
			{
				return OrigDocumentDiscountRate.Name;
			}
		}
		#endregion
		#region GroupDiscountRate
		protected abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
		protected Type GroupDiscountRate = typeof(groupDiscountRate);
		protected string _GroupDiscountRate
		{
			get
			{
				return GroupDiscountRate.Name;
			}
		}
		#endregion
		#region DocumentDiscountRate
		protected abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
		protected Type DocumentDiscountRate = typeof(documentDiscountRate);
		protected string _DocumentDiscountRate
		{
			get
			{
				return DocumentDiscountRate.Name;
			}
		}
		#endregion
		#region TermsID
		protected string _TermsID = "TermsID";
		public Type TermsID
		{
			set
			{
				_TermsID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryID
		protected string _CuryID = "CuryID";
		public Type CuryID
		{
			set
			{
				_CuryID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryDocBal
		protected string _CuryDocBal = "CuryDocBal";
		public Type CuryDocBal
		{
			set
			{
				_CuryDocBal = (value != null) ? value.Name : null;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryTaxDiscountTotal
		protected string _CuryTaxDiscountTotal = "CuryOrigTaxDiscAmt";
		public Type CuryDocBalUndiscounted
		{
			set
			{
				_CuryTaxDiscountTotal = (value != null) ? value.Name : null;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryTaxTotal
		protected string _CuryTaxTotal = "CuryTaxTotal";
		public Type CuryTaxTotal
		{
			set
			{
				_CuryTaxTotal = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryOrigDiscAmt
		protected string _CuryOrigDiscAmt = "CuryOrigDiscAmt";
		public Type CuryOrigDiscAmt
		{
			set
			{
				_CuryOrigDiscAmt = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryWhTaxTotal
		protected string _CuryWhTaxTotal = "CuryOrigWhTaxAmt";
		public Type CuryWhTaxTotal
		{
			set
			{
				_CuryWhTaxTotal = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryLineTotal
		protected abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		public Type CuryLineTotal = typeof(curyLineTotal);
		protected string _CuryLineTotal
		{
			get
			{
				return CuryLineTotal.Name;
			}
		}
		#endregion
		#region CuryDiscTot
		protected abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }
		protected Type CuryDiscTot = typeof(curyDiscTot);
		protected string _CuryDiscTot
		{
			get
			{
				return CuryDiscTot.Name;
			}
		}
		#endregion
		#region TaxCalc
		protected TaxCalc _TaxCalc = TaxCalc.Calc;
		public TaxCalc TaxCalc
		{
			set
			{
				_TaxCalc = value;
			}
			get
			{
				return _TaxCalc;
			}
		}
		#endregion
		#region TaxCalcMode
		protected string _TaxCalcMode = null;
		public Type TaxCalcMode
		{
			set
			{
				_TaxCalcMode = value.Name;
			}
			get
			{
				return null;
			}
		}
		protected bool _isTaxCalcModeEnabled
		{
			get { return !String.IsNullOrEmpty(_TaxCalcMode) && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>(); }
		}

		#endregion
		#region Precision
		public int? Precision { get; set; }
		#endregion

		#region RetainageApplyFieldName
		protected virtual string RetainageApplyFieldName => nameof(APInvoice.RetainageApply);
		#endregion
		protected virtual bool CalcGrossOnDocumentLevel { get; set; }
		protected virtual bool AskRecalculationOnCalcModeChange { get; set; }
		protected virtual string _PreviousTaxCalcMode { get; set; }

		public Type ChildBranchIDField { get; set; }
		public Type ChildFinPeriodIDField { get; set; }

		protected bool _NoSumTaxable = false;

		public static List<PXEventSubscriberAttribute> GetAttributes<Field, Target>(PXCache sender, object data)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool exactfind = false;

			var res = new List<PXEventSubscriberAttribute>();
			var q = sender.GetAttributes<Field>(data).Where(
				(attr) => (!exactfind || data == null) && ((exactfind = attr.GetType() == typeof(Target))
				|| attr is TaxAttribute && typeof(Target) == typeof(TaxAttribute)));

			foreach (var a in q)
			{
				a.IsDirty = true;
				res.Add(a);
			}


			res.Sort((a, b) => ((IComparable)a).CompareTo(b));

			return res;
		}

		public static void SetTaxCalc<Field>(PXCache cache, object data, TaxCalc isTaxCalc)
			where Field : IBqlField
		{
			SetTaxCalc<Field, TaxAttribute>(cache, data, isTaxCalc);
		}

		public static void SetTaxCalc<Field, Target>(PXCache cache, object data, TaxCalc isTaxCalc)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(cache, data))
			{
				((TaxAttribute)attr).TaxCalc = isTaxCalc;
			}
		}

		public static TaxCalc GetTaxCalc<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return GetTaxCalc<Field, TaxAttribute>(cache, data);
		}

		public static TaxCalc GetTaxCalc<Field, Target>(PXCache cache, object data)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(cache, data))
			{
				if (((TaxAttribute)attr).TaxCalc != TaxCalc.NoCalc)
				{
					return TaxCalc.Calc;
				}
			}
			return TaxCalc.NoCalc;
		}

		public virtual object Insert(PXCache cache, object item)
		{
			return cache.Insert(item);
		}

		public virtual object Delete(PXCache cache, object item)
		{
			return cache.Delete(item);
		}

		public static void Calculate<Field>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
		{
			Calculate<Field, TaxAttribute>(sender, e);
		}

		public static void Calculate<Field, Target>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool isCalcedByAttribute = false;
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(sender, e.Row))
			{
				isCalcedByAttribute = true;

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualLineCalc)
				{
					((TaxAttribute)attr).TaxCalc = TaxCalc.Calc;

					try
					{
						((IPXRowInsertedSubscriber)attr).RowInserted(sender, e);
					}
					finally
					{
						((TaxAttribute)attr).TaxCalc = TaxCalc.ManualLineCalc;
					}
				}

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualCalc)
				{
					object copy;
					if (((TaxAttribute)attr).inserted.TryGetValue(e.Row, out copy))
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						((TaxAttribute)attr).inserted.Remove(e.Row);

						if (((TaxAttribute)attr).updated.TryGetValue(e.Row, out copy))
						{
							((TaxAttribute)attr).updated.Remove(e.Row);
						}
					}
				}
			}

			if (!isCalcedByAttribute)
			{
				var tgraph = sender.Graph.GetType();
				var extensions = tgraph.GetField("Extensions", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(sender.Graph) as PXGraphExtension[];
				var ext = extensions?.FirstOrDefault(extension => IsInstanceOfGenericType(typeof(Extensions.SalesTax.TaxBaseGraph<,>), extension));

				if (ext == null) return;

				var method = ext.GetType().GetMethod("RecalcTaxes", BindingFlags.NonPublic | BindingFlags.Instance);
				method?.Invoke(ext, null);
			}
		}

		public static void Calculate<Field>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
		{
			Calculate<Field, TaxAttribute>(sender, e);
		}

		public static void Calculate<Field, Target>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool isCalcedByAttribute = false;
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(sender, e.Row))
			{
				isCalcedByAttribute = true;

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualLineCalc)
				{
					((TaxAttribute)attr).TaxCalc = TaxCalc.Calc;

					try
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, e);
					}
					finally
					{
						((TaxAttribute)attr).TaxCalc = TaxCalc.ManualLineCalc;
					}
				}

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualCalc)
				{
					object copy;
					if (((TaxAttribute)attr).updated.TryGetValue(e.Row, out copy))
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						((TaxAttribute)attr).updated.Remove(e.Row);
					}
				}
			}

			if (!isCalcedByAttribute)
			{
				var tgraph = sender.Graph.GetType();
				var extensions = tgraph.GetField("Extensions", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(sender.Graph) as PXGraphExtension[];
				var ext = extensions?.FirstOrDefault(extension => IsInstanceOfGenericType(typeof(Extensions.SalesTax.TaxBaseGraph<,>), extension));

				if (ext == null) return;

				var method = ext.GetType().GetMethod("RecalcTaxes", BindingFlags.NonPublic | BindingFlags.Instance);
				method?.Invoke(ext, null);
			}
		}

		private static bool IsInstanceOfGenericType(Type genericType, object instance)
		{
			Type type = instance.GetType();
			while (type != null)
			{
				if (type.IsGenericType &&
					type.GetGenericTypeDefinition() == genericType)
				{
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		protected virtual string GetTaxZone(PXCache sender, object row)
		{
			return (string)ParentGetValue(sender.Graph, _TaxZoneID);
		}

		protected virtual DateTime? GetDocDate(PXCache sender, object row)
		{
			return (DateTime?)ParentGetValue(sender.Graph, _DocDate);
		}

		protected virtual string GetTaxCategory(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, _TaxCategoryID);
		}



		protected virtual decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType = "I")
		{
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected virtual decimal? GetDocLineFinalAmtNoRounding(PXCache sender, object row, string TaxCalcType = "I")
		{
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected virtual string GetTaxID(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, _TaxID);
		}

		protected virtual object InitializeTaxDet(object data)
		{
			return data;
		}

		protected virtual void AddOneTax(PXCache cache, object detrow, ITaxDetail taxitem)
		{
			if (taxitem != null)
			{
				object newdet;
				TaxParentAttribute.NewChild(cache, detrow, _ChildType, out newdet);
				((ITaxDetail)newdet).TaxID = taxitem.TaxID;
				newdet = InitializeTaxDet(newdet);
				object insdet = Insert(cache, newdet);

				if (insdet != null) PXParentAttribute.SetParent(cache, insdet, _ChildType, detrow);
			}
		}

		public virtual ITaxDetail MatchesCategory(PXCache sender, object row, ITaxDetail zoneitem)
		{
			string taxcat = GetTaxCategory(sender, row);
			string taxid = GetTaxID(sender, row);
			DateTime? docdate = GetDocDate(sender, row);

			TaxRev rev = PXSelect<TaxRev, Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>, And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>, And<TaxRev.outdated, Equal<False>>>>>.Select(sender.Graph, zoneitem.TaxID, docdate);

			if (rev == null)
			{
				return null;
			}

			if (string.Equals(taxid, zoneitem.TaxID))
			{
				return zoneitem;
			}

			TaxCategory cat = (TaxCategory)PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			if (cat == null)
			{
				return null;
			}
			else
			{
				return MatchesCategory(sender, row, new ITaxDetail[] { zoneitem }).FirstOrDefault();
			}

		}

		public virtual IEnumerable<ITaxDetail> MatchesCategory(PXCache sender, object row, IEnumerable<ITaxDetail> zonetaxlist)
		{
			string taxcat = GetTaxCategory(sender, row);

			List<ITaxDetail> ret = new List<ITaxDetail>();

			TaxCategory cat = (TaxCategory)PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			if (cat == null)
			{
				return ret;
			}

			HashSet<string> cattaxlist = new HashSet<string>();
			foreach (TaxCategoryDet detail in PXSelect<TaxCategoryDet, Where<TaxCategoryDet.taxCategoryID, Equal<Required<TaxCategoryDet.taxCategoryID>>>>.Select(sender.Graph, taxcat))
			{
				cattaxlist.Add(detail.TaxID);
			}

			foreach (ITaxDetail zoneitem in zonetaxlist)
			{
				bool zonematchestaxcat = cattaxlist.Contains(zoneitem.TaxID);
				if (cat.TaxCatFlag == false && zonematchestaxcat || cat.TaxCatFlag == true && !zonematchestaxcat)
				{
					ret.Add(zoneitem);
				}
			}

			return ret;
		}


		protected abstract IEnumerable<ITaxDetail> ManualTaxes(PXCache sender, object row);

		protected virtual void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];
			string taxzone = GetTaxZone(sender, row);
			string taxcat = GetTaxCategory(sender, row);
			DateTime? docdate = GetDocDate(sender, row);

			var applicableTaxes = new HashSet<string>();

			foreach (PXResult<TaxZoneDet, TaxCategory, TaxRev, TaxCategoryDet> r in PXSelectJoin<TaxZoneDet,
				CrossJoin<TaxCategory,
				InnerJoin<TaxRev, On<TaxRev.taxID, Equal<TaxZoneDet.taxID>>,
				LeftJoin<TaxCategoryDet, On<TaxCategoryDet.taxID, Equal<TaxZoneDet.taxID>,
					And<TaxCategoryDet.taxCategoryID, Equal<TaxCategory.taxCategoryID>>>>>>,
				Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>,
					And<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>,
					And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>, And<TaxRev.outdated, Equal<False>,
					And<Where<TaxCategory.taxCatFlag, Equal<False>, And<TaxCategoryDet.taxCategoryID, IsNotNull,
						Or<TaxCategory.taxCatFlag, Equal<True>, And<TaxCategoryDet.taxCategoryID, IsNull>>>>>>>>>>.Select(sender.Graph, taxzone, taxcat, docdate))
			{
				AddOneTax(cache, row, (TaxZoneDet)r);
				applicableTaxes.Add(((TaxZoneDet)r).TaxID);
			}

			string taxID;
			if ((taxID = GetTaxID(sender, row)) != null)
			{
				AddOneTax(cache, row, new TaxZoneDet() { TaxID = taxID });
				applicableTaxes.Add(taxID);
			}

			foreach (ITaxDetail r in ManualTaxes(sender, row))
			{
				if (applicableTaxes.Contains(r.TaxID))
					applicableTaxes.Remove(r.TaxID);
			}

			foreach (string applicableTax in applicableTaxes)
			{
				AddTaxTotals(cache, applicableTax, row);
			}

			if (DefaultExisting)
			{
				foreach (ITaxDetail r in MatchesCategory(sender, row, ManualTaxes(sender, row)))
				{
					AddOneTax(cache, row, r);
				}
			}
		}

		protected virtual void DefaultTaxes(PXCache sender, object row)
		{
			DefaultTaxes(sender, row, true);
		}

		private Type GetFieldType(PXCache cache, string FieldName)
		{
			List<Type> fields = cache.BqlFields;
			for (int i = 0; i < fields.Count; i++)
			{
				if (String.Compare(fields[i].Name, FieldName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return fields[i];
				}
			}
			return null;
		}

		private Type GetTaxIDType(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXSelectorAttribute)
				{
					if (((PXSelectorAttribute)attr).Field == typeof(Tax.taxID))
					{
						return GetFieldType(cache, ((PXSelectorAttribute)attr).FieldName);
					}
				}
			}
			return null;
		}

		private Type AddWhere(Type command, Type where)
		{
			if (command.IsGenericType)
			{
				Type[] args = command.GetGenericArguments();
				Type[] pars = new Type[args.Length + 1];
				pars[0] = command.GetGenericTypeDefinition();
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].IsGenericType && (
						args[i].GetGenericTypeDefinition() == typeof(Where<,>) ||
						args[i].GetGenericTypeDefinition() == typeof(Where2<,>) ||
						args[i].GetGenericTypeDefinition() == typeof(Where<,,>)))
					{
						pars[i + 1] = typeof(Where2<,>).MakeGenericType(args[i], typeof(And<>).MakeGenericType(where));
					}
					else
					{
						pars[i + 1] = args[i];
					}
				}
				return BqlCommand.Compose(pars);
			}
			return null;
		}

		protected List<object> SelectTaxes(PXCache sender, object row, PXTaxCheck taxchk)
		{
			return SelectTaxes<Where<True, Equal<True>>>(sender.Graph, row, taxchk);
		}

		protected abstract List<Object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new();

		protected abstract List<Object> SelectDocumentLines(PXGraph graph, object row);

		protected Tax AdjustTaxLevel(PXGraph graph, Tax taxToAdjust)
		{
			if (_isTaxCalcModeEnabled && taxToAdjust.TaxCalcLevel != CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
			{
				string TaxCalcMode = GetTaxCalcMode(graph);
				if (!String.IsNullOrEmpty(TaxCalcMode))
				{
					Tax adjdTax = (Tax)graph.Caches[typeof(Tax)].CreateCopy(taxToAdjust);
					switch (TaxCalcMode)
					{
						case TaxCalculationMode.Gross:
							adjdTax.TaxCalcLevel = CSTaxCalcLevel.Inclusive;
							break;
						case TaxCalculationMode.Net:
							adjdTax.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
							break;
						case TaxCalculationMode.TaxSetting:
							break;
					}
					return adjdTax;
				}
			}
			return taxToAdjust;
		}

		protected virtual void ClearTaxes(PXCache sender, object row)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];
			foreach (object taxrow in SelectTaxes(sender, row, PXTaxCheck.Line))
			{
				Delete(cache, ((PXResult)taxrow)[0]);
			}
		}

		private decimal Sum(PXGraph graph, List<Object> list, Type field)
		{
			decimal ret = 0.0m;
			if (field != null)
			{
				list.ForEach(new Action<Object>(delegate (object a)
				{
					decimal? val = (decimal?)graph.Caches[BqlCommand.GetItemType(field)].GetValue(((PXResult)a)[BqlCommand.GetItemType(field)], field.Name);
					ret += (val ?? 0m);
				}
				));
			}

			return ret;
		}

		protected virtual void AddTaxTotals(PXCache sender, string taxID, object row)
		{
			PXCache cache = sender.Graph.Caches[_TaxSumType];

			object newdet = Activator.CreateInstance(_TaxSumType);
			((TaxDetail)newdet).TaxID = taxID;
			newdet = InitializeTaxDet(newdet);
			object insdet = Insert(cache, newdet);
		}

		protected Terms SelectTerms(PXGraph graph)
		{
			string TermsID = (string)ParentGetValue(graph, _TermsID);
			Terms ret = TermsAttribute.SelectTerms(graph, TermsID);
			ret = ret ?? new Terms();

			return ret;
		}

		protected virtual void SetTaxableAmt(PXCache sender, object row, decimal? value)
		{
		}

		protected virtual void SetTaxAmt(PXCache sender, object row, decimal? value)
		{
		}

		protected virtual bool IsDeductibleVATTax(Tax tax)
		{
			return tax?.DeductibleVAT == true;
		}

		protected virtual bool IsExemptTaxCategory(PXGraph graph, object row)
		{
			PXCache sender = graph.Caches[_ChildType];
			return IsExemptTaxCategory(sender, row);
		}

		protected virtual bool IsExemptTaxCategory(PXCache sender, object row)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.exemptedTaxReporting>() != true)
				return false;

			bool isExemptTaxCategory = false;
			string taxCategory = GetTaxCategory(sender, row);

			if (!string.IsNullOrEmpty(taxCategory))
			{
				TaxCategory category = (TaxCategory)PXSelect<
					TaxCategory,
					Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>
					.Select(sender.Graph, taxCategory);

				isExemptTaxCategory = category?.Exempt == true;
			}

			return isExemptTaxCategory;
		}

		protected abstract decimal? GetTaxableAmt(PXCache sender, object row);

		protected abstract decimal? GetTaxAmt(PXCache sender, object row);

		protected List<object> SelectInclusiveTaxes(PXGraph graph, object row)
		{
			List<object> res = new List<object>();

			if (IsExemptTaxCategory(graph, row))
			{
				return res;
			}

			string calcMode = _isTaxCalcModeEnabled ? GetTaxCalcMode(graph) : TaxCalculationMode.TaxSetting;

			if (!_isTaxCalcModeEnabled || calcMode == TaxCalculationMode.TaxSetting)
			{
				res = SelectTaxes<Where<
						Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
						And<Tax.taxType, NotEqual<CSTaxType.withholding>,
						And<Tax.directTax, Equal<False>>>>>(graph, row, PXTaxCheck.Line);
			}
			else if (calcMode == TaxCalculationMode.Gross)
			{
				res = SelectTaxes<Where<
						Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
						And<Tax.taxType, NotEqual<CSTaxType.withholding>,
						And<Tax.directTax, Equal<False>>>>>(graph, row, PXTaxCheck.Line);
			}

			return res;
		}

		protected List<object> SelectLvl1Taxes(PXGraph graph, object row)
		{
			return
				IsExemptTaxCategory(graph, row)
					? new List<object>()
					: SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.calcOnItemAmt>,
				And<Tax.taxCalcLevel2Exclude, Equal<False>>>>(graph, row, PXTaxCheck.Line);
		}

		protected virtual void TaxSetLineDefault(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException(nameof(taxrow), ErrorMessages.ArgumentNullException);
			}

			TaxDetail taxDetail = (TaxDetail)((PXResult)taxrow)[0];
			Tax tax = PXResult.Unwrap<Tax>(taxrow);
			TaxRev taxRev = PXResult.Unwrap<TaxRev>(taxrow);

			if (taxRev.TaxID == null)
			{
				taxRev.TaxableMin = 0m;
				taxRev.TaxableMax = 0m;
				taxRev.TaxRate = 0m;
			}

			if (IsPerUnitTax(tax))
			{
				TaxSetLineDefaultForPerUnitTaxes(sender, row, tax, taxRev, taxDetail);
				return;
			}

			PXCache cache = sender.Graph.Caches[_TaxType];
			decimal curyTranAmt = (decimal)GetCuryTranAmt(sender, row, tax.TaxCalcType);
			Terms terms = SelectTerms(sender.Graph);

			List<object> inclusiveTaxes = SelectInclusiveTaxes(sender.Graph, row);
			decimal curyInclTaxAmt = SumWithReverseAdjustment(sender.Graph, inclusiveTaxes, GetFieldType(cache, _CuryTaxAmt));
			decimal curyInclTaxDiscountAmt = 0m;
			Type curyTaxDiscountAmtField = GetFieldType(cache, _CuryTaxDiscountAmt);

			if (curyTaxDiscountAmtField != null)
			{
				curyInclTaxDiscountAmt = SumWithReverseAdjustment(sender.Graph, inclusiveTaxes, curyTaxDiscountAmtField);
			}

			decimal curyTaxableAmt = 0.0m;
			decimal curyTaxableDiscountAmt = 0.0m;
			decimal taxableAmt = 0.0m;
			decimal curyTaxAmt = 0.0m;
			decimal curyTaxDiscountAmt = 0.0m;

			DiscPercentsDict.TryGetValue(ParentRow(sender.Graph), out decimal? discPercent);

			decimal calculatedTaxRate = (decimal)taxRev.TaxRate / 100;
			decimal undiscountedPercent = 1 - (discPercent ?? terms.DiscPercent ?? 0m) / 100;

			switch (tax.TaxCalcLevel)
			{
				case CSTaxCalcLevel.Inclusive:
					 (curyTaxableAmt, curyTaxAmt) = CalculateInclusiveTaxAmounts(sender, row, cache, taxDetail, inclusiveTaxes,
																				 calculatedTaxRate, curyTranAmt);
					break;
				case CSTaxCalcLevel.CalcOnItemAmt:
					{
						decimal curyPerUnitTaxAmount = GetPerUnitTaxAmountForTaxableAdjustmentCalculation(tax, taxDetail, cache, row, sender);
						curyTaxableAmt = curyTranAmt - curyInclTaxAmt - curyInclTaxDiscountAmt + curyPerUnitTaxAmount;
						break;
					}
				case CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt:
					{
						decimal curyPerUnitTaxAmount = GetPerUnitTaxAmountForTaxableAdjustmentCalculation(tax, taxDetail, cache, row, sender);
						List<object> lvl1Taxes = SelectLvl1Taxes(sender.Graph, row);

						decimal curyLevel1TaxAmt = SumWithReverseAdjustment(sender.Graph, lvl1Taxes, GetFieldType(cache, _CuryTaxAmt));

						curyTaxableAmt = curyTranAmt - curyInclTaxAmt + curyLevel1TaxAmt - curyInclTaxDiscountAmt + curyPerUnitTaxAmount;
						break;
					}
			}

			ApplyDiscounts(tax, sender, row, undiscountedPercent, calculatedTaxRate,
						   ref curyTaxableAmt, ref curyTaxableDiscountAmt, ref curyTaxDiscountAmt, ref curyTaxAmt);

			if (tax.TaxCalcType == CSTaxCalcType.Item
				&& (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt
					|| tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt))
			{
				if (cache.Fields.Contains(_CuryOrigTaxableAmt))
				{
					cache.SetValue(taxDetail, _CuryOrigTaxableAmt, PXDBCurrencyAttribute.RoundCury(cache, taxDetail, curyTaxableAmt, Precision));
				}

				AdjustMinMaxTaxableAmt(cache, taxDetail, taxRev, ref curyTaxableAmt, ref taxableAmt);

				curyTaxAmt = curyTaxableAmt * calculatedTaxRate;
				curyTaxDiscountAmt = curyTaxableDiscountAmt * calculatedTaxRate;

				if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount)
				{
					curyTaxAmt *= undiscountedPercent;
				}
			}
			else if (tax.TaxCalcType != CSTaxCalcType.Item)
			{
				//curyTaxAmt = 0.0m;
				//curyTaxDiscountAmt = 0.0m;
			}

			taxDetail.TaxRate = taxRev.TaxRate;
			taxDetail.NonDeductibleTaxRate = taxRev.NonDeductibleTaxRate;
			SetValueOptional(cache, taxDetail, PXDBCurrencyAttribute.RoundCury(cache, taxDetail, curyTaxableDiscountAmt), _CuryTaxableDiscountAmt);
			SetValueOptional(cache, taxDetail, PXDBCurrencyAttribute.RoundCury(cache, taxDetail, curyTaxDiscountAmt), _CuryTaxDiscountAmt);

			decimal roundedCuryTaxableAmt = PXDBCurrencyAttribute.RoundCury(cache, taxDetail, curyTaxableAmt, Precision);

			bool isExemptTaxCategory = IsExemptTaxCategory(sender, row);
			if (isExemptTaxCategory)
			{
				SetTaxDetailExemptedAmount(cache, taxDetail, roundedCuryTaxableAmt);
			}
			else
			{
				SetTaxDetailTaxableAmount(cache, taxDetail, roundedCuryTaxableAmt);
			}

			decimal roundedCuryTaxAmt = PXDBCurrencyAttribute.RoundCury(cache, taxDetail, curyTaxAmt, Precision);

			if (IsDeductibleVATTax(tax))
			{
				taxDetail.CuryExpenseAmt = PXDBCurrencyAttribute.RoundCury(cache, taxDetail, curyTaxAmt * (1 - (taxRev.NonDeductibleTaxRate ?? 0m) / 100), Precision);
				curyTaxAmt = roundedCuryTaxAmt - (decimal)taxDetail.CuryExpenseAmt;

				decimal expenseAmt;
				PXDBCurrencyAttribute.CuryConvBase(cache, taxDetail, taxDetail.CuryExpenseAmt.Value, out expenseAmt);
				taxDetail.ExpenseAmt = expenseAmt;
			}
			else
			{
				curyTaxAmt = roundedCuryTaxAmt;
			}

			if (!isExemptTaxCategory)
			{
				SetTaxDetailTaxAmount(cache, taxDetail, curyTaxAmt);
			}

			if (taxRev.TaxID != null && tax.DirectTax != true)
			{
				cache.Update(taxDetail);
				if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive)
				{
					sender.MarkUpdated(row);
				}
			}
			else
			{
				Delete(cache, taxDetail);
			}
		}

		private (decimal InclTaxTaxable, decimal InclTaxAmount) CalculateInclusiveTaxAmounts(PXCache sender, object row, PXCache cache,
																							 TaxDetail nonPerUnitTaxDetail, List<object> inclusiveTaxes,
																							 in decimal calculatedTaxRate, in decimal curyTranAmt)
		{
			var (inclusivePerUnitTaxesIncludedInTaxOnTaxCalc, 
				 inclusivePerUnitTaxesExcludedFromTaxOnTaxCalc, inclusiveNonPerUnitTaxes) = SegregateInclusiveTaxes(inclusiveTaxes);

			decimal curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc = 0m;
			decimal curyPerUnitTaxAmountExcludedFromTaxOnTaxCalc = 0m;
			decimal totalInclusiveNonPerUnitTaxRate = SumWithReverseAdjustment(sender.Graph, inclusiveNonPerUnitTaxes, typeof(TaxRev.taxRate)) / 100;
			Type taxDetailsCuryTaxAmountFieldType = cache.GetBqlField(_CuryTaxAmt);

			if (taxDetailsCuryTaxAmountFieldType != null)
			{
				curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc = 
					SumWithReverseAdjustment(sender.Graph, inclusivePerUnitTaxesIncludedInTaxOnTaxCalc, taxDetailsCuryTaxAmountFieldType);

				curyPerUnitTaxAmountExcludedFromTaxOnTaxCalc = 
					SumWithReverseAdjustment(sender.Graph, inclusivePerUnitTaxesExcludedFromTaxOnTaxCalc, taxDetailsCuryTaxAmountFieldType);
			}

			//The general formula for line Taxable Amount with the consideration for Per-Unit taxes:
			// Taxable = (LineAmount - PerUnitTaxesAmountExcluded from Tax On Tax Calculation) / (1 + Total Rate) - PerUnitTaxesAmountIncluded in Tax On Tax Calculation
			decimal curyRealTaxableAmt =
				((curyTranAmt - curyPerUnitTaxAmountExcludedFromTaxOnTaxCalc) / (1 + totalInclusiveNonPerUnitTaxRate)) - curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc;

			//Taxable for inclusive non per-unit taxes should be 
			decimal curyTaxableForNonPerUnitInclusiveTax = curyRealTaxableAmt + curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc;

			//Calculate tax amount for non per-unit inclusive tax by multiplying taxable on its tax rate
			decimal nonPerUnitTaxAmount = curyTaxableForNonPerUnitInclusiveTax * calculatedTaxRate;
			decimal curyNonPerUnitTaxAmount = PXDBCurrencyAttribute.RoundCury(cache, nonPerUnitTaxDetail, nonPerUnitTaxAmount, Precision);	

			//Recalculate total inclusive amount 
			decimal curyTotalInclusiveTaxAmt = curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc + curyPerUnitTaxAmountExcludedFromTaxOnTaxCalc;
			var taxRevCache = sender.Graph.Caches[typeof(TaxRev)];

			foreach (PXResult inclusiveNonPerUnitTaxRow in inclusiveNonPerUnitTaxes)
			{
				object inclusiveTaxRevision = inclusiveNonPerUnitTaxRow[typeof(TaxRev)];
				Tax currentInclusiveTax = (Tax)inclusiveNonPerUnitTaxRow[typeof(Tax)];
				decimal? taxRate = taxRevCache.GetValue<TaxRev.taxRate>(inclusiveTaxRevision) as decimal?;
				decimal multiplier = currentInclusiveTax.ReverseTax == true
					? Decimal.MinusOne
					: Decimal.One;

				decimal curyCurrentInclusiveTaxAmount = (curyTaxableForNonPerUnitInclusiveTax * taxRate / 100m) ?? 0m;
				curyCurrentInclusiveTaxAmount = PXDBCurrencyAttribute.RoundCury(cache, nonPerUnitTaxDetail, curyCurrentInclusiveTaxAmount, Precision) * multiplier;

				curyTotalInclusiveTaxAmt += curyCurrentInclusiveTaxAmount;
			}

			curyRealTaxableAmt = curyTranAmt - curyTotalInclusiveTaxAmt;
			curyTaxableForNonPerUnitInclusiveTax = curyRealTaxableAmt + curyInclusivePerUnitTaxAmountIncludedInTaxOnTaxCalc;

			SetTaxableAmt(sender, row, curyRealTaxableAmt);
			SetTaxAmt(sender, row, curyTotalInclusiveTaxAmt);

			return (curyTaxableForNonPerUnitInclusiveTax, curyNonPerUnitTaxAmount);
		}

		private (List<object> PerUnitTaxesIncludedInTaxOnTaxCalc, List<object> PerUnitTaxesExcludedFromTaxOnTaxCalc, List<object> NonPerUnitTaxes) SegregateInclusiveTaxes(List<object> inclusiveTaxes)
		{
			List<object> perUnitTaxesIncludedInTaxOnTaxCalc = new List<object>();
			List<object> perUnitTaxesExcludedFromTaxOnTaxCalc = new List<object>();
			List<object> inclusiveNonPerUnitTaxes = new List<object>(capacity: inclusiveTaxes.Count);

			foreach (PXResult inclusiveTaxRow in inclusiveTaxes)
			{
				Tax inclusiveTax = inclusiveTaxRow.GetItem<Tax>();

				if (inclusiveTax == null)
					continue;

				if (!IsPerUnitTax(inclusiveTax))
				{
					inclusiveNonPerUnitTaxes.Add(inclusiveTaxRow);
				}
				else if (inclusiveTax.TaxCalcLevel2Exclude == true)
				{
					perUnitTaxesExcludedFromTaxOnTaxCalc.Add(inclusiveTaxRow);
				}
				else
				{
					perUnitTaxesIncludedInTaxOnTaxCalc.Add(inclusiveTaxRow);
				}
			}

			return (perUnitTaxesIncludedInTaxOnTaxCalc, perUnitTaxesExcludedFromTaxOnTaxCalc, inclusiveNonPerUnitTaxes);
		}

		private void ApplyDiscounts(Tax tax, PXCache sender, object row, decimal undiscountedPercent, decimal calculatedTaxRate,
									ref decimal curyTaxableAmt, ref decimal curyTaxableDiscountAmt, ref decimal curyTaxDiscountAmt, ref decimal curyTaxAmt)
		{
			if (ConsiderDiscount(tax))
			{
				curyTaxableAmt *= undiscountedPercent;
			}
			else if (ConsiderEarlyPaymentDiscountDetail(sender, row, tax))
			{
				curyTaxableDiscountAmt = curyTaxableAmt * (1m - undiscountedPercent);
				curyTaxableAmt *= undiscountedPercent;
				curyTaxDiscountAmt = curyTaxableDiscountAmt * calculatedTaxRate;
			}
			else if (ConsiderInclusiveDiscountDetail(sender, row, tax))
			{
				curyTaxableDiscountAmt = curyTaxableAmt * (1m - undiscountedPercent);
				curyTaxDiscountAmt = curyTaxableDiscountAmt * calculatedTaxRate;
				curyTaxableAmt *= undiscountedPercent;
				curyTaxAmt *= undiscountedPercent;
			}
		}

		protected virtual bool ConsiderDiscount(Tax tax)
		{
			return (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt
								|| tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
							&& tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount;
		}
		private bool ConsiderEarlyPaymentDiscountDetail(PXCache sender, object detail, Tax tax)
		{
			object parent = PXParentAttribute.SelectParent(sender, detail, _ParentType);
			return ConsiderEarlyPaymentDiscount(sender, parent, tax);
		}
		private bool ConsiderInclusiveDiscountDetail(PXCache sender, object detail, Tax tax)
		{
			object parent = PXParentAttribute.SelectParent(sender, detail, _ParentType);
			return ConsiderInclusiveDiscount(sender, parent, tax);
		}
		protected virtual bool ConsiderEarlyPaymentDiscount(PXCache sender, object parent, Tax tax)
		{
			return false;
		}
		protected virtual bool ConsiderInclusiveDiscount(PXCache sender, object parent, Tax tax)
		{
			return false;
		}

		protected virtual void SetTaxDetailTaxableAmount(PXCache cache, TaxDetail taxdet, decimal? curyTaxableAmt)
		{
			cache.SetValue(taxdet, _CuryTaxableAmt, curyTaxableAmt);
		}

		protected virtual void SetTaxDetailExemptedAmount(PXCache cache, TaxDetail taxdet, decimal? curyExemptedAmt)
		{
			if (!string.IsNullOrEmpty(_CuryExemptedAmt))
			{
				cache.SetValue(taxdet, _CuryExemptedAmt, curyExemptedAmt);
			}
		}

		protected virtual void SetTaxDetailTaxAmount(PXCache cache, TaxDetail taxdet, decimal? curyTaxAmt)
		{
			cache.SetValue(taxdet, _CuryTaxAmt, curyTaxAmt);
		}

		#region CuryOrigDiscAmt TaxRecalculation
		[Obsolete("This method is obsolete and will be removed in future versions of Acumatica. Use PX.Objects.Common.SquareEquationSolver instead")]
		public static Pair<double, double> SolveQuadraticEquation(double a, double b, double c)
		{
			var roots = Common.SquareEquationSolver.SolveQuadraticEquation(a, b, c);
			return roots.HasValue
				? new Pair<double, double>(roots.Value.X1, roots.Value.X2)
				: null;
		}

		private Dictionary<object, bool> OrigDiscAmtExtCallDict = new Dictionary<object, bool>();
		private Dictionary<object, decimal?> DiscPercentsDict = new Dictionary<object, decimal?>();

		protected virtual void CuryOrigDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null)
				return;

			OrigDiscAmtExtCallDict[e.Row] = e.ExternalCall;
		}

		protected virtual bool ShouldUpdateFinPeriodID(PXCache sender, object oldRow, object newRow)
		{
			return (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
				   && (string)sender.GetValue(oldRow, _FinPeriodID) != (string)sender.GetValue(newRow, _FinPeriodID);
		}

		protected virtual void ParentRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row == null)
				return;

			int? oldBranchID = null;
			int? newBranchID = null;

			if (ParentBranchIDField != null)
			{
				oldBranchID = (int?)sender.GetValue(e.OldRow, ParentBranchIDField.Name);
				newBranchID = (int?)sender.GetValue(e.Row, ParentBranchIDField.Name);
			}

			if (oldBranchID != newBranchID
				|| ShouldUpdateFinPeriodID(sender, e.OldRow, e.Row))
			{
				PXCache cache = sender.Graph.Caches[_TaxSumType];
				List<object> details = TaxParentAttribute.ChildSelect(cache, e.Row, _ParentType);
				foreach (object det in details)
				{
					if (oldBranchID != newBranchID)
					{
						cache.SetDefaultExt(det, ChildBranchIDField.Name);
					}

					if (ShouldUpdateFinPeriodID(sender, e.OldRow, e.Row))
					{
						cache.SetDefaultExt(det, ChildFinPeriodIDField.Name);
					}

					cache.MarkUpdated(det);
				}
			}

			bool externallCall = false;
			OrigDiscAmtExtCallDict.TryGetValue(e.Row, out externallCall);
			if (!externallCall)
				return;

			decimal newDiscAmt = ((decimal?)sender.GetValue(e.Row, _CuryOrigDiscAmt)).GetValueOrDefault();
			decimal oldDiscAmt = ((decimal?)sender.GetValue(e.OldRow, _CuryOrigDiscAmt)).GetValueOrDefault();

			if (newDiscAmt != oldDiscAmt && !DiscPercentsDict.ContainsKey(e.Row))
			{
				DiscPercentsDict.Add(e.Row, 0m);
				PXFieldUpdatedEventArgs args = new PXFieldUpdatedEventArgs(e.Row, oldDiscAmt, false);

				using (new TermsAttribute.UnsubscribeCalcDiscScope(sender))
				{
					try
					{
						if (newDiscAmt == 0m)
							return;

						ParentFieldUpdated(sender, args);
						DiscPercentsDict[e.Row] = null;

						bool considerEarlyPaymentDiscount = false;
						decimal reducedTaxAmt = 0m;
						PXCache cache = sender.Graph.Caches[_TaxSumType];

						foreach (object taxitem in SelectTaxes(sender, e.Row, PXTaxCheck.RecalcTotals))
						{
							object taxsum = ((PXResult)taxitem)[0];
							Tax tax = PXResult.Unwrap<Tax>(taxitem);

							if (RecalcTaxableRequired(tax))
							{
								reducedTaxAmt += (tax.ReverseTax == true ? -1m : 1m) * (((decimal?)cache.GetValue(taxsum, _CuryTaxAmt)).GetValueOrDefault() +
									(IsDeductibleVATTax(tax) ? ((decimal?)cache.GetValue(taxsum, _CuryExpenseAmt)).GetValueOrDefault() : 0m));
							}
							else if (ConsiderEarlyPaymentDiscount(sender, e.Row, tax) || ConsiderInclusiveDiscount(sender, e.Row, tax))
							{
								considerEarlyPaymentDiscount = true;
								break; //as combination of reduce taxable and reduce taxable on early payment is forbidden, we can skip further calculation
							}
						}
						if (considerEarlyPaymentDiscount)
						{
							decimal curyDocBal = ((decimal?)sender.GetValue(e.Row, _CuryDocBal)).GetValueOrDefault();
							DiscPercentsDict[e.Row] = 100 * newDiscAmt / curyDocBal;
						}
						else if (reducedTaxAmt != 0m)
						{
							decimal curyDocBal = ((decimal?)sender.GetValue(e.Row, _CuryDocBal)).GetValueOrDefault();
							DiscPercentsDict[e.Row] = CalculateCashDiscountPercent(curyDocBal, reducedTaxAmt, newDiscAmt);							
						}
					}
					catch
					{
						DiscPercentsDict[e.Row] = null;
					}
					finally
					{
						ParentFieldUpdated(sender, args);
						sender.RaiseRowUpdated(e.Row, e.OldRow);

						OrigDiscAmtExtCallDict.Remove(e.Row);
						DiscPercentsDict.Remove(e.Row);
					}
				}
			}
		}

		private decimal? CalculateCashDiscountPercent(decimal curyDocBalanceOld, decimal reducableTaxAmountOld, decimal newCashDiscountAmount)
		{
			var roots = Common.SquareEquationSolver.SolveQuadraticEquation(reducableTaxAmountOld, -curyDocBalanceOld, newCashDiscountAmount);

			if (roots == null)
				return null;
			
			var (x1, x2) = roots.Value;
			return x1 >= 0 && x1 <= 1
					? x1 * 100
					: x2 >= 0 && x2 <= 1
						? x2 * 100
						: (decimal?)null;		
		}

		protected virtual bool RecalcTaxableRequired(Tax tax)
		{
			return tax?.TaxCalcLevel != CSTaxCalcLevel.Inclusive &&
											tax?.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount;
		}

		#endregion

		protected virtual void AdjustTaxableAmount(PXCache cache, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
		{
		}

		protected virtual void AdjustExemptedAmount(PXCache cache, object row, List<object> taxitems, ref decimal CuryExemptedAmt, string TaxCalcType)
		{
		}

		protected virtual TaxDetail CalculateTaxSum(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
			}

			PXCache cache = sender.Graph.Caches[_TaxType];
			PXCache sumcache = sender.Graph.Caches[_TaxSumType];

			TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
			Tax tax = PXResult.Unwrap<Tax>(taxrow);
			TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

			if (taxrev.TaxID == null)
			{
				taxrev.TaxableMin = 0m;
				taxrev.TaxableMax = 0m;
				taxrev.TaxRate = 0m;
			}

			decimal curyOrigTaxableAmt = 0m;
			decimal CuryTaxAmtSumm = 0m;
			decimal CuryTaxableAmt = 0.0m;
			decimal CuryTaxableDiscountAmt = 0.0m;
			decimal TaxableAmt = 0.0m;
			decimal CuryTaxAmt = 0.0m;
			decimal CuryTaxDiscountAmt = 0.0m;
			decimal CuryLevel1TaxAmt = 0.0m;
			decimal CuryExpenseAmt = 0.0m;
			decimal CuryExemptedAmt = 0.0m;

			List<object> taxitems = SelectTaxes<Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, row, PXTaxCheck.RecalcLine, taxdet.TaxID);

			if (taxitems.Count == 0 || taxrev.TaxID == null)
			{
				return null;
			}

			if (tax.TaxCalcType == CSTaxCalcType.Item)
			{
				if (cache.Fields.Contains(_CuryOrigTaxableAmt))
				{
					curyOrigTaxableAmt = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryOrigTaxableAmt));
				}

				CuryTaxableAmt = Sum(sender.Graph,
					taxitems,
					GetFieldType(cache, _CuryTaxableAmt));

				Type curyTaxableDiscountAmtField = GetFieldType(cache, _CuryTaxableDiscountAmt);
				if (curyTaxableDiscountAmtField != null)
				{
					CuryTaxableDiscountAmt = Sum(sender.Graph,
					taxitems,
					curyTaxableDiscountAmtField);
				}

				AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

				CuryTaxAmt = CuryTaxAmtSumm = Sum(sender.Graph,
					taxitems,
					GetFieldType(cache, _CuryTaxAmt));

				Type curyTaxDiscountAmtField = GetFieldType(cache, _CuryTaxDiscountAmt);
				if (curyTaxDiscountAmtField != null)
				{
					CuryTaxDiscountAmt = Sum(sender.Graph, taxitems, curyTaxDiscountAmtField);
				}

				CuryExpenseAmt = Sum(sender.Graph,
					taxitems,
					GetFieldType(cache, _CuryExpenseAmt));
			}
			else if (
				tax.TaxType  != CSTaxType.Withholding && (
				CalcGrossOnDocumentLevel && _isTaxCalcModeEnabled && GetTaxCalcMode(sender.Graph) == TaxCalculationMode.Gross ||
				tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive && (!_isTaxCalcModeEnabled || GetTaxCalcMode(sender.Graph) != TaxCalculationMode.Net)))
			{
				CuryTaxableAmt = Sum(sender.Graph, taxitems, GetFieldType(cache, _CuryTaxableAmt));
				CuryTaxAmt = CuryTaxAmtSumm = Sum(sender.Graph, taxitems, GetFieldType(cache, _CuryTaxAmt));

				Type curyTaxableDiscountAmtField = GetFieldType(cache, _CuryTaxableDiscountAmt);
				if (curyTaxableDiscountAmtField != null)
				{
					CuryTaxableDiscountAmt = Sum(sender.Graph,
					taxitems,
					curyTaxableDiscountAmtField);
				}

				Type curyTaxDiscountAmtField = GetFieldType(cache, _CuryTaxDiscountAmt);
				if (curyTaxDiscountAmtField != null)
				{
					CuryTaxDiscountAmt = Sum(sender.Graph, taxitems, curyTaxDiscountAmtField);
				}

				var docLines = SelectDocumentLines(sender.Graph, row);
				if (docLines.Any())
				{
					var docLineCache = sender.Graph.Caches[docLines[0].GetType()];
					var realLineAmounts = docLines.ToDictionary(
						_ => (int)docLineCache.GetValue(_, _LineNbr),
						_ => GetDocLineFinalAmtNoRounding(docLineCache, _, tax.TaxCalcType) ?? 0.0m);

					List<object> alltaxitems = SelectTaxes(sender, row, PXTaxCheck.RecalcLine);
					var taxLines = alltaxitems.Select(_ => new
					{
						LineNbr = (int)cache.GetValue((TaxDetail)((PXResult)_)[0], _LineNbr),
						TaxID = PXResult.Unwrap<Tax>(_).TaxID,
						TaxRate = PXResult.Unwrap<TaxRev>(_).TaxRate ?? 0,
						TaxRateMultiplier = PXResult.Unwrap<Tax>(_).ReverseTax == true ? -1.0M : 1.0M
					});

					var currentTaxRate = (taxdet.TaxRate != 0.0m) ? taxdet.TaxRate : taxLines.First(_ => _.TaxID == taxdet.TaxID).TaxRate;
					var currentTaxLines = taxLines.Where(_ => _.TaxID == taxdet.TaxID).Select(_ => _.LineNbr).ToList();

					var groups = new List<InclusiveTaxGroup>();
					foreach (var lineNbr in currentTaxLines)
					{
						var lineTaxes = taxLines.Where(_ => _.LineNbr == lineNbr).OrderBy(_ => _.TaxID).ToList();
						var groupKey = string.Join("::", lineTaxes.Select(_ => _.TaxID));
						var sumTaxRate = lineTaxes.Sum(_ => _.TaxRate * _.TaxRateMultiplier);
						var lineAmt = realLineAmounts[lineNbr];
						if (groups.Any(g => g.Key == groupKey))
						{
							groups.Single(g => g.Key == groupKey).TotalAmount += lineAmt;
						}
						else
						{
							groups.Add(new InclusiveTaxGroup() { Key = groupKey, Rate = sumTaxRate, TotalAmount = lineAmt });
						}
					}

					CuryTaxAmt = groups.Sum(g => PXDBCurrencyAttribute.RoundCury(sender, taxdet,
						(g.TotalAmount / (1 + g.Rate / 100.0m) * currentTaxRate / 100.0m) ?? 0.0m, Precision))
						- CuryTaxDiscountAmt;

					CuryTaxableAmt = PXDBCurrencyAttribute.RoundCury(sender, taxdet,
						groups.Sum(g => g.TotalAmount / (1 + g.Rate / 100.0m)), Precision)
						- CuryTaxableDiscountAmt;
				}

				if (tax.DeductibleVAT == true)
					CuryExpenseAmt = CuryTaxAmt * (1.0M - (taxrev.NonDeductibleTaxRate ?? 0.0M) / 100.0M);
			}
			else
			{
				List<object> lvl1Taxes = SelectLvl1Taxes(sender.Graph, row);

				if (_NoSumTaxable && (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt || lvl1Taxes.Count == 0))
				{
					// When changing doc date will 
					// not recalculate taxable amount
					//
					CuryTaxableAmt = (decimal)sumcache.GetValue(taxdet, _CuryTaxableAmt);
					CuryTaxableDiscountAmt = GetOptionalDecimalValue(sumcache, taxdet, _CuryTaxableDiscountAmt);
				}
				else
				{
					CuryTaxableAmt = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryTaxableAmt));

					CuryTaxAmtSumm = Sum(sender.Graph, taxitems, GetFieldType(cache, _CuryTaxAmt));
					CuryTaxAmtSumm = PXDBCurrencyAttribute.RoundCury(sender, taxdet, CuryTaxAmtSumm, Precision);

					Type curyTaxableDiscountAmtField = GetFieldType(cache, _CuryTaxableDiscountAmt);
					if (curyTaxableDiscountAmtField != null)
					{
						CuryTaxableDiscountAmt = Sum(sender.Graph, taxitems, curyTaxableDiscountAmtField);
					}

					AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

					if (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
					{
						CuryLevel1TaxAmt = Sum(sender.Graph, lvl1Taxes, GetFieldType(sumcache, _CuryTaxAmt));
						CuryTaxableAmt += CuryLevel1TaxAmt;
					}
				}

				curyOrigTaxableAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxableAmt, Precision);

				AdjustMinMaxTaxableAmt(sumcache, taxdet, taxrev, ref CuryTaxableAmt, ref TaxableAmt);

				CuryTaxAmt = CuryTaxableAmt * (decimal)taxrev.TaxRate / 100;
				CuryTaxAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxAmt, Precision);

				CuryTaxDiscountAmt = CuryTaxableDiscountAmt * (decimal)taxrev.TaxRate / 100;

				AdjustExpenseAmt(tax, taxrev, CuryTaxAmt, ref CuryExpenseAmt);
				AdjustTaxAmtOnDiscount(sender, tax, ref CuryTaxAmt);
			}

			taxdet = (TaxDetail)sumcache.CreateCopy(taxdet);

			if (sumcache.Fields.Contains(_CuryOrigTaxableAmt))
			{
				sumcache.SetValue(taxdet, _CuryOrigTaxableAmt, curyOrigTaxableAmt);
			}

			CuryExemptedAmt = Sum(sender.Graph,
				taxitems,
				GetFieldType(cache, _CuryExemptedAmt));

			AdjustExemptedAmount(sender, row, taxitems, ref CuryExemptedAmt, tax.TaxCalcType);

			taxdet.TaxRate = taxrev.TaxRate;
			taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate;
			sumcache.SetValue(taxdet, _CuryTaxableAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxableAmt, Precision));
			sumcache.SetValue(taxdet, _CuryExemptedAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryExemptedAmt, Precision));
			sumcache.SetValue(taxdet, _CuryTaxAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxAmt, Precision));
			sumcache.SetValue(taxdet, _CuryTaxAmtSumm, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxAmtSumm, Precision));
			SetValueOptional(sumcache, taxdet, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxableDiscountAmt), _CuryTaxableDiscountAmt);
			taxdet.CuryExpenseAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryExpenseAmt, Precision);
			SetValueOptional(sumcache, taxdet, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxDiscountAmt), _CuryTaxDiscountAmt);

			if (IsDeductibleVATTax(tax) && tax.TaxCalcType != CSTaxCalcType.Item)
			{
				sumcache.SetValue(taxdet, _CuryTaxAmt,
					(decimal)(sumcache.GetValue(taxdet, _CuryTaxAmt) ?? 0m) -
					(decimal)(sumcache.GetValue(taxdet, _CuryExpenseAmt) ?? 0m));
			}

			if (IsPerUnitTax(tax))
			{
				taxdet = FillAggregatedTaxDetailForPerUnitTax(sender, row, tax, taxrev, taxdet, taxitems);
			}

			return taxdet;
		}

		protected class InclusiveTaxGroup
		{
			public string Key { get; set; }
			public decimal Rate { get; set; }
			public decimal TotalAmount { get; set; }
		}


		protected virtual void CalculateTaxSumTaxAmt(
			PXCache sender,
			TaxDetail taxdet,
			Tax tax,
			TaxRev taxrev)
		{
			if (tax.TaxType == CSTaxType.PerUnit)
			{
				PXTrace.WriteError(Messages.PerUnitTaxesNotSupportedOperation);
				throw new PXException(Messages.PerUnitTaxesNotSupportedOperation);
			}

			PXCache sumcache = sender.Graph.Caches[_TaxSumType];

			decimal taxableAmt = 0.0m;
			decimal curyExpenseAmt = 0.0m;

			decimal curyTaxableAmt = GetOptionalDecimalValue(sender, taxdet, _CuryTaxableAmt);
			decimal curyTaxableDiscountAmt = GetOptionalDecimalValue(sender, taxdet, _CuryTaxableDiscountAmt);
			decimal curyOrigTaxableAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxableAmt, Precision);

			decimal taxRate = taxrev.TaxRate ?? 0.0m;

			AdjustMinMaxTaxableAmt(sender, taxdet, taxrev, ref curyTaxableAmt, ref taxableAmt);

			decimal curyTaxAmt = curyTaxableAmt * taxRate / 100;
			decimal curyTaxDiscountAmt = curyTaxableDiscountAmt * taxRate / 100;

			AdjustExpenseAmt(tax, taxrev, curyTaxAmt, ref curyExpenseAmt);
			AdjustTaxAmtOnDiscount(sender, tax, ref curyTaxAmt);

			if (sumcache.Fields.Contains(_CuryOrigTaxableAmt))
			{
				sumcache.SetValue(taxdet, _CuryOrigTaxableAmt, curyOrigTaxableAmt);
			}

			taxdet.TaxRate = taxRate;
			taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate ?? 0.0m;
			sumcache.SetValue(taxdet, _CuryTaxableAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxableAmt, Precision));
			sumcache.SetValue(taxdet, _CuryTaxAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxAmt, Precision));
			SetValueOptional(sumcache, taxdet, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxableDiscountAmt), _CuryTaxableDiscountAmt);
			taxdet.CuryExpenseAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyExpenseAmt, Precision);
			SetValueOptional(sumcache, taxdet, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxDiscountAmt), _CuryTaxDiscountAmt);

			if (IsDeductibleVATTax(tax) && tax.TaxCalcType != CSTaxCalcType.Item)
			{
				sumcache.SetValue(taxdet, _CuryTaxAmt,
					(decimal)(sumcache.GetValue(taxdet, _CuryTaxAmt) ?? 0m) -
					(decimal)(sumcache.GetValue(taxdet, _CuryExpenseAmt) ?? 0m));
			}
		}

		private void AdjustExpenseAmt(
			Tax tax,
			TaxRev taxrev,
			decimal curyTaxAmt,
			ref decimal curyExpenseAmt)
		{
			if (IsDeductibleVATTax(tax))
			{
				curyExpenseAmt = curyTaxAmt * (1 - (taxrev.NonDeductibleTaxRate ?? 0m) / 100);
			}
		}

		private void AdjustTaxAmtOnDiscount(
			PXCache sender,
			Tax tax,
			ref decimal curyTaxAmt)
		{
			if ((tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt || tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt) &&
				tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount)
			{
				decimal? DiscPercent = null;
				DiscPercentsDict.TryGetValue(ParentRow(sender.Graph), out DiscPercent);

				Terms terms = SelectTerms(sender.Graph);

				curyTaxAmt = curyTaxAmt * (1 - (DiscPercent ?? terms.DiscPercent ?? 0m) / 100);
			}
		}

		private void AdjustMinMaxTaxableAmt(
			PXCache sumcache,
			TaxDetail taxdet,
			TaxRev taxrev,
			ref decimal curyTaxableAmt,
			ref decimal taxableAmt)
		{
			PXDBCurrencyAttribute.CuryConvBase(sumcache, taxdet, curyTaxableAmt, out taxableAmt);

			if (taxrev.TaxableMin != 0.0m)
			{
				if (taxableAmt < taxrev.TaxableMin)
				{
					curyTaxableAmt = 0.0m;
					taxableAmt = 0.0m;
				}
			}

			if (taxrev.TaxableMax != 0.0m)
			{
				if (taxableAmt > taxrev.TaxableMax)
				{
					PXDBCurrencyAttribute.CuryConvCury(sumcache, taxdet, (decimal)taxrev.TaxableMax, out curyTaxableAmt);
					taxableAmt = (decimal)taxrev.TaxableMax;
				}
			}
		}

		private static void SetValueOptional(PXCache cache, object data, object value, string field)
		{
			int ordinal = cache.GetFieldOrdinal(field);
			if (ordinal >= 0)
			{
				cache.SetValue(data, ordinal, value);
			}
		}

		private TaxDetail TaxSummarize(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
			}

			PXCache sumcache = sender.Graph.Caches[_TaxSumType];
			TaxDetail taxSum = CalculateTaxSum(sender, taxrow, row);

			if (taxSum != null)
			{
				return (TaxDetail)sumcache.Update(taxSum);
			}
			else
			{
				TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
				Delete(sumcache, taxdet);
				return null;
			}
		}

		protected virtual void CalcTaxes(PXCache sender, object row)
		{
			CalcTaxes(sender, row, PXTaxCheck.RecalcLine);
		}

		/// <summary>
		/// This method is intended to select document line for given tax row.
		/// Do not use it to select parent document foir given line.
		/// </summary>
		/// <param name="cache">Cache of the tax row.</param>
		/// <param name="row">Tax row for which line will be returned.</param>
		/// <returns>Document line object.</returns>
		protected virtual object SelectParent(PXCache cache, object row)
		{
			return PXParentAttribute.SelectParent(cache, row, _ChildType);
		}

		protected virtual void CalcTaxes(PXCache sender, object row, PXTaxCheck taxchk)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];

			object detrow = row;

			foreach (object taxrow in SelectTaxes(sender, row, taxchk))
			{
				if (row == null)
				{
					detrow = SelectParent(cache, ((PXResult)taxrow)[0]);
				}

				if (detrow != null)
				{
					TaxSetLineDefault(sender, taxrow, detrow);
				}
			}
			CalcTotals(sender, row, true);
		}

		public virtual IEnumerable<T> DistributeTaxDiscrepancy<T, CuryTaxField, BaseTaxField>(PXGraph graph, IEnumerable<T> taxDetList, decimal CuryTaxAmt, bool updateCache)
			where T : TaxDetail, ITranTax
			where CuryTaxField : IBqlField
			where BaseTaxField : IBqlField
		{
			decimal curyTaxSum = 0m;
			decimal curyTaxableSum = 0m;

			T maxDetail = null;
			PXCache taxDetCache = graph.Caches[_TaxType];

			foreach (var taxLine in taxDetList)
			{
				decimal curyTaxAmt = (decimal)taxDetCache.GetValue<CuryTaxField>(taxLine);
				decimal curyTaxableAmt = (decimal)(taxDetCache.GetValue(taxLine, _CuryTaxableAmt) ?? 0m);

				curyTaxSum += curyTaxAmt;
				curyTaxableSum += curyTaxableAmt;

				if (maxDetail == null)
				{
					maxDetail = taxLine;
				}
				else
				{
					decimal curyTaxableAmtMax = (decimal)(taxDetCache.GetValue(maxDetail, _CuryTaxableAmt) ?? 0m);
					if (Math.Abs(curyTaxableAmtMax) < Math.Abs(curyTaxableAmt))
					{
						maxDetail = taxLine;
					}
				}
			}

			decimal discrepancy = CuryTaxAmt - curyTaxSum;
			if (Math.Abs(discrepancy) > 0m)
			{
				decimal discrSum = 0m;
				foreach (T taxLine in taxDetList)
				{
					decimal partDiscr = PXDBCurrencyAttribute.RoundCury(taxDetCache, taxLine,
						discrepancy * (curyTaxableSum != 0 ? (decimal)(taxDetCache.GetValue(taxLine, _CuryTaxableAmt) ?? 0m) / curyTaxableSum : (1m / taxDetList.Count())));
					decimal curyTaxAmt = (decimal)taxDetCache.GetValue<CuryTaxField>(taxLine) + partDiscr;
					taxDetCache.SetValue<CuryTaxField>(taxLine, curyTaxAmt);
					discrSum += partDiscr;
					decimal taxAmt;
					PXDBCurrencyAttribute.CuryConvBase(taxDetCache, taxLine, curyTaxAmt, out taxAmt);
					taxDetCache.SetValue<BaseTaxField>(taxLine, taxAmt);

					if (updateCache)
					{
						taxDetCache.Update(taxLine);
					}
				}

				if (discrSum != discrepancy)
				{
					decimal curyTaxAmt = (decimal)taxDetCache.GetValue<CuryTaxField>(maxDetail) + discrepancy - discrSum;
					taxDetCache.SetValue<CuryTaxField>(maxDetail, curyTaxAmt);
					decimal taxAmt;
					PXDBCurrencyAttribute.CuryConvBase(taxDetCache, maxDetail, curyTaxAmt, out taxAmt);
					taxDetCache.SetValue<BaseTaxField>(maxDetail, taxAmt);

					if (updateCache)
					{
						taxDetCache.Update(maxDetail);
					}
				}
			}

			return taxDetList;
		}

		protected virtual void CalcDocTotals(
			PXCache sender,
			object row,
			decimal CuryTaxTotal,
			decimal CuryInclTaxTotal,
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			_CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);
		}

		protected virtual decimal CalcLineTotal(PXCache sender, object row)
		{
			decimal CuryLineTotal = 0m;

			object[] details = PXParentAttribute.SelectSiblings(sender, null);

			if (details != null)
			{
				foreach (object detrow in details)
				{
					CuryLineTotal += GetCuryTranAmt(sender, sender.ObjectsEqual(detrow, row) ? row : detrow) ?? 0m;
				}
			}
			return CuryLineTotal;
		}

		protected virtual void _CalcDocTotals(
			PXCache sender,
			object row,
			decimal CuryTaxTotal,
			decimal CuryInclTaxTotal,
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			decimal CuryLineTotal = CalcLineTotal(sender, row);

			decimal CuryDocTotal = CuryLineTotal + CuryTaxTotal - CuryInclTaxTotal;

			decimal doc_CuryLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);
			decimal doc_CuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);

			if (!Equals(CuryLineTotal, doc_CuryLineTotal) ||
				!Equals(CuryTaxTotal, doc_CuryTaxTotal))
			{
				ParentSetValue(sender.Graph, _CuryLineTotal, CuryLineTotal);
				ParentSetValue(sender.Graph, _CuryTaxTotal, CuryTaxTotal);

				if (!string.IsNullOrEmpty(_CuryDocBal))
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
					return;
				}
			}

			if (!string.IsNullOrEmpty(_CuryDocBal))
			{
				decimal doc_CuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);

				if (!Equals(CuryDocTotal, doc_CuryDocBal))
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
				}
			}
		}

		protected virtual void CalcTotals(PXCache sender, object row, bool CalcTaxes)
		{
			bool IsUseTax = false;

			decimal CuryTaxTotal = 0m;
			decimal CuryTaxDiscountTotal = 0m;
			decimal CuryInclTaxTotal = 0m;
			decimal CuryInclTaxDiscountTotal = 0m;
			decimal CuryWhTaxTotal = 0m;

			foreach (object taxrow in SelectTaxes(sender, row, PXTaxCheck.RecalcTotals))
			{
				TaxDetail taxdet = null;
				if (CalcTaxes)
				{
					taxdet = TaxSummarize(sender, taxrow, row);
				}
				else
				{
					taxdet = (TaxDetail)((PXResult)taxrow)[0];
				}

				if (taxdet != null && PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Use)
				{
					IsUseTax = true;
				}
				else if (taxdet != null)
				{
					PXCache taxDetCache = sender.Graph.Caches[taxdet.GetType()];
					decimal CuryTaxAmt = (decimal)taxDetCache.GetValue(taxdet, _CuryTaxAmt);
					decimal CuryTaxDiscountAmt = GetOptionalDecimalValue(taxDetCache, taxdet, _CuryTaxDiscountAmt);

					//assuming that tax cannot be withholding and reverse at the same time
					Decimal multiplier = PXResult.Unwrap<Tax>(taxrow).ReverseTax == true ? Decimal.MinusOne : Decimal.One;

					if (PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Withholding)
					{
						CuryWhTaxTotal += multiplier * CuryTaxAmt;
					}


					if (PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel == "0")
					{
						CuryInclTaxTotal += multiplier * CuryTaxAmt;
						CuryInclTaxDiscountTotal += multiplier * CuryTaxDiscountAmt;
					}

					CuryTaxTotal += multiplier * CuryTaxAmt;
					CuryTaxDiscountTotal += multiplier * CuryTaxDiscountAmt;

					if (IsDeductibleVATTax(PXResult.Unwrap<Tax>(taxrow)))
					{
						CuryTaxTotal += multiplier * (decimal)taxdet.CuryExpenseAmt;

						if (PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel == "0")
						{
							CuryInclTaxTotal += multiplier * (decimal)taxdet.CuryExpenseAmt;
						}
					}
				}
			}

			if (ParentGetStatus(sender.Graph) != PXEntryStatus.Deleted && ParentGetStatus(sender.Graph) != PXEntryStatus.InsertedDeleted)
			{
				CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal + CuryInclTaxDiscountTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);
			}

			if (IsUseTax && !sender.Graph.UnattendedMode)
			{
				ParentCache(sender.Graph).RaiseExceptionHandling(_CuryTaxTotal, ParentRow(sender.Graph), CuryTaxTotal,
					new PXSetPropertyException(Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}
		}

		private decimal GetOptionalDecimalValue(PXCache cache, object data, string field)
		{
			decimal value = 0m;
			int fieldOrdinal = cache.GetFieldOrdinal(field);
			if (fieldOrdinal >= 0)
			{
				value = (decimal)(cache.GetValue(data, fieldOrdinal) ?? 0m);
			}
			return value;
		}

		protected virtual PXCache ParentCache(PXGraph graph)
		{
			return graph.Caches[_ParentType];
		}

		protected virtual object ParentRow(PXGraph graph)
		{
			if (_ParentRow == null)
			{
				return ParentCache(graph).Current;
			}
			else
			{
				return _ParentRow;
			}
		}

		protected virtual PXEntryStatus ParentGetStatus(PXGraph graph)
		{
			PXCache cache = ParentCache(graph);
			if (_ParentRow == null)
			{
				return cache.GetStatus(cache.Current);
			}
			else
			{
				return cache.GetStatus(_ParentRow);
			}
		}

		protected virtual void ParentSetValue(PXGraph graph, string fieldname, object value)
		{
			PXCache cache = ParentCache(graph);

			if (_ParentRow == null)
			{
				object copy = cache.CreateCopy(cache.Current);
				cache.SetValueExt(cache.Current, fieldname, value);
				cache.MarkUpdated(cache.Current);
				cache.RaiseRowUpdated(cache.Current, copy);
			}
			else
			{
				cache.SetValueExt(_ParentRow, fieldname, value);
			}
		}

		protected virtual object ParentGetValue(PXGraph graph, string fieldname)
		{
			PXCache cache = ParentCache(graph);
			if (_ParentRow == null)
			{
				return cache.GetValue(cache.Current, fieldname);
			}
			else
			{
				return cache.GetValue(_ParentRow, fieldname);
			}
		}

		protected object ParentGetValue<Field>(PXGraph graph)
			where Field : IBqlField
		{
			return ParentGetValue(graph, typeof(Field).Name.ToLower());
		}

		protected void ParentSetValue<Field>(PXGraph graph, object value)
			where Field : IBqlField
		{
			ParentSetValue(graph, typeof(Field).Name.ToLower(), value);
		}

		protected virtual bool CompareZone(PXGraph graph, string zoneA, string zoneB)
		{
			if (!string.Equals(zoneA, zoneB, StringComparison.OrdinalIgnoreCase))
			{
				foreach (PXResult<TaxZoneDet> r in PXSelectGroupBy<TaxZoneDet, Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>, Or<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>>>, Aggregate<GroupBy<TaxZoneDet.taxID, Count>>>.Select(graph, zoneA, zoneB))
				{
					if (r.RowCount == 1)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXRowInsertedSubscriber) ||
				typeof(ISubscriber) == typeof(IPXRowUpdatedSubscriber) ||
				typeof(ISubscriber) == typeof(IPXRowDeletedSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is IPXRowInsertedSubscriber)
					{
						((IPXRowInsertedSubscriber)_Attributes[i]).RowInserted(sender, e);
					}
				}

				object copy;
				if (!inserted.TryGetValue(e.Row, out copy))
				{
					inserted[e.Row] = sender.CreateCopy(e.Row);
				}
			}

			decimal? val;
			if (GetTaxCategory(sender, e.Row) == null && ((val = GetCuryTranAmt(sender, e.Row)) == null || val == 0m))
			{
				return;
			}

			if (_TaxCalc == TaxCalc.Calc)
			{
				Preload(sender);

				DefaultTaxes(sender, e.Row);
				CalcTaxes(sender, e.Row, PXTaxCheck.Line);
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, false);
			}
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is IPXRowUpdatedSubscriber)
					{
						((IPXRowUpdatedSubscriber)_Attributes[i]).RowUpdated(sender, e);
					}
				}

				object copy;
				if (!updated.TryGetValue(e.Row, out copy))
				{
					updated[e.Row] = sender.CreateCopy(e.Row);
				}
			}

			if (_TaxCalc == TaxCalc.Calc)
			{
				if (!object.Equals(GetTaxCategory(sender, e.OldRow), GetTaxCategory(sender, e.Row)))
				{
					Preload(sender);
					ReDefaultTaxes(sender, e.OldRow, e.Row);
				}
				else if (!object.Equals(GetTaxID(sender, e.OldRow), GetTaxID(sender, e.Row)))
				{
					PXCache cache = sender.Graph.Caches[_TaxType];
					TaxDetail taxDetail = (TaxDetail)cache.CreateInstance();
					taxDetail.TaxID = GetTaxID(sender, e.OldRow);
					DelOneTax(cache, e.Row, taxDetail);
					AddOneTax(cache, e.Row, new TaxZoneDet() { TaxID = GetTaxID(sender, e.Row) });
				}

				bool calculated = false;

				if (ShouldRecalculateTaxesOnRowUpdate(sender, e.Row, e.OldRow))
				{
					CalcTaxes(sender, e.Row, PXTaxCheck.Line);
					calculated = true;
				}

				if (!calculated)
				{
					CalcTotals(sender, e.Row, false);
				}
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, false);
			}
		}

		private bool ShouldRecalculateTaxesOnRowUpdate(PXCache rowCache, object newRow, object oldRow)
		{
			string oldTaxCategory = GetTaxCategory(rowCache, oldRow);
			string newTaxCategory = GetTaxCategory(rowCache, newRow);

			if (oldTaxCategory != newTaxCategory)
				return true;

			decimal? oldCuryTranAmount = GetCuryTranAmt(rowCache, oldRow);
			decimal? newCuryTranAmount = GetCuryTranAmt(rowCache, newRow);

			if (oldCuryTranAmount != newCuryTranAmount)
				return true;

			string oldTaxID = GetTaxID(rowCache, oldRow);
			string newTaxID= GetTaxID(rowCache, newRow);

			if (oldTaxID != newTaxID)
				return true;

			if (PXAccess.FeatureInstalled<FeaturesSet.perUnitTaxSupport>())
			{
				decimal oldQuantity = GetLineQty(rowCache, oldRow) ?? 0m;
				decimal newQuantity = GetLineQty(rowCache, newRow) ?? 0m;

				if (oldQuantity != newQuantity)
					return true;

				string oldUOM = GetUOM(rowCache, oldRow);
				string newUOM = GetUOM(rowCache, newRow);

				if (oldUOM != newUOM)
					return true;
			}

			return false;		
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc)
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is IPXRowDeletedSubscriber)
					{
						((IPXRowDeletedSubscriber)_Attributes[i]).RowDeleted(sender, e);
					}
				}
			}

			PXEntryStatus parentStatus = ParentGetStatus(sender.Graph);
			if (parentStatus == PXEntryStatus.Deleted || parentStatus == PXEntryStatus.InsertedDeleted) return;

			decimal? val;
			if (GetTaxCategory(sender, e.Row) == null && ((val = GetCuryTranAmt(sender, e.Row)) == null || val == 0m))
			{
				return;
			}

			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				ClearTaxes(sender, e.Row);
				CalcTaxes(sender, null, PXTaxCheck.Line);
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, false);
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
			{
				if (inserted != null)
					inserted.Clear();
				if (updated != null)
					updated.Clear();
			}
		}


		protected object _ParentRow;

		protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (e.Row != null && ((CurrencyInfo)e.Row).CuryRate != null && (e.OldRow == null || !sender.ObjectsEqual<CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow)))
				{
					PXView siblings = CurrencyInfoAttribute.GetView(sender.Graph, _ChildType, _CuryKeyField);
					if (siblings != null && siblings.SelectSingle() != null)
					{
						CalcTaxes(siblings.Cache, null);
					}
				}
			}
		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (e.Row.GetType() == _ParentType)
				{
					_ParentRow = e.Row;
				}
				CalcTaxes(sender.Graph.Caches[_ChildType], null);
				_ParentRow = null;
			}
		}

		protected virtual void IsTaxSavedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
			decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);

			CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0m);
		}

		protected virtual List<object> ChildSelect(PXCache cache, object data)
		{
			return TaxParentAttribute.ChildSelect(cache, data, this._ParentType);
		}

		protected virtual void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var originalTaxCalc = TaxCalc;
			try
			{
			TaxZone newTaxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
			TaxZone oldTaxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)e.OldValue);

			if (oldTaxZone != null && oldTaxZone.IsExternal == true)
			{
				TaxCalc = TaxCalc.Calc;
			}

			if (newTaxZone != null && newTaxZone.IsExternal == true)
			{
				TaxCalc = TaxCalc.ManualCalc;
			}


			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				if (!CompareZone(sender.Graph, (string)e.OldValue, (string)sender.GetValue(e.Row, _TaxZoneID)) || sender.GetValue(e.Row, _TaxZoneID) == null)
				{
					Preload(sender);

					List<object> details = this.ChildSelect(cache, e.Row);
					ReDefaultTaxes(cache, details);

					_ParentRow = e.Row;
					CalcTaxes(cache, null);
					_ParentRow = null;
				}
			}
		}
			finally
			{
				TaxCalc = originalTaxCalc;
			}
		}

		protected virtual void ReDefaultTaxes(PXCache cache, List<object> details)
		{
			foreach (object det in details)
			{
				ClearTaxes(cache, det);
				ClearChildTaxAmts(cache, det);
			}

			foreach (object det in details)
			{
				DefaultTaxes(cache, det, false);
			}
		}

		protected virtual void ClearChildTaxAmts(PXCache cache, object childRow)
		{
			PXCache childCache = cache.Graph.Caches[_ChildType];
			SetTaxableAmt(childCache, childRow, 0);
			SetTaxAmt(childCache, childRow, 0);
			if (childCache.Locate(childRow) != null && //if record is not in cache then it is just being inserted - no need for manual update
				(childCache.GetStatus(childRow) == PXEntryStatus.Notchanged
				|| childCache.GetStatus(childRow) == PXEntryStatus.Held))
			{
				childCache.Update(childRow);
			}
		}

		protected virtual void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true)
		{
			ClearTaxes(cache, clearDet);
			ClearChildTaxAmts(cache, clearDet);
			DefaultTaxes(cache, defaultDet, defaultExisting);
		}

		protected virtual void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				Preload(sender);

				PXCache cache = sender.Graph.Caches[_ChildType];
				List<object> details = this.ChildSelect(cache, e.Row);
				foreach (object det in details)
				{
					ReDefaultTaxes(cache, det, det, true);
				}
				_ParentRow = e.Row;
				_NoSumTaxable = true;
				try
				{
					CalcTaxes(cache, null);
				}
				finally
				{
					_ParentRow = null;
					_NoSumTaxable = false;
				}
			}
		}

		protected abstract void SetExtCostExt(PXCache sender, object child, decimal? value);

		protected abstract string GetExtCostLabel(PXCache sender, object row);

		protected string GetTaxCalcMode(PXGraph graph)
		{
			if (!_isTaxCalcModeEnabled)
			{
				throw new PXException(Messages.DocumentTaxCalculationModeNotEnabled);
			}
			return (string)ParentGetValue(graph, _TaxCalcMode);
		}
		protected string GetOriginalTaxCalcMode(PXGraph graph)
		{
			if (!_isTaxCalcModeEnabled)
			{
				throw new PXException(Messages.DocumentTaxCalculationModeNotEnabled);
			}
			return string.IsNullOrEmpty(_PreviousTaxCalcMode) ? (string)ParentGetValue(graph, _TaxCalcMode) : _PreviousTaxCalcMode;
		}
		protected virtual bool AskRecalculate(PXCache sender, PXCache detailCache, object detail)
		{
			PXView view = sender.Graph.Views[sender.Graph.PrimaryView];
			string askMessage = PXLocalizer.LocalizeFormat(Messages.RecalculateExtCost, GetExtCostLabel(detailCache, detail));
			return view.Ask(askMessage, MessageButtons.YesNo) == WebDialogResult.Yes;
		}

		protected virtual void TaxCalcModeUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			string newValue = sender.GetValue(e.Row, _TaxCalcMode) as string;
			_PreviousTaxCalcMode = e.OldValue as string;
			if (newValue != (string)e.OldValue)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				List<object> details = this.ChildSelect(cache, e.Row);

				decimal? taxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
				if (details != null && details.Count != 0 && AskRecalculationOnCalcModeChange)
				{
					if (taxTotal.HasValue && taxTotal.Value != 0 && AskRecalculate(sender, cache, details[0]))
					{
						PXCache taxDetCache = cache.Graph.Caches[_TaxType];
						foreach (object det in details)
						{
							TaxDetail taxSum = TaxSummarizeOneLine(cache, det, SummType.All);
							if (taxSum == null) continue;
							decimal? taxableAmount;
							decimal? taxAmount;
							switch (newValue)
							{
								case TaxCalculationMode.Net:
									taxableAmount = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxableAmt);
									SetExtCostExt(cache, det, PXDBCurrencyAttribute.RoundCury(cache, det, taxableAmount.Value, Precision));
									break;
								case TaxCalculationMode.Gross:
									taxableAmount = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxableAmt);
									taxAmount = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxAmt);
									SetExtCostExt(cache, det, PXDBCurrencyAttribute.RoundCury(cache, det, taxableAmount.Value + taxAmount.Value, Precision));
									break;
								case TaxCalculationMode.TaxSetting:
									TaxDetail taxSumInclusive = TaxSummarizeOneLine(cache, det, SummType.Inclusive);
									decimal? ExtCost;
									if (taxSumInclusive != null)
									{
										ExtCost = (decimal?)taxDetCache.GetValue(taxSumInclusive, _CuryTaxableAmt) + (decimal?)taxDetCache.GetValue(taxSumInclusive, _CuryTaxAmt);
									}
									else
									{
										ExtCost = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxableAmt);
									}
									SetExtCostExt(cache, det, PXDBCurrencyAttribute.RoundCury(cache, det, ExtCost.Value, Precision));
									break;
							}
						}
					}
				}

				Preload(sender);
				if (details != null)
				{
					foreach (object det in details)
					{
						ReDefaultTaxes(cache, det, det, false);
					}
				}
				_ParentRow = e.Row;
				CalcTaxes(cache, null);
				_ParentRow = null;
			}
		}

		private enum SummType
		{
			Inclusive, All
		}

		private TaxDetail TaxSummarizeOneLine(PXCache cache, object row, SummType summType)
		{
			List<object> taxitems = new List<object>();
			switch (summType)
			{
				case SummType.All:
					if (CalcGrossOnDocumentLevel && _isTaxCalcModeEnabled)
					{
						taxitems = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
							And<Tax.directTax, Equal<False>>>>>(cache.Graph, row, PXTaxCheck.Line);
					}
					else
					{
						taxitems = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
							And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
							And<Tax.directTax, Equal<False>>>>>>(cache.Graph, row, PXTaxCheck.Line);
					}
					break;
				case SummType.Inclusive:
					if (CalcGrossOnDocumentLevel && _isTaxCalcModeEnabled)
					{
						taxitems = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
							And<Tax.directTax, Equal<False>>>>>(cache.Graph, row, PXTaxCheck.Line);
					}
					else
					{
						taxitems = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
							And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
							And<Tax.directTax, Equal<False>>>>>>(cache.Graph, row, PXTaxCheck.Line);
					}
					break;
			}

			if (taxitems.Count == 0) return null;

			PXCache taxDetCache = cache.Graph.Caches[_TaxType];
			TaxDetail taxLineSumDet = (TaxDetail)taxDetCache.CreateInstance();
			decimal? CuryTaxableAmt = (decimal?)taxDetCache.GetValue(((PXResult)taxitems[0])[0], _CuryTaxableAmt);

			//AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

			decimal? CuryTaxAmt = SumWithReverseAdjustment(cache.Graph,
				taxitems,
				GetFieldType(taxDetCache, _CuryTaxAmt));

			if (CalcGrossOnDocumentLevel && _isTaxCalcModeEnabled)
			{
				var oldCalcMode = this.GetOriginalTaxCalcMode(cache.Graph);
				var newCalcMode = this.GetTaxCalcMode(cache.Graph);

				if (newCalcMode == TaxCalculationMode.Gross && oldCalcMode != TaxCalculationMode.Gross)
				{
					foreach (var taxitem in taxitems)
					{
						var tax = (Tax)((PXResult)taxitem)[typeof(Tax)];
						var taxRev = (TaxRev)((PXResult)taxitem)[typeof(TaxRev)];
						if (tax?.TaxCalcType == CSTaxCalcType.Doc)
						{
							var origTaxAmt = (decimal?)taxDetCache.GetValue(((PXResult)taxitem)[0], _CuryTaxAmt);
							var calculatedTaxAmt = CuryTaxableAmt * taxRev.TaxRate / 100.0M;
							CuryTaxAmt += calculatedTaxAmt - origTaxAmt;
						}
					}
				}
			}

			decimal? CuryExpenseAmt = SumWithReverseAdjustment(cache.Graph,
				taxitems,
				GetFieldType(taxDetCache, _CuryExpenseAmt));

			taxDetCache.SetValue(taxLineSumDet, _CuryTaxableAmt, CuryTaxableAmt);
			taxDetCache.SetValue(taxLineSumDet, _CuryTaxAmt, CuryTaxAmt + CuryExpenseAmt);

			return taxLineSumDet;
		}

		private decimal SumWithReverseAdjustment(PXGraph graph, List<Object> list, Type field)
		{
			decimal ret = 0.0m;
			list.ForEach(a =>
			{
				decimal? val = (decimal?)graph.Caches[BqlCommand.GetItemType(field)].GetValue(((PXResult)a)[BqlCommand.GetItemType(field)], field.Name);
				Tax tax = (Tax)((PXResult)a)[typeof(Tax)];
				decimal multiplier = tax.ReverseTax == true ? Decimal.MinusOne : Decimal.One;
				ret += (val ?? 0m) * multiplier;
			}
			);
			return ret;
		}

		protected virtual void TaxSum_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			object newdet = e.Row;

			if (newdet == null) 
				return;

			Dictionary<string, object> newdetKeys = GetKeyFieldValues(cache, newdet);
			bool insertNewTaxTran = true;

			if (ExternalTax.IsExternalTax(cache.Graph, (string)cache.GetValue(newdet, _TaxZoneID)) != true)
			{
				if (e.ExternalCall && e.Row is TaxDetail taxDetail && CheckIfTaxDetailHasPerUnitTaxType(cache.Graph, taxDetail.TaxID))	//Forbid to insert per-unit taxes manually from the UI
				{
					e.Cancel = true;
					throw new PXSetPropertyException(Messages.PerUnitTaxCannotBeInsertedManuallyErrorMsg);
				}

				foreach (object cacheddet in cache.Cached)
				{
					Dictionary<string, object> cacheddetKeys = new Dictionary<string, object>();
					cacheddetKeys = GetKeyFieldValues(cache, cacheddet);
					bool recordsEqual = true;
					PXEntryStatus status = cache.GetStatus(cacheddet);

					if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
					{
						foreach (KeyValuePair<string, object> keyValue in newdetKeys)
						{
							if (cacheddetKeys.ContainsKey(keyValue.Key) && !Object.Equals(cacheddetKeys[keyValue.Key], keyValue.Value))
							{
								recordsEqual = false;
								break;
							}
						}
						if (recordsEqual)
						{
							if (cache.Graph.IsMobile) // if inserting from mobile - override old detail
							{
								cache.Delete(cacheddet);
							}
							else
							{
								insertNewTaxTran = false;
								break;
							}
						}
					}
				}
				if (!insertNewTaxTran)
					e.Cancel = true;
			}
		}


		private Dictionary<string, object> GetKeyFieldValues(PXCache cache, object row)
		{
			Dictionary<string, object> keyValues = new Dictionary<string, object>();
			foreach (string key in cache.Keys)
			{
				if (key != _RecordID)
					keyValues.Add(key, cache.GetValue(row, key));
			}
			return keyValues;
		}

		protected virtual void DelOneTax(PXCache sender, object detrow, object taxrow)
		{
			PXCache cache = sender.Graph.Caches[_ChildType];
			foreach (object taxdet in SelectTaxes(cache, detrow, PXTaxCheck.Line))
			{
				if (object.Equals(((TaxDetail)((PXResult)taxdet)[0]).TaxID, ((TaxDetail)taxrow).TaxID))
				{
					sender.Delete(((PXResult)taxdet)[0]);
				}
			}
		}

		protected virtual void Preload(PXCache sender)
		{
			SelectTaxes(sender, null, PXTaxCheck.RecalcTotals);
		}

		/// <summary>
		/// During the import process, some fields may not have a default value.
		/// </summary>
		private static void InvokeExceptForExcelImport(PXCache cache, Action action)
		{
			if (!cache.Graph.IsImportFromExcel)
			{
				action.Invoke();
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			_ChildType = sender.GetItemType();

			inserted = new Dictionary<object, object>();
			updated = new Dictionary<object, object>();

			PXCache cache = sender.Graph.Caches[_TaxType];

			sender.Graph.FieldUpdated.AddHandler(_ParentType, _DocDate, (s, e) => InvokeExceptForExcelImport(s, () => DateUpdated(s, e)));
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _TaxZoneID, (s, e) => InvokeExceptForExcelImport(s, () => ZoneUpdated(s, e)));
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _IsTaxSaved, (s, e) => InvokeExceptForExcelImport(s, () => IsTaxSavedFieldUpdated(s, e)));
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _TermsID, ParentFieldUpdated);
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _CuryID, ParentFieldUpdated);			
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _CuryOrigDiscAmt, CuryOrigDiscAmt_FieldUpdated);
			
			sender.Graph.RowUpdated.AddHandler(_ParentType, ParentRowUpdated);
						
			sender.Graph.RowInserting.AddHandler(_TaxSumType, TaxSum_RowInserting);

			if (PXAccess.FeatureInstalled<FeaturesSet.perUnitTaxSupport>())
			{
				sender.Graph.RowPersisting.AddHandler(_ChildType, DocumentLineCheckPerUnitTaxesOnRowPersisting);
				sender.Graph.RowPersisting.AddHandler(_ParentType, DocumentCheckPerUnitTaxesOnRowPersisting);
				sender.Graph.RowSelected.AddHandler(_ParentType, CheckCurrencyAndRetainageOnDocumentRowSelected);

				sender.Graph.RowDeleting.AddHandler(_TaxSumType, CheckForPerUnitTaxesOnAggregatedTaxRowDeleting);
				sender.Graph.RowSelected.AddHandler(_TaxSumType, DisablePerUnitTaxesOnAggregatedTaxDetailRowSelected);
			}

			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is CurrencyInfoAttribute)
				{
					_CuryKeyField = sender.GetBqlField(attr.FieldName);
					break;
				}
			}

			if (_CuryKeyField != null)
			{
				sender.Graph.RowUpdated.AddHandler<CurrencyInfo>(CurrencyInfo_RowUpdated);
			}

			sender.Graph.Caches.SubscribeCacheCreated<Tax>(delegate
			{
				PXUIFieldAttribute.SetVisible<Tax.exemptTax>(sender.Graph.Caches[typeof(Tax)], null, false);
				PXUIFieldAttribute.SetVisible<Tax.statisticalTax>(sender.Graph.Caches[typeof(Tax)], null, false);
				PXUIFieldAttribute.SetVisible<Tax.reverseTax>(sender.Graph.Caches[typeof(Tax)], null, false);
				PXUIFieldAttribute.SetVisible<Tax.pendingTax>(sender.Graph.Caches[typeof(Tax)], null, false);
				PXUIFieldAttribute.SetVisible<Tax.taxType>(sender.Graph.Caches[typeof(Tax)], null, false);
			});

			if (_isTaxCalcModeEnabled)
			{
				sender.Graph.FieldUpdated.AddHandler(_ParentType, _TaxCalcMode, (s, e) => InvokeExceptForExcelImport(s, () => TaxCalcModeUpdated(s, e)));
			}
		}

		public TaxBaseAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type CalcMode = null, Type parentBranchIDField = null)
		{
			ParentBranchIDField = parentBranchIDField;

			ChildFinPeriodIDField = typeof(TaxTran.finPeriodID);
			ChildBranchIDField = typeof(TaxTran.branchID);

			_ParentType = ParentType;
			_TaxType = TaxType;
			_TaxSumType = TaxSumType;

			if (CalcMode != null)
			{
				if (!typeof(IBqlField).IsAssignableFrom(CalcMode))
				{
					throw new PXArgumentException("CalcMode", ErrorMessages.ArgumentException);
				}
				TaxCalcMode = CalcMode;
			}
		}

		public virtual int CompareTo(object other)
		{
			return 0;
		}

		protected virtual IComparer<Tax> GetTaxByCalculationLevelComparer() => TaxByCalculationLevelComparer.Instance;
	}
}
