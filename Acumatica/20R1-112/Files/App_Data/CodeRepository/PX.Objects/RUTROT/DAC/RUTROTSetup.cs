using System;
using PX.Data;

namespace PX.Objects.RUTROT
{
	[Serializable]
	public class RUTROTSetup : IBqlTable
	{
		#region NsMain
		public abstract class nsMain : PX.Data.BQL.BqlString.Field<nsMain> { }
		[PXDBString]
		[PXDefault]
		[PXUIField(DisplayName = "NsMain", Enabled = true, Visible = true, Visibility = PXUIVisibility.Visible, FieldClass = RUTROTMessages.FieldClass)]
		public virtual string NsMain
		{
			get;
			set;
		}
		#endregion

		#region NsHtko
		public abstract class nsHtko : PX.Data.BQL.BqlString.Field<nsHtko> { }
		[PXDBString]
		[PXDefault]
		[PXUIField(DisplayName = "NsHtko", Enabled = true, Visible = true, Visibility = PXUIVisibility.Visible, FieldClass = RUTROTMessages.FieldClass)]
		public virtual string NsHtko
		{
			get;
			set;
		}
		#endregion

		#region NsXsi
		public abstract class nsXsi : PX.Data.BQL.BqlString.Field<nsXsi> { }
		[PXDBString]
		[PXDefault]
		[PXUIField(DisplayName = "NsXsi", Enabled = true, Visible = true, Visibility = PXUIVisibility.Visible, FieldClass = RUTROTMessages.FieldClass)]
		public virtual string NsXsi
		{
			get;
			set;
		}
		#endregion

		#region Schema location
		public abstract class schemaLocation : PX.Data.BQL.BqlString.Field<schemaLocation> { }
		[PXDBString]
		[PXDefault]
		[PXUIField(DisplayName = "Schema location", Enabled = true, Visible = true, Visibility = PXUIVisibility.Visible, FieldClass = RUTROTMessages.FieldClass)]
		public virtual string SchemaLocation
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
