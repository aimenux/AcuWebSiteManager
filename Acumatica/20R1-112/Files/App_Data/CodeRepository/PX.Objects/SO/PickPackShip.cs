using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.Extensions;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.SO.PickPackShip, PX.Objects.SO.PickPackShipHost, PX.Objects.SO.SOShipment, PX.Objects.SO.PickPackShip.Header>;

namespace PX.Objects.SO
{
	public class PickPackShipHost : SOShipmentEntry
	{
		public override Type PrimaryItemType => typeof(PickPackShip.Header);
		public PXFilter<PickPackShip.Header> HeaderView;
		public PickPackShip WMS => FindImplementation<PickPackShip>();
	}
	public class PickPackShip : WMSBase
	{
		public class UserSetup : PXUserSetup<UserSetup, PickPackShipHost, Header, SOPickPackShipUserSetup, SOPickPackShipUserSetup.userID> { }

		#region Attached Fields
		[PXUIField(DisplayName = Msg.PackedQtyPerBox)]
		public class PackedQtyPerBox : PXFieldAttachedTo<SOShipLineSplit>.By<PickPackShipHost>.AsDecimal.Named<PackedQtyPerBox>
		{
			public override decimal? GetValue(SOShipLineSplit row)
			{
				Header header = Base.WMS.HeaderView.Current;
				SOShipLineSplitPackage package = Base.PackageDetailExt.PackageDetailSplit.SelectMain(header.RefNbr, header.PackageLineNbrUI).FirstOrDefault(t => t.ShipmentSplitLineNbr == row.SplitLineNbr);
				return package?.PackedQty ?? 0m;
			}
		}

		[PXUIField(DisplayName = Msg.CartQty)]
		public class CartQty : PXFieldAttachedTo<SOShipLineSplit>.By<PickPackShipHost>.AsDecimal.Named<CartQty>
		{
			public override decimal? GetValue(SOShipLineSplit row) => Base.WMS.GetCartQty(row);
			protected override bool? Visible => Base.WMS.With(wms => wms.IsCartRequired(wms.HeaderView.Current));
		}

		[PXUIField(DisplayName = Msg.CartOverallQty)]
		public class OverallCartQty : PXFieldAttachedTo<SOShipLineSplit>.By<PickPackShipHost>.AsDecimal.Named<OverallCartQty>
		{
			public override decimal? GetValue(SOShipLineSplit row) => Base.WMS.GetOverallCartQty(row);
			protected override bool? Visible => Base.WMS.With(wms => wms.IsCartRequired(wms.HeaderView.Current));
		}

		[PXUIField(DisplayName = Msg.Fits)]
		public class Fits : PXFieldAttachedTo<SOShipLineSplit>.By<PickPackShipHost>.AsBool.Named<Fits>
		{
			public override bool? GetValue(SOShipLineSplit row)
			{
				var header = Base.WMS.HeaderView.Current;
				bool fits = true;
				if (header.LocationID != null)
					fits &= header.LocationID == row.LocationID;
				if (header.InventoryID != null)
					fits &= header.InventoryID == row.InventoryID && header.SubItemID == row.SubItemID;
				if (header.LotSerialNbr != null)
					fits &= header.LotSerialNbr == row.LotSerialNbr || header.Mode == Modes.Pick && header.IsEntarableLotSerial && row.PickedQty == 0;
				return fits;
			}
		}

		[PXUIField(DisplayName = Msg.Fits)]
		public class FitsWS : PXFieldAttachedTo<SOPickerListEntry>.By<PickPackShipHost>.AsBool.Named<FitsWS>
		{
			public override bool? GetValue(SOPickerListEntry row)
			{
				var header = Base.WMS.HeaderView.Current;
				bool fits = true;
				if (header.LocationID != null)
					fits &= header.LocationID == row.LocationID;
				if (header.InventoryID != null)
					fits &= header.InventoryID == row.InventoryID && header.SubItemID == row.SubItemID;
				if (header.LotSerialNbr != null)
					fits &= header.LotSerialNbr == row.LotSerialNbr || header.Mode.IsIn(Modes.PickWave, Modes.PickBatch) && header.IsEntarableLotSerial && row.PickedQty == 0;
				return fits;
			}
		}

		[PXUIField(Visible = false)]
		public class ShowPickWS : PXFieldAttachedTo<Header>.By<PickPackShipHost>.AsBool.Named<ShowPickWS>
		{
			public override bool? GetValue(Header row) => Base.WMS.Setup.Current.ShowPickTab == true && (row.Mode == Modes.Free && row.WorksheetNbr != null || row.Mode.IsIn(Modes.PickWave, Modes.PickBatch));
		}

		[PXUIField(Visible = false)]
		public class ShowPick : PXFieldAttachedTo<Header>.By<PickPackShipHost>.AsBool.Named<ShowPick>
		{
			public override bool? GetValue(Header row) => Base.WMS.Setup.Current.ShowPickTab == true && (row.Mode == Modes.Free && row.WorksheetNbr == null || row.Mode == Modes.Pick);
		}

		[PXUIField(Visible = false)]
		public class ShowPack : PXFieldAttachedTo<Header>.By<PickPackShipHost>.AsBool.Named<ShowPack>
		{
			public override bool? GetValue(Header row) => Base.WMS.Setup.Current.ShowPackTab == true && row.Mode.IsIn(Modes.Free, Modes.Pack);
		}

		[PXUIField(Visible = false)]
		public class ShowShip : PXFieldAttachedTo<Header>.By<PickPackShipHost>.AsBool.Named<ShowShip>
		{
			public override bool? GetValue(Header row) => Base.WMS.Setup.Current.ShowShipTab == true && row.Mode.IsIn(Modes.Free, Modes.Ship);
		}

		[PXUIField(Visible = false)]
		public class ShowLog : PXFieldAttachedTo<Header>.By<PickPackShipHost>.AsBool.Named<ShowLog>
		{
			public override bool? GetValue(Header row) => Base.WMS.Setup.Current.ShowScanLogTab == true;
		}

		public class ProcessingStatusIcon : PXFieldAttachedTo<Header>.By<PickPackShipHost>.AsBool.Named<ProcessingStatusIcon>
		{
			private bool? ProcessingSucceeded => Base.WMS.HeaderView.Current?.ProcessingSucceeded;
			public override bool? GetValue(Header row) => ProcessingSucceeded == true;
			protected override bool? Visible => ProcessingSucceeded != null;
		}
		#endregion

		#region DACs
		public class Header : WMSHeader, ILSMaster
		{
			#region ShipmentNbr
			[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXUIField(DisplayName = "Shipment Nbr.", Enabled = false)]
			[PXUIVisible(typeof(mode.IsIn<Modes.pick, Modes.pack, Modes.ship>.Or<mode.IsEqual<Modes.free>.And<worksheetNbr.IsNull>>))]
			[PXSelector(typeof(SOShipment.shipmentNbr))]
			public override string RefNbr { get; set; }
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region WorksheetNbr
			[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXUIField(DisplayName = "Worksheet Nbr.", Enabled = false)]
			[PXUIVisible(typeof(mode.IsIn<Modes.pickWave, Modes.pickBatch>.Or<mode.IsEqual<Modes.free>.And<worksheetNbr.IsNotNull>>))]
			[PXSelector(typeof(SOPickingWorksheet.worksheetNbr))]
			public virtual string WorksheetNbr { get; set; }
			public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
			#endregion
			#region PickerNbr
			[PXInt]
			[PXUIField(DisplayName = "Picker Nbr.", Enabled = false)]
			public virtual Int32? PickerNbr { get; set; }
			public abstract class pickerNbr : PX.Data.BQL.BqlInt.Field<pickerNbr> { }
			#endregion
			#region TranDate
			[PXDate]
			public virtual DateTime? TranDate { get; set; }
			public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
			#endregion

