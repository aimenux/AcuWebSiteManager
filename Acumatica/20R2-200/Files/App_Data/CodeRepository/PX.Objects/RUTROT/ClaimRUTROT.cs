using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.RUTROT
{
	public partial class ClaimRUTROT : PXGraph<ClaimRUTROT>
	{
        

        public PXCancel<ClaimRUTROTFilter> Cancel;
		public PXFilter<ClaimRUTROTFilter> Filter;

		[PXFilterable]
        public PXFilteredProcessing<RUTROTItem, ClaimRUTROTFilter, 
            Where<RUTROTItem.rUTROTType, Equal<Current<ClaimRUTROTFilter.rUTROTType>>, And<
                Where2<Where<Current<ClaimRUTROTFilter.action>, Equal<ClaimActions.claim>, And<RUTROTItem.paymentReleased, Equal<True>, And<RUTROTItem.isClaimed, NotEqual<True>>>>, 
                    Or2<Where<Current<ClaimRUTROTFilter.action>, Equal<ClaimActions.export>, And<RUTROTItem.isClaimed, Equal<True>>>,
                    Or<Where<Current<ClaimRUTROTFilter.action>, Equal<ClaimActions.balance>, And<RUTROTItem.isClaimed, NotEqual<True>,
                        And<RUTROTItem.balancingCreditMemoRefNbr, IsNull, And<RUTROTItem.balancingDebitMemoRefNbr, IsNull>>>>>>>>>> Documents;

        public static void ExportDocs(List<RUTROTItem> documents)
        {
			var processing = PXGraph.CreateInstance<ClaimRUTROTProcess>();
			processing.ExportDocuments(Convert(processing, documents));
		}

        public static void ClaimDocs(List<RUTROTItem> documents)
        {
            var processing = PXGraph.CreateInstance<ClaimRUTROTProcess>();
			processing.ClaimDocuments(Convert(processing, documents));
        }

        public static void BalanceDocs(List<RUTROTItem> documents)
        {
            var processing = PXGraph.CreateInstance<ClaimRUTROTProcess>();
            processing.BalanceDocuments(Convert(processing, documents));
        }

        public virtual void ClaimRUTROTFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var filter = (ClaimRUTROTFilter)e.Row;

            if (filter == null || string.IsNullOrEmpty(filter.Action))
            {
                return;
            }

            if (filter.Action == ClaimActions.Export)
            {
                PXUIFieldAttribute.SetVisible(Documents.Cache, nameof(RUTROT.ClaimDate), true);
                PXUIFieldAttribute.SetVisible(Documents.Cache, nameof(RUTROT.ExportRefNbr), true);
                Documents.SetProcessDelegate(ExportDocs);
            }

            if (filter.Action == ClaimActions.Claim)
            {
                PXUIFieldAttribute.SetVisible(Documents.Cache, nameof(RUTROT.ClaimDate), false);
                PXUIFieldAttribute.SetVisible(Documents.Cache, nameof(RUTROT.ExportRefNbr), false);
                Documents.SetProcessDelegate(ClaimDocs);
            }

            if (filter.Action == ClaimActions.Balance)
            {
                PXUIFieldAttribute.SetVisible(Documents.Cache, nameof(RUTROT.ClaimDate), false);
                PXUIFieldAttribute.SetVisible(Documents.Cache, nameof(RUTROT.ExportRefNbr), false);
                Documents.SetProcessDelegate(BalanceDocs);
            }
		}

		public virtual void ClaimRUTROTFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			this.Documents.Cache.Clear();
		}

        protected static List<RUTROT> Convert(PXGraph graph, List<RUTROTItem> items) => 
	         items.Select(m => (RUTROT)PXSelect<RUTROT, Where<RUTROT.docType, Equal<Required<RUTROT.docType>>,
	            And<RUTROT.refNbr, Equal<Required<RUTROT.refNbr>>>>>.Select(graph, m.DocType, m.RefNbr)).ToList();

        public PXAction<ClaimRUTROTFilter> viewDocument;

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            RUTROTItem item = Documents.Current;

            if (item == null)
            {
                return adapter.Get();
            }

            ARInvoice invoice = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<RUTROT.docType>>,
                And<ARInvoice.refNbr, Equal<Required<RUTROT.refNbr>>>>>.Select(this, item.DocType, item.RefNbr);

            ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
            graph.Document.Current = invoice;

            throw new PXRedirectRequiredException(graph, true, Messages.Document) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        public PXAction<ClaimRUTROTFilter> viewCreditMemo;

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewCreditMemo(PXAdapter adapter)
        {
            RUTROTItem item = Documents.Current;

            if (item == null)
            {
                return adapter.Get();
            }

            ARInvoice creditMemo = PXSelectJoin<ARInvoice,
                InnerJoin<RUTROT, On<RUTROT.balancingCreditMemoDocType, Equal<ARInvoice.docType>, 
                                And<RUTROT.balancingCreditMemoRefNbr, Equal<ARInvoice.refNbr>>>>,
                Where<RUTROT.docType, Equal<Required<RUTROT.docType>>,
                And<RUTROT.refNbr, Equal<Required<RUTROT.refNbr>>>>>.Select(this, item.DocType, item.RefNbr);

            ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
            graph.Document.Current = creditMemo;

            throw new PXRedirectRequiredException(graph, true, Messages.Document) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        public PXAction<ClaimRUTROTFilter> viewDebitMemo;

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewDebitMemo(PXAdapter adapter)
        {
            RUTROTItem item = Documents.Current;

            if (item == null)
            {
                return adapter.Get();
            }

            ARInvoice debitMemo = PXSelectJoin<ARInvoice,
                InnerJoin<RUTROT, On<RUTROT.balancingDebitMemoDocType, Equal<ARInvoice.docType>, 
                                And<RUTROT.balancingDebitMemoRefNbr, Equal<ARInvoice.refNbr>>>>,
                Where<RUTROT.docType, Equal<Required<RUTROT.docType>>,
                And<RUTROT.refNbr, Equal<Required<RUTROT.refNbr>>>>>.Select(this, item.DocType, item.RefNbr);

            ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
            graph.Document.Current = debitMemo;

            throw new PXRedirectRequiredException(graph, true, Messages.Document) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
    }
}
