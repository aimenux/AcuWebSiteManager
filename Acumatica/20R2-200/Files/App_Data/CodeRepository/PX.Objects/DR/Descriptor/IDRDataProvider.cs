using PX.Data;
using System.Collections.Generic;


namespace PX.Objects.DR
{
	public interface IDRDataProvider
	{
		PXResultset<DRScheduleDetail> GetScheduleDetailsResultset(int? scheduleID);
		void DeleteAllDetails(int? ScheduleID);
	}
}