			#region SiteID
			[Site]
			public virtual int? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region CartID
			[PXInt]
			[PXUIField(DisplayName = "Cart ID", Enabled = false)]
			[PXSelector(typeof(Search2<INCart.cartID, InnerJoin<INSite, On<INCart.FK.Site>>, Where<INCart.active, Equal<True>, And<Match<INSite, Current<AccessInfo.userName>>>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
			[PXUIVisible(typeof(Where<mode.IsIn<Modes.pick, Modes.pickBatch, Modes.pickWave>.And<FeatureInstalled<FeaturesSet.wMSCartTracking>>.And<SOPickPackShipSetup.useCartsForPick.FromCurrent.IsEqual<True>>>))]
			public virtual int? CartID { get; set; }
			public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
			#endregion
			#region LocationID
			[Location]
			public virtual int? LocationID { get; set; }
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			#endregion

			#region InventoryID
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
			#region SubItemID
			public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			#endregion
			#region LotSerialNbr
			[INLotSerialNbr(typeof(inventoryID), typeof(subItemID), typeof(locationID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual string LotSerialNbr { get; set; }
			public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
			#endregion
			#region ExpirationDate
			[INExpireDate(typeof(inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual DateTime? ExpireDate { get; set; }
			public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
			#endregion

			#region LotSerTrack
			[PXString(1, IsFixed = true)]
			public virtual String LotSerTrack { get; set; }
			public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
			#endregion
			#region LotSerAssign
			[PXString(1, IsFixed = true)]
			public virtual String LotSerAssign { get; set; }
			public abstract class lotSerAssign : PX.Data.BQL.BqlString.Field<lotSerAssign> { }
			#endregion
			#region LotSerTrackExpiration
			[PXBool]
			public virtual Boolean? LotSerTrackExpiration { get; set; }
			public abstract class lotSerTrackExpiration : PX.Data.BQL.BqlBool.Field<lotSerTrackExpiration> { }
			#endregion
			#region LotSerIssueMethod
			[PXString(1, IsFixed = true)]
			public virtual String LotSerIssueMethod { get; set; }
			public abstract class lotSerIssueMethod : PX.Data.BQL.BqlString.Field<lotSerIssueMethod> { }
			#endregion
			#region AutoNextNbr
			[PXBool]
			public virtual Boolean? AutoNextNbr { get; set; }
			public abstract class autoNextNbr : PX.Data.BQL.BqlBool.Field<autoNextNbr> { }
			#endregion

			#region PackageLineNbr
			[PXInt]
			[PXFormula(typeof(Null.When<refNbr.IsNull>.Else<packageLineNbr>))]
			public virtual int? PackageLineNbr { get; set; }
			public abstract class packageLineNbr : PX.Data.BQL.BqlInt.Field<packageLineNbr> { }
			#endregion
			#region PackageLineNbrUI
			[PXInt]
			[PXUIField(DisplayName = "Package")]
			[PXSelector(
				typeof(Search<SOPackageDetailEx.lineNbr, Where<SOPackageDetailEx.shipmentNbr, Equal<Current<Header.refNbr>>>>),
				typeof(BqlFields.FilledWith<
					SOPackageDetailEx.confirmed,
					SOPackageDetailEx.lineNbr,
					SOPackageDetailEx.boxID,
					SOPackageDetailEx.boxDescription,
					SOPackageDetailEx.weight,
					SOPackageDetailEx.maxWeight,
					SOPackageDetailEx.weightUOM,
					SOPackageDetailEx.carrierBox,
					SOPackageDetailEx.length,
					SOPackageDetailEx.width,
					SOPackageDetailEx.height>),
				DescriptionField = typeof(SOPackageDetailEx.boxID), DirtyRead = true, SuppressUnconditionalSelect = true)]
			[PXFormula(typeof(packageLineNbr))]
			public virtual int? PackageLineNbrUI { get; set; }
			public abstract class packageLineNbrUI : PX.Data.BQL.BqlInt.Field<packageLineNbrUI> { }
			#endregion
			#region Weight
			[PXDecimal(6)]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			public virtual decimal? Weight { get; set; }
			public abstract class weight : PX.Data.BQL.BqlDecimal.Field<weight> { }
			#endregion
			#region LastWeighingTime
			[PXDate]
			public virtual DateTime? LastWeighingTime { get; set; }
			public abstract class lastWeighingTime : PX.Data.BQL.BqlDateTime.Field<lastWeighingTime> { }
			#endregion

			#region CartLoaded
			[PXBool, PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Cart Unloading", Enabled = false)]
			[PXUIVisible(typeof(cartLoaded))]
			public virtual bool? CartLoaded { get; set; }
			public abstract class cartLoaded : PX.Data.BQL.BqlBool.Field<cartLoaded> { }
			#endregion
			#region ToteID
			[PXDBInt]
			public virtual int? RemoveFromToteID { get; set; }
			public abstract class toteID : PX.Data.BQL.BqlInt.Field<toteID> { }
			#endregion

			#region ProcessingSucceeded
			[PXBool]
			public virtual bool? ProcessingSucceeded { get; set; }
			public abstract class processingSucceeded : PX.Data.BQL.BqlBool.Field<processingSucceeded> { }
			#endregion

			#region ILSMaster
			string ILSMaster.TranType => "";

			[PXUnboundDefault((short)-1)] // issue
			public short? InvtMult { get; set; }

			int? ILSMaster.ProjectID
			{
				get { return null; }
				set { }
			}

			int? ILSMaster.TaskID
			{
				get { return null; }
				set { }
			}
			#endregion
			public decimal GetBaseQty(PickPackShipHost graph) => INUnitAttribute.ConvertToBase(graph.Transactions.Cache, InventoryID, UOM, Qty ?? 0, INPrecision.NOROUND);
			public virtual bool IsEntarableLotSerial => LotSerAssign == INLotSerAssign.WhenUsed || LotSerIssueMethod == INLotSerIssueMethod.UserEnterable;
			public virtual bool HasLotSerial => LotSerTrack == INLotSerTrack.LotNumbered || LotSerTrack == INLotSerTrack.SerialNumbered;
			public virtual bool IsReturnMode => Mode == Modes.Pick && InvtMult == 1; // TODO: make separate mode RETURN
		}
		#endregion

		#region Views
		public override PXFilter<Header> HeaderView => Base.HeaderView;

		public PXSetupOptional<SOPickPackShipSetup, Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>> Setup;

		public SelectFrom<SOShipmentProcessedByUser>.
			Where<SOShipmentProcessedByUser.FK.Shipment.SameAsCurrent.
				And<SOShipmentProcessedByUser.userID.IsEqual<AccessInfo.userID.FromCurrent>>>.
			View ShipmentProcessedByUser;

		#region Worksheets
		public
			SelectFrom<SOPickingWorksheet>.
			Where<SOPickingWorksheet.worksheetNbr.IsEqual<Header.worksheetNbr.FromCurrent.NoDefault>>.
			View Worksheet;

		public
			SelectFrom<SOPicker>.
			Where<
				SOPicker.worksheetNbr.IsEqual<Header.worksheetNbr.FromCurrent.NoDefault>.
				And<SOPicker.pickerNbr.IsEqual<Header.pickerNbr.FromCurrent.NoDefault>>>.
			View Picker;

		public
			SelectFrom<SOPickerToShipmentLink>.
			Where<SOPickerToShipmentLink.FK.Picker.SameAsCurrent>.
			View ShipmentsOfPicker;

		public
			SelectFrom<SOPickerListEntry>.
			InnerJoin<SOPicker>.On<SOPickerListEntry.FK.Picker>.
			InnerJoin<INLocation>.On<SOPickerListEntry.FK.Location>.
			LeftJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Picker.And<SOPickerToShipmentLink.shipmentNbr.IsEqual<SOPickerListEntry.shipmentNbr>>>.
			LeftJoin<INTote>.On<SOPickerToShipmentLink.FK.Tote>.
			Where<SOPickerListEntry.FK.Picker.SameAsCurrent>.
			OrderBy<INLocation.pathPriority.Asc, INLocation.locationCD.Asc>.
			View PickListOfPicker;
		#endregion

		#region Pick
		public
			SelectFrom<SOShipLineSplit>
			.InnerJoin<SOShipLine>.On<SOShipLineSplit.FK.ShipLine>
			.OrderBy<SOShipLineSplit.shipmentNbr.Asc, SOShipLineSplit.isUnassigned.Desc, SOShipLineSplit.lineNbr.Asc>
			.View Picked;
		protected virtual IEnumerable picked()
		{
			var assignedOnly =
				SelectFrom<SOShipLineSplit>.
				InnerJoin<SOShipLine>.On<SOShipLineSplit.FK.ShipLine>.
				InnerJoin<INLocation>.On<SOShipLineSplit.locationID.IsEqual<INLocation.locationID>>.
				Where<SOShipLineSplit.shipmentNbr.IsEqual<Header.refNbr.FromCurrent>>.
				View.Select(Base)
				.AsEnumerable()
				.Cast<PXResult<SOShipLineSplit, SOShipLine, INLocation>>();

			IEnumerable<PXResult<SOShipLineSplit, SOShipLine, INLocation>> splits;
			if (HeaderView.Current.Mode.IsIn(Modes.Pick, Modes.Free) || HeaderView.Current.Mode.IsIn(Modes.Pack, Modes.Free) && NoPick)
			{
				var unassignedOnly =
					SelectFrom<Unassigned.SOShipLineSplit>.
					InnerJoin<SOShipLine>.On<Unassigned.SOShipLineSplit.FK.ShipLine>.
					InnerJoin<INLocation>.On<Unassigned.SOShipLineSplit.locationID.IsEqual<INLocation.locationID>>.
					Where<Unassigned.SOShipLineSplit.shipmentNbr.IsEqual<Header.refNbr.FromCurrent>>.
					View.Select(Base).AsEnumerable()
					.Cast<PXResult<Unassigned.SOShipLineSplit, SOShipLine, INLocation>>()
					.Select(r => new PXResult<SOShipLineSplit, SOShipLine, INLocation>(MakeAssigned(r), r, r));

				splits = assignedOnly.Concat(unassignedOnly);
			}
			else
				splits = assignedOnly;

			(var picked, var nonpicked) = splits.DisuniteBy(s => s.GetItem<SOShipLineSplit>().PickedQty >= s.GetItem<SOShipLineSplit>().Qty);

			var delegateResult = new PXDelegateResult { IsResultSorted = true };

			delegateResult.AddRange(
				nonpicked
				.OrderBy(
					r => Setup.Current.ShipmentLocationOrdering == SOPickPackShipSetup.shipmentLocationOrdering.Pick
						? r.GetItem<INLocation>().PickPriority
						: r.GetItem<INLocation>().PathPriority)
				.ThenBy(r => r.GetItem<SOShipLineSplit>().IsUnassigned == false) // unassigned first
				.ThenBy(r => r.GetItem<SOShipLineSplit>().InventoryID)
				.ThenBy(r => r.GetItem<SOShipLineSplit>().LotSerialNbr));

			delegateResult.AddRange(
				picked
				.OrderByDescending(
					r => Setup.Current.ShipmentLocationOrdering == SOPickPackShipSetup.shipmentLocationOrdering.Pick
						? r.GetItem<INLocation>().PickPriority
						: r.GetItem<INLocation>().PathPriority)
				.ThenByDescending(r => r.GetItem<SOShipLineSplit>().InventoryID)
				.ThenByDescending(r => r.GetItem<SOShipLineSplit>().LotSerialNbr));

			return delegateResult;
		}

		#region Cart
		public
			PXSelect<INCart,
			Where<INCart.siteID, Equal<Current<Header.siteID>>,
				And<INCart.cartID, Equal<Current<Header.cartID>>>>>
			Cart;

		public PXSelect<INCartSplit, Where<INCartSplit.FK.Cart.SameAsCurrent>> CartSplits;

		public PXSelect<SOCartShipment> CartsLinks;

		public
			PXSelectJoin<INCartSplit,
			InnerJoin<INCart, On<INCartSplit.FK.Cart>,
			InnerJoin<SOCartShipment, On<SOCartShipment.FK.Cart>>>,
			Where<SOCartShipment.FK.Shipment.SameAsCurrent>>
			AllCartSplits;

		public
			PXSelectJoin<SOShipmentSplitToCartSplitLink,
			InnerJoin<INCartSplit, On<SOShipmentSplitToCartSplitLink.FK.CartSplit>>,
			Where<SOShipmentSplitToCartSplitLink.FK.Cart.SameAsCurrent>>
			CartSplitLinks;

		public
			PXSelectJoin<SOShipmentSplitToCartSplitLink,
			InnerJoin<INCartSplit, On<SOShipmentSplitToCartSplitLink.FK.CartSplit>,
			InnerJoin<INCart, On<INCartSplit.FK.Cart>,
			InnerJoin<SOCartShipment, On<SOCartShipment.FK.Cart>>>>,
			Where<SOCartShipment.FK.Shipment.SameAsCurrent>>
			AllCartSplitLinks;

		public
			SelectFrom<SOPickListEntryToCartSplitLink>.
			InnerJoin<INCartSplit>.On<SOPickListEntryToCartSplitLink.FK.CartSplit>.
			Where<SOPickListEntryToCartSplitLink.FK.Cart.SameAsCurrent>.
			View PickerCartSplitLinks;
		#endregion
		#endregion

		#region Pack
		public
			SelectFrom<SOShipLineSplit>
			.InnerJoin<SOShipLine>.On<SOShipLineSplit.FK.ShipLine>
			.OrderBy<SOShipLineSplit.shipmentNbr.Asc, SOShipLineSplit.isUnassigned.Desc, SOShipLineSplit.lineNbr.Asc>
			.View PickedForPack;
		protected virtual IEnumerable pickedForPack() => Picked.Select();

		public PXSelect<SOShipLineSplit> Packed;
		protected virtual IEnumerable packed()
		{
			return HeaderView.Current == null
				? Enumerable.Empty<PXResult<SOShipLineSplit, SOShipLine>>() :
				from link in Base.PackageDetailExt.PackageDetailSplit.SelectMain(HeaderView.Current.RefNbr, HeaderView.Current.PackageLineNbrUI)
				from split in Picked.Select().Cast<PXResult<SOShipLineSplit, SOShipLine>>()
				where ((SOShipLineSplit)split).ShipmentNbr == link.ShipmentNbr
					  && ((SOShipLineSplit)split).LineNbr == link.ShipmentLineNbr
					  && ((SOShipLineSplit)split).SplitLineNbr == link.ShipmentSplitLineNbr
				select split;
		}

		public
			SelectFrom<SOPackageDetailEx>.
			Where<SOPackageDetailEx.shipmentNbr.IsEqual<Header.refNbr.FromCurrent>.
				And<SOPackageDetailEx.lineNbr.IsEqual<Header.packageLineNbrUI.FromCurrent.NoDefault>>>.
			View ShownPackage;

		public PXSetupOptional<CommonSetup> CommonSetupUOM;
		#endregion

		#endregion

		#region Buttons
		public PXAction<Header> ScanConfirmShipment;
		[PXButton, PXUIField(DisplayName = "Confirm Shipment")]
		protected virtual IEnumerable scanConfirmShipment(PXAdapter adapter) => scanBarcode(adapter, ScanCommands.ConfirmShipment);

		public PXAction<Header> ScanConfirmShipmentAll;
		[PXButton, PXUIField(DisplayName = "Confirm Shipment As Is")]
		protected virtual IEnumerable scanConfirmShipmentAll(PXAdapter adapter) => scanBarcode(adapter, ScanCommands.ConfirmShipmentAll);

		public PXAction<Header> ScanConfirmPickList;
		[PXButton, PXUIField(DisplayName = "Confirm Pick List")]
		protected virtual IEnumerable scanConfirmPickList(PXAdapter adapter) => scanBarcode(adapter, ScanCommands.ConfirmPickList);

		public PXAction<Header> ReviewPick;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable reviewPick(PXAdapter adapter) => adapter.Get();

		public PXAction<Header> ReviewPack;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable reviewPack(PXAdapter adapter)
		{
			HeaderSetter.Set(h => h.PackageLineNbrUI, null);
			return adapter.Get();
		}

		public PXAction<Header> ReviewPickWS;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable reviewPickWS(PXAdapter adapter) => adapter.Get();

		public PXAction<Header> ViewOrder;
		[PXButton, PXUIField(DisplayName = "View Order")]
		protected virtual IEnumerable viewOrder(PXAdapter adapter)
		{
			SOShipLineSplit currentSplit = Picked.Current;
			if (currentSplit == null)
				return adapter.Get();

			SOShipLine currentLine = Picked.Search<SOShipLineSplit.splitLineNbr>(currentSplit.SplitLineNbr).FirstOrDefault()?.GetItem<SOShipLine>();
			if (currentLine == null)
				return adapter.Get();

			var orderEntry = PXGraph.CreateInstance<SOOrderEntry>();
			orderEntry.Document.Current = orderEntry.Document.Search<SOOrder.orderType, SOOrder.orderNbr>(currentLine.OrigOrderType, currentLine.OrigOrderNbr);
			throw new PXRedirectRequiredException(orderEntry, true, nameof(ViewOrder)) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<Header> GetReturnLabels;
		[PXUIField(DisplayName = SOShipmentEntryActionsAttribute.Messages.GetReturnLabels, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable getReturnLabels(PXAdapter adapter)
		{
			Save.Press();

			var headers = adapter.Get<PickPackShip.Header>().ToList();
			var massProcess = adapter.MassProcess;
			PickPackShipHost clone = !massProcess ? Base.Clone() : null;
			PXLongOperation.StartOperation(Base, () => { GetReturnLabelsProcess(headers, massProcess, clone); });

			return headers;
		}

		public PXAction<Header> RefreshRates;
		[PXUIField(DisplayName = Messages.RefreshRatesButton, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		public virtual IEnumerable refreshRates(PXAdapter adapter)
		{
			if (!string.IsNullOrEmpty(HeaderView.Current?.RefNbr))
			{
				Save.Press();

				PickPackShipHost clone = Base.Clone();

				PXLongOperation.StartOperation(Base, () =>
				{
					PXLongOperation.SetCustomInfo(clone); // Redirect

					UpdateRates(clone);
				});

				Base.RowSelected.AddHandler<SOCarrierRate>((sender, args) =>
				{
					if (args.Row != null)
						sender.Adjust<PXUIFieldAttribute>(args.Row).For<SOCarrierRate.amount>(a =>
						{
							if (a.ErrorLevel == PXErrorLevel.Error)
								((IPXInterfaceField)a).ErrorLevel = PXErrorLevel.RowError;
						});
				});
			}

			return adapter.Get();
		}

		protected static void UpdateRates(PickPackShipHost graph)
		{
			var carrierRateErrors = new Dictionary<SOCarrierRate, PXSetPropertyException>();
			void saveCarrierRateError(PXCache cache, PXExceptionHandlingEventArgs args)
			{
				if (args.Exception is PXSetPropertyException ex)
					carrierRateErrors[(SOCarrierRate)args.Row] = ex;
			};
			try
			{
				graph.ExceptionHandling.AddHandler<SOCarrierRate.method>(saveCarrierRateError);
				graph.CarrierRatesExt.UpdateRates();
			}
			finally
			{
				graph.ExceptionHandling.RemoveHandler<SOCarrierRate.method>(saveCarrierRateError);
			}
			var carrierRateCache = graph.Caches<SOCarrierRate>();
			foreach (var eInfo in carrierRateErrors)
			{
				var carrierRate = eInfo.Key;
				var error = eInfo.Value;
				error = new PXSetPropertyException(error.Message, PXErrorLevel.Error) { ErrorValue = carrierRate.Amount };
				carrierRateCache.RaiseExceptionHandling<SOCarrierRate.amount>(carrierRate, carrierRate.Method, error);
			}
		}
		#endregion

		#region Event Handlers
		protected virtual void _(Events.FieldUpdated<Header, WMSHeader.qty> e)
		{
			if (HeaderView.Current.ScanState == ScanStates.Confirm)
				SetScanState(ScanStates.Confirm);
		}

		protected override void _(Events.RowSelected<Header> e)
		{
			base._(e);

			if (e.Row == null)
				return;

			ScanConfirm.SetVisible(e.Row.Mode != Modes.Ship && (ExplicitLineConfirmation || e.Row.Mode == Modes.Pack || Info.Current?.MessageType == WMSMessageTypes.Warning));
			ScanRemove.SetVisible(e.Row.Mode != Modes.Ship);

			ScanConfirmShipment.SetVisible(e.Row.Mode.IsIn(Modes.Pick, Modes.Pack, Modes.Ship));
			ScanConfirmPickList.SetVisible(e.Row.Mode.IsIn(Modes.PickWave, Modes.PickBatch));
			ScanConfirmShipmentAll.SetVisible(false);

			ReviewPick.SetVisible(Base.IsMobile && e.Row.Mode == Modes.Pick);
			ReviewPack.SetVisible(Base.IsMobile && e.Row.Mode == Modes.Pack);
			ReviewPickWS.SetVisible(Base.IsMobile && e.Row.Mode.IsIn(Modes.PickWave, Modes.PickBatch));

			if (IsCartRequired(e.Row))
				Cart.Current = Cart.Select();

			ScanModePick.SetEnabled(e.Row.Mode.IsNotIn(Modes.Pick, Modes.PickWave, Modes.PickBatch));
			ScanModePack.SetEnabled(e.Row.Mode != Modes.Pack);
			ScanModeShip.SetEnabled(e.Row.Mode != Modes.Ship);

			bool? isConfirmed = e.Row.Mode.IsIn(Modes.PickWave, Modes.PickBatch)
				? SOPicker.PK.Find(Base, e.Row.WorksheetNbr, e.Row.PickerNbr)?.Confirmed
				: e.Row.RefNbr.With(nbr => SOShipment.PK.Find(Base, nbr)?.Confirmed);
			if (isConfirmed == true)
			{
				new[] {
					Picked.Cache,
					Base.Packages.Cache,
					Base.PackageDetailExt.PackageDetailSplit.Cache
				}.Modify(c => c.Adjust<PXUIFieldAttribute>().ForAllFields(a => a.Enabled = false)).Consume();
			}
			var isNotConfirmed = isConfirmed == false;

			PickListOfPicker.Cache.AdjustUI().For<SOPickerListEntry.shipmentNbr>(a => a.Visible = e.Row.Mode == Modes.PickWave);

			new[] {
				Picked.Cache,
				Base.Packages.Cache,
				Base.PackageDetailExt.PackageDetailSplit.Cache
			}
			.Modify(c => c.AllowInsert = c.AllowUpdate = c.AllowDelete = isNotConfirmed)
			.Consume();

			ScanRemove.SetEnabled(e.Row.Remove == false && isNotConfirmed && (e.Row.Mode == Modes.Pack).Implies(HasUnconfirmedBoxes));
			ScanConfirmShipment.SetEnabled(isNotConfirmed);
			ScanConfirmShipmentAll.SetEnabled(isNotConfirmed);
			ScanConfirmPickList.SetEnabled(isNotConfirmed);
			RefreshRates.SetEnabled(isNotConfirmed);
			GetReturnLabels.SetEnabled(isNotConfirmed);

			if (String.IsNullOrEmpty(e.Row.RefNbr))
				Base.Document.Current = null;
			else
				Base.Document.Current = Base.Document.Search<SOShipment.shipmentNbr>(e.Row.RefNbr);

			if (String.IsNullOrEmpty(e.Row.WorksheetNbr))
			{
				Worksheet.Current = null;
				Picker.Current = null;
			}
			else
			{
				Worksheet.Current = Worksheet.Search<SOPickingWorksheet.worksheetNbr>(e.Row.WorksheetNbr);
				Picker.Current = Picker.Search<SOPicker.worksheetNbr>(e.Row.WorksheetNbr);
			}
		}

		protected virtual void _(Events.RowUpdated<SOPickPackShipUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
		protected virtual void _(Events.RowInserted<SOPickPackShipUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);

		protected virtual void _(Events.FieldSelecting<SOShipLineSplit, SOShipLineSplit.lotSerialNbr> e)
		{
			if (e.Row != null && e.Row.IsUnassigned == true)
				e.ReturnValue = IN.Messages.Unassigned;
		}

		protected virtual void _(Events.RowSelected<SOShipLineSplit> e)
		{
			if (e.Row != null && e.Row.IsUnassigned == true)
				e.Cache.Adjust<PXUIFieldAttribute>(e.Row).ForAllFields(a => a.Enabled = false);
		}
		#endregion

		#region DAC overrides
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		protected virtual void _(Events.CacheAttached<SOShipLineSplit.lineNbr> e) { }

		[PXCustomizeBaseAttribute(typeof(SOShipLotSerialNbrAttribute), nameof(SOShipLotSerialNbrAttribute.ForceDisable), true)]
		protected virtual void _(Events.CacheAttached<SOShipLineSplit.lotSerialNbr> e) { }

		[PXCustomizeBaseAttribute(typeof(SiteAttribute), nameof(SiteAttribute.Enabled), false)]
		protected virtual void _(Events.CacheAttached<SOShipLineSplit.siteID> e) { }

		[PXCustomizeBaseAttribute(typeof(SOLocationAvailAttribute), nameof(SOLocationAvailAttribute.Enabled), false)]
		protected virtual void _(Events.CacheAttached<SOShipLineSplit.locationID> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Enabled), false)]
		protected virtual void _(Events.CacheAttached<SOShipLineSplit.qty> e) { }

		[PXMergeAttributes]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderNbr.IsEqual<SOShipLine.origOrderType.FromCurrent>>>))]
		protected virtual void _(Events.CacheAttached<SOShipLine.origOrderNbr> e) { }
		#endregion


		public SOPackageDetailEx SelectedPackage => Base.Packages.SelectMain().FirstOrDefault(t => t.LineNbr == HeaderView.Current.PackageLineNbr);
		protected virtual SOShipment Shipment => SOShipment.PK.Find(Base, Base.Document.Current);
		protected virtual SOPickerToShipmentLink ShipmentWithoutTote => ShipmentsOfPicker.SelectMain().FirstOrDefault(s => s.ToteID == null);
		protected override BqlCommand DocumentSelectCommand()
			=> new SelectFrom<SOShipment>
				.Where<SOShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr.AsOptional>>();

		protected override WMSModeOf<PickPackShip, PickPackShipHost> DefaultMode
		{
			get
			{
				UserPreferences userPreferences = SelectFrom<UserPreferences>.Where<UserPreferences.userID.IsEqual<AccessInfo.userID.FromCurrent>>.View.Select(Base);
				var preferencesExt = userPreferences?.GetExtension<DefaultPickPackShipModeByUser>();

				return
					preferencesExt?.PPSMode == DefaultPickPackShipModeByUser.pPSMode.Pick && Setup.Current.ShowPickTab == true ? Modes.Pick :
					preferencesExt?.PPSMode == DefaultPickPackShipModeByUser.pPSMode.Pack && Setup.Current.ShowPackTab == true ? Modes.Pack :
					preferencesExt?.PPSMode == DefaultPickPackShipModeByUser.pPSMode.Ship && Setup.Current.ShowShipTab == true ? Modes.Ship :
					Setup.Current.ShowPickTab == true ? Modes.Pick :
					Setup.Current.ShowPackTab == true ? Modes.Pack :
					Setup.Current.ShowShipTab == true ? Modes.Ship :
					Modes.Free;
			}
		}

		public override string CurrentModeName =>
			HeaderView.Current.Mode.IsIn(Modes.Pick, Modes.PickWave, Modes.PickBatch) ? Msg.PickMode :
			HeaderView.Current.Mode == Modes.Pack ? Msg.PackMode :
			HeaderView.Current.Mode == Modes.Ship ? Msg.ShipMode :
			Msg.FreeMode;
		protected override string GetModePrompt()
		{
			if (HeaderView.Current.Mode == Modes.Pick)
			{
				if (HeaderView.Current.RefNbr == null)
					return Localize(Msg.ShipmentPrompt);
				if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
					return Localize(Msg.CartPrompt);
				return Localize(Msg.LocationPrompt);
			}
			else if (HeaderView.Current.Mode == Modes.PickWave)
			{
				if (HeaderView.Current.WorksheetNbr == null)
					return Localize(Msg.WorksheetPrompt);
				if (ShipmentWithoutTote != null)
					return Localize(Msg.TotePrompt, ShipmentWithoutTote.ShipmentNbr);
				return Localize(Msg.LocationPrompt);
			}
			else if (HeaderView.Current.Mode == Modes.PickBatch)
			{
				if (HeaderView.Current.WorksheetNbr == null)
					return Localize(Msg.WorksheetPrompt);
				if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
					return Localize(Msg.CartPrompt);
				return Localize(Msg.LocationPrompt);
			}
			if (HeaderView.Current.Mode == Modes.Pack)
			{
				if (HeaderView.Current.RefNbr == null)
					return Localize(Msg.ShipmentPrompt);
				else if (CanPack)
					return Localize(Msg.BoxPrompt);
				else if (HasUnconfirmedBoxes)
					return Localize(Msg.BoxConfirmPrompt);
				else
					return Localize(Msg.UseCommandToContinue);
			}
			if (HeaderView.Current.Mode == Modes.Ship)
				return string.Empty;
			return null;
		}

		#region Scan State logic
		protected override string GetDefaultState(Header header = null) => ScanStates.RefNbr;

		protected override bool ProcessCommand(string barcode)
		{
			switch (barcode)
			{
				case ScanCommands.Confirm:
					if (DocumentIsConfirmed)
						return true;
					if (HeaderView.Current.Mode == Modes.Pick)
						return ConfirmPicked();
					if (HeaderView.Current.Mode == Modes.PickWave)
						return ConfirmPicked();
					if (HeaderView.Current.Mode == Modes.PickBatch)
						return ConfirmPicked();
					if (HeaderView.Current.Mode == Modes.Pack)
					{
						if (HeaderView.Current.ScanState == ScanStates.BoxWeight)
						{
							SkipBoxWeightInput();
							return true;
						}
						else if (ConfirmPackedBox())
						{
							if (Setup.Current.ConfirmEachPackageWeight == false && SelectedPackage?.Confirmed == false)
								SkipBoxWeightInput();
							return true;
						}
					}
					return false;

				case ScanCommands.Remove:
					if (DocumentIsConfirmed) return true;
					HeaderSetter.Set(h => h.Remove, true);
					Report(Msg.RemoveMode);
					if (HeaderView.Current.ScanState == ScanStates.Command || HeaderView.Current.Mode == Modes.PickWave)
					{
						if (HeaderView.Current.Mode == Modes.Pick)
							SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item);
						else if (HeaderView.Current.Mode == Modes.PickWave)
							SetScanState(ScanStates.RemoveFromTote);
						else if (HeaderView.Current.Mode == Modes.PickBatch)
							SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item);
						else if (HeaderView.Current.Mode == Modes.Pack)
							SetScanState(ScanStates.Box);
					}
					return true;

				case ScanCommands.CartIn:
					if (IsCartRequired(HeaderView.Current) == false) return false;
					ClearHeaderInfo();
					HeaderSetter.Set(h => h.CartLoaded, false);
					if (CanPick)
						SetScanState(ScanStates.Location, Msg.CartLoading);
					else
						SetScanState(ScanStates.Command, Msg.CartLoading);
					return true;

				case ScanCommands.CartOut:
					if (IsCartRequired(HeaderView.Current) == false) return false;
					ClearHeaderInfo();
					HeaderSetter.Set(h => h.CartLoaded, true);
					if (CanPick)
						SetScanState(ScanStates.Item, Msg.CartUnloading);
					else
						SetScanState(ScanStates.Command, Msg.CartUnloading);
					return true;

				case ScanCommands.ConfirmShipment:
					if (DocumentIsConfirmed) return true;
					ConfirmShipment(Setup.Current.ShowPickTab != true && Setup.Current.ShowPackTab != true);
					return true;

				case ScanCommands.ConfirmShipmentAll:
					if (DocumentIsConfirmed) return true;
					ConfirmShipment(true);
					return true;

				case ScanCommands.ConfirmPickList:
					if (DocumentIsConfirmed) return true;
					ConfirmPickList();
					return true;
			}
			return false;
		}

		protected override bool ProcessByState(Header doc)
		{
			switch (doc.ScanState)
			{
				case ScanStates.Box:
					ProcessBoxBarcode(doc.Barcode);
					return true;

				case ScanStates.BoxWeight:
					ProcessBoxWeightBarcode(doc.Barcode);
					return true;

				case ScanStates.SortingLocation:
					ProcessSortingLocationBarcode(doc.Barcode);
					return true;

				case ScanStates.AssignTote:
					ProcessAssignToteBarcode(doc.Barcode);
					return true;

				case ScanStates.ConfirmTote:
					ProcessConfirmToteBarcode(doc.Barcode);
					return true;

				case ScanStates.RemoveFromTote:
					ProcessRemoveFromToteBarcode(doc.Barcode);
					return true;

				default:
					return base.ProcessByState(doc);
			}
		}

		protected override void ApplyState(string state)
		{
			switch (state)
			{
				case ScanStates.Command:
					Prompt(Msg.UseCommandToContinue);
					break;
				case ScanStates.Cart:
					Prompt(Msg.CartPrompt);
					break;
				case ScanStates.RefNbr:
					if (HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch))
						Prompt(Msg.WorksheetPrompt);
					else
						Prompt(Msg.ShipmentPrompt);
					break;
				case ScanStates.RemoveFromTote:
					Prompt(Msg.ToteToRemoveFromPrompt);
					break;
				case ScanStates.AssignTote:
					Prompt(Msg.TotePrompt, ShipmentWithoutTote.ShipmentNbr);
					break;
				case ScanStates.ConfirmTote:
					var properTote = GetSelectedPickListEntry(HeaderView.Current).With(GetToteForPickListEntry);
					if (properTote == null)
						SetScanState(ScanStates.Confirm);
					else
						Prompt(Msg.ToteConfirmPrompt, properTote.ToteCD);
					break;
				case ScanStates.Item:
					Prompt(Msg.InventoryPrompt);
					break;
				case ScanStates.Location:
					if (IsLocationRequired(HeaderView.Current))
						Prompt(Msg.LocationPrompt);
					else if ((HeaderView.Current.Mode == Modes.Pick && CanPick == false || HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch) && CanPickWS == false) && HeaderView.Current.Remove == false)
						SetScanState(ScanStates.Command);
					else
						SetScanState(HeaderView.Current.Mode == Modes.Pick && HeaderView.Current.IsReturnMode ? ScanStates.Confirm : ScanStates.Item);
					break;
				case ScanStates.SortingLocation:
					Prompt(Msg.SortingLocationPrompt);
					break;
				case ScanStates.LotSerial:
					Prompt(Msg.LotSerialPrompt);
					break;
				case ScanStates.ExpireDate:
					Prompt(Msg.LotSerialExpireDatePrompt);
					break;
				case ScanStates.Box:
					if (HeaderView.Current.Mode == Modes.Pack && CanPack == false && HasUnconfirmedBoxes == false && HeaderView.Current.Remove == false)
						SetScanState(ScanStates.Command);
					else if (HeaderView.Current.Mode == Modes.Pack && HasSingleAutoPackage(HeaderView.Current.RefNbr, out SOPackageDetailEx package))
					{
						HeaderSetter.WithEventFiring.Set(h => h.PackageLineNbr, package.LineNbr);
						Base.Packages.Current = package;
						SetScanState(IsLocationRequiredInPack(HeaderView.Current) ? ScanStates.Location : ScanStates.Item);
					}
					else
						Prompt(Msg.BoxPrompt);
					break;
				case ScanStates.BoxWeight:
					if (HeaderView.Current.Mode == Modes.Pack && HasSingleAutoPackage(HeaderView.Current.RefNbr, out var _))
					{
						SetScanState(ScanStates.Command, Msg.ShipmentPacked, HeaderView.Current.RefNbr);
					}
					else
						Prompt(Msg.BoxWeightPrompt);
					break;
				case ScanStates.Carrier:
					Prompt(Msg.CarrierPrompt);
					break;
				case ScanStates.Confirm:
				{
					if (IsMandatoryQtyInput)
					{
						Prompt(Msg.QtyPrompt);
						SetScanState(ScanStates.Qty);
					}
					else if (HeaderView.Current.Mode.IsIn(Modes.Pick, Modes.PickWave, Modes.PickBatch))
					{
						if (ExplicitLineConfirmation)
							Prompt(Msg.PickConfirmationPrompt, HeaderView.Cache.GetValueExt<Header.inventoryID>(HeaderView.Current), HeaderView.Current.Qty);
						else
							ConfirmPicked();
					}
					else if (HeaderView.Current.Mode == Modes.Pack)
					{
						ConfirmPacked();
					}
					break;
				}
			}
		}

		protected override bool PrepareRedirect(string command)
		{
			switch (command)
			{
				case ScanRedirects.ModePick:
					if (HeaderView.Current.RefNbr != null && Shipment.CurrentWorksheetNbr != null)
					{
						ReportError(Msg.ShipmentCannotBePickedSeparately, Shipment.ShipmentNbr, Shipment.CurrentWorksheetNbr);
						return false;
					}
					else return true;
			}
			return true;
		}

		protected override bool CompleteRedirect(string command)
		{
			switch (command)
			{
				case ScanRedirects.ModePick:
					if (HeaderView.Current.RefNbr == null)
						SetScanState(ScanStates.RefNbr);
					else if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
						SetScanState(ScanStates.Cart);
					else 
						SetScanState(Shipment?.Released == true || CanPick == false ? ScanStates.Command : ScanStates.Location);
					return true;

				case ScanRedirects.ModePack:
					if (HeaderView.Current.RefNbr == null)
						SetScanState(ScanStates.RefNbr);
					else
						SetScanState(Shipment?.Released == true || CanPack == false && HasUnconfirmedBoxes == false ? ScanStates.Command : ScanStates.Box);
					return true;

				case ScanRedirects.ModeShip:
					HeaderView.Current.CartLoaded = false;
					if (HeaderView.Current.RefNbr != null)
					{
						bool needToConfirmPackage = HasSingleAutoPackage(HeaderView.Current.RefNbr, out SOPackageDetailEx autoPackage) && autoPackage.Confirmed != true;
						if (needToConfirmPackage)
						{
							autoPackage.Confirmed = true;
							Base.Packages.Update(autoPackage);
							Base.Document.Current.IsPackageValid = true;
							Base.Document.UpdateCurrent();
							ClearHeaderInfo();
							Save.Press();
						}
						UpdateRates();
					}
					return true;
			}

			return true;
		}


		protected override void ProcessDocumentNumber(string barcode)
		{
			SOShipment shipment =
				SelectFrom<SOShipment>.
				InnerJoin<INSite>.On<SOShipment.FK.Site>.
				LeftJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
				Where<
					Match<INSite, AccessInfo.userName.FromCurrent>.
					And<
						Customer.bAccountID.IsNull.
						Or<Match<Customer, AccessInfo.userName.FromCurrent>>>.
					And<SOShipment.shipmentNbr.IsEqual<@P.AsString>>>.
				View.Select(Base, barcode);
			if (shipment == null)
			{
				(bool isHandled, SOShipment replacement) = HandleShipmentAbsence(barcode);
				if (isHandled)
				{
					if (replacement != null)
						shipment = replacement;
					else
						return; // absence was handled but shipment replacement was not provided - assume that some other type of barcode was processed
				}
				else
				{
					ReportError(Msg.ShipmentNbrMissing, barcode);
					return;
				}
			}

			if (shipment.Status != SOShipmentStatus.Open || (shipment.Operation == SOOperation.Receipt && HeaderView.Current.Mode.IsIn(Modes.Pack, Modes.Ship)))
			{
				ReportError(Msg.ShipmentInvalid, shipment.ShipmentNbr);
				return;
			}

			if (HeaderView.Current.CartID != null && shipment.SiteID != HeaderView.Current.SiteID)
			{
				ReportError(Msg.ShipmentInvalidSite, shipment.ShipmentNbr);
				return;
			}

			if (HeaderView.Current.Mode.IsIn(Modes.Pick, Modes.PickWave, Modes.PickBatch) && shipment.CurrentWorksheetNbr != null)
			{
				ReportError(Msg.ShipmentCannotBePickedSeparately, shipment.ShipmentNbr, shipment.CurrentWorksheetNbr);
				return;
			}

			if (HeaderView.Current.Mode.IsIn(Modes.Pack, Modes.Ship) && shipment.CurrentWorksheetNbr != null && shipment.Picked == false)
			{
				ReportError(Msg.ShipmentShouldBePickedFirst, shipment.ShipmentNbr);
				return;
			}

			if (HeaderView.Current.Mode == Modes.Pack)
				HasSingleAutoPackage(shipment.ShipmentNbr, out var _);

			HeaderView.Current.ProcessingSucceeded = null;
			Base.Document.Current = shipment;
			Worksheet.Current = null;
			Picker.Current = null;

			HeaderSetter.Set(h => h.RefNbr, shipment.ShipmentNbr);
			HeaderSetter.Set(h => h.WorksheetNbr, null);
			HeaderSetter.Set(h => h.PickerNbr, null);
			HeaderSetter.Set(h => h.SiteID, shipment.SiteID);
			HeaderSetter.Set(h => h.TranDate, shipment.ShipDate);
			HeaderSetter.Set(h => h.NoteID, shipment.NoteID);
			HeaderSetter.Set(h => h.InvtMult, (short)(shipment.Operation != SOOperation.Receipt ? -1 /* Issue */ : 1 /* Receipt */));

			if (HeaderView.Current.Mode.IsIn(Modes.PickBatch, Modes.PickWave))
				RedirectToMode(NoPick ? Modes.Pack : Modes.Pick);

			Report(Msg.ShipmentReady, shipment.ShipmentNbr);

			OnShipmentProcessed(shipment);
		}

		protected virtual (bool isHandled, SOShipment replacement) HandleShipmentAbsence(string barcode)
		{
			if (HeaderView.Current.Mode.IsIn(Modes.Pick, Modes.PickWave, Modes.PickBatch))
			{
				SOShipment returnShipment =
					PXSelectJoin<SOShipment,
					InnerJoin<INSite, On<INSite.siteID, Equal<SOShipment.siteID>>,
					InnerJoin<SOOrderShipment, On<SOOrderShipment.shipmentNbr, Equal<SOShipment.shipmentNbr>>,
					InnerJoin<SOOrder, On<
						SOOrder.orderType, Equal<SOOrderShipment.orderType>,
						And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
					LeftJoinSingleTable<Customer, On<SOShipment.customerID, Equal<Customer.bAccountID>>>>>>,
					Where2<Match<INSite, Current<AccessInfo.userName>>,
						And<SOShipment.status, Equal<SOShipmentStatus.open>,
						And<SOShipment.operation, Equal<SOOperation.receipt>,
						And<SOOrder.customerRefNbr, Equal<Required<SOOrder.customerRefNbr>>,
						And2<Where<SOShipment.siteID, Equal<Current<Header.siteID>>, Or<Current2<Header.siteID>, IsNull>>,
						And<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>>
					.Select(Base, barcode);

				if (returnShipment != null)
					return (isHandled: true, replacement: returnShipment);
			}

			if (HeaderView.Current.Mode == Modes.Pack)
			{
				SOShipment shipmentInTote =
					SelectFrom<SOShipment>.
					InnerJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Shipment>.
					InnerJoin<INTote>.On<SOPickerToShipmentLink.FK.Tote>.
					InnerJoin<SOPicker>.On<SOPickerToShipmentLink.FK.Picker>.
					InnerJoin<SOPickingWorksheet>.On<SOPicker.FK.Worksheet>.
					Where<
						INTote.toteCD.IsEqual<@P.AsString>.
						And<SOPickingWorksheet.worksheetType.IsEqual<SOPickingWorksheet.worksheetType.wave>>.
						And<SOPicker.confirmed.IsEqual<True>>.
						And<SOShipment.picked.IsEqual<True>>.
						And<SOShipment.confirmed.IsEqual<False>>.
						And<Not<Exists<
							SelectFrom<SOShipLineSplit>.
							Where<
								SOShipLineSplit.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
								And<SOShipLineSplit.packedQty.IsNotEqual<decimal0>>>>>>>.
					View.Select(Base, barcode).TopFirst;

				if (shipmentInTote != null)
					return (isHandled: true, replacement: shipmentInTote);
			}

			if (HeaderView.Current.Mode.IsIn(Modes.Pick, Modes.PickWave, Modes.PickBatch) && barcode.Contains("/"))
			{
				(string worksheetNbr, string pickerNbrStr) = barcode.Split('/');
				if (int.TryParse(pickerNbrStr, out int _))
				{
					if (ProcessWorksheetBarcode(barcode))
						return (isHandled: true, replacement: null);
				}
			}

			return (isHandled: false, replacement: null);
		}

		protected virtual void OnShipmentProcessed(SOShipment shipment)
		{
			if (HeaderView.Current.Mode == Modes.Pick)
			{
				if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
					SetScanState(ScanStates.Cart);
				else
					SetScanState(CanPick ? HeaderView.Current.IsReturnMode ? ScanStates.Item : ScanStates.Location : ScanStates.Command);
			}
			else if (HeaderView.Current.Mode == Modes.Pack)
			{
				SetScanState(CanPack || HasUnconfirmedBoxes ? ScanStates.Box : ScanStates.Command);
			}
			else if (HeaderView.Current.Mode == Modes.Ship)
			{
				SetScanState(ScanStates.Carrier);
				UpdateRates();
			}
		}


		protected virtual bool ProcessWorksheetBarcode(string barcode)
		{
			(string worksheetNbr, string pickerNbrStr) = barcode.Split('/');
			int pickerNbr = int.Parse(pickerNbrStr);

			var doc = (PXResult<SOPickingWorksheet, INSite, SOPicker>)
				SelectFrom<SOPickingWorksheet>.
				InnerJoin<INSite>.On<SOPickingWorksheet.FK.Site>.
				LeftJoin<SOPicker>.On<SOPicker.FK.Worksheet.And<SOPicker.pickerNbr.IsEqual<@P.AsInt>>>.
				Where<
					SOPickingWorksheet.worksheetNbr.IsEqual<@P.AsString>.
					And<Match<INSite, AccessInfo.userName.FromCurrent>>>.
				View.Select(Base, pickerNbr, worksheetNbr);

			if (doc == null)
			{
				ReportError(Msg.WorksheetNbrMissing, barcode);
				return false;
			}

			(SOPickingWorksheet worksheet, var _, SOPicker picker) = doc;

			if (worksheet.Status.IsNotIn(SOPickingWorksheet.status.Picking, SOPickingWorksheet.status.Open))
			{
				ReportError(Msg.WorksheetInvalid, worksheet.WorksheetNbr);
				return true;
			}

			if (HeaderView.Current.CartID != null && worksheet.SiteID != HeaderView.Current.SiteID)
			{
				ReportError(Msg.WorksheetInvalidSite, worksheet.WorksheetNbr);
				return true;
			}

			if (picker?.PickerNbr == null)
			{
				ReportError(Msg.PickerPositionMissing, pickerNbr, worksheetNbr);
				return true;
			}

			if (picker.UserID.IsNotIn(null, Base.Accessinfo.UserID))
			{
				ReportError(Msg.PickerPositionOccupied, pickerNbr, worksheetNbr);
				return true;
			}

			Base.Document.Current = null;
			Worksheet.Current = worksheet;
			Picker.Current = picker;

			HeaderSetter.Set(h => h.RefNbr, null);
			HeaderSetter.Set(h => h.WorksheetNbr, worksheet.WorksheetNbr);
			HeaderSetter.Set(h => h.PickerNbr, picker.PickerNbr);
			HeaderSetter.Set(h => h.SiteID, worksheet.SiteID);
			HeaderSetter.Set(h => h.TranDate, worksheet.PickDate);
			HeaderSetter.Set(h => h.NoteID, worksheet.NoteID);
			HeaderSetter.Set(h => h.InvtMult, (short)-1 /* Issue */);

			var newMode =
				worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Wave ? Modes.PickWave :
				worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Batch ? Modes.PickBatch :
				throw new NotSupportedException();

			if (HeaderView.Current.Mode != newMode)
				RedirectToMode(newMode);

			HeaderSetter.Set(h => h.CartID, picker.CartID); // set cartID only after redirect because redirect causes CartID get cleared

			Report(Msg.WorksheetReady, worksheet.WorksheetNbr);

			OnWorksheetProcessed(doc);
			return true;
		}

		protected virtual void OnWorksheetProcessed(PXResult<SOPickingWorksheet, INSite, SOPicker> pickerPosition)
		{
			if (HeaderView.Current.Mode == Modes.PickBatch && IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
				SetScanState(ScanStates.Cart);
			else
				SetScanState(
					HeaderView.Current.Mode == Modes.PickWave && ShipmentWithoutTote != null ? ScanStates.AssignTote :
					CanPickWS ? ScanStates.Location :
					ScanStates.Command);
		}


		protected override void ProcessCartBarcode(string barcode)
		{
			// for pick mode only
			if (HeaderView.Current.Mode.IsNotIn(Modes.Pick, Modes.PickBatch))
				throw new NotSupportedException();

			INCart cart = ReadCartByBarcode(barcode);
			if (cart == null)
			{
				ReportError(Msg.CartMissing, barcode);
			}
			else if (HeaderView.Current.RefNbr != null
				&& (HeaderView.Current.Mode == Modes.Pick && Shipment.SiteID != cart.SiteID
				|| HeaderView.Current.Mode == Modes.PickBatch && Worksheet.Current.SiteID != cart.SiteID))
			{
				ReportError(Msg.CartInvalidSite, barcode);
			}
			else
			{
				HeaderSetter.Set(h => h.CartID, cart.CartID);
				HeaderSetter.Set(h => h.SiteID, cart.SiteID);
				Cart.Current = Cart.Select();

				if (Picker.Current != null)
				{
					Picker.Current.CartID = cart.CartID;
					Picker.UpdateCurrent();
				}

				Report(Msg.CartReady, cart.CartCD);
				
				if (HeaderView.Current.Mode == Modes.Pick)
					SetScanState(CanPick ? HeaderView.Current.IsReturnMode ? ScanStates.Item : ScanStates.Location : ScanStates.Command);
				else if (HeaderView.Current.Mode == Modes.PickBatch)
					SetScanState(CanPickWS ? ScanStates.Location : ScanStates.Command);
			}
		}


		protected override void ProcessLocationBarcode(string barcode)
		{
			INLocation location = ReadLocationByBarcode(HeaderView.Current.SiteID, barcode);
			if (location == null)
				return;

			if (HeaderView.Current.Mode == Modes.Pick && !HeaderView.Current.IsReturnMode && Picked.SelectMain().All(t => t.LocationID != location.LocationID))
			{
				ReportError(Msg.LocationMissingInShipment, location.LocationCD);
				return;
			}

			if (HeaderView.Current.Mode == Modes.Pack && Picked.SelectMain().All(t => t.LocationID != location.LocationID))
			{
				ReportError(Msg.LocationMissingInShipment, location.LocationCD);
				return;
			}

			if (HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch) && PickListOfPicker.SelectMain().All(t => t.LocationID != location.LocationID))
			{
				ReportError(Msg.LocationMissingInPickList, location.LocationCD);
				return;
			}

			HeaderSetter.Set(h => h.LocationID, location.LocationID);
			Report(Msg.LocationReady, location.LocationCD);

			OnLocationProcessed(location);
		}

		protected virtual void OnLocationProcessed(INLocation location)
		{
			if (HeaderView.Current.Mode == Modes.Pick)
				SetScanState(HeaderView.Current.IsReturnMode ? ScanStates.Confirm : ScanStates.Item);
			else if (HeaderView.Current.Mode == Modes.Pack)
				SetScanState(ScanStates.Item);
			else if (HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch))
				SetScanState(ScanStates.Item);
		}


		protected virtual void ProcessSortingLocationBarcode(string barcode)
		{
			INLocation location = ReadLocationByBarcode(HeaderView.Current.SiteID, barcode);
			if (location == null)
				return;
			if (location.IsSorting == false)
			{
				ReportError(Msg.SortingLocationMissing, barcode);
				return;
			}

			ConfirmPickList(location.LocationID.Value);
		}

		protected virtual void ProcessAssignToteBarcode(string barcode)
		{
			INTote tote = INTote.UK.Find(Base, HeaderView.Current.SiteID, barcode);

			if (tote == null)
			{
				if (TryAssignTotesFromCart(barcode) == false)
					ReportError(Msg.ToteMissing, barcode);
				return;
			}

			if (tote.Active == false)
			{
				ReportError(Msg.ToteInactive, tote.ToteCD);
				return;
			}

			if (tote.AssignedCartID != null)
			{
				ReportError(Msg.AssignedToteCannotBeUsedSeparatly, tote.ToteCD);
				return;
			}

			bool toteIsBusy =
				SelectFrom<SOPickerToShipmentLink>.
				InnerJoin<INTote>.On<SOPickerToShipmentLink.FK.Tote>.
				InnerJoin<SOShipment>.On<SOPickerToShipmentLink.FK.Shipment>.
				Where<
					INTote.siteID.IsEqual<@P.AsInt>.
					And<INTote.toteID.IsEqual<@P.AsInt>>.
					And<SOShipment.confirmed.IsEqual<False>>>.
				View.Select(Base, tote.SiteID, tote.ToteID).Any();
			if (toteIsBusy)
			{
				ReportError(Msg.ToteBusy, tote.ToteCD);
				return;
			}

			var shipmentWithoutTote = ShipmentWithoutTote;
			shipmentWithoutTote.ToteID = tote.ToteID;
			ShipmentsOfPicker.Update(shipmentWithoutTote);

			if (Picker.Current.UserID == null)
			{
				Picker.Current.UserID = Base.Accessinfo.UserID;
				Picker.UpdateCurrent();
			}
			SaveChanges();

			Report(Msg.ToteAssigned, tote.ToteCD, shipmentWithoutTote.ShipmentNbr);

			if (ShipmentWithoutTote != null)
				SetScanState(ScanStates.AssignTote); // set the same state to change the prompt message (see ApplyState method)
			else
				SetScanState(ScanStates.Location);
		}

		protected virtual bool TryAssignTotesFromCart(string barcode)
		{
			INCart cart = ReadCartByBarcode(barcode);
			if (cart == null)
				return false;

			var shipmentsOfPicker = ShipmentsOfPicker.SelectMain();
			if (shipmentsOfPicker.Any(s => s.ToteID != null))
			{
				ReportError(Msg.ToteAlreadyAssignedCannotAssignCart, cart.CartCD);
				return true;
			}

			bool cartIsBusy =
				SelectFrom<SOPickerToShipmentLink>.
				InnerJoin<SOPickingWorksheet>.On<SOPickerToShipmentLink.FK.Worksheet>.
				InnerJoin<INTote>.On<SOPickerToShipmentLink.FK.Tote>.
				InnerJoin<INCart>.On<INTote.FK.Cart>.
				InnerJoin<SOShipment>.On<SOPickerToShipmentLink.FK.Shipment>.
				Where<
					SOPickingWorksheet.worksheetType.IsEqual<SOPickingWorksheet.worksheetType.wave>.
					And<SOShipment.confirmed.IsEqual<False>>.
					And<INCart.siteID.IsEqual<@P.AsInt>>.
					And<INCart.cartID.IsEqual<@P.AsInt>>>.
				View.ReadOnly.Select(Base, cart.SiteID, cart.CartID).Any();
			if (cartIsBusy)
			{
				ReportError(Msg.CartIsOccupied, cart.CartCD);
				return true;
			}

			var totes = INTote.FK.Cart.SelectChildren(Base, cart).Where(t => t.Active == true).ToArray();
			if (shipmentsOfPicker.Length > totes.Length)
			{
				ReportError(Msg.TotesAreNotEnoughInCart, cart.CartCD);
				return true;
			}

			foreach (var (link, tote) in shipmentsOfPicker.Zip(totes, (link, tote) => (link, tote)))
			{
				link.ToteID = tote.ToteID;
				ShipmentsOfPicker.Update(link);
			}
			Picker.Current.CartID = cart.CartID;
			Picker.UpdateCurrent();

			SaveChanges();

			HeaderView.Current.CartID = cart.CartID;
			Cart.Current = Cart.Select();

			Report(Msg.TotesFromCartAreAssigned, shipmentsOfPicker.Length, cart.CartCD);

			SetScanState(ScanStates.Location);
			return true;
		}

		protected virtual void ProcessConfirmToteBarcode(string barcode)
		{
			INTote tote = INTote.UK.Find(Base, HeaderView.Current.SiteID, barcode);

			if (tote == null)
			{
				ReportError(Msg.ToteMissing, barcode);
				return;
			}

			if (tote.Active == false)
			{
				ReportError(Msg.ToteInactive, tote.ToteCD);
				return;
			}

			var selectedSplit = GetSelectedPickListEntry(HeaderView.Current);
			INTote properTote = GetToteForPickListEntry(selectedSplit);
			if (properTote.ToteID != tote.ToteID)
			{
				ReportError(Msg.ToteMismatch, tote.ToteCD);
				return;
			}

			SetScanState(ScanStates.Confirm);
		}

		protected virtual void ProcessRemoveFromToteBarcode(string barcode)
		{
			INTote tote = INTote.UK.Find(Base, HeaderView.Current.SiteID, barcode);

			if (tote == null)
			{
				ReportError(Msg.ToteMissing, barcode);
				return;
			}

			if (tote.Active == false)
			{
				ReportError(Msg.ToteInactive, tote.ToteCD);
				return;
			}

			HeaderSetter.Set(h => h.RemoveFromToteID, tote.ToteID);

			SetScanState(ScanStates.Location);
		}

		protected INTote GetToteForPickListEntry(SOPickerListEntry selectedSplit)
		{
			if (selectedSplit == null)
				return null;

			INTote tote =
				SelectFrom<INTote>.
				InnerJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Tote>.
				Where<
					SOPickerToShipmentLink.worksheetNbr.IsEqual<@P.AsString>.
					And<SOPickerToShipmentLink.pickerNbr.IsEqual<@P.AsInt>>.
					And<SOPickerToShipmentLink.shipmentNbr.IsEqual<@P.AsString>>>.
				View.Select(Base, selectedSplit.WorksheetNbr, selectedSplit.PickerNbr, selectedSplit.ShipmentNbr);
			return tote;
		}


		protected override void ProcessItemBarcode(string barcode)
		{
			var item = ReadItemByBarcode(barcode, INPrimaryAlternateType.CPN);
			if (item == null)
			{
				(bool isHandled, var replacement) = HandleItemAbsence(barcode);
				if (isHandled)
				{
					if (replacement != null)
						item = replacement;
					else
						return; // absence was handled but item replacement was not provided - assume that some other type of barcode was processed
				}
				else
				{
					ReportError(Msg.InventoryMissing, barcode);
					return;
				}
			}

			(INItemXRef xref, InventoryItem inventoryItem, var _, INLotSerClass lsclass) = item;

			if (HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch) && PickListOfPicker.SelectMain().All(t => t.InventoryID != inventoryItem.InventoryID))
			{
				ReportError(Msg.InventoryMissingInPickList, inventoryItem.InventoryCD);
				return;
			}

			if (HeaderView.Current.Mode == Modes.Pick && Picked.SelectMain().All(t => t.InventoryID != inventoryItem.InventoryID))
			{
				ReportError(Msg.InventoryMissingInShipment, inventoryItem.InventoryCD);
				return;
			}

			if (HeaderView.Current.Mode == Modes.Pack && Picked.SelectMain().All(t => t.InventoryID != inventoryItem.InventoryID))
			{
				ReportError(Msg.InventoryMissingInShipment, inventoryItem.InventoryCD);
				return;
			}

			if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				if (xref.UOM.IsNotIn(null, inventoryItem.BaseUnit))
				{
					ReportError(Msg.SerialItemNotComplexQty);
					return;
				}
			}

			HeaderSetter.Set(h => h.InventoryID, xref.InventoryID);
			HeaderSetter.Set(h => h.SubItemID, xref.SubItemID);

			if (HeaderView.Current.UOM == null)
				HeaderSetter.Set(h => h.UOM,
					lsclass.LotSerTrack == INLotSerTrack.SerialNumbered
						? xref.UOM ?? inventoryItem.BaseUnit
						: xref.UOM ?? inventoryItem.SalesUnit);

			HeaderSetter.Set(h => h.LotSerTrack, lsclass.LotSerTrack);
			HeaderSetter.Set(h => h.LotSerAssign, lsclass.LotSerAssign);
			HeaderSetter.Set(h => h.LotSerTrackExpiration, lsclass.LotSerTrackExpiration);
			HeaderSetter.Set(h => h.LotSerIssueMethod, lsclass.LotSerIssueMethod);
			HeaderSetter.Set(h => h.AutoNextNbr, lsclass.AutoNextNbr);

			Report(Msg.InventoryReady, inventoryItem.InventoryCD);

			OnItemProcessed(item);
		}

		protected virtual (bool isHandled, PXResult<INItemXRef, InventoryItem, INSubItem, INLotSerClass> replacement) HandleItemAbsence(string barcode)
		{
			bool tryLocation = !PromptLocationForEveryLine;
			if (tryLocation)
				tryLocation &=
					HeaderView.Current.Mode == Modes.Pick ? IsCartRequired(HeaderView.Current).Implies(HeaderView.Current.CartLoaded != true) && !HeaderView.Current.IsReturnMode :
					HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch) ? IsCartRequired(HeaderView.Current).Implies(HeaderView.Current.CartLoaded != true) :
					HeaderView.Current.Mode == Modes.Pack ? IsLocationRequiredInPack(HeaderView.Current) :
					false;

			if (tryLocation)
			{
				ProcessLocationBarcode(barcode);
				if (Info.Current.MessageType == WMSMessageTypes.Information)
					return (isHandled: true, replacement: null); // location found
			}

			if (HeaderView.Current.Mode == Modes.Pack && HeaderView.Current.Remove == false)
			{
				CSBox box = CSBox.PK.Find(Base, barcode);
				if (box != null)
				{
					if (!AutoConfirmPackage(Setup.Current.ConfirmEachPackageWeight == false))
						return (isHandled: true, replacement: null);

					ProcessBoxBarcode(barcode);
					if (Info.Current.MessageType == WMSMessageTypes.Information)
						return (isHandled: true, replacement: null); // box processed
				}
			}

			return (isHandled: false, replacement: null);
		}

		protected virtual void OnItemProcessed(PXResult<INItemXRef, InventoryItem, INSubItem, INLotSerClass> item)
		{
			if (HeaderView.Current.HasLotSerial && (DefaultLotSerial == false || HeaderView.Current.IsEntarableLotSerial)) // TODO: make same as for putaway
				SetScanState(ScanStates.LotSerial);
			else if (HeaderView.Current.Mode == Modes.Pick)
				SetScanState(HeaderView.Current.IsReturnMode ? ScanStates.Location : ScanStates.Confirm);
			else if (HeaderView.Current.Mode == Modes.PickWave)
				SetScanState(ConfirmToteForEveryLine ? ScanStates.ConfirmTote : ScanStates.Confirm);
			else if (HeaderView.Current.Mode == Modes.PickBatch)
				SetScanState(ScanStates.Confirm);
			else if (HeaderView.Current.Mode == Modes.Pack)
				SetScanState(ScanStates.Confirm);
		}


		protected override void ProcessLotSerialBarcode(string barcode)
		{
			string lotSerialNbr = barcode;

			if (IsValid<Header.lotSerialNbr>(lotSerialNbr, out string error) == false)
			{
				ReportError(error);
				return;
			}

			if (HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch) && !HeaderView.Current.IsEntarableLotSerial && PickListOfPicker.SelectMain().All(t => t.LotSerialNbr != lotSerialNbr))
			{
				ReportError(Msg.LotSerialMissingInPickList, lotSerialNbr);
				return;
			}

			if (HeaderView.Current.Mode == Modes.Pick && !HeaderView.Current.IsEntarableLotSerial && Picked.SelectMain().All(t => t.LotSerialNbr != lotSerialNbr))
			{
				ReportError(Msg.LotSerialMissingInShipment, lotSerialNbr);
				return;
			}

			if (HeaderView.Current.Mode == Modes.Pack && !HeaderView.Current.IsEntarableLotSerial && Picked.SelectMain().All(t => t.LotSerialNbr != lotSerialNbr))
			{
				ReportError(Msg.LotSerialMissingInShipment, lotSerialNbr);
				return;
			}

			HeaderSetter.Set(h => h.LotSerialNbr, lotSerialNbr);
			Report(Msg.LotSerialReady, lotSerialNbr);

			OnLotSerialProcessed(lotSerialNbr);
		}

		protected virtual void OnLotSerialProcessed(string lotSerialNbr)
		{
			bool entarableExpirationDate =
				HeaderView.Current.Remove == false
				&& HeaderView.Current.IsEntarableLotSerial
				&& HeaderView.Current.LotSerTrackExpiration == true;

			if (HeaderView.Current.Mode == Modes.Pick)
			{
				if (entarableExpirationDate && Picked.SelectMain().Any(t => t.IsUnassigned == true || t.LotSerialNbr == HeaderView.Current.LotSerialNbr && t.PickedQty == 0)) // TODO: make same as for Receive
					SetScanState(ScanStates.ExpireDate);
				else
					SetScanState(HeaderView.Current.IsReturnMode ? ScanStates.Location : ScanStates.Confirm);
			}
			else if (HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch))
			{
				if (entarableExpirationDate && PickListOfPicker.SelectMain().Any(t => /*t.IsUnassigned == true ||*/ t.LotSerialNbr == HeaderView.Current.LotSerialNbr && t.PickedQty == 0))
					SetScanState(ScanStates.ExpireDate);
				else
					SetScanState(ConfirmToteForEveryLine ? ScanStates.ConfirmTote : ScanStates.Confirm);
			}
			else if (HeaderView.Current.Mode == Modes.Pack)
			{
				if (NoPick && entarableExpirationDate && Picked.SelectMain().Any(t => t.IsUnassigned == true || t.LotSerialNbr == HeaderView.Current.LotSerialNbr && t.PackedQty == 0))
					SetScanState(ScanStates.ExpireDate);
				else
					SetScanState(ScanStates.Confirm);
			}
		}


