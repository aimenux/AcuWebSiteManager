using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using System.Diagnostics;

namespace PX.Objects.FS
{
    public class AppointmentExternalTaxCalc : PXGraph<AppointmentExternalTaxCalc>
    {
        [PXFilterable]
        public PXProcessingJoin<FSAppointment,
        InnerJoin<PX.Objects.TX.TaxZone, On<PX.Objects.TX.TaxZone.taxZoneID, Equal<FSAppointment.taxZoneID>>>,
        Where<PX.Objects.TX.TaxZone.isExternal, Equal<True>,
            And<FSAppointment.isTaxValid, Equal<False>>>> Items;

        public AppointmentExternalTaxCalc()
        {
            Items.SetProcessDelegate(
                delegate (List<FSAppointment> list)
                {
                    List<FSAppointment> newlist = new List<FSAppointment>(list.Count);
                    foreach (FSAppointment doc in list)
                    {
                        newlist.Add(doc);
                    }
                    Process(newlist, true);
                }
            );

        }

        public static void Process(FSAppointment doc)
        {
            List<FSAppointment> list = new List<FSAppointment>();

            list.Add(doc);
            Process(list, false);
        }

        public static void Process(List<FSAppointment> list, bool isMassProcess)
        {
            AppointmentEntry appointmentEntryGraph = PXGraph.CreateInstance<AppointmentEntry>();
            
            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    appointmentEntryGraph.Clear();
                    appointmentEntryGraph.AppointmentRecords.Current = PXSelect<FSAppointment, Where<FSAppointment.srvOrdType, Equal<Required<FSAppointment.srvOrdType>>, And<FSAppointment.refNbr, Equal<Required<FSAppointment.refNbr>>>>>.Select(appointmentEntryGraph, list[i].SrvOrdType, list[i].RefNbr);
                    appointmentEntryGraph.CalculateExternalTax(appointmentEntryGraph.AppointmentRecords.Current);
                    PXProcessing<FSAppointment>.SetInfo(i, ActionsMessages.RecordProcessed);
                }
                catch (Exception e)
                {
                    if (isMassProcess)
                    {
                        PXProcessing<FSAppointment>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
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
