using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.IN.INScanReceive, PX.Objects.IN.INScanReceiveHost, PX.Objects.IN.INRegister, PX.Objects.IN.INScanReceive.Header>;

namespace PX.Objects.IN
{
	public class INScanReceiveHost : INReceiptEntry
	{
		public override Type PrimaryItemType => typeof(INScanReceive.Header);
		public PXFilter<INScanReceive.Header> HeaderView;
	}

	public class INScanReceive : WMSBase
	{
		public class UserSetup : PXUserSetupPerMode<UserSetup, INScanReceiveHost, Header, INScanUserSetup, INScanUserSetup.userID, INScanUserSetup.mode, Modes.scanReceipt> { }

		#region DACs
		public class Header : WMSHeader, ILSMaster
		{
			#region ReceiptNbr
			[PXUnboundDefault(typeof(INRegister.refNbr))]
			[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
			[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<INDocType.receipt>>>))]
			public override string RefNbr { get; set; }
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region TranDate
			[PXDate]
			[PXUnboundDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? TranDate { get; set; }
			public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
			#endregion
			#region SiteID
			[Site]
			public virtual int? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
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
			[PXRestrictor(typeof(Where<ReasonCode.usage.IsEqual<ReasonCodeUsages.receipt>>), Messages.ReasonCodeDoesNotMatch)]
			public virtual string ReasonCodeID { get; set; }
			public abstract class reasonCodeID : PX.Data.BQL.BqlString.Field<reasonCodeID> { }
			#endregion
			#region ILSMaster implementation
			public string TranType => string.Empty;
			public short? InvtMult { get => 1; set { } }
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

			bool notReleaseAndHasLines = Receipt?.Released != true && ((INTran)Base.transactions.Select()) != null;
			ScanRemove.SetEnabled(notReleaseAndHasLines);
			ScanRelease.SetEnabled(notReleaseAndHasLines);
			ScanModeInReceive.SetEnabled(e.Row != null && e.Row.Mode != Modes.ScanInReceive);

			Review.SetVisible(Base.IsMobile);

			Logs.AllowInsert = Logs.AllowDelete = Logs.AllowUpdate = false;
			Base.transactions.AllowInsert = false;
			Base.transactions.AllowDelete = Base.transactions.AllowUpdate = (Receipt == null || Receipt.Released != true);
		}

		protected virtual void _(Events.RowSelected<INTran> e)
		{
			bool isMobileAndNotReleased = Base.IsMobile && (Receipt == null || Receipt.Released != true);

			Base.transactions.Cache
			.Adjust<PXUIFieldAttribute>()
			.For<INTran.inventoryID>(ui => ui.Enabled = false)
			.SameFor<INTran.tranDesc>()
			.SameFor<INTran.locationID>()
			.SameFor<INTran.qty>()
			.SameFor<INTran.uOM>()
			.For<INTran.lotSerialNbr>(ui => ui.Enabled = isMobileAndNotReleased)
			.SameFor<INTran.expireDate>()
			.SameFor<INTran.reasonCode>();
		}

		protected virtual void _(Events.FieldDefaulting<Header, Header.siteID> e) => e.NewValue = IsWarehouseRequired() ? null : DefaultSiteID;

		protected virtual void _(Events.RowPersisted<Header> e)
		{
			e.Row.RefNbr = Receipt?.RefNbr;
			e.Row.TranDate = Receipt?.TranDate;
			e.Row.NoteID = Receipt?.NoteID;

			Base.transactions.Cache.Clear();
			Base.transactions.Cache.ClearQueryCacheObsolete();
		}

		protected virtual void _(Events.FieldUpdated<Header, Header.refNbr> e)
		{
			Base.receipt.Current = Base.receipt.Search<INRegister.refNbr>(e.Row.RefNbr, INDocType.Receipt);
		}

