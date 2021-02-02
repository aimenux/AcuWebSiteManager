using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using System;

namespace PX.Objects.IN.Matrix.DAC
{
	[PXCacheName(Messages.AttributeDescriptionItemDAC)]
	public class INAttributeDescriptionItem : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INCostStatus>.By<templateID, groupID, attributeID, valueID>
		{
			public static INCostStatus Find(PXGraph graph, int? templateID, int? groupID, string attributeID, string valueID)
				=> FindBy(graph, templateID, groupID, attributeID, valueID);
		}
		public static class FK
		{
			public class TemplateItem : InventoryItem.PK.ForeignKeyOf<INCostStatus>.By<templateID> { }
		}
		#endregion

		#region TemplateID
		public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }
		/// <summary>
		/// References to Inventory Item which is template.
		/// </summary>
		[PXUIField(DisplayName = "Template Item")]
		[TemplateInventory(IsKey = true)]
		[PXParent(typeof(FK.TemplateItem))]
		public virtual int? TemplateID
		{
			get;
			set;
		}
		#endregion
		#region GroupID
		public abstract class groupID : PX.Data.BQL.BqlInt.Field<groupID> { }
		/// <summary>
		/// Identifier group of attributes
		/// </summary>
		[PXDBInt(IsKey = true)]
		public virtual int? GroupID
		{
			get;
			set;
		}
		#endregion
		#region AttributeID
		public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }
		/// <summary>
		/// Identifier of attribute
		/// </summary>
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = "")]
		public virtual string AttributeID
		{
			get;
			set;
		}
		#endregion
		#region ValueID
		public abstract class valueID : PX.Data.BQL.BqlString.Field<valueID> { }
		/// <summary>
		/// Value of attribute
		/// </summary>
		[PXAttributeValue]
		public virtual string ValueID
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
