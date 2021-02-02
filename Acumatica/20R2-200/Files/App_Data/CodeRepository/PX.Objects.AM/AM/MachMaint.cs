using System;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Machine Maintenance
    /// </summary>
    public class MachMaint : PXGraph<MachMaint, AMMach>
    {
        public PXSelect<AMMach> Machines;
        public PXSelect<AMMach, Where<AMMach.machID, Equal<Current<AMMach.machID>>>> MachSelected;
        [PXHidden]
        public PXSelect<AMWCMach> MachineRecords;

        protected virtual void AMMach_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            AMMach machinerecord = (AMMach)e.Row;
            if (machinerecord != null && machinerecord.ActiveFlg == true)
            {
                // Check for Machine in Work Centers where Machine Override is false
                AMWCMach amMachRecord = PXSelectJoin<AMWCMach, InnerJoin<AMWC, On<AMWCMach.wcID, Equal<AMWC.wcID>>>,
                    Where<AMWCMach.machID, Equal<Required<AMWCMach.machID>>,
                        And<AMWC.activeFlg, Equal<True>>>>.Select(this, machinerecord.MachID);

                // Check for Machine in Work Centers where Machine Override is false
                if (amMachRecord != null)
                {
                    e.Cancel = true;
                    throw new PXException(Messages.GetLocal(Messages.MachineUsedinActiveWorkCenter), amMachRecord.WcID);
                }
            }
        }

        protected virtual void AMMach_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            var row = (AMMach)e.Row;
            if (row == null || e.TranStatus != PXTranStatus.Open )
            {
                return;
            }

            if (e.Operation != PXDBOperation.Update)
            {
                return;
            }

            foreach (AMWCMach amMachRecord in PXSelectJoin<AMWCMach, 
                InnerJoin<AMWC, On<AMWCMach.wcID, Equal<AMWC.wcID>>>,
                    Where<AMWCMach.machID, Equal<Required<AMWCMach.machID>>,
                        And<AMWCMach.machineOverride, Equal<False>, 
                        And<AMWC.activeFlg, Equal<True>>>>>.Select(this, row.MachID))
                {
                amMachRecord.MachAcctID = row.MachAcctID;
                amMachRecord.MachSubID = row.MachSubID;
                amMachRecord.StdCost = row.StdCost;
                    MachineRecords.Cache.Update(amMachRecord);
            }
        }

        protected virtual void AMMach_MachID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            var newValueString = Convert.ToString(e.NewValue);
            if (string.IsNullOrWhiteSpace(newValueString))
            {
                return;
            }
            // Prevent silly users from entering leading spaces...
            e.NewValue = newValueString.TrimStart();
        }
    }
}