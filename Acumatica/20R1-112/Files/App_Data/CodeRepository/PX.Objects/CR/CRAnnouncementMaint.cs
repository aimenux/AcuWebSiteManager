using System;
using PX.Common;
using PX.Data;
using PX.Objects.SM;
using PX.SM;

namespace PX.Objects.CR
{
	public class CRAnnouncementMaint : PXGraph<CRAnnouncementMaint>
	{
		#region Select
		public PXSelect<CRAnnouncement> Announcement;
		#endregion

		#region Action
		public PXSave<CRAnnouncement> Save;
		public PXCancel<CRAnnouncement> Cancel;
		public PXInsert<CRAnnouncement> Insert;
		public PXDelete<CRAnnouncement> Delete;
		public PXFirst<CRAnnouncement> First;
		public PXPrevious<CRAnnouncement> Prev;
		public PXNext<CRAnnouncement> Next;
		public PXLast<CRAnnouncement> Last;
		#endregion

		#region Announcement handlers
		protected virtual void CRAnnouncement_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				CRAnnouncement announcement = (CRAnnouncement)e.Row;
				PXUIFieldAttribute.SetEnabled<Notification.notificationID>(cache, announcement, true);
			}
		}

		protected virtual void CRAnnouncement_Category_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row != null)
			{
				e.Cancel = true;
			}
		}
		#endregion
	}
}
