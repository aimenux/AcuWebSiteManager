using System;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;

namespace PX.Objects.IN.WMS
{
	public abstract class ScanDAC: PXMappedCacheExtension
	{
		#region InventoryID
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region SubItemID
		public virtual int? SubItemID { get; set; }
		public abstract class subItemID : BqlInt.Field<subItemID> { }
		#endregion

		#region UOM
		public virtual string UOM { get; set; }
		public abstract class uOM : BqlString.Field<uOM> { }
		#endregion

		#region ReasonCode
		public virtual string ReasonCode { get; set; }
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		#endregion

		#region SiteID
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region ToSiteID
		public virtual int? ToSiteID { get; set; }
		public abstract class toSiteID : BqlInt.Field<toSiteID> { }
		#endregion

		#region ToLocationID
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : BqlInt.Field<locationID> { }
		#endregion

		#region ToLocationID
		public virtual Int32? ToLocationID { get; set; }
		public abstract class toLocationID : BqlInt.Field<toLocationID> { }
		#endregion

		#region LotSerialNbr
		public virtual string LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : BqlString.Field<lotSerialNbr> { }
		#endregion

		#region ExpireDate
		public virtual DateTime? ExpireDate { get; set; }
		public abstract class expireDate : BqlDateTime.Field<expireDate> { }
		#endregion

		#region Qty
		public virtual decimal? Qty { get; set; }
		public abstract class qty : BqlDecimal.Field<qty> { }
		#endregion
	}

	public class ScanHeader : ScanDAC
	{
		#region RefNbr
		public virtual string RefNbr { get; set; }
		public abstract class refNbr : BqlString.Field<refNbr> { }
		#endregion

		#region Remove
		public virtual bool? Remove { get; set; }
		public abstract class remove : BqlBool.Field<remove> { }
		#endregion

		#region CartID
		public virtual int? CartID { get; set; }
		public abstract class cartID : BqlInt.Field<cartID> { }
		#endregion

		#region CartLoaded
		public virtual bool? CartLoaded { get; set; }
		public abstract class cartLoaded : BqlBool.Field<cartLoaded> { }
		#endregion

		#region LotSerTrack
		public virtual string LotSerTrack { get; set; }
		public abstract class lotSerTrack :BqlString.Field<lotSerTrack> { }
		#endregion

		#region LotSerAssign
		public virtual string LotSerAssign { get; set; }
		public abstract class lotSerAssign : BqlString.Field<lotSerAssign> { }
		#endregion

		#region IsQtyOverridable
		public virtual bool? IsQtyOverridable { get; set; }
		public abstract class isQtyOverridable : BqlBool.Field<isQtyOverridable> { }
		#endregion

		#region Base fields
		public abstract new class inventoryID : BqlInt.Field<inventoryID> { }
		public abstract new class subItemID : BqlInt.Field<subItemID> { }
		public abstract new class uOM : BqlString.Field<uOM> { }
		public abstract new class reasonCode : BqlString.Field<reasonCode> { }
		public abstract new class siteID : BqlInt.Field<siteID> { }
		public abstract new class toSiteID : BqlInt.Field<toSiteID> { }
		public abstract new class locationID : BqlInt.Field<locationID> { }
		public abstract new class toLocationID : BqlInt.Field<toLocationID> { }
		public abstract new class lotSerialNbr : BqlString.Field<lotSerialNbr> { }
		public abstract new class expireDate : BqlDateTime.Field<expireDate> { }
		public abstract new class qty : BqlDecimal.Field<qty> { }
		#endregion
	}

	public class ScanLine : ScanDAC
	{
		#region LineNbr
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		#endregion

		#region Base fields
		public abstract new class inventoryID : BqlInt.Field<inventoryID> { }
		public abstract new class subItemID : BqlInt.Field<subItemID> { }
		public abstract new class uOM : BqlString.Field<uOM> { }
		public abstract new class reasonCode : BqlString.Field<reasonCode> { }
		public abstract new class siteID : BqlInt.Field<siteID> { }
		public abstract new class toSiteID : BqlInt.Field<toSiteID> { }
		public abstract new class locationID : BqlInt.Field<locationID> { }
		public abstract new class toLocationID : BqlInt.Field<toLocationID> { }
		public abstract new class lotSerialNbr : BqlString.Field<lotSerialNbr> { }
		public abstract new class expireDate : BqlDateTime.Field<expireDate> { }
		public abstract new class qty : BqlDecimal.Field<qty> { }
		#endregion
	}

