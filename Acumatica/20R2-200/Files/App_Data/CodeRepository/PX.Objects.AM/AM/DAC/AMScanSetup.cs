using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.AM
{
	[PXCacheName(Messages.AMScanSetup, PXDacType.Config)]
	[Serializable]
	public class AMScanSetup : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<AMScanSetup>.By<branchID>
		{
			public static AMScanSetup Find(PXGraph graph, int? branchID) => FindBy(graph, branchID);
		}
		#endregion

		#region BranchID
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(Search<PX.Objects.GL.Branch.branchID, Where<PX.Objects.GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Branch")]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion

		#region ExplicitLineConfirmation
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Explicit Line Confirmation")]
		public virtual bool? ExplicitLineConfirmation { get; set; }
		public abstract class explicitLineConfirmation : PX.Data.BQL.BqlBool.Field<explicitLineConfirmation> { }
        #endregion
        #region UseDefaultQtyInMove
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Use Default Quantity in Move/Labor")]
        public virtual bool? UseDefaultQtyInMove { get; set; }
        public abstract class useDefaultQtyInMove : PX.Data.BQL.BqlBool.Field<useDefaultQtyInMove> { }
        #endregion
        #region UseDefaultQtyInMaterials
        [PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Default Quantity in Materials")]
		public virtual bool? UseDefaultQtyInMaterials { get; set; }
		public abstract class useDefaultQtyInMaterials : PX.Data.BQL.BqlBool.Field<useDefaultQtyInMaterials> { }
		#endregion
        #region UseRemainingQtyInMove
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Use Remaining Quantity in Move")]
        public virtual bool? UseRemainingQtyInMove { get; set; }
        public abstract class useRemainingQtyInMove : PX.Data.BQL.BqlBool.Field<useRemainingQtyInMove> { }
        #endregion
        #region UseRemainingQtyInMaterials
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Use Remaining Quantity in Materials")]
        public virtual bool? UseRemainingQtyInMaterials { get; set; }
        public abstract class useRemainingQtyInMaterials : PX.Data.BQL.BqlBool.Field<useRemainingQtyInMaterials> { }
        #endregion
		#region UseDefaultOrderType
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Default Order Type")]
		public virtual bool? UseDefaultOrderType { get; set; }
		public abstract class useDefaultOrderType : PX.Data.BQL.BqlBool.Field<useDefaultOrderType> { }
        #endregion
        #region RequestLocationForEachItemInMove
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Request Location for Each Item in Move/Labor")]
        public virtual bool? RequestLocationForEachItemInMove { get; set; }
        public abstract class requestLocationForEachItemInMove : PX.Data.BQL.BqlBool.Field<requestLocationForEachItemInMove> { }
        #endregion
        #region RequestLocationForEachItemInMaterials
        [PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request Location for Each Item in Materials")]
		public virtual bool? RequestLocationForEachItemInMaterials { get; set; }
		public abstract class requestLocationForEachItemInMaterials : PX.Data.BQL.BqlBool.Field<requestLocationForEachItemInMaterials> { }
		#endregion

		#region DefaultWarehouse
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Default Warehouse from User Profile")]
		public virtual bool? DefaultWarehouse { get; set; }
		public abstract class defaultWarehouse : PX.Data.BQL.BqlBool.Field<defaultWarehouse> { }
		#endregion
		#region DefaultLotSerialNumber
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Default Auto-Generated Lot/Serial Nbr.")]
		public virtual bool? DefaultLotSerialNumber { get; set; }
		public abstract class defaultLotSerialNumber : PX.Data.BQL.BqlBool.Field<defaultLotSerialNumber> { }
		#endregion
		#region DefaultExpireDate
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Default Expiration Date")]
		public virtual bool? DefaultExpireDate { get; set; }
		public abstract class defaultExpireDate : PX.Data.BQL.BqlBool.Field<defaultExpireDate> { }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}
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
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID>
		{
		}
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
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID>
		{
		}
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
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime>
		{
		}
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
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
		{
		}
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
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
		{
		}
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
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
		{
		}
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
}