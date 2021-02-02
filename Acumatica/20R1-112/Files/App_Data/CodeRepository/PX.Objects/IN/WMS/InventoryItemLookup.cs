using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.IN.InventoryItemLookup, PX.Objects.IN.InventoryItemLookupHost, PX.Objects.IN.InventoryItem, PX.Objects.IN.InventoryItemLookup.Header>;

namespace PX.Objects.IN
{
	public class InventoryItemLookupHost : InventorySummaryEnq
	{
		public override Type PrimaryItemType => typeof(InventoryItemLookup.Header);
		public PXFilter<InventoryItemLookup.Header> HeaderView;
	}

	public class InventoryItemLookup : WMSBase
	{
		#region DACs
		public class Header : WMSHeader
		{
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#region SiteID
			[Site(Enabled = false)]
			public virtual int? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
		}
		#endregion

		#region Buttons
		public PXAction<Header> ReviewAvailability;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable reviewAvailability(PXAdapter adapter) => adapter.Get();
		#endregion

		#region Event Handlers
		protected virtual void _(Events.FieldDefaulting<Header, Header.siteID> e) => e.NewValue = IsWarehouseRequired(e.Row) ? null : DefaultSiteID;
		protected virtual void _(Events.FieldDefaulting<WMSInfo, WMSInfo.message> e) => e.NewValue = PXMessages.LocalizeNoPrefix(Msg.CommandInventory);
		protected virtual void _(Events.FieldDefaulting<InventorySummaryEnqFilter, InventorySummaryEnqFilter.expandByLotSerialNbr> e) => e.NewValue = PXAccess.FeatureInstalled<CS.FeaturesSet.lotSerialTracking>();
		protected virtual void _(Events.FieldDefaulting<InventorySummaryEnqFilter, InventorySummaryEnqFilter.siteID> e) => e.NewValue = IsWarehouseRequired(HeaderView.Current) ? null : DefaultSiteID;

		protected override void _(Events.RowSelected<Header> e)
		{
			base._(e);
			ReviewAvailability.SetVisible(Base.IsMobile);
			ScanModeItemLookup.SetEnabled(e.Row?.Mode != Modes.Lookup);

			ScanQty.SetVisible(false);
			ScanConfirm.SetVisible(false);
			ScanRemove.SetVisible(false);
		}

		protected virtual void _(Events.RowSelected<InventorySummaryEnqFilter> e) => e.Cache.IsDirty = false;
		#endregion

		#region Views
		public override PXFilter<Header> HeaderView => Base.HeaderView;

		public SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<Header.inventoryID.FromCurrent>>.View.ReadOnly InventoryItem;
		public PXSetupOptional<INScanSetup, Where<INScanSetup.branchID.IsEqual<AccessInfo.branchID.FromCurrent>>> Setup;
		#endregion

		protected override WMSModeOf<InventoryItemLookup, InventoryItemLookupHost> DefaultMode => Modes.Lookup;
		public override string CurrentModeName => Msg.LookupMode;
		protected override string GetModePrompt()
		{
			if (HeaderView.Current.SiteID == null)
				return Localize(Msg.WarehousePrompt);
			return Localize(Msg.InventoryPrompt);
		}

		protected override bool ProcessCommand(string barcode) => false;

		protected override string GetDefaultState(Header header) => IsWarehouseRequired(header) ? ScanStates.Warehouse : ScanStates.Item;

		protected override bool ProcessByState(Header doc)
		{
			switch (doc.ScanState)
			{
				case ScanStates.Warehouse:
					ProcessWarehouse(doc.Barcode);
					return true;
				default:
					return base.ProcessByState(doc);
			}
		}

		protected override void ApplyState(string state)
		{
			switch (state)
			{
				case ScanStates.Warehouse:
					Prompt(Msg.WarehousePrompt);
					break;
				case ScanStates.Item:
					Prompt(Msg.InventoryPrompt);
					break;
			}
		}

		protected virtual void ProcessWarehouse(string barcode)
		{
			INSite site =
				SelectFrom<INSite>
				.Where<INSite.siteCD.IsEqual<@P.AsString>>
				.View.ReadOnly.Select(Base, barcode);

			if (site == null)
			{
				ReportError(Msg.WarehouseMissing, barcode);
			}
			else if (IsValid<Header.siteID>(site.SiteID, out string error) == false)
			{
				ReportError(error);
				return;
			}
			else
			{
				HeaderSetter.Set(h => h.SiteID, site.SiteID);

				Base.Filter.Cache.SetValueExt<InventorySummaryEnqFilter.siteID>(Base.Filter.Current, site.SiteID);
				Base.Filter.UpdateCurrent();

				SetScanState(ScanStates.Item, Msg.WarehouseReady, site.SiteCD);
			}
		}