		protected virtual void _(Events.RowUpdated<INScanUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
		protected virtual void _(Events.RowInserted<INScanUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
		#endregion

		private INRegister Receipt => Base.CurrentDocument.Current;
		protected override BqlCommand DocumentSelectCommand()
			=> new SelectFrom<INRegister>.
				Where<INRegister.docType.IsEqual<INRegister.docType.AsOptional>
					.And<INRegister.refNbr.IsEqual<INRegister.refNbr.AsOptional>>>();

		protected virtual bool IsWarehouseRequired() => UserSetup.For(Base).DefaultWarehouse != true || DefaultSiteID == null;
		protected virtual bool IsLotSerialRequired() => HeaderView.Current.HasLotSerial;
		protected virtual bool IsExpirationDateRequired() => HeaderView.Current.LotSerTrackExpiration == true && EnsureExpireDateDefault() == null;
		protected virtual bool IsReasonCodeRequired(Header header) => Setup.Current.UseDefaultReasonCodeInReceipt != true;

		protected override WMSModeOf<INScanReceive, INScanReceiveHost> DefaultMode => Modes.ScanInReceive;
		public override string CurrentModeName =>
			HeaderView.Current.Mode == Modes.ScanInReceive ? Msg.ScanInReceiveMode :
			Msg.FreeMode;
		protected override string GetModePrompt()
		{
			if (HeaderView.Current.Mode == Modes.ScanInReceive)
			{
				if (HeaderView.Current.SiteID == null)
					return Localize(Msg.WarehousePrompt);

				if (PromptLocationForEveryLine)
				{
				if (HeaderView.Current.InventoryID == null)
					return Localize(Msg.InventoryPrompt);
				if (HeaderView.Current.LotSerialNbr == null && IsLotSerialRequired())
					return Localize(Msg.LotSerialPrompt);
				if (HeaderView.Current.LocationID == null)
					return Localize(Msg.LocationPrompt);
				}
				else
				{
					if (HeaderView.Current.LocationID == null)
						return Localize(Msg.LocationPrompt);
					if (HeaderView.Current.InventoryID == null)
						return Localize(Msg.InventoryPrompt);
					if (HeaderView.Current.LotSerialNbr == null && IsLotSerialRequired())
						return Localize(Msg.LotSerialPrompt);
				}

				return Localize(Msg.ConfirmationPrompt, HeaderView.Current.Qty, HeaderView.Cache.GetValueExt<Header.inventoryID>(HeaderView.Current));
			}
			return null;
		}

		protected override bool ProcessCommand(string barcode)
		{
			switch (barcode)
			{
				case ScanCommands.Confirm:
					if (HeaderView.Current.Remove != true) ProcessConfirm();
					else ProcessConfirmRemove();
					return true;

				case ScanCommands.Remove:
					HeaderSetter.Set(h => h.Remove, true);
					SetScanState(ScanStates.Item, Msg.RemoveMode);
					return true;

				case ScanCommands.Release:
					ProcessRelease();
					return true;
			}
			return false;
		}

		protected override bool ProcessByState(Header doc)
		{
			if (Receipt?.Released == true)
			{
				ClearHeaderInfo();
				HeaderSetter.Set(h => h.RefNbr, null);
				Base.CurrentDocument.Insert();
			}

			switch (doc.ScanState)
			{
				case ScanStates.Warehouse:
					ProcessWarehouse(doc.Barcode);
					return true;
				case ScanStates.ReasonCode:
					ProcessReasonCode(doc.Barcode);
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
				HeaderSetter.Set(h => h.SiteID, site.SiteID);
				SetScanState(PromptLocationForEveryLine ? ScanStates.Item : ScanStates.Location, Msg.WarehouseReady, site.SiteCD);
			}
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
			var uom = xref.UOM ?? inventoryItem.PurchaseUnit;

			if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered &&
				HeaderView.Current.LotSerAssign != INLotSerAssign.WhenUsed &&
				uom != inventoryItem.BaseUnit)
			{
				ReportError(Msg.SerialItemNotComplexQty);
				return;
			}

			HeaderSetter.Set(h => h.InventoryID, xref.InventoryID);
			HeaderSetter.Set(h => h.SubItemID, xref.SubItemID);
			if (HeaderView.Current.UOM == null)
				HeaderSetter.Set(h => h.UOM, uom);
			HeaderSetter.Set(h => h.LotSerTrack, lsclass.LotSerTrack);
			HeaderSetter.Set(h => h.LotSerTrackExpiration, lsclass.LotSerTrackExpiration);
			HeaderSetter.Set(h => h.LotSerAssign, lsclass.LotSerAssign);

			Report(Msg.InventoryReady, inventoryItem.InventoryCD);

			if (IsLotSerialRequired() && lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
				SetScanState(ScanStates.LotSerial);
			else
				SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Confirm);
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

			HeaderSetter.Set(h => h.LotSerialNbr, barcode);
			Report(Msg.LotSerialReady, barcode);

			if (IsExpirationDateRequired())
				SetScanState(ScanStates.ExpireDate);
			else
				SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Confirm);
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

			HeaderSetter.Set(h => h.ExpireDate, value);
			SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Confirm, Msg.LotSerialExpireDateReady, barcode);
		}

