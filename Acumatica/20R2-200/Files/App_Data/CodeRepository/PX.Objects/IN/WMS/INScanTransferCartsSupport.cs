using PX.Data;
using PX.Objects.IN.WMS;
using System;
using System.Collections;
using System.Linq;
using Header = PX.Objects.IN.INScanTransfer.Header;

namespace PX.Objects.IN
{
	public partial class INScanTransfer
	{
		public class INScanTransferCartsSupport : INScanRegisterCartsSupport<INScanTransferCartsSupport, INScanTransfer, Header, INScanTransferHost>
		{
			public override INRegister Register => Base.CurrentDocument.Current;
			protected override bool PromptLocationForEveryLine => Base1.PromptLocationForEveryLine;
			protected override bool IsLotSerialRequired => Base1.IsLotSerialRequired();
			protected override bool IsCartRequired => Base1.IsCartRequired(Base1.HeaderView.Current);
			protected override bool IsLocationRequired => Base1.IsLocationRequired(Base1.HeaderView.Current);
			protected override string MainCycleStartState => Base1.MainCycleStartState;

			public class CartQty : CartQtyExt<CartQty> { }

			#region Mappings

			public override ScanHeaderMapping GetHeaderMapping()
				=> new ScanHeaderMapping(typeof(INScanTransfer.Header))
				{
					RefNbr = typeof(Header.refNbr),
					Remove = typeof(Header.remove),
					CartID = typeof(Header.cartID),
					CartLoaded = typeof(Header.cartLoaded),
					LotSerTrack = typeof(Header.lotSerTrack),
					LotSerAssign = typeof(Header.lotSerAssign),
					IsQtyOverridable = typeof(Header.isQtyOverridable),

					InventoryID = typeof(Header.inventoryID),
					SubItemID = typeof(Header.subItemID),
					UOM = typeof(Header.uOM),
					ReasonCode = typeof(Header.reasonCodeID),
					SiteID = typeof(Header.siteID),
					ToSiteID = typeof(Header.toSiteID),
					LocationID = typeof(Header.locationID),
					ToLocationID = typeof(Header.toLocationID),
					LotSerialNbr = typeof(Header.lotSerialNbr),
					ExpireDate = typeof(Header.expireDate),
					Qty = typeof(Header.qty)
				};

			#endregion

			public IEnumerable transactions()
			{
				if (!IsCartRequired)
					return null;
				PXDelegateResult sorted = new PXDelegateResult
				{
					IsResultSorted = true,
					IsResultFiltered = true,
					IsResultTruncated = true
				};
				var view = new PXView(Base, false, Base.transactions.View.BqlSelect);
				int startRow = PXView.StartRow;
				int totalRow = 0;
				var lines = view.Select(
					PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
					ref startRow, PXView.MaximumRows, ref totalRow)
					.OfType<INTran>()
					.Select(x => new { Line = x, IsInCart = (GetCartQty(x) ?? 0) > 0 })
					.OrderByDescending(x => x.IsInCart)
					.Select(x => x.Line)
					.ToList();
				sorted.AddRange(lines);
				return sorted;
			}

			protected override void _(Events.RowSelected<Header> e)
			{
				if (IsCartRequired)
					Base1.ScanRelease.SetEnabled(Register != null && Register.Released != true && IsEmptyCart);
			}

			public override void Initialize()
			{
				base.Initialize();

				HeaderView = new PXSelectExtension<ScanHeader>(Base.HeaderView);
				LinesView = new PXSelectExtension<ScanLine>(Base.transactions);
				SplitsView = new PXSelectExtension<ScanSplit>(Base.splits);
			}

			protected override bool VerifyConfirmedLine(ScanHeader header, ScanLine line, Action rollbackAction, out WMSFlowStatus flowStatus)
			{
				if (!base.VerifyConfirmedLine(header, line, rollbackAction, out flowStatus))
					return false;
				if (header.Remove == true)
					return true;
				var args = new WMSLineVerifyingArguments<Header, INTran>(
					(Header)HeaderView.Cache.GetMain(header),
					(INTran)LinesView.Cache.GetMain(line));
				Base1.VerifyAvailability(args);
				if(args.Cancel)
				{
					flowStatus = WMSFlowStatus.Fail(args.ErrorInfo.MessageFormat, args.ErrorInfo.MessageArguments);

					rollbackAction();

					args.Processed = true;

					SetScanState(MainCycleStartState);
					return false;
				}
				flowStatus = WMSFlowStatus.Ok;
				return true;
			}

			[PXOverride]
			public virtual void OnWaitEnd(PXLongRunStatus status, INRegister primaryRow, Action<PXLongRunStatus, INRegister> baseImpl)
			{
				if (IsCartRequired)
					Base1.OnWaitEnd(status, primaryRow?.Released == true,
						INScanTransfer.Msg.DocumentIsReleased, null,
						INScanTransfer.Msg.DocumentReleaseFailed, ScanStates.ToLocation);
				else
					baseImpl(status, primaryRow);
			}

			protected override WMSFlowStatus LineMissingStatus() => WMSFlowStatus.Fail(INScanTransfer.Msg.TransferLineMissing, InventoryItem.PK.Find(Base, CurrentHeader.InventoryID).InventoryCD);
			protected override void ProcessToLocationBarcode(string barcode) => Base1.ProcessToLocationBarcode(barcode);
			protected override void ClearHeaderInfo() => Base1.ClearHeaderInfo();
			protected override void SetScanState(string state) => Base1.SetScanState(state);
			protected override void Report(string infoMsg, params object[] args) => Base1.Report(infoMsg, args);
			protected override void ReportError(string errorMsg, params object[] args) => Base1.ReportError(errorMsg, args);
			protected override void Prompt(string promptMsg, params object[] args) => Base1.Prompt(promptMsg, args);
			protected override INCart ReadCartByBarcode(string barcode) => Base1.ReadCartByBarcode(barcode);
			protected override WMSFlowStatus ExecuteAndCompleteFlow(Func<WMSFlowStatus> func) => Base1.ExecuteAndCompleteFlow(func);
		}
	}
}
