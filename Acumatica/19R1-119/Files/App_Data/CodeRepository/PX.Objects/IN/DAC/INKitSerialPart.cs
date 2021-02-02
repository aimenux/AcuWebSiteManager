using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[Serializable]
	public partial class INKitSerialPart : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INKitSerialPart>.By<docType, refNbr, kitLineNbr, kitSplitLineNbr, partLineNbr, partSplitLineNbr>
		{
			public static INKitSerialPart Find(PXGraph graph, string docType, string refNbr, int? kitLineNbr, int? kitSplitLineNbr, int? partLineNbr, int? partSplitLineNbr) 
				=> FindBy(graph, docType, refNbr, kitLineNbr, kitSplitLineNbr, partLineNbr, partSplitLineNbr);
		}
		public static class FK
		{
			public class Register : INRegister.PK.ForeignKeyOf<INKitSerialPart>.By<docType, refNbr> { }
			public class TranSplit : INTranSplit.PK.ForeignKeyOf<INKitSerialPart>.By<docType, refNbr, kitLineNbr, kitSplitLineNbr> { }
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(typeof(INRegister.docType))]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INRegister.refNbr))]
		[PXParent(typeof(FK.Register))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region KitLineNbr
		public abstract class kitLineNbr : PX.Data.BQL.BqlInt.Field<kitLineNbr> { }
		protected Int32? _KitLineNbr;
		[PXDBInt(IsKey = true)]
		public virtual Int32? KitLineNbr
		{
			get
			{
				return this._KitLineNbr;
			}
			set
			{
				this._KitLineNbr = value;
			}
		}
		#endregion
		#region KitSplitLineNbr
		public abstract class kitSplitLineNbr : PX.Data.BQL.BqlInt.Field<kitSplitLineNbr> { }
		protected Int32? _KitSplitLineNbr;
		[PXDBInt(IsKey=true)]
		public virtual Int32? KitSplitLineNbr
		{
			get
			{
				return this._KitSplitLineNbr;
			}
			set
			{
				this._KitSplitLineNbr = value;
			}
		}
		#endregion
		#region PartLineNbr
		public abstract class partLineNbr : PX.Data.BQL.BqlInt.Field<partLineNbr> { }
		protected Int32? _PartLineNbr;
		[PXDBInt(IsKey = true)]
		public virtual Int32? PartLineNbr
		{
			get
			{
				return this._PartLineNbr;
			}
			set
			{
				this._PartLineNbr = value;
			}
		}
		#endregion
		#region PartSplitLineNbr
		public abstract class partSplitLineNbr : PX.Data.BQL.BqlInt.Field<partSplitLineNbr> { }
		protected Int32? _PartSplitLineNbr;
		[PXDBInt(IsKey = true)]
		public virtual Int32? PartSplitLineNbr
		{
			get
			{
				return this._PartSplitLineNbr;
			}
			set
			{
				this._PartSplitLineNbr = value;
			}
		}
		#endregion

		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}
}
