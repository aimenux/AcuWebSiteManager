using PX.Data;
using System;
using System.Linq;

namespace PX.Objects.FS
{
    public class AppointmentAutoNumberAttribute : AlternateAutoNumberAttribute, IPXRowInsertingSubscriber
    {
        protected override string GetInitialRefNbr(string baseRefNbr)
        {
            return baseRefNbr.Trim() + "-1";
        }

        public AppointmentAutoNumberAttribute(Type setupField, Type dateField)
            : base(setupField, dateField)
        {
        }

        void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
        }

        /// <summary>
        /// Allows to calculate the <c>RefNbr</c> sequence when trying to insert a new register.
        /// </summary>
        protected override bool SetRefNbr(PXCache cache, object row)
        {
            FSAppointment fsAppointmentRow = (FSAppointment)row;

            if (fsAppointmentRow.SOID == null || fsAppointmentRow.SOID < 0)
            {
                return false;
            }

            FSAppointment fsAppointmentRowTmp = PXSelectReadonly<FSAppointment,
                                                Where<
                                                    FSAppointment.sOID, Equal<Current<FSAppointment.sOID>>>,
                                                OrderBy<
                                                    Desc<FSAppointment.appointmentID>>>
                                                .SelectWindowed(cache.Graph, 0, 1);

            string lastRefNbr = fsAppointmentRowTmp?.RefNbr;

            fsAppointmentRow.RefNbr = GetNextRefNbr(fsAppointmentRow.SORefNbr, lastRefNbr);

            return true;
        }
    }
}