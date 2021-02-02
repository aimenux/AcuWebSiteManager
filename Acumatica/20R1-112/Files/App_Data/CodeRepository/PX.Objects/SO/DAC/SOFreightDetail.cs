namespace PX.Objects.SO
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.TX;
	using PX.Objects.CS;
	using PX.Objects.CR;
	using PX.Objects.IN;
	using PX.Objects.AR;
	using PX.Objects.GL;
	using PX.Objects.PM;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.SOFreightDetail)]
	public partial class SOFreightDetail : PX.Data.IBqlTable, IFreightBase
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOFreightDetail>.By<docType, refNbr, orderType, orderNbr, shipmentType, shipmentNbr>
		{
			public static SOFreightDetail Find(PXGraph graph, string docType, string refNbr, string orderType, string orderNbr, string shipmentType, string shipmentNbr)
				=> FindBy(graph, docType, refNbr, orderType, orderNbr, shipmentType, shipmentNbr);
		}
		public static class FK
		{
			public class Shipment : SOShipment.PK.ForeignKeyOf<SOFreightDetail>.By<shipmentNbr> { }
			public class OrderShipment : SOOrderShipment.PK.ForeignKeyOf<SOFreightDetail>.By<shipmentType, shipmentNbr, orderType, orderNbr> { }
			public class Invoice : SOInvoice.PK.ForeignKeyOf<SOFreightDetail>.By<docType, refNbr> { }
			public class ShipTerms : CS.ShipTerms.PK.ForeignKeyOf<SOFreightDetail>.By<shipTermsID> { }
			public class ShipZone : CS.ShippingZone.PK.ForeignKeyOf<SOFreightDetail>.By<shipZoneID> { }
		}
		#endregion

		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(ARInvoice.docType))]
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
		[PXDBDefault(typeof(ARInvoice.refNbr))]
		[PXParent(typeof(Select<ARInvoice, Where<ARInvoice.docType, Equal<Current<SOFreightDetail.docType>>, And<ARInvoice.refNbr, Equal<Current<SOFreightDetail.refNbr>>>>>))]
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
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
		[PXSelector(typeof(Search<SOOrderType.orderType>), CacheGlobal = true)]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<SOFreightDetail.orderType>>>>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region ShipmentType
		public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
		protected String _ShipmentType;
		[PXDefault()]
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXUIField(DisplayName = "Shipment Type", Enabled = false)]
		[SOShipmentType.List]
		public virtual String ShipmentType
		{
			get
			{
				return this._ShipmentType;
			}
			set
			{
				this._ShipmentType = value;
			}
		}
		#endregion
		#region ShipmentNbr
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		protected String _ShipmentNbr;
		[PXDefault]
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Shipment Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<Navigate.SOOrderShipment.shipmentNbr,
			Where<Navigate.SOOrderShipment.orderType, Equal<Current<SOFreightDetail.orderType>>,
			And<Navigate.SOOrderShipment.orderNbr, Equal<Current<SOFreightDetail.orderNbr>>,
			And<Navigate.SOOrderShipment.shipmentType, Equal<Current<SOFreightDetail.shipmentType>>>>>>))]
		public virtual String ShipmentNbr
		{
			get
			{
				return this._ShipmentNbr;
			}
			set
			{
				this._ShipmentNbr = value;
			}
		}
		#endregion
		#region ShipTermsID
		public abstract class shipTermsID : PX.Data.BQL.BqlString.Field<shipTermsID> { }
		protected String _ShipTermsID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Terms", Enabled=false)]
		[PXSelector(typeof(ShipTerms.shipTermsID), CacheGlobal = true)]
		public virtual String ShipTermsID
		{
			get
			{
				return this._ShipTermsID;
			}
			set
			{
				this._ShipTermsID = value;
			}
		}
		#endregion
		#region ShipZoneID
		public abstract class shipZoneID : PX.Data.BQL.BqlString.Field<shipZoneID> { }
		protected String _ShipZoneID;
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Zone ID", Enabled=false)]
		[PXSelector(typeof(ShippingZone.zoneID), DescriptionField = typeof(ShippingZone.description), CacheGlobal = true)]
		public virtual String ShipZoneID
		{
			get
			{
				return this._ShipZoneID;
			}
			set
			{
				this._ShipZoneID = value;
			}
		}
		#endregion
		#region ShipVia
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		protected String _ShipVia;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via", Enabled=false)]
		[PXSelector(typeof(Search<Carrier.carrierID>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
		public virtual String ShipVia
		{
			get
			{
				return this._ShipVia;
			}
			set
			{
				this._ShipVia = value;
			}
		}
		#endregion
		#region Weight
		public abstract class weight : PX.Data.BQL.BqlDecimal.Field<weight> { }
		protected Decimal? _Weight;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Weight", Enabled = false)]
		public virtual Decimal? Weight
		{
			get
			{
				return this._Weight;
			}
			set
			{
				this._Weight = value;
			}
		}
		#endregion
		#region Volume
		public abstract class volume : PX.Data.BQL.BqlDecimal.Field<volume> { }
		protected Decimal? _Volume;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Volume", Enabled = false)]
		public virtual Decimal? Volume
		{
			get
			{
				return this._Volume;
			}
			set
			{
				this._Volume = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(ARInvoice.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;
		[PXDBCurrency(typeof(SOFreightDetail.curyInfoID), typeof(SOFreightDetail.lineTotal))]
		[PXUIField(DisplayName = "Line Total", Enabled=false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return this._CuryLineTotal;
			}
			set
			{
				this._CuryLineTotal = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;
		[PXDBDecimal(4)]
		public virtual Decimal? LineTotal
		{
			get
			{
				return this._LineTotal;
			}
			set
			{
				this._LineTotal = value;
			}
		}
		#endregion
		#region CuryFreightCost
		public abstract class curyFreightCost : PX.Data.BQL.BqlDecimal.Field<curyFreightCost> { }
		protected Decimal? _CuryFreightCost;
		[PXDBCurrency(typeof(SOFreightDetail.curyInfoID), typeof(SOFreightDetail.freightCost))]
		[PXFormula(null, typeof(SumCalc<ARInvoice.curyFreightCost>))]
		[PXUIField(DisplayName = "Freight Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFreightCost
		{
			get
			{
				return this._CuryFreightCost;
			}
			set
			{
				this._CuryFreightCost = value;
			}
		}
		#endregion
		#region FreightCost
		public abstract class freightCost : PX.Data.BQL.BqlDecimal.Field<freightCost> { }
		protected Decimal? _FreightCost;
		[PXDBDecimal(4)]
		public virtual Decimal? FreightCost
		{
			get
			{
				return this._FreightCost;
			}
			set
			{
				this._FreightCost = value;
			}
		}
		#endregion
		#region CuryFreightAmt
		public abstract class curyFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyFreightAmt> { }
		protected Decimal? _CuryFreightAmt;
		[PXDBCurrency(typeof(SOFreightDetail.curyInfoID), typeof(SOFreightDetail.freightAmt))]
		[PXFormula(null, typeof(SumCalc<ARInvoice.curyFreightAmt>))]
		[PXUIField(DisplayName = "Freight Price")]
		[PXUIVerify(typeof(Where<SOFreightDetail.curyFreightAmt, GreaterEqual<decimal0>>), PXErrorLevel.Error, CS.Messages.Entry_GE, typeof(decimal0))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFreightAmt
		{
			get
			{
				return this._CuryFreightAmt;
			}
			set
			{
				this._CuryFreightAmt = value;
			}
		}
		#endregion
		#region FreightAmt
		public abstract class freightAmt : PX.Data.BQL.BqlDecimal.Field<freightAmt> { }
		protected Decimal? _FreightAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? FreightAmt
		{
			get
			{
				return this._FreightAmt;
			}
			set
			{
				this._FreightAmt = value;
			}
		}
		#endregion
		#region CuryPremiumFreightAmt
		public abstract class curyPremiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyPremiumFreightAmt> { }
		protected Decimal? _CuryPremiumFreightAmt;
		[PXDBCurrency(typeof(SOFreightDetail.curyInfoID), typeof(SOFreightDetail.premiumFreightAmt))]
		[PXFormula(null, typeof(SumCalc<ARInvoice.curyPremiumFreightAmt>))]
		[PXUIField(DisplayName = "Premium Freight Price")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryPremiumFreightAmt
		{
			get
			{
				return this._CuryPremiumFreightAmt;
			}
			set
			{
				this._CuryPremiumFreightAmt = value;
			}
		}
		#endregion
		#region PremiumFreightAmt
		public abstract class premiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<premiumFreightAmt> { }
		protected Decimal? _PremiumFreightAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? PremiumFreightAmt
		{
			get
			{
				return this._PremiumFreightAmt;
			}
			set
			{
				this._PremiumFreightAmt = value;
			}
		}
		#endregion
		#region CuryTotalFreightAmt
		public abstract class curyTotalFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyTotalFreightAmt> { }
		protected Decimal? _CuryTotalFreightAmt;
		[PXDBCurrency(typeof(SOFreightDetail.curyInfoID), typeof(SOFreightDetail.totalFreightAmt))]
		[PXFormula(typeof(Add<SOFreightDetail.curyPremiumFreightAmt, SOFreightDetail.curyFreightAmt>))]
		[PXUIField(DisplayName = "Total Freight Price", Enabled=false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTotalFreightAmt
		{
			get
			{
				return this._CuryTotalFreightAmt;
			}
			set
			{
				this._CuryTotalFreightAmt = value;
			}
		}
		#endregion
		#region TotalFreightAmt
		public abstract class totalFreightAmt : PX.Data.BQL.BqlDecimal.Field<totalFreightAmt> { }
		protected Decimal? _TotalFreightAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? TotalFreightAmt
		{
			get
			{
				return this._TotalFreightAmt;
			}
			set
			{
				this._TotalFreightAmt = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(DisplayName = "Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(typeof(SOFreightDetail.accountID), DisplayName = "Sub.", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt()]
		[PXDefault(typeof(ARInvoice.projectID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDefault(typeof(Search<PMAccountTask.taskID, Where<PMAccountTask.projectID, Equal<Current<SOFreightDetail.projectID>>, And<PMAccountTask.accountID, Equal<Current<SOFreightDetail.accountID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SOFreightDetailTask(typeof(SOFreightDetail.projectID), typeof(SOFreightDetail.curyTotalFreightAmt), DisplayName = "Project Task")]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
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
		#endregion

		#region IFreightBase Members


		public decimal? OrderWeight
		{
			get
			{
				return Weight;
			}
		}

		public decimal? PackageWeight
		{
			get
			{
				return Weight;
			}
		}

		public decimal? OrderVolume
		{
			get
			{
				return Volume;
			}
		}

		#endregion
	}
}
