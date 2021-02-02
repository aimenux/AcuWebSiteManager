using System;
using PX.Data;

namespace PX.Objects.CN.Common.DAC
{
	[Serializable]
	public class BaseCache
	{
		[PXDBTimestamp]
		public virtual byte[] Tstamp
		{
			get;
			set;
		}

		[PXDBCreatedByID]
		public virtual Guid? CreatedById
		{
			get;
			set;
		}

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenId
		{
			get;
			set;
		}

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}

		[PXDBLastModifiedByID(Visibility = PXUIVisibility.Invisible)]
		public virtual Guid? LastModifiedById
		{
			get;
			set;
		}

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenId
		{
			get;
			set;
		}

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}

		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
	}
}