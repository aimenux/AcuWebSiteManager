using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.IN;

	/// <summary>
	/// For <see cref="DiscountSequence">discount sequences</see> applicable to certain 
	/// <see cref="InventoryItem">inventory items</see>, records of this type define 
	/// specific items to which the corresponding sequence applies. The entities of 
	/// this type can be edited on the Items tab of the Discounts (AR209500) form, 
	/// which corresponds to the <see cref="ARDiscountSequenceMaint"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.DiscountItem)]
	public partial class DiscountItem : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<DiscountItem>.By<discountID, discountSequenceID, inventoryID>
		{
			public static DiscountItem Find(PXGraph graph, string discountID, string discountSequenceID, int? inventoryID)
				=> FindBy(graph, discountID, discountSequenceID, inventoryID);
		}

		public class DiscountSequenceFK : DiscountSequence.PK.ForeignKeyOf<DiscountItem>.By<discountID, discountSequenceID> { }
		public class InventoryFK : InventoryItem.PK.ForeignKeyOf<DiscountItem>.By<inventoryID> { }
		#endregion
		#region DiscountID
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
		protected String _DiscountID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(DiscountSequence.discountID))]
		public virtual String DiscountID
		{
			get
			{
				return this._DiscountID;
			}
			set
			{
				this._DiscountID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[InventoryIncludingTemplates(IsKey=true)]
        [PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<DiscountItem.inventoryID>>>>))]
        [PXDefault()]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region DiscountSequenceID
		public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
		protected String _DiscountSequenceID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(DiscountSequence.discountSequenceID))]
		[PXParent(typeof(Select<DiscountSequence, 
		Where<DiscountSequence.discountSequenceID, Equal<Current<DiscountItem.discountSequenceID>>,
		And<DiscountSequence.discountID, Equal<Current<DiscountItem.discountID>>>>>))]
		public virtual String DiscountSequenceID
		{
			get
			{
				return this._DiscountSequenceID;
			}
			set
			{
				this._DiscountSequenceID = value;
			}
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		protected Decimal? _Amount;
		[PXDBPriceCost(MinValue=0)]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Visible=false)]
		public virtual Decimal? Amount
		{
			get
			{
				return this._Amount;
			}
			set
			{
				this._Amount = value;
			}
		}
		#endregion
		#region Quantity
		public abstract class quantity : PX.Data.BQL.BqlDecimal.Field<quantity> { }
		protected Decimal? _Quantity;
		[PXDBQuantity(MinValue=0)]
		[PXUIField(DisplayName="Quantity", Visibility=PXUIVisibility.Visible, Visible=false)]
		public virtual Decimal? Quantity
		{
			get
			{
				return this._Quantity;
			}
			set
			{
				this._Quantity = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<ARSalesPrice.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(DiscountItem.inventoryID), Visible=false)]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
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
