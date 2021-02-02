using System;
using PX.Data;
using System.Diagnostics;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRAttribute)]
	[DebuggerDisplay("[{AttributeID}]: {Description}")]
	[Serializable]
	public partial class PRAttribute : IBqlTable
	{
		public const int Text = 1;
		public const int Combo = 2;
		public const int CheckBox = 4;
		public const int Datetime = 5;
		public const int MultiSelectCombo = 6;

		#region AttributeID
		public abstract class attributeID : IBqlField { }
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Attribute ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(PRAttribute.attributeID))]
		public virtual String AttributeID { get; set; }
		#endregion
		#region Description
		public abstract class description : IBqlField { }
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description { get; set; }
		#endregion
		#region ControlType
		public abstract class controlType : IBqlField { }
		[PXDBInt()]
		[PXDefault(1)]
		[PXUIField(DisplayName = "Control Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXIntList(new int[] { Text, Combo, MultiSelectCombo, CheckBox, Datetime }, new string[] { "Text", "Combo", "Multi Select Combo", "Checkbox", "Datetime" })]
		public virtual Int32? ControlType { get; set; }
		#endregion
		#region EntryMask
		public abstract class entryMask : IBqlField { }
		[PXDBString(60)]
		[PXUIField(DisplayName = "Entry Mask")]
		public virtual String EntryMask { get; set; }
		#endregion
		#region RegExp
		public abstract class regExp : IBqlField { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Reg. Exp.")]
		public virtual String RegExp { get; set; }
		#endregion
		#region List
		public abstract class list : IBqlField { }
		[PXDBLocalizableString(IsUnicode = true)]
		public virtual String List { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
	}
}