	public class ScanSplit: PXMappedCacheExtension
	{
		#region LotSerialNbr
		public virtual string LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : BqlString.Field<lotSerialNbr> { }
		#endregion

		#region ToLocationID
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : BqlInt.Field<locationID> { }
		#endregion
	}

	public abstract class ScanDACMapping : IBqlMapping
	{
		public abstract Type Extension { get; }
		public Type Table { get; }

		public Type InventoryID { get; set; }
		public Type SubItemID { get; set; }
		public Type UOM { get; set; }
		public Type ReasonCode { get; set; }
		public Type SiteID { get; set; }
		public Type ToSiteID { get; set; }
		public Type LocationID { get; set; }
		public Type ToLocationID { get; set; }
		public Type LotSerialNbr { get; set; }
		public Type ExpireDate { get; set; }
		public Type Qty { get; set; }

		protected ScanDACMapping(Type table)
		{
			Table = table;
		}
	}

	public class ScanHeaderMapping : ScanDACMapping
	{
		public override Type Extension => typeof(ScanHeader);

		public Type RefNbr { get; set; }
		public Type Remove { get; set; }
		public Type CartID { get; set; }
		public Type CartLoaded { get; set; }
		public Type LotSerTrack { get; set; }
		public Type LotSerAssign { get; set; }
		public Type IsQtyOverridable { get; set; }

		public ScanHeaderMapping(Type table): base(table)
		{
		}
	}

	public class ScanLineMapping : ScanDACMapping
	{
		public override Type Extension => typeof(ScanLine);

		public Type LineNbr { get; set; }

		public ScanLineMapping(Type table): base(table)
		{
			
		}
	}

	public class ScanSplitMapping: IBqlMapping
	{
		public Type Extension => typeof(ScanSplit);
		public Type Table { get; }

		public Type LotSerialNbr;

		public ScanSplitMapping(Type table)
		{
			Table = table;
		}
	}

	public abstract class CartsSupport<TSelf, TWMSSystem, TWMSHeader, TGraph, TScanDocument, TScanLine> : PXGraphExtension<TWMSSystem, TGraph>
		where TSelf: CartsSupport<TSelf, TWMSSystem, TWMSHeader, TGraph, TScanDocument, TScanLine>
		where TWMSSystem : WarehouseManagementSystemGraph<TWMSSystem, TGraph, TScanDocument, TWMSHeader>
		where TWMSHeader : WMSHeader, IBqlTable, new()
		where TGraph : PXGraph
		where TScanDocument : class, IBqlTable, new()
		where TScanLine : class, IBqlTable, new()
	{
		#region Attached Fields

		public abstract class DynamicDecimalField<TSelfField> 
			: PXFieldAttachedTo<TScanLine>.By<TGraph>.ByExt1<TWMSSystem>.ByExt2<TSelf>.AsDecimal.Named<TSelfField>
			where TSelfField : DynamicDecimalField<TSelfField>
		{
			protected sealed override bool? Visible => Base2.IsCartRequired;
		}

		[PXUIField(DisplayName = Msg.CartQty)]
		public abstract class CartQtyExt<TCartQty> : DynamicDecimalField<TCartQty> where TCartQty: CartQtyExt<TCartQty>
		{
			public sealed override decimal? GetValue(TScanLine row) => Base2.GetCartQty(row);
		}

		protected decimal? GetCartQty(ScanLine line) => GetCartQty(LinesView.Cache.GetMain(line) as TScanLine);

		protected virtual decimal? GetCartQty(TScanLine line) => null;

		#endregion

		#region Mappings

		public abstract ScanHeaderMapping GetHeaderMapping();
		public abstract ScanLineMapping GetLineMapping();

		public virtual ScanSplitMapping GetSplitMapping() => null;

		private Lazy<ScanHeaderMapping> HeaderMap;
		private Lazy<ScanLineMapping> LineMap;
		private Lazy<ScanSplitMapping> SplitMap;

		protected CartsSupport()
		{
			HeaderMap = new Lazy<ScanHeaderMapping>(GetHeaderMapping);
			LineMap = new Lazy<ScanLineMapping>(GetLineMapping);
			SplitMap = new Lazy<ScanSplitMapping>(GetSplitMapping);
		}

		#endregion

		#region Views

		public PXSelectExtension<ScanHeader> HeaderView;

		public PXSelectExtension<ScanLine> LinesView;

		public PXSelectExtension<ScanSplit> SplitsView;

		public SelectFrom<INCart>
			.Where<INCart.siteID.IsEqual<ScanHeader.siteID.FromCurrent>
				.And<INCart.cartID.IsEqual<ScanHeader.cartID.FromCurrent>>>.View Cart;

		public SelectFrom<INCartSplit>.Where<INCartSplit.FK.Cart.SameAsCurrent>.View CartSplits;

		#endregion

		protected ValueSetter<ScanHeader> HeaderSetter => HeaderView.GetSetterForCurrent();

		protected virtual bool IsEmptyCart => !CartSplits.SelectMain().Any();

		protected ScanHeader CurrentHeader => HeaderView.Current;

		protected abstract string MainCycleStartState { get; }

		public override void Initialize()
		{
			base.Initialize();

			Base.FieldSelecting.AddHandler(typeof(TScanLine), LineMap.Value.Qty.Name, QtySelecting);
			if (LineMap.Value.ToLocationID != null)
				Base.FieldSelecting.AddHandler(typeof(TScanLine), LineMap.Value.ToLocationID.Name, ToLocationSelecting);
		}

		#region Base implementation
		protected abstract bool IsCartRequired { get; }
		protected abstract bool IsLotSerialRequired { get; }
		protected abstract bool IsLocationRequired { get; }
		protected abstract bool PromptLocationForEveryLine { get; }

		protected abstract void ClearHeaderInfo();
		protected abstract void SetScanState(string state);
		protected abstract void Report(string infoMsg, params object[] args);
		protected abstract void ReportError(string errorMsg, params object[] args);
		protected abstract void Prompt(string promptMsg, params object[] args);
		protected abstract INCart ReadCartByBarcode(string barcode);
		protected abstract WMSFlowStatus ExecuteAndCompleteFlow(Func<WMSFlowStatus> func);
		#endregion

		#region Event handlers

		protected virtual void _(Events.RowSelected<TWMSHeader> e)
		{
			if (IsCartRequired)
				Cart.Current = Cart.Select();
		}

		protected virtual void QtySelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null && e.ExternalCall 
				&& IsCartRequired
				&& GetCartQty((TScanLine)e.Row) != null)
			{
				e.ReturnValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void ToLocationSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null && e.ExternalCall
				&& IsCartRequired
				&& GetCartQty((TScanLine)e.Row) != null)
			{
				e.ReturnValue = null;
				e.Cancel = true;
			}
		}

		#endregion

		#region Overrides

		[PXOverride]
		public virtual string GetModePrompt(Func<string> baseImpl)
		{
			if(IsCartRequired && CurrentHeader.CartID == null)
				return PXMessages.LocalizeFormatNoPrefix(Msg.CartPrompt);
			return baseImpl();
		}

		[PXOverride]
		public virtual string GetDefaultState(TWMSHeader header, Func<TWMSHeader, string> baseImpl)
		{
			if (IsCartRequired)
				return ScanStates.Cart;
			return baseImpl(header);
		}

		[PXOverride]
		public virtual void ClearMode(Action baseImpl)
		{
			if (IsCartRequired)
			{
				ClearHeaderInfo();
				if (CurrentHeader.CartLoaded == true)
					OnCartOut();
				else
					OnCartIn();
				Report(Msg.ScreenCleared);
			}
			else
				baseImpl();	
		}

		[PXOverride]
		public virtual void ApplyState(string state, Action<string> baseImpl)
		{
			if (state == ScanStates.Cart)
				Prompt(Msg.CartPrompt);
			else
				baseImpl(state);
		}

		[PXOverride]
		public virtual bool ProcessCommand(string barcode, Func<string, bool> baseImpl)
		{
			switch(barcode)
			{
				case ScanCommands.Confirm:
					if (IsCartRequired)
					{
						ProcessConfirmation();
						return true;
					}
					break;
				case ScanCommands.CartIn:
					if (!IsCartRequired)
						return false;
					ClearHeaderInfo();
					HeaderSetter.Set(x => x.CartLoaded, false);
					Report(Msg.CartLoading);

					OnCartIn();
					return true;

				case ScanCommands.CartOut:
					if (!IsCartRequired)
						return false;
					ClearHeaderInfo();
					HeaderSetter.Set(x => x.CartLoaded, true);
					Report(Msg.CartUnloading);

					OnCartOut();
					return true;
			}
			return baseImpl(barcode);
		}

		[PXOverride]
		public virtual void ProcessCartBarcode(string barcode, Action<string> baseImpl)
		{
			INCart cart = ReadCartByBarcode(barcode);
			if (cart == null)
			{
				ReportError(Msg.CartMissing, barcode);
				return;
			}
			if (!ValidateCart(cart))
				return;

			HeaderSetter.Set(x => x.CartID, cart.CartID);
			HeaderSetter.Set(x => x.SiteID, cart.SiteID);
			Cart.Current = Cart.Select();

			Report(Msg.CartReady, cart.CartCD);

			OnCartProcessed();
		}

		#endregion

		protected virtual void OnCartIn()
		{
			if (CurrentHeader.CartID == null)
				SetScanState(ScanStates.Cart);
			else
				OnCartProcessed();
		}

		protected virtual void OnCartOut()
		{
			SetScanState(ScanStates.ToLocation);
		}

		protected virtual void ProcessConfirmation()
		{
			if (CurrentHeader.Remove == true)
			{
				if (CurrentHeader.CartLoaded == true)
					MoveToCart();
				else
					RemoveFromCart();
			}
			else
			{
				if (CurrentHeader.CartLoaded == true)
					ConfirmCartOut();
				else
					ConfirmCartIn();
			}
		}

		protected virtual bool ValidateCart(INCart cart)
		{
			if (cart.SiteID != CurrentHeader.SiteID)
			{
				ReportError(Msg.CartInvalidSite, cart.CartCD);
				return false;
			}
			return true;
		}

		protected virtual void OnCartProcessed()
		{
			SetScanState(ScanStates.Location);
		}

		protected virtual bool ValidateConfirmation(out WMSFlowStatus flowStatus)
		{
			flowStatus = WMSFlowStatus.Ok;

			var needLotSerialNbr = IsLotSerialRequired
				&& CurrentHeader.LotSerAssign == INLotSerAssign.WhenReceived;

			if (needLotSerialNbr && CurrentHeader.LotSerialNbr == null)
			{
				flowStatus = WMSFlowStatus.Fail(Msg.LotSerialNotSet);
				return false;
			}
			if (IsLocationRequired 
				&& CurrentHeader.CartLoaded == true 
				&& CurrentHeader.ToLocationID == null)
			{
				flowStatus = WMSFlowStatus.Fail(Msg.ToLocationNotSelected);
				return false;
			}
			if (CurrentHeader.LotSerTrack == INLotSerTrack.SerialNumbered
				&& CurrentHeader.LotSerAssign.IsNotIn(null, INLotSerAssign.WhenUsed)
				&& CurrentHeader.Qty != 1)
			{
				flowStatus = WMSFlowStatus.Fail(Msg.SerialItemNotComplexQty);
				return false;
			}
			return true;
		}

		protected abstract TScanDocument SyncWithDocument(ScanHeader header);

		protected virtual ScanLine FindFixedLineFromCart(ScanHeader header) 
			=> FindLine(header, line =>
				line.LocationID == header.LocationID 
				&& line.ToLocationID == header.LocationID 
				&& GetCartQty(line) != null);

		protected virtual ScanLine FindAnyLineFromCart(ScanHeader header)
			=> FindLine(header, 
				line => GetCartQty(line) != null);

		protected virtual ScanLine FindLineFromDocument(ScanHeader header)
			=> FindLine(header, line =>
				line.LocationID == header.LocationID
				&& line.ToLocationID == header.ToLocationID 
				&& GetCartQty(line) == null);

		protected virtual ScanLine FindLine(ScanHeader header, Func<ScanLine, bool> search)
		{
			var existLines = LinesView.SelectMain().Where(t =>
				t.InventoryID == header.InventoryID
				&& t.SiteID == header.SiteID
				&& t.ToSiteID == header.ToSiteID
				//&& t.LocationID == (header.LocationID ?? t.LocationID)
				&& t.ReasonCode == (header.ReasonCode ?? t.ReasonCode)
				&& t.UOM == header.UOM
				&& (search == null || search(t)));

			ScanLine existLine = null;

			if (IsLotSerialRequired && SplitsView != null)
			{
				foreach (var line in existLines)
				{
					LinesView.Current = line;
					if (SplitsView.SelectMain().Any(t => (t.LotSerialNbr ?? "") == (header.LotSerialNbr ?? "")))
					{
						existLine = line;
						break;
					}
				}
			}
			else
			{
				existLine = existLines.FirstOrDefault();
			}
			return existLine;
		}

		protected virtual bool EnsureSplitsLotSerial(string userLotSerial, ScanHeader header, Action rollbackAction, out WMSFlowStatus flowStatus)
		{
			if (!string.IsNullOrEmpty(userLotSerial) 
				&& SplitsView != null
				&& SplitsView.SelectMain().Any(s => s.LotSerialNbr != userLotSerial))
			{
				flowStatus = WMSFlowStatus.Fail(Msg.QtyIssueExceedsQtyOnLot, userLotSerial,
				HeaderView.Cache.GetValueExt<ScanHeader.inventoryID>(header));

				rollbackAction();

				SetScanState(MainCycleStartState);

				return false;
			}
			flowStatus = WMSFlowStatus.Ok;
			return true;
		}

		protected virtual bool EnsureSplitsLocation(int? userLocationID, ScanHeader header, Action rollbackAction, out WMSFlowStatus flowStatus)
		{
			if (IsLocationRequired
				&& SplitsView != null
				&& SplitsView.SelectMain().Any(s => s.LocationID != userLocationID))
			{
				flowStatus = WMSFlowStatus.Fail(Msg.QtyIssueExceedsQtyOnLocation,
					HeaderView.Cache.GetValueExt<ScanHeader.locationID>(header),
					HeaderView.Cache.GetValueExt<ScanHeader.inventoryID>(header));

				rollbackAction();

				SetScanState(MainCycleStartState);

				return false;
			}
			flowStatus = WMSFlowStatus.Ok;
			return true;
		}

		protected virtual bool VerifyConfirmedLine(ScanHeader header, ScanLine line, Action rollbackAction, out WMSFlowStatus flowStatus)
			=> EnsureSplitsLotSerial(header.LotSerialNbr, header, rollbackAction, out flowStatus)
				&& (header.Remove == true || EnsureSplitsLocation(header.LocationID, header, rollbackAction, out flowStatus));

		protected virtual WMSFlowStatus LineMissingStatus() => WMSFlowStatus.Fail(Msg.InventoryNotSet);

		protected virtual bool SyncWithLines(ScanHeader header, decimal? qty, ref ScanLine line, out WMSFlowStatus flowStatus)
		{
			var linesCache = LinesView.Cache;
			Action rollbackAction = null;
			ScanLine existLine = line;
			if (existLine != null)
			{
				decimal? newQty = existLine.Qty + qty;

				var backup = linesCache.CreateCopy(existLine) as ScanLine;
				if (newQty == 0)
				{
					//remove
					linesCache.SetValueExt<ScanLine.qty>(existLine, newQty);
					existLine = (ScanLine)linesCache.Delete(existLine);
					rollbackAction = () =>
					{
						linesCache.Insert(backup);
					};
				}
				else
				{
					if (CurrentHeader.LotSerTrack == INLotSerTrack.SerialNumbered && newQty != 1)
					{
						flowStatus = WMSFlowStatus.Fail(Msg.SerialItemNotComplexQty);
						return false;
					}

					linesCache.SetValueExt<ScanLine.qty>(existLine, newQty);
					linesCache.SetValueExt<ScanLine.lotSerialNbr>(existLine, null);
					existLine = (ScanLine)linesCache.Update(existLine);

					linesCache.SetValueExt<INTran.lotSerialNbr>(existLine, header.LotSerialNbr);
					existLine = (ScanLine)linesCache.Update(existLine);

					rollbackAction = () =>
					{
						linesCache.Delete(existLine);
						linesCache.Insert(backup);
					};
				}
			}
			else
			{
				if (qty < 0)
				{
					flowStatus = LineMissingStatus();
					return false;
				}
				existLine = (ScanLine)linesCache.Insert();
				linesCache.SetValueExt<ScanLine.inventoryID>(existLine, header.InventoryID);
				linesCache.SetValueExt<ScanLine.siteID>(existLine, header.SiteID);
				linesCache.SetValueExt<ScanLine.toSiteID>(existLine, header.ToSiteID);
				linesCache.SetValueExt<ScanLine.locationID>(existLine, header.LocationID);
				linesCache.SetValueExt<ScanLine.toLocationID>(existLine, header.ToLocationID);
				linesCache.SetValueExt<ScanLine.uOM>(existLine, header.UOM);
				linesCache.SetValueExt<ScanLine.reasonCode>(existLine, header.ReasonCode);
				existLine = (ScanLine)linesCache.Update(existLine);

				linesCache.SetValueExt<INTran.qty>(existLine, qty);
				existLine = (ScanLine)linesCache.Update(existLine);

				linesCache.SetValueExt<INTran.lotSerialNbr>(existLine, header.LotSerialNbr);
				existLine = (ScanLine)linesCache.Update(existLine);

				rollbackAction = () => linesCache.Delete(existLine);
			}

			if (!VerifyConfirmedLine(header, existLine, rollbackAction, out flowStatus))
				return false;

			flowStatus = WMSFlowStatus.Ok;
			line = existLine;
			return true;
		}

		protected virtual void SetQtyOverridable(bool overridable)
		{
			if (overridable)
				HeaderSetter.Set(x => x.IsQtyOverridable, true);
		}

		protected virtual void ConfirmCartIn()
		{
			WMSFlowStatus Implementation()
			{
				WMSFlowStatus status;
				if (!ValidateConfirmation(out status))
					return status;

				ScanHeader header = CurrentHeader;
				bool isSerialItem = header.LotSerTrack == INLotSerTrack.SerialNumbered;
				var userLotSerial = header.LotSerialNbr;

				TScanDocument doc = SyncWithDocument(header);
				ScanLine existLine = FindFixedLineFromCart(header);

				if (!SyncWithLines(header, header.Qty, ref existLine, out status))
					return status;

				INCartSplit cartSplit = SyncWithCart(header, header.Qty);

				SyncWithDocumentCart(header, existLine, cartSplit, header.Qty);

				Report(Msg.InventoryAdded, LinesView.Cache.GetValueExt<ScanLine.inventoryID>(existLine), header.Qty, header.UOM);

				OnCartInConfirmed();

				return WMSFlowStatus.Ok.WithPostAction(() => SetQtyOverridable(!isSerialItem));
			}

			using (var ts = new PXTransactionScope())
			{
				var res = ExecuteAndCompleteFlow(Implementation);
				if (res.IsError == false)
					ts.Complete();
			}
		}

		protected virtual void OnCartInConfirmed()
		{
			SetScanState(MainCycleStartState);
		}

		protected virtual void ConfirmCartOut()
		{
			WMSFlowStatus Implementation()
			{
				WMSFlowStatus status;
				if (!ValidateConfirmation(out status))
					return status;

				bool isSerialItem = CurrentHeader.LotSerTrack == INLotSerTrack.SerialNumbered;
				decimal? confirmQty = CurrentHeader.Qty;

				//get item from the cart
				ScanLine existLine = FindAnyLineFromCart(CurrentHeader);
				if (existLine == null)
					return LineMissingStatus();
				HeaderSetter.Set(x => x.LocationID, existLine.LocationID);

				if (!SyncWithLines(CurrentHeader, -confirmQty, ref existLine, out status))
					return status;

				INCartSplit cartSplit = SyncWithCart(CurrentHeader, -confirmQty);
				SyncWithDocumentCart(CurrentHeader, existLine, cartSplit, -confirmQty);
				
				HeaderSetter.Set(x => x.SiteID, existLine.SiteID);
				HeaderSetter.Set(x => x.LotSerialNbr, existLine.LotSerialNbr);
				HeaderSetter.Set(x => x.ReasonCode, existLine.ReasonCode);
				HeaderSetter.Set(x => x.ExpireDate, existLine.ExpireDate);
				HeaderSetter.Set(x => x.SubItemID, existLine.SubItemID);

				//put item to the location
				existLine = FindLineFromDocument(CurrentHeader);
				if (!SyncWithLines(CurrentHeader, confirmQty, ref existLine, out status))
					return status;

				Report(Msg.InventoryAdded, LinesView.Cache.GetValueExt<ScanLine.inventoryID>(existLine), confirmQty, CurrentHeader.UOM);

				OnCartOutConfirmed();
				
				return WMSFlowStatus.Ok.WithPostAction(() => SetQtyOverridable(!isSerialItem));
			}

			using (var ts = new PXTransactionScope())
			{
				var res = ExecuteAndCompleteFlow(Implementation);
				if (res.IsError == false)
					ts.Complete();
			}
		}

		protected virtual void OnCartOutConfirmed()
		{
			SetScanState(MainCycleStartState);
		}

		protected virtual void MoveToCart()
		{
			WMSFlowStatus Implementation()
			{
				WMSFlowStatus status;
				if (!ValidateConfirmation(out status))
					return status;

				ScanHeader header = CurrentHeader;
				bool isSerialItem = header.LotSerTrack == INLotSerTrack.SerialNumbered;
				decimal? confirmQty = header.Qty;

				ScanLine existLine = FindLineFromDocument(header);
				if (existLine == null)
					return LineMissingStatus();

				if (!SyncWithLines(CurrentHeader, -confirmQty, ref existLine, out status))
					return status;

				HeaderSetter.Set(x => x.LocationID, existLine.LocationID);
				HeaderSetter.Set(x => x.ToLocationID, existLine.LocationID);
				HeaderSetter.Set(x => x.SiteID, existLine.SiteID);
				HeaderSetter.Set(x => x.LotSerialNbr, existLine.LotSerialNbr);
				HeaderSetter.Set(x => x.ReasonCode, existLine.ReasonCode);
				HeaderSetter.Set(x => x.ExpireDate, existLine.ExpireDate);
				HeaderSetter.Set(x => x.SubItemID, existLine.SubItemID);

				//put item to the cart
				existLine = FindFixedLineFromCart(CurrentHeader);
				if (!SyncWithLines(CurrentHeader, confirmQty, ref existLine, out status))
					return status;

				INCartSplit cartSplit = SyncWithCart(CurrentHeader, confirmQty);
				SyncWithDocumentCart(CurrentHeader, existLine, cartSplit, confirmQty);

				Report(Msg.InventoryRemoved, LinesView.Cache.GetValueExt<ScanLine.inventoryID>(existLine), confirmQty);

				OnMovedToCart();

				return WMSFlowStatus.Ok.WithPostAction(() => SetQtyOverridable(!isSerialItem));
			};
			using (var ts = new PXTransactionScope())
			{
				var res = ExecuteAndCompleteFlow(Implementation);
				if (res.IsError == false)
					ts.Complete();
			}
		}

		protected virtual void OnMovedToCart()
		{
			SetScanState(MainCycleStartState);
		}

		protected virtual void RemoveFromCart()
		{
			WMSFlowStatus Implementation()
			{
				WMSFlowStatus status;
				if (!ValidateConfirmation(out status))
					return status;

				ScanHeader header = CurrentHeader;
				bool isSerialItem = header.LotSerTrack == INLotSerTrack.SerialNumbered;
				decimal? qty = -header.Qty;

				TScanDocument doc = SyncWithDocument(header);
				ScanLine existLine = FindFixedLineFromCart(header);

				if (!SyncWithLines(header, qty, ref existLine, out status))
					return status;

				INCartSplit cartSplit = SyncWithCart(header, qty);

				SyncWithDocumentCart(header, existLine, cartSplit, qty);

				Report(Msg.InventoryRemoved, LinesView.Cache.GetValueExt<ScanLine.inventoryID>(existLine), -qty);

				OnRemovedFromCart();

				return WMSFlowStatus.Ok.WithPostAction(() => SetQtyOverridable(!isSerialItem));
			};
			using (var ts = new PXTransactionScope())
			{
				var res = ExecuteAndCompleteFlow(Implementation);
				if (res.IsError == false)
					ts.Complete();
			}
		}

		protected virtual void OnRemovedFromCart()
		{
			SetScanState(MainCycleStartState);
		}

		#region INCartSplit synchronization

		protected virtual INCartSplit SyncWithCart(ScanHeader header, decimal? qty)
		{
			INCartSplit cartSplit = CartSplits.Search<INCartSplit.inventoryID, INCartSplit.subItemID, INCartSplit.fromLocationID, INCartSplit.lotSerialNbr>(
				header.InventoryID, header.SubItemID, header.LocationID, string.IsNullOrEmpty(header.LotSerialNbr) ? null : header.LotSerialNbr);
			if (cartSplit == null)
			{
				cartSplit = CartSplits.Insert(new INCartSplit
				{
					CartID = header.CartID,
					InventoryID = header.InventoryID,
					SubItemID = header.SubItemID,
					LotSerialNbr = header.LotSerialNbr,
					ExpireDate = header.ExpireDate,
					UOM = header.UOM,
					SiteID = header.SiteID,
					FromLocationID = header.LocationID,
					Qty = qty
				});
			}
			else
			{
				var copy = (INCartSplit)CartSplits.Cache.CreateCopy(cartSplit);
				copy.Qty += qty;
				cartSplit = CartSplits.Update(copy);

				if (cartSplit.Qty == 0)
					cartSplit = CartSplits.Delete(cartSplit);
			}

			return cartSplit;
		}

		#endregion

		#region DocumentCart synchronization

		protected virtual void SyncWithDocumentCart(ScanHeader header, ScanLine line, INCartSplit cartSplit, decimal? qty)
			=> SyncWithDocumentCart(HeaderView.Cache.GetMain(header) as TWMSHeader, LinesView.Cache.GetMain(line) as TScanLine, cartSplit, qty);

		protected virtual void SyncWithDocumentCart(TWMSHeader header, TScanLine scanLine, INCartSplit cartSplit, decimal? qty)
		{

		}

		#endregion

		#region Constants & Messages

		public abstract class ScanCommands : WarehouseManagementSystemGraph<TWMSSystem, TGraph, TScanDocument, TWMSHeader>.ScanCommands
		{
			public const string CartIn = Marker + "CART*IN";
			public const string CartOut = Marker + "CART*OUT";
		}

		[PXLocalizable]
		public abstract class Msg : WarehouseManagementSystemGraph<TWMSSystem, TGraph, TScanDocument, TWMSHeader>.Msg
		{
			public const string NothingToPutAway = "No items to put away.";

			public const string SelectFreeCart = "The {0} cart is in use. Select another cart.";
		}

		public abstract class ScanStates : WarehouseManagementSystemGraph<TWMSSystem, TGraph, TScanDocument, TWMSHeader>.ScanStates
		{
			public const string Warehouse = "SITE";
			public const string ChooseWarehouse = "WLOC";
			public const string Confirm = "CONF";
			public const string ToLocation = "TLOC";
			public const string ReasonCode = "RSNC";
		}

		#endregion
	}
}