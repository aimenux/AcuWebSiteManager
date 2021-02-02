using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.Common;
using System.Diagnostics;

namespace PX.Objects.PM
{
	public class PMExternalTaxCalc : PXGraph<PMExternalTaxCalc>
	{
		[PXFilterable]
		public PXProcessingJoin<PMProforma,
		InnerJoin<TX.TaxZone, On<TX.TaxZone.taxZoneID, Equal<PMProforma.taxZoneID>>>,
		Where<TX.TaxZone.isExternal, Equal<True>,
			And<PMProforma.isTaxValid, Equal<False>>>> Items;

		public PMExternalTaxCalc()
		{
			Items.SetProcessDelegate(
				delegate(List<PMProforma> list)
				{
					List<PMProforma> newlist = new List<PMProforma>(list.Count);
					foreach (PMProforma doc in list)
					{
						newlist.Add(doc);
					}
					Process(newlist, true);
				}
			);

		}

		public static void Process(PMProforma doc)
		{
			List<PMProforma> list = new List<PMProforma>();

			list.Add(doc);
			Process(list, false);
		}
		
		public static void Process(List<PMProforma> list, bool isMassProcess)
		{
			Stopwatch sw = new Stopwatch();
			ProformaEntry rg = PXGraph.CreateInstance<ProformaEntry>();
			rg.SuppressRowSeleted = true;
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					rg.Document.Current = PXSelect<PMProforma, Where<PMProforma.refNbr, Equal<Required<PMProforma.refNbr>>>>.Select(rg, list[i].RefNbr);
					rg.CalculateExternalTax(rg.Document.Current);
					PXProcessing<PMProforma>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						PXProcessing<PMProforma>.SetError(i, e is PXOuterException ? e.Message + Environment.NewLine + string.Join(Environment.NewLine, ((PXOuterException)e).InnerMessages) : e.Message);
					}
					else
					{
						throw new PXMassProcessException(i, e);
					}
				}

			}
		}

	}
}
