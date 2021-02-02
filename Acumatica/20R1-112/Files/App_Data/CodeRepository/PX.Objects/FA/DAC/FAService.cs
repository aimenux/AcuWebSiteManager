 using System;
 using PX.Data;
 using PX.Objects.AP;
 using PX.Objects.CM;
 using PX.Objects.EP;

namespace PX.Objects.FA
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.FAService)]
	public partial class FAService : PX.Data.IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FAService.assetID>>>>))]
		[PXDBLiteDefault(typeof(FixedAsset.assetID))]
		public virtual Int32? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region ServiceNumber
		public abstract class serviceNumber : PX.Data.BQL.BqlString.Field<serviceNumber> { }
		protected String _ServiceNumber;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Service Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		public virtual String ServiceNumber
		{
			get
			{
				return this._ServiceNumber;
			}
			set
			{
				this._ServiceNumber = value;
			}
		}
		#endregion
		#region ServiceDate
		public abstract class serviceDate : PX.Data.BQL.BqlDateTime.Field<serviceDate> { }
		protected DateTime? _ServiceDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Service Date", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 2)]
		public virtual DateTime? ServiceDate
		{
			get
			{
				return this._ServiceDate;
			}
			set
			{
				this._ServiceDate = value;
			}
		}
		#endregion
		#region ScheduledDate
		public abstract class scheduledDate : PX.Data.BQL.BqlDateTime.Field<scheduledDate> { }
		protected DateTime? _ScheduledDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Scheduled Date", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 3)]
		public virtual DateTime? ScheduledDate
		{
			get
			{
				return this._ScheduledDate;
			}
			set
			{
				this._ScheduledDate = value;
			}
		}
		#endregion
		#region PerfomedBy
		public abstract class perfomedBy : PX.Data.BQL.BqlGuid.Field<perfomedBy> { }
		protected Guid? _PerfomedBy;
		[PXDBField()]
		[PXDefault()]
		[PXSelector(typeof(EPEmployee.userID), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
		[PXUIField(DisplayName = "Perfomed By", TabOrder = 4)]
		public virtual Guid? PerfomedBy
		{
			get
			{
				return this._PerfomedBy;
			}
			set
			{
				this._PerfomedBy = value;
			}
		}
		#endregion
		#region InspectedBy
		public abstract class inspectedBy : PX.Data.BQL.BqlGuid.Field<inspectedBy> { }
		protected Guid? _InspectedBy;
		[PXDBField()]
		[PXSelector(typeof(EPEmployee.userID), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
		[PXUIField(DisplayName = "Inspected By", TabOrder = 5)]
		public virtual Guid? InspectedBy
		{
			get
			{
				return this._InspectedBy;
			}
			set
			{
				this._InspectedBy = value;
			}
		}
		#endregion 
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", TabOrder = 6)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region ServiceAmount
		public abstract class serviceAmount : PX.Data.BQL.BqlDecimal.Field<serviceAmount> { }
		protected Decimal? _ServiceAmount;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Service Amount")]
		public virtual Decimal? ServiceAmount
		{
			get
			{
				return this._ServiceAmount;
			}
			set
			{
				this._ServiceAmount = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor()]
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
		#region BillNumber
		public abstract class billNumber : PX.Data.BQL.BqlString.Field<billNumber> { }
		protected String _BillNumber;
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Bill Number", Visibility = PXUIVisibility.SelectorVisible)]
		[APInvoiceType.RefNbr(typeof(Search2<AP.Standalone.APRegisterAlias.refNbr,
			InnerJoinSingleTable<APInvoice, On<APInvoice.docType, Equal<AP.Standalone.APRegisterAlias.docType>,
				And<APInvoice.refNbr, Equal<AP.Standalone.APRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Vendor, On<AP.Standalone.APRegisterAlias.vendorID, Equal<Vendor.bAccountID>>>>,
			Where<AP.Standalone.APRegisterAlias.docType, Equal<APInvoiceType.invoice>,
				And<AP.Standalone.APRegisterAlias.vendorID, Equal<Current<FAService.vendorID>>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>>, 
			OrderBy<Desc<AP.Standalone.APRegisterAlias.refNbr>>>), Filterable = true)]
		public virtual String BillNumber
		{
			get
			{
				return this._BillNumber;
			}
			set
			{
				this._BillNumber = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		protected Boolean? _Completed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Completed")]
		public virtual Boolean? Completed
		{
			get
			{
				return this._Completed;
			}
			set
			{
				this._Completed = value;
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
	}
}
