using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
	/// <exclude/>
	[Serializable]
    [PXHidden]
	public partial class CRCaseArticle : IBqlTable
	{
		#region CaseCD
		public abstract class caseCD : PX.Data.BQL.BqlString.Field<caseCD> { }

		[PXDBString(10, IsKey = false)]
		[PXDBDefault(typeof(CRCase.caseCD))]
		[PXParent(typeof(Select<CRCase, Where<CRCase.caseCD, Equal<Current<caseCD>>>>))]
		public virtual string CaseCD { get; set; }
		#endregion

		#region PageID
		public abstract class pageID : PX.Data.BQL.BqlGuid.Field<pageID> { }

		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search2<SimpleWikiPage.pageID,
			InnerJoin<WikiDescriptor, On<WikiDescriptor.pageID, Equal<SimpleWikiPage.wikiID>>>,
			Where<WikiDescriptor.articleType, Equal<WikiArticleTypeAttribute.kb>,
				And<SimpleWikiPage.name, NotLike<GenTemplateLeftLike>,
				And<SimpleWikiPage.name, NotLike<TemplateLeftLike>,
				And<SimpleWikiPage.name, NotLike<ContainerTemplateLeftLike>>>>>>))]
		public virtual Guid? PageID { get; set; }
		#endregion


		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = "Date Reported", Enabled = false)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		[PXUIField(DisplayName = "Last Activity", Enabled = false)]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}
