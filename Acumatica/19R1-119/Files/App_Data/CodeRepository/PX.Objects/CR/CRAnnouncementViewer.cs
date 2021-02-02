using PX.Data;

namespace PX.Objects.CR
{
	public class CRAnnouncementViewer : PXGraph<CRAnnouncementViewer>
	{
		#region Select
		public PXSelect<CRAnnouncement> Announcement;
		#endregion

		#region Action
		public PXCancel<CRAnnouncement> Cancel;
		#endregion
	}
}
