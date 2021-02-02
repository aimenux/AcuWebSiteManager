using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using PX.Common;
using PX.Data;
using PX.Objects.CR.DAC;
using PX.SM;

namespace PX.Objects.CR
{
	[DashboardType((int) DashboardTypeAttribute.Type.Default)]
    [Serializable]
	public class CRCommunicationAnnouncement : PXGraph<CRCommunicationAnnouncement>
	{
		#region Local Types
        [Serializable]
		[PXHidden]
		public partial class CRAnnouncementFilter : IBqlTable
		{
			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			protected string _Status;
			[PXStringList(
				new string[] { NotificationStatusAttribute.Draft, NotificationStatusAttribute.Published, NotificationStatusAttribute.Archived },
				new string[] { "Draft", "Published", "Archived" }, BqlField = typeof(CRAnnouncement.status))]
			[PXString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Status")]
			public virtual string Status { get; set; }
			#endregion

			#region CreatedByID
			public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
			[PXGuid]
			[PXSelector(typeof(Search<Users.pKID>), new Type[] { typeof(Users.username), typeof(Users.fullName) }, DescriptionField = typeof(Users.displayName))]
			[PXUIField(DisplayName = "Created By")]
			public virtual Guid? CreatedByID { get; set; }
			#endregion

			#region
			public abstract class category : PX.Data.BQL.BqlString.Field<category> { }
			protected String _Category;
			[PXString(255, InputMask = "", IsUnicode = true)]
			[PXSelector(typeof(Search4<CRAnnouncement.category, Where<CRAnnouncement.category, IsNotNull>, Aggregate<GroupBy<CRAnnouncement.category>>>), typeof(CRAnnouncement.category))]
			[PXUIField(DisplayName = "Category", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Category
			{
				get
				{
					return this._Category;
				}
				set
				{
					this._Category = value;
				}
			}
			#endregion
		}
		#endregion

		#region Select
		public PXFilter<CRAnnouncementFilter> Filter;

		[PXFilterable]
		public PXSelect<CRAnnouncement,
		Where2<Where<Current<CRAnnouncementFilter.category>, IsNull,
					Or<CRAnnouncement.category, Equal<Current<CRAnnouncementFilter.category>>>>,
				And2<Where<Current<CRAnnouncementFilter.createdByID>, IsNull,
					Or<CRAnnouncement.createdByID, Equal<Current<CRAnnouncementFilter.createdByID>>>>,
				And<Where<CRAnnouncement.status, Equal<NotificationStatusAttribute.published>>>>>,
		OrderBy<Desc<CRAnnouncement.publishedDateTime>>> Announcements;
		#endregion

		#region Action
		public PXCancel<CRAnnouncementFilter> Cancel;
		public PXAction<CRAnnouncementFilter> viewDetails;
		[PXUIField(DisplayName = Messages.ViewDetails, MapViewRights = PXCacheRights.Select, Enabled = false, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			var graph = PXGraph.CreateInstance<CRCommunicationAnnouncementPreview>();
			graph.AnnouncementsDetails.Current.Body = "<html><head></head><body>";
			graph.AnnouncementsDetails.Current.Body = graph.AnnouncementsDetails.Current.Body + "<font size=\"4\">";
			graph.AnnouncementsDetails.Current.Body = graph.AnnouncementsDetails.Current.Body + Announcements.Current.Subject;
			graph.AnnouncementsDetails.Current.Body = graph.AnnouncementsDetails.Current.Body + "</font>";
			graph.AnnouncementsDetails.Current.Body = graph.AnnouncementsDetails.Current.Body + "<br/><br/>" + Tools.RemoveHeader(Announcements.Current.Body);
			graph.AnnouncementsDetails.Current.Body = graph.AnnouncementsDetails.Current.Body + "</body></html>";
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}
		#endregion

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
	}


	public class CRCommunicationAnnouncementPreview : PXGraph<CRCommunicationAnnouncementPreview>
	{
		#region Local Types
		[Serializable]
		[PXHidden]
		public partial class CRAnnouncementDetails : IBqlTable
		{
			#region Body
			public abstract class body : PX.Data.BQL.BqlString.Field<body> { }
			protected String _Body;
			[PXString(IsUnicode = true)]
			[PXUIField(DisplayName = "Body")]
			public virtual String Body
			{
				get
				{
					return this._Body;
				}
				set
				{
					this._Body = value;
				}
			}	
			#endregion
		}
		#endregion

		#region Select
		public PXFilter<CRAnnouncementDetails> AnnouncementsDetails;
		#endregion
	}
}
