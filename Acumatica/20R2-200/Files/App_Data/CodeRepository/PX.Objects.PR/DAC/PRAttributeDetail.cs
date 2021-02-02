namespace PX.Objects.PR
{
	using System;
	using Data;

	[PXCacheName(Messages.PRAttributeDetail)]
	[Serializable]
	public partial class PRAttributeDetail : IBqlTable
	{
		public const int ParameterIdLength = 10;

		#region AttributeID
		public abstract class attributeID : IBqlField { }
		[PXDBString(ParameterIdLength, IsKey = true)]
		[PXDBDefault(typeof(PRAttribute.attributeID))]
		[PXUIField(DisplayName = "Attribute ID")]
		[PXParent(typeof(Select<PRAttribute, Where<PRAttribute.attributeID, Equal<Current<PRAttributeDetail.attributeID>>>>))]
		public virtual String AttributeID { get; set; }
		#endregion
		#region ValueID
		public abstract class valueID : IBqlField { }
		[PXDBString(10, InputMask = "", IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Value ID")]
		public virtual String ValueID { get; set; }
		#endregion
		#region Description
		public abstract class description : IBqlField { }
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description { get; set; }
		#endregion
		#region SortOrder
		public abstract class sortOrder : IBqlField
		{
		}
		protected Int16? _SortOrder;
		[PXDBShort()]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual Int16? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
	}
}
