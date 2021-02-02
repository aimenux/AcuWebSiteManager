using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Exceptions;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.IN.INScanTransfer, PX.Objects.IN.INScanTransferHost, PX.Objects.IN.INRegister, PX.Objects.IN.INScanTransfer.Header>;

namespace PX.Objects.IN
{
	public class INScanTransferHost : INTransferEntry
	{
		public override Type PrimaryItemType => typeof(INScanTransfer.Header);
		public PXFilter<INScanTransfer.Header> HeaderView;
	}

	public partial class INScanTransfer : WMSBase
	{
		public class UserSetup : PXUserSetupPerMode<UserSetup, INScanTransferHost, Header, INScanUserSetup, INScanUserSetup.userID, INScanUserSetup.mode, Modes.scanINTransfer> { }

		#region DACs
		public class Header : WMSHeader, ILSMaster
		{
			#region TransferNbr
			[PXUnboundDefault(typeof(INRegister.refNbr))]
			[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
			[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<INDocType.transfer>>>))]
			public override string RefNbr { get; set; }
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region SiteID
			[Site]
			public virtual int? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region ToSiteID
			[Site(DisplayName = "To Warehouse")]
			[PXUIVisible(typeof(toSiteID.IsNotNull.And<toSiteID.IsNotEqual<siteID>>))]
			public virtual int? ToSiteID { get; set; }
			public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
			#endregion
			#region LocationID
			[Location]
			public virtual int? LocationID { get; set; }
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			#endregion
			#region ToLocationID
			[Location]
			public virtual int? ToLocationID { get; set; }
			public abstract class toLocationID : PX.Data.BQL.BqlInt.Field<toLocationID> { }
			#endregion
			#region AmbiguousLocationCD
			[PXString]
			public virtual string AmbiguousLocationCD { get; set; }
			public abstract class ambiguousLocationCD : PX.Data.BQL.BqlString.Field<ambiguousLocationCD> { }
			#endregion
			#region PreviouseScanState
			[PXString(4, IsFixed = true)]
			public virtual string PreviouseScanState { get; set; }
			public abstract class previouseScanState : PX.Data.BQL.BqlString.Field<previouseScanState> { }
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
			#region LotSerTrack
			[PXString(1, IsFixed = true)]
			public virtual String LotSerTrack { get; set; }
			public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
			#endregion
			#region LotSerTrackExpiration
			[PXBool]
			public virtual Boolean? LotSerTrackExpiration { get; set; }
			public abstract class lotSerTrackExpiration : PX.Data.BQL.BqlBool.Field<lotSerTrackExpiration> { }
			#endregion
			#region LotSerAssign
			[PXString(1, IsFixed = true)]
			public virtual String LotSerAssign { get; set; }
			public abstract class lotSerAssign : PX.Data.BQL.BqlString.Field<lotSerAssign> { } 
			#endregion
			#region ExpirationDate
			[INExpireDate(typeof(inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual DateTime? ExpireDate { get; set; }
			public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
			#endregion
			#region ReasonCodeID
			[PXString]
			[PXSelector(typeof(SearchFor<ReasonCode.reasonCodeID>))]
			[PXRestrictor(typeof(Where<ReasonCode.usage.IsEqual<ReasonCodeUsages.transfer>>), Messages.ReasonCodeDoesNotMatch)]
			public virtual string ReasonCodeID { get; set; }
			public abstract class reasonCodeID : PX.Data.BQL.BqlString.Field<reasonCodeID> { }
			#endregion
			#region CartID
			[PXInt]
			[PXUIField(DisplayName = "Cart ID", Enabled = false)]
			[PXSelector(typeof(Search2<INCart.cartID, InnerJoin<INSite, On<INCart.FK.Site>>, Where<INCart.active, Equal<True>, And<Match<INSite, Current<AccessInfo.userName>>>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
			[PXUIVisible(typeof(Where2<FeatureInstalled<FeaturesSet.wMSCartTracking>, And<Current<INScanSetup.useCartsForTransfers>, Equal<True>>>))]
			public virtual int? CartID { get; set; }
			public abstract class cartID : BqlInt.Field<cartID> { }
			#endregion
			#region CartLoaded
			[PXBool, PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Cart Unloading", Enabled = false)]
			[PXUIVisible(typeof(cartLoaded))]
			public virtual bool? CartLoaded { get; set; }
			public abstract class cartLoaded : BqlBool.Field<cartLoaded> { }
			#endregion
			#region ILSMaster implementation
			public string TranType => string.Empty;
			public DateTime? TranDate => null;
			public short? InvtMult { get => -1; set { } }
			public int? ProjectID { get; set; }
			public int? TaskID { get; set; }
			#endregion

			public virtual bool HasLotSerial => LotSerTrack == INLotSerTrack.LotNumbered || LotSerTrack == INLotSerTrack.SerialNumbered;
		}
		#endregion

		#region Views
		public override PXFilter<Header> HeaderView => Base.HeaderView;
		public PXSetupOptional<INScanSetup, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>> Setup;
		#endregion

		#region Buttons
		public PXAction<Header> ScanRelease;
		[PXButton, PXUIField(DisplayName = "Release")]
		protected virtual IEnumerable scanRelease(PXAdapter adapter) => scanBarcode(adapter, ScanCommands.Release);

		public PXAction<Header> Review;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable review(PXAdapter adapter) => adapter.Get();
		#endregion

		#region Event Handlers
		protected override void _(Events.RowSelected<Header> e)
		{
			base._(e);

			if (Transfer == null && !string.IsNullOrEmpty(HeaderView.Current?.RefNbr))
				HeaderView.Cache.RaiseFieldUpdated<Header.refNbr>(HeaderView.Current, null);

			bool notReleasedAndHasLines = Transfer?.Released != true && ((INTran)Base.transactions.Select()) != null;
			ScanRemove.SetEnabled(notReleasedAndHasLines);
			ScanRelease.SetEnabled(notReleasedAndHasLines);
			ScanModeInTransfer.SetEnabled(e.Row != null && e.Row.Mode != Modes.ScanINTransfer);

			Review.SetVisible(Base.IsMobile);

			e.Cache.Adjust<PXUIFieldAttribute>().For<Header.toSiteID>(a => a.Visible = e.Row?.ToSiteID != null && e.Row.SiteID != e.Row.ToSiteID);

			Logs.AllowInsert = Logs.AllowDelete = Logs.AllowUpdate = false;
			Base.transactions.AllowInsert = false;
			Base.transactions.AllowDelete = Base.transactions.AllowUpdate = (Transfer == null || Transfer.Released != true);
		}

		protected virtual void _(Events.RowSelected<INTran> e)
		{
			bool isMobileAndNotReleased = Base.IsMobile && (Transfer == null || Transfer.Released != true);

			Base.transactions.Cache
			.Adjust<PXUIFieldAttribute>()
			.For<INTran.inventoryID>(ui => ui.Enabled = false)
			.SameFor<INTran.tranDesc>()
			.SameFor<INTran.qty>()
			.SameFor<INTran.uOM>()
			.For<INTran.lotSerialNbr>(ui => ui.Enabled = isMobileAndNotReleased)
			.SameFor<INTran.locationID>()
			.SameFor<INTran.toLocationID>()
			.SameFor<INTran.expireDate>()
			.SameFor<INTran.reasonCode>();
		}

		protected virtual void _(Events.FieldDefaulting<Header, Header.siteID> e) => e.NewValue = IsWarehouseRequired() ? null : DefaultSiteID;

		protected virtual void _(Events.RowPersisted<Header> e)
		{
			e.Row.RefNbr = Transfer?.RefNbr;
			e.Row.NoteID = Transfer?.NoteID;

			Base.transactions.Cache.Clear();
			Base.transactions.Cache.ClearQueryCacheObsolete();
		}

		protected virtual void _(Events.FieldUpdated<Header, Header.refNbr> e)
		{
			Base.transfer.Current = Base.transfer.Search<INRegister.refNbr>(e.Row.RefNbr, INDocType.Transfer);
		}

		protected virtual void _(Events.RowUpdated<INScanUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
		protected virtual void _(Events.RowInserted<INScanUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
		#endregion

		private INRegister Transfer => Base.CurrentDocument.Current;
		protected override BqlCommand DocumentSelectCommand()
			=> new SelectFrom<INRegister>.
				Where<INRegister.docType.IsEqual<INRegister.docType.AsOptional>
					.And<INRegister.refNbr.IsEqual<INRegister.refNbr.AsOptional>>>();

		protected virtual bool IsWarehouseRequired() => UserSetup.For(Base).DefaultWarehouse != true || DefaultSiteID == null;
		protected virtual bool IsLotSerialRequired() => HeaderView.Current.HasLotSerial;
		protected override bool IsCartRequired(Header header) => base.IsCartRequired(header) && Setup.Current.UseCartsForTransfers == true;
		protected virtual bool IsReasonCodeRequired(Header header) => Setup.Current.UseDefaultReasonCodeInTransfer != true;
		protected virtual string MainCycleStartState => PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item;
		protected override WMSModeOf<INScanTransfer, INScanTransferHost> DefaultMode => Modes.ScanINTransfer;
		public override string CurrentModeName =>
			HeaderView.Current.Mode == Modes.ScanINTransfer ? Msg.ScanINTransferMode :
			Msg.FreeMode;
		protected override string GetModePrompt()
		{
			if (HeaderView.Current.Mode == Modes.ScanINTransfer)
			{
				if (HeaderView.Current.SiteID == null)
					return Localize(Msg.WarehousePrompt);
				if (HeaderView.Current.LocationID == null)
					return Localize(Msg.FromLocationPrompt);

				if (PromptLocationForEveryLine)
				{
					if (HeaderView.Current.InventoryID == null)
						return Localize(Msg.InventoryPrompt);
					if (HeaderView.Current.LotSerialNbr == null && IsLotSerialRequired())
						return Localize(Msg.LotSerialPrompt);
					if (HeaderView.Current.ToLocationID == null)
						return Localize(Msg.ToLocationPrompt);
					if (HeaderView.Current.ReasonCodeID == null)
						return Localize(Msg.ReasonCodePrompt);
					return Localize(Msg.ConfirmationPrompt, HeaderView.Current.Qty, HeaderView.Cache.GetValueExt<Header.inventoryID>(HeaderView.Current));
				}
				else
				{
					if (HeaderView.Current.ToLocationID == null)
						return Localize(Msg.ToLocationPrompt);
					if (HeaderView.Current.ReasonCodeID == null)
						return Localize(Msg.ReasonCodePrompt);
					if (HeaderView.Current.InventoryID == null)
						return Localize(Msg.InventoryPrompt);
					if (HeaderView.Current.LotSerialNbr == null && IsLotSerialRequired())
						return Localize(Msg.LotSerialPrompt);
					return Localize(Msg.ConfirmationPrompt, HeaderView.Current.Qty, HeaderView.Cache.GetValueExt<Header.inventoryID>(HeaderView.Current));
				}
			}
			return null;
		}

		protected override bool ProcessCommand(string barcode)
		{
			switch (barcode)
			{
				case ScanCommands.Confirm:
					if (HeaderView.Current.Remove != true)
						ProcessConfirm();
					else
						ProcessConfirmRemove();
					return true;

				case ScanCommands.Remove:
					HeaderSetter.Set(x => x.Remove, true);
					SetScanState(ScanStates.Location, Msg.RemoveMode);
					return true;

				case ScanCommands.Release:
					ProcessRelease();
					return true;
			}
			return false;
		}

		protected override bool ProcessByState(Header doc)
		{
			if (Transfer?.Released == true)
			{
				ClearHeaderInfo();
				HeaderSetter.Set(x => x.RefNbr, null);
				Base.CurrentDocument.Insert();
			}

			switch (doc.ScanState)
			{
				case ScanStates.Warehouse:
					ProcessWarehouse(doc.Barcode);
					return true;
				case ScanStates.ChooseWarehouse:
					ProcessWarehouseChoiсe(doc.Barcode);
					return true;
				case ScanStates.ReasonCode:
					ProcessReasonCode(doc.Barcode);
					return true;
				case ScanStates.ToLocation:
					ProcessToLocationBarcode(doc.Barcode);
					return true;
				default:
					return base.ProcessByState(doc);
			}
		}

		protected virtual void ProcessWarehouse(string barcode)
		{
			INSite site =
				PXSelectReadonly<INSite,
				Where<INSite.siteCD, Equal<Required<Header.barcode>>>>
				.Select(Base, barcode);

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
				HeaderSetter.Set(x => x.SiteID, site.SiteID);
				Report(Msg.WarehouseReady, site.SiteCD);
				OnWarehouseProcessed();
			}
			}

		protected virtual void OnWarehouseProcessed()
		{
			SetScanState(ScanStates.Location);
		}

		protected virtual void ProcessWarehouseChoiсe(string barcode)
		{
			if (INSite.UK.Find(Base, barcode) == null)
			{
				ReportError(Msg.WarehouseMissing, barcode);
				return;
			}

			(INLocation location, INSite site) =
				SelectFrom<INLocation>.
				InnerJoin<INSite>.On<INLocation.FK.Site>.
				Where<
					Match<INSite, AccessInfo.userName.FromCurrent>.
					And<INSite.siteCD.IsEqual<@P.AsString>>.
					And<INLocation.locationCD.IsEqual<@P.AsString>>>.
				View.Select(Base, barcode, HeaderView.Current.AmbiguousLocationCD)
				.AsEnumerable().Cast<PXResult<INLocation, INSite>>().FirstOrDefault() ?? new PXResult<INLocation, INSite>(null, null);

			if (location == null || site == null)
			{
				ReportError(Msg.NoLocationSiteRelation, HeaderView.Current.AmbiguousLocationCD, barcode);
				return;
			}

			location = EnsureLocation(location, HeaderView.Current.AmbiguousLocationCD, site.SiteID, false);
			if (location == null)
				return;

			if (HeaderView.Current.PreviouseScanState == ScanStates.Location)
			{
				HeaderSetter.Set(x => x.SiteID, location.SiteID);
				HeaderSetter.Set(x => x.ToSiteID, location.SiteID);
				HeaderSetter.Set(x => x.LocationID, location.LocationID);
				SetScanState(PromptLocationForEveryLine ? ScanStates.Item : ScanStates.ToLocation, Msg.FromLocationReady, location.LocationCD);
			}
			else if (HeaderView.Current.PreviouseScanState == ScanStates.ToLocation)
			{
				if (site.BuildingID == null || site.BuildingID != INSite.PK.Find(Base, HeaderView.Current.SiteID).BuildingID)
				{
					ReportError(Msg.InterWarehouseTransferNotPossible);
					return;
				}

				HeaderSetter.Set(x => x.ToSiteID, location.SiteID);
				HeaderSetter.Set(x => x.ToLocationID, location.LocationID);
				SetScanState(PromptLocationForEveryLine ? ScanStates.ReasonCode : ScanStates.Item, Msg.ToLocationReady, location.LocationCD);
			}
			else
			{
				throw new InvalidOperationException();
			}

			HeaderSetter.Set(x => x.AmbiguousLocationCD, null);
			HeaderSetter.Set(x => x.PreviouseScanState, null);
		}

		protected override void ProcessLocationBarcode(string barcode)
		{
			INLocation location;

			if (Transfer == null)
			{
				var locations = ReadLocationsByBarcode(HeaderView.Current.SiteID, barcode);
				if (locations.Any() == false)
					return;

				if (locations.Count() > 1)
				{
					HeaderSetter.Set(x => x.AmbiguousLocationCD, barcode);
					HeaderSetter.Set(x => x.PreviouseScanState, HeaderView.Current.ScanState);
					SetScanState(ScanStates.ChooseWarehouse, Msg.AmbiguousLocation, barcode);
					return;
				}

				location = locations.First().Item1;
			}
			else
			{
				location = ReadLocationByBarcode(HeaderView.Current.SiteID, barcode);
			if (location == null)
				return;
			}

			if (location.SiteID != HeaderView.Current.SiteID || HeaderView.Current.ToSiteID == null)
			{
				if (Transfer == null)
				{
					HeaderSetter.Set(x => x.SiteID, location.SiteID);
					HeaderSetter.Set(x => x.ToSiteID, location.SiteID);
				}
				else if (HeaderView.Current.ToSiteID == null && location.SiteID == Transfer.ToSiteID)
				{
					HeaderSetter.Set(x => x.ToSiteID, Transfer.ToSiteID);
				}
				else
				{
					ReportError(Msg.FromLocationMismatch, location.LocationCD);
					return;
				}
			}

			HeaderSetter.Set(x => x.LocationID, location.LocationID);
			Report(Msg.FromLocationReady, location.LocationCD);

			OnLocationProcessed();
		}

		protected virtual void OnLocationProcessed()
		{
			SetScanState(PromptLocationForEveryLine ? ScanStates.Item : ScanStates.ToLocation);
		}

		protected override void ProcessItemBarcode(string barcode)
		{
			var item = ReadItemByBarcode(barcode);
			if (item == null)
			{
				if (HandleItemAbsence(barcode) == false)
				ReportError(Msg.InventoryMissing, barcode);
				return;
			}

			INItemXRef xref = item;
			InventoryItem inventoryItem = item;
			INLotSerClass lsclass = item;
			var uom = xref.UOM ?? inventoryItem.BaseUnit;

			if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered &&
				HeaderView.Current.LotSerAssign == INLotSerAssign.WhenReceived &&
				uom != inventoryItem.BaseUnit)
			{
				ReportError(Msg.SerialItemNotComplexQty);
				return;
			}

			HeaderSetter.Set(x => x.InventoryID, xref.InventoryID);
			HeaderSetter.Set(x => x.SubItemID, xref.SubItemID);
			if (HeaderView.Current.UOM == null)
				HeaderSetter.Set(x => x.UOM, uom);
			HeaderSetter.Set(x => x.LotSerTrack, lsclass.LotSerTrack);
			HeaderSetter.Set(x => x.LotSerTrackExpiration, lsclass.LotSerTrackExpiration);
			HeaderSetter.Set(x => x.LotSerAssign, lsclass.LotSerAssign);

			Report(Msg.InventoryReady, inventoryItem.InventoryCD);

			OnItemProcessed();
		}

		protected virtual void OnItemProcessed()
		{
			if (IsLotSerialRequired()
				&& HeaderView.Current.LotSerAssign != INLotSerAssign.WhenUsed)
			{
				SetScanState(ScanStates.LotSerial);
			}
			else
			{
				SetScanState(PromptLocationForEveryLine ? ScanStates.ToLocation : ScanStates.Confirm);
			}
		}

		protected virtual bool HandleItemAbsence(string barcode)
		{
			ProcessLocationBarcode(barcode);
			if (Info.Current.MessageType == WMSMessageTypes.Information)
				return true; // location found

			ProcessReasonCode(barcode);
			if (Info.Current.MessageType == WMSMessageTypes.Information)
				return true; // reason code found

			return false;
		}

		protected override void ProcessLotSerialBarcode(string barcode)
		{
			if (IsValid<Header.lotSerialNbr>(barcode, out string error) == false)
			{
				ReportError(error);
				return;
			}
			HeaderSetter.Set(x => x.LotSerialNbr, barcode);
			Report(Msg.LotSerialReady, barcode);
			OnLotSerialProcessed();
		}

		protected virtual void OnLotSerialProcessed()
		{
			SetScanState(PromptLocationForEveryLine ? ScanStates.ToLocation : ScanStates.Confirm);
		}

		protected override void ProcessExpireDate(string barcode) => throw new NotImplementedException();

		protected virtual void ProcessToLocationBarcode(string barcode)
		{
			INLocation location;

			if (Transfer == null)
			{
				int? buildingID = INSite.PK.Find(Base, HeaderView.Current.SiteID)?.BuildingID;
				var locations = ReadLocationsByBarcode(HeaderView.Current.ToSiteID, barcode);
				var sameBuildingLocations = locations.Where(r => r.Item2.With(s => s.SiteID == HeaderView.Current.ToSiteID || buildingID != null && s.BuildingID == buildingID)).ToArray();
				if (locations.Any() == false)
					return;

				if (sameBuildingLocations.Any() == false)
				{
					ReportError(Msg.InterWarehouseTransferNotPossible);
					return;
				}

				if (sameBuildingLocations.Count() > 1)
				{
					HeaderSetter.Set(x => x.AmbiguousLocationCD, barcode);
					HeaderSetter.Set(x => x.PreviouseScanState, HeaderView.Current.ScanState);
					SetScanState(ScanStates.ChooseWarehouse, Msg.AmbiguousLocation, barcode);
					return;
				}

				location = sameBuildingLocations.First().Item1;
			}
			else
			{
				location = ReadLocationByBarcode(HeaderView.Current.ToSiteID, barcode);
			if (location == null)
				return;
			}

			if (location.SiteID != HeaderView.Current.ToSiteID)
			{
				if (Transfer == null)
					HeaderSetter.Set(x => x.ToSiteID, location.SiteID);
				else
				{
					ReportError(Msg.ToLocationMismatch, location.LocationCD);
					return;
				}
			}

			HeaderSetter.Set(x => x.ToLocationID, location.LocationID);

			Report(Msg.ToLocationReady, location.LocationCD);

			OnToLocationProcessed();
		}

		protected virtual void OnToLocationProcessed()
		{
			SetScanState(
				PromptLocationForEveryLine || HeaderView.Current.ReasonCodeID == null
					? ScanStates.ReasonCode
					: ScanStates.Item);
		}

		protected override bool ProcessQtyBarcode(string barcode)
		{
			var result = base.ProcessQtyBarcode(barcode);

			if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered &&
				HeaderView.Current.LotSerAssign != INLotSerAssign.WhenUsed &&
				HeaderView.Current.Qty != 1)
			{
				HeaderSetter.Set(x => x.Qty, 1);
				ReportError(Msg.SerialItemNotComplexQty);
			}

			return result;
		}

		protected virtual void ProcessReasonCode(string barcode)
		{
			ReasonCode reasonCode = ReasonCode.PK.Find(Base, barcode);

			if (reasonCode == null)
			{
				ReportError(Msg.ReasonCodeMissing, barcode);
				return;
			}
			if (IsValid<Header.reasonCodeID>(reasonCode.ReasonCodeID, out string error) == false)
			{
				ReportError(error);
				return;
			}

			HeaderSetter.Set(x => x.ReasonCodeID, reasonCode.ReasonCodeID);
			Report(Msg.ReasonCodeReady, reasonCode.Descr ?? reasonCode.ReasonCodeID);
			OnReasonCodeProcessed();
			}

		protected virtual void OnReasonCodeProcessed()
		{
			SetScanState(PromptLocationForEveryLine 
				? ScanStates.Confirm 
				: ScanStates.Item);
		}

		protected virtual void ProcessConfirm()
		{
			if (!ValidateConfirmation())
			{
				if (ExplicitLineConfirmation == false)
					ClearHeaderInfo();
				return;
			}

			var header = HeaderView.Current;
			bool isSerialItem = header.LotSerTrack == INLotSerTrack.SerialNumbered;
			var isQtyOverridable = IsQtyOverridable(HeaderView.Current.LotSerTrack);

			if (Transfer == null)
				Base.CurrentDocument.Insert();

			Base.CurrentDocument.Cache.SetValueExt<INRegister.siteID>(Transfer, header.SiteID);
			Base.CurrentDocument.Cache.SetValueExt<INRegister.toSiteID>(Transfer, header.ToSiteID);
			Base.CurrentDocument.Update(Transfer);

			INTran existTransaction = FindTransferRow(header);

			Action rollbackAction = null;

			if (existTransaction != null)
			{
				decimal? newQty = existTransaction.Qty + header.Qty;

				if (isSerialItem && newQty != 1)
				{
					if (ExplicitLineConfirmation == false)
						ClearHeaderInfo();
					ReportError(Msg.SerialItemNotComplexQty);
					return;
				}

				var backup = Base.transactions.Cache.CreateCopy(existTransaction) as INTran;

				Base.transactions.Cache.SetValueExt<INTran.qty>(existTransaction, newQty);
				Base.transactions.Cache.SetValueExt<INTran.lotSerialNbr>(existTransaction, null);
				existTransaction = Base.transactions.Update(existTransaction);

				Base.transactions.Cache.SetValueExt<INTran.lotSerialNbr>(existTransaction, header.LotSerialNbr);
				existTransaction = Base.transactions.Update(existTransaction);

				rollbackAction = () =>
				{
					Base.transactions.Delete(existTransaction);
					Base.transactions.Insert(backup);
				};
			}
			else
			{
				existTransaction = Base.transactions.Insert();
				Base.transactions.Cache.SetValueExt<INTran.inventoryID>(existTransaction, header.InventoryID);
				Base.transactions.Cache.SetValueExt<INTran.siteID>(existTransaction, header.SiteID);
				Base.transactions.Cache.SetValueExt<INTran.toSiteID>(existTransaction, header.ToSiteID);
				Base.transactions.Cache.SetValueExt<INTran.locationID>(existTransaction, header.LocationID);
				Base.transactions.Cache.SetValueExt<INTran.toLocationID>(existTransaction, header.ToLocationID);
				Base.transactions.Cache.SetValueExt<INTran.uOM>(existTransaction, header.UOM);
				Base.transactions.Cache.SetValueExt<INTran.reasonCode>(existTransaction, header.ReasonCodeID);
				existTransaction = Base.transactions.Update(existTransaction);

				Base.transactions.Cache.SetValueExt<INTran.qty>(existTransaction, header.Qty);
				existTransaction = Base.transactions.Update(existTransaction);

				Base.transactions.Cache.SetValueExt<INTran.lotSerialNbr>(existTransaction, header.LotSerialNbr);
				existTransaction = Base.transactions.Update(existTransaction);

				rollbackAction = () => Base.transactions.Delete(existTransaction);
			}

			if (!VerifyConfirmedLine(header, existTransaction, rollbackAction))
				return;

			ClearHeaderInfo();
			Report(Msg.InventoryAdded, Base.transactions.Cache.GetValueExt<INTran.inventoryID>(existTransaction), header.Qty, header.UOM);

			OnConfirmed();

			if (isQtyOverridable)
				HeaderSetter.Set(x => x.IsQtyOverridable, true);
		}

		protected virtual void OnConfirmed()
		{
			SetScanState(MainCycleStartState);
		}

		protected virtual bool ValidateConfirmation()
		{
			var needLotSerialNbr = IsLotSerialRequired()
				&& HeaderView.Current.LotSerAssign.IsNotIn(null, INLotSerAssign.WhenUsed);

			if (needLotSerialNbr && HeaderView.Current.LotSerialNbr == null)
			{
				ReportError(Msg.LotSerialNotSet);
				return false;
			}
			if (IsLocationRequired(HeaderView.Current) && HeaderView.Current.ToLocationID == null)
			{
				ReportError(Msg.ToLocationNotSelected);
				return false;
			}
			if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered 
				&& HeaderView.Current.LotSerAssign.IsNotIn(null, INLotSerAssign.WhenUsed)
				&& HeaderView.Current.Qty != 1)
			{
				ReportError(Msg.SerialItemNotComplexQty);
				return false;
			}

			return true;
		}

		protected virtual bool VerifyConfirmedLine(Header header, INTran line, Action rollbackAction)
		{
			var args = new WMSLineVerifyingArguments<Header, INTran>(header, line)
			{
				Rollback = rollbackAction
			};
			VerifyConfirmedLine(args);
			if(args.Cancel)
			{
				if (!args.Processed)
				{
					rollbackAction();

					ClearHeaderInfo();

					SetScanState(MainCycleStartState);

					if(!string.IsNullOrEmpty(args.ErrorInfo?.MessageFormat))
						ReportError(args.ErrorInfo.MessageFormat, args.ErrorInfo.MessageArguments);

					args.Processed = true;
				}
				return false;
			}
			return true;
		}

		protected virtual void VerifyConfirmedLine(WMSLineVerifyingArguments<Header, INTran> args)
		{
			if (!EnsureSplitsLotSerial(args.Header.LotSerialNbr, args.Header, args.Rollback)
				|| !EnsureSplitsLocation(args.Header.LocationID, args.Header, args.Rollback))
			{
				args.Cancel = true;
				args.Processed = true;
				return;
			}
			VerifyAvailability(args);
		}

		protected virtual bool EnsureSplitsLotSerial(string userLotSerial, Header header, Action rollbackAction)
		{
			if (!string.IsNullOrEmpty(userLotSerial) &&
				Base.splits.SelectMain().Any(s => s.LotSerialNbr != userLotSerial))
			{
				rollbackAction();
				ClearHeaderInfo();
				SetScanState(MainCycleStartState);

				ReportError(Msg.QtyIssueExceedsQtyOnLot, userLotSerial,
					HeaderView.Cache.GetValueExt<Header.inventoryID>(header));

				return false;
			}
			return true;
		}

		protected virtual bool EnsureSplitsLocation(int? userLocationID, Header header, Action rollbackAction)
		{
			if (IsLocationRequired(header) &&
				Base.splits.SelectMain().Any(s => s.LocationID != userLocationID))
			{
				rollbackAction();
				ClearHeaderInfo();
				SetScanState(MainCycleStartState);

				ReportError(Msg.QtyIssueExceedsQtyOnLocation,
					HeaderView.Cache.GetValueExt<Header.locationID>(header),
					HeaderView.Cache.GetValueExt<Header.inventoryID>(header));

				return false;
			}
			return true;
		}

		protected virtual bool VerifyAvailability(WMSLineVerifyingArguments<Header, INTran> args)
		{
			PXExceptionInfo errorInfo = Base.lsselect.GetAvailabilityCheckErrors(Base.transactions.Cache, args.Line).FirstOrDefault();
			if(errorInfo != null)
			{
				args.ErrorInfo = errorInfo;
				args.ErrorInfo.MessageArguments = new object[]
				{
					Base.lsselect.Cache.GetStateExt<INTran.inventoryID>(args.Line),
					Base.lsselect.Cache.GetStateExt<INTran.subItemID>(args.Line),
					Base.lsselect.Cache.GetStateExt<INTran.siteID>(args.Line),
					Base.lsselect.Cache.GetStateExt<INTran.locationID>(args.Line),
					Base.lsselect.Cache.GetValue<INTran.lotSerialNbr>(args.Line)
				};
				args.Cancel = true;
			}
			return true;
		}

		protected virtual void ProcessConfirmRemove()
		{
			if (!ValidateConfirmation())
			{
				if (ExplicitLineConfirmation == false)
					ClearHeaderInfo();
				return;
			}

			var header = HeaderView.Current;
			var isQtyOverridable = IsQtyOverridable(HeaderView.Current.LotSerTrack);

			INTran existTransaction = FindTransferRow(header);

			if (existTransaction != null)
			{
				if (existTransaction.Qty == header.Qty)
				{
					Base.transactions.Delete(existTransaction);
				}
				else
				{
					var newQty = existTransaction.Qty - header.Qty;

					if (!IsValid<INTran.qty, INTran>(existTransaction, newQty, out string error))
					{
						if (ExplicitLineConfirmation == false)
							ClearHeaderInfo();
						ReportError(error);
						return;
					}

					Base.transactions.Cache.SetValueExt<INTran.qty>(existTransaction, newQty);
					Base.transactions.Update(existTransaction);
				}

				SetScanState(MainCycleStartState, Msg.InventoryRemoved, Base.transactions.Cache.GetValueExt<INTran.inventoryID>(existTransaction), header.Qty, header.UOM);
				ClearHeaderInfo();

				if (isQtyOverridable)
					HeaderSetter.Set(x => x.IsQtyOverridable, true);
			}
			else
			{
				ReportError(Msg.TransferLineMissing, InventoryItem.PK.Find(Base, header.InventoryID).InventoryCD);
				ClearHeaderInfo();
				SetScanState(ScanStates.Location);
			}
		}

		protected virtual void ProcessRelease()
		{
			if (Transfer != null)
			{
				if (Transfer.Released == true)
				{
					ReportError(Messages.Document_Status_Invalid);
					return;
				}

				if (Transfer.Hold != false) Base.CurrentDocument.Cache.SetValueExt<INRegister.hold>(Transfer, false);

				Save.Press();

				var clone = Base.Clone();

				WaitFor(
				(wArgs) =>
				{
					INDocumentRelease.ReleaseDoc(new List<INRegister>() { wArgs.Document }, false);
					PXLongOperation.SetCustomInfo(clone);
					throw new PXOperationCompletedException(Msg.DocumentIsReleased);
				}, null, new DocumentWaitArguments(Transfer), Msg.DocumentReleasing, Base.transfer.Current.RefNbr);
			}
		}

		protected override void OnWaitEnd(PXLongRunStatus status, INRegister primaryRow)
			=> OnWaitEnd(status, primaryRow?.Released == true, 
				Msg.DocumentIsReleased, null,
				Msg.DocumentReleaseFailed, ScanStates.Location);

		protected virtual INTran FindTransferRow(Header header)
		{
			var existTransactions = Base.transactions.SelectMain().Where(t =>
				t.InventoryID == header.InventoryID &&
				t.SiteID == header.SiteID &&
				t.ToSiteID == header.ToSiteID &&
				t.LocationID == (header.LocationID ?? t.LocationID) &&
				t.ToLocationID == (header.ToLocationID ?? t.ToLocationID) &&
				t.ReasonCode == (header.ReasonCodeID ?? t.ReasonCode) &&
				t.UOM == header.UOM);

			INTran existTransaction = null;

			if (IsLotSerialRequired())
			{
				foreach (var tran in existTransactions)
				{
					Base.transactions.Current = tran;
					if (Base.splits.SelectMain().Any(t => (t.LotSerialNbr ?? "") == (header.LotSerialNbr ?? "")))
					{
						existTransaction = tran;
						break;
					}
				}
			}
			else
			{
				existTransaction = existTransactions.FirstOrDefault();
			}

			return existTransaction;
		}

		protected override void ClearHeaderInfo(bool redirect = false)
		{
			base.ClearHeaderInfo(redirect);

			if (redirect)
			{
				HeaderSetter.Set(x => x.SiteID, null);
				HeaderSetter.Set(x => x.ToSiteID, null);
			}

			if (redirect || PromptLocationForEveryLine)
			{
				HeaderSetter.Set(x => x.LocationID, null);
				HeaderSetter.Set(x => x.ToLocationID, null);
				HeaderSetter.Set(x => x.ReasonCodeID, null);
			}

			HeaderSetter.Set(x => x.LotSerialNbr, null);
			HeaderSetter.Set(x => x.LotSerTrack, null);
			HeaderSetter.Set(x => x.LotSerTrackExpiration , null);
			HeaderSetter.Set(x => x.LotSerAssign, null);
			HeaderSetter.Set(x => x.AmbiguousLocationCD, null);
			HeaderSetter.Set(x => x.PreviouseScanState , null);
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
				case ScanStates.Location:
					if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
						Prompt(Msg.FromLocationPrompt);
					else
						SetScanState(ScanStates.Item);
					break;
				case ScanStates.ToLocation:
					if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
						Prompt(Msg.ToLocationPrompt);
					else
						SetScanState(ScanStates.ReasonCode);
					break;
				case ScanStates.ChooseWarehouse:
					Prompt(Msg.ChooseWarehouseForLocation, HeaderView.Current.AmbiguousLocationCD);
					break;
				case ScanStates.LotSerial:
					Prompt(Msg.LotSerialPrompt);
					break;
				case ScanStates.ReasonCode:
					if (IsReasonCodeRequired(HeaderView.Current))
						Prompt(Msg.ReasonCodePrompt);
					else
						SetScanState(PromptLocationForEveryLine ? ScanStates.Confirm : ScanStates.Item);
					break;
				case ScanStates.Confirm:
					if (IsMandatoryQtyInput)
					{
						Prompt(Msg.QtyPrompt);
						SetScanState(ScanStates.Qty);
					}
					else if (ExplicitLineConfirmation)
						Prompt(Msg.ConfirmationPrompt, HeaderView.Current.Qty, HeaderView.Cache.GetValueExt<Header.inventoryID>(HeaderView.Current));
					else
						ProcessCommand(ScanCommands.Confirm);
					break;
			}
		}

		protected override string GetDefaultState(Header header = null) => IsWarehouseRequired() ? ScanStates.Warehouse : ScanStates.Location;

		protected override void ClearMode()
		{
			ClearHeaderInfo();
			SetScanState(HeaderView.Current.SiteID == null ? ScanStates.Warehouse : ScanStates.Location, Msg.ScreenCleared);
		}

		protected override void ProcessDocumentNumber(string barcode) => throw new NotImplementedException();

		protected override void ProcessCartBarcode(string barcode) => throw new NotImplementedException();

		protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQtyInTransfer != true;
		protected override bool ExplicitLineConfirmation => Setup.Current.ExplicitLineConfirmation == true;
		protected override bool DocumentLoaded => Transfer != null;
		protected bool PromptLocationForEveryLine => Setup.Current.RequestLocationForEachItemInTransfer == true;

		protected override bool RequiredConfirmation(Header header)
		{
			return base.RequiredConfirmation(header)
				|| Transfer?.Released != true && header?.ScanState == ScanStates.Confirm;
		}

		#region Constants & Messages
		public new abstract class Modes : WMSBase.Modes
		{
			public static WMSModeOf<INScanTransfer, INScanTransferHost> ScanINTransfer { get; } = WMSMode("INTR");

			public class scanINTransfer : PX.Data.BQL.BqlString.Constant<scanINTransfer> { public scanINTransfer() : base(ScanINTransfer) { } }
		}

		public new abstract class ScanStates : WMSBase.ScanStates
		{
			public const string Warehouse = "SITE";
			public const string ChooseWarehouse = "WLOC";
			public const string Confirm = "CONF";
			public const string ToLocation = "TLOC";
			public const string ReasonCode = "RSNC";
		}

		public new abstract class ScanCommands : WMSBase.ScanCommands
		{
			public const string Release = Marker + "RELEASE*TRANSFER";
		}

		[PXLocalizable]
		public new abstract class Msg : WMSBase.Msg
		{
			public const string ScanINTransferMode = "Scan and Transfer";

			public const string ConfirmationPrompt = "Confirm transferring {0} {1}.";

			public const string TransferLineMissing = "Line {0} is not found in the transfer.";

			public const string AmbiguousLocation = "The {0} location is defined in multiple warehouses.";
			public const string ChooseWarehouseForLocation = "Scan the barcode of the warehouse to which the {0} location belongs.";
			public const string InterWarehouseTransferNotPossible = "You can perform one-step transfers only between warehouses located in the same building.";
			public const string NoLocationSiteRelation = "The {0} location does not belong to the {1} warehouse.";

			public const string FromLocationPrompt = "Scan the barcode of the origin location.";
			public const string FromLocationReady = "The {0} location is selected as origin.";
			public const string FromLocationMismatch = "The {0} location cannot be used because it does not belong to the origin warehouse.";

			public const string ToLocationPrompt = "Scan the barcode of the destination location.";
			public const string ToLocationReady = "The {0} location is selected as destination.";
			public const string ToLocationMismatch = "The {0} location cannot be used because it does not belong to the destination warehouse.";

			public const string DocumentReleasing = "The {0} transfer is being released.";
			public const string DocumentIsReleased = "The transfer is successfully released.";
			public const string DocumentReleaseFailed = "The transfer release failed.";
		}
		#endregion
	}
}