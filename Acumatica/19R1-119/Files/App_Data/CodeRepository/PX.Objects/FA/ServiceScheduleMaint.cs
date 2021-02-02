using PX.Data;

namespace PX.Objects.FA
{
    public class ServiceScheduleMaint : PXGraph<ServiceScheduleMaint>
    {
        public PXCancel<FAServiceSchedule> Cancel;
		public PXSavePerRow<FAServiceSchedule, FAServiceSchedule.scheduleID> Save;
		#region Selects Declaration
        public PXSelect<FAServiceSchedule> ServiceSchedule;
		public PXSetup<FASetup> FASetup;
		#endregion

		#region Constructor
		public ServiceScheduleMaint() 
		{
			FASetup setup = FASetup.Current;
		}

        #endregion

		#region
		protected virtual void FAServiceSchedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FAServiceSchedule sch = (FAServiceSchedule)e.Row;
			if (e.Operation == PXDBOperation.Delete)
			{
				if (PXSelect<FixedAsset, Where<FixedAsset.serviceScheduleID, Equal<Current<FAServiceSchedule.scheduleID>>>>.SelectSingleBound(this, new object[] { e.Row }).Count > 0)
				{
					throw new PXRowPersistingException("ScheduleCD", sch.ScheduleCD, Messages.ScheduleExistsHistory);
				}
			}
		}

		protected virtual void FAServiceSchedule_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FAServiceSchedule sch = (FAServiceSchedule)e.Row;
			
			if (PXSelect<FixedAsset, Where<FixedAsset.serviceScheduleID, Equal<Current<FAServiceSchedule.scheduleID>>>>.SelectSingleBound(this, new object[] { e.Row }).Count > 0)
			{
				throw new PXSetPropertyException(Messages.ScheduleExistsHistory);    	
			}
		}
		#endregion
    }		
}