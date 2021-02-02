using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using System;

namespace PX.Commerce.Objects
{
	[Serializable]
	[PXCacheName("BC Inventory File Urls")]
	public class BCInventoryFileUrls : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BCInventoryFileUrls>.By<BCInventoryFileUrls.fileID, BCInventoryFileUrls.inventoryID>
		{
			public static BCInventoryFileUrls Find(PXGraph graph, int? fileID, int? inventoryID) => FindBy(graph, fileID, inventoryID);
		}
		public static class FK
		{
			public class Item : InventoryItem.PK.ForeignKeyOf<BCInventoryFileUrls>.By<inventoryID> { }
		}
		#endregion
		#region FileID
		[PXDBIdentity(IsKey = true)]
		//[PXUIField(DisplayName = "File ID", Visible = false)]
		public int? FileID { get; set; }
		public abstract class fileID : IBqlField { }
		#endregion

		#region InventoryID
		[PXDBInt]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : IBqlField { }
		#endregion

		#region FileURL
		[PXDBString(IsUnicode =true)]
		[PXUIField(DisplayName = "URL")]
	    [PXDefault]
		[PXFieldDescription]
		public virtual string FileURL { get; set; }
		public abstract class fileURL : IBqlField { }
		#endregion

		#region FileType
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Type")]
		[BCFileType]
		[PXDefault(BCFileTypeAttribute.Image)]
		public virtual string FileType { get; set; }
		public abstract class fileType : IBqlField { }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
	}
}
