using System;
using PX.Data;

namespace PX.Objects.GDPR
{
	[Serializable]
	[PXHidden]
	public class SMPersonalDataIndex : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion
		
		#region UIKey
		public abstract class uIKey : PX.Data.BQL.BqlString.Field<uIKey> { }

		[PXString]
		[PXUIField(DisplayName = "Key")]
		public virtual string UIKey
		{
			get { return CombinedKey; }
		}
		#endregion

		#region CombinedKey
		public abstract class combinedKey : PX.Data.BQL.BqlString.Field<combinedKey> { }

		[PXDBString(IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Key")]
		public virtual string CombinedKey { get; set; }
		#endregion

		#region IndexID
		public abstract class indexID : PX.Data.BQL.BqlGuid.Field<indexID> { }

		[PXDBGuid]
		public virtual Guid? IndexID { get; set; }
		#endregion

		#region Content
		public abstract class content : PX.Data.BQL.BqlString.Field<content> { }

		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Entity")]
		public virtual String Content { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
	}
}