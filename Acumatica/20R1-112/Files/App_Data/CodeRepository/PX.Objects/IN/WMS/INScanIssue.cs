using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.IN.INScanIssue, PX.Objects.IN.INScanIssueHost, PX.Objects.IN.INRegister, PX.Objects.IN.INScanIssue.Header>;

namespace PX.Objects.IN
{
	public class INScanIssueHost : INIssueEntry
	{
		public override Type PrimaryItemType => typeof(INScanIssue.Header);
		public PXFilter<INScanIssue.Header> HeaderView;
	}

	public class INScanIssue : WMSBase
	{
		public class UserSetup : PXUserSetupPerMode<UserSetup, INScanIssueHost, Header, INScanUserSetup, INScanUserSetup.userID, INScanUserSetup.mode, Modes.scanIssue> { }

		#region DACs
		public class Header : WMSHeader, ILSMaster
		{
			#region IssueNbr
			[PXUnboundDefault(typeof(INRegister.refNbr))]
			[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
			[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<INDocType.issue>>>))]
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
			#region ExpirationDate
			[INExpireDate(typeof(inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual DateTime? ExpireDate { get; set; }
			public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
			#endregion
			#region ReasonCodeID
			[PXString]
			[PXSelector(typeof(SearchFor<ReasonCode.reasonCodeID>))]
			[PXRestrictor(typeof(Where<ReasonCode.usage.IsEqual<ReasonCodeUsages.issue>>), Messages.ReasonCodeDoesNotMatch)]
			public virtual string ReasonCodeID { get; set; }
			public abstract class reasonCodeID : PX.Data.BQL.BqlString.Field<reasonCodeID> { }
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
			#region AutoNextNbr
			[PXBool]
			public virtual Boolean? AutoNextNbr { get; set; }
			public abstract class autoNextNbr : PX.Data.BQL.BqlBool.Field<autoNextNbr> { }
			#endregion

			#region ILSMaster implementation
			public string TranType => string.Empty;
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

			bool notReleaseAndHasLines = Issue?.Released != true && ((INTran)Base.transactions.Select()) != null;
			ScanRemove.SetEnabled(notReleaseAndHasLines);
			ScanRelease.SetEnabled(notReleaseAndHasLines);
			ScanModeIssue.SetEnabled(e.Row != null && e.Row.Mode != Modes.ScanIssue);

			Review.SetVisible(Base.IsMobile);

			Logs.AllowInsert = Logs.AllowDelete = Logs.AllowUpdate = false;
			Base.transactions.AllowInsert = false;
			Base.transactions.AllowDelete = Base.transactions.AllowUpdate = (Issue == null || Issue.Released != true);
		}

		protected virtual void _(Events.RowSelected<INTran> e)
		{
			bool isMobileAndNotReleased = Base.IsMobile && (Issue == null || Issue.Released != true);

			Base.transactions.Cache
			.Adjust<PXUIFieldAttribute>()
			.For<INTran.inventoryID>(ui => ui.Enabled = false)
			.SameFor<INTran.tranDesc>()
			.SameFor<INTran.qty>()
			.SameFor<INTran.uOM>()
			.For<INTran.lotSerialNbr>(ui => ui.Enabled = isMobileAndNotReleased)
			.SameFor<INTran.expireDate>()
			.SameFor<INTran.locationID>()
			.SameFor<INTran.reasonCode>();
		}

		protected virtual void _(Events.FieldDefaulting<Header, Header.siteID> e) => e.NewValue = IsWarehouseRequired() ? null : DefaultSiteID;

		protected virtual void _(Events.RowPersisted<Header> e)
		{
			e.Row.RefNbr = Issue?.RefNbr;
			e.Row.TranDate = Issue?.TranDate;
			e.Row.NoteID = Issue?.NoteID;

			Base.transactions.Cache.Clear();
			Base.transactions.Cache.ClearQueryCacheObsolete();
		}

		protected virtual void _(Events.FieldUpdated<Header, Header.refNbr> e)
		{
			Base.issue.Current = Base.issue.Search<INRegister.refNbr>(e.Row.RefNbr, INDocType.Issue);
		}

		protected virtual void _(Events.RowUpdated<INScanUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
		protected virtual void _(Events.RowInserted<INScanUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
		#endregion

		private INRegister Issue => Base.CurrentDocument.Current;
		protected override BqlCommand DocumentSelectCommand() 
			=> new SelectFrom<INRegister>. 
				Where<INRegister.docType.IsEqual<INRegister.docType.AsOptional>
					.And<INRegister.refNbr.IsEqual<INRegister.refNbr.AsOptional>>>();

		protected virtual bool IsWarehouseRequired() => UserSetup.For(Base).DefaultWarehouse != true || DefaultSiteID == null;
		protected virtual bool IsLotSerialRequired() => HeaderView.Current.HasLotSerial;
		protected virtual bool IsExpirationDateRequired() => HeaderView.Current.LotSerTrackExpiration == true && EnsureExpireDateDefault() == null;
		protected virtual bool IsReasonCodeRequired(Header header) => Setup.Current.UseDefaultReasonCodeInIssue != true;

		protected override WMSModeOf<INScanIssue, INScanIssueHost> DefaultMode => Modes.ScanIssue;
		public override string CurrentModeName =>
			HeaderView.Current.Mode == Modes.ScanIssue ? Msg.ScanIssueMode :
			Msg.FreeMode;
		protected override string GetModePrompt()
		{
			if (HeaderView.Current.Mode == Modes.ScanIssue)
			{
				if (HeaderView.Current.SiteID == null)
					return Localize(Msg.WarehousePrompt);
				if (HeaderView.Current.LocationID == null)
					return Localize(Msg.LocationPrompt);
				if (HeaderView.Current.InventoryID == null)
					return Localize(Msg.InventoryPrompt);
				if (HeaderView.Current.LotSerialNbr == null && IsLotSerialRequired())
					return Localize(Msg.LotSerialPrompt);
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
			if (Issue?.Released == true)
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
				SetScanState(ScanStates.Location, Msg.WarehouseReady, site.SiteCD);
			}
		}

		protected override void ProcessLocationBarcode(string barcode)
		{
			INLocation location = ReadLocationByBarcode(HeaderView.Current.SiteID, barcode);
			if (location == null)
				return;

			HeaderSetter.Set(h => h.LocationID, location.LocationID);
			SetScanState(
				PromptLocationForEveryLine
					? ScanStates.Item
					: HeaderView.Current.ReasonCodeID == null
						? ScanStates.ReasonCode
						: ScanStates.Item,
				Msg.LocationReady, location.LocationCD);
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
			var uom = xref.UOM ?? inventoryItem.SalesUnit;

			if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered &&
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
			HeaderSetter.Set(h => h.LotSerAssign, lsclass.LotSerAssign);
			HeaderSetter.Set(h => h.LotSerTrackExpiration, lsclass.LotSerTrackExpiration);
			HeaderSetter.Set(h => h.AutoNextNbr, lsclass.AutoNextNbr);

			Report(Msg.InventoryReady, inventoryItem.InventoryCD);

			if (IsLotSerialRequired())
				SetScanState(ScanStates.LotSerial);
			else
				SetScanState(PromptLocationForEveryLine ? ScanStates.ReasonCode : ScanStates.Confirm);
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

			if (HeaderView.Current.LotSerAssign == INLotSerAssign.WhenUsed && IsExpirationDateRequired())
				SetScanState(ScanStates.ExpireDate);
			else
				SetScanState(PromptLocationForEveryLine ? ScanStates.ReasonCode : ScanStates.Confirm);
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
			SetScanState(PromptLocationForEveryLine ? ScanStates.ReasonCode : ScanStates.Confirm, Msg.LotSerialExpireDateReady, barcode);
		}

		protected override bool ProcessQtyBarcode(string barcode)
		{
			var result = base.ProcessQtyBarcode(barcode);

			if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered &&
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
			var userLotSerial = header.LotSerialNbr;
			var isQtyOverridable = IsQtyOverridable(HeaderView.Current.LotSerTrack);

			if (Issue == null) Base.CurrentDocument.Insert();

			INTran existTransaction = FindIssueRow(header);

			Action rollbackAction = null;

			if (existTransaction != null)
			{
				decimal? newQty = existTransaction.Qty + header.Qty;

				if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered && newQty != 1)
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

				Base.transactions.Cache.SetValueExt<INTran.lotSerialNbr>(existTransaction, userLotSerial);
				if (header.LotSerTrackExpiration == true && header.ExpireDate != null)
					Base.transactions.Cache.SetValueExt<INTran.expireDate>(existTransaction, header.ExpireDate);
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
				Base.transactions.Cache.SetValueExt<INTran.locationID>(existTransaction, header.LocationID);
				Base.transactions.Cache.SetValueExt<INTran.uOM>(existTransaction, header.UOM);
				Base.transactions.Cache.SetValueExt<INTran.reasonCode>(existTransaction, header.ReasonCodeID);
				existTransaction = Base.transactions.Update(existTransaction);

				Base.transactions.Cache.SetValueExt<INTran.qty>(existTransaction, header.Qty);
				existTransaction = Base.transactions.Update(existTransaction);

				Base.transactions.Cache.SetValueExt<INTran.lotSerialNbr>(existTransaction, userLotSerial);
				if (header.LotSerTrackExpiration == true && header.ExpireDate != null)
					Base.transactions.Cache.SetValueExt<INTran.expireDate>(existTransaction, header.ExpireDate);
				existTransaction = Base.transactions.Update(existTransaction);

				rollbackAction = () => Base.transactions.Delete(existTransaction);
			}

			if (!EnsureSplitsLotSerial(userLotSerial, header, rollbackAction))
			{
				if (ExplicitLineConfirmation == false)
					ClearHeaderInfo();
				return;
			}
			if (!EnsureSplitsLocation(header.LocationID, header, rollbackAction))
			{
				if (ExplicitLineConfirmation == false)
					ClearHeaderInfo();
				return;
			}

			ClearHeaderInfo();
			SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item, Msg.InventoryAdded, Base.transactions.Cache.GetValueExt<INTran.inventoryID>(existTransaction), header.Qty, header.UOM);

			if (isQtyOverridable)
				HeaderSetter.Set(x => x.IsQtyOverridable, true);
		}

		protected virtual bool ValidateConfirmation()
		{
			if (Issue?.Released == true)
			{
				ReportError(IN.Messages.Document_Status_Invalid);
				return false;
			}
			if (!HeaderView.Current.InventoryID.HasValue)
			{
				ReportError(Msg.InventoryNotSet);
				return false;
			}
			if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered && HeaderView.Current.Qty != 1)
			{
				ReportError(Msg.SerialItemNotComplexQty);
				return false;
			}

			return true;
		}

		protected virtual bool EnsureSplitsLotSerial(string userLotSerial, Header header, Action rollbackAction)
		{
			if (!string.IsNullOrEmpty(userLotSerial) &&
				Base.splits.SelectMain().Any(s => s.LotSerialNbr != userLotSerial))
			{
				ReportError(Msg.QtyIssueExceedsQtyOnLot, userLotSerial,
					HeaderView.Cache.GetValueExt<Header.inventoryID>(header));

				rollbackAction();

				ClearHeaderInfo();
				SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item);

				return false;
			}
			return true;
		}

