using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[Serializable]
	[PXPrimaryGraph(typeof(INAvailabilitySchemeMaint))]
	[PXCacheName(Messages.INAvailabilityScheme, PXDacType.Catalogue, CacheGlobal = true)]
	public class INAvailabilityScheme : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INAvailabilityScheme>.By<availabilitySchemeID>
		{
			public static INAvailabilityScheme Find(PXGraph graph, string availabilitySchemeID) => FindBy(graph, availabilitySchemeID);
		}
		#endregion
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXDefault]
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Availability Calculation Rule", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(INAvailabilityScheme.availabilitySchemeID))]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Description { get; set; }
        #endregion

        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Deduct Qty. on Service Orders", FieldClass = "SERVICEMANAGEMENT")]
        public virtual bool? InclQtyFSSrvOrdBooked { get; set; }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Deduct Qty. Allocated for Service Orders", FieldClass = "SERVICEMANAGEMENT")]
        public virtual bool? InclQtyFSSrvOrdAllocated { get; set; }
        #endregion
        #region InclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Deduct Qty. on Service Orders Prepared", FieldClass = "SERVICEMANAGEMENT")]
        public virtual bool? InclQtyFSSrvOrdPrepared { get; set; }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Qty. on Sales Returns")]
		public virtual bool? InclQtySOReverse { get; set; }
		#endregion
		#region InclQtySOBackOrdered
		public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Deduct Qty. on Back Orders")]
		public virtual bool? InclQtySOBackOrdered { get; set; }
		#endregion
		#region InclQtySOPrepared
		public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Deduct Qty. on Sales Prepared")]
		public virtual bool? InclQtySOPrepared { get; set; }
		#endregion
		#region InclQtySOBooked
		public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Deduct Qty. on Sales Orders")]
		public virtual bool? InclQtySOBooked { get; set; }
		#endregion
		#region InclQtySOShipped
		public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Deduct Qty. Shipped")]
		public virtual bool? InclQtySOShipped { get; set; }
		#endregion
		#region InclQtySOShipping
		public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Deduct Qty. Allocated")]
		public virtual bool? InclQtySOShipping { get; set; }
		#endregion
		#region InclQtyInTransit
		public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Qty. in Transit")]
		public virtual bool? InclQtyInTransit { get; set; }
		#endregion
		#region InclQtyPOReceipts
		public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Qty. on PO Receipts")]
		public virtual bool? InclQtyPOReceipts { get; set; }
		#endregion
		#region InclQtyPOPrepared
		public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Qty. on Purchase Prepared")]
		public virtual bool? InclQtyPOPrepared { get; set; }
		#endregion
		#region InclQtyPOOrders
		public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Qty. on Purchase Orders")]
		public virtual bool? InclQtyPOOrders { get; set; }
		#endregion
		#region InclQtyINIssues
		public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Deduct Qty. on Issues")]
		public virtual bool? InclQtyINIssues { get; set; }
		#endregion
		#region InclQtyINReceipts
		public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Qty. on Receipts")]
		public virtual bool? InclQtyINReceipts { get; set; }
		#endregion
		#region InclQtyINAssemblyDemand
		public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Deduct Qty. of Kit Assembly Demand")]
		public virtual bool? InclQtyINAssemblyDemand { get; set; }
		#endregion
		#region InclQtyINAssemblySupply
		public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Qty. of Kit Assembly Supply")]
		public virtual bool? InclQtyINAssemblySupply { get; set; }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected Boolean? _InclQtyProductionSupplyPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>true</c>) that the Product Supply Prepared quantity is added to the total item availability.  
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include Qty. of Production Supply Prepared")]
        public virtual Boolean? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected Boolean? _InclQtyProductionSupply;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>true</c>) that the Product Supply quantity is added to the total item availability.  
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include Qty. of Production Supply")]
        public virtual Boolean? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected Boolean? _InclQtyProductionDemandPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>true</c>) that the Production Demand Prepared quantity is deducted from the total item availability.  
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Deduct Qty. on Production Demand Prepared")]
        public virtual Boolean? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected Boolean? _InclQtyProductionDemand;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>true</c>) that the Production Demand quantity is deducted from the total item availability.  
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Deduct Qty. on Production Demand")]
        public virtual Boolean? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected Boolean? _InclQtyProductionAllocated;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>true</c>) that the Production Allocated quantity is deducted from the total item availability.  
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Deduct Qty. on Production Allocated")]
        public virtual Boolean? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}