		protected override void ProcessExpireDate(string barcode)
		{
			DateTime? expireDate = DateTime.TryParse(barcode.Trim(), out DateTime ed) ? ed : (DateTime?)null;
			if (expireDate == null)
			{
				ReportError(Msg.LotSerialExpireDateBadFormat);
				return;
			}

			if (IsValid<Header.expireDate>(expireDate, out string error) == false)
			{
				ReportError(error);
				return;
			}

			HeaderSetter.Set(h => h.ExpireDate, expireDate);
			Report(Msg.LotSerialExpireDateReady, expireDate);

			OnExpireDateProcessed(expireDate);
		}

		protected virtual void OnExpireDateProcessed(DateTime? expireDate)
		{
			if (HeaderView.Current.Mode == Modes.Pick)
				SetScanState(HeaderView.Current.IsReturnMode ? ScanStates.Location : ScanStates.Confirm);
			else if (HeaderView.Current.Mode == Modes.PickWave)
				SetScanState(ConfirmToteForEveryLine ? ScanStates.ConfirmTote : ScanStates.Confirm);
			else if (HeaderView.Current.Mode == Modes.PickBatch)
				SetScanState(ScanStates.Confirm);
			else if (HeaderView.Current.Mode == Modes.Pack && NoPick)
				SetScanState(ScanStates.Confirm);
		}


