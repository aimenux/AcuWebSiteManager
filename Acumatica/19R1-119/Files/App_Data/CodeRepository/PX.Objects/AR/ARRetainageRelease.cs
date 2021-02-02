using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using static PX.Objects.AR.ARInvoiceEntry;
using PX.Objects.Common;

namespace PX.Objects.AR
{
	[Serializable]
	public partial class ARRetainageFilter : IBqlTable
	{
	    #region BranchIDARActiveProjectAttibute
	    public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

	    [Branch(PersistingCheck = PXPersistingCheck.Nothing)]
	    public virtual Int32? BranchID { get; set; }
	    #endregion

        #region DocDate
        public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate { get; set; }
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[AROpenPeriod(typeof(ARRetainageFilter.docDate),
			typeof(ARRetainageFilter.branchID),
			useMasterOrganizationIDByDefault: true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinPeriodID { get; set; }
		#endregion

		

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[Customer(
			Visibility = PXUIVisibility.SelectorVisible,
			Required = false,
			DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[PM.ActiveProjectOrContractBaseAttribute(typeof(ARInvoice.customerID), FieldClass = PM.ProjectAttribute.DimensionName)]
		public virtual Int32? ProjectID { get; set; }
		#endregion

		#region ShowBillsWithOpenBalance
		public abstract class showBillsWithOpenBalance : PX.Data.BQL.BqlBool.Field<showBillsWithOpenBalance> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Invoices with Open Balance", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ShowBillsWithOpenBalance { get; set; }
		#endregion
	}

	[Serializable]
	public class ARInvoiceExt : ARInvoice
	{
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		/// <summary>
		/// Code of the <see cref="PX.Objects.CM.Currency">Currency</see> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">company's base currency</see>.
		/// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, FieldClass = nameof(FeaturesSet.Multicurrency))]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public override string CuryID
		{
			get;
			set;
		}

		#endregion

		#region RetainageReleasePct
		public abstract class retainageReleasePct : PX.Data.BQL.BqlDecimal.Field<retainageReleasePct> { }

		[UnboundRetainagePercent(
			typeof(True),
			typeof(decimal100),
			typeof(ARInvoiceExt.curyRetainageUnreleasedAmt),
			typeof(ARInvoiceExt.curyRetainageReleasedAmt),
			typeof(ARInvoiceExt.retainageReleasePct),
			DisplayName = "Percent to Release")]
		public virtual decimal? RetainageReleasePct
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainageReleasedAmt
		public abstract class curyRetainageReleasedAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageReleasedAmt> { }

		[UnboundRetainageAmount(
			typeof(ARInvoiceExt.curyInfoID),
			typeof(ARInvoiceExt.curyRetainageUnreleasedAmt),
			typeof(ARInvoiceExt.curyRetainageReleasedAmt),
			typeof(ARInvoiceExt.retainageReleasedAmt),
			typeof(ARInvoiceExt.retainageReleasePct),
			DisplayName = "Retainage to Release")]
		public virtual decimal? CuryRetainageReleasedAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageReleasedAmt

		public abstract class retainageReleasedAmt : PX.Data.BQL.BqlDecimal.Field<retainageReleasedAmt> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainageReleasedAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainageUnreleasedCalcAmt
		public abstract class curyRetainageUnreleasedCalcAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageUnreleasedCalcAmt> { }

		[PXCurrency(typeof(ARInvoiceExt.curyInfoID), typeof(ARInvoiceExt.retainageUnreleasedCalcAmt))]
		[PXUIField(DisplayName = "Unreleased Retainage")]
		[PXFormula(typeof(Sub<ARInvoiceExt.curyRetainageUnreleasedAmt, ARInvoiceExt.curyRetainageReleasedAmt>))]
		public virtual decimal? CuryRetainageUnreleasedCalcAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageUnreleasedCalcAmt
		public abstract class retainageUnreleasedCalcAmt : PX.Data.BQL.BqlDecimal.Field<retainageUnreleasedCalcAmt> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainageUnreleasedCalcAmt
		{
			get;
			set;
		}
		#endregion
		
	}

	[TableAndChartDashboardType]
	public class ARRetainageRelease : PXGraph<ARRetainageRelease>
	{
		public PXFilter<ARRetainageFilter> Filter;
		public PXCancel<ARRetainageFilter> Cancel;

		[PXFilterable]
		public PXFilteredProcessing<ARInvoiceExt, ARRetainageFilter,
			Where2<
				Where<Current2<ARRetainageFilter.customerID>, IsNull, Or<ARInvoiceExt.customerID, Equal<Current2<ARRetainageFilter.customerID>>>>,
				And2<Where<Current2<ARRetainageFilter.projectID>, IsNull, Or<ARInvoiceExt.projectID, Equal<Current2<ARRetainageFilter.projectID>>>>,
				And2<Where<Current2<ARRetainageFilter.branchID>, IsNull, Or<ARInvoiceExt.branchID, Equal<Current2<ARRetainageFilter.branchID>>>>,
				And2<Where<Current<ARRetainageFilter.showBillsWithOpenBalance>, Equal<True>,
					Or<Where<ARInvoiceExt.curyDocBal, Equal<decimal0>,
					And<Current<ARRetainageFilter.showBillsWithOpenBalance>, NotEqual<True>>>>>,
				And<ARInvoiceExt.curyRetainageUnreleasedAmt, Greater<decimal0>,
				And<ARInvoiceExt.curyRetainageTotal, Greater<decimal0>,
				And<ARInvoiceExt.retainageApply, Equal<True>,
				And<ARInvoiceExt.released, Equal<True>,
				And<ARInvoiceExt.docDate, LessEqual<Current<ARRetainageFilter.docDate>>>>>>>>>>>,
			OrderBy<Asc<ARInvoiceExt.refNbr>>> DocumentList;

		public PXSetup<ARSetup> ARSetup;

		public PXAction<ARRetainageFilter> viewDocument;
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (DocumentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(DocumentList.Cache, DocumentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		protected virtual IEnumerable documentList()
		{
			foreach (PXResult<ARInvoiceExt> result in PXSelect<ARInvoiceExt,
				Where2<
					Where<Current2<ARRetainageFilter.customerID>, IsNull, Or<ARInvoiceExt.customerID, Equal<Current2<ARRetainageFilter.customerID>>>>,
					And2<Where<Current2<ARRetainageFilter.projectID>, IsNull, Or<ARInvoiceExt.projectID, Equal<Current2<ARRetainageFilter.projectID>>>>,
					And2<Where<Current2<ARRetainageFilter.branchID>, IsNull, Or<ARInvoiceExt.branchID, Equal<Current2<ARRetainageFilter.branchID>>>>,
					And2<Where<Current<ARRetainageFilter.showBillsWithOpenBalance>, Equal<True>,
						Or<Where<ARInvoiceExt.curyDocBal, Equal<decimal0>,
						And<Current<ARRetainageFilter.showBillsWithOpenBalance>, NotEqual<True>>>>>,
					And<ARInvoiceExt.curyRetainageUnreleasedAmt, Greater<decimal0>,
					And<ARInvoiceExt.curyRetainageTotal, Greater<decimal0>,
					And<ARInvoiceExt.docType, Equal<ARDocType.invoice>,
					And<ARInvoiceExt.retainageApply, Equal<True>,
					And<ARInvoiceExt.released, Equal<True>,
					And<ARInvoiceExt.docDate, LessEqual<Current<ARRetainageFilter.docDate>>>>>>>>>>>>,
				OrderBy<Asc<ARInvoiceExt.refNbr>>>.Select(this))
			{
				ARInvoiceExt doc = result;

				PXResult<ARRetainageInvoice> NotReleasedRetainageInvoice = PXSelect<ARRetainageInvoice,
				Where<ARRetainageInvoice.isRetainageDocument, Equal<True>,
					And<ARRetainageInvoice.origDocType, Equal<Required<ARInvoice.docType>>,
					And<ARRetainageInvoice.origRefNbr, Equal<Required<ARInvoice.refNbr>>,
					And<ARRetainageInvoice.released, NotEqual<True>>>>>>.SelectSingleBound(this, null, doc.DocType, doc.RefNbr);

				if (NotReleasedRetainageInvoice == null)
					yield return doc;
			}
		}

		public ARRetainageRelease()
		{
			ARSetup setup = ARSetup.Current;
		}

		public static void ReleaseRetainage(ARInvoiceEntry graph, ARInvoiceExt invoice, ARRetainageFilter filter, bool isAutoRelease)
		{
			graph.Clear(PXClearOption.ClearAll);
			PXUIFieldAttribute.SetError(graph.Document.Cache, null, null, null);

			RetainageOptions retainageOptions = new RetainageOptions();
			retainageOptions.DocDate = filter.DocDate;
			retainageOptions.MasterFinPeriodID = FinPeriodIDAttribute.CalcMasterPeriodID<ARRetainageFilter.finPeriodID>(graph.Caches[typeof(ARRetainageFilter)], filter);
			retainageOptions.CuryRetainageAmt = invoice.CuryRetainageReleasedAmt;

			ARInvoiceEntryRetainage retainageExt = graph.GetExtension<ARInvoiceEntryRetainage>();

			ARInvoice retainageInvoice = retainageExt.ReleaseRetainageProc(invoice, retainageOptions, isAutoRelease);
			graph.Save.Press();

			if (isAutoRelease)
			{
				List<ARRegister> toRelease = new List<ARRegister>() { retainageInvoice };
				using (new PXTimeStampScope(null))
				{
					ARDocumentRelease.ReleaseDoc(toRelease, true);
				}
			}
		}

		protected virtual void ARRetainageFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARRetainageFilter filter = e.Row as ARRetainageFilter;
			if (filter == null) return;

			bool isAutoRelease = ARSetup.Current.RetainageInvoicesAutoRelease == true;

			DocumentList.SetProcessDelegate<ARInvoiceEntry>(
				 delegate (ARInvoiceEntry graph, ARInvoiceExt item)
				 {
					 ReleaseRetainage(graph, item, filter, isAutoRelease);
				 }
				);
		}

		protected virtual void ARInvoiceExt_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARInvoiceExt invoice = e.Row as ARInvoiceExt;
			if (invoice == null) return;

			PXUIFieldAttribute.SetEnabled(sender, invoice, false);
			PXUIFieldAttribute.SetEnabled<ARInvoiceExt.selected>(sender, invoice, true);
			PXUIFieldAttribute.SetEnabled<ARInvoiceExt.retainageReleasePct>(sender, invoice, true);
			PXUIFieldAttribute.SetEnabled<ARInvoiceExt.curyRetainageReleasedAmt>(sender, invoice, true);
			
			if (invoice.Selected ?? true)
			{
				Dictionary<String, String> errors = PXUIFieldAttribute.GetErrors(sender, invoice, PXErrorLevel.Error);
				if (errors.Count > 0)
				{
					invoice.Selected = false;
					DocumentList.Cache.SetStatus(invoice, PXEntryStatus.Updated);
					sender.RaiseExceptionHandling<ARInvoiceExt.selected>(
						invoice,
						null,
						new PXSetPropertyException(PX.Objects.AP.Messages.ErrorRaised, PXErrorLevel.RowError));

					PXUIFieldAttribute.SetEnabled<ARInvoiceExt.selected>(sender, invoice, false);
				}
			}
		}

		public override bool IsDirty => false;
	}
}
