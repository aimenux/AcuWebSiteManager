using PX.Data;
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
	public class TaxParentAttribute : PXParentAttribute
	{
		public TaxParentAttribute(Type selectParent)
			: base(selectParent)
		{
			throw new PXArgumentException();
		}

		public static void NewChild(PXCache cache, object parentrow, Type ParentType, out object child)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute && ((PXParentAttribute)attr).ParentType.IsAssignableFrom(ParentType))
				{
					Type childType = cache.GetItemType();

					PXView parentView = ((PXParentAttribute)attr).GetParentSelect(cache);
					Type parentType = parentView.BqlSelect.GetFirstTable();

					PXView childView = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					BqlCommand selectChild = childView.BqlSelect;

					IBqlParameter[] pars = selectChild.GetParameters();
					Type[] refs = selectChild.GetReferencedFields(false);

					child = Activator.CreateInstance(childType);
					PXCache parentcache = cache.Graph.Caches[parentType];

					for (int i = 0; i < Math.Min(pars.Length, refs.Length); i++)
					{
						Type partype = pars[i].GetReferencedType();
						object val = parentcache.GetValue(parentrow, partype.Name);

						cache.SetValue(child, refs[i].Name, val);
					}
					return;
				}
			}
			child = null;
		}

		public static object ParentSelect(PXCache cache, object row, Type ParentType)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute && ((PXParentAttribute)attr).ParentType.IsAssignableFrom(ParentType))
				{
					PXView parentview = ((PXParentAttribute)attr).GetParentSelect(cache);
					return parentview.SelectSingleBound(new object[] { row });
				}
			}
			return null;
		}

		public static List<object> ChildSelect(PXCache cache, object row)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute)
				{
					PXView view = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { row });
				}
			}
			return null;
		}

		public static List<object> ChildSelect(PXCache cache, object row, Type ParentType)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute && ((PXParentAttribute)attr).ParentType.IsAssignableFrom(ParentType))
				{
					PXView view = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { row });
				}
			}
			return null;
		}

	}

	public enum PXTaxCheck
	{
		Line,
		RecalcLine,
		RecalcTotals,
	}

	public enum TaxCalc
	{
		NoCalc,
		Calc,
		ManualCalc,
		ManualLineCalc
	}

	[Obsolete("This class is obsolete as mutiple installments are now supported with Pending taxes.")]
	public class TaxTranSelect<InvoiceTable, TermsID, InvoiceTaxTran, TaxID, WhereSelect>
		: PXSelectJoin<InvoiceTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<TaxID>>>, WhereSelect>
		where InvoiceTable : class, IBqlTable, new()
		where TermsID : IBqlField
		where InvoiceTaxTran : class, IBqlTable, new()
		where TaxID : IBqlField
		where WhereSelect : IBqlWhere, new()
	{
		#region Ctor
		public TaxTranSelect(PXGraph graph)
			: base(graph)
		{
			AddHandlers(graph);
		}

		public TaxTranSelect(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			AddHandlers(graph);
		}
		#endregion

		#region Implementation
		private void AddHandlers(PXGraph graph)
		{
			graph.RowPersisting.AddHandler<InvoiceTable>(RowPersisting);
		}

		protected virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.vATReporting>() || e.Row == null) return;

			string termsID = (string)sender.GetValue<TermsID>(e.Row);
			Terms terms = TermsAttribute.SelectTerms(sender.Graph, termsID);

			if (terms?.InstallmentType == TermsInstallmentType.Multiple)
			{
				foreach (PXResult<InvoiceTaxTran, Tax> taxtran in View.SelectMulti())
				{
					Tax tax = taxtran;
					if (tax?.PendingTax == true)
					{
						sender.RaiseExceptionHandling<TermsID>(e.Row, termsID,
							new PXSetPropertyException(Messages.MultInstallmentTermsWithSVAT));
						break;
					}
				}
			}
		}
		#endregion
	}

	public class WhereTaxBase<TaxID, TaxFlag> : IBqlWhere
		where TaxID : IBqlOperand
		where TaxFlag : IBqlField
	{
		readonly IBqlCreator _where = new Where<Selector<TaxID, TaxFlag>, Equal<True>, 
			And<Selector<TaxID, Tax.statisticalTax>, Equal<False>, 
			And<Selector<TaxID, Tax.reverseTax>, Equal<False>, 
			And<Selector<TaxID, Tax.taxType>, Equal<CSTaxType.vat>>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> _where.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			_where.Verify(cache, item, pars, ref result, ref value);
		}
	}

	public class WhereExempt<TaxID> : WhereTaxBase<TaxID, Tax.exemptTax>
		where TaxID : IBqlOperand
	{
	}

	public class WhereTaxable<TaxID> : WhereTaxBase<TaxID, Tax.includeInTaxable>
		where TaxID : IBqlOperand
		{
	}

	public class WhereAPPPDTaxable<TaxID> : IBqlWhere
		where TaxID : IBqlOperand
	{
		readonly IBqlCreator _where = new Where<Selector<TaxID, Tax.includeInTaxable>, Equal<True>,
			And<Selector<TaxID, Tax.statisticalTax>, Equal<False>,
			And<Selector<TaxID, Tax.taxType>, Equal<CSTaxType.vat>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;
			status &= _where.AppendExpression(ref exp, graph, info, selection);
			return status;
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			_where.Verify(cache, item, pars, ref result, ref value);
		}
	}

	[System.SerializableAttribute()]
	public abstract class TaxDetail : ITaxDetail
	{		
		#region TaxID
		protected String _TaxID;
		public virtual String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion
		#region TaxRate
		protected Decimal? _TaxRate;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? TaxRate
		{
			get
			{
				return this._TaxRate;
			}
			set
			{
				this._TaxRate = value;
			}
		}
		#endregion
		#region CuryInfoID
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[PXDefault()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region NonDeductibleTaxRate
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "100.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Deductible Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? NonDeductibleTaxRate { get; set; }
		#endregion
		#region ExpenseAmt
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ExpenseAmt { get; set; }
		#endregion
		#region CuryExpenseAmt
        [PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryExpenseAmt { get; set; }
		#endregion

		#region Per Unit Taxes
		#region TaxUOM
		/// <summary>
		///The unit of measure used by tax. Specific/Per Unit taxes are calculated on quantities in this UOM.
		/// </summary>
		public abstract class taxUOM : PX.Data.BQL.BqlString.Field<taxUOM> { }

		/// <summary>
		/// The unit of measure used by tax. Specific/Per Unit taxes are calculated on quantities in this UOM
		/// </summary>
		[IN.INUnit(DisplayName = "Tax UOM", FieldClass = nameof(FeaturesSet.PerUnitTaxSupport))]
		public virtual string TaxUOM
		{
			get;
			set;
		}
		#endregion

		#region TaxableQty
		/// <summary>
		///The taxable quantity for per unit taxes.
		/// </summary>
		public abstract class taxableQty : PX.Data.BQL.BqlString.Field<taxableQty> { }

		/// <summary>
		///The taxable quantity for per unit taxes.
		/// </summary>
		[IN.PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Qty.", Enabled = false, FieldClass = nameof(FeaturesSet.PerUnitTaxSupport))]
		public virtual decimal? TaxableQty
		{
			get;
			set;
		}
		#endregion
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}

	public class VendorTaxPeriodType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SemiMonthly, Monthly, BiMonthly, Quarterly, SemiAnnually, Yearly, FiscalPeriod },
				new string[] { Messages.HalfMonth, Messages.Month, Messages.TwoMonths, Messages.Quarter, Messages.HalfYear, Messages.Year, Messages.FinancialPeriod }) { }
		}

		public const string Monthly = "M";
		public const string SemiMonthly = "B";
		public const string Quarterly = "Q";
		public const string Yearly = "Y";
		public const string FiscalPeriod = "F";
        public const string BiMonthly = "E";
		public const string SemiAnnually = "H";

		public class monthly : PX.Data.BQL.BqlString.Constant<monthly>
		{
			public monthly() : base(Monthly) { ;}
		}
		public class semiMonthly : PX.Data.BQL.BqlString.Constant<semiMonthly>
		{
			public semiMonthly() : base(SemiMonthly) { ;}
		}
		public class quarterly : PX.Data.BQL.BqlString.Constant<quarterly>
		{
			public quarterly() : base(Quarterly) { ;}
		}
		public class yearly : PX.Data.BQL.BqlString.Constant<yearly>
		{
			public yearly() : base(Yearly) { ;}
		}
		public class fiscalPeriod : PX.Data.BQL.BqlString.Constant<fiscalPeriod>
		{
			public fiscalPeriod() : base(FiscalPeriod) { ;}
		}
        public class biMonthly : PX.Data.BQL.BqlString.Constant<biMonthly>
        {
            public biMonthly() : base(BiMonthly) { ;}
        }
		public class semiAnnually : PX.Data.BQL.BqlString.Constant<semiAnnually>
		{
			public semiAnnually() : base(SemiAnnually) { ;}
		}
	}

	public class VendorSVATTaxEntryRefNbr
	{
		public class InputListAttribute : PXStringListAttribute
		{
			public InputListAttribute()
				: base(
				new string[] { DocumentRefNbr, PaymentRefNbr, ManuallyEntered },
				new string[] { Messages.DocumentRefNbr, Messages.PaymentRefNbr, Messages.ManuallyEntered })
			{ }
		}

		public class OutputListAttribute : PXStringListAttribute
		{
			public OutputListAttribute()
				: base(
				new string[] { DocumentRefNbr, PaymentRefNbr, TaxInvoiceNbr, ManuallyEntered },
				new string[] { Messages.DocumentRefNbr, Messages.PaymentRefNbr, Messages.TaxInvoiceNbr, Messages.ManuallyEntered })
			{ }
		}

		public const string DocumentRefNbr = "D";
		public const string PaymentRefNbr = "P";
		public const string TaxInvoiceNbr = "T";
		public const string ManuallyEntered = "M";
		
		public class documentRefNbr : PX.Data.BQL.BqlString.Constant<documentRefNbr>
		{
			public documentRefNbr() : base(DocumentRefNbr) { }
		}

		public class paymentRefNbr : PX.Data.BQL.BqlString.Constant<paymentRefNbr>
		{
			public paymentRefNbr() : base(PaymentRefNbr) { }
		}

		public class taxInvoiceNbr : PX.Data.BQL.BqlString.Constant<taxInvoiceNbr>
		{
			public taxInvoiceNbr() : base(TaxInvoiceNbr) { }
		}

		public class manuallyEntered : PX.Data.BQL.BqlString.Constant<manuallyEntered>
		{
			public manuallyEntered() : base(ManuallyEntered) { }
		}
	}

	public class TaxReportLineSelector : PXSelectorAttribute
	{		
		public TaxReportLineSelector(Type search, params Type[] fields) : base(search, fields)
		{
			this.DescriptionField = typeof (TaxReportLine.descr);
			_UnconditionalSelect = 
				BqlCommand.CreateInstance(typeof(Search<TaxReportLine.lineNbr, 
													Where<TaxReportLine.vendorID, Equal<Current<TaxReportLine.vendorID>>, 
													And<TaxReportLine.lineNbr, Equal<Required<TaxReportLine.lineNbr>>>>>));
			_CacheGlobal = false;
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || e.NewValue == null || e.NewValue is int)
				return;

			if (!int.TryParse(e.NewValue.ToString(), out int newValue))
			{
				this.CustomMessageElementDoesntExist = Messages.ValueCannotBeFoundInSystem;
				throwNoItem(hasRestrictedAccess(sender, _PrimarySimpleSelect, e.Row), e.ExternalCall, e.NewValue);
			}
		}
	}
}
