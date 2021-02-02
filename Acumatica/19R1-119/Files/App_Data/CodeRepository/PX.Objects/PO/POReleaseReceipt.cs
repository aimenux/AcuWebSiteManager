using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AP.MigrationMode;

namespace PX.Objects.PO
{
	[TableAndChartDashboardType]
	public class POReleaseReceipt : PXGraph<POReleaseReceipt>
	{
		public PXCancel<POReceipt> Cancel;
		public PXAction<POReceipt> ViewDocument;
		[PXFilterable]
		public PXProcessing<POReceipt> Orders;

		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton()]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (this.Orders.Current != null)
			{
				if (this.Orders.Current.Released == false)
				{
					POReceiptEntry graph = PXGraph.CreateInstance<POReceiptEntry>();
					POReceipt poDoc = graph.Document.Search<POReceipt.receiptNbr>(this.Orders.Current.ReceiptNbr, this.Orders.Current.ReceiptType);
					if( poDoc != null)
					{
						graph.Document.Current = poDoc;
						throw new PXRedirectRequiredException(graph, true, "Document"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
					}
				}				
			}
			return adapter.Get();
		}


		public POReleaseReceipt()
		{
			APSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			Orders.SetSelected<POReceipt.selected>();
			Orders.SetProcessCaption(Messages.Process);
			Orders.SetProcessAllCaption(Messages.ProcessAll);
			Orders.SetProcessDelegate(delegate(List<POReceipt> list)
			{
				ReleaseDoc(list,true);
			});
		}


		public virtual IEnumerable orders()
		{
			foreach (PXResult<POReceipt> res in PXSelectJoinGroupBy<POReceipt,
			LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<POReceipt.vendorID>>,
			InnerJoin<POReceiptLine, On<POReceiptLine.receiptNbr, Equal<POReceipt.receiptNbr>>,			
			LeftJoin<APTran, On<APTran.receiptNbr, Equal<POReceiptLine.receiptNbr>, 
				And<APTran.receiptLineNbr, Equal<POReceiptLine.lineNbr>>>>>>,
			Where2<Where<Vendor.bAccountID, IsNull, Or<Match<Vendor, Current<AccessInfo.userName>>>>, 
			And<POReceipt.hold, Equal<boolFalse>,
			And<POReceipt.released, Equal<boolFalse>,			
			And<APTran.refNbr, IsNull>>>>,
			Aggregate<GroupBy<POReceipt.receiptNbr,
			GroupBy<POReceipt.receiptType,
			GroupBy<POReceipt.released,
			GroupBy<POReceipt.hold,
			GroupBy<POReceipt.autoCreateInvoice>>>>>>>
			.Select(this))
			{
				POReceipt sel = res;
				POReceipt order;
				if ((order = (POReceipt)Orders.Cache.Locate(sel)) != null)
				{
					sel.Selected = order.Selected;
				}
				
				yield return sel;
			}
			Orders.Cache.IsDirty = false;
		}

		public static void ReleaseDoc( List<POReceipt> list, bool aIsMassProcess)
		{
			POReceiptEntry docgraph = PXGraph.CreateInstance<POReceiptEntry>();
            DocumentList<INRegister> created = new DocumentList<INRegister>(docgraph);
			DocumentList<APInvoice> invoicesCreated = new DocumentList<APInvoice>(docgraph);
			INReceiptEntry iRe = null;
			INIssueEntry iIe = null;
			AP.APInvoiceEntry apInvoiceGraph = docgraph.CreateAPInvoiceEntry();
			int iRow = 0;
			bool failed = false;			
			foreach (POReceipt order in list)
			{
				try
				{
					switch (order.ReceiptType)
					{
						case POReceiptType.POReceipt:
                        case POReceiptType.TransferReceipt:
							if (iRe == null) iRe = docgraph.CreateReceiptEntry();
							docgraph.ReleaseReceipt(iRe, apInvoiceGraph, order, created, invoicesCreated, aIsMassProcess);
							break;
						case POReceiptType.POReturn:
							if (iIe == null) iIe = docgraph.CreateIssueEntry();
							docgraph.ReleaseReturn(iIe, apInvoiceGraph, order, created, invoicesCreated, aIsMassProcess);
							break;
					}
					PXProcessing<POReceipt>.SetInfo(iRow, ActionsMessages.RecordProcessed);
				}
				catch (Exception e) 
				{
					if (aIsMassProcess)
					{
						PXProcessing<POReceipt>.SetError(iRow, e);
						failed = true;
					}
					else
						throw;
				}
				iRow++;
			}
			if (failed)
			{
				throw new PXException(Messages.ReleaseOfOneOrMoreReceiptsHasFailed);
			}
		}
	}
}
