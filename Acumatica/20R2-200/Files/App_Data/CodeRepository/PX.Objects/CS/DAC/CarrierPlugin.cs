using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.CarrierService;
using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CA;
using PX.Objects.IN;
using PX.SM;

namespace PX.Objects.CS
{
    [Serializable]
	[PXCacheName(Messages.CarrierPlugin, PXDacType.Catalogue)]
	public partial class CarrierPlugin : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CarrierPlugin>.By<carrierPluginID>
		{
			public static CarrierPlugin Find(PXGraph graph, string carrierPluginID) => FindBy(graph, carrierPluginID);
		}
		#endregion
		#region CarrierPluginID
		public abstract class carrierPluginID : PX.Data.BQL.BqlString.Field<carrierPluginID> { }
		protected String _CarrierPluginID;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Carrier ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID>), CacheGlobal = true)]
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
		#region DetailLineCntr
		[PXDBInt()]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? DetailLineCntr { get; set; }

		public abstract class detailLineCntr : PX.Data.BQL.BqlInt.Field<detailLineCntr> { }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
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
		#region PluginTypeName
		public abstract class pluginTypeName : PX.Data.BQL.BqlString.Field<pluginTypeName> { }
		protected String _PluginTypeName;
		[PXProviderTypeSelectorAttribute(typeof(ICarrierService))]
		[PXDBString(255)]
		[PXDefault]
		[PXUIField(DisplayName = "Plug-In (Type)")]
		public virtual String PluginTypeName
		{
			get
			{
				return this._PluginTypeName;
			}
			set
			{
				this._PluginTypeName = value;
			}
		}
		#endregion
		#region UnitType
		public abstract class unitType : PX.Data.BQL.BqlString.Field<unitType> { }
		protected string _UnitType;

		[PXDBString(1, IsFixed = true)]
		[CarrierUnitsType.List()]
		[PXDefault(CarrierUnitsType.SI)]
		[PXUIField(DisplayName = "Carrier Units")]
		public virtual string UnitType
		{
			get
			{
				return this._UnitType;
			}
			set
			{
				this._UnitType = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected string _UOM;
		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
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
		#region LinearUOM
		public abstract class linearUOM : PX.Data.BQL.BqlString.Field<linearUOM> { }
		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		public virtual String LinearUOM
		{
			get;
			set;
		}
		#endregion

		#region ReturnLabelNotification

		public abstract class returnLabelNotification : PX.Data.BQL.BqlInt.Field<returnLabelNotification> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Notification Template for Return Label")]
		[PXSelector(typeof(Search<Notification.notificationID>),
						DescriptionField = typeof(Notification.name))]
		public virtual int? ReturnLabelNotification { get; set; }

		#endregion
		#region SiteID

		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID>
		{
		}
		[Site]
		public virtual int? SiteID
		{
			get;
			set;
		}

		#endregion

		#region KilogramUOM
		public abstract class kilogramUOM : BqlString.Field<kilogramUOM> { }
		[PXUnboundDefault(typeof(Switch<Case<Where<unitType, Equal<CarrierUnitsType.si>>, uOM>, Null>))]
		[CarrierUnboundUnit(typeof(uOM), CarrierUnitsType.SI, DisplayName = "Kilogram")]
		public virtual string KilogramUOM { get; set; }
		#endregion
		#region PoundUOM
		public abstract class poundUOM : BqlString.Field<poundUOM> { }
		[PXUnboundDefault(typeof(Switch<Case<Where<unitType, Equal<CarrierUnitsType.us>>, uOM>, Null>))]
		[CarrierUnboundUnit(typeof(uOM), CarrierUnitsType.US, DisplayName = "Pound")]
		public virtual string PoundUOM { get; set; }
		#endregion
		#region CentimeterUOM
		public abstract class centimeterUOM : BqlString.Field<centimeterUOM> { }
		[PXUnboundDefault(typeof(Switch<Case<Where<unitType, Equal<CarrierUnitsType.si>>, linearUOM>, Null>))]
		[CarrierUnboundUnit(typeof(linearUOM), CarrierUnitsType.SI, DisplayName = "Centimeter")]
		public virtual string CentimeterUOM { get; set; }
		#endregion
		#region InchUOM
		public abstract class inchUOM : BqlString.Field<inchUOM> { }
		[PXUnboundDefault(typeof(Switch<Case<Where<unitType, Equal<CarrierUnitsType.us>>, linearUOM>, Null>))]
		[CarrierUnboundUnit(typeof(linearUOM), CarrierUnitsType.US, DisplayName = "Inch")]
		public virtual string InchUOM { get; set; }
		#endregion

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
}