		protected virtual void ProcessBoxBarcode(string barcode)
		{
			CSBox box = CSBox.PK.Find(Base, barcode);
			if (box == null)
			{
				ReportError(Msg.BoxMissing, barcode);
				return;
			}

			SOPackageDetailEx package = Base.Packages.SelectMain().FirstOrDefault(p => string.Equals(p.BoxID.Trim(), barcode.Trim(), StringComparison.OrdinalIgnoreCase) && p.Confirmed == false);
			if (package == null)
			{
				package = (SOPackageDetailEx)Base.Packages.Cache.CreateInstance();
				package.BoxID = box.BoxID;
				package.ShipmentNbr = HeaderView.Current.RefNbr;
				package = Base.Packages.Insert(package);
				Save.Press();
			}

			HeaderSetter.WithEventFiring.Set(h => h.PackageLineNbr, package.LineNbr);
			Base.Packages.Current = package;
			Report(Msg.BoxReady, box.BoxID);

			OnBoxProcessed(box);
		}

		protected virtual void OnBoxProcessed(CSBox box)
		{
			if (CanPack || HeaderView.Current.Remove == true)
				SetScanState(IsLocationRequiredInPack(HeaderView.Current) ? ScanStates.Location : ScanStates.Item);
			else
			{
				SetScanState(ScanStates.Command);
				if (HasUnconfirmedBoxes)
					Prompt(Msg.BoxConfirmPrompt);
			}
		}


		protected virtual void ProcessBoxWeightBarcode(string barcode)
		{
			if (decimal.TryParse(barcode, out decimal value))
				SetPackageWeight(Math.Abs(value));
			else
				ReportError(Msg.QtyBadFormat);
		}
		#endregion

		#region Pick line confirmation
		protected virtual bool ConfirmPicked()
		{
			var isQtyOverridable = IsQtyOverridable(HeaderView.Current.LotSerTrack);

			if (HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch))
			{
				WSConfirmPickedNoCart();
			}
			else if (HeaderView.Current.Mode == Modes.Pick)
			{
				if (IsCartRequired(HeaderView.Current) == false)
					ConfirmPickedNoCart();
				else if (HeaderView.Current.CartLoaded == false)
					ConfirmPickedInCart();
				else
					ConfirmPickedOutCart();
			}
			else throw new NotSupportedException();

			if (isQtyOverridable && Info.Current.MessageType == WMSMessageTypes.Information)
				HeaderSetter.Set(x => x.IsQtyOverridable, true);

			return true;
		}

		protected virtual WMSFlowStatus ConfirmPickedNoCart()
		{
			WMSFlowStatus Implementation()
			{
				Header header = HeaderView.Current;
				bool remove = header.Remove == true;

				var pickedSplit = Picked
					.SelectMain()
					.Where(r => IsSelectedSplit(r, header))
					.OrderByDescending(split => split.IsUnassigned == false && remove ? split.PickedQty > 0 : split.Qty > split.PickedQty)
					.OrderByDescending(split => remove ? split.PickedQty > 0 : split.Qty > split.PickedQty)
					.ThenByDescending(split => split.LotSerialNbr == (header.LotSerialNbr ?? split.LotSerialNbr))
					.ThenByDescending(split => string.IsNullOrEmpty(split.LotSerialNbr))
					.ThenByDescending(split => (split.Qty > split.PickedQty || remove) && split.PickedQty > 0)
					.ThenByDescending(split => Sign.MinusIf(remove) * (split.Qty - split.PickedQty))
					.FirstOrDefault();
				if (pickedSplit == null)
					return WMSFlowStatus.Fail(remove ? Msg.NothingToRemove : Msg.NothingToPick).ClearIsNeeded;

				decimal qty = header.GetBaseQty(Base);
				decimal threshold = Base.GetQtyThreshold(pickedSplit);

				if (qty != 0)
				{
					bool splitUpdated = false;

					if (remove)
					{
						if (pickedSplit.PickedQty - qty < 0)
							return WMSFlowStatus.Fail(Msg.Underpicking);
						if (pickedSplit.PickedQty - qty < pickedSplit.PackedQty)
							return WMSFlowStatus.Fail(Msg.UnderpickingByPack);
					}
					else
					{
						if (pickedSplit.PickedQty + qty > pickedSplit.Qty * threshold)
							return WMSFlowStatus.Fail(Msg.Overpicking);

						if (pickedSplit.LotSerialNbr != header.LotSerialNbr && header.IsEntarableLotSerial)
						{
							if (!SetLotSerialNbrAndQty(header, pickedSplit, qty))
								return WMSFlowStatus.Fail(Msg.Overpicking);
							splitUpdated = true;
						}
					}

					if (!splitUpdated)
					{
						EnsureAssignedSplitEditing(pickedSplit);

						if (header.IsReturnMode && header.Remove == false && header.LocationID != null)
							pickedSplit.LocationID = header.LocationID;
						pickedSplit.PickedQty += remove ? -qty : qty;

						if (remove && header.InvtMult == -1 && header.IsEntarableLotSerial)
						{
							if (pickedSplit.PickedQty + qty == pickedSplit.Qty)
								pickedSplit.Qty = pickedSplit.PickedQty;

							if (pickedSplit.Qty == 0)
								Picked.Delete(pickedSplit);
							else
								Picked.Update(pickedSplit);
						}
						else
							Picked.Update(pickedSplit);
					}
				}

				EnsureShipmentUserLink();

				if (CanPick)
					SetScanState(
						header.IsReturnMode || PromptLocationForEveryLine == false
							? ScanStates.Item
							: ScanStates.Location,
						remove
							? Msg.InventoryRemoved
							: Msg.InventoryAdded,
						InventoryItem.PK.Find(Base, header.InventoryID.Value).InventoryCD, header.Qty, header.UOM);
				else
					SetScanState(ScanStates.Command, Msg.ShipmentPicked, header.RefNbr);

				return WMSFlowStatus.Ok;
			}

			return ExecuteAndCompleteFlow(Implementation);
		}

