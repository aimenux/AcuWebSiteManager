using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Common;
using PX.Objects.Common;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.Extensions;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.SO.PickPackShip, PX.Objects.SO.PickPackShipHost, PX.Objects.SO.SOShipment, PX.Objects.SO.PickPackShip.Header>;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.SM;

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
					fits &= header.LotSerialNbr == row.LotSerialNbr || header.Mode == Modes.Pick && header.LotSerAssign == INLotSerAssign.WhenUsed && row.PickedQty == 0;
				return fits;
			}
		}

		[PXUIField(Visible = false)]
		public class ShowPick : PXFieldAttachedTo<Header>.By<PickPackShipHost>.AsBool.Named<ShowPick>
		{
			public override bool? GetValue(Header row) => Base.WMS.Setup.Current.ShowPickTab == true && row.Mode.IsIn(Modes.Free, Modes.Pick);
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
		#endregion

		#region DACs
		public class Header : WMSHeader, ILSMaster
		{
			#region ShipmentNbr
			[PXUnboundDefault]
			[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXUIField(DisplayName = "Shipment Nbr.", Enabled = false)]
			[PXSelector(typeof(
				Search2<SOShipment.shipmentNbr,
				InnerJoin<INSite, On<INSite.siteID, Equal<SOShipment.siteID>>,
				LeftJoinSingleTable<Customer, On<SOShipment.customerID, Equal<Customer.bAccountID>>>>,
				Where2<Match<INSite, Current<AccessInfo.userName>>,
					And<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>>>))]
			[PXRestrictor(typeof(Where<SOShipment.status, Equal<SOShipmentStatus.open>, And<Where<
				Current<Header.mode>, NotIn3<Modes.pack, Modes.ship>,
				Or<SOShipment.operation, NotEqual<SOOperation.receipt>>>>>), Msg.ShipmentInvalid, typeof(SOShipment.shipmentNbr))]
			[PXRestrictor(typeof(Where<SOShipment.siteID, Equal<Current<Header.siteID>>, Or<Current2<Header.siteID>, IsNull>>), Msg.ShipmentInvalidSite, typeof(SOShipment.shipmentNbr))]
			public override string RefNbr { get; set; }
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
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
			[PXUIVisible(typeof(Where<mode, Equal<Modes.pick>, And2<FeatureInstalled<FeaturesSet.wMSCartTracking>, And<Current<SOPickPackShipSetup.useCartsForPick>, Equal<True>>>>))]
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
				DescriptionField = typeof(SOPackageDetailEx.boxID), DirtyRead = true)]
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
		}
		#endregion

		#region Views
		public override PXFilter<Header> HeaderView => Base.HeaderView;

		public PXSetupOptional<SOPickPackShipSetup, Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>> Setup;

		public SelectFrom<SOShipmentProcessedByUser>.
			Where<SOShipmentProcessedByUser.FK.Shipment.SameAsCurrent.
				And<SOShipmentProcessedByUser.userID.IsEqual<AccessInfo.userID.FromCurrent>>>.
			View ShipmentProcessedByUser;

		#region Pick
		public
			SelectFrom<SOShipLineSplit>
			.InnerJoin<SOShipLine>.On<SOShipLineSplit.FK.ShipLine>
			.OrderBy<SOShipLineSplit.shipmentNbr.Asc, SOShipLineSplit.isUnassigned.Desc, SOShipLineSplit.lineNbr.Asc>
			.View Picked;
		protected virtual IEnumerable picked()
		{
			return Enumerable.Concat(
					SelectFrom<SOShipLineSplit>
					.InnerJoin<SOShipLine>.On<SOShipLineSplit.FK.ShipLine>
					.Where<SOShipLineSplit.shipmentNbr.IsEqual<Header.refNbr.FromCurrent>>
					.View.Select(Base).AsEnumerable()
					.Cast<PXResult<SOShipLineSplit, SOShipLine>>(),
					HeaderView.Current.Mode == Modes.Pick || NoPick
						?	SelectFrom<Unassigned.SOShipLineSplit>
							.InnerJoin<SOShipLine>.On<Unassigned.SOShipLineSplit.FK.ShipLine>
							.Where<Unassigned.SOShipLineSplit.shipmentNbr.IsEqual<Header.refNbr.FromCurrent>>
							.View.Select(Base).AsEnumerable()
							.Cast<PXResult<Unassigned.SOShipLineSplit, SOShipLine>>()
							.Select(r => new PXResult<SOShipLineSplit, SOShipLine>(MakeAssigned(r), r))
						: Enumerable.Empty<PXResult<SOShipLineSplit, SOShipLine>>())
				.ToArray();
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
			PXSelect<SOPackageDetailEx,
			Where<SOPackageDetailEx.shipmentNbr, Equal<Current<Header.refNbr>>,
				And<Where<
					Current2<Header.packageLineNbrUI>, IsNull,
					Or<SOPackageDetailEx.lineNbr, Equal<Current2<Header.packageLineNbrUI>>>>>>>
			ShownPackage;

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

		public PXAction<Header> ReviewPick;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable reviewPick(PXAdapter adapter) => adapter.Get();

		public PXAction<Header> ReviewPack;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable reviewPack(PXAdapter adapter)
		{
			HeaderView.Current.PackageLineNbrUI = null;
			return adapter.Get();
		}

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
					clone.CarrierRatesExt.UpdateRates();
				});
			}

			return adapter.Get();
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

			if (IsCartRequired(e.Row))
				Cart.Current = Cart.Select();

			ScanModePick.SetEnabled(e.Row.Mode != Modes.Pick);
			ScanModePack.SetEnabled(e.Row.Mode != Modes.Pack);
			ScanModeShip.SetEnabled(e.Row.Mode != Modes.Ship);

			bool isNotConfirmed = HeaderView.Current.RefNbr.With(nbr => SOShipment.PK.Find(Base, nbr)?.Confirmed) == false;
			if (isNotConfirmed == false)
			{
				new[] {
					Picked.Cache,
					Base.Packages.Cache,
					Base.PackageDetailExt.PackageDetailSplit.Cache
				}.Modify(c => c.Adjust<PXUIFieldAttribute>().ForAllFields(a => a.Enabled = false)).Consume();
			}

			new[] {
				Picked.Cache,
				Base.Packages.Cache,
				Base.PackageDetailExt.PackageDetailSplit.Cache
			}
			.Modify(c => c.AllowInsert = c.AllowUpdate = c.AllowDelete = isNotConfirmed)
			.Consume();

			ReviewPick.SetVisible(Base.IsMobile && e.Row.Mode == Modes.Pick);
			ReviewPack.SetVisible(Base.IsMobile && e.Row.Mode == Modes.Pack);

			ScanRemove.SetEnabled(e.Row.Remove == false && isNotConfirmed && (e.Row.Mode == Modes.Pack).Implies(HasUnconfirmedBoxes));
			new[] {
				ScanConfirmShipment,
				ScanConfirmShipmentAll,
				RefreshRates,
				GetReturnLabels
			}.Modify(b => b.SetEnabled(isNotConfirmed)).Consume();

			ScanConfirmShipmentAll.SetVisible(false);
			ScanRemove.SetVisible(e.Row.Mode != Modes.Ship);

			bool hasNotConfirmedLines = isNotConfirmed && (CanPick || HasUnconfirmedBoxes && e.Row.PackageLineNbr != null || e.Row.Remove == true);
			ScanConfirm.SetEnabled(hasNotConfirmedLines || e.Row.ScanState == ScanStates.BoxWeight);

			if (String.IsNullOrEmpty(e.Row.RefNbr))
				Base.Document.Current = null;
			else
				Base.Document.Current = Base.Document.Search<SOShipment.shipmentNbr>(e.Row.RefNbr);
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
		#endregion

		private SOShipment Shipment => SOShipment.PK.Find(Base, Base.Document.Current);
		protected override BqlCommand DocumentSelectCommand()
			=> new SelectFrom<SOShipment>
				.Where<SOShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr.AsOptional>>();

		protected override WMSModeOf<PickPackShip, PickPackShipHost> DefaultMode =>
			Setup.Current.ShowPickTab == true ? Modes.Pick :
			Setup.Current.ShowPackTab == true ? Modes.Pack :
			Setup.Current.ShowShipTab == true ? Modes.Ship :
			Modes.Free;
		public override string CurrentModeName => 
			HeaderView.Current.Mode == Modes.Pick ? Msg.PickMode :
			HeaderView.Current.Mode == Modes.Pack ? Msg.PackMode :
			HeaderView.Current.Mode == Modes.Ship ? Msg.ShipMode :
			Msg.FreeMode;
		protected override string GetModePrompt()
		{
			if (HeaderView.Current.Mode == Modes.Pick)
			{
				if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
					return Localize(Msg.CartPrompt);
				if (HeaderView.Current.RefNbr == null)
					return Localize(Msg.ShipmentPrompt);
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
		protected override string GetDefaultState(Header header = null) => IsCartRequired(header ?? HeaderView.Current) ? ScanStates.Cart : ScanStates.RefNbr;

		protected override bool ProcessCommand(string barcode)
		{
			switch (barcode)
			{
				case ScanCommands.Confirm:
					if (Shipment?.Confirmed == true)
						return true;
					if (HeaderView.Current.Mode == Modes.Pick)
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
							if (Setup.Current.ConfirmEachPackageWeight == false)
								SkipBoxWeightInput();
							return true;
						}
					}
					return false;

				case ScanCommands.Remove:
					if (Shipment?.Confirmed == true) return true;
					HeaderView.Current.Remove = true;
					if (HeaderView.Current.ScanState == ScanStates.Command)
					{
						if (HeaderView.Current.Mode == Modes.Pick)
							SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item, Msg.RemoveMode);
						else if (HeaderView.Current.Mode == Modes.Pack)
							SetScanState(ScanStates.Box, Msg.RemoveMode);
					}
					else
					{
						Report(Msg.RemoveMode);
					}
					return true;

				case ScanCommands.CartIn:
					if (IsCartRequired(HeaderView.Current) == false) return false;
					ClearHeaderInfo();
					HeaderView.Current.CartLoaded = false;
					if (CanPick)
						SetScanState(ScanStates.Location, Msg.CartLoading);
					else
						SetScanState(ScanStates.Command, Msg.CartLoading);
					return true;

				case ScanCommands.CartOut:
					if (IsCartRequired(HeaderView.Current) == false) return false;
					ClearHeaderInfo();
					HeaderView.Current.CartLoaded = true;
					if (CanPick)
						SetScanState(ScanStates.Item, Msg.CartUnloading);
					else
						SetScanState(ScanStates.Command, Msg.CartUnloading);
					return true;

				case ScanCommands.ConfirmShipment:
					if (Shipment?.Confirmed == true) return true;
					ConfirmShipment(false);
					return true;

				case ScanCommands.ConfirmShipmentAll:
					if (Shipment?.Confirmed == true) return true;
					ConfirmShipment(true);
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
					if (IsCartRequired(HeaderView.Current))
						Prompt(Msg.CartPrompt);
					else
						SetScanState(ScanStates.RefNbr);
					break;
				case ScanStates.RefNbr:
					Prompt(Msg.ShipmentPrompt);
					break;
				case ScanStates.Item:
					Prompt(Msg.InventoryPrompt);
					break;
				case ScanStates.Location:
					if (IsLocationRequired(HeaderView.Current))
						Prompt(Msg.LocationPrompt);
					else if (HeaderView.Current.Mode == Modes.Pick && CanPick == false && HeaderView.Current.Remove == false)
						SetScanState(ScanStates.Command);
					else
						SetScanState(HeaderView.Current.Mode == Modes.Pick && ViseVersaFlow(HeaderView.Current) ? ScanStates.Confirm : ScanStates.Item);
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
						HeaderView.Cache.SetValueExt<Header.packageLineNbr>(HeaderView.Current, package.LineNbr);
						Base.Packages.Current = package;
						SetScanState(NoPick ? ScanStates.Location : ScanStates.Item);
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
					else if (HeaderView.Current.Mode == Modes.Pick) // include PickWave and PickBatch in further versions
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

		protected override bool CompleteRedirect(string command)
		{
			switch (command)
			{
				case ScanRedirects.ModePick:
					if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
						SetScanState(ScanStates.Cart);
					else if (HeaderView.Current.RefNbr == null)
						SetScanState(ScanStates.RefNbr);
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
			bool isReturn = false;
			var shipment = (SOShipment)PXSelectorAttribute.Select<Header.refNbr>(HeaderView.Cache, HeaderView.Current, barcode);
			if (shipment == null && HeaderView.Current.Mode == Modes.Pick)
			{
				shipment =
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
				isReturn = shipment != null;
			}

			string shipmentNbr = shipment?.ShipmentNbr ?? barcode;

			if (shipment == null)
			{
				ReportError(Msg.ShipmentNbrMissing, shipmentNbr);
			}
			else if (!isReturn && IsValid<Header.refNbr>(shipmentNbr, out string error) == false)
			{
				ReportError(error);
			}
			else
			{
				if (HeaderView.Current.Mode == Modes.Pack)
					HasSingleAutoPackage(shipment.ShipmentNbr, out var _);

				HeaderView.Current.RefNbr = shipment.ShipmentNbr;
				HeaderView.Current.SiteID = shipment.SiteID;
				HeaderView.Current.TranDate = shipment.ShipDate;
				HeaderView.Current.NoteID = shipment.NoteID;
				HeaderView.Current.InvtMult = (short)(shipment.Operation != SOOperation.Receipt ? -1 /* Issue */ : 1 /* Receipt */);
				Base.Document.Current = shipment;

				Report(Msg.ShipmentReady, shipment.ShipmentNbr);

				if (HeaderView.Current.Mode == Modes.Pick)
				{
					if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
						SetScanState(ScanStates.Cart);
					else
						SetScanState(CanPick ? ViseVersaFlow(HeaderView.Current) ? ScanStates.Item : ScanStates.Location : ScanStates.Command);
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
		}

		protected override void ProcessCartBarcode(string barcode)
		{
			// for pick mode only
			if (HeaderView.Current.Mode != Modes.Pick)
				throw new NotSupportedException();

			INCart cart = ReadCartByBarcode(barcode);
			if (cart == null)
			{
				ReportError(Msg.CartMissing, barcode);
			}
			else if (HeaderView.Current.RefNbr != null && Shipment.SiteID != cart.SiteID)
			{
				ReportError(Msg.CartInvalidSite, barcode);
			}
			else
			{
				HeaderView.Current.CartID = cart.CartID;
				HeaderView.Current.SiteID = cart.SiteID;
				Cart.Current = Cart.Select();

				Report(Msg.CartReady, cart.CartCD);

				if (HeaderView.Current.RefNbr == null)
					SetScanState(ScanStates.RefNbr);
				else
					SetScanState(CanPick ? ViseVersaFlow(HeaderView.Current) ? ScanStates.Item : ScanStates.Location : ScanStates.Command);
			}
		}

		protected override void ProcessLocationBarcode(string barcode)
		{
			INLocation location = ReadLocationByBarcode(HeaderView.Current.SiteID, barcode);
			if (location == null)
				return;

			if (!ViseVersaFlow(HeaderView.Current) && Picked.SelectMain().All(t => t.LocationID != location.LocationID))
			{
				ReportError(Msg.LocationMissingInShipment, location.LocationCD);
			}
			else
			{
				HeaderView.Current.LocationID = location.LocationID;
				SetScanState(ViseVersaFlow(HeaderView.Current) ? ScanStates.Confirm : ScanStates.Item, Msg.LocationReady, location.LocationCD);
			}
		}

		protected override void ProcessItemBarcode(string barcode)
		{
			var item = ReadItemByBarcode(barcode, INPrimaryAlternateType.CPN);
			if (item == null)
			{
				if (HandleItemAbsence(barcode) == false)
					ReportError(Msg.InventoryMissing, barcode);
				return;
			}

			INItemXRef xref = item;
			InventoryItem inventoryItem = item;
			INLotSerClass lsclass = item;

			if (Picked.SelectMain().All(t => t.InventoryID != inventoryItem.InventoryID))
			{
				ReportError(Msg.InventoryMissingInShipment, inventoryItem.InventoryCD);
				return;
			}

			string uom;
			if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				uom = xref.UOM ?? inventoryItem.BaseUnit;
				if (uom != inventoryItem.BaseUnit)
				{
					ReportError(Msg.SerialItemNotComplexQty);
					return;
				}
			}
			else
			{
				uom = xref.UOM ?? inventoryItem.SalesUnit;
			}

			HeaderView.Current.InventoryID = xref.InventoryID;
			HeaderView.Current.SubItemID = xref.SubItemID;
			if (HeaderView.Current.UOM == null)
				HeaderView.Current.UOM = uom;
			HeaderView.Current.LotSerTrack = lsclass.LotSerTrack;
			HeaderView.Current.LotSerAssign = lsclass.LotSerAssign;
			HeaderView.Current.LotSerTrackExpiration = lsclass.LotSerTrackExpiration;
			HeaderView.Current.LotSerIssueMethod = lsclass.LotSerIssueMethod;
			HeaderView.Current.AutoNextNbr = lsclass.AutoNextNbr;

			Report(Msg.InventoryReady, inventoryItem.InventoryCD);

			if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered && (DefaultLotSerial == false || lsclass.LotSerAssign == INLotSerAssign.WhenUsed || lsclass.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable)) // TODO: make same as for putaway
				SetScanState(ScanStates.LotSerial);
			else if (HeaderView.Current.Mode == Modes.Pick)
				SetScanState(ViseVersaFlow(HeaderView.Current) ? ScanStates.Location : ScanStates.Confirm);
			else if (HeaderView.Current.Mode == Modes.Pack)
				SetScanState(ScanStates.Confirm);
		}

		protected virtual bool HandleItemAbsence(string barcode)
		{
			if (HeaderView.Current.Mode == Modes.Pick
				? !ViseVersaFlow(HeaderView.Current) && !PromptLocationForEveryLine && IsCartRequired(HeaderView.Current).Implies(HeaderView.Current.CartLoaded != true)
				: NoPick)
			{
				ProcessLocationBarcode(barcode);
				if (Info.Current.MessageType == WMSMessageTypes.Information)
					return true; // location found
			}

			if (HeaderView.Current.Mode == Modes.Pack && HeaderView.Current.Remove == false)
			{
				CSBox box = CSBox.PK.Find(Base, barcode);
				if (box != null)
				{
					if (!AutoConfirmPackage(Setup.Current.ConfirmEachPackageWeight == false))
						return true;

					ProcessBoxBarcode(barcode);
					if (Info.Current.MessageType == WMSMessageTypes.Information)
						return true; // box processed
				}
			}

			return false;
		}

		protected override void ProcessLotSerialBarcode(string barcode)
		{
			if (IsValid<Header.lotSerialNbr>(barcode, out string error) == false)
			{
				ReportError(error);
				return;
			}

			if (HeaderView.Current.LotSerAssign != INLotSerAssign.WhenUsed &&
				HeaderView.Current.LotSerIssueMethod != INLotSerIssueMethod.UserEnterable &&
				Picked.SelectMain().All(t => t.LotSerialNbr != barcode))
			{
				ReportError(Msg.LotSerialMissingInShipment, barcode);
				return;
			}

			HeaderView.Current.LotSerialNbr = barcode;
			Report(Msg.LotSerialReady, barcode);
			bool entarableExpirationDate = 
				HeaderView.Current.Remove == false
				&& HeaderView.Current.LotSerTrackExpiration == true
				&& (HeaderView.Current.LotSerAssign == INLotSerAssign.WhenUsed || HeaderView.Current.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable);

			if (HeaderView.Current.Mode == Modes.Pick)
			{
				if (entarableExpirationDate && Picked.SelectMain().Any(t => t.IsUnassigned == true || t.LotSerialNbr == barcode && t.PickedQty == 0)) // TODO: make same as for Receive
					SetScanState(ScanStates.ExpireDate);
				else
					SetScanState(ViseVersaFlow(HeaderView.Current) ? ScanStates.Location : ScanStates.Confirm);
			}
			else if (HeaderView.Current.Mode == Modes.Pack)
			{
				if (NoPick && entarableExpirationDate && Picked.SelectMain().Any(t => t.IsUnassigned == true || t.LotSerialNbr == barcode && t.PackedQty == 0))
					SetScanState(ScanStates.ExpireDate);
				else
					SetScanState(ScanStates.Confirm);
			}
		}

		protected override void ProcessExpireDate(string barcode)
		{
			if (DateTime.TryParse(barcode.Trim(), out DateTime value) == false)
			{
				ReportError(Msg.LotSerialExpireDateBadFormat);
				return;
			}

			if (IsValid<Header.expireDate>(value, out string error) == false)
			{
				ReportError(error);
				return;
			}

			HeaderView.Current.ExpireDate = value;
			Report(Msg.LotSerialExpireDateReady, barcode);

			if (HeaderView.Current.Mode == Modes.Pick)
				SetScanState(ViseVersaFlow(HeaderView.Current) ? ScanStates.Location : ScanStates.Confirm);
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

			HeaderView.Cache.SetValueExt<Header.packageLineNbr>(HeaderView.Current, package.LineNbr);
			Base.Packages.Current = package;
			Report(Msg.BoxReady, box.BoxID);

			if (CanPack || HeaderView.Current.Remove == true)
				SetScanState(Setup.Current.ShowPickTab == false ? ScanStates.Location : ScanStates.Item);
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
			bool isSerialItem = HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered;
			bool noQtyState = HeaderView.Current.PrevScanState != ScanStates.Qty;

			if (IsCartRequired(HeaderView.Current) == false)
				ConfirmPickedNoCart();
			else if (HeaderView.Current.CartLoaded == false)
				ConfirmPickedInCart();
			else
				ConfirmPickedOutCart();

			if (!isSerialItem && noQtyState && Info.Current.MessageType == WMSMessageTypes.Information)
				HeaderView.Current.IsQtyOverridable = true;
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
					.OrderByDescending(split => split.IsUnassigned == false)
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

						if (pickedSplit.LotSerialNbr != header.LotSerialNbr && (header.LotSerAssign == INLotSerAssign.WhenUsed || header.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable))
						{
							if (!SetLotSerialNbrAndQty(header, pickedSplit, qty))
								return WMSFlowStatus.Fail(Msg.Overpicking);
							splitUpdated = true;
						}
					}

					if (!splitUpdated)
					{
						EnsureAssignedSplitEditing(pickedSplit);

						if (ViseVersaFlow(header) && header.Remove == false && header.LocationID != null)
							pickedSplit.LocationID = header.LocationID;
						pickedSplit.PickedQty += remove ? -qty : qty;

						if (remove && header.InvtMult == -1 && (header.LotSerAssign == INLotSerAssign.WhenUsed || header.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable))
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
						ViseVersaFlow(header) || PromptLocationForEveryLine == false
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
					.OrderByDescending(split => split.IsUnassigned == false)
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
						ViseVersaFlow(header) || PromptLocationForEveryLine == false
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

		protected void EnsureCartShipmentLink()
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
				decimal? TargetQty(SOShipLineSplit s) => noPick ? s.Qty * Base.GetQtyThreshold(s) : s.PickedQty;

				if (header.PackageLineNbr == null || header.InventoryID == null || header.Qty == 0)
					return WMSFlowStatus.Fail(remove ? Msg.NothingToRemove : Msg.NothingToPack);

				var packageDetail = Base.Packages.SelectMain().FirstOrDefault(t => t.LineNbr == header.PackageLineNbr);
				var packedSplits = Picked.SelectMain().Where(
					r => r.InventoryID == header.InventoryID
							&& r.SubItemID == header.SubItemID
							&& (r.IsUnassigned == true || r.LotSerialNbr == (header.LotSerialNbr ?? r.LotSerialNbr))
							&& noPick.Implies(r.LocationID == (header.LocationID ?? r.LocationID))
							&& (remove ? r.PackedQty > 0 : TargetQty(r) > r.PackedQty));

				if (noPick)
					packedSplits = packedSplits
					.OrderByDescending(split => split.IsUnassigned == false)
					.OrderByDescending(split => remove ? split.PickedQty > 0 : split.Qty > split.PickedQty)
					.ThenByDescending(split => split.LotSerialNbr == (header.LotSerialNbr ?? split.LotSerialNbr))
					.ThenByDescending(split => string.IsNullOrEmpty(split.LotSerialNbr))
					.ThenByDescending(split => (split.Qty > split.PickedQty || remove) && split.PickedQty > 0)
					.ThenByDescending(split => Sign.MinusIf(remove) * (split.Qty - split.PickedQty));

				void KeepPackageSelection() => HeaderView.Cache.SetValueExt<Header.packageLineNbr>(HeaderView.Current, packageDetail.LineNbr);
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
					HeaderView.Cache.SetValueExt<Header.packageLineNbr>(HeaderView.Current, null);
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

			bool isSerialItem = HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered;
			bool noQtyState = HeaderView.Current.PrevScanState != ScanStates.Qty;

			var res = ExecuteAndCompleteFlow(Implementation);

			if (!isSerialItem && noQtyState && Info.Current.MessageType == WMSMessageTypes.Information)
				HeaderView.Current.IsQtyOverridable = true;

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

				if (HeaderView.Current.LotSerAssign == INLotSerAssign.WhenUsed || HeaderView.Current.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable)
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
			SOPackageDetailEx package = Base.Packages.SelectMain().FirstOrDefault(t => t.LineNbr == HeaderView.Current.PackageLineNbr);
			if (package == null)
				return false;

			HeaderView.Current.Weight = package.Weight == 0
				? AutoCalculateBoxWeightBasedOnItems(package)
				: package.Weight.Value;

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
				if (Base.PackageDetailExt.PackageDetailSplit.SelectMain(HeaderView.Current.RefNbr, HeaderView.Current.PackageLineNbrUI).Any())
				{
					if (ConfirmPackedBox())
					{
						if (skipBoxWeightInput)
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
			decimal calculatedWeight = 0m;
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
			string scaleID = UserSetup.For(Base).ScaleID;
			SMScale scale = SMScale.PK.Find(Base, scaleID);

			if (scale == null)
			{
				ReportError(Msg.ScaleMissing, scaleID);
				return false;
			}
			else if (scale.LastModifiedDateTime.Value.AddHours(1) < DateTime.Now)
			{
				ReportError(Msg.ScaleDisconnected, scaleID);
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
					ReportWarning(Msg.ScaleNoBox, scaleID);
					Prompt(Msg.ScaleSkipPrompt);
					HeaderView.Current.LastWeighingTime = scale.LastModifiedDateTime.Value;
					return null;
				}
			}
			else if (scale.LastModifiedDateTime.Value.AddSeconds(ScaleWeightValiditySeconds) < DateTime.Now)
			{
				ReportError(Msg.ScaleTimeout, scaleID, ScaleWeightValiditySeconds);
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
			SOPackageDetailEx packageDetail = Base.Packages.SelectMain().FirstOrDefault(t => t.LineNbr == HeaderView.Current.PackageLineNbr);
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

			if (IsCartRequired(HeaderView.Current) && HeaderView.Current.CartID == null)
			{
				SetScanState(ScanStates.Cart);
			}
			else if (HeaderView.Current.RefNbr == null)
			{
				SetScanState(ScanStates.RefNbr);
			}
			else if (HeaderView.Current.Mode == Modes.Pick)
			{
				SetScanState(ViseVersaFlow(HeaderView.Current) ? ScanStates.Item : ScanStates.Location);
			}
			else if (HeaderView.Current.Mode == Modes.Pack)
			{
				SetScanState(ScanStates.Box);
			}
			else if (HeaderView.Current.Mode == Modes.Ship)
			{
				SetScanState(ScanStates.Carrier);
			}
		}

		protected override void ClearHeaderInfo(bool redirect = false)
		{
			base.ClearHeaderInfo(redirect);

			HeaderView.Current.PackageLineNbr = null;
			HeaderView.Current.LotSerialNbr = null;
			HeaderView.Current.ExpireDate = null;
			HeaderView.Current.Weight = null;
			HeaderView.Current.LastWeighingTime = null;
			if (redirect)
			{
				HeaderView.Current.CartLoaded = false;
				HeaderView.Current.LocationID = null;
			}
		}
		#endregion

		protected virtual void ConfirmShipment(bool confirmAll)
		{
			if (Picked.SelectMain().All(s => s.PickedQty == 0))
			{
				ReportError(Msg.ShipmentCannotBeConfirmed);
				return;
			}

			if (Info.Current.MessageType != WMSMessageTypes.Warning && Picked.SelectMain().Any(s => s.PickedQty < s.Qty * Base.GetMinQtyThreshold(s)))
			{
				if (CannotConfirmPartialShipments)
					ReportError(Msg.ShipmentCannotBeConfirmedInPart);
				else
					ReportWarning(Msg.ShipmentShouldNotBeConfirmedInPart);
				return;
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
				return;
			}

			if (Setup.Current.ConfirmEachPackageWeight == false && AutoConfirmPackage(true) == false)
				return;

			CloseShipmentUserLinks();
			ClearHeaderInfo();

			Base.Document.Current.IsPackageValid = true;
			Base.Document.UpdateCurrent();

			Save.Press();

			string shipmentNbr = HeaderView.Current.RefNbr;
			bool printConfirmation = UserSetup.For(Base).PrintShipmentConfirmation == true;
			bool printCarrierLabels = UserSetup.For(Base).PrintShipmentLabels == true;
			var setup = Setup.Current;

			SOPackageDetailEx autoPackage = null;
			bool needToConfirmPackage = HeaderView.Current.Mode != Modes.Pick && HasSingleAutoPackage(shipmentNbr, out autoPackage) && autoPackage.Confirmed != true;
			
			WaitFor(
			(wArgs) =>
			{
				var shipment = wArgs.Document;
				var cmd = wArgs.DocumentCommand;

				var graph = PXGraph.CreateInstance<PickPackShipHost>();
				var copy = graph.WMS;

				SOShipmentEntry shipmentEntry = PXGraph.CreateInstance<SOShipmentEntry>();
				shipment = (SOShipment)shipmentEntry.TypedViews.GetView(cmd, true).SelectSingleBound(new object[] { shipment });
				shipmentEntry.Document.Current = shipment;

				var RequireShipping = Func.Memorize((int inventoryID) => InventoryItem.PK.Find(shipmentEntry, inventoryID).With(item => item.StkItem == true || item.NonStockShip == true));

				if (!confirmAll && (setup.ShowPickTab == true || setup.ShowPackTab == true))
				{
					PXSelectBase<SOShipLine> lines = shipmentEntry.Transactions;
					PXSelectBase<SOShipLineSplit> splits = shipmentEntry.splits;

					foreach (SOShipLine line in lines.Select())
					{
						lines.Current = line;
						decimal lineQty = 0;

						decimal GetNewQty(SOShipLineSplit split) => setup.ShowPickTab == true ? split.PickedQty ?? 0 : Math.Max(split.PickedQty ?? 0, split.PackedQty ?? 0);
						if (copy.IsNonStockKit(line.InventoryID))
						{
							// kitInventoryID -> compInventory -> qty
							var nonStockKitSpec = copy.GetNonStockKitSpec(line.InventoryID.Value).Where(pair => RequireShipping(pair.Key)).ToDictionary();
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

				if (needToConfirmPackage)
				{
					autoPackage.Confirmed = true;
					shipmentEntry.Packages.Update(autoPackage);
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>() && shipmentEntry.Document.Current.IsPackageValid == false && shipmentEntry.Packages.SelectMain().Any(p => p.PackageType == SOPackageType.Auto))
				{
					shipmentEntry.Document.Current.IsPackageValid = true;
					shipmentEntry.Document.UpdateCurrent();
				}

				if (shipmentEntry.IsDirty)
				{
					shipmentEntry.Document.Current.IsPackageValid = true;
					shipmentEntry.Document.UpdateCurrent();
					shipmentEntry.Save.Press();
				}

				if (UseExternalShippingApplication(shipment, out Carrier carrier))
				{
					// Shipping Tool will confirm the shipment.
					throw new PXRedirectToUrlException(
						$"../../Frames/ShipmentAppLauncher.html?ShipmentApplicationType={carrier.ShippingApplicationType}&ShipmentNbr={shipment.ShipmentNbr}",
						PXBaseRedirectException.WindowMode.NewWindow, true, string.Empty);
				}

				PXView.Dummy view = PXView.Dummy.For<SOShipment>(shipmentEntry);
				shipmentEntry.action.PressButton(new PXAdapter(view).Apply(it => it.Menu = SOShipmentEntryActionsAttribute.Messages.ConfirmShipment));

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
					if (printCarrierLabels)
					{
						try
						{
							shipmentEntry.PrintCarrierLabels(new List<SOShipment>() { shipmentEntry.Document.Current }, deviceHubAdapter);
						}
						catch (PXBaseRedirectException) { } //Never redirect in Pick Pack Ship
					}

					if (printConfirmation)
					{
						try
						{
							shipmentEntry.Report(deviceHubAdapter, SOReports.PrintShipmentConfirmation);
						}
						catch (PXBaseRedirectException) { } //Never redirect in Pick Pack Ship
					}
				}

				throw new PXOperationCompletedException(Msg.ShipmentIsConfirmed);
			},
			null,
			new DocumentWaitArguments(Shipment) { DocumentCommand = DocumentSelectCommand() }, 
			Msg.ShipmentConfirming, shipmentNbr);
		}

		protected override void OnWaitEnd(PXLongRunStatus status, SOShipment primaryRow)
		{
			bool sentToExternalShippingApp = status == PXLongRunStatus.Completed && UseExternalShippingApplication(primaryRow, out Carrier carrier);
			bool completed = primaryRow != null && (primaryRow.Confirmed == true || sentToExternalShippingApp);

			OnWaitEnd(status, completed, 
				!sentToExternalShippingApp
					? Msg.ShipmentIsConfirmed
					: Localize(Msg.ShipmentConfirming, primaryRow?.ShipmentNbr),
				Msg.ShipmentConfirmationFailed);
		}

		protected virtual bool UseExternalShippingApplication(SOShipment shipment, out Carrier carrier)
		{
			carrier = Carrier.PK.Find(Base, shipment.ShipVia);
			return Base.IsMobile == false && carrier != null && carrier.IsExternalShippingApplication == true;
		}

		protected bool IsSelectedSplit(SOShipLineSplit split, Header header)
		{
			return
				split.InventoryID == header.InventoryID &&
				split.SubItemID == header.SubItemID &&
				split.SiteID == header.SiteID &&
				(ViseVersaFlow(header) || split.LocationID == (header.LocationID ?? split.LocationID)) &&
				(split.LotSerialNbr == (header.LotSerialNbr ?? split.LotSerialNbr) || header.Mode == Modes.Pick &&
					(header.LotSerAssign == INLotSerAssign.WhenUsed || (header.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable && split.PackedQty == 0))
					&& header.Remove == false);
		}

		private SOShipLineSplit MakeAssigned(Unassigned.SOShipLineSplit unassignedSplit) => PropertyTransfer.Transfer(unassignedSplit, new SOShipLineSplit());

		protected override bool IsCartRequired(Header header) => base.IsCartRequired(header) && Setup.Current.UseCartsForPick == true && header.Mode == Modes.Pick && !ViseVersaFlow(header);
		protected decimal GetCartQty(SOShipLineSplit sosplit) => CartSplitLinks.SelectMain().Where(link => SOShipmentSplitToCartSplitLink.FK.ShipmentSplitLine.Match(Base, sosplit, link)).Sum(_ => _.Qty ?? 0);
		protected decimal GetOverallCartQty(SOShipLineSplit sosplit) => AllCartSplitLinks.SelectMain().Where(link => SOShipmentSplitToCartSplitLink.FK.ShipmentSplitLine.Match(Base, sosplit, link)).Sum(_ => _.Qty ?? 0);

		protected override bool IsLocationRequired(Header header) => base.IsLocationRequired(header) && DefaultLocation == false;

		protected bool DefaultLocation => UserSetup.For(Base).DefaultLocationFromShipment == true;
		protected bool DefaultLotSerial => UserSetup.For(Base).DefaultLotSerialFromShipment == true;
		protected bool NoPick => Setup.Current.ShowPickTab == false;
		protected virtual bool CanPick => HeaderView.Current.Mode == Modes.Pick && (Picked.SelectMain().Any(s => s.PickedQty < s.Qty));
		protected virtual bool CanPack => HeaderView.Current.Mode == Modes.Pack && Picked.SelectMain().Any(s => Setup.Current.ShowPickTab == true ? s.PickedQty > 0 && s.PackedQty < s.PickedQty : s.PackedQty < s.Qty);
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

		protected void CloseShipmentUserLinks()
		{
			var pickingTimeout = TimeSpan.FromMinutes(10);

			DateTime businessNow = Base.Accessinfo.BusinessDate.Value.Add(DateTime.Now.TimeOfDay); // TODO: change it to server time
			foreach (SOShipmentProcessedByUser shipByUser in
				SelectFrom<SOShipmentProcessedByUser>.
				Where<SOShipmentProcessedByUser.FK.Shipment.SameAsCurrent>.
				View.Select(Base))
			{
				shipByUser.Confirmed = true;
				if (shipByUser.EndDateTime == null)
					shipByUser.EndDateTime = Tools.Min(businessNow, shipByUser.LastModifiedDateTime.Value.Add(pickingTimeout));

				ShipmentProcessedByUser.Cache.Update(shipByUser);
			}
		}

		protected virtual void UpdateRates()
		{
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
					SOShipLineSplit originalSplit = new PXSelectJoin<SOShipLineSplit,
						InnerJoin<SOLineSplit, On<SOShipLineSplit.origOrderType, Equal<SOLineSplit.orderType>,
							And<SOShipLineSplit.origOrderNbr, Equal<SOLineSplit.orderNbr>,
							And<SOShipLineSplit.origLineNbr, Equal<SOLineSplit.lineNbr>,
							And<SOShipLineSplit.origSplitLineNbr, Equal<SOLineSplit.splitLineNbr>>>>>>,
						Where<SOShipLineSplit.shipmentNbr, Equal<Current<Header.refNbr>>,
							And<SOLineSplit.lotSerialNbr, Equal<Required<SOLineSplit.lotSerialNbr>>>>>
							(Base).Select(header.LotSerialNbr);

					if (originalSplit == null)
					{
						if (ViseVersaFlow(header) && header.Remove == false)
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
						if (ViseVersaFlow(header) && header.Remove == false)
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
					if (ViseVersaFlow(header) && header.Remove == false)
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

		protected bool ViseVersaFlow(Header header) => header.Mode == Modes.Pick && header.InvtMult == 1;

		protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQty != true;
		protected override bool ExplicitLineConfirmation => Setup.Current.ExplicitLineConfirmation == true;
		protected bool CannotConfirmPartialShipments => Setup.Current.ShortShipmentConfirmation == SOPickPackShipSetup.shortShipmentConfirmation.Forbid;
		protected bool PromptLocationForEveryLine => Setup.Current.RequestLocationForEachItem == true;
		public const double ScaleWeightValiditySeconds = 30;

		protected override bool DocumentIsEditable => base.DocumentIsEditable && Shipment.Confirmed == false;
		protected override string DocumentIsNotEditableMessage => Msg.ShipmentIsNotEditable;

		#region Constants & Messages
		public new abstract class Modes : WMSBase.Modes
		{
			public static WMSModeOf<PickPackShip, PickPackShipHost> Pick { get; } = WMSMode("PICK");
			public static WMSModeOf<PickPackShip, PickPackShipHost> Pack { get; } = WMSMode("PACK");
			public static WMSModeOf<PickPackShip, PickPackShipHost> Ship { get; } = WMSMode("SHIP");

			public class pick : PX.Data.BQL.BqlString.Constant<pick> { public pick() : base(Pick) { } }
			public class pack : PX.Data.BQL.BqlString.Constant<pack> { public pack() : base(Pack) { } }
			public class ship : PX.Data.BQL.BqlString.Constant<ship> { public ship() : base(Ship) { } }
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
		}

		public new abstract class ScanCommands : WMSBase.ScanCommands
		{
			public const string CartIn = Marker + "CART*IN";
			public const string CartOut = Marker + "CART*OUT";

			public const string ConfirmShipment = Marker + "CONFIRM*SHIPMENT";
			public const string ConfirmShipmentAll = Marker + "CONFIRM*SHIPMENT*ALL";
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

			public const string InventoryMissingInShipment = "The {0} inventory item is not present in the shipment.";
			public const string LocationMissingInShipment = "The {0} location is not present in the shipment.";
			public const string LotSerialMissingInShipment = "The {0} lot/serial number is not present in the shipment.";

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
			public const string ShipmentCannotBePacked = "The {0} shipment cannot be processed in the Pack mode because the shipment has two or more packages assigned.";
			public const string ShipmentPicked = "The {0} shipment is picked.";
			public const string ShipmentPacked = "The {0} shipment is packed.";
			public const string ShipmentIsNotEditable = "The shipment became unavailable for editing. Contact your manager.";

			public const string ShipmentCannotBeConfirmed = "The shipment cannot be confirmed because no items have been picked.";
			public const string ShipmentCannotBeConfirmedInPart = "The shipment cannot be confirmed because it is not complete.";
			public const string ShipmentShouldNotBeConfirmedInPart = "The shipment is incomplete and should not be confirmed. Do you want to confirm the shipment?";

			public const string ShipmentConfirming = "The {0} shipment is being confirmed.";
			public const string ShipmentIsConfirmed = "The shipment is successfully confirmed.";
			public const string ShipmentConfirmationFailed = "The shipment confirmation failed.";

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