		protected override void ProcessItemBarcode(string barcode)
		{
			var inventoryItem = (InventoryItem)ReadItemByBarcode(barcode);

			if (inventoryItem == null)
			{
				ReportError(Msg.InventoryMissing, barcode);
			}
			else if (IsValid<Header.inventoryID>(inventoryItem.InventoryID, out string error) == false)
			{
				ReportError(error);
			}
			else
			{
				HeaderSetter.Set(h => h.InventoryID, inventoryItem.InventoryID);
				HeaderSetter.Set(h => h.NoteID, inventoryItem.NoteID);

				Base.Filter.Cache.SetValueExt<InventorySummaryEnqFilter.inventoryID>(Base.Filter.Current, inventoryItem.InventoryID);
				Base.Filter.UpdateCurrent();

				if (HeaderView.Current.Mode == Modes.Lookup)
					SetScanState(ScanStates.Item, Msg.InventoryReady, inventoryItem.InventoryCD);
			}
		}

		protected override void ProcessDocumentNumber(string barcode) => throw new NotImplementedException();
		protected override void ProcessCartBarcode(string barcode) => throw new NotImplementedException();
		protected override void ProcessLotSerialBarcode(string barcode) => throw new NotImplementedException();
		protected override void ProcessExpireDate(string barcode) => throw new NotImplementedException();
		protected override void ProcessLocationBarcode(string barcode) => throw new NotImplementedException();

		protected override void ClearHeaderInfo(bool redirect = false)
		{
			base.ClearHeaderInfo(redirect);
			HeaderSetter.Set(h => h.SiteID, null);
		}

		protected override void ClearMode()
		{
			ClearHeaderInfo();
			SetScanState(HeaderView.Current.SiteID == null ? ScanStates.Warehouse : ScanStates.Item, Msg.ScreenCleared);
		}

		protected override bool UseQtyCorrectection => true;
		protected override bool ExplicitLineConfirmation => true;

		protected virtual bool IsWarehouseRequired(Header header) => (Setup.Current.DefaultWarehouse != true || DefaultSiteID == null);

		#region Constants & Messages
		public new abstract class Modes : WMSBase.Modes
		{
			public static WMSModeOf<InventoryItemLookup, InventoryItemLookupHost> Lookup { get; } = WMSMode("LOOK");

			public class lookup : PX.Data.BQL.BqlString.Constant<lookup> { public lookup() : base(Lookup) { } }
		}

		public new abstract class ScanStates : WMSBase.ScanStates
		{
			public const string Warehouse = "SITE";
		}

		[PXLocalizable]
		public new abstract class Msg : WMSBase.Msg
		{
			public const string LookupMode = "LOOKUP";
			public const string CommandInventory = "Ready to search by item barcode.";
		}
		#endregion
	}

	public class InventoryLookupUnassignedLocationFix : PXGraphExtension<InventoryItemLookupHost>
	{
		[PXOverride]
		public virtual void InventorySummaryEnquiryResult_LocationID_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e, Action<PXCache, PXFieldSelectingEventArgs> baseMethod)
		{
			string locationName = null;

			switch (e.ReturnValue)
			{
				case null:
					locationName = PXMessages.LocalizeNoPrefix(Messages.Unassigned);
					break;
				case InventorySummaryEnquiryResult.TotalLocationID:
					locationName = PXMessages.LocalizeNoPrefix(Messages.TotalLocation);
					break;
				default:
					locationName = INLocation.PK.Find(Base, e.ReturnValue as int?).With(loc => Base.IsMobile ? loc.Descr : loc.LocationCD);
					break;
			}

			if (locationName != null)
			{
				e.ReturnState = PXFieldState.CreateInstance(locationName, typeof(string), false, null, null, null, null, null,
					nameof(InventorySummaryEnquiryResult.locationID), null, GetLocationDisplayName(), null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
				e.Cancel = true;
			}

			string GetLocationDisplayName()
			{
				var displayName = PXUIFieldAttribute.GetDisplayName<InventorySummaryEnquiryResult.locationID>(cache);
				if (displayName != null) displayName = PXMessages.LocalizeNoPrefix(displayName);

				return displayName;
			}
		}

		[PXDBInt]
		[PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.Visible, FieldClass = LocationAttribute.DimensionName)]
		protected virtual void _(Events.CacheAttached<InventorySummaryEnquiryResult.locationID> e) { }
	}
}