		protected virtual bool EnsureSplitsLocation(int? userLocationID, Header header, Action rollbackAction)
		{
			if (IsLocationRequired(header) &&
				Base.splits.SelectMain().Any(s => s.LocationID != userLocationID))
			{
				ReportError(Msg.QtyIssueExceedsQtyOnLocation,
					HeaderView.Cache.GetValueExt<Header.locationID>(header),
					HeaderView.Cache.GetValueExt<Header.inventoryID>(header));

				rollbackAction();

				ClearHeaderInfo();
				SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item);

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

			INTran existTransaction = FindIssueRow(header);

			if (existTransaction != null)
			{
				var isQtyOverridable = IsQtyOverridable(HeaderView.Current.LotSerTrack);
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

				SetScanState(
					PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item,
					Msg.InventoryRemoved,
					Base.transactions.Cache.GetValueExt<INTran.inventoryID>(existTransaction), header.Qty, header.UOM);
				ClearHeaderInfo();

				if (isQtyOverridable)
					HeaderSetter.Set(x => x.IsQtyOverridable, true);
			}
			else
			{
				ReportError(Msg.IssueLineMissing, InventoryItem.PK.Find(Base, header.InventoryID).InventoryCD);
				ClearHeaderInfo();
				SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item);
			}
		}