		protected virtual WMSFlowStatus ConfirmPickedInCart()
		{
			WMSFlowStatus Implementation()
			{
				Header header = HeaderView.Current;
				bool remove = header.Remove == true;

				var pickedSplit = Picked
					.SelectMain()
					.Where(r => IsSelectedSplit(r, header))
					.OrderByDescending(split => split.IsUnassigned == false && remove ? split.PickedQty > 0 : split.Qty > split.PickedQty)
					.OrderByDescending(split => remove ? split.PickedQty > 0 : split.Qty > split.PickedQty)
					.ThenByDescending(split => split.LotSerialNbr == (header.LotSerialNbr ?? split.LotSerialNbr))
					.ThenByDescending(split => string.IsNullOrEmpty(split.LotSerialNbr))
					.ThenByDescending(split => (split.Qty > split.PickedQty || remove) && split.PickedQty > 0)
					.ThenByDescending(split => Sign.MinusIf(remove) * (split.Qty - split.PickedQty))
					.FirstOrDefault();
				if (pickedSplit == null)
					return WMSFlowStatus.Fail(remove ? Msg.NothingToRemove : Msg.NothingToPick).ClearIsNeeded;

				decimal qty = header.GetBaseQty(Base);
				decimal threshold = Base.GetQtyThreshold(pickedSplit);

				if (qty != 0)
				{
					if (!remove && pickedSplit.PickedQty + GetOverallCartQty(pickedSplit) + qty > pickedSplit.Qty * threshold)
						return WMSFlowStatus.Fail(Msg.Overpicking);
					if ( remove && GetCartQty(pickedSplit) < qty)
						return WMSFlowStatus.Fail(Msg.CartUnderpicking);

					try
					{
						WMSFlowStatus cartStatus = SyncWithCart(header, pickedSplit, Sign.MinusIf(remove) * qty);
						if (cartStatus.IsError != false)
							return cartStatus;
					}
					finally
					{
						EnsureCartShipmentLink();
					}
				}

				EnsureShipmentUserLink();

				if (CanPick)
					SetScanState(
						header.IsReturnMode || PromptLocationForEveryLine == false
							? ScanStates.Item
							: ScanStates.Location,
						remove
							? Msg.InventoryRemoved
							: Msg.InventoryAdded,
						InventoryItem.PK.Find(Base, header.InventoryID.Value).InventoryCD, header.Qty, header.UOM);
				else
					SetScanState(ScanStates.Command, Msg.ShipmentPicked, header.RefNbr);

				return WMSFlowStatus.Ok;
			}

			return ExecuteAndCompleteFlow(Implementation);
		}

		protected virtual WMSFlowStatus ConfirmPickedOutCart()
		{
			WMSFlowStatus Implementation()
			{
				Header header = HeaderView.Current;
				bool remove = header.Remove == true;

				var pickedSplits =
					Picked.SelectMain().Where(
						r => r.InventoryID == header.InventoryID
							&& r.SubItemID == header.SubItemID
							&& r.LotSerialNbr == (header.LotSerialNbr ?? r.LotSerialNbr)
							&& (remove ? r.PickedQty > 0 : GetCartQty(r) > 0));
				if (pickedSplits.Any() == false)
					return WMSFlowStatus.Fail(remove ? Msg.NothingToRemove : Msg.NothingToPick).ClearIsNeeded;

				decimal qty = Sign.MinusIf(remove) * header.GetBaseQty(Base);
				if (qty != 0)
				{
					if (remove)
					{
						if (pickedSplits.Sum(_ => _.PickedQty) < -qty)
							return WMSFlowStatus.Fail(Msg.Underpicking);

						if (pickedSplits.Sum(_ => _.PickedQty - _.PackedQty) < -qty)
							return WMSFlowStatus.Fail(Msg.UnderpickingByPack);
					}
					else
					{
						if (pickedSplits.Sum(_ => _.Qty * Base.GetQtyThreshold(_) - _.PickedQty) < qty)
							return WMSFlowStatus.Fail(Msg.Overpicking);

						if (pickedSplits.Sum(_ => GetCartQty(_)) < qty)
							return WMSFlowStatus.Fail(Msg.CartUnderpicking);
					}

					try
					{
						decimal unassignedQty = qty;
						foreach (var pickedSplit in remove ? pickedSplits.Reverse() : pickedSplits)
						{
							EnsureAssignedSplitEditing(pickedSplit);

							decimal currentQty = remove
								? -Math.Min(pickedSplit.PickedQty.Value, -unassignedQty)
								: Math.Min(GetCartQty(pickedSplit), unassignedQty);

							if (currentQty == 0)
								continue;

							WMSFlowStatus cartStatus = SyncWithCart(header, pickedSplit, -currentQty);
							if (cartStatus.IsError != false)
								return cartStatus;

							pickedSplit.PickedQty += currentQty;
							Picked.Update(pickedSplit);

							unassignedQty -= currentQty;
							if (unassignedQty == 0)
								break;
						}
					}
					finally
					{
						EnsureCartShipmentLink();
					}
				}

				EnsureShipmentUserLink();

				if (CanPick == false)
					SetScanState(ScanStates.Command, Msg.ShipmentPicked, header.RefNbr);
				else if (CartSplits.SelectMain().Any())
					SetScanState(
						ScanStates.Item,
						remove
							? Msg.InventoryRemoved
							: Msg.InventoryAdded,
						InventoryItem.PK.Find(Base, header.InventoryID.Value).InventoryCD, header.Qty, header.UOM);
				else
				{
					header.CartLoaded = false;
					SetScanState(ScanStates.Location, Msg.CartIsEmpty, INCart.PK.Find(Base, header.SiteID, header.CartID).CartCD);
				}

				return WMSFlowStatus.Ok;
			}

			return ExecuteAndCompleteFlow(Implementation);
		}


		protected virtual WMSFlowStatus WSConfirmPickedNoCart()
		{
			WMSFlowStatus Implementation()
			{
				Header header = HeaderView.Current;
				bool remove = header.Remove == true;

				SOPickerListEntry pickedSplit = GetSelectedPickListEntry(header);
				if (pickedSplit == null)
					return WMSFlowStatus.Fail(remove ? Msg.NothingToRemove : Msg.NothingToPick).ClearIsNeeded;

				if (!remove && header.IsEntarableLotSerial && header.LotSerTrack == INLotSerTrack.SerialNumbered && header.LotSerialNbr != null)
				{
					bool existingSerial =
						SelectFrom<SOPickerListEntry>.
						Where<
							SOPickerListEntry.inventoryID.IsEqual<@P.AsInt>.
							And<SOPickerListEntry.subItemID.IsEqual<@P.AsInt>>.
							And<SOPickerListEntry.lotSerialNbr.IsEqual<@P.AsString>>>.
						View.Select(Base, header.InventoryID, header.SubItemID, header.LotSerialNbr).Any();

					if (existingSerial == false)
					{
						existingSerial |=
							SelectFrom<INItemLotSerial>.
							Where<
								INItemLotSerial.inventoryID.IsEqual<@P.AsInt>.
								And<INItemLotSerial.lotSerialNbr.IsEqual<@P.AsString>>>.
							View.Select(Base, header.InventoryID, header.LotSerialNbr).Any();
					}

					if (existingSerial)
						return
							WMSFlowStatus.Fail(
								IN.Messages.SerialNumberAlreadyIssued,
								header.LotSerialNbr,
								InventoryItem.PK.Find(Base, header.InventoryID).InventoryCD);
				}

				decimal qty = header.GetBaseQty(Base);
				//decimal threshold = Base.GetQtyThreshold(pickedSplit);

				if (qty != 0)
				{
					bool splitUpdated = false;

					if (remove)
					{
						if (pickedSplit.PickedQty - qty < 0)
							return WMSFlowStatus.Fail(Msg.Underpicking);
					}
					else
					{
						if (pickedSplit.PickedQty + qty > pickedSplit.Qty/* * threshold*/)
							return WMSFlowStatus.Fail(Msg.Overpicking);

						if (pickedSplit.LotSerialNbr != header.LotSerialNbr && header.IsEntarableLotSerial)
						{
							if (!SetLotSerialNbrAndQty(header, pickedSplit, qty))
								return WMSFlowStatus.Fail(Msg.Overpicking);
							splitUpdated = true;
						}
					}

					if (Picker.Current.UserID == null)
					{
						Picker.Current.UserID = Base.Accessinfo.UserID;
						Picker.UpdateCurrent();
					}

					if (SOPickingWorksheet.PK.Find(Base, Worksheet.Current).Status == SOPickingWorksheet.status.Open)
					{
						Worksheet.Current.Status = SOPickingWorksheet.status.Picking;
						Worksheet.UpdateCurrent();
					}

					if (!splitUpdated)
					{
						//EnsureAssignedSplitEditing(pickedSplit);

						pickedSplit.PickedQty += remove ? -qty : qty;

						if (remove && header.IsEntarableLotSerial)
						{
							if (pickedSplit.PickedQty + qty == pickedSplit.Qty)
							{
								if (pickedSplit.PickedQty == 0)
								{
									PickListOfPicker.Delete(pickedSplit);
								}
								else
								{
									pickedSplit.Qty = pickedSplit.PickedQty;
									PickListOfPicker.Update(pickedSplit);
								}
							}
						}
						else
							PickListOfPicker.Update(pickedSplit);
					}
				}

				if (header.CartID != null)
				{
					WMSFlowStatus cartStatus = SyncWithCart(header, pickedSplit, Sign.MinusIf(remove) * qty);
					if (cartStatus.IsError != false)
						return cartStatus; 
				}

				bool wave = Worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Wave;
				INTote targetTote = GetToteForPickListEntry(pickedSplit);

				if (CanPickWS)
					SetScanState(
						PromptLocationForEveryLine == false
							? ScanStates.Item
							: ScanStates.Location,
						remove
							? wave ? Msg.InventoryRemovedFromTote : Msg.InventoryRemoved
							: wave ? Msg.InventoryAddedToTote : Msg.InventoryAdded,
						InventoryItem.PK.Find(Base, header.InventoryID.Value).InventoryCD, header.Qty, header.UOM, targetTote?.ToteCD);
				else
					SetScanState(ScanStates.Command, Msg.PickListPicked, header.WorksheetNbr);

				return WMSFlowStatus.Ok;
			}

			return ExecuteAndCompleteFlow(Implementation);
		}

		protected virtual SOPickerListEntry GetSelectedPickListEntry(Header header)
		{
			bool remove = header.Remove == true;
			var pickedSplit = PickListOfPicker
				.Select().AsEnumerable()
				.With(view => remove && header.RemoveFromToteID != null
					? view.Where(e => e.GetItem<SOPickerToShipmentLink>().ToteID == header.RemoveFromToteID)
					: view)
				.Select(row =>
				(
					Split: row.GetItem<SOPickerListEntry>(),
					Location: row.GetItem<INLocation>()
				))
				.Where(r => IsSelectedSplit(r.Split, header))
				.OrderByDescending(r => r.Split.IsUnassigned == false && remove ? r.Split.PickedQty > 0 : r.Split.Qty > r.Split.PickedQty)
				.ThenByDescending(r => remove ? r.Split.PickedQty > 0 : r.Split.Qty > r.Split.PickedQty)
				.ThenByDescending(r => r.Split.LotSerialNbr == (header.LotSerialNbr ?? r.Split.LotSerialNbr))
				.ThenByDescending(r => string.IsNullOrEmpty(r.Split.LotSerialNbr))
				.ThenByDescending(r => (r.Split.Qty > r.Split.PickedQty || remove) && r.Split.PickedQty > 0)
				.ThenBy(r => Sign.MinusIf(remove) * r.Location.PathPriority)
				.With(view => remove
					? view.ThenByDescending(r => r.Location.LocationCD)
					: view.ThenBy(r => r.Location.LocationCD))
				.ThenByDescending(r => Sign.MinusIf(remove) * (r.Split.Qty - r.Split.PickedQty))
				.Select(r => r.Split)
				.FirstOrDefault();
			return pickedSplit;
		}

		#region Cart sync
		protected virtual WMSFlowStatus SyncWithCart(Header header, SOShipLineSplit pickedSplit, decimal qty)
		{
			INCartSplit[] linkedSplits =
				PXSelectJoin<SOShipmentSplitToCartSplitLink,
				InnerJoin<INCartSplit, On<SOShipmentSplitToCartSplitLink.FK.CartSplit>>,
				Where2<SOShipmentSplitToCartSplitLink.FK.ShipmentSplitLine.SameAsCurrent,
					And<SOShipmentSplitToCartSplitLink.siteID, Equal<Required<Header.siteID>>,
					And<SOShipmentSplitToCartSplitLink.cartID, Equal<Required<Header.cartID>>>>>>
				.SelectMultiBound(Base, new object[] { pickedSplit }, header.SiteID, header.CartID)
				.RowCast<INCartSplit>()
				.ToArray();

			INCartSplit[] appropriateSplits =
				PXSelect<INCartSplit,
				Where<INCartSplit.cartID, Equal<Required<Header.cartID>>,
					And<INCartSplit.inventoryID, Equal<Current<SOShipLineSplit.inventoryID>>,
					And<INCartSplit.subItemID, Equal<Current<SOShipLineSplit.subItemID>>,
					And<INCartSplit.siteID, Equal<Current<SOShipLineSplit.siteID>>,
					And<INCartSplit.fromLocationID, Equal<Current<SOShipLineSplit.locationID>>,
					And<INCartSplit.lotSerialNbr, Equal<Current<SOShipLineSplit.lotSerialNbr>>>>>>>>>
				.SelectMultiBound(Base, new object[] { pickedSplit }, header.CartID)
				.RowCast<INCartSplit>()
				.ToArray();

			INCartSplit[] existingINSplits = linkedSplits.Concat(appropriateSplits).ToArray();

			INCartSplit cartSplit = existingINSplits.FirstOrDefault(s => s.LotSerialNbr == (header.LotSerialNbr ?? s.LotSerialNbr));
			if (cartSplit == null)
			{
				cartSplit = CartSplits.Insert(new INCartSplit
				{
					CartID = header.CartID,
					InventoryID = pickedSplit.InventoryID,
					SubItemID = pickedSplit.SubItemID,
					LotSerialNbr = pickedSplit.LotSerialNbr,
					ExpireDate = pickedSplit.ExpireDate,
					UOM = pickedSplit.UOM,
					SiteID = pickedSplit.SiteID,
					FromLocationID = pickedSplit.LocationID,
					Qty = qty
				});
			}
			else
			{
				cartSplit.Qty += qty;
				cartSplit = CartSplits.Update(cartSplit);
			}

			if (cartSplit.Qty == 0)
			{
				CartSplits.Delete(cartSplit);
				return WMSFlowStatus.Ok;
			}
			else
				return EnsureShipmentCartSplitLink(pickedSplit, cartSplit, qty);
		}

		protected virtual WMSFlowStatus EnsureShipmentCartSplitLink(SOShipLineSplit soSplit, INCartSplit cartSplit, decimal deltaQty)
		{
			var allLinks =
				PXSelect<SOShipmentSplitToCartSplitLink,
				Where2<SOShipmentSplitToCartSplitLink.FK.CartSplit.SameAsCurrent,
					Or<SOShipmentSplitToCartSplitLink.FK.ShipmentSplitLine.SameAsCurrent>>>
				.SelectMultiBound(Base, new object[] { cartSplit, soSplit })
				.RowCast<SOShipmentSplitToCartSplitLink>()
				.ToArray();

			SOShipmentSplitToCartSplitLink currentLink = allLinks.FirstOrDefault(
				link => SOShipmentSplitToCartSplitLink.FK.CartSplit.Match(Base, cartSplit, link)
					&& SOShipmentSplitToCartSplitLink.FK.ShipmentSplitLine.Match(Base, soSplit, link));

			decimal cartQty = allLinks.Where(link => SOShipmentSplitToCartSplitLink.FK.CartSplit.Match(Base, cartSplit, link)).Sum(_ => _.Qty ?? 0);

			if (cartQty + deltaQty > cartSplit.Qty)
			{
				return WMSFlowStatus.Fail(Msg.LinkCartOverpicking);
			}
			if (currentLink == null ? deltaQty < 0 : currentLink.Qty + deltaQty < 0)
			{
				return WMSFlowStatus.Fail(Msg.LinkUnderpicking);
			}

			if (currentLink == null)
			{
				currentLink = CartSplitLinks.Insert(new SOShipmentSplitToCartSplitLink
				{
					ShipmentNbr = soSplit.ShipmentNbr,
					ShipmentLineNbr = soSplit.LineNbr,
					ShipmentSplitLineNbr = soSplit.SplitLineNbr,
					SiteID = cartSplit.SiteID,
					CartID = cartSplit.CartID,
					CartSplitLineNbr = cartSplit.SplitLineNbr,
					Qty = deltaQty
				});
			}
			else
			{
				currentLink.Qty += deltaQty;
				currentLink = CartSplitLinks.Update(currentLink);
			}

			if (currentLink.Qty == 0)
				CartSplitLinks.Delete(currentLink);

			return WMSFlowStatus.Ok;
		}

		protected virtual WMSFlowStatus SyncWithCart(Header header, SOPickerListEntry entry, decimal qty)
		{
			INCartSplit[] linkedSplits =
				SelectFrom<SOPickListEntryToCartSplitLink>.
				InnerJoin<INCartSplit>.On<SOPickListEntryToCartSplitLink.FK.CartSplit>.
				Where<SOPickListEntryToCartSplitLink.FK.PickListEntry.SameAsCurrent.
					And<SOPickListEntryToCartSplitLink.siteID.IsEqual<@P.AsInt>>.
					And<SOPickListEntryToCartSplitLink.cartID.IsEqual<@P.AsInt>>>.
				View
				.SelectMultiBound(Base, new object[] { entry }, header.SiteID, header.CartID)
				.RowCast<INCartSplit>()
				.ToArray();

			INCartSplit[] appropriateSplits =
				SelectFrom<INCartSplit>.
				Where<INCartSplit.cartID.IsEqual<@P.AsInt>.
					And<INCartSplit.inventoryID.IsEqual<SOPickerListEntry.inventoryID.FromCurrent>>.
					And<INCartSplit.subItemID.IsEqual<SOPickerListEntry.subItemID.FromCurrent>>.
					And<INCartSplit.siteID.IsEqual<SOPickerListEntry.siteID.FromCurrent>>.
					And<INCartSplit.fromLocationID.IsEqual<SOPickerListEntry.locationID.FromCurrent>>.
					And<INCartSplit.lotSerialNbr.IsEqual<SOPickerListEntry.lotSerialNbr.FromCurrent>>>.
				View
				.SelectMultiBound(Base, new object[] { entry }, header.CartID)
				.RowCast<INCartSplit>()
				.ToArray();

			INCartSplit[] existingINSplits = linkedSplits.Concat(appropriateSplits).ToArray();

			INCartSplit cartSplit = existingINSplits.FirstOrDefault(s => s.LotSerialNbr == (header.LotSerialNbr ?? s.LotSerialNbr));
			if (cartSplit == null)
			{
				cartSplit = CartSplits.Insert(new INCartSplit
				{
					CartID = header.CartID,
					InventoryID = entry.InventoryID,
					SubItemID = entry.SubItemID,
					LotSerialNbr = entry.LotSerialNbr,
					ExpireDate = entry.ExpireDate,
					UOM = entry.UOM,
					SiteID = entry.SiteID,
					FromLocationID = entry.LocationID,
					Qty = qty
				});
			}
			else
			{
				cartSplit.Qty += qty;
				cartSplit = CartSplits.Update(cartSplit);
			}

			if (cartSplit.Qty == 0)
			{
				CartSplits.Delete(cartSplit);
				return WMSFlowStatus.Ok;
			}
			else
				return EnsurePickerCartSplitLink(entry, cartSplit, qty);
		}

