using System;
using System.Collections.Generic;
using System.Security.Permissions;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class EPCalendarSync : PXGraph
	{
		//[SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
		public IEnumerable<CRActivity> GetCalendarEvents(Guid settingsId)
		{
			var result = new List<CRActivity>();
			Load();
			if (IsPublished(settingsId)) result.AddRange(GetEvents(settingsId)); //read items before scope desposing
			return result;
		}

		public bool IsPublished(Guid id)
		{
			PXResultset<SMCalendarSettings> set = PXSelect<SMCalendarSettings,
				Where<SMCalendarSettings.urlGuid,
					Equal<Required<SMCalendarSettings.urlGuid>>>>.Select(this, id);
			if (set != null && set.Count > 0) return ((SMCalendarSettings)set[0]).IsPublic.Value;
			return false;
		}

		public virtual IEnumerable<CRActivity> GetEvents(Guid id)
		{
			foreach (var item in PXSelectJoin<CRActivity,
				LeftJoin<EPAttendee, 
					On<EPAttendee.eventNoteID, Equal<CRActivity.noteID>>,
				InnerJoin<SMCalendarSettings, 
					On<SMCalendarSettings.userID, Equal<CRActivity.createdByID>,
					Or<SMCalendarSettings.userID, Equal<CRActivity.ownerID>,
					Or<SMCalendarSettings.userID, Equal<EPAttendee.userID>>>>>>,
				Where2<
					Where<CRActivity.classID, Equal<CRActivityClass.events>>,
					And<SMCalendarSettings.urlGuid, Equal<Required<SMCalendarSettings.urlGuid>>,
					And<SMCalendarSettings.isPublic, Equal<True>>>>,
				OrderBy<
					Desc<CRActivity.priority, 
					Asc<CRActivity.startDate, 
					Asc<CRActivity.endDate>>>>>.
					Select(this, id))
			{
				yield return item;
			}
		}
	}
}
