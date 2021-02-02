using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AR
{
	using System;
	using PX.Data;

	/// <summary>
	/// For <see cref="DiscountSequence">discount sequences</see> applicable to certain 
	/// <see cref="ARPriceClass">customer price classes</see>, records of this type define 
	/// specific customer price classes to which the corresponding sequence applies. The 
	/// entities of this type can be edited on the Customer Price Classes tab of the Discounts 
	/// (AR209500) form, which corresponds to the <see cref="ARDiscountSequenceMaint"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.DiscountCustomerPriceClass)]
	public partial class DiscountCustomerPriceClass : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<DiscountCustomerPriceClass>.By<discountID, discountSequenceID, customerPriceClassID>
		{
			public static DiscountCustomerPriceClass Find(PXGraph graph, string discountID, string discountSequenceID, string customerPriceClassID)
				=> FindBy(graph, discountID, discountSequenceID, customerPriceClassID);
		}

		public class DiscountSequenceFK : DiscountSequence.PK.ForeignKeyOf<DiscountCustomerPriceClass>.By<discountID, discountSequenceID> { }
		public class CustomerPriceClassFK : ARPriceClass.PK.ForeignKeyOf<DiscountCustomerPriceClass>.By<customerPriceClassID> { }
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
		#region CustomerPriceClassID
		public abstract class customerPriceClassID : PX.Data.BQL.BqlString.Field<customerPriceClassID> { }
		protected String _CustomerPriceClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXSelector(typeof(ARPriceClass.priceClassID))]
		[PXUIField(DisplayName="Price Class ID", Visibility=PXUIVisibility.Visible)]
		public virtual String CustomerPriceClassID
		{
			get
			{
				return this._CustomerPriceClassID;
			}
			set
			{
				this._CustomerPriceClassID = value;
			}
		}
		#endregion
		#region DiscountSequenceID
		public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
		protected String _DiscountSequenceID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(DiscountSequence.discountSequenceID))]
		[PXParent(typeof(Select<DiscountSequence, 
			Where<DiscountSequence.discountSequenceID, Equal<Current<DiscountCustomerPriceClass.discountSequenceID>>,
			And<DiscountSequence.discountID, Equal<Current<DiscountCustomerPriceClass.discountID>>>>>))]
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
