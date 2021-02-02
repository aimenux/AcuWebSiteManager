namespace PX.Objects.AP
{
	using System;
	using PX.Data;
    using PX.Data.BQL.Fluent;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.APDiscountLocation)]
	public class APDiscountLocation : PX.Data.IBqlTable
	{
		#region DiscountID
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
		protected string _DiscountID;
		[PXDBString(10, IsKey = true, IsUnicode = true)]
        [PXDBDefault(typeof(VendorDiscountSequence.discountID))]
		[PXUIField(DisplayName = "DiscountID")]
		public virtual string DiscountID
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
		#region DiscountSequenceID
		public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
		protected string _DiscountSequenceID;
		[PXDBString(10, IsKey = true, IsUnicode = true)]
        [PXDBDefault(typeof(AP.VendorDiscountSequence.discountSequenceID))]
		[PXParent(typeof(SelectFrom<VendorDiscountSequence>.
			Where<VendorDiscountSequence.discountID.IsEqual<discountID>
				.And<VendorDiscountSequence.discountSequenceID.IsEqual<discountSequenceID>>>))]
		[PXUIField(DisplayName = "DiscountSequenceID")]
		public virtual string DiscountSequenceID
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
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected int? _VendorID;
        [PXDefault(typeof(VendorDiscountSequence.vendorID))]
        [PXDBInt(IsKey = true)]
        public virtual int? VendorID
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
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected int? _LocationID;
		[PXDefault()]
        [CS.LocationID(
             typeof(Where<CR.Location.bAccountID, Equal<Optional<AP.VendorDiscountSequence.vendorID>>,
                 And<CR.Location.isActive, Equal<True>,
                 And<MatchWithBranch<CR.Location.vBranchID>>>>),
             DescriptionField = typeof(CR.Location.descr),
             Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
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
		protected string _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID
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
		protected string _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID
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
