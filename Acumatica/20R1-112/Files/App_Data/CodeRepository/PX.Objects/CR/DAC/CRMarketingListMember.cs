using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.MarketingListMember)]
	public partial class CRMarketingListMember : IBqlTable
	{
		#region ContactID

		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Name")]
		[PXSelector(typeof(Search2<Contact.contactID,
		        LeftJoin<GL.Branch,
		            On<GL.Branch.bAccountID, Equal<Contact.bAccountID>,
		            And<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>>,
            Where<Branch.bAccountID, IsNull, 
				And<Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>
			>>),
			typeof(Contact.fullName),
			typeof(Contact.displayName),
			typeof(Contact.eMail),
			typeof(Contact.phone1),
			typeof(Contact.bAccountID),
			typeof(Contact.salutation),
			typeof(Contact.contactType),
			typeof(Contact.isActive),
		    typeof(Contact.memberName),
            DescriptionField = typeof(Contact.memberName), 
			Filterable = true, 
			DirtyRead = true)]
		[PXParent(typeof(Select<Contact, Where<Contact.contactID, Equal<Current<CRMarketingListMember.contactID>>>>))]
		public virtual Int32? ContactID { get; set; }

		#endregion

		#region MarketingListID

		public abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }

		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(CRMarketingList.marketingListID))]
		[PXUIField(DisplayName = "Marketing List ID")]
		[PXSelector(typeof(Search<CRMarketingList.marketingListID,
			Where<CRMarketingList.isDynamic, Equal<False>>>),
		    DescriptionField = typeof(CRMarketingList.mailListCode))]
		public virtual Int32? MarketingListID { get; set; }

		#endregion

		#region IsSubscribed

		public abstract class isSubscribed : PX.Data.BQL.BqlBool.Field<isSubscribed> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Subscribed")]
		public virtual Boolean? IsSubscribed { get; set; }

		#endregion

		#region Format

		public abstract class format : PX.Data.BQL.BqlString.Field<format> { }
		[PXDBString]
		[PXDefault(NotificationFormat.Html)]
		[PXUIField(DisplayName = "Format")]		
		[NotificationFormat.TemplateList]
		public virtual string Format { get; set; }

		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region CreatedByScreenID

		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }

		#endregion

		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }

		#endregion

		#region CreatedDateTime

		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }

		#endregion

		#region LastModifiedByID

		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }

		#endregion

		#region LastModifiedByScreenID

		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }

		#endregion

		#region LastModifiedDateTime

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }

		#endregion

		#region tstamp

		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }

		#endregion
	}
}
