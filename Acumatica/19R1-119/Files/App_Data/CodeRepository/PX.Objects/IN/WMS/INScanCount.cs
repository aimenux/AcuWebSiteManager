using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections;
using System.Linq;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.IN.INScanCount, PX.Objects.IN.INScanCountHost, PX.Objects.IN.INPIDetail, PX.Objects.IN.INScanCount.Header>;

namespace PX.Objects.IN
{
	public class INScanCountHost : INPICountEntry
	{
		public override Type PrimaryItemType => typeof(INScanCount.Header);
		public PXFilter<INScanCount.Header> HeaderView;

		protected override void INPIHeader_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			// We don't execute the base method (with the body: "e.Cancel = true")
			// because needs save a new value "LineCntr" when add a new detail row.
		}
	}

	public class INScanCount : WMSBase
	{
		#region DACs
		public class Header : WMSHeader, ILSMaster
		{
			#region PIID
			[PXUnboundDefault(typeof(INPIDetail.pIID))]
			[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
			[PXSelector(typeof(Search<INPIHeader.pIID>))]
			public override string RefNbr { get; set; }
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
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
			#region ILSMaster implementation
			public string TranType => string.Empty;
			public DateTime? TranDate => null;
			public short? InvtMult { get => 1; set { } }
			public int? ProjectID { get; set; }
			public int? TaskID { get; set; }
			#endregion
		}
		#endregion

		#region Views
		public override PXFilter<Header> HeaderView => Base.HeaderView;
		public PXSetupOptional<INScanSetup, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>> Setup;
		public PXSelect<INPIClass, Where<INPIClass.pIClassID, Equal<Current<INPIHeader.pIClassID>>>> Class;
		#endregion

		#region Buttons
		public PXAction<Header> ScanConfirmDocument;
		[PXButton, PXUIField(DisplayName = "Confirm")]
		protected virtual IEnumerable scanConfirmDocument(PXAdapter adapter) => scanBarcode(adapter, ScanCommands.ConfirmDocument);

		public PXAction<Header> Review;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable review(PXAdapter adapter) => adapter.Get();
		#endregion

		#region Event Handlers
		#region Header
		protected virtual void _(Events.FieldUpdated<Header, Header.refNbr> e) => Base.PIHeader.Current = Base.PIHeader.Search<INPIHeader.pIID>(e.Row.RefNbr);

		protected override void _(Events.RowSelected<Header> e)
		{
			base._(e);

			ScanModePhysicalCount.SetEnabled(e.Row != null && e.Row.Mode != Modes.ScanInCount);

			Review.SetVisible(Base.IsMobile);

			ScanRemove.SetEnabled(IsDocumentEditable && e.Row != null);

			ScanConfirm.SetEnabled(PIHeader?.PIID != null && e.Row?.ScanState == ScanStates.Confirm);

			Logs.AllowInsert = Logs.AllowDelete = Logs.AllowUpdate = false;
			Base.PIDetail.AllowInsert = Base.PIDetail.AllowDelete = Base.PIDetail.AllowUpdate = false;

			ScanConfirmDocument.SetEnabled(IsDocumentEditable);

			if (e.Row?.RefNbr != null && Base.PIHeader.Current == null)
			{
				Base.PIHeader.Current = Base.PIHeader.Search<INPIHeader.pIID>(e.Row.RefNbr);
			}
		}

		protected virtual void _(Events.FieldVerifying<Header, Header.inventoryID> e)
		{
			var piHeader = Base.PIHeader.Current;
			if (e.NewValue == null || e.Row == null || piHeader?.SiteID == null) return;

			var inspector = new PhysicalInventory.PILocksInspector(piHeader.SiteID.Value);
			if (!inspector.IsInventoryLocationIncludedInPI((int?)e.NewValue, e.Row.LocationID, piHeader.PIID))
			{
				throw new PXSetPropertyException(Messages.InventoryShouldBeUsedInCurrentPI);
			}
		}
		#endregion
		#endregion

		#region DAC overrides
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Enabled), false)]
		protected virtual void _(Events.CacheAttached<INPIDetail.physicalQty> e) { }
		#endregion

		private INPIHeader PIHeader => Base.PIHeader.Current;

		protected bool IsDocumentEditable => PIHeader?.PIID != null && IsDocumentStatusEditable(PIHeader?.Status);
		protected bool IsDocumentStatusEditable(string status) => status == INPIHdrStatus.Counting;

		protected override WMSModeOf<INScanCount, INScanCountHost> DefaultMode => Modes.ScanInCount;
		public override string CurrentModeName =>
			HeaderView.Current.Mode == Modes.ScanInCount ? Msg.ScanInCountMode :
			Msg.FreeMode;
		protected override string GetModePrompt()
		{
			if (HeaderView.Current.Mode == Modes.ScanInCount)
			{
				if (PIHeader?.PIID == null)
					return Localize(Msg.DocumentNumberPrompt);
				if (HeaderView.Current.LocationID == null)
					return Localize(Msg.LocationPrompt);
				if (HeaderView.Current.InventoryID == null)
					return Localize(Msg.InventoryPrompt);
				if (HeaderView.Current.LotSerialNbr == null)
					return Localize(Msg.LotSerialPrompt);
				return Localize(Msg.ConfirmationPrompt);
			}
			return null;
		}

		protected override bool ProcessCommand(string barcode)
		{
			switch (barcode)
			{
				case ScanCommands.Confirm:
					ProcessConfirm();
					return true;

				case ScanCommands.Remove:
					HeaderView.Current.Remove = true;
					SetScanState(ScanStates.Item, Msg.RemoveMode);
					return true;

				case ScanCommands.ConfirmDocument:
					ProcessConfirmDocument();
					return true;
			}
			return false;
		}

		protected override void ProcessDocumentNumber(string barcode)
		{
			var piHeader = (INPIHeader)PXSelectorAttribute.Select<Header.refNbr>(HeaderView.Cache, HeaderView.Current, barcode);

			if (piHeader == null)
			{
				ReportError(Msg.DocumentMissing, barcode);
			}
			else if (IsValid<Header.refNbr>(barcode, out string error) == false)
			{
				ReportError(error);
			}
			else if (!IsDocumentStatusEditable(piHeader.Status))
			{
				ReportError(Msg.InvalidDocumentStatus, Base.PIHeader.Cache.GetStateExt<INPIHeader.status>(piHeader));
			}
			else
			{
				HeaderView.Current.RefNbr = barcode;
				HeaderView.Current.SiteID = piHeader.SiteID;
				HeaderView.Current.NoteID = piHeader.NoteID;
				Base.PIHeader.Current = piHeader;
				SetScanState(ScanStates.Location, Msg.DocumentReady, barcode);
			}
		}

		protected override void ProcessLocationBarcode(string barcode)
		{
			INLocation location = ReadLocationByBarcode(HeaderView.Current.SiteID, barcode);
			if (location == null)
				return;

			var statusLocation =
				(INPIStatusLoc)PXSelectReadonly<INPIStatusLoc,
				Where<INPIStatusLoc.pIID, Equal<Current<Header.refNbr>>,
					And<Where<INPIStatusLoc.locationID, Equal<Required<Header.locationID>>, Or<INPIStatusLoc.locationID, IsNull>>>>>.Select(Base, location.LocationID);

			if (statusLocation == null)
			{
				ReportError(Msg.LocationNotPresent, barcode);
				return;
			}

			HeaderView.Current.LocationID = location.LocationID;
			SetScanState(ScanStates.Item, Msg.LocationReady, location.LocationCD);
		}

		protected override void ProcessItemBarcode(string barcode)
		{
			var item = ReadItemByBarcode(barcode, INPrimaryAlternateType.CPN);
			if (item == null)
			{
				ProcessLocationBarcode(barcode);
				if (Info.Current.MessageType == WMSMessageTypes.Information)
				{
					return; // location found
				}
				else
				{
					ReportError(Msg.InventoryMissing, barcode);
					return;
				}
			}

			INItemXRef xref = item;
			InventoryItem inventoryItem = item;
			INLotSerClass lsclass = item;
			var uom = xref.UOM ?? inventoryItem.PurchaseUnit;

			if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered &&
				lsclass.LotSerAssign != INLotSerAssign.WhenUsed &&
				uom != inventoryItem.BaseUnit)
			{
				ReportError(Msg.SerialItemNotComplexQty);
				return;
			}

			if (!IsValid<Header.inventoryID>(inventoryItem.InventoryID, out string errorInventory))
			{
				ReportError(errorInventory);
				return;
			}

			HeaderView.Current.InventoryID = xref.InventoryID;
			HeaderView.Current.SubItemID = xref.SubItemID;
			if (HeaderView.Current.UOM == null)
				HeaderView.Current.UOM = uom;
			HeaderView.Current.LotSerTrack = lsclass.LotSerTrack;
			HeaderView.Current.LotSerAssign = lsclass.LotSerAssign;

			if (!IsValid<Header.qty>(HeaderView.Current.Qty, out string errorQty))
			{
				ReportError(errorQty);
				return;
			}

			if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered && lsclass.LotSerAssign != INLotSerAssign.WhenUsed)
			{
				SetScanState(ScanStates.LotSerial, Msg.InventoryReady, inventoryItem.InventoryCD);
			}
			else
			{
				SetScanState(ScanStates.Confirm, Msg.InventoryReady, inventoryItem.InventoryCD);
			}
		}

		protected override void ProcessLotSerialBarcode(string barcode)
		{
			if (IsValid<Header.lotSerialNbr>(barcode, out string error) == false)
			{
				ReportError(error);
				return;
			}

			HeaderView.Current.LotSerialNbr = barcode;
			SetScanState(ScanStates.Confirm, Msg.LotSerialReady, barcode);
		}

		protected override bool ProcessQtyBarcode(string barcode)
		{
			var result = base.ProcessQtyBarcode(barcode);
			if (!result) return false;

			if (!IsValid<Header.qty>(HeaderView.Current.Qty, out string error))
			{
				HeaderView.Current.Qty = 1;
				ReportError(error);
				return false;
			}

			if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered &&
				HeaderView.Current.LotSerAssign != INLotSerAssign.WhenUsed &&
				HeaderView.Current.Qty != 1)
			{
				HeaderView.Current.Qty = 1;
				ReportError(Msg.SerialItemNotComplexQty);
				return false;
			}

			return true;
		}

		protected override void ProcessExpireDate(string barcode) => throw new NotImplementedException();
		protected override void ProcessCartBarcode(string barcode) => throw new NotImplementedException();

		protected virtual void ProcessConfirm()
		{
			if (!ValidateConfirmation())
			{
				if (ExplicitLineConfirmation == false)
					ClearHeaderInfo();
				return;
			}

			var header = HeaderView.Current;

			INPIDetail row = FindDetailRow(header);

			decimal? newQty = row?.PhysicalQty ?? 0;
			if (HeaderView.Current.Remove != true)
				newQty += header.BaseQty;
			else
				newQty -= header.BaseQty;

			bool noQtyState = HeaderView.Current.PrevScanState != ScanStates.Qty;
			bool isSerialItem = HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered;
			if (isSerialItem &&
				HeaderView.Current.LotSerAssign != INLotSerAssign.WhenUsed &&
				newQty.IsNotIn(0, 1))
			{
				if (ExplicitLineConfirmation == false)
					ClearHeaderInfo();
				ReportError(Msg.SerialItemNotComplexQty);
				return;
			}

			if (row == null)
			{
				if (!IsValid<Header.inventoryID>(HeaderView.Current.InventoryID, out string errorInventory))
				{
					ClearHeaderInfo();
					SetScanState(header.LocationID == null ? ScanStates.Location : ScanStates.Item);
					ReportError(Msg.InventoryNotPresent, HeaderView.Cache.GetValueExt<Header.inventoryID>(header));
					return;
				}

				row = (INPIDetail)Base.PIDetail.Cache.CreateInstance();
				row.PIID = header.RefNbr;
				row.LineNbr = (int)PXLineNbrAttribute.NewLineNbr<INPIDetail.lineNbr>(Base.PIDetail.Cache, Base.PIHeader.Current);
				row.InventoryID = header.InventoryID;
				row.SubItemID = header.SubItemID;
				row.SiteID = header.SiteID;
				row.LocationID = header.LocationID;
				row.LotSerialNbr = header.LotSerialNbr;
				row.PhysicalQty = header.Qty;
				row.BookQty = 0;
				row.ExpireDate = header.ExpireDate;
				Base.PIDetail.Insert(row);
				Save.Press();
			}

			Base.PIDetail.Cache.SetValueExt<INPIDetail.physicalQty>(row, newQty);
			row = Base.PIDetail.Update(row);

			SetScanState(ScanStates.Item, Msg.InventoryAdded, Base.PIDetail.Cache.GetValueExt<INTran.inventoryID>(row), header.Qty, header.UOM);
			ClearHeaderInfo();
			if (!isSerialItem && noQtyState)
				HeaderView.Current.IsQtyOverridable = true;
		}

		protected virtual bool ValidateConfirmation()
		{
			var needLotSerialNbr = HeaderView.Current.LotSerTrack != INLotSerTrack.NotNumbered &&
				HeaderView.Current.LotSerAssign != INLotSerAssign.WhenUsed;

			if (PIHeader == null)
			{
				ReportError(Msg.DocumentNotSelected);
				return false;
			}

			if (!IsDocumentEditable)
			{
				ReportError(Msg.InvalidDocumentStatus, Base.PIHeader.Cache.GetStateExt<INPIHeader.status>(PIHeader));
				return false;
			}

			if (HeaderView.Current.InventoryID == null)
			{
				ReportError(Msg.InventoryNotSet);
				return false;
			}

			if (needLotSerialNbr && HeaderView.Current.LotSerialNbr == null)
			{
				ReportError(Msg.LotSerialNotSet);
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

		protected virtual void ProcessConfirmDocument()
		{
			if (PIHeader == null)
			{
				ReportError(Msg.DocumentNotSelected);
				return;
			}

			if (!IsDocumentEditable)
			{
				ReportError(Msg.InvalidDocumentStatus, Base.PIHeader.Cache.GetStateExt<INPIHeader.status>(PIHeader));
				return;
			}

			var detailsNull = Base.PIDetail.SelectMain().AsEnumerable().Where(d => d.PhysicalQty == null);

			foreach (var detail in detailsNull)
			{
				Base.PIDetail.SetValueExt<INPIDetail.physicalQty>(detail, 0m);
				Base.PIDetail.Update(detail);
			}

			this.Save.Press();

			ClearHeaderInfo(true);
			HeaderView.Current.RefNbr = null;
			HeaderView.Current.SiteID = null;
			Base.PIHeader.Current = null;

			SetScanState(GetDefaultState(), Msg.CountConfirmed);
		}

		protected virtual INPIDetail FindDetailRow(Header header)
		{
			return Base.PIDetail.SelectMain().FirstOrDefault(t =>
				t.InventoryID == header.InventoryID &&
				t.SiteID == header.SiteID &&
				t.LocationID == (header.LocationID ?? t.LocationID) &&
				t.LotSerialNbr == (header.LotSerialNbr ?? t.LotSerialNbr));
		}

		protected override void ClearHeaderInfo(bool redirect = false)
		{
			base.ClearHeaderInfo(redirect);

			if (redirect)
			{
				HeaderView.Current.SiteID = null;
				HeaderView.Current.LocationID = null;
			}
			HeaderView.Current.LotSerialNbr = null;
			HeaderView.Current.LotSerTrack = null;
			HeaderView.Current.LotSerAssign = null;
			HeaderView.Current.ExpireDate = null;
		}

		protected override string GetDefaultState(Header header = null) => ScanStates.RefNbr;

		protected override void ApplyState(string state)
		{
			switch (state)
			{
				case ScanStates.RefNbr:
					Prompt(Msg.DocumentNumberPrompt);
					break;
				case ScanStates.Location:
					if (IsLocationRequired(HeaderView.Current))
						Prompt(Msg.LocationPrompt);
					else
						SetScanState(ScanStates.Item);
					break;
				case ScanStates.Item:
					Prompt(Msg.InventoryPrompt);
					break;
				case ScanStates.LotSerial:
					Prompt(Msg.LotSerialPrompt);
					break;
				case ScanStates.Confirm:
					if (IsMandatoryQtyInput)
					{
						Prompt(Msg.QtyPrompt);
						SetScanState(ScanStates.Qty);
					}
					else
					{
						Prompt(Msg.ConfirmationPrompt);

						var row = FindDetailRow(HeaderView.Current);

						if (!IsValid<Header.inventoryID>(HeaderView.Current.InventoryID, out string errorInventory))
						{
							ClearHeaderInfo();
							SetScanState(ScanStates.Location);
							ReportError(Msg.InventoryNotPresent, HeaderView.Cache.GetValueExt<Header.inventoryID>(HeaderView.Current));
						}
						else if (Class.SelectSingle()?.IncludeZeroItems == false && row?.BookQty == 0)
							ReportWarning(Msg.InventoryQtyZero, HeaderView.Cache.GetValueExt<Header.inventoryID>(HeaderView.Current));
						else if (ExplicitLineConfirmation == false)
							ProcessConfirm();
					}
					break;
			}
		}

		protected override void ClearMode()
		{
			ClearHeaderInfo();
			SetScanState(HeaderView.Current.RefNbr == null ? ScanStates.RefNbr :
				(HeaderView.Current.LocationID == null ? ScanStates.Location : ScanStates.Item), Msg.ScreenCleared);
		}

		protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQtyInCount != true;
		protected override bool ExplicitLineConfirmation => Setup.Current.ExplicitLineConfirmation == true;

		#region Constants & Messages
		public new abstract class Modes : WMSBase.Modes
		{
			public static WMSModeOf<INScanCount, INScanCountHost> ScanInCount { get; } = WMSMode("INCO");

			public class scanCount : PX.Data.BQL.BqlString.Constant<scanCount> { public scanCount() : base(ScanInCount) { } }
		}

		public new abstract class ScanStates : WMSBase.ScanStates
		{
			public const string Confirm = "CONF";
		}

		public new abstract class ScanCommands : WMSBase.ScanCommands
		{
			public const string ConfirmDocument = Marker + "CONFIRM*DOCUMENT";
		}

		[PXLocalizable]
		public new abstract class Msg : WMSBase.Msg
		{
			public const string ScanInCountMode = "Scan and Count";

			public const string DocumentNumberPrompt = "Scan or enter a reference number of the PI count.";
			public const string DocumentReady = "The {0} PI count has been loaded and is ready for processing.";
			public const string DocumentMissing = "The {0} PI count was not found.";
			public const string DocumentNotSelected = "Document number is not selected.";
			public const string InvalidDocumentStatus = "Document has the {0} status, cannot be used for count.";

			public new const string LocationPrompt = "Scan the Location ID. All items scanned after scanning the location ID will be assigned to this location.";
			public const string LocationNotPresent = "The {0} location is not in the list and cannot be added.";

			public const string ConfirmationPrompt = "Confirm the line, or scan or enter the line quantity.";

			public const string InventoryNotPresent = "The {0} item is not in the list and cannot be added.";
			public const string InventoryQtyZero = "The {0} item is not in the list. Would you like to add the item?";

			public const string CountConfirmed = "The count has been saved.";
		}
		#endregion
	}
}