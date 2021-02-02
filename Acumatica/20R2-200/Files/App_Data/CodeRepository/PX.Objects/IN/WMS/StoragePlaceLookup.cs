using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.IN.StoragePlaceLookup, PX.Objects.IN.StoragePlaceLookupHost, PX.Objects.IN.StoragePlace, PX.Objects.IN.StoragePlaceLookup.Header>;

namespace PX.Objects.IN
{
	public class StoragePlaceLookupHost : StoragePlaceEnq
	{
		public override Type PrimaryItemType => typeof(StoragePlaceLookup.Header);
		public PXFilter<StoragePlaceLookup.Header> HeaderView;
	}

	public class StoragePlaceLookup : WMSBase
	{
		#region DACs
		public class Header : WMSHeader
		{
			#region SiteID
			[Site(Enabled = false)]
			public virtual int? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region StorageID
			[PXInt]
			[PXUIField(DisplayName = "Storage ID", Enabled = false)]
			[PXSelector(
				typeof(Search<StoragePlace.storageID, Where<StoragePlace.active, Equal<True>, And<StoragePlace.siteID, Equal<Current<siteID>>>>>),
				typeof(StoragePlace.siteID), typeof(StoragePlace.storageCD), typeof(StoragePlace.isCart), typeof(StoragePlace.active),
				SubstituteKey = typeof(StoragePlace.storageCD),
				DescriptionField = typeof(StoragePlace.descr),
				ValidateValue = false)]
			public int? StorageID { get; set; }
			public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
			#endregion
			#region IsCart
			[PXBool]
			[PXUIField(DisplayName = "Cart", IsReadOnly = true, FieldClass = "Carts")]
			public virtual Boolean? IsCart { get; set; }
			public abstract class isCart : PX.Data.BQL.BqlBool.Field<isCart> { }
			#endregion
		}
		#endregion

		#region Buttons
		public PXAction<Header> ReviewAvailability;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable reviewAvailability(PXAdapter adapter) => adapter.Get();
		#endregion

		#region Event Handlers
		protected virtual void _(Events.FieldDefaulting<WMSInfo, WMSInfo.message> e) => e.NewValue = Msg.CommandStorage;
		protected virtual void _(Events.FieldDefaulting<Header, Header.siteID> e) => e.NewValue = DefaultSiteID;
		//protected virtual void _(Events.FieldDefaulting<StoragePlaceEnq.StoragePlaceFilter, StoragePlaceEnq.StoragePlaceFilter.expandByLotSerialNbr> e) => e.NewValue = true;

		protected virtual void _(Events.RowUpdated<StoragePlaceEnq.StoragePlaceFilter> e) => e.Cache.IsDirty = false;
		protected virtual void _(Events.RowInserted<StoragePlaceEnq.StoragePlaceFilter> e) => e.Cache.IsDirty = false;
		protected override void _(Events.RowSelected<Header> e)
		{
			base._(e);
			ReviewAvailability.SetVisible(Base.IsMobile);
			ScanModeItemLookup.SetEnabled(e.Row?.Mode != Modes.Lookup);
			ScanQty.SetVisible(false);
			ScanConfirm.SetVisible(false);
			ScanRemove.SetVisible(false);
			//Base.storages.Cache.Adjust<PXUIFieldAttribute>().For<StoragePlaceStatus.splittedIcon>(a => a.Visible = e.Row.IsCart == false);
		}
		#endregion

		#region Views
		public override PXFilter<Header> HeaderView => Base.HeaderView;
		public PXSetupOptional<INScanSetup, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>> Setup;
		#endregion

		protected override WMSModeOf<StoragePlaceLookup, StoragePlaceLookupHost> DefaultMode => Modes.Lookup;
		public override string CurrentModeName => Msg.LookupMode;
		protected override string GetModePrompt()
		{
			if (HeaderView.Current.SiteID == null)
				return Localize(Msg.WarehousePrompt);
			return Localize(StoragePrompt);
		}

		private string StoragePrompt => IsCartRequired(HeaderView.Current) ? Msg.StoragePromptWithCart : Msg.StoragePrompt;

		protected override bool ProcessCommand(string barcode) => false;

		protected override string GetDefaultState(Header header) => IsWarehouseRequired(header) ? ScanStates.Warehouse : ScanStates.Location;

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
				case ScanStates.Location:
					Prompt(StoragePrompt);
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
				SetScanState(ScanStates.Location, Msg.WarehouseReady, site.SiteCD);
			}
		}

		protected override void ProcessLocationBarcode(string barcode)
		{
			StoragePlace storage =
				SelectFrom<StoragePlace>
				.Where<StoragePlace.siteID.IsEqual<@P.AsInt>
					.And<StoragePlace.storageCD.IsEqual<@P.AsString>>>
				.View.ReadOnly.Select(Base, HeaderView.Current.SiteID, barcode);

			if (storage == null)
			{
				ReportError("Storage {0} is not found.", barcode);
			}
			else if (IsValid<Header.storageID>(storage.StorageID, out string error) == false)
			{
				ReportError(error);
			}
			else
			{
				HeaderSetter.Set(h => h.StorageID, storage.StorageID);
				HeaderSetter.Set(h => h.IsCart, storage.IsCart);

				Base.Filter.Insert();
				Base.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.siteID>(Base.Filter.Current, storage.SiteID);
				Base.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.storageID>(Base.Filter.Current, storage.StorageID);
				Base.Filter.Cache.IsDirty = false;
				Base.Filter.UpdateCurrent();

				if (HeaderView.Current.Mode == Modes.Lookup)
					SetScanState(ScanStates.Location, Msg.StorageReady, storage.StorageCD);
			}
		}

		protected override void ProcessDocumentNumber(string barcode) => throw new NotImplementedException();
		protected override void ProcessItemBarcode(string barcode) => throw new NotImplementedException();
		protected override void ProcessLotSerialBarcode(string barcode) => throw new NotImplementedException();
		protected override void ProcessExpireDate(string barcode) => throw new NotImplementedException();
		protected override void ProcessCartBarcode(string barcode) => throw new NotImplementedException();

		protected override void ClearHeaderInfo(bool redirect = false)
		{
			base.ClearHeaderInfo(redirect);

			HeaderSetter.Set(h => h.SiteID, null);
			HeaderSetter.Set(h => h.StorageID, null);
		}

		protected override void ClearMode()
		{
			ClearHeaderInfo();
			SetScanState(HeaderView.Current.SiteID == null ? ScanStates.Warehouse : ScanStates.Location, Msg.ScreenCleared);
		}

		protected override bool UseQtyCorrectection => true;
		protected override bool ExplicitLineConfirmation => true;

		protected virtual bool IsWarehouseRequired(Header header) => (Setup.Current.DefaultWarehouse != true || DefaultSiteID == null) && header.SiteID == null;

		#region Constants & Messages
		public new abstract class Modes : WMSBase.Modes
		{
			public static WMSModeOf<StoragePlaceLookup, StoragePlaceLookupHost> Lookup { get; } = WMSMode("LOOK");

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
			public const string CommandStorage = "Ready to search by storage barcode.";
			public const string StoragePrompt = "Scan the barcode of the location.";
			public const string StoragePromptWithCart = "Scan the barcode of the location or of the cart.";
			public const string StorageReady = "The {0} storage is selected.";
		}
		#endregion
	}
}