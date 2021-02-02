using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INCart, PXDacType.Catalogue)]
	public class INCart : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INCart>.By<siteID, cartID>
		{
			public static INCart Find(PXGraph graph, int? siteID, int? cartID) => FindBy(graph, siteID, cartID);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INCart>.By<siteID> { }
		}
		#endregion

		#region SiteID
		[PXDBDefault(typeof(INSite.siteID))]
		[Site(IsKey = true, Visible = false)]
		[PXParent(typeof(FK.Site))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region CartID
		[PXReferentialIntegrityCheck]
		[PXDBForeignIdentity(typeof(INCostSite))]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public int? CartID { get; set; }
		public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
		#endregion
		#region CartCD
		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Cart ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<cartCD, Where<active, Equal<True>>>), DescriptionField = typeof(descr))]
		[PXCheckUnique]
		[PX.Data.EP.PXFieldDescription]
		public string CartCD { get; set; }
		public abstract class cartCD : PX.Data.BQL.BqlString.Field<cartCD> { } 
		#endregion
		#region Descr
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public string Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { } 
		#endregion
		#region Active
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public bool? Active { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
		#region AssignedNbrOfTotes
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Assigned Number of Totes", Enabled = false)]
		public virtual int? AssignedNbrOfTotes { get; set; }
		public abstract class assignedNbrOfTotes : PX.Data.BQL.BqlInt.Field<assignedNbrOfTotes> { }
		#endregion

		#region NoteID
		[PXNote(DescriptionField = typeof(cartCD), Selector = typeof(cartCD))]
		public virtual Guid? NoteID { get; set; }
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion
		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#region tstamp
		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}