		protected override void ProcessLocationBarcode(string barcode)
		{
			INLocation location = ReadLocationByBarcode(HeaderView.Current.SiteID, barcode);
			if (location == null)
				return;

			HeaderSetter.Set(h => h.LocationID, location.LocationID);
			SetScanState(
				PromptLocationForEveryLine
					? ScanStates.ReasonCode
					: HeaderView.Current.ReasonCodeID == null
						? ScanStates.ReasonCode
						: ScanStates.Item,
				Msg.LocationReady, location.LocationCD);
		}

		protected override bool ProcessQtyBarcode(string barcode)
		{
			var result = base.ProcessQtyBarcode(barcode);

			if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered &&
				HeaderView.Current.LotSerAssign != INLotSerAssign.WhenUsed &&
				HeaderView.Current.Qty != 1)
			{
				HeaderSetter.Set(h => h.Qty, 1);
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
			}
			else if (IsValid<Header.reasonCodeID>(reasonCode.ReasonCodeID, out string error) == false)
			{
				ReportError(error);
				return;
			}
			else
			{
				HeaderSetter.Set(h => h.ReasonCodeID, reasonCode.ReasonCodeID);
				SetScanState(PromptLocationForEveryLine ? ScanStates.Confirm : ScanStates.Item, Msg.ReasonCodeReady, reasonCode.Descr ?? reasonCode.ReasonCodeID);
			}
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
			var isQtyOverridable = IsQtyOverridable(HeaderView.Current.LotSerTrack);

			if (Receipt == null) Base.CurrentDocument.Insert();

			INTran existTransaction = FindReceiptRow(header);

			if (existTransaction != null)
			{
				var newQty = existTransaction.Qty + header.Qty;

				if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered &&
					HeaderView.Current.LotSerAssign == INLotSerAssign.WhenReceived &&
					newQty != 1)
				{
					if (ExplicitLineConfirmation == false)
						ClearHeaderInfo();
					ReportError(Msg.SerialItemNotComplexQty);
					return;
				}

