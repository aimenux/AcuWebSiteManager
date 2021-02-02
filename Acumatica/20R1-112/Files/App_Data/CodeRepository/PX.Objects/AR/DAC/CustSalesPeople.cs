namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.CR;
    using PX.Objects.CS;
	using PX.Data.ReferentialIntegrity.Attributes;

	/// <summary>
	/// Represents a salesperson associated with a customer location. The entity implements the 
	/// many-to-many relationship between <see cref="Location">customer locations</see> 
	/// and <see cref="SalesPerson">salespeople</see>. For a salesperson, records 
	/// of this type allow to specify different default commission percentages for 
	/// different customer locations. The entities of this type can be edited on the 
	/// Salespersons (AR205000) form, which corresponds to the <see cref="SalesPersonMaint"/> 
	/// graph. They can also be edited on the Salespersons tab of the Customer (AR303000) form, 
	/// which corresponds to the <see cref="CustomerMaint"/> graph.
	/// </summary>
	/// <remarks>
	/// When a user specifies a <see cref="SalesPerson">salesperson</see> in an invoice line, the
	/// system looks for a <see cref="CustSalesPeople"/> record that corresponds 
	/// to the customer and location specified in the invoice, and defaults the value of 
	/// <see cref="ARTran.CommnPct"/> from the value of <see cref="CommisionPct"/>. If such 
	/// a record does not exist, the system takes the <see cref="SalesPerson.CommnPct">
	/// default commission percentage of the salesperson</see>.
	/// </remarks>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.CustSalesPeople)]
	public partial class CustSalesPeople : PX.Data.IBqlTable
	{
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		/// <summary>
		/// The integer identifier of the salesperson. This field is a part 
		/// of the compound key of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="SalesPerson.SalesPersonID"/> field.
		/// </value>
		[PXDBLiteDefault(typeof(SalesPerson.salesPersonID))]
		[PXDBInt(IsKey=true)]
		[PXParent(typeof(Select<SalesPerson,Where<SalesPerson.salesPersonID,Equal<Current<CustSalesPeople.salesPersonID>>>>))]
		public virtual Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		/// <summary>
		/// The integer identifier of the customer. This field is a part 
		/// of the compound key of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[Customer( DescriptionField = typeof(Customer.acctName),IsKey=true)]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		/// <summary>
		/// The integer identifier of the customer location. This 
		/// field is a part of the compound key of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Location.LocationID"/> field.
		/// </value>
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CustSalesPeople.bAccountID>>>), IsKey = true, DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(Search<Customer.defLocationID,Where<Customer.bAccountID,Equal<Current<CustSalesPeople.bAccountID>>>>))]
		[PXParent(typeof(Select<Location,
			Where<Location.bAccountID, Equal<Current<CustSalesPeople.bAccountID>>,
				And<Location.locationID, Equal<Current<CustSalesPeople.locationID>>>>>))]
		public virtual Int32? LocationID
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
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		protected Boolean? _IsDefault;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the salesperson
		/// is used by default for the customer location, which is defined 
		/// by the <see cref="BAccountID"/> and <see cref="LocationID"/> 
		/// fields.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Default", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? IsDefault
		{
			get
			{
				return this._IsDefault;
			}
			set
			{
				this._IsDefault = value;
			}
		}
		#endregion
		#region CommisionPct
		public abstract class commisionPct : PX.Data.BQL.BqlDecimal.Field<commisionPct> { }
		protected Decimal? _CommisionPct;
		/// <summary>
		/// The default sales commission percentage received 
		/// by the <see cref="SalesPersonID">salesperson</see> for the 
		/// specified <see cref="BAccountID">customer</see> and 
		/// <see cref="LocationID">location</see>.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(typeof(SalesPerson.commnPct))]
		[PXUIField(DisplayName = "Commission %")]
		public virtual Decimal? CommisionPct
		{
			get
			{
				return this._CommisionPct;
			}
			set
			{
				this._CommisionPct = value;
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
	}

    [PXProjection(typeof(Select5<CustSalesPeople,
        InnerJoin<BAccount, On<BAccount.bAccountID, Equal<CustSalesPeople.bAccountID>>,
        InnerJoin<Location, On<Location.bAccountID, Equal<CustSalesPeople.bAccountID>>,
        LeftJoin<CustSalesPeople2, On<CustSalesPeople2.bAccountID, Equal<Location.bAccountID>, And<CustSalesPeople2.locationID, Equal<Location.locationID>, And<CustSalesPeople2.isDefault, Equal<True>>>>>>>,
        Where<CustSalesPeople.locationID, Equal<BAccount.defLocationID>, And<CustSalesPeople2.salesPersonID, IsNull, 
            Or<CustSalesPeople2.salesPersonID, Equal<CustSalesPeople.salesPersonID>>>>,
        Aggregate<
            GroupBy<CustSalesPeople.salesPersonID, 
            GroupBy<CustSalesPeople.bAccountID, 
            GroupBy<CustSalesPeople.isDefault, 
            GroupBy<Location.locationID>>>>>>))]
    [Serializable]
    [PXHidden]
    public partial class CustDefSalesPeople : IBqlTable
    {
        #region SalesPersonID
        public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
        protected Int32? _SalesPersonID;
        [PXDBInt(BqlField = typeof(CustSalesPeople.salesPersonID), IsKey = true)]
        public virtual Int32? SalesPersonID
        {
            get
            {
                return this._SalesPersonID;
            }
            set
            {
                this._SalesPersonID = value;
            }
        }
        #endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        protected Int32? _BAccountID;
        [PXDBInt(BqlField = typeof(CustSalesPeople.bAccountID), IsKey = true)]
        public virtual Int32? BAccountID
        {
            get
            {
                return this._BAccountID;
            }
            set
            {
                this._BAccountID = value;
            }
        }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        protected Int32? _LocationID;
        [PXDBInt(BqlField = typeof(Location.locationID), IsKey = true)]
        public virtual Int32? LocationID
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
        #region IsDefault
        public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
        protected Boolean? _IsDefault;
        [PXDBBool(BqlField = typeof(CustSalesPeople.isDefault))]
        public virtual Boolean? IsDefault
        {
            get
            {
                return this._IsDefault;
            }
            set
            {
                this._IsDefault = value;
            }
        }
        #endregion
		[PXHidden]
        [Serializable]
        public class BAccount : IBqlTable
        {
            #region BAccountID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            protected Int32? _BAccountID;
            [PXDBInt(IsKey = true)]
            public virtual Int32? BAccountID
            {
                get
                {
                    return this._BAccountID;
                }
                set
                {
                    this._BAccountID = value;
                }
            }
            #endregion
            #region DefLocationID
            public abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
            protected Int32? _DefLocationID;
            [PXDBInt()]
            public virtual Int32? DefLocationID
            {
                get
                {
                    return this._DefLocationID;
                }
                set
                {
                    this._DefLocationID = value;
                }
            }
            #endregion
        }
		[PXHidden]
        [Serializable]
        public class Location : IBqlTable
        {
            #region BAccountID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            protected Int32? _BAccountID;
            [PXDBInt(IsKey = true)]
            public virtual Int32? BAccountID
            {
                get
                {
                    return this._BAccountID;
                }
                set
                {
                    this._BAccountID = value;
                }
            }
            #endregion
            #region LocationID
            public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
            protected Int32? _LocationID;
            [PXDBInt(IsKey = true)]
            public virtual Int32? LocationID
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
        }
		[PXHidden]
        [Serializable]
        public class CustSalesPeople2 : CustSalesPeople
        {
            #region SalesPersonID
            public new abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
            #endregion
            #region BAccountID
            public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            #endregion
            #region LocationID
            public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
            #endregion
			#region isDefault
			public new abstract class isDefault : PX.Data.IBqlField
			{
			}
			#endregion
        }
    }
}
