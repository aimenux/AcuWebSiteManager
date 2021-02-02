using System;
using PX.Data;

namespace PX.Objects.CR.DAC
{
	[PXCacheName(Messages.CommunacationClass)]
    [Serializable]
	public partial class CommunicationSummary : IBqlTable
	{
		#region UserID
		public abstract class userID : PX.Data.BQL.BqlString.Field<userID> { }
		[PXUIField(DisplayName = "UserID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXString(250, IsUnicode = true)]
		public virtual String UserID { get; set; }
		#endregion
		
		#region Emails
		public abstract class emails : PX.Data.BQL.BqlString.Field<emails> { }
		[PXUIField(DisplayName = "Emails", Visibility = PXUIVisibility.SelectorVisible)]
		[PXString(250, IsUnicode = true)]
		public virtual String Emails { get; set; }
		#endregion

		#region Tasks
		public abstract class tasks : PX.Data.BQL.BqlString.Field<tasks> { }
		[PXUIField(DisplayName = "Tasks", Visibility = PXUIVisibility.SelectorVisible)]
		[PXString(250, IsUnicode = true)]
		public virtual String Tasks { get; set; }
		#endregion

		#region Events
		public abstract class events : PX.Data.BQL.BqlString.Field<events> { }
		[PXUIField(DisplayName = "Events", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(250, IsUnicode = true)]
		public virtual String Events { get; set; }
		#endregion

		#region Approvals
		public abstract class approvals : PX.Data.BQL.BqlString.Field<approvals> { }
		[PXUIField(DisplayName = "", Visibility = PXUIVisibility.SelectorVisible)]
		[PXString(250, IsUnicode = true)]
		public virtual String Approvals { get; set; }
		#endregion

		#region Announcements
		public abstract class announcements : PX.Data.BQL.BqlString.Field<announcements> { }
		[PXUIField(DisplayName = "Announcements", Visibility = PXUIVisibility.SelectorVisible)]
		[PXString(250, IsUnicode = true)]
		public virtual String Announcements { get; set; }
		#endregion
	}
}