		protected virtual void ProcessRelease()
		{
			if (Issue != null)
			{
				if (Issue.Released == true)
				{
					ReportError(Messages.Document_Status_Invalid);
					return;
				}

				if (Issue.Hold != false) Base.CurrentDocument.Cache.SetValueExt<INRegister.hold>(Issue, false);

				Save.Press();

				var clone = Base.Clone();

				WaitFor(
				(wArgs) =>
				{
					INDocumentRelease.ReleaseDoc(new List<INRegister>() { wArgs.Document }, false);
					PXLongOperation.SetCustomInfo(clone);
					throw new PXOperationCompletedException(Msg.DocumentIsReleased);
				}, null, new DocumentWaitArguments(Issue), Msg.DocumentReleasing, Base.issue.Current.RefNbr);
			}
		}

		protected override void OnWaitEnd(PXLongRunStatus status, INRegister primaryRow)
			=> OnWaitEnd(status, primaryRow?.Released == true, Msg.DocumentIsReleased, Msg.DocumentReleaseFailed);

		protected virtual INTran FindIssueRow(Header header)
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
						SetScanState(ScanStates.Item);
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
					else if (HeaderView.Current.Remove == false)
						ProcessConfirm();
					else
						ProcessConfirmRemove();
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

		private DateTime? EnsureExpireDateDefault() => LSSelect.ExpireDateByLot(Base, HeaderView.Current, null);

		protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQtyInIssue != true;
		protected override bool ExplicitLineConfirmation => Setup.Current.ExplicitLineConfirmation == true;
		protected override bool DocumentLoaded => Issue != null;
		protected bool PromptLocationForEveryLine => Setup.Current.RequestLocationForEachItemInIssue == true;

		protected override bool RequiredConfirmation(Header header)
		{
			return base.RequiredConfirmation(header)
				|| Issue?.Released != true && header?.ScanState == ScanStates.Confirm;
		}

		#region Constants & Messages
		public new abstract class Modes : WMSBase.Modes
		{
			public static WMSModeOf<INScanIssue, INScanIssueHost> ScanIssue { get; } = WMSMode("ISSU");

			public class scanIssue : PX.Data.BQL.BqlString.Constant<scanIssue> { public scanIssue() : base(ScanIssue) { } }
		}

		public new abstract class ScanStates : WMSBase.ScanStates
		{
			public const string Warehouse = "SITE";
			public const string Confirm = "CONF";
			public const string ReasonCode = "RSNC";
		}

		public new abstract class ScanCommands : WMSBase.ScanCommands
		{
			public const string Release = Marker + "RELEASE*ISSUE";
		}

		[PXLocalizable]
		public new abstract class Msg : WMSBase.Msg
		{
			public const string ScanIssueMode = "Scan and Issue";

			public const string ConfirmationPrompt = "Confirm issuing {0} {1}.";

			public const string IssueLineMissing = "Line {0} is not found in the issue.";

			public const string DocumentReleasing = "The {0} issue is being released.";
			public const string DocumentIsReleased = "The issue is successfully released.";
			public const string DocumentReleaseFailed = "The issue release failed.";
		}
		#endregion
	}
}