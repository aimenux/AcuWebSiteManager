namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	using PX.Data.BQL;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.AP;
	using PX.Objects.GL;
	using PX.Objects.TX;
using System.Collections.Generic;
	using PX.Objects.IN;
using PX.Objects.CA;

	/// <summary>
	/// Represents a Carrier used by the company for shipping goods or a company's own
	/// shipping option.
	/// The records of this type are created and edited through the Ship Via Codes (CS.20.75.00) screen
	/// (Corresponds to the <see cref="CarrierMaint"/> graph).
	/// If the <see cref="FeaturesSet.CarrierIntegration"/> feature is enabled, the corresponding settings and plugins
	/// are defined through the Carriers (CS.20.77.00) screen
	/// (Corresponds to the <see cref="CarrierPluginMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] { typeof(CarrierMaint)},
		new Type[] { typeof(Select<Carrier, 
			Where<Carrier.carrierID, Equal<Current<Carrier.carrierID>>>>)
		})]
	[PXCacheName(SO.Messages.Carrier, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class Carrier : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<Carrier>.By<carrierID>
		{
			public static Carrier Find(PXGraph graph, string carrierID) => FindBy(graph, carrierID);
		}
		#endregion
		
		#region CarrierID
		public abstract class carrierID : PX.Data.BQL.BqlString.Field<carrierID> { }
		protected String _CarrierID;

        /// <summary>
        /// Key field.
        /// A unique code of a non-integrated carrier, a method of the integrated carrier or a shipping option of the company.
        /// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Ship Via", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Carrier.carrierID>), CacheGlobal = true)]
		public virtual String CarrierID
		{
			get
			{
				return this._CarrierID;
			}
			set
			{
				this._CarrierID = value;
			}
		}
		#endregion
		#region CalcMethod
		public abstract class calcMethod : PX.Data.BQL.BqlString.Field<calcMethod> { }
		protected String _CalcMethod;

        /// <summary>
        /// The method used to calculate freight charges using the rate breakdown specified in the related <see cref="FreightRate"/> records.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// <c>"P"</c> - Per Unit,
        /// <c>"N"</c> - Net (flat rates),
        /// <c>"M"</c> - Manual.
        /// Defaults to Manual (<c>"M"</c>).
        /// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CarrierCalcMethod.Manual)]
		[CarrierCalcMethod.List]
		[PXUIField(DisplayName = "Calculation Method")]
		public virtual String CalcMethod
		{
			get
			{
				return this._CalcMethod;
			}
			set
			{
				this._CalcMethod = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;

        /// <summary>
        /// The description of the carrier or shipping option.
        /// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
        protected String _TaxCategoryID;

        /// <summary>
        /// Identifier of the <see cref="TaxCategory">Tax Category</see> to be applied to the freight amount
        /// when goods are shipped with this shipping option.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="TaxCategory.TaxCategoryID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
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
		#region CalendarID
		public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }
		protected String _CalendarID;

        /// <summary>
        /// The <see cref="CSCalendar">Calendar</see> associated with the carrier, which reflects its work hours and the days when it ships the goods.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CSCalendar.CalendarID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar")]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual String CalendarID
		{
			get
			{
				return this._CalendarID;
			}
			set
			{
				this._CalendarID = value;
			}
		}
		#endregion
		#region FreightSalesAcctID
		public abstract class freightSalesAcctID : PX.Data.BQL.BqlInt.Field<freightSalesAcctID> { }
		protected Int32? _FreightSalesAcctID;

        /// <summary>
        /// Identifier of the General Ledger income <see cref="Account"/>, that is used to record the freight charges to be paid to the company.
        /// </summary>
		[Account(DisplayName = "Freight Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault()]
		[PXForeignReference(typeof(Field<Carrier.freightSalesAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? FreightSalesAcctID
		{
			get
			{
				return this._FreightSalesAcctID;
			}
			set
			{
				this._FreightSalesAcctID = value;
			}
		}
		#endregion
		#region FreightSalesSubID
		public abstract class freightSalesSubID : PX.Data.BQL.BqlInt.Field<freightSalesSubID> { }
        protected Int32? _FreightSalesSubID;

        /// <summary>
        /// Identifier of the General Ledger <see cref="Sub">Subaccount</see>, that is used to record the freight charges to be paid to the company.
        /// </summary>
		[SubAccount(typeof(Carrier.freightSalesAcctID), DisplayName = "Freight Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		[PXForeignReference(typeof(Field<Carrier.freightSalesSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? FreightSalesSubID
		{
			get
			{
				return this._FreightSalesSubID;
			}
			set
			{
				this._FreightSalesSubID = value;
			}
		}
		#endregion
		#region FreightExpenseAcctID
		public abstract class freightExpenseAcctID : PX.Data.BQL.BqlInt.Field<freightExpenseAcctID> { }
        protected Int32? _FreightExpenseAcctID;

        /// <summary>
        /// Identifier of the General Ledger expense <see cref="Account"/>, that is used to record the freight charges to be paid to the Carrier.
        /// </summary>
		[Account(DisplayName = "Freight Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault()]
		[PXForeignReference(typeof(Field<Carrier.freightExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? FreightExpenseAcctID
		{
			get
			{
				return this._FreightExpenseAcctID;
			}
			set
			{
				this._FreightExpenseAcctID = value;
			}
		}
		#endregion
		#region FreightExpenseSubID
		public abstract class freightExpenseSubID : PX.Data.BQL.BqlInt.Field<freightExpenseSubID> { }
        protected Int32? _FreightExpenseSubID;

        /// <summary>
        /// Identifier of the General Ledger <see cref="Sub">Subaccount</see>, that is used to record the freight charges to be paid to the Carrier.
        /// </summary>
		[SubAccount(typeof(Carrier.freightExpenseAcctID), DisplayName = "Freight Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		[PXForeignReference(typeof(Field<Carrier.freightExpenseSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? FreightExpenseSubID
		{
			get
			{
				return this._FreightExpenseSubID;
			}
			set
			{
				this._FreightExpenseSubID = value;
			}
		}
		#endregion
		#region BaseRate
		public abstract class baseRate : PX.Data.BQL.BqlDecimal.Field<baseRate> { }
		protected Decimal? _BaseRate;

        /// <summary>
        /// The flat-rate charge, which is added to the freight amount calculated according to the related <see cref="FreightRate"/> records.
        /// This field is relevan only if <see cref="IsExternal"/> is set to <c>false</c>.
        /// </summary>
        /// <value>
        /// Defaults to <c>0.0</c>.
        /// </value>
		[PXDBDecimal(typeof(Search<CommonSetup.decPlPrcCst>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Rate")]
		public virtual Decimal? BaseRate
		{
			get
			{
				return this._BaseRate;
			}
			set
			{
				this._BaseRate = value;
			}
		}
		#endregion
		#region IsExternal
		public abstract class isExternal : PX.Data.BQL.BqlBool.Field<isExternal> { }
		protected Boolean? _IsExternal;

        /// <summary>
        /// When set to <c>true</c>, indicates that the system must use a plugin, specified in the <see cref="CarrierPluginID"/> field,
        /// to provide integration with the carrier for this shipping option.
        /// This field is relevant only if the <see cref="FeaturesSet.CarrierIntegration"/> feature is enabled.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "External Plug-in")]
		public virtual Boolean? IsExternal
		{
			get
			{
				return this._IsExternal;
			}
			set
			{
				this._IsExternal = value;
			}
		}
		#endregion
		#region CarrierPluginID
		public abstract class carrierPluginID : PX.Data.BQL.BqlString.Field<carrierPluginID> { }
		protected String _CarrierPluginID;

        /// <summary>
        /// Identifier of the <see cref="CarrierPlugin"/> used to provide integration with the carrier for this shipping option.
        /// This field is relevant only if <see cref="IsExternal"/> is set to <c>true</c>.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CarrierPlugin.CarrierPluginID"/> field.
        /// </value>
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Carrier")]
		[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID, Where<CarrierPlugin.pluginTypeName, IsNotNull>>), CacheGlobal = true)]
		public virtual String CarrierPluginID
		{
			get
			{
				return this._CarrierPluginID;
			}
			set
			{
				this._CarrierPluginID = value;
			}
		}
		#endregion
		#region PluginMethod
		public abstract class pluginMethod : PX.Data.BQL.BqlString.Field<pluginMethod> { }
		protected String _PluginMethod;

        /// <summary>
        /// The code of the <see cref="CarrierPluginMethod">Service Method</see> of the <see cref="CarrierPluginID">Carrier Plugin</see>
        /// that is used to provide integration with the carrier for this shipping option.
        /// This field is relevant only if <see cref="IsExternal"/> is set to <c>true</c>.
        /// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Service Method")]
		[CarrierMethodSelector]
		public virtual String PluginMethod
		{
			get
			{
				return this._PluginMethod;
			}
			set
			{
				this._PluginMethod = value;
			}
		}
		#endregion

		#region ConfirmationRequired
		public abstract class confirmationRequired : PX.Data.BQL.BqlBool.Field<confirmationRequired> { }
		protected Boolean? _ConfirmationRequired;

        /// <summary>
        /// When set to <c>true</c>, indicates that to confirm the <see cref="SO.SOShipment">shipment</see> the system will require
        /// confirmation for each <see cref="SO.SOPackageDetail">package</see> used for shipment with this shipment option.
        /// This field is relevant only if <see cref="IsExternal"/> is set to <c>true</c>.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Confirmation for Each Box Is Required", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? ConfirmationRequired
		{
			get
			{
				return this._ConfirmationRequired;
			}
			set
			{
				this._ConfirmationRequired = value;
			}
		}
		#endregion
		#region PackageRequired
		public abstract class packageRequired : PX.Data.BQL.BqlBool.Field<packageRequired> { }
        protected Boolean? _PackageRequired;

        /// <summary>
        /// When set to <c>true</c>, indicates that at least one <see cref="SO.SOPackageDetail">package</see> must be specified
        /// to create (and confirm) a <see cref="SO.SOShipment">shipment</see> with this shipment option.
        /// This field is relevant only if <see cref="IsExternal"/> is set to <c>true</c>.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "At least one Package Is Required", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? PackageRequired
		{
			get
			{
				return this._PackageRequired;
			}
			set
			{
				this._PackageRequired = value;
			}
		}
		#endregion
        #region IsCommonCarrier
        public abstract class isCommonCarrier : PX.Data.BQL.BqlBool.Field<isCommonCarrier> { }
        protected Boolean? _IsCommonCarrier;

        /// <summary>
        /// Indicates whether the carrier is a common carrier.
        /// Because common carriers deliver goods from a company branch to the customer location that is
        /// a selling point, the value of this field affects the set of taxes that applies to the corresponding invoice.
        /// </summary>
        /// <value>
        /// When set to <c>true</c>, the carrier is considered common and hence the taxes
        /// from the <see cref="AR.Customer.TaxZoneID">customer's tax zone</see> are applied.
        /// Otherwise, the system applies the taxes from the <see cref="TX.TaxZone">taz zone</see> associated with the selling branch.
        /// </value>
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Common Carrier", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Boolean? IsCommonCarrier
        {
            get
            {
                return this._IsCommonCarrier;
            }
            set
            {
                this._IsCommonCarrier = value;
            }
        }
        #endregion
		#region ReturnLabel
		public abstract class returnLabel : PX.Data.BQL.BqlBool.Field<returnLabel> { }
		protected Boolean? _ReturnLabel;

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Generate Return Label Automatically")]
		public virtual bool? ReturnLabel
		{
			get
			{
				return this._ReturnLabel;
			}
			set
			{
				this._ReturnLabel = value;
			}
		}
		#endregion
		#region ValidatePackedQty
		public abstract class validatePackedQty : PX.Data.BQL.BqlBool.Field<validatePackedQty> { }
		protected Boolean? _ValidatePackedQty;

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Validate Packed Quantities on Shipment Confirmation")]
		public virtual bool? ValidatePackedQty
		{
			get
			{
				return this._ValidatePackedQty;
			}
			set
			{
				this._ValidatePackedQty = value;
			}
		}
		#endregion
		#region IsExternalShippingApplication
		public abstract class isExternalShippingApplication : PX.Data.BQL.BqlBool.Field<isExternalShippingApplication> { }
		protected Boolean? _IsExternalShippingApplication;

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use External Shipping Application")]
		public virtual bool? IsExternalShippingApplication
		{
			get
			{
				return this._IsExternalShippingApplication;
			}
			set
			{
				this._IsExternalShippingApplication = value;
			}
		}
		#endregion
		#region ShippingApplicationType
		public abstract class shippingApplicationType : PX.Data.BQL.BqlString.Field<shippingApplicationType> { }
		protected string _ShippingApplicationType;

		[PXDBString(IsFixed = true, IsUnicode = true)]
		[ShippingApplicationTypes.List()]
		[PXUIField(DisplayName = "Shipping Application")]
		public virtual string ShippingApplicationType
		{
			get
			{
				return this._ShippingApplicationType;
			}
			set
			{
				this._ShippingApplicationType = value;
			}
		}
		#endregion
		#region CalculateFreightOnReturn
		public abstract class calcFreightOnReturn : PX.Data.BQL.BqlBool.Field<calcFreightOnReturn> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Calculate Freight on Returns")]
		public virtual bool? CalcFreightOnReturn
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field.
        /// </value>
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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

	public static class CarrierUnitsType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SI, US },
				new string[] { Messages.SIUnits, Messages.USUnits }) { ; }
		}
		public const string SI = "S";
		public const string US = "U";

		public sealed class si : BqlString.Constant<si>
		{
			public si() : base(SI) { }
		}
		public sealed class us : BqlString.Constant<us>
		{
			public us() : base(US) { }
		}
	}

	public static class ShippingApplicationTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { UPS, FEDEX },
				new string[] { Messages.ShippingApplicationTypeUPS, Messages.ShippingApplicationTypeFEDEX })
			{ }
		}
		public const string UPS = "UPS";
		public const string FEDEX = "FED";
	}

	public static class CarrierCalcMethod
	{
		public const string PerUnit = "P";
		public const string Net = "N";
		public const string Manual = "M";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { PerUnit, Net, Manual },
				new string[] { Messages.PerUnit, Messages.Net, Messages.Manual })
			{; }
		}
	}
}