		protected virtual WMSFlowStatus EnsurePickerCartSplitLink(SOPickerListEntry entry, INCartSplit cartSplit, decimal deltaQty)
		{
			var allLinks =
				SelectFrom<SOPickListEntryToCartSplitLink>.
				Where<SOPickListEntryToCartSplitLink.FK.CartSplit.SameAsCurrent.
					Or<SOPickListEntryToCartSplitLink.FK.PickListEntry.SameAsCurrent>>.
				View
				.SelectMultiBound(Base, new object[] { cartSplit, entry })
				.RowCast<SOPickListEntryToCartSplitLink>()
				.ToArray();

			SOPickListEntryToCartSplitLink currentLink = allLinks.FirstOrDefault(
				link => SOPickListEntryToCartSplitLink.FK.CartSplit.Match(Base, cartSplit, link)
					&& SOPickListEntryToCartSplitLink.FK.PickListEntry.Match(Base, entry, link));

			decimal cartQty = allLinks.Where(link => SOPickListEntryToCartSplitLink.FK.CartSplit.Match(Base, cartSplit, link)).Sum(_ => _.Qty ?? 0);

			if (cartQty + deltaQty > cartSplit.Qty)
			{
				return WMSFlowStatus.Fail(Msg.LinkCartOverpicking);
			}
			if (currentLink == null ? deltaQty < 0 : currentLink.Qty + deltaQty < 0)
			{
				return WMSFlowStatus.Fail(Msg.LinkUnderpicking);
			}

			if (currentLink == null)
			{
				currentLink = PickerCartSplitLinks.Insert(new SOPickListEntryToCartSplitLink
				{
					WorksheetNbr = entry.WorksheetNbr,
					PickerNbr = entry.PickerNbr,
					EntryNbr = entry.EntryNbr,
					SiteID = cartSplit.SiteID,
					CartID = cartSplit.CartID,
					CartSplitLineNbr = cartSplit.SplitLineNbr,
					Qty = deltaQty
				});
			}
			else
			{
				currentLink.Qty += deltaQty;
				currentLink = PickerCartSplitLinks.Update(currentLink);
			}

			if (currentLink.Qty == 0)
				PickerCartSplitLinks.Delete(currentLink);

			return WMSFlowStatus.Ok;
		}

		private void EnsureCartShipmentLink()
		{
			if (HeaderView.Current.CartID != null && HeaderView.Current.SiteID != null && HeaderView.Current.RefNbr != null)
			{
				var link = new SOCartShipment
				{
					SiteID = HeaderView.Current.SiteID,
					CartID = HeaderView.Current.CartID,
					ShipmentNbr = HeaderView.Current.RefNbr,
				};

				if (CartSplits.SelectMain().Any())
					CartsLinks.Update(link); // also insert
				else
					CartsLinks.Delete(link);
			}
		}
		#endregion
		#endregion

		#region Pack line confirmation
		protected virtual WMSFlowStatus ConfirmPacked()
		{
			WMSFlowStatus Implementation()
			{
				Header header = HeaderView.Current;
				bool remove = header.Remove == true;
				bool noPick = NoPick;
				bool locationIsRequired = IsLocationRequiredInPack(header);
				string specialPickType = SpecialPickType;
				decimal? TargetQty(SOShipLineSplit s) =>
					specialPickType == null ? (noPick ? s.Qty * Base.GetQtyThreshold(s) : s.PickedQty) :
					specialPickType == SOPickingWorksheet.worksheetType.Wave ? s.PickedQty :
					specialPickType == SOPickingWorksheet.worksheetType.Batch ? s.PickedQty * Base.GetQtyThreshold(s) :
					0m;

				if (header.PackageLineNbr == null)
					return WMSFlowStatus.Fail(remove ? Msg.NothingToRemove : Msg.NothingToPack);

				var packageDetail = SelectedPackage;
				void KeepPackageSelection() => HeaderSetter.WithEventFiring.Set(h => h.PackageLineNbr, packageDetail.LineNbr);

				if (header.InventoryID == null || header.Qty == 0)
					return WMSFlowStatus.Fail(remove ? Msg.NothingToRemove : Msg.NothingToPack).WithPostAction(KeepPackageSelection);

				var packedSplits = Picked.SelectMain().Where(
					r => r.InventoryID == header.InventoryID
							&& r.SubItemID == header.SubItemID
							&& (r.IsUnassigned == true || r.LotSerialNbr == (header.LotSerialNbr ?? r.LotSerialNbr))
							&& locationIsRequired.Implies(r.LocationID == (header.LocationID ?? r.LocationID))
							&& (remove ? r.PackedQty > 0 : TargetQty(r) > r.PackedQty));

				if (noPick && Shipment?.PickedViaWorksheet == false)
					packedSplits = packedSplits
					.OrderByDescending(split => split.IsUnassigned == false)
					.OrderByDescending(split => remove ? split.PickedQty > 0 : split.Qty > split.PickedQty)
					.ThenByDescending(split => split.LotSerialNbr == (header.LotSerialNbr ?? split.LotSerialNbr))
					.ThenByDescending(split => string.IsNullOrEmpty(split.LotSerialNbr))
					.ThenByDescending(split => (split.Qty > split.PickedQty || remove) && split.PickedQty > 0)
					.ThenByDescending(split => Sign.MinusIf(remove) * (split.Qty - split.PickedQty));
				if (packedSplits.Any() == false)
					return WMSFlowStatus.Fail(remove ? Msg.NothingToRemove : Msg.NothingToPack).ClearIsNeeded.WithPostAction(KeepPackageSelection);

				decimal qty = header.GetBaseQty(Base);
				string inventoryCD = InventoryItem.PK.Find(Base, header.InventoryID.Value).InventoryCD;

				if (remove ? packedSplits.Sum(s => s.PackedQty) - qty < 0 : packedSplits.Sum(s => TargetQty(s) - s.PackedQty) < qty)
					return WMSFlowStatus.Fail(remove ? Msg.BoxCanNotUnpack : Msg.BoxCanNotPack, inventoryCD, header.Qty, header.UOM);

				decimal unassignedQty = Sign.MinusIf(remove) * qty;
				foreach (var packedSplit in packedSplits)
				{
					decimal currentQty = remove
						? -Math.Min(packedSplit.PackedQty.Value, -unassignedQty)
						: Math.Min(TargetQty(packedSplit).Value - packedSplit.PackedQty.Value, unassignedQty);

					if (PackSplit(packedSplit, packageDetail, currentQty) == false)
						return WMSFlowStatus.Fail(remove ? Msg.BoxCanNotUnpack : Msg.BoxCanNotPack, inventoryCD, header.Qty, header.UOM);

					unassignedQty -= currentQty;
					if (unassignedQty == 0)
						break;
				}

				bool packageRemoved = false;
				if (Base.PackageDetailExt.PackageDetailSplit.Select(packageDetail.ShipmentNbr, packageDetail.LineNbr).Count == 0)
				{
					Base.Packages.Delete(packageDetail);
					HeaderSetter.WithEventFiring.Set(h => h.PackageLineNbr, null);
					packageRemoved = true;
				}

				EnsureShipmentUserLink();

				if (CanPack)
				{
					SetScanState(
						packageRemoved
							? ScanStates.Box
							: ScanStates.Item,
						remove
							? Msg.InventoryRemoved
							: Msg.InventoryAdded,
						InventoryItem.PK.Find(Base, header.InventoryID.Value).InventoryCD, header.Qty, header.UOM);

					if (packageRemoved == false)
						Prompt(Setup.Current.ShowPickTab == true
								? Msg.PackConfirmationPrompt
								: Msg.PackNoPickConfirmationPrompt);
				}
				else
				{
					SetScanState(ScanStates.Command, Msg.ShipmentPacked, header.RefNbr);
					if (HasSingleAutoPackage(header.RefNbr, out var _) == false)
						Prompt(Msg.BoxConfirmPrompt);
				}

				return packageRemoved
					? WMSFlowStatus.Ok
					: WMSFlowStatus.Ok.WithPostAction(KeepPackageSelection);
			}

			var isQtyOverridable = IsQtyOverridable(HeaderView.Current.LotSerTrack);

			var res = ExecuteAndCompleteFlow(Implementation);

			if (isQtyOverridable && Info.Current.MessageType == WMSMessageTypes.Information)
				HeaderSetter.Set(x => x.IsQtyOverridable, true);

			return res;
		}

		protected virtual bool PackSplit(SOShipLineSplit split, SOPackageDetailEx package, decimal qty)
		{
			if (NoPick == false)
				EnsureAssignedSplitEditing(split);
			else if (split.IsUnassigned == true)
			{
				var existingSplit = Picked.SelectMain().FirstOrDefault(s => s.LineNbr == split.LineNbr && s.IsUnassigned == false && s.LotSerialNbr == (HeaderView.Current.LotSerialNbr ?? s.LotSerialNbr) && IsSelectedSplit(s, HeaderView.Current));

				if (existingSplit == null)
				{
					var newSplit = (SOShipLineSplit)Picked.Cache.CreateCopy(split);
					newSplit.SplitLineNbr = null;
					newSplit.LotSerialNbr = HeaderView.Current.LotSerialNbr;
					newSplit.ExpireDate = HeaderView.Current.ExpireDate;
					newSplit.Qty = qty;
					newSplit.PickedQty = qty;
					newSplit.PackedQty = 0;
					newSplit.IsUnassigned = false;
					newSplit.PlanID = null;

					split = Picked.Insert(newSplit);
				}
				else
				{
					existingSplit.Qty += qty;
					existingSplit.ExpireDate = HeaderView.Current.ExpireDate;
					split = Picked.Update(existingSplit);
				}
			}

			SOShipLineSplitPackage link = Base.PackageDetailExt.PackageDetailSplit
				.SelectMain(package.ShipmentNbr, package.LineNbr)
				.FirstOrDefault(t =>
					t.ShipmentNbr == split.ShipmentNbr
					&& t.ShipmentLineNbr == split.LineNbr
					&& t.ShipmentSplitLineNbr == split.SplitLineNbr);

			if (qty < 0)
			{
				if (link == null || link.PackedQty + qty < 0)
					return false;

				if (HeaderView.Current.IsEntarableLotSerial)
				{
					split.Qty += qty;
					if (split.Qty == 0)
						split = Picked.Delete(split);
					else
						split = Picked.Update(split);
				}

				if (link.PackedQty + qty > 0)
				{
					link.PackedQty += qty;
					Base.PackageDetailExt.PackageDetailSplit.Update(link);
				}
				else if (link.PackedQty + qty == 0)
				{
					Base.PackageDetailExt.PackageDetailSplit.Delete(link);
				}

				package.Confirmed = false;
				Base.Packages.Update(package);
			}
			else
			{
				if (link == null)
				{
					link = (SOShipLineSplitPackage)Base.PackageDetailExt.PackageDetailSplit.Cache.CreateInstance();

					PXFieldVerifying ver = (c, a) => a.Cancel = true;
					Base.FieldVerifying.AddHandler<SOShipLineSplitPackage.shipmentSplitLineNbr>(ver);
					link.ShipmentSplitLineNbr = split.SplitLineNbr;
					link.PackedQty = qty;
					link = Base.PackageDetailExt.PackageDetailSplit.Insert(link);
					Base.FieldVerifying.RemoveHandler<SOShipLineSplitPackage.shipmentSplitLineNbr>(ver);

					link.ShipmentNbr = split.ShipmentNbr;
					link.ShipmentLineNbr = split.LineNbr;
					link.PackageLineNbr = package.LineNbr;
					link.InventoryID = split.InventoryID;
					link.SubItemID = split.SubItemID;
					link.LotSerialNbr = split.LotSerialNbr;
					link.UOM = split.UOM;

					link = Base.PackageDetailExt.PackageDetailSplit.Update(link);
				}
				else
				{
					link.PackedQty += qty;
					Base.PackageDetailExt.PackageDetailSplit.Update(link);
				}
			}

			return true;
		}

		protected virtual bool ConfirmPackedBox()
		{
			SOPackageDetailEx package = SelectedPackage;
			if (package == null)
				return false;

			HeaderSetter.Set(
				h => h.Weight,
				package.Weight == 0
					? AutoCalculateBoxWeightBasedOnItems(package)
					: package.Weight.Value);

			if (UserSetup.For(Base).UseScale != true)
				SetScanState(ScanStates.BoxWeight, Msg.BoxConfirm, package.BoxID, HeaderView.Current.Weight, CommonSetupUOM.Current.WeightUOM);
			else if (ProcessScaleWeight(package) == false)
				SetScanState(ScanStates.BoxWeight);

			return true;
		}

		protected virtual bool AutoConfirmPackage(bool skipBoxWeightInput)
		{
			if (HeaderView.Current.PackageLineNbr != null)
			{
				var package = SelectedPackage;
				if (package != null)
				{
					if (ConfirmPackedBox())
					{
						if (skipBoxWeightInput && package.Confirmed == false)
							return SkipBoxWeightInput();
					}
					else
						return false;
				}
			}
			return true;
		}

		protected decimal AutoCalculateBoxWeightBasedOnItems(SOPackageDetailEx package)
		{
			decimal calculatedWeight = CSBox.PK.Find(Base, package.BoxID)?.BoxWeight ?? 0m;
			SOShipLineSplitPackage[] links = Base.PackageDetailExt.PackageDetailSplit.SelectMain(package.ShipmentNbr, package.LineNbr);
			foreach (var link in links)
			{
				var inventory = InventoryItem.PK.Find(Base, link.InventoryID);
				calculatedWeight += (inventory.BaseWeight ?? 0) * (link.BasePackedQty ?? 0);
			}

			return Math.Round(calculatedWeight, 4);
		}

		protected virtual bool SkipBoxWeightInput()
		{
			if (HeaderView.Current.Weight.IsIn(null, 0))
			{
				ReportError(Msg.BoxWeightNoSkip);
				return false;
			}
			else
			{
				try
				{
					SetPackageWeight(HeaderView.Current.Weight.Value);
					return true;
				}
				catch (PXSetPropertyException outer) when (Setup.Current.ConfirmEachPackageWeight == false && outer.InnerException is PXSetPropertyException inner)
				{
					ReportError(inner.MessageNoPrefix);
					return false;
				}
			}
		}

		protected virtual bool? ProcessScaleWeight(SOPackageDetailEx package)
		{
			Guid? scaleDeviceID = UserSetup.For(Base).ScaleDeviceID;

			Base.Caches<SMScale>().ClearQueryCache();
			SMScale scale = SMScale.PK.Find(Base, scaleDeviceID);

			if (scale == null)
			{
				ReportError(Msg.ScaleMissing, "");
				return false;
			}

			DateTime dbNow = GetServerTime();

			if (scale.LastModifiedDateTime.Value.AddHours(1) < dbNow)
			{
				ReportError(Msg.ScaleDisconnected, scale.ScaleID);
				return false;
			}
			else if (scale.LastWeight.GetValueOrDefault() == 0)
			{
				if (HeaderView.Current.LastWeighingTime == scale.LastModifiedDateTime.Value)
				{
					SkipBoxWeightInput();
					return true;
				}
				else
				{
					ReportWarning(Msg.ScaleNoBox, scale.ScaleID);
					Prompt(Msg.ScaleSkipPrompt);
					HeaderSetter.Set(h => h.LastWeighingTime, scale.LastModifiedDateTime.Value);
					return null;
				}
			}
			else if (scale.LastModifiedDateTime.Value.AddSeconds(ScaleWeightValiditySeconds) < dbNow)
			{
				ReportError(Msg.ScaleTimeout, scale.ScaleID, ScaleWeightValiditySeconds);
				return null;
			}
			else
			{
				decimal weight = ConvertKilogramToWeightUnit(scale.LastWeight.GetValueOrDefault(), CommonSetupUOM.Current.WeightUOM);
				SetPackageWeight(weight);
				return true;
			}
		}

		protected virtual void SetPackageWeight(decimal value)
		{
			SOPackageDetailEx packageDetail = SelectedPackage;
			if (packageDetail != null)
			{
				packageDetail.Weight = Math.Round(value, 4);
				packageDetail.Confirmed = true;
				Base.Packages.Update(packageDetail);

				if (CanPack)
					SetScanState(ScanStates.Box, Msg.BoxConfirmed, packageDetail.Weight, CommonSetupUOM.Current.WeightUOM);
				else
					SetScanState(ScanStates.Command, Msg.ShipmentPacked, HeaderView.Current.RefNbr);

				ClearHeaderInfo();
				HeaderSetter.WithEventFiring.Set(h => h.PackageLineNbr, packageDetail.LineNbr);

				Save.Press();
			}
		}

		protected virtual decimal ConvertKilogramToWeightUnit(decimal weight, string weightUnit)
		{
			weightUnit = weightUnit.Trim().ToUpperInvariant();
			decimal conversionFactor =
				weightUnit == "KG" ? 1m :
				weightUnit == "LB" ? 0.453592m :
				throw new PXException(Msg.PackageWrongWeightUnit, weightUnit);

			return weight / conversionFactor;
		}
		#endregion

		#region Clearing logic
		protected override void ClearMode()
		{
			ClearHeaderInfo(true);
			Report(Msg.ScreenCleared);

			if (HeaderView.Current.Mode == Modes.PickWave)
			{
				if (HeaderView.Current.WorksheetNbr == null || DocumentIsConfirmed)
					SetScanState(ScanStates.RefNbr);
				else if (ShipmentWithoutTote != null)
					SetScanState(ScanStates.AssignTote);
				else
					SetScanState(ScanStates.Location);
			}
			else if (HeaderView.Current.Mode == Modes.PickBatch)
			{
				if (HeaderView.Current.WorksheetNbr == null || DocumentIsConfirmed)
					SetScanState(ScanStates.RefNbr);
				else if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
					SetScanState(ScanStates.Cart);
				else
					SetScanState(ScanStates.Location);
			}
			else if (HeaderView.Current.Mode == Modes.Pick)
			{
				if (HeaderView.Current.RefNbr == null || DocumentIsConfirmed)
					SetScanState(ScanStates.RefNbr);
				else if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
					SetScanState(ScanStates.Cart);
				else
					SetScanState(HeaderView.Current.IsReturnMode ? ScanStates.Item : ScanStates.Location);
			}
			else if (HeaderView.Current.Mode == Modes.Pack)
			{
				if (HeaderView.Current.RefNbr == null || DocumentIsConfirmed)
					SetScanState(ScanStates.RefNbr);
				else
					SetScanState(ScanStates.Box);
			}
			else if (HeaderView.Current.Mode == Modes.Ship)
			{
				if (HeaderView.Current.RefNbr == null || DocumentIsConfirmed)
					SetScanState(ScanStates.RefNbr);
				else
					SetScanState(ScanStates.Carrier);
			}
		}

		protected override void ClearHeaderInfo(bool redirect = false)
		{
			base.ClearHeaderInfo(redirect);

			HeaderSetter.Set(h => h.PackageLineNbr, null);
			HeaderSetter.Set(h => h.PackageLineNbrUI, null);
			HeaderSetter.Set(h => h.LotSerialNbr, null);
			HeaderSetter.Set(h => h.ExpireDate, null);
			HeaderSetter.Set(h => h.Weight, null);
			HeaderSetter.Set(h => h.LastWeighingTime, null);
			HeaderSetter.Set(h => h.RemoveFromToteID, null);
			HeaderSetter.Set(h => h.ProcessingSucceeded, null);
			if (redirect)
			{
				HeaderSetter.Set(h => h.CartLoaded, false);
				HeaderSetter.Set(h => h.LocationID, null);
				if (HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch))
					HeaderSetter.Set(h => h.CartID, null);
			}
		}
		#endregion

		protected virtual bool OnShipmentConfirming(bool confirmAsIs)
		{
			if (confirmAsIs)
				return true;

			if (HasPick && !OnPickedShipmentConfirming())
				return false;

			if (HasPack && !OnPackedShipmentConfirming())
				return false;

			return true;
		}

		protected virtual bool OnPickedShipmentConfirming()
		{
			var splits = Picked.SelectMain();
			if (splits.All(s => s.PickedQty == 0))
			{
				ReportError(Msg.ShipmentCannotBeConfirmed);
				return false;
			}

			if (Info.Current.MessageType != WMSMessageTypes.Warning && splits.Any(s => s.PickedQty < s.Qty * Base.GetMinQtyThreshold(s)))
			{
				if (CannotConfirmPartialShipments)
					ReportError(Msg.ShipmentCannotBeConfirmedInPart);
				else
					ReportWarning(Msg.ShipmentShouldNotBeConfirmedInPart);
				return false;
			}

			bool hasIncompleteLines =
				SelectFrom<SOLine>.
				InnerJoin<SOOrder>.On<SOLine.FK.Order>.
				InnerJoin<SOShipLine>.On<SOShipLine.FK.OrigLine>.
				InnerJoin<SOShipLineSplit>.On<SOShipLineSplit.FK.ShipLine>.
				Where<
					SOShipLine.FK.Shipment.SameAsCurrent.
					And<SOShipLineSplit.pickedQty.IsLess<SOShipLineSplit.qty.Multiply<SOLine.completeQtyMin.Divide<decimal100>>>>.
					And<SOOrder.shipComplete.IsEqual<SOShipComplete.shipComplete>.
						Or<SOLine.shipComplete.IsEqual<SOShipComplete.shipComplete>>>>.
				View.SelectMultiBound(Base, new[] { Shipment }).Any();

			if (hasIncompleteLines)
			{
				ReportError(Msg.ShipmentCannotBeConfirmedInPart);
				return false;
			}
			return true;
		}

		protected virtual bool OnPackedShipmentConfirming()
		{
			var splits = Picked.SelectMain();
			if (splits.All(s => s.PackedQty == 0))
				return true;

			if (Info.Current.MessageType != WMSMessageTypes.Warning && splits.Any(s => s.PackedQty < s.Qty * Base.GetMinQtyThreshold(s)))
			{
				if (CannotConfirmPartialShipments)
					ReportError(Msg.ShipmentCannotBeConfirmedInPart);
				else
					ReportWarning(Msg.ShipmentShouldNotBeConfirmedInPart);
				return false;
			}

			bool hasIncompleteLines =
				SelectFrom<SOLine>.
				InnerJoin<SOOrder>.On<SOLine.FK.Order>.
				InnerJoin<SOShipLine>.On<SOShipLine.FK.OrigLine>.
				InnerJoin<SOShipLineSplit>.On<SOShipLineSplit.FK.ShipLine>.
				Where<
					SOShipLine.FK.Shipment.SameAsCurrent.
					And<SOShipLineSplit.packedQty.IsLess<SOShipLineSplit.qty.Multiply<SOLine.completeQtyMin.Divide<decimal100>>>>.
					And<SOOrder.shipComplete.IsEqual<SOShipComplete.shipComplete>.
						Or<SOLine.shipComplete.IsEqual<SOShipComplete.shipComplete>>>>.
				View.SelectMultiBound(Base, new[] { Shipment }).Any();

			if (hasIncompleteLines)
			{
				ReportError(Msg.ShipmentCannotBeConfirmedInPart);
				return false;
			}
			return true;
		}

		protected virtual void ConfirmShipment(bool confirmAll)
		{
			if (!OnShipmentConfirming(confirmAll))
				return;

			if (Setup.Current.ConfirmEachPackageWeight == false && AutoConfirmPackage(true) == false)
				return;

			int? packageLineNbr = HeaderView.Current.PackageLineNbr;
			ClearHeaderInfo();
			HeaderSetter.WithEventFiring.Set(h => h.PackageLineNbr, packageLineNbr);

			string shipmentNbr = HeaderView.Current.RefNbr;
			SOPickPackShipSetup setup = Setup.Current;
			SOPickPackShipUserSetup userSetup = UserSetup.For(Base);
			SOPackageDetailEx autoPackageToConfirm = null;
			if (!confirmAll && HeaderView.Current.Mode.IsIn(Modes.Pack, Modes.Ship))
				HasSingleAutoPackage(shipmentNbr, out autoPackageToConfirm);

			Save.Press();

			WaitFor((wArgs) => ConfirmShipmentHandler(wArgs.Document.ShipmentNbr, confirmAll, setup, userSetup, autoPackageToConfirm), null, new DocumentWaitArguments(Shipment), Msg.ShipmentConfirming, shipmentNbr);
		}

		protected static void ConfirmShipmentHandler(string shipmentNbr, bool confirmAsIs, SOPickPackShipSetup setup, SOPickPackShipUserSetup userSetup, SOPackageDetailEx autoPackageToConfirm)
		{
			SOShipmentEntry shipmentEntry = PXGraph.CreateInstance<SOShipmentEntry>();
			shipmentEntry.Document.Current = SelectFrom<SOShipment>.Where<SOShipment.shipmentNbr.IsEqual<@P.AsString>>.View.Select(shipmentEntry, shipmentNbr);

			var kitSpecHelper = new NonStockKitSpecHelper(shipmentEntry);

			CloseShipmentUserLinks(shipmentEntry, shipmentNbr);

			var RequireShipping = Func.Memorize((int inventoryID) => InventoryItem.PK.Find(shipmentEntry, inventoryID).With(item => item.StkItem == true || item.NonStockShip == true));

			if (!confirmAsIs && (setup.ShowPickTab == true || setup.ShowPackTab == true))
			{
				PXSelectBase<SOShipLine> lines = shipmentEntry.Transactions;
				PXSelectBase<SOShipLineSplit> splits = shipmentEntry.splits;

				foreach (SOShipLine line in lines.Select())
				{
					lines.Current = line;
					decimal lineQty = 0;

					decimal GetNewQty(SOShipLineSplit split) => setup.ShowPickTab == true ? split.PickedQty ?? 0 : Math.Max(split.PickedQty ?? 0, split.PackedQty ?? 0);
					if (kitSpecHelper.IsNonStockKit(line.InventoryID))
					{
						// kitInventoryID -> compInventory -> qty
						var nonStockKitSpec = kitSpecHelper.GetNonStockKitSpec(line.InventoryID.Value).Where(pair => RequireShipping(pair.Key)).ToDictionary();
						var nonStockKitSplits = splits.SelectMain().GroupBy(r => r.InventoryID.Value).ToDictionary(g => g.Key, g => g.Sum(s => GetNewQty(s)));

						lineQty = nonStockKitSpec.Keys.Count() == 0 || nonStockKitSpec.Keys.Except(nonStockKitSplits.Keys).Count() > 0
							? 0
							: (from split in nonStockKitSplits
							   join spec in nonStockKitSpec on split.Key equals spec.Key
							   select Math.Floor(decimal.Divide(split.Value, spec.Value))).Min();
					}
					else
					{
						foreach (SOShipLineSplit split in splits.Select())
						{
							splits.Current = split;

							decimal newQty = GetNewQty(splits.Current);
							if (newQty != splits.Current.Qty)
							{
								splits.Current.Qty = newQty;
								splits.UpdateCurrent();
							}

							if (splits.Current.Qty != 0)
								lineQty += splits.Current.Qty ?? 0;
						}
						lineQty = INUnitAttribute.ConvertFromBase(lines.Cache, lines.Current.InventoryID, lines.Current.UOM, lineQty, INPrecision.NOROUND);
					}

					lines.Current.Qty = lineQty;
					lines.UpdateCurrent();

					if (lines.Current.Qty == 0)
						lines.DeleteCurrent();
				}

				foreach (SOPackageDetailEx package in shipmentEntry.Packages.SelectMain())
					if (package.PackageType == SOPackageType.Manual && shipmentEntry.PackageDetailExt.PackageDetailSplit.Select(package.ShipmentNbr, package.LineNbr).Count == 0)
						shipmentEntry.Packages.Delete(package);
			}

			foreach (SOCartShipment cartLink in PXSelect<SOCartShipment, Where<SOCartShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>.Select(shipmentEntry))
				shipmentEntry.Caches<SOCartShipment>().Delete(cartLink);

			if (autoPackageToConfirm?.Confirmed == false)
			{
				autoPackageToConfirm.Confirmed = true;
				shipmentEntry.Packages.Update(autoPackageToConfirm);
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>())
			{
				var packages = shipmentEntry.Packages.SelectMain();
				if (confirmAsIs)
				{
					foreach (var package in packages.Where(x => x.Confirmed != true))
					{
						package.Confirmed = true;
						shipmentEntry.Packages.Cache.Update(package);
					}
				}
				if (shipmentEntry.Document.Current.IsPackageValid == false && packages.Any(p => p.PackageType == SOPackageType.Auto))
			{
				shipmentEntry.Document.Current.IsPackageValid = true;
				shipmentEntry.Document.UpdateCurrent();
			}
			}

			if (shipmentEntry.IsDirty)
			{
				shipmentEntry.Document.Current.IsPackageValid = true;
				shipmentEntry.Document.UpdateCurrent();
				shipmentEntry.Save.Press();
			}

			if (UseExternalShippingApplication(shipmentEntry, shipmentEntry.Document.Current, out Carrier carrier))
			{
				// Shipping Tool will confirm the shipment.
				throw new PXRedirectToUrlException(
					$"../../Frames/ShipmentAppLauncher.html?ShipmentApplicationType={carrier.ShippingApplicationType}&ShipmentNbr={shipmentNbr}",
					PXBaseRedirectException.WindowMode.NewWindow, true, string.Empty);
			}

			PXView.Dummy view = PXView.Dummy.For<SOShipment>(shipmentEntry);
			var confirmAdapter = new PXAdapter(view)
			{
				Arguments = { ["actionID"] = SOShipmentEntryActionsAttribute.ConfirmShipment }
			};
			shipmentEntry.action.PressButton(confirmAdapter);

			shipmentEntry.Clear();
			shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);

			if (PXAccess.FeatureInstalled<FeaturesSet.deviceHub>())
			{
				PXAdapter deviceHubAdapter = new PXAdapter(view)
				{
					MassProcess = true, //Device Hub require this flag to know if supported
					Arguments = { [nameof(PX.SM.IPrintable.PrintWithDeviceHub)] = true, [nameof(PX.SM.IPrintable.DefinePrinterManually)] = false }
				};

				//Labels should ALWAYS be printer first because they go out faster, and that gives time to user to peel/stick them while shipment confirmation is spooling
				if (userSetup.PrintShipmentLabels == true)
					WithSuppressedRedirects(() => shipmentEntry.PrintCarrierLabels(new List<SOShipment>() { shipmentEntry.Document.Current }, deviceHubAdapter));

				if (userSetup.PrintShipmentConfirmation == true)
					WithSuppressedRedirects(() => shipmentEntry.Report(deviceHubAdapter, SOReports.PrintShipmentConfirmation));
			}

			throw new PXOperationCompletedException(Msg.ShipmentIsConfirmed);
		}

		protected virtual void ConfirmPickList()
		{
			if (PickListOfPicker.SelectMain().All(s => s.PickedQty == 0))
			{
				ReportError(Msg.PickListCannotBeConfirmed);
				return;
			}

			if (Info.Current.MessageType != WMSMessageTypes.Warning && PickListOfPicker.SelectMain().Any(s => s.PickedQty < s.Qty))
			{
				if (CannotConfirmPartialShipments)
					ReportError(Msg.PickListCannotBeConfirmedInPart);
				else
					ReportWarning(Msg.PickListShouldNotBeConfirmedInPart);
				return;
			}

			if (Worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Batch)
				SetScanState(ScanStates.SortingLocation);
			else if (Worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Wave)
				ConfirmPickList(sortingLocationID: null);
		}

		protected virtual void ConfirmPickList(int? sortingLocationID)
		{
			SOPickingWorksheet worksheet = Worksheet.Current;
			SOPicker pickList = Picker.Current;
			WaitFor(() => ConfirmPickListHandler(worksheet, pickList, sortingLocationID), Msg.PickListConfirming, pickList.PickListNbr);
		}

		protected static void ConfirmPickListHandler(SOPickingWorksheet worksheet, SOPicker pickList, int? sortingLocationID)
		{
			using (var ts = new PXTransactionScope())
			{
				WithSuppressedRedirects(() =>
				{
					var wsGraph = PXGraph.CreateInstance<SOPickingWorksheetReview>();
					wsGraph.PickListConfirmation.ConfirmPickList(pickList, sortingLocationID);
					wsGraph.PickListConfirmation.FulfillShipmentsAndConfirmWorksheet(worksheet);
				});
				ts.Complete();
			}
		}

		protected override void OnWaitEnd(PXLongRunStatus status, SOShipment primaryRow)
		{
			bool sentToExternalShippingApp = status == PXLongRunStatus.Completed && UseExternalShippingApplication(primaryRow, out Carrier carrier);
			bool shipmentConfirmed = primaryRow != null && (primaryRow.Confirmed == true || sentToExternalShippingApp);

			bool wsPicking = HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch);
			var picker = wsPicking ? SOPicker.PK.Find(Base, HeaderView.Current.WorksheetNbr, HeaderView.Current.PickerNbr) : null;
			if (picker?.Confirmed == true || shipmentConfirmed)
			{
				if (Base.IsMobile)
					Clear(wsPicking ? Msg.PickListIsConfirmed : Msg.ShipmentIsConfirmed);
				else
				{
					Base.Clear();
					Base.SelectTimeStamp();
					SetScanState(GetDefaultState());
				}

				string msg = wsPicking ? Msg.PickListIsConfirmed
						: !sentToExternalShippingApp ? Msg.ShipmentIsConfirmed
							: Localize(Msg.ShipmentConfirming, primaryRow?.ShipmentNbr);
				ReportComplete(msg);
				HeaderSetter.Set(h => h.ProcessingSucceeded, true);
			}
			else if (status == PXLongRunStatus.Aborted)
			{
				SetScanState(ScanStates.Command, wsPicking ? Msg.PickListConfirmationFailed : Msg.ShipmentConfirmationFailed);
				HeaderSetter.Set(h => h.ProcessingSucceeded, false);
			}
		}

		protected virtual bool UseExternalShippingApplication(SOShipment shipment, out Carrier carrier)
			=> UseExternalShippingApplication(Base, shipment, out carrier);

		private static bool UseExternalShippingApplication(PXGraph graph, SOShipment shipment, out Carrier carrier)
		{
			carrier = Carrier.PK.Find(graph, shipment.ShipVia);
			return graph.IsMobile == false && carrier != null && carrier.IsExternalShippingApplication == true;
		}

		protected virtual bool IsSelectedSplit(SOShipLineSplit split, Header header)
		{
			return
				split.InventoryID == header.InventoryID &&
				split.SubItemID == header.SubItemID &&
				split.SiteID == header.SiteID &&
				(header.IsReturnMode || split.LocationID == (header.LocationID ?? split.LocationID)) &&
				(split.LotSerialNbr == (header.LotSerialNbr ?? split.LotSerialNbr) || header.Mode == Modes.Pick &&
					(header.LotSerAssign == INLotSerAssign.WhenUsed || (header.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable && split.PackedQty == 0))
					&& header.Remove == false);
		}

		protected virtual bool IsSelectedSplit(SOPickerListEntry split, Header header)
		{
			return
				split.InventoryID == header.InventoryID &&
				split.SubItemID == header.SubItemID &&
				split.SiteID == header.SiteID &&
				split.LocationID == (header.LocationID ?? split.LocationID) &&
				(split.LotSerialNbr == (header.LotSerialNbr ?? split.LotSerialNbr) || header.Remove == false && (header.LotSerAssign == INLotSerAssign.WhenUsed || header.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable));
		}

		private SOShipLineSplit MakeAssigned(Unassigned.SOShipLineSplit unassignedSplit) => PropertyTransfer.Transfer(unassignedSplit, new SOShipLineSplit());

		protected override bool IsCartRequired(Header header) => base.IsCartRequired(header) && Setup.Current.UseCartsForPick == true && header.Mode.IsIn(Modes.Pick, Modes.PickBatch) && !header.IsReturnMode;
		protected decimal GetCartQty(SOShipLineSplit sosplit) => CartSplitLinks.SelectMain().Where(link => SOShipmentSplitToCartSplitLink.FK.ShipmentSplitLine.Match(Base, sosplit, link)).Sum(_ => _.Qty ?? 0);
		protected decimal GetOverallCartQty(SOShipLineSplit sosplit) => AllCartSplitLinks.SelectMain().Where(link => SOShipmentSplitToCartSplitLink.FK.ShipmentSplitLine.Match(Base, sosplit, link)).Sum(_ => _.Qty ?? 0);

		protected override bool IsLocationRequired(Header header) => base.IsLocationRequired(header) && DefaultLocation == false;
		protected virtual bool IsLocationRequiredInPack(Header header)
		{
			switch (SpecialPickType)
			{
				case SOPickingWorksheet.worksheetType.Batch:	return true;
				case SOPickingWorksheet.worksheetType.Wave:		return false;
				case null:										return NoPick;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		protected bool DefaultLocation => UserSetup.For(Base).DefaultLocationFromShipment == true;
		protected bool DefaultLotSerial => UserSetup.For(Base).DefaultLotSerialFromShipment == true;
		protected bool NoPick => Setup.Current.ShowPickTab == false;
		protected bool HasPick => Setup.Current.ShowPickTab == true;
		protected bool HasPack => Setup.Current.ShowPackTab == true;
		protected virtual bool CanPick => HeaderView.Current.Mode == Modes.Pick && Picked.SelectMain().Any(s => s.PickedQty < s.Qty);
		protected virtual bool CanPack => HeaderView.Current.Mode == Modes.Pack && Picked.SelectMain().Any(s => Setup.Current.ShowPickTab == true ? s.PickedQty > 0 && s.PackedQty < s.PickedQty : s.PackedQty < s.Qty);
		private bool CanPickWS => HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch) && PickListOfPicker.SelectMain().Any(s => s.PickedQty < s.Qty);
		protected string SpecialPickType => Shipment.With(sh => sh.PickedViaWorksheet == true ? sh.CurrentWorksheetNbr.With(ws => SOPickingWorksheet.PK.Find(Base, ws)?.WorksheetType) : null);
		protected bool HasUnconfirmedBoxes => Base.Packages.SelectMain().Any(p => p.Confirmed == false);
		protected bool HasSingleAutoPackage(string shipmentNbr, out SOPackageDetailEx package)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>())
			{
				var packages = SelectFrom<SOPackageDetailEx>.Where<SOPackageDetailEx.shipmentNbr.IsEqual<@P.AsString>>.View.Select(Base, shipmentNbr).RowCast<SOPackageDetailEx>().ToArray();
				if (packages.Length == 1 && packages[0].PackageType == SOPackageType.Auto)
				{
					package = packages[0];
					return true;
				}
				else if (packages.Any(p => p.PackageType == SOPackageType.Auto))
					throw new PXInvalidOperationException(Msg.ShipmentCannotBePacked, shipmentNbr);
			}

			package = null;
			return false;
		}
		protected void EnsureAssignedSplitEditing(SOShipLineSplit split)
		{
			if (split.IsUnassigned == true)
				throw new InvalidOperationException("Unassigned splits should not be edited directly by WMS screen");
		}

		protected void EnsureShipmentUserLink()
		{
			var pickingTimeout = TimeSpan.FromMinutes(10);

			DateTime businessNow = Base.Accessinfo.BusinessDate.Value.Add(DateTime.Now.TimeOfDay); // TODO: change it to server time
			var shipByUserLinks = ShipmentProcessedByUser.Select().RowCast<SOShipmentProcessedByUser>().ToArray();

			bool isInitialChange =
				(HeaderView.Current.Mode == Modes.Pick || NoPick)
				&& HeaderView.Current.Remove == false
				&& !shipByUserLinks.Any();

			if (isInitialChange)
				ShipmentProcessedByUser.Insert().Apply(r => r.StartDateTime = r.LastModifiedDateTime = businessNow);
			else
			{
				var currentLink = shipByUserLinks.FirstOrDefault(s => s.EndDateTime == null)
					?? ShipmentProcessedByUser.Insert().Apply(r => r.StartDateTime = r.LastModifiedDateTime = businessNow);

				if (currentLink.LastModifiedDateTime.Value.Add(pickingTimeout) > businessNow)
				{
					currentLink.LastModifiedDateTime = currentLink.LastModifiedDateTime.Value.Add(pickingTimeout);
					ShipmentProcessedByUser.Update(currentLink);
				}
				else
				{
					currentLink.EndDateTime = currentLink.LastModifiedDateTime.Value.Add(pickingTimeout);
					ShipmentProcessedByUser.Update(currentLink);
					ShipmentProcessedByUser.Insert().Apply(r => r.StartDateTime = r.LastModifiedDateTime = businessNow);
				}
			}
		}
		
		private static void CloseShipmentUserLinks(PXGraph graph, string shipmentNbr)
		{
			var pickingTimeout = TimeSpan.FromMinutes(10);

			DateTime businessNow = graph.Accessinfo.BusinessDate.Value.Add(DateTime.Now.TimeOfDay); // TODO: change it to server time
			foreach (SOShipmentProcessedByUser shipByUser in
				SelectFrom<SOShipmentProcessedByUser>.
				Where<SOShipmentProcessedByUser.shipmentNbr.IsEqual<@P.AsString>>.
				View.Select(graph, shipmentNbr))
			{
				shipByUser.Confirmed = true;
				if (shipByUser.EndDateTime == null)
					shipByUser.EndDateTime = Tools.Min(businessNow, shipByUser.LastModifiedDateTime.Value.Add(pickingTimeout));

				graph.Caches<SOShipmentProcessedByUser>().Update(shipByUser);
			}
		}

		protected virtual void UpdateRates()
		{
			if ((SOPackageDetailEx)Base.Packages.SelectWindowed(0, 1) == null)
				return;

			try
			{
				Base.CarrierRatesExt.UpdateRates();
			}
			catch (PXException exception)
			{
				ReportError(exception.MessageNoPrefix);
			}
		}

		protected virtual void GetReturnLabelsProcess(IEnumerable<PickPackShip.Header> headers, bool massProcess, PXGraph clone)
		{
			if (!massProcess) PXLongOperation.SetCustomInfo(clone); // Redirect

			var notEmptyHeaders = headers.Where(s => !string.IsNullOrEmpty(s.RefNbr));
			foreach (var header in notEmptyHeaders)
			{
				var shipment = (SOShipment)PXSelect<SOShipment, Where<SOShipment.shipmentNbr, Equal<Required<Header.refNbr>>>>.Select(Base, header.RefNbr);

				try
				{
					if (massProcess) PXProcessing<SOShipment>.SetCurrentItem(header);
					Base.GetReturnLabels(shipment);
				}
				catch (Exception ex)
				{
					if (!massProcess) throw ex;
					PXProcessing<SOShipment>.SetError(ex);
				}
			}

			if (massProcess) this.Save.Press();
		}

		protected virtual bool SetLotSerialNbrAndQty(Header header, SOShipLineSplit pickedSplit, decimal qty)
		{
			if (pickedSplit.PickedQty == 0 && pickedSplit.IsUnassigned == false)
			{
				if (header.LotSerTrack == INLotSerTrack.SerialNumbered && header.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable)
				{
					SOShipLineSplit originalSplit =
						SelectFrom<SOShipLineSplit>.
						InnerJoin<SOLineSplit>.On<SOShipLineSplit.FK.OrigLineSplit>.
						Where<SOShipLineSplit.shipmentNbr.IsEqual<Header.refNbr.FromCurrent>.
							And<SOLineSplit.lotSerialNbr.IsEqual<@P.AsString>>>.
						View.Select(Base, header.LotSerialNbr);

					if (originalSplit == null)
					{
						if (header.IsReturnMode && header.Remove == false)
							pickedSplit.LocationID = header.LocationID;
						pickedSplit.LotSerialNbr = header.LotSerialNbr;
						pickedSplit.PickedQty += qty;
						pickedSplit = Picked.Update(pickedSplit);
					}
					else
					{
						if (originalSplit.LotSerialNbr == header.LotSerialNbr) return false;

						var tempOriginalSplit = (SOShipLineSplit)Picked.Cache.CreateCopy(originalSplit);
						var tempPickedSplit = (SOShipLineSplit)Picked.Cache.CreateCopy(pickedSplit);

						originalSplit.Qty = 0;
						originalSplit.LotSerialNbr = header.LotSerialNbr;
						originalSplit = Picked.Update(originalSplit);
						originalSplit.Qty = tempOriginalSplit.Qty;
						originalSplit.PickedQty = tempPickedSplit.PickedQty + qty;
						originalSplit.ExpireDate = tempPickedSplit.ExpireDate;
						originalSplit = Picked.Update(originalSplit);

						pickedSplit.Qty = 0;
						if (header.IsReturnMode && header.Remove == false)
							pickedSplit.LocationID = header.LocationID;
						pickedSplit.LotSerialNbr = tempOriginalSplit.LotSerialNbr;
						pickedSplit = Picked.Update(pickedSplit);
						pickedSplit.Qty = tempPickedSplit.Qty;
						pickedSplit.PickedQty = tempOriginalSplit.PickedQty;
						pickedSplit.ExpireDate = tempOriginalSplit.ExpireDate;
						pickedSplit = Picked.Update(pickedSplit);
					}
				}
				else
				{
					if (header.IsReturnMode && header.Remove == false)
						pickedSplit.LocationID = header.LocationID;
					pickedSplit.LotSerialNbr = header.LotSerialNbr;
					if (header.LotSerTrackExpiration == true && header.ExpireDate != null)
						pickedSplit.ExpireDate = header.ExpireDate;
					pickedSplit.PickedQty += qty;
					pickedSplit = Picked.Update(pickedSplit);
				}
			}
			else
			{
				var existingSplit = pickedSplit.IsUnassigned == true || header.LotSerTrack == INLotSerTrack.LotNumbered
					? Picked.SelectMain().FirstOrDefault(s => s.LineNbr == pickedSplit.LineNbr && s.IsUnassigned == false && s.LotSerialNbr == (header.LotSerialNbr ?? s.LotSerialNbr) && IsSelectedSplit(s, header))
					: null;

				bool suppressMode = pickedSplit.IsUnassigned == false && header.LotSerTrack == INLotSerTrack.LotNumbered;

				if (existingSplit != null)
				{
					existingSplit.PickedQty += qty;
					if (existingSplit.PickedQty > existingSplit.Qty)
						existingSplit.Qty = existingSplit.PickedQty;

					using (Base.lsselect.SuppressedModeScope(suppressMode))
						existingSplit = Picked.Update(existingSplit);
				}
				else
				{
					var newSplit = ((SOShipLineSplit)Picked.Cache.CreateCopy(pickedSplit));
					newSplit.SplitLineNbr = null;
					newSplit.LotSerialNbr = header.LotSerialNbr;
					if (pickedSplit.Qty - qty > 0 || pickedSplit.IsUnassigned == true)
					{
						newSplit.Qty = qty;
						newSplit.PickedQty = qty;
						newSplit.PackedQty = 0;
						newSplit.IsUnassigned = false;
						newSplit.PlanID = null;
					}
					else
					{
						newSplit.Qty = pickedSplit.Qty;
						newSplit.PickedQty = pickedSplit.PickedQty;
						newSplit.PackedQty = pickedSplit.PackedQty;
					}

					using (Base.lsselect.SuppressedModeScope(suppressMode))
						newSplit = Picked.Insert(newSplit);
				}

				if (pickedSplit.IsUnassigned == false) // Unassigned splits will be processed automatically
				{
					if (pickedSplit.Qty <= 0)
						pickedSplit = Picked.Delete(pickedSplit);
					else
					{
						pickedSplit.Qty -= qty;
						pickedSplit = Picked.Update(pickedSplit);
					}
				}
			}

			return true;
		}
		
		protected virtual bool SetLotSerialNbrAndQty(Header header, SOPickerListEntry pickedSplit, decimal qty)
		{
			var existingAssignedSplit = pickedSplit.IsUnassigned == true && header.LotSerTrack != INLotSerTrack.SerialNumbered
				? PickListOfPicker.SelectMain().FirstOrDefault(s => s.IsUnassigned == false && s.LotSerialNbr == (header.LotSerialNbr ?? s.LotSerialNbr) && IsSelectedSplit(s, header))
				: null;

			if (existingAssignedSplit != null)
			{
				existingAssignedSplit.PickedQty += qty;
				if (existingAssignedSplit.PickedQty > existingAssignedSplit.Qty)
					existingAssignedSplit.Qty = existingAssignedSplit.PickedQty;

				existingAssignedSplit = PickListOfPicker.Update(existingAssignedSplit);
			}
			else
			{
				var newSplit = (SOPickerListEntry)PickListOfPicker.Cache.CreateCopy(pickedSplit);

				newSplit.EntryNbr = null;
				newSplit.LotSerialNbr = header.LotSerialNbr;
				newSplit.Qty = qty;
				newSplit.PickedQty = qty;
				newSplit.IsUnassigned = false;
				if (header.LotSerTrackExpiration == true && header.ExpireDate != null)
					newSplit.ExpireDate = header.ExpireDate;

				newSplit = PickListOfPicker.Insert(newSplit);
			}

			return true;
		}

		protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQty != true;
		protected override bool ExplicitLineConfirmation => Setup.Current.ExplicitLineConfirmation == true;
		protected override bool DocumentLoaded => HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch) && HeaderView.Current.WorksheetNbr != null || base.DocumentLoaded;
		protected virtual bool DocumentIsConfirmed => HeaderView.Current.Mode.IsIn(Modes.PickWave, Modes.PickBatch)
			? SOPicker.PK.Find(Base, HeaderView.Current.WorksheetNbr, HeaderView.Current.PickerNbr)?.Confirmed == true 
			: Shipment?.Confirmed == true;
		protected bool CannotConfirmPartialShipments => Setup.Current.ShortShipmentConfirmation == SOPickPackShipSetup.shortShipmentConfirmation.Forbid;
		protected bool PromptLocationForEveryLine => Setup.Current.RequestLocationForEachItem == true;
		protected bool ConfirmToteForEveryLine =>
			Setup.Current.ConfirmToteForEachItem == true &&
			HeaderView.Current.Remove == false &&
			HeaderView.Current.WorksheetNbr != null &&
			Worksheet.Current?.WorksheetType == SOPickingWorksheet.worksheetType.Wave &&
			ShipmentsOfPicker.Select().Count > 1;

		public const double ScaleWeightValiditySeconds = 30;

		protected override bool DocumentIsEditable => base.DocumentIsEditable && !DocumentIsConfirmed;
		protected override string DocumentIsNotEditableMessage => Msg.ShipmentIsNotEditable;

		protected override bool RequiredConfirmation(Header header)
		{
			if (header == null)
				return false;

			if (header.ScanState == ScanStates.BoxWeight)
				return true;

			bool isConfirmed = header.Mode.IsIn(Modes.PickWave, Modes.PickBatch)
				? SOPicker.PK.Find(Base, header.WorksheetNbr, header.PickerNbr)?.Confirmed == true
				: header.RefNbr.With(nbr => SOShipment.PK.Find(Base, nbr)?.Confirmed) == true;

			if (isConfirmed)
				return false;

			return base.RequiredConfirmation(header)
				|| CanPick
				|| CanPickWS
				|| HasUnconfirmedBoxes && header.PackageLineNbr != null;
		}

		#region Constants & Messages
		public new abstract class Modes : WMSBase.Modes
		{
			public static WMSModeOf<PickPackShip, PickPackShipHost> PickWave { get; } = WMSMode("PICKW");
			public static WMSModeOf<PickPackShip, PickPackShipHost> PickBatch { get; } = WMSMode("PICKB");
			public static WMSModeOf<PickPackShip, PickPackShipHost> Pick { get; } = WMSMode("PICK");
			public static WMSModeOf<PickPackShip, PickPackShipHost> Pack { get; } = WMSMode("PACK");
			public static WMSModeOf<PickPackShip, PickPackShipHost> Ship { get; } = WMSMode("SHIP");

			public class pickWave : BqlString.Constant<pickWave> { public pickWave() : base(PickWave) { } }
			public class pickBatch : BqlString.Constant<pickBatch> { public pickBatch() : base(PickBatch) { } }
			public class pick : BqlString.Constant<pick> { public pick() : base(Pick) { } }
			public class pack : BqlString.Constant<pack> { public pack() : base(Pack) { } }
			public class ship : BqlString.Constant<ship> { public ship() : base(Ship) { } }
		}

		public new abstract class ScanStates : WMSBase.ScanStates
		{
			[Obsolete]
			public const string PickConfirmation = "PICK";
			[Obsolete]
			public const string PackConfirmation = "PACK";

			public const string Box = "BOX";
			public const string BoxWeight = "BWGT";
			public const string Carrier = "RATE";
			public const string Confirm = "CONF";
			public const string SortingLocation = "SLOC";
			public const string AssignTote = "ASST";
			public const string ConfirmTote = "CNFT";
			public const string RemoveFromTote = "RMFT";
		}

		public new abstract class ScanCommands : WMSBase.ScanCommands
		{
			public const string CartIn = Marker + "CART*IN";
			public const string CartOut = Marker + "CART*OUT";

			public const string ConfirmShipment = Marker + "CONFIRM*SHIPMENT";
			public const string ConfirmShipmentAll = Marker + "CONFIRM*SHIPMENT*ALL";
			public const string ConfirmPickList = Marker + "CONFIRM*PICK";
		}

		[PXLocalizable]
		public new abstract class Msg : WMSBase.Msg
		{
			public const string PickMode = "PICK";
			public const string PackMode = "PACK";
			public const string ShipMode = "SHIP";

			public const string PickConfirmationPrompt = "Confirm picking of {1} {0}.";
			public const string PackConfirmationPrompt = "Confirm package or scan the next item.";
			public const string PackNoPickConfirmationPrompt = "Confirm package, or scan the next item or the next location.";

			public const string InventoryAddedToTote = "{0} x {1} {2} has been added to the {3} tote.";
			public const string InventoryRemovedFromTote = "{0} x {1} {2} has been removed from the {3} tote.";

			public const string InventoryMissingInShipment = "The {0} inventory item is not present in the shipment.";
			public const string LocationMissingInShipment = "The {0} location is not present in the shipment.";
			public const string LotSerialMissingInShipment = "The {0} lot/serial number is not present in the shipment.";

			public const string InventoryMissingInPickList = "The {0} inventory item is not present in the pick list.";
			public const string LocationMissingInPickList = "The {0} location is not present in the pick list.";
			public const string LotSerialMissingInPickList = "The {0} lot/serial number is not present in the pick list.";

			public const string SortingLocationPrompt = "Scan the sorting location.";
			public const string SortingLocationMissing = "The {0} location cannot be selected because it is not a sorting location.";

			public const string TotePrompt = "Scan the tote barcode for the {0} shipment.";
			public const string ToteAssigned = "The {0} tote is selected for the {1} shipment.";
			public const string ToteMissing = "The {0} tote is not found.";
			public const string ToteInactive = "The {0} tote is inactive.";
			public const string AssignedToteCannotBeUsedSeparatly = "The {0} tote cannot be used separately from the cart.";
			public const string ToteBusy = "The {0} tote cannot be selected because it is already assigned to another shipment.";
			public const string ToteAlreadyAssignedCannotAssignCart = "Totes from the {0} cart cannot be auto assigned to the pick list because it already has manual assignments.";
			public const string TotesAreNotEnoughInCart = "There are not enough active totes in the {0} cart to assign them to all of the shipments of the pick list.";
			public const string TotesFromCartAreAssigned = "The {0} first totes from the {1} cart were automatically assigned to the shipments of the pick list.";

			public const string ToteConfirmPrompt = "Scan the barcode of the {0} tote to confirm picking of the items.";
			public const string ToteMismatch = "Incorrect tote barcode ({0}) has been scanned.";
			public const string ToteToRemoveFromPrompt = "Scan the barcode of a tote from which you want to remove the items.";
			public const string ToteToRemoveFromReady = "The {0} tote is selected.";

			public const string CarrierPrompt = "Select a carrier.";

			public const string BoxPrompt = "Scan the box barcode.";
			public const string BoxReady = "The {0} box is selected.";
			public const string BoxMissing = "The {0} box is not found in the database.";
			public const string BoxWeightPrompt = "Enter the actual total weight of the package. To skip weighting, click OK.";
			public const string BoxWeightNoSkip = "The package does not have a predefined weight. Enter the package weight.";
			public const string BoxConfirmPrompt = "Confirm the package.";
			public const string BoxConfirm = "The {0} package is ready to be confirmed. The calculated weight is {1} {2}.";
			public const string BoxConfirmed = "The package is confirmed, and its weight is set to {0} {1}.";
			public const string BoxCanNotPack = "Cannot pack {1} {2} of {0}.";
			public const string BoxCanNotUnpack = "Cannot unpack {1} {2} of {0}.";

			public const string NothingToPick = "No items to pick.";
			public const string NothingToPack = "No items to pack.";

			public const string PackedQtyPerBox = "Packed Qty.";

			public const string ShipmentPrompt = "Scan the shipment number.";
			public const string ShipmentReady = "The {0} shipment is loaded and ready to be processed.";
			public const string ShipmentNbrMissing = "The {0} shipment is not found.";
			public const string ShipmentInvalid = "The {0} shipment has the status invalid for processing.";
			public const string ShipmentInvalidSite = "The warehouse specified in the {0} shipment differs from the warehouse assigned to the selected cart.";
			public const string ShipmentHasNonStocKit = "The {0} shipment cannot be processed because it contains a non-stock kit item.";
			public const string ShipmentShouldBePickedFirst = "The {0} shipment cannot be packed because the items have not been picked.";
			public const string ShipmentCannotBePacked = "The {0} shipment cannot be processed in the Pack mode because the shipment has two or more packages assigned.";
			public const string ShipmentCannotBePickedSeparately = "The {0} shipment cannot be picked individually because the shipment is assigned to the {1} picking worksheet.";
			public const string ShipmentPicked = "The {0} shipment is picked.";
			public const string ShipmentPacked = "The {0} shipment is packed.";
			public const string PickListPicked = "The {0} pick list is picked.";
			public const string ShipmentIsNotEditable = "The shipment became unavailable for editing. Contact your manager.";

			public const string ShipmentCannotBeConfirmed = "The shipment cannot be confirmed because no items have been picked.";
			public const string ShipmentCannotBeConfirmedNoPacked = "The shipment cannot be confirmed because no items have been packed.";
			public const string ShipmentCannotBeConfirmedInPart = "The shipment cannot be confirmed because it is not complete.";
			public const string ShipmentShouldNotBeConfirmedInPart = "The shipment is incomplete and should not be confirmed. Do you want to confirm the shipment?";

			public const string ShipmentConfirming = "The {0} shipment is being confirmed.";
			public const string ShipmentIsConfirmed = "The shipment is successfully confirmed.";
			public const string ShipmentConfirmationFailed = "The shipment confirmation failed.";

			public const string WorksheetPrompt = "Scan the picking worksheet number.";
			public const string WorksheetReady = "The {0} picking worksheet is loaded and ready to be processed.";
			public const string WorksheetNbrMissing = "The {0} picking worksheet is not found.";
			public const string WorksheetInvalid = "The {0} picking worksheet has the status invalid for processing.";
			public const string WorksheetInvalidSite = "The warehouse specified in the {0} picking worksheet differs from the warehouse assigned to the selected cart.";

			public const string PickListConfirming = "The {0} pick list is being confirmed.";
			public const string PickListIsConfirmed = "The pick list is successfully confirmed.";
			public const string PickListConfirmationFailed = "The pick list confirmation failed.";

			public const string PickListCannotBeConfirmed = "The pick list cannot be confirmed because no items have been picked.";
			public const string PickListCannotBeConfirmedInPart = "The pick list cannot be confirmed because it is not complete.";
			public const string PickListShouldNotBeConfirmedInPart = "The pick list is incomplete and should not be confirmed. Do you want to confirm the pick list?";

			public const string PickerPositionMissing = "The picker slot {0} is not found in the {1} picking worksheet.";
			public const string PickerPositionOccupied = "The picker slot {0} is already assigned to another user in the {1} picking worksheet.";

			public const string Overpicking = "The picked quantity cannot be greater than the quantity in shipment line.";
			public const string Underpicking = "The picked quantity cannot become negative.";
			public const string UnderpickingByPack = "The picked quantity cannot be less than the already packed quantity.";

			public const string LinkCartOverpicking = "Link quantity cannot be greater than the quantity of a cart line split.";
			public const string LinkUnderpicking = "Link quantity cannot be negative.";

			public const string ScaleMissing = "The {0} scale is not found in the database.";
			public const string ScaleDisconnected = "No information from the {0} scales. Check connection of the scales.";
			public const string ScaleTimeout = "Measurement on the {0} scale is more than {1} seconds old. Remove the package from the scale and weigh it again.";
			public const string ScaleNoBox = "No information from the {0} scales. Make sure that items are on the scales.";
			public const string ScaleSkipPrompt = "Put a package on the scales and click OK. To skip weighting, do not use the scales and click OK.";
			public const string PackageWrongWeightUnit = "Wrong weight unit: {0}, only KG and LB are supported.";
		}
		#endregion
	}
}