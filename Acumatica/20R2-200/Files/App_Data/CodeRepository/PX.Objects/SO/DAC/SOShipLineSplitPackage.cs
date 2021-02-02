using PX.Data;
using System;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOShipLineSplitPackage, PXDacType.Details)]
	public class SOShipLineSplitPackage : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOShipLineSplitPackage>.By<shipmentNbr, shipmentLineNbr, shipmentSplitLineNbr, packageLineNbr>
		{
			public static SOShipLineSplitPackage Find(PXGraph graph, string shipmentNbr, int? shipmentLineNbr, int? shipmentSplitLineNbr, int? packageLineNbr) => FindBy(graph, shipmentNbr, shipmentLineNbr, shipmentSplitLineNbr, packageLineNbr);
		}

		public static class FK
		{
			public class SOPackageDetail : PX.Objects.SO.SOPackageDetail.PK.ForeignKeyOf<SOShipLineSplitPackage>.By<shipmentNbr, packageLineNbr> { }
			public class SOShipLineSplit : PX.Objects.SO.SOShipLineSplit.PK.ForeignKeyOf<SOShipLineSplitPackage>.By<shipmentNbr, shipmentLineNbr, shipmentSplitLineNbr> { }
		}
		#endregion

		#region RecordID
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID { get; set; }
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		#endregion
		#region ShipmentNbr
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXDBDefault(typeof(SOShipment.shipmentNbr))]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		#endregion
		#region ShipmentLineNbr
		[PXDBInt]
		[PXFormula(typeof(Selector<shipmentSplitLineNbr, SOShipLineSplit.lineNbr>))]
		public virtual Int32? ShipmentLineNbr { get; set; }
		public abstract class shipmentLineNbr : PX.Data.BQL.BqlInt.Field<shipmentLineNbr> { }
		#endregion
		#region ShipmentSplitLineNbr
		[PXDBInt]
		[PXUIField(DisplayName = "Shipment Split Line Nbr.")]
		[PXParent(typeof(FK.SOShipLineSplit))]
		[PXSelector(typeof(Search<SOShipLineSplitNbrVisible.splitLineNbr, Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOPackageDetail.shipmentNbr>>>>),
			new[] {
				typeof(SOShipLineSplit.lineNbr),
				typeof(SOShipLineSplit.splitLineNbr),
				typeof(SOShipLineSplit.origOrderType),
				typeof(SOShipLineSplit.origOrderNbr),
				typeof(SOShipLineSplit.inventoryID),
				typeof(SOShipLineSplit.lotSerialNbr),
				typeof(SOShipLineSplit.qty),
				typeof(SOShipLineSplit.packedQty),
				typeof(SOShipLineSplit.uOM) }, DirtyRead = true)]
		public virtual Int32? ShipmentSplitLineNbr { get; set; }
		public abstract class shipmentSplitLineNbr : PX.Data.BQL.BqlInt.Field<shipmentSplitLineNbr> { }
		#endregion
		#region PackageLineNbr
		[PXDBInt]
		[PXDBDefault(typeof(SOPackageDetail.lineNbr))]
		[PXParent(typeof(FK.SOPackageDetail))]
		public virtual Int32? PackageLineNbr { get; set; }
		public abstract class packageLineNbr : PX.Data.BQL.BqlInt.Field<packageLineNbr> { }
		#endregion
		#region InventoryID
		[Inventory(Enabled = false)]
		[PXFormula(typeof(Selector<shipmentSplitLineNbr, SOShipLineSplit.inventoryID>))]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		[SubItem(typeof(inventoryID), Enabled = false)]
		[PXFormula(typeof(Selector<shipmentSplitLineNbr, SOShipLineSplit.subItemID>))]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial", Enabled = false)]
		[PXFormula(typeof(Selector<shipmentSplitLineNbr, SOShipLineSplit.lotSerialNbr>))]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region UOM
		[INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
		[PXFormula(typeof(Selector<shipmentSplitLineNbr, SOShipLineSplit.uOM>))]
		public virtual String UOM { get; set; }
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		#endregion
		#region Qty
		[PXDBQuantity(typeof(uOM), typeof(basePackedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? PackedQty { get; set; }
		public abstract class packedQty : PX.Data.BQL.BqlDecimal.Field<packedQty> { }
		#endregion
		#region BaseQty
		[PXDBDecimal(6)]
		public virtual Decimal? BasePackedQty { get; set; }
		public abstract class basePackedQty : PX.Data.BQL.BqlDecimal.Field<basePackedQty> { }
		#endregion
		#region UnitPriceFactor
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnitPriceFactor { get; set; }
		public abstract class unitPriceFactor : PX.Data.BQL.BqlDecimal.Field<unitPriceFactor> { }
		#endregion
		#region WeightFactor
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? WeightFactor { get; set; }
		public abstract class weightFactor : PX.Data.BQL.BqlDecimal.Field<weightFactor> { }
		#endregion

		#region System Columns
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBTimestamp]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion

		/// <summary>
		/// An alias descendant version of <see cref="SOShipLineSplit"/>. Makes the SplitLineNbr field visible in selectors by default.
		/// </summary>
		[Serializable]
		[PXHidden]
		public class SOShipLineSplitNbrVisible : SOShipLineSplit
		{
			public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			public new abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Split Line Nbr.", Visible = true)]
			public override int? SplitLineNbr { get; set; }
		}
	}
}