using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR.Standalone;

namespace PX.Objects.AR
{
	public class ARExternalTaxCalc : PXGraph<ARExternalTaxCalc>
	{
        public PXCancel<ARInvoice> Cancel;
        [PXFilterable]
        [PX.SM.PXViewDetailsButton(typeof(ARInvoice.refNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXProcessingJoin<ARInvoice,
		InnerJoin<TX.TaxZone, On<TX.TaxZone.taxZoneID, Equal<ARInvoice.taxZoneID>>>, 
		Where<TX.TaxZone.isExternal, Equal<True>,
			And<ARInvoice.isTaxValid, Equal<False>,
			And<ARInvoice.released, Equal<False>>>>> Items;

		public ARExternalTaxCalc()
		{
			Items.SetProcessDelegate(ProcessAll);
		}

		public static List<ARCashSale> Process(List<ARCashSale> list, bool isMassProcess)
		{
			List<ARCashSale> listWithTax = new List<ARCashSale>(list.Count);
			ARCashSaleEntry rg = PXGraph.CreateInstance<ARCashSaleEntry>();
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					rg.Clear();
					rg.Document.Current = PXSelect<ARCashSale, Where<ARCashSale.docType, Equal<Required<ARCashSale.docType>>, And<ARCashSale.refNbr, Equal<Required<ARCashSale.refNbr>>>>>.Select(rg, list[i].DocType, list[i].RefNbr);
					listWithTax.Add(rg.CalculateExternalTax(rg.Document.Current));
					PXProcessing<ARCashSale>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						PXProcessing<ARCashSale>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
					}
					else
					{
						throw new PXMassProcessException(i, e);
					}
				}

			}

			return listWithTax;
		}		

		public static void ProcessAll(List<ARInvoice> list)
		{
		    List<ARInvoice> listWithTax = new List<ARInvoice>(list.Count);
		    ARInvoiceEntry rg = PXGraph.CreateInstance<ARInvoiceEntry>();
		    for (int i = 0; i < list.Count; i++)
		    {
		        try
		        {
		            rg.Clear();
		            rg.Document.Current = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(rg, list[i].DocType, list[i].RefNbr);
		            listWithTax.Add(rg.CalculateExternalTax(rg.Document.Current));
		            PXProcessing<ARInvoice>.SetInfo(i, ActionsMessages.RecordProcessed);
		        }
		        catch (Exception e)
		        {
		                PXProcessing<ARInvoice>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
		        }

		    }
		}
	}
}