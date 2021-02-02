using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.IN
{
	/// <summary>
	/// Represents a Auto-Incremental Value of a Lot/Serial Class.
	/// Auto-Incremental Value of a Lot/Serial Class are available only if the <see cref="FeaturesSet.LotSerialTracking">Lot/Serial Tracking</see> feature is enabled.
	/// The records of this type are created through the Lot/Serial Classes (IN.20.70.00) screen
	/// (corresponds to the <see cref="INLotSerClassMaint"/> graph)
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(INLotSerClassMaint))]
	[PXCacheName(Messages.LotSerClassAutoIncrementalValue)]
	public class INLotSerClassLotSerNumVal : IBqlTable, ILotSerNumVal
	{
		#region Keys
		public class PK : PrimaryKeyOf<INLotSerClassLotSerNumVal>.By<lotSerClassID>
		{
			public static INLotSerClassLotSerNumVal Find(PXGraph graph, string lotSerClassID) => FindBy(graph, lotSerClassID);
			public static INLotSerClassLotSerNumVal FindDirty(PXGraph graph, string lotSerClassID)
				=> (INLotSerClassLotSerNumVal)PXSelect<INLotSerClassLotSerNumVal,
					Where<lotSerClassID, Equal<Required<lotSerClassID>>>>.SelectWindowed(graph, 0, 1, lotSerClassID);
		}
		public static class FK
		{
			public class LotSerClass : INLotSerClass.PK.ForeignKeyOf<INLotSerClassLotSerNumVal>.By<lotSerClassID> { }
		}
		#endregion
		#region LotSerClassID
		public abstract class lotSerClassID : BqlString.Field<lotSerClassID> { }
		protected string _LotSerClassID;

		/// <summary>
		/// The <see cref="INLotSerClass">lot/serial class</see>, to which the item is assigned.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INLotSerClass.LotSerClassID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INLotSerClass.lotSerClassID))]
		[PXParent(typeof(FK.LotSerClass))]
		public virtual string LotSerClassID
		{
			get
			{
				return this._LotSerClassID;
			}
			set
			{
				this._LotSerClassID = value;
			}
		}
		#endregion
		#region LotSerNumVal
		public abstract class lotSerNumVal : BqlString.Field<lotSerNumVal> { }
		protected string _LotSerNumVal;
		[PXDBString(30, InputMask = "999999999999999999999999999999")]
		[PXUIField(DisplayName = "Auto-Incremental Value")]
		public virtual string LotSerNumVal
		{
			get
			{
				return this._LotSerNumVal;
			}
			set
			{
				this._LotSerNumVal = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
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
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
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
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
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
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
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
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
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
		#region tstamp
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		protected byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual byte[] tstamp
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
	}
}
