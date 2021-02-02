using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.IN.DAC;
using System;
using System.Linq;

namespace PX.Objects.IN.WMS
{
	public abstract class INScanRegisterCartsSupport<TSelf, TWMSSystem, TWMSHeader, TGraph> : CartsSupport<TSelf, TWMSSystem, TWMSHeader, TGraph, INRegister, INTran>
		where TSelf : INScanRegisterCartsSupport<TSelf, TWMSSystem, TWMSHeader, TGraph>
		where TWMSSystem : WarehouseManagementSystemGraph<TWMSSystem, TGraph, INRegister, TWMSHeader>
		where TWMSHeader : WMSHeader, IBqlTable, new()
		where TGraph : PXGraph
	{
		public abstract INRegister Register { get; }

		protected override bool IsEmptyCart => !RegisterCartLines.SelectMain().Any();

		#region Views

		public SelectFrom<INRegisterCart>
			.Where<INRegisterCart.FK.Cart.SameAsCurrent
				.And<INRegisterCart.FK.Register.SameAsCurrent>>.View RegisterCart;

		public SelectFrom<INRegisterCartLine>
			.Where<INRegisterCartLine.FK.RegisterCart.SameAsCurrent>.View RegisterCartLines;

		#endregion

		#region Mappings

		public override ScanLineMapping GetLineMapping()
			=> new ScanLineMapping(typeof(INTran))
			{
				LineNbr = typeof(INTran.lineNbr),
				InventoryID = typeof(INTran.inventoryID),
				SubItemID = typeof(INTran.subItemID),
				UOM = typeof(INTran.uOM),
				ReasonCode = typeof(INTran.reasonCode),
				SiteID = typeof(INTran.siteID),
				ToSiteID = typeof(INTran.toSiteID),
				LocationID = typeof(INTran.locationID),
				ToLocationID = typeof(INTran.toLocationID),
				LotSerialNbr = typeof(INTran.lotSerialNbr),
				ExpireDate = typeof(INTran.expireDate),
				Qty = typeof(INTran.qty)
			};

		public override ScanSplitMapping GetSplitMapping()
			=> new ScanSplitMapping(typeof(INTranSplit))
			{
				LotSerialNbr = typeof(INTranSplit.lotSerialNbr)
			};

		#endregion

		[PXOverride]
		public virtual void OnLocationProcessed(Action baseImpl)
		{
			if (IsCartRequired)
			{
				HeaderSetter.Set(x => x.ToLocationID, HeaderView.Current.LocationID);
				SetScanState(ScanStates.Item);
			}
			else
				baseImpl();
		}

		[PXOverride]
		public virtual void OnItemProcessed(Action baseImpl)
		{
			if (IsCartRequired)
			{
				string nextState;
				if (IsLotSerialRequired
					&& CurrentHeader.LotSerAssign == INLotSerAssign.WhenReceived)
				{
					nextState = ScanStates.LotSerial;
				}
				else
				{
					if (CurrentHeader.CartLoaded == true)
						nextState = PromptLocationForEveryLine || CurrentHeader.Remove == true ? ScanStates.ToLocation : ScanStates.Confirm;
					else
						nextState = ScanStates.Confirm;
				}
				SetScanState(nextState);
			}
			else
				baseImpl();
		}

		public virtual INRegisterCart[] GetCartRegisters(INCart cart)
			=> SelectFrom<INRegisterCart>
				.Where<INRegisterCart.FK.Cart.SameAsCurrent>
				.View.ReadOnly.SelectMultiBound(Base, new object[] { cart }).RowCast<INRegisterCart>().ToArray();

		protected override bool ValidateCart(INCart cart)
		{
			if (!base.ValidateCart(cart))
				return false;
			INRegisterCart[] carts = GetCartRegisters(cart);
			if(carts.Length > 1 
				|| (carts.Length == 1 && carts[0].DocType != INDocType.Transfer))
			{
				ReportError(Msg.SelectFreeCart, cart.CartCD);
				return false;
			}
			return true;
		}

		protected override void OnCartProcessed()
		{
			INCart cart = Cart.Current;
			INRegisterCart[] carts = GetCartRegisters(cart);
			if(carts.Length == 1)
			{
				INRegisterCart registerCart = RegisterCart.Current = carts[0];
				HeaderSetter.WithEventFiring.Set(x => x.RefNbr, registerCart.RefNbr);
				HeaderSetter.WithEventFiring.Set(x => x.ToSiteID, Register?.ToSiteID);
			}
			
			base.OnCartProcessed();
		}

		[PXOverride]
		public virtual bool HandleItemAbsence(string barcode, Func<string, bool> baseImpl)
		{
			if (IsCartRequired && CurrentHeader.CartLoaded == true)
			{
				ProcessToLocationBarcode(barcode);
				return Base1.Info.Current.MessageType == WMSMessageTypes.Information;
			}
			return baseImpl(barcode);
		}

		protected abstract void ProcessToLocationBarcode(string barcode);

		[PXOverride]
		public virtual void OnToLocationProcessed(Action baseImpl)
		{
			if (IsCartRequired && CurrentHeader.CartLoaded == true)
			{
				string nextState;
				if (PromptLocationForEveryLine || CurrentHeader.Remove == true)
					nextState = ScanStates.Confirm;
				else
					nextState = ScanStates.Item;
				SetScanState(nextState);
			}
			else
				baseImpl();
		}

		#region Documents synchronization

		protected override INRegister SyncWithDocument(ScanHeader header)
		{
			var cache = Base.Caches<INRegister>();
			if (Register == null)
				cache.Insert();

			cache.SetValueExt<INRegister.siteID>(Register, header.SiteID);
			cache.SetValueExt<INRegister.toSiteID>(Register, header.ToSiteID);
			return (INRegister)cache.Update(Register);
		}

		protected override void SyncWithDocumentCart(ScanHeader header, ScanLine line, INCartSplit cartSplit, decimal? qty)
		{
			INRegisterCart registerCart = RegisterCart.Current;
			if (registerCart == null)
				registerCart = RegisterCart.Insert();

			RegisterCart.Cache.SetValue<INRegisterCart.docType>(registerCart, Register.DocType);
			RegisterCart.Cache.SetValue<INRegisterCart.refNbr>(registerCart, Register.RefNbr);

			SyncWithDocumentCartLine(header, line, cartSplit, qty);

			if (IsEmptyCart)
				RegisterCart.Delete(registerCart);
		}

		protected virtual void SyncWithDocumentCartLine(ScanHeader header, ScanLine line, INCartSplit cartSplit, decimal? qty)
		{
			bool emptyLine = line.Qty.GetValueOrDefault() == 0;

			INRegisterCartLine docCartLine = RegisterCartLines.Search<INRegisterCartLine.lineNbr>(line.LineNbr);
			if (docCartLine == null)
			{
				if (qty <= 0)
					throw new PXArgumentException(nameof(qty));
				docCartLine = RegisterCartLines.Insert();
				RegisterCartLines.Cache.SetValue<INRegisterCartLine.cartSplitLineNbr>(docCartLine, cartSplit.SplitLineNbr);
			}
			docCartLine = (INRegisterCartLine)RegisterCartLines.Cache.CreateCopy(docCartLine);
			docCartLine.Qty += qty;
			RegisterCartLines.Cache.Update(docCartLine);

			if (docCartLine.Qty == 0)
				RegisterCartLines.Delete(docCartLine);
		}

		#endregion

		protected override decimal? GetCartQty(INTran line)
		{
			if (line == null)
				return null;
			INRegisterCartLine docCartLine = RegisterCartLines.Search<INRegisterCartLine.lineNbr>(line.LineNbr);
			return docCartLine?.Qty;
		}
	}
}
