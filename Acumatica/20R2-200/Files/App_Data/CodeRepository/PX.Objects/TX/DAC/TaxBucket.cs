namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	using PX.Objects.AP;
	using PX.Objects.CS;
	using PX.Objects.GL;

	/// <summary>
	/// Represents a reporting group. The head of the master-detail aggregate, 
	/// which specifies a set of report lines to which tax amounts should be aggregated on report prepare.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxBucket)]
	public partial class TaxBucket : PX.Data.IBqlTable
	{
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;

		/// <summary>
		/// The foreign key to <see cref="PX.Objects.AP.Vendor">, which specifies a tax agency to which the reporting group belongs.
		/// The field is a part of the primary key.
		/// </summary>
		[PXDBInt(IsKey=true)]
		[PXDefault()]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region BucketID
		public abstract class bucketID : PX.Data.BQL.BqlInt.Field<bucketID> { }
		protected Int32? _BucketID;

		/// <summary>
		/// The identifier of the reporting group.
		/// The field is a part of the primary key.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Reporting Group", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? BucketID
		{
			get
			{
				return this._BucketID;
			}
			set
			{
				this._BucketID = value;
			}
		}
		#endregion
		#region BucketType
		public abstract class bucketType : PX.Data.BQL.BqlString.Field<bucketType> { }
		protected String _BucketType;

		/// <summary>
		/// The type of the reporting group.
		/// </summary>
		/// /// <value>
		/// The field can have one of the following values:
		/// <c>"S"</c>: Output (sales).
		/// <c>"P"</c>: Input (purchase).
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault("S")]
		[PXUIField(DisplayName="Group Type", Visibility=PXUIVisibility.SelectorVisible)]
		[PXStringList(new string[] {"S", "P"}, new string[] {"Output", "Input"})]
		public virtual String BucketType
		{
			get
			{
				return this._BucketType;
			}
			set
			{
				this._BucketType = value;
			}
		}
		#endregion
		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		protected String _Name;

		/// <summary>
		/// The name of the reporting group, which can be specified by the user.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
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
	}

	public static class CSTaxBucketType
	{
		public const int DEFAULT_INPUT_GROUP = -1;
		public const int DEFAULT_OUTPUT_GROUP = -2;

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Sales, Purchase },
				new string[] { Messages.Sales, Messages.Purchase }) { }
		}

		public const string Sales = "S";
		public const string Purchase = "P";

		public class sales : PX.Data.BQL.BqlString.Constant<sales>
		{
			public sales() : base(Sales) { ;}
		}

		public class purchase : PX.Data.BQL.BqlString.Constant<purchase>
		{
			public purchase() : base(Purchase) { ;}
		}

	}

}