				Base.transactions.Cache.SetValueExt<INTran.qty>(existTransaction, newQty);
				existTransaction = Base.transactions.Update(existTransaction);
			}
			else
			{
				INTran tran = Base.transactions.Insert();
				Base.transactions.Cache.SetValueExt<INTran.inventoryID>(tran, header.InventoryID);
				tran = existTransaction = Base.transactions.Update(tran);

				Base.transactions.Cache.SetValueExt<INTran.siteID>(tran, header.SiteID);
				Base.transactions.Cache.SetValueExt<INTran.locationID>(tran, header.LocationID);
				Base.transactions.Cache.SetValueExt<INTran.uOM>(tran, header.UOM);
				Base.transactions.Cache.SetValueExt<INTran.qty>(tran, header.Qty);
				Base.transactions.Cache.SetValueExt<INTran.expireDate>(tran, header.ExpireDate);
				Base.transactions.Cache.SetValueExt<INTran.lotSerialNbr>(tran, header.LotSerialNbr);
				Base.transactions.Cache.SetValueExt<INTran.reasonCode>(tran, header.ReasonCodeID);
				existTransaction = Base.transactions.Update(tran);
			}

			if (!string.IsNullOrEmpty(header.LotSerialNbr))
			{
				foreach (INTranSplit split in Base.splits.Select())
				{
					Base.splits.Cache.SetValueExt<INTranSplit.expireDate>(split, header.ExpireDate ?? existTransaction.ExpireDate);
					Base.splits.Cache.SetValueExt<INTranSplit.lotSerialNbr>(split, header.LotSerialNbr);
					Base.splits.Update(split);
				}
			}

			ClearHeaderInfo();
			SetScanState(ScanStates.Item, Msg.InventoryAdded, Base.transactions.Cache.GetValueExt<INTran.inventoryID>(existTransaction), header.Qty, header.UOM);

			if (isQtyOverridable)
				HeaderSetter.Set(x => x.IsQtyOverridable, true);
		}

		protected virtual bool ValidateConfirmation()
		{
			var needLotSerialNbr = IsLotSerialRequired()
				&& HeaderView.Current.LotSerAssign == INLotSerAssign.WhenReceived;

			if (needLotSerialNbr && HeaderView.Current.LotSerialNbr == null)
			{
				ReportError(Msg.LotSerialNotSet);
				return false;
			}
			if (needLotSerialNbr 
				&& HeaderView.Current.ExpireDate == null
				&& IsExpirationDateRequired())
			{
				ReportError(Msg.LotSerialExpireDateNotSet);
				return false;
			}
			if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered &&
				HeaderView.Current.LotSerAssign != INLotSerAssign.WhenUsed &&
				HeaderView.Current.Qty != 1)
			{
				ReportError(Msg.SerialItemNotComplexQty);
				return false;
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

			INTran existTransaction = FindReceiptRow(header);

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

				SetScanState(ScanStates.Item, Msg.InventoryRemoved, Base.transactions.Cache.GetValueExt<INTran.inventoryID>(existTransaction), header.Qty, header.UOM);
				ClearHeaderInfo();

				if (isQtyOverridable)
					HeaderSetter.Set(x => x.IsQtyOverridable, true);
			}
			else
			{
				ReportError(Msg.ReceiptLineMissing, InventoryItem.PK.Find(Base, header.InventoryID).InventoryCD);
				ClearHeaderInfo();
				SetScanState(ScanStates.Item);
			}
		}

		protected virtual void ProcessRelease()
		{
			if (Receipt != null)
			{
				if (Receipt.Released == true)
				{
					ReportError(Messages.Document_Status_Invalid);
					return;
				}

				if (Receipt.Hold != false) Base.CurrentDocument.Cache.SetValueExt<INRegister.hold>(Receipt, false);

				Save.Press();

				bool printInventory = UserSetup.For(Base).PrintInventoryLabelsAutomatically == true;
				string printLabelsReportID = UserSetup.For(Base).InventoryLabelsReportID;

				var clone = Base.Clone();

				WaitFor(
				(wArgs) =>
				{
					INDocumentRelease.ReleaseDoc(new List<INRegister>() { wArgs.Document }, false);
					PXLongOperation.SetCustomInfo(clone);
					if (PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && printInventory && wArgs.Document.RefNbr != null && !string.IsNullOrEmpty(printLabelsReportID))
					{
						var reportParameters = new Dictionary<string, string>()
						{
							[nameof(INRegister.RefNbr)] = wArgs.Document.RefNbr
						};

						PrintReportViaDeviceHub<BAccount>(Base, printLabelsReportID, reportParameters, INNotificationSource.None, null);
					}
					throw new PXOperationCompletedException(Msg.DocumentIsReleased);
				}, null, new DocumentWaitArguments(Receipt), Msg.DocumentReleasing, Base.receipt.Current.RefNbr);
			}
		}

		protected override void OnWaitEnd(PXLongRunStatus status, INRegister primaryRow)
			=> OnWaitEnd(status, primaryRow?.Released == true, Msg.DocumentIsReleased, Msg.DocumentReleaseFailed);

		protected virtual INTran FindReceiptRow(Header header)
		{
			var existTransactions = Base.transactions.SelectMain().Where(t =>
				t.InventoryID == header.InventoryID &&
				t.SiteID == header.SiteID &&
				t.LocationID == (header.LocationID ?? t.LocationID) &&
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
				HeaderSetter.Set(h => h.SiteID, null);
			}

			if (redirect || PromptLocationForEveryLine)
			{
				HeaderSetter.Set(h => h.LocationID, null);
				HeaderSetter.Set(h => h.ReasonCodeID, null);
			}

			HeaderSetter.Set(h => h.LotSerialNbr, null);
			HeaderSetter.Set(h => h.LotSerTrack, null);
			HeaderSetter.Set(h => h.LotSerTrackExpiration, null);
			HeaderSetter.Set(h => h.LotSerAssign, null);
			HeaderSetter.Set(h => h.ExpireDate, null);
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
						Prompt(Msg.LocationPrompt);
					else
						SetScanState(ScanStates.ReasonCode);
					break;
				case ScanStates.LotSerial:
					Prompt(Msg.LotSerialPrompt);
					break;
				case ScanStates.ExpireDate:
					Prompt(Msg.LotSerialExpireDatePrompt);
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
					else if (HeaderView.Current.Remove == false)
						ProcessConfirm();
					else
						ProcessConfirmRemove();
					break;
			}
		}

		protected override string GetDefaultState(Header header = null) => IsWarehouseRequired() ? ScanStates.Warehouse : PromptLocationForEveryLine ? ScanStates.Item : ScanStates.Location;

		protected override void ClearMode()
		{
			ClearHeaderInfo();
			SetScanState(HeaderView.Current.SiteID == null ? ScanStates.Warehouse : PromptLocationForEveryLine ? ScanStates.Item : ScanStates.Location, Msg.ScreenCleared);
		}

		protected override void ProcessDocumentNumber(string barcode) => throw new NotImplementedException();
		protected override void ProcessCartBarcode(string barcode) => throw new NotImplementedException();

		private DateTime? EnsureExpireDateDefault() => LSSelect.ExpireDateByLot(Base, HeaderView.Current, null);

		protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQtyInReceipt != true;
		protected override bool ExplicitLineConfirmation => Setup.Current.ExplicitLineConfirmation == true;
		protected override bool DocumentLoaded => Receipt != null;
		protected bool PromptLocationForEveryLine => Setup.Current.RequestLocationForEachItemInReceipt == true;

		protected override bool RequiredConfirmation(Header header)
		{
			return base.RequiredConfirmation(header)
				|| Receipt?.Released != true && header?.ScanState == ScanStates.Confirm;
		}

		#region Constants & Messages
		public new abstract class Modes : WMSBase.Modes
		{
			public static WMSModeOf<INScanReceive, INScanReceiveHost> ScanInReceive { get; } = WMSMode("INRE");

			public class scanReceipt : PX.Data.BQL.BqlString.Constant<scanReceipt> { public scanReceipt() : base(ScanInReceive) { } }
		}

		public new abstract class ScanStates : WMSBase.ScanStates
		{
			public const string Warehouse = "SITE";
			public const string Confirm = "CONF";
			public const string ReasonCode = "RSNC";
		}

		public new abstract class ScanCommands : WMSBase.ScanCommands
		{
			public const string Release = Marker + "RELEASE*RECEIPT";
		}

		[PXLocalizable]
		public new abstract class Msg : WMSBase.Msg
		{
			public const string ScanInReceiveMode = "Scan and Receive";

			public const string ConfirmationPrompt = "Confirm receiving {0} {1}.";

			public const string DocumentReleasing = "The {0} receipt is being released.";
			public const string DocumentIsReleased = "The receipt is successfully released.";
			public const string DocumentReleaseFailed = "The receipt release failed.";

			public const string ReceiptLineMissing = "Line {0} is not found in the receipt.";
		}
		#endregion
	}
}