using PX.Data;

namespace PX.Objects.IN.Matrix.DAC.Unbound
{
	[PXCacheName(Messages.TemplateAttributesDAC)]
	public class TemplateAttributes : IBqlTable
	{
		#region TemplateItemID
		public abstract class templateItemID : PX.Data.BQL.BqlInt.Field<templateItemID> { }
		/// <summary>
		/// References to parent Inventory Item.
		/// </summary>
		[PXUIField(DisplayName = "Template Item")]
		[TemplateInventory(IsKey = true)]
		public virtual int? TemplateItemID
		{
			get;
			set;
		}
		#endregion
		#region AttributeIdentifiers
		public abstract class attributeIdentifiers : PX.Data.BQL.BqlByteArray.Field<attributeIdentifiers> { }
		/// <summary>
		/// Array to store attribute identifiers (CSAttribute.attributeID) of additional attributes which are not from matrix (columns)
		/// </summary>
		public virtual string[] AttributeIdentifiers
		{
			get;
			set;
		}
		#endregion
	}
}
