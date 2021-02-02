using PX.Data;
using PX.Objects.AR;
using PX.Objects.TX;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class ServiceOrderExternalTaxCalc : PXGraph<ServiceOrderExternalTaxCalc>
    {
        [PXFilterable]
        public PXProcessingJoin<FSServiceOrder,
               InnerJoin<TaxZone,
               On<
                   TaxZone.taxZoneID, Equal<FSServiceOrder.taxZoneID>>>,
               Where<
                   TaxZone.isExternal, Equal<True>,
                   And<FSServiceOrder.isTaxValid, Equal<False>>>> Items;

        public ServiceOrderExternalTaxCalc()
        {
            Items.SetProcessDelegate(
                delegate (List<FSServiceOrder> list)
                {
                    List<FSServiceOrder> newlist = new List<FSServiceOrder>(list.Count);
                    foreach (FSServiceOrder doc in list)
                    {
                        newlist.Add(doc);
                    }
                    Process(newlist, true);
                }
            );

        }

        public static void Process(FSServiceOrder doc)
        {
            List<FSServiceOrder> list = new List<FSServiceOrder>();

            list.Add(doc);
            Process(list, false);
        }

        public static void Process(List<FSServiceOrder> list, bool isMassProcess)
        {
            ServiceOrderEntry serviceOrderEntryGraph = PXGraph.CreateInstance<ServiceOrderEntry>();

            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    serviceOrderEntryGraph.Clear();
                    serviceOrderEntryGraph.ServiceOrderRecords.Current = PXSelect<FSServiceOrder, Where<FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>, And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>.Select(serviceOrderEntryGraph, list[i].SrvOrdType, list[i].RefNbr);
                    serviceOrderEntryGraph.CalculateExternalTax(serviceOrderEntryGraph.ServiceOrderRecords.Current);
                    PXProcessing<FSServiceOrder>.SetInfo(i, ActionsMessages.RecordProcessed);
                }
                catch (Exception e)
                {
                    if (isMassProcess)
                    {
                        PXProcessing<FSServiceOrder>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
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
