using System;
using PX.Data;

namespace PX.Objects.CR
{
    [Serializable]
    [PXHidden]
	[PXCacheName(Messages.SelectionCriteria)]
	public partial class CRFixedFilterRow : IBqlTable
	{
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;
		[PXDBGuid(IsKey = true)]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion

		#region FilterRowNbr
		public abstract class filterRowNbr : PX.Data.BQL.BqlInt.Field<filterRowNbr> { }
		protected Int32? _FilterRowNbr;
		[PXDefault]
		[RowNbr]
		[PXDBInt(IsKey = true)]
		[CRUnsafeUIField(Visible = false, Enabled = false)]
		public virtual Int32? FilterRowNbr
		{
			get
			{
				return this._FilterRowNbr;
			}
			set
			{
				this._FilterRowNbr = value;
			}
		}
		#endregion

		#region IsUsed
		public abstract class isUsed : PX.Data.BQL.BqlBool.Field<isUsed> { }
		protected bool? _IsUsed;
		[PXDefault(true)]
		[PXDBBool()]
		[CRUnsafeUIField(DisplayName = "Active")]
		public virtual bool? IsUsed
		{
			get
			{
				return this._IsUsed;
			}
			set
			{
				this._IsUsed = value;
			}
		}
		#endregion

		#region OpenBrackets
		public abstract class openBrackets : PX.Data.BQL.BqlInt.Field<openBrackets> { }
		protected Int32? _OpenBrackets;
		[PXDefault(0)]
		[PXDBInt]
		[FilterRow.OpenBrackets]
		[CRUnsafeUIField(DisplayName = "Brackets")]
		public virtual Int32? OpenBrackets
		{
			get
			{
				return _OpenBrackets;
			}
			set
			{
				_OpenBrackets = value;
			}
		}
		#endregion

		#region CloseBrackets
		public abstract class closeBrackets : PX.Data.BQL.BqlInt.Field<closeBrackets> { }
		protected Int32? _CloseBrackets;
		[PXDefault(0)]
		[PXDBInt]
		[FilterRow.CloseBrackets]
		[CRUnsafeUIField(DisplayName = "Brackets")]
		public virtual Int32? CloseBrackets
		{
			get
			{
				return _CloseBrackets;
			}
			set
			{
				_CloseBrackets = value;
			}
		}
		#endregion

		#region DataField
		public abstract class dataField : PX.Data.BQL.BqlString.Field<dataField> { }
		protected String _DataField;
		[PXDefault]
		[PXDBString(50)]
		[CRUnsafeUIField(DisplayName = "Property")]
		public virtual String DataField
		{
			get
			{
				return this._DataField;
			}
			set
			{
				this._DataField = value;
			}
		}
		#endregion

		#region Condition
		public abstract class condition : PX.Data.BQL.BqlByte.Field<condition> { }
		protected Byte? _Condition;
		[PXDefault]
		[RowCondition]
		[CRUnsafeUIField(DisplayName = "Condition")]
		public virtual Byte? Condition
		{
			get
			{
				return this._Condition;
			}
			set
			{
				this._Condition = value;
			}
		}
		#endregion

		#region ValueSt
		public abstract class valueSt : PX.Data.BQL.BqlString.Field<valueSt> { }
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
		public virtual string ValueSt { get; set; }
		#endregion
		#region ValueSt2
		public abstract class valueSt2 : PX.Data.BQL.BqlString.Field<valueSt2> { }
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Value2")]
		public virtual string ValueSt2 { get; set; }
		#endregion

		#region Operator
		public abstract class @operator : PX.Data.BQL.BqlInt.Field<@operator> { }
		protected Int32? _Operator;
		[PXDefault(0)]
		[PXDBInt]
		[CRUnsafeUIField(DisplayName = "Operator")]
		[PXIntList(new int[] { 0, 1}, new string[] { "And", "Or" })]
		public virtual Int32? Operator
		{
			get
			{
				return this._Operator;
			}
			set
			{
				this._Operator = value;
			}
		}
		#endregion
	}
}
