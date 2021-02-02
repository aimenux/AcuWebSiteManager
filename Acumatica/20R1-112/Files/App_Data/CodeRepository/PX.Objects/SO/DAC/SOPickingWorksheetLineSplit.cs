using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using System;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOPickingWorksheetLineSplit, PXDacType.Details)]
	public class SOPickingWorksheetLineSplit : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOPickingWorksheetLineSplit>.By<worksheetNbr, lineNbr, splitNbr>
		{
			public static SOPickingWorksheetLineSplit Find(PXGraph graph, string worksheetNbr, int? lineNbr, int? splitNbr)
				=> FindBy(graph, worksheetNbr, lineNbr, splitNbr);
		}

		public static class FK
		{
			public class Worksheet : SOPickingWorksheet.PK.ForeignKeyOf<SOPickingWorksheetLineSplit>.By<worksheetNbr> { }
			public class WorksheetLine : SOPickingWorksheetLine.PK.ForeignKeyOf<SOPickingWorksheetLineSplit>.By<worksheetNbr, lineNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<SOPickingWorksheetLineSplit>.By<siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<SOPickingWorksheetLineSplit>.By<locationID> { }
			public class SortingLocation : INLocation.PK.ForeignKeyOf<SOPickingWorksheetLineSplit>.By<sortingLocationID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SOPickingWorksheetLineSplit>.By<inventoryID> { }
		}
		#endregion

		#region WorksheetNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOPickingWorksheet.worksheetNbr))]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visible = false, Enabled = false)]
		[PXParent(typeof(FK.Worksheet))]
		public virtual String WorksheetNbr { get; set; }
		public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
		#endregion
		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(SOPickingWorksheetLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
		[PXParent(typeof(FK.WorksheetLine))]
		public virtual Int32? LineNbr { get; set; }
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion
		#region SplitNbr
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(SOPickingWorksheet))]
		[PXUIField(DisplayName = "Split Nbr.", Visible = false, Enabled = false)]
		public virtual Int32? SplitNbr { get; set; }
		public abstract class splitNbr : PX.Data.BQL.BqlInt.Field<splitNbr> { }
		#endregion

		#region SiteID
		[Site(Enabled = false)]
		[PXForeignReference(typeof(FK.Site))]
		[PXDefault(typeof(SOPickingWorksheetLine.siteID))]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), Enabled = false)]
		[PXForeignReference(typeof(FK.Location))]
		[PXDefault]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region InventoryID
		[Inventory(Enabled = false)]
		[PXForeignReference(typeof(FK.InventoryItem))]
		[PXDefault(typeof(SOPickingWorksheetLine.inventoryID))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		[SubItem(typeof(inventoryID), Enabled = false)]
		[PXDefault]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial", Enabled = false)]
		[PXDefault("")]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region ExpireDate
		[PXDBDate(InputMask = "d", DisplayMask = "d")]
		[PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial", Enabled = false)]
		public virtual DateTime? ExpireDate { get; set; }
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region UOM
		[INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
		[PXDefault]
		public virtual String UOM { get; set; }
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		#endregion
		#region Qty
		[PXDBQuantity(typeof(uOM), typeof(baseQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity", Enabled = false)]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region BaseQty
		[PXDBDecimal(6)]
		public virtual Decimal? BaseQty { get; set; }
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		#endregion
		#region PickedQty
		[PXDBQuantity(typeof(uOM), typeof(basePickedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Picked Quantity", Enabled = false)]
		public virtual Decimal? PickedQty { get; set; }
		public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
		#endregion
		#region BasePickedQty
		[PXDBDecimal(6)]
		public virtual Decimal? BasePickedQty { get; set; }
		public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
		#endregion
		#region PackedQty
		[PXDBQuantity(typeof(uOM), typeof(basePackedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Packed Quantity", Enabled = false)]
		public virtual Decimal? PackedQty { get; set; }
		public abstract class packedQty : PX.Data.BQL.BqlDecimal.Field<packedQty> { }
		#endregion
		#region BasePackedQty
		[PXDBDecimal(6)]
		public virtual Decimal? BasePackedQty { get; set; }
		public abstract class basePackedQty : PX.Data.BQL.BqlDecimal.Field<basePackedQty> { }
		#endregion
		#region IsUnassigned
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? IsUnassigned { get; set; }
		public abstract class isUnassigned : PX.Data.BQL.BqlBool.Field<isUnassigned> { }
		#endregion
		#region SortingLocationID
		[Location(typeof(siteID), DisplayName = "Sorting Location", Enabled = false)]
		[PXForeignReference(typeof(FK.SortingLocation))]
		public virtual Int32? SortingLocationID { get; set; }
		public abstract class sortingLocationID : PX.Data.BQL.BqlInt.Field<sortingLocationID> { }
		#endregion

		#region Audit Fields
		#region tstamp
		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		[PXUIField(DisplayName = "Created At", Enabled = false, IsReadOnly = true)]
		public virtual String CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedByID, Enabled = false, IsReadOnly = true)]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		[PXUIField(DisplayName = "Last Modified At", Enabled = false, IsReadOnly = true)]
		public virtual String LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion
	}
}