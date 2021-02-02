using System;
using PX.Data;
using PX.SM;
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.PM
{
	[PXHidden]
	[PXProjection(typeof(Select<NotificationSetup,
		Where<NotificationSetup.module, Equal<PXModule.pm>>>), Persistent = true)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMNotification : NotificationSetup
	{
		#region SetupID
		public new abstract class setupID : PX.Data.BQL.BqlGuid.Field<setupID> { }
		#endregion
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault(PXModule.PM)]
		public override string Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region SourceCD
		public new abstract class sourceCD : PX.Data.BQL.BqlString.Field<sourceCD> { }
		[PXDefault(PMNotificationSource.Project)]
		[PXDBString(10, InputMask = ">aaaaaaaaaa")]
		public override string SourceCD
		{
			get
			{
				return this._SourceCD;
			}
			set
			{
				this._SourceCD = value;
			}
		}
		#endregion
		#region NotificationCD
		public new abstract class notificationCD : PX.Data.BQL.BqlString.Field<notificationCD> { }
		#endregion
		#region ReportID
		public new abstract class reportID : PX.Data.BQL.BqlString.Field<reportID> { }
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report ID")]
		[PXSelector(typeof(Search<SiteMap.screenID,
			Where<SiteMap.screenID, Like<PXModule.pm_>, And<SiteMap.url, Like<Common.urlReports>>>,
			OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
			Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
			DescriptionField = typeof(SiteMap.title))]
		public override String ReportID
		{
			get
			{
				return this._ReportID;
			}
			set
			{
				this._ReportID = value;
			}
		}
		#endregion
		#region TemplateID
		public abstract class templateID : PX.Data.IBqlField { }
		#endregion
		#region Active
		public new abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
	}

	public class PMNotificationSource
	{
		//public const string Customer = "Customer";
		//public class customer : PX.Data.BQL.BqlString.Constant<customer> { public customer() : base(Customer) { } }

		public const string Project = "Project";
		public class project : PX.Data.BQL.BqlString.Constant<project> { public project() : base(Project) { } }
	}
}
