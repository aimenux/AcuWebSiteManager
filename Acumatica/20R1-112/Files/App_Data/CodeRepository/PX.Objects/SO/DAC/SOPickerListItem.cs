using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using System;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOPickerListEntry, PXDacType.Details)]
	public class SOPickerListEntry : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOPickerListEntry>.By<worksheetNbr, pickerNbr, entryNbr>
		{
			public static SOPickerListEntry Find(PXGraph graph, string worksheetNbr, int? pickerNbr, int? entryNbr) => FindBy(graph, worksheetNbr, pickerNbr, entryNbr);
		}

		public static class FK
		{
			public class Worksheet : SOPickingWorksheet.PK.ForeignKeyOf<SOPickerListEntry>.By<worksheetNbr> { }
			public class Picker : SOPicker.PK.ForeignKeyOf<SOPickerListEntry>.By<worksheetNbr, pickerNbr> { }
			public class Shipment : SOShipment.PK.ForeignKeyOf<SOPickerListEntry>.By<shipmentNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<SOPickerListEntry>.By<siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<SOPickerListEntry>.By<locationID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SOPickerListEntry>.By<inventoryID> { }
		}
		#endregion

		#region WorksheetNbr
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBDefault(typeof(SOPickingWorksheet.worksheetNbr))]
		[PXParent(typeof(FK.Worksheet))]
		public virtual String WorksheetNbr { get; set; }
		public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
		#endregion
		#region PickerNbr
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(SOPicker.pickerNbr))]
		[PXUIField(DisplayName = "Picker Nbr.")]
		[PXParent(typeof(FK.Picker))]
		public virtual Int32? PickerNbr { get; set; }
		public abstract class pickerNbr : PX.Data.BQL.BqlInt.Field<pickerNbr> { }
		#endregion
		#region EntryNbr
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(SOPicker))]
		[PXUIField(DisplayName = "Entry Nbr.")]
		public virtual Int32? EntryNbr { get; set; }
		public abstract class entryNbr : PX.Data.BQL.BqlInt.Field<entryNbr> { }
		#endregion
		#region ShipmentNbr
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Shipment Nbr.", Enabled = false)]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		#endregion
		#region SiteID
		[Site(Enabled = false)]
		[PXForeignReference(typeof(FK.Site))]
		[PXDefault]
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
		[PXDefault]
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
		[PXDefault]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region ExpireDate
		[PXDBDate(InputMask = "d", DisplayMask = "d")]
		[PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial", Enabled = false)]
		public virtual DateTime? ExpireDate { get; set; }
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region OrderLineUOM
		[INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
		[PXDefault]
		public virtual String OrderLineUOM { get; set; }
		public abstract class orderLineUOM : PX.Data.BQL.BqlString.Field<orderLineUOM> { }
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
		#region IsUnassigned
		[PXDBBool]
		[PXDefault(false)]
		[IsUnassigned]
		public virtual Boolean? IsUnassigned { get; set; }
		public abstract class isUnassigned : PX.Data.BQL.BqlBool.Field<isUnassigned> { }
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

		public class IsUnassignedAttribute : PXEventSubscriberAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber, IPXRowDeletedSubscriber
		{
			public void RowInserted(PXCache cache, PXRowInsertedEventArgs e)
			{
				var row = (SOPickerListEntry)e.Row;
				if (row.IsUnassigned == true) return;
				if (IsUnassignable(cache.Graph, row.InventoryID) == false) return;

				UpdateUnassigned(cache, row, -row.Qty);
			}

			public void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
			{
				var row = (SOPickerListEntry)e.Row;
				var oldRow = (SOPickerListEntry)e.OldRow;
				if (row.IsUnassigned == true) return;
				if (cache.ObjectsEqual<qty>(row, oldRow)) return;
				if (IsUnassignable(cache.Graph, row.InventoryID) == false) return;

				UpdateUnassigned(cache, row, oldRow.Qty - row.Qty);
			}

			public void RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
			{
				var row = (SOPickerListEntry)e.Row;
				if (row.IsUnassigned == true) return;
				if (IsUnassignable(cache.Graph, row.InventoryID) == false) return;

				UpdateUnassigned(cache, row, row.Qty);
			}

			protected virtual void UpdateUnassigned(PXCache cache, SOPickerListEntry assigned, decimal? deltaQty)
			{
				SOPickerListEntry unassigned = deltaQty > 0 && GetLotSerClass(cache.Graph, assigned.InventoryID).LotSerTrack == INLotSerTrack.SerialNumbered
					? null
					:	SelectFrom<SOPickerListEntry>.
						Where<
							isUnassigned.IsEqual<True>.
							And<worksheetNbr.IsEqual<worksheetNbr.FromCurrent>>.
							And<pickerNbr.IsEqual<pickerNbr.FromCurrent>>.
							And<inventoryID.IsEqual<inventoryID.FromCurrent>>.
							And<subItemID.IsEqual<subItemID.FromCurrent>>.
							And<locationID.IsEqual<locationID.FromCurrent>>.
							And<orderLineUOM.IsEqual<orderLineUOM.FromCurrent>>>.
						View.SelectSingleBound(cache.Graph, new[] { assigned }).TopFirst;

				if (unassigned == null)
				{
					unassigned = PXCache<SOPickerListEntry>.CreateCopy(assigned);

					unassigned.EntryNbr = null;
					unassigned.LotSerialNbr = "";
					unassigned.ExpireDate = null;
					unassigned.Qty = 0;
					unassigned.BaseQty = 0;
					unassigned.PickedQty = 0;
					unassigned.BasePickedQty = 0;
					unassigned.IsUnassigned = true;

					unassigned = (SOPickerListEntry)cache.Insert(unassigned);
				}

				unassigned.Qty += deltaQty;
				if (unassigned.Qty == 0)
					unassigned = (SOPickerListEntry)cache.Delete(unassigned);
				else
					unassigned = (SOPickerListEntry)cache.Update(unassigned);
			}

			protected virtual bool IsUnassignable(PXGraph graph, int? inventoryID) => GetLotSerClass(graph, inventoryID).With(lsc => lsc.IsUnassigned);

			protected virtual INLotSerClass GetLotSerClass(PXGraph graph, int? inventoryID) => InventoryItem.PK.Find(graph, inventoryID).With(ii => INLotSerClass.PK.Find(graph, ii.LotSerClassID));
		}
	}
}