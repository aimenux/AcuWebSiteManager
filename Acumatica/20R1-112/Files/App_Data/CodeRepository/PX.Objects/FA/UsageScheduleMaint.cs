using PX.Data;

namespace PX.Objects.FA
{
    public class UsageScheduleMaint : PXGraph<UsageScheduleMaint>
    {
        public PXCancel<FAUsageSchedule> Cancel;
		public PXSavePerRow<FAUsageSchedule, FAUsageSchedule.scheduleID> Save;
		#region Selects Declaration
        public PXSelect<FAUsageSchedule> UsageSchedule;
		public PXSetup<FASetup> FASetup;
		#endregion

		#region Constructor
		public UsageScheduleMaint() 
		{
			FASetup setup = FASetup.Current;
		}

        #endregion

		#region
		protected virtual void FAUsageSchedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FAUsageSchedule sch = (FAUsageSchedule)e.Row;
			if (e.Operation == PXDBOperation.Delete)
			{
				if (PXSelect<FixedAsset, Where<FixedAsset.usageScheduleID, Equal<Current<FAUsageSchedule.scheduleID>>>>.SelectSingleBound(this, new object[] { e.Row }).Count > 0)
				{
					throw new PXRowPersistingException("ScheduleCD", sch.ScheduleCD, Messages.ScheduleExistsHistory);
				}
			}
		}

		protected virtual void FAUsageSchedule_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FAUsageSchedule sch = (FAUsageSchedule)e.Row;
			
			if (PXSelect<FixedAsset, Where<FixedAsset.usageScheduleID, Equal<Current<FAUsageSchedule.scheduleID>>>>.SelectSingleBound(this, new object[] { e.Row }).Count > 0)
			{
				throw new PXSetPropertyException(Messages.ScheduleExistsHistory);    	
			}
		}
		#endregion
    }		
}