using PX.Data;

namespace PX.Objects.FS
{
    public static class ServiceOrderHandlers
    {
        public static void FSSODet_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var fsSODetRow = (FSSODet)e.Row;
            FSAppointment fsAppointmentRow = null;

            using (new PXConnectionScope())
            {
                fsAppointmentRow = PXSelectJoin<FSAppointment,
                                                InnerJoin<FSAppointmentDet,
                                                    On<
                                                        FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>>,
                                                Where<
                                                    FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>,
                                                    And<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>>>,
                                                OrderBy<
                                                    Desc<FSAppointmentDet.createdDateTime>>>
                                             .SelectWindowed(cache.Graph, 0, 1, fsSODetRow.SOID, fsSODetRow.SODetID);

                SharedFunctions.SetInventoryItemExtensionInfo(cache.Graph, fsSODetRow.InventoryID, fsSODetRow);
            }

            if (fsAppointmentRow != null)
            {
                fsSODetRow.Mem_LastReferencedBy = fsAppointmentRow.RefNbr;
            }
            else
            {
                fsSODetRow.Mem_LastReferencedBy = null;
            }

            fsSODetRow.IsFree = ServiceOrderAppointmentHandlers.IsFree(fsSODetRow.BillingRule, fsSODetRow.ManualPrice, fsSODetRow.LineType);
        }

        public static void FSSODet_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (FSSODet)e.Row;

            if (row.LineRef == null)
            {
                row.LineRef = row.LineNbr.Value.ToString("0000");
            }
        }
    }
}
