using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
    [Serializable]
	public class CRAnnouncementsExplore : PXGraph<CRAnnouncementsExplore>
	{
		#region CRAnnouncementFilter
        [Serializable]
		[PXHidden]
		public partial class CRAnnouncementFilter : IBqlTable
		{
			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			protected string _Status;
			[PXStringList(
				new string[]
					{NotificationStatusAttribute.Draft, NotificationStatusAttribute.Published, NotificationStatusAttribute.Archived},
				new string[] {"Draft", "Published", "Archived"}, BqlField = typeof(CRAnnouncement.status))]
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

		[PXViewDetailsButton(typeof(CRAnnouncementFilter))]
		public PXSelect<CRAnnouncement,
		Where2<Where<Current<CRAnnouncementFilter.category>, IsNull,
					Or<CRAnnouncement.category, Equal<Current<CRAnnouncementFilter.category>>>>,
				And2<Where<Current<CRAnnouncementFilter.createdByID>, IsNull,
					Or<CRAnnouncement.createdByID, Equal<Current<CRAnnouncementFilter.createdByID>>>>,
				And<Where<Current<CRAnnouncementFilter.status>, IsNull,
					Or<CRAnnouncement.status, Equal<Current<CRAnnouncementFilter.status>>>>>>>,
		OrderBy<Desc<CRAnnouncement.publishedDateTime>>> Announcements;
		#endregion

		#region Action
		public PXCancel<CRAnnouncementFilter> Cancel;
		#endregion
	}
}
