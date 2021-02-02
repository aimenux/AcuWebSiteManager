using PX.Data;
using System;
using System.Linq;

namespace PX.Objects.FS
{
    public class AppointmentAutoNumberAttribute : AlternateAutoNumberAttribute, IPXRowInsertingSubscriber
    {
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

            FSAppointment fsAppointmentRow_tmp = PXSelectReadonly<FSAppointment,
                                                    Where<
                                                        FSAppointment.sOID, Equal<Current<FSAppointment.sOID>>>,
                                                    OrderBy<
                                                        Desc<FSAppointment.appointmentID>>>
                                               .SelectWindowed(cache.Graph, 0, 1);

            string refNbr = string.Empty;

            if (fsAppointmentRow_tmp != null)
            {
                refNbr = fsAppointmentRow_tmp.RefNbr;
            }
                
            fsAppointmentRow.RefNbr = SharedFunctions.GetNextRefNbr(fsAppointmentRow.SORefNbr, refNbr);

            return true;
        }
    }
}