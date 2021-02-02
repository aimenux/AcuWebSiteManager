using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.SOPackageDetail)]
	public partial class SOPackageDetail : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOPackageDetail>.By<shipmentNbr, lineNbr>
		{
			public static SOPackageDetail Find(PXGraph graph, string shipmentNbr, int? lineNbr) => FindBy(graph, shipmentNbr, lineNbr);
		}
		public static class FK
		{
			public class CSBox : CS.CSBox.PK.ForeignKeyOf<SOPackageDetail>.By<boxID> { }
			public class Shipment : SOShipment.PK.ForeignKeyOf<SOPackageDetail>.By<shipmentNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SOPackageDetail>.By<inventoryID> { }
		}
		#endregion
		#region ShipmentNbr
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		protected String _ShipmentNbr;
		[PXParent(typeof(FK.Shipment))]
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDBDefault(typeof(SOShipment.shipmentNbr))]
		[PXUIField(DisplayName = "Shipment Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(SOShipment.packageLineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region BoxID
		public abstract class boxID : PX.Data.BQL.BqlString.Field<boxID> { }
		protected String _BoxID;
		[PXDBString(15, IsUnicode = true)]
		[PXDefault()]
		[PXSelector(typeof(Search2<CSBox.boxID, 
		    LeftJoin<CarrierPackage, On<CSBox.boxID, Equal<CarrierPackage.boxID>, And<Current<SOShipment.shipVia>, IsNotNull>>>,
 			Where<Current<SOShipment.shipVia>, IsNull, 
			Or<Where<CarrierPackage.carrierID, Equal<Current<SOShipment.shipVia>>, And<Current<SOShipment.shipVia>, IsNotNull>>>>>))]
		[PXUIField(DisplayName = "Box ID")]
		public virtual String BoxID
		{
			get
			{
				return this._BoxID;
			}
			set
			{
				this._BoxID = value;
			}
		}
		#endregion
		#region Weight
		public abstract class weight : PX.Data.BQL.BqlDecimal.Field<weight> { }
		protected Decimal? _Weight;
		/// <summary>
		/// Gross (Brutto) Weight. Weight of a box with contents. (includes weight of the box itself).
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Weight")]
		[PXFormula(null, typeof(SumCalc<SOShipment.packageWeight>))]
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
		#region WeightUOM
		public abstract class weightUOM : PX.Data.BQL.BqlString.Field<weightUOM> { }
		protected String _WeightUOM;
		[PXUIField(DisplayName = "UOM", Enabled = false)]
		[PXString()]
		public virtual String WeightUOM
		{
			get
			{
				return this._WeightUOM;
			}
			set
			{
				this._WeightUOM = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Visible=false)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
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
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Qty", Enabled = false)]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region QtyUOM
		public abstract class qtyUOM : PX.Data.BQL.BqlString.Field<qtyUOM> { }
		protected String _QtyUOM;
		[PXUIField(DisplayName = "Qty. UOM", Enabled = false)]
		[PXDBString()]
		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<SOPackageDetail.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String QtyUOM
		{
			get
			{
				return this._QtyUOM;
			}
			set
			{
				this._QtyUOM = value;
			}
		}
		#endregion
		#region TrackNumber
		public abstract class trackNumber : PX.Data.BQL.BqlString.Field<trackNumber> { }
		protected String _TrackNumber;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName="Tracking Number")]
		public virtual String TrackNumber
		{
			get
			{
				return this._TrackNumber;
			}
			set
			{
				this._TrackNumber = value;
			}
		}
		#endregion
		#region TrackData
		public abstract class trackData : PX.Data.BQL.BqlString.Field<trackData> { }
		protected String _TrackData;
		[PXDBString(4000)]
		public virtual String TrackData
		{
			get
			{
				return this._TrackData;
			}
			set
			{
				this._TrackData = value;
			}
		}
		#endregion
		#region DeclaredValue
		public abstract class declaredValue : PX.Data.BQL.BqlDecimal.Field<declaredValue> { }
		protected Decimal? _DeclaredValue;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Declared Value")]
		public virtual Decimal? DeclaredValue
		{
			get
			{
				return this._DeclaredValue;
			}
			set
			{
				this._DeclaredValue = value;
			}
		}
		#endregion
		#region COD
		public abstract class cOD : PX.Data.BQL.BqlDecimal.Field<cOD> { }
		protected Decimal? _COD;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "C.O.D. Amount")]
		public virtual Decimal? COD
		{
			get
			{
				return this._COD;
			}
			set
			{
				this._COD = value;
			}
		}
		#endregion
		#region Confirmed
		public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
		protected Boolean? _Confirmed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Confirmed", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Confirmed
		{
			get
			{
				return this._Confirmed;
			}
			set
			{
				this._Confirmed = value;
			}
		}
		#endregion
		#region CustomRefNbr1
		public abstract class customRefNbr1 : PX.Data.BQL.BqlString.Field<customRefNbr1> { }
		protected String _CustomRefNbr1;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Custom Ref. Nbr. 1")]
		public virtual String CustomRefNbr1
		{
			get
			{
				return this._CustomRefNbr1;
			}
			set
			{
				this._CustomRefNbr1 = value;
			}
		}
		#endregion
		#region CustomRefNbr2
		public abstract class customRefNbr2 : PX.Data.BQL.BqlString.Field<customRefNbr2> { }
		protected String _CustomRefNbr2;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Custom Ref. Nbr. 2")]
		public virtual String CustomRefNbr2
		{
			get
			{
				return this._CustomRefNbr2;
			}
			set
			{
				this._CustomRefNbr2 = value;
			}
		}
		#endregion
		#region PackageType
		public abstract class packageType : PX.Data.BQL.BqlString.Field<packageType> { }
		protected String _PackageType;
		[PXDefault(SOPackageType.Manual)]
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Enabled=false )]
		[SOPackageType.List]
		public virtual String PackageType
		{
			get
			{
				return this._PackageType;
			}
			set
			{
				this._PackageType = value;
			}
		}
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

	[PXProjection(typeof(Select2<SOPackageDetail, LeftJoin<CSBox, On<CSBox.boxID, Equal<SOPackageDetail.boxID>>>>), new Type[] { typeof(SOPackageDetail) })]
	public partial class SOPackageDetailEx : SOPackageDetail
	{
		public new abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		public new abstract class boxID : PX.Data.BQL.BqlString.Field<boxID> { }
		public new abstract class packageType : PX.Data.BQL.BqlString.Field<packageType> { }

		#region BoxDescription
		[PXDefault(typeof(Search<CSBox.description, Where<CSBox.boxID, Equal<Current<boxID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(255, IsUnicode = true, BqlField = typeof(CSBox.description))]
		[PXUIField(DisplayName = "Box Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String BoxDescription { get; set; }
		public abstract class boxDescription : PX.Data.BQL.BqlString.Field<boxDescription> { }
		#endregion
		#region CarrierBox
		[PXDefault(typeof(Search<CSBox.carrierBox, Where<CSBox.boxID, Equal<Current<boxID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(60, IsUnicode = true, BqlField = typeof(CSBox.carrierBox))]
		[PXUIField(DisplayName = "Carrier's Package", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String CarrierBox { get; set; }
		public abstract class carrierBox : PX.Data.BQL.BqlString.Field<carrierBox> { }
		#endregion
		#region Length
		[PXDefault(typeof(Search<CSBox.length, Where<CSBox.boxID, Equal<Current<boxID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt(MinValue = 0, BqlField = typeof(CSBox.length))]
		[PXUIField(DisplayName = "Length", Enabled = false)]
		public virtual int? Length { get; set; }
		public abstract class length : PX.Data.BQL.BqlInt.Field<length> { }
		#endregion
		#region Width
		[PXDefault(typeof(Search<CSBox.width, Where<CSBox.boxID, Equal<Current<boxID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt(MinValue = 0, BqlField = typeof(CSBox.width))]
		[PXUIField(DisplayName = "Width", Enabled = false)]
		public virtual int? Width { get; set; }
		public abstract class width : PX.Data.BQL.BqlInt.Field<width> { }
		#endregion
		#region Height
		[PXDefault(typeof(Search<CSBox.height, Where<CSBox.boxID, Equal<Current<boxID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt(MinValue = 0, BqlField = typeof(CSBox.height))]
		[PXUIField(DisplayName = "Height", Enabled = false)]
		public virtual int? Height { get; set; }
		public abstract class height : PX.Data.BQL.BqlInt.Field<height> { }
		#endregion
		#region BoxWeight
		[PXDefault(typeof(Search<CSBox.boxWeight, Where<CSBox.boxID, Equal<Current<boxID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4, MinValue = 0, BqlField = typeof(CSBox.boxWeight))]
		[PXUIField(DisplayName = "Box Weight", Enabled = false)]
		public virtual Decimal? BoxWeight { get; set; }
		public abstract class boxWeight : PX.Data.BQL.BqlDecimal.Field<boxWeight> { }
		#endregion
		#region MaxWeight
		[PXDefault(typeof(Search<CSBox.maxWeight, Where<CSBox.boxID, Equal<Current<boxID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4, BqlField = typeof(CSBox.maxWeight))]
		[PXUIField(DisplayName = "Max Weight", Enabled = false)]
		public virtual Decimal? MaxWeight { get; set; }
		public abstract class maxWeight : PX.Data.BQL.BqlDecimal.Field<maxWeight> { }
		#endregion

		#region NetWeight
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Net Weight", Enabled = false)]
		[PXFormula(typeof(Switch<Case<Where<weight, GreaterEqual<boxWeight>>, Sub<weight, boxWeight>>, decimal0>))]
		public virtual Decimal? NetWeight { get; set; }
		public abstract class netWeight : PX.Data.BQL.BqlDecimal.Field<netWeight> { }
		#endregion

		public virtual SOPackageInfoEx ToPackageInfo(int? siteID)
		{
			var info = new SOPackageInfoEx
			{
				BoxID = BoxID,
				Weight = NetWeight,
				GrossWeight = Weight,
				WeightUOM = WeightUOM,
				Qty = Qty,
				QtyUOM = QtyUOM,
				InventoryID = InventoryID,
				DeclaredValue = DeclaredValue,
				COD = COD > 0,
				SiteID = siteID,

				BoxWeight = BoxWeight,
				CarrierBox = CarrierBox,
				Description = BoxDescription,
				Height = Height,
				Length = Length,
				Width = Width,
				MaxWeight = MaxWeight
			};
			return info;
		}
	}

	public class SOPackageType
	{
		public const string Auto = "A";
		public const string Manual = "M";

		public class auto : PX.Data.BQL.BqlString.Constant<auto> { public auto() : base(Auto) { } }
		public class manual : PX.Data.BQL.BqlString.Constant<manual> { public manual() : base(Manual) { } }

		[PXLocalizable]
		public abstract class DisplayNames
		{
			public const string Auto = "Auto";
			public const string Manual = "Manual";
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				Pair(Auto, DisplayNames.Auto),
				Pair(Manual, DisplayNames.Manual))
			{ }
		}

		public class ForFiltering : SOPackageType
		{
			public const string Both = "B";
			public class both : PX.Data.BQL.BqlString.Constant<both> { public both() : base(Both) { } }

			public new class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					Pair(Both, DisplayNames.Both),
					Pair(Auto, DisplayNames.Auto),
					Pair(Manual, DisplayNames.Manual))
				{ }
			}

			[PXLocalizable]
			public new abstract class DisplayNames : SOPackageType.DisplayNames
			{
				public const string Both = "Auto and Manual";
			}
		}
	}
}