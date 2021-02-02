using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.Attributes;
using PX.Objects.IN.Matrix.DAC.Unbound;
using PX.Objects.IN.Matrix.GraphExtensions;

namespace PX.Objects.IN.Matrix.Graphs
{
	public class MatrixItemsStatusInquiry : PXGraph<MatrixItemsStatusInquiry, EntryHeader>
	{
		public class MatrixItemsStatusInquiryImpl : MatrixGridExt<MatrixItemsStatusInquiry, EntryHeader>
		{
			public override bool AddTotals => true;

			public override void Initialize()
			{
				base.Initialize();

				PXUIFieldAttribute.SetRequired<EntryHeader.templateItemID>(Header.Cache, true);
				PXUIFieldAttribute.SetRequired<EntryHeader.colAttributeID>(Header.Cache, true);
				PXUIFieldAttribute.SetRequired<EntryHeader.rowAttributeID>(Header.Cache, true);

				PXUIFieldAttribute.SetVisible<EntryHeader.showAvailable>(Header.Cache, null, false);
				Header.Cache.Adjust<PlanType.ListAttribute>().For<EntryHeader.displayPlanType>(a => a.RefillLabels());
			}

			[PXMergeAttributes(Method = MergeMethod.Append)]
			[PXRestrictor(typeof(Where<InventoryItem.stkItem, Equal<True>>), Messages.InventoryItemIsNotAStock)]
			protected virtual void _(Events.CacheAttached<EntryHeader.templateItemID> e)
			{
			}

			protected virtual void _(Events.FieldUpdated<EntryHeader, EntryHeader.locationID> eventArgs)
				=> RecalcMatrixGrid();

			protected virtual void _(Events.FieldUpdated<EntryHeader, EntryHeader.displayPlanType> eventArgs)
				=> RecalcMatrixGrid();

			protected override void FillInventoryMatrixItem(EntryMatrix newRow, int colAttributeIndex, InventoryMapValue inventoryValue)
			{
				newRow.InventoryIDs[colAttributeIndex] = inventoryValue?.InventoryID;

				switch (Header.Current.DisplayPlanType)
				{
					case PlanType.Available:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyAvail>(inventoryValue);
						break;
					case PlanType.AvailableforShipment:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyHardAvail>(inventoryValue);
						break;
					case PlanType.NotAvailable:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyNotAvail>(inventoryValue);
						break;
					case PlanType.SOBooked:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtySOBooked>(inventoryValue);
						break;
					case PlanType.SOShipped:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtySOShipped>(inventoryValue);
						break;
					case PlanType.SOShipping:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtySOShipping>(inventoryValue);
						break;
					case PlanType.SOBackOrdered:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtySOBackOrdered>(inventoryValue);
						break;
					case PlanType.InAssemblyDemand:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyINAssemblyDemand>(inventoryValue);
						break;
					case PlanType.INIssues:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyINIssues>(inventoryValue);
						break;
					case PlanType.INReceipts:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyINReceipts>(inventoryValue);
						break;
					case PlanType.InTransit:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyInTransit>(inventoryValue);
						break;
					case PlanType.PurchasePrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOPrepared>(inventoryValue);
						break;
					case PlanType.PurchaseOrders:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOOrders>(inventoryValue);
						break;
					case PlanType.POReceipts:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOReceipts>(inventoryValue);
						break;
					case PlanType.SOtoPurchase:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtySOFixed>(inventoryValue);
						break;
					case PlanType.PurchaseforSO:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOFixedOrders>(inventoryValue);
						break;
					case PlanType.PurchaseforSOPrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOFixedPrepared>(inventoryValue);
						break;
					case PlanType.PurchaseReceiptsForSO:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOFixedReceipts>(inventoryValue);
						break;
					case PlanType.SOtoDropShip:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtySODropShip>(inventoryValue);
						break;
					case PlanType.DropShipforSO:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPODropShipOrders>(inventoryValue);
						break;
					case PlanType.DropShipforSOPrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPODropShipPrepared>(inventoryValue);
						break;
					case PlanType.DropShipforSOReceipts:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPODropShipReceipts>(inventoryValue);
						break;
					case PlanType.InAssemblySupply:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyINAssemblySupply>(inventoryValue);
						break;
					case PlanType.OnHand:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyOnHand>(inventoryValue);
						break;
					case PlanType.Expired:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyExpired>(inventoryValue);
						break;

					case PlanType.FSSrvOrdPrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyFSSrvOrdPrepared>(inventoryValue);
						break;
					case PlanType.FSSrvOrdBooked:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyFSSrvOrdBooked>(inventoryValue);
						break;
					case PlanType.FSSrvOrdAllocated:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyFSSrvOrdAllocated>(inventoryValue);
						break;
					case PlanType.FixedFSSrvOrd:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyFixedFSSrvOrd>(inventoryValue);
						break;
					case PlanType.POFixedFSSrvOrd:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOFixedFSSrvOrd>(inventoryValue);
						break;
					case PlanType.POFixedFSSrvOrdPrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOFixedFSSrvOrdPrepared>(inventoryValue);
						break;
					case PlanType.POFixedFSSrvOrdReceipts:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOFixedFSSrvOrdReceipts>(inventoryValue);
						break;

					case PlanType.InTransitToProduction:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyInTransitToProduction>(inventoryValue);
						break;
					case PlanType.ProductionSupplyPrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProductionSupplyPrepared>(inventoryValue);
						break;
					case PlanType.ProductionSupply:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProductionSupply>(inventoryValue);
						break;
					case PlanType.POFixedProductionPrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOFixedProductionPrepared>(inventoryValue);
						break;
					case PlanType.POFixedProductionOrders:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyPOFixedProductionOrders>(inventoryValue);
						break;
					case PlanType.ProductionDemandPrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProductionDemandPrepared>(inventoryValue);
						break;
					case PlanType.ProductionDemand:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProductionDemand>(inventoryValue);
						break;
					case PlanType.ProductionAllocated:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProductionAllocated>(inventoryValue);
						break;
					case PlanType.SOFixedProduction:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtySOFixedProduction>(inventoryValue);
						break;
					case PlanType.ProdFixedPurchase:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProdFixedPurchase>(inventoryValue);
						break;
					case PlanType.ProdFixedProduction:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProdFixedProduction>(inventoryValue);
						break;
					case PlanType.ProdFixedProdOrdersPrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProdFixedProdOrdersPrepared>(inventoryValue);
						break;
					case PlanType.ProdFixedProdOrders:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProdFixedProdOrders>(inventoryValue);
						break;
					case PlanType.ProdFixedSalesOrdersPrepared:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProdFixedSalesOrdersPrepared>(inventoryValue);
						break;
					case PlanType.ProdFixedSalesOrders:
						newRow.Quantities[colAttributeIndex] = GetQty<INSiteStatus.qtyProdFixedSalesOrders>(inventoryValue);
						break;

					default:
						throw new PXArgumentException(nameof(Header.Current.DisplayPlanType));
				}
			}

			protected virtual decimal GetQty<TField>(InventoryMapValue inventoryValue)
				where TField : IBqlField
			{
				INLocationStatus ls = inventoryValue?.LocationStatus;
				if (ls != null)
				{
					return (decimal?)Base.Caches<INLocationStatus>().GetValue<TField>(ls) ?? 0m;
				}
				else
				{
					INSiteStatus ws = inventoryValue?.SiteStatus;
					return (decimal?)Base.Caches<INSiteStatus>().GetValue<TField>(ws) ?? 0m;
				}
			}

			protected override void FieldSelectingImpl(int attributeNumber, PXCache s, PXFieldSelectingEventArgs e, string fieldName)
			{
				var matrix = (EntryMatrix)e.Row;
				int? inventoryId = GetValueFromArray(matrix?.InventoryIDs, attributeNumber);
				decimal? qty = GetValueFromArray(matrix?.Quantities, attributeNumber);
				string error = GetValueFromArray(matrix?.Errors, attributeNumber);

				var state = PXDecimalState.CreateInstance(e.ReturnState, _precision.Value, fieldName, false, 0, 0m, null);
				state.Enabled = false;
				state.Error = error;
				state.ErrorLevel = string.IsNullOrEmpty(error) ? PXErrorLevel.Undefined : PXErrorLevel.Warning;
				e.ReturnState = state;
				e.ReturnValue = (inventoryId != null || matrix?.IsTotal == true) ? qty : null;

				var anyMatrixRow = matrix ?? GetFirstMatrixRow();
				if (attributeNumber < anyMatrixRow?.ColAttributeValueDescrs?.Length)
				{
					state.DisplayName = anyMatrixRow.ColAttributeValueDescrs[attributeNumber] ?? anyMatrixRow.ColAttributeValues[attributeNumber];
					state.Visibility = PXUIVisibility.Visible;
					state.Visible = true;
				}
				else
				{
					state.DisplayName = null;
					state.Visibility = PXUIVisibility.Invisible;
					state.Visible = false;
				}
			}

			protected override void FieldUpdatingImpl(int attributeNumber, PXCache s, PXFieldUpdatingEventArgs e, string fieldName)
			{
			}

			protected override EntryMatrix GenerateTotalRow(IEnumerable<EntryMatrix> rows)
			{
				bool rowsExist = false;
				var totalRow = (EntryMatrix)Matrix.Cache.CreateInstance();

				foreach (EntryMatrix row in Matrix.Cache.Cached)
				{
					rowsExist = true;

					if (totalRow.Quantities == null)
						totalRow.Quantities = new decimal?[row.Quantities.Length];

					for (int columnIndex = 0; columnIndex < row.Quantities.Length; columnIndex++)
					{
						totalRow.Quantities[columnIndex] = totalRow.Quantities[columnIndex] ?? 0m;
						totalRow.Quantities[columnIndex] += row.Quantities[columnIndex] ?? 0m;
					}
				}

				totalRow.RowAttributeValueDescr = PXLocalizer.Localize(Messages.TotalQty);
				totalRow.IsTotal = true;
				totalRow.LineNbr = int.MaxValue;

				return rowsExist ? totalRow : null;
			}

			protected override void TotalFieldSelecting(PXCache s, PXFieldSelectingEventArgs e, string fieldName)
			{
				var matrix = (EntryMatrix)e.Row;

				var state = PXDecimalState.CreateInstance(e.ReturnState, _precision.Value, fieldName, false, 0, 0m, null);
				e.ReturnState = state;
				state.Enabled = false;

				state.DisplayName = PXLocalizer.Localize(Messages.TotalQty);

				var anyMatrixRow = matrix ?? GetFirstMatrixRow();
				if (anyMatrixRow?.ColAttributeValueDescrs?.Length > 0)
				{
					state.Visibility = PXUIVisibility.Visible;
					state.Visible = true;
				}
				else
				{
					state.Visibility = PXUIVisibility.Invisible;
					state.Visible = false;
				}

				decimal sum = 0;
				for (int columnIndex = 0; columnIndex < matrix?.Quantities?.Length; columnIndex++)
					sum += (matrix.Quantities[columnIndex] ?? 0m);

				e.ReturnValue = sum;
			}

			protected override List<PXResult<CSAnswers, CSAttributeGroup, InventoryItem>> SelectInventoryWithAttributes()
			{
				Type sumDecimalFields = BqlCommand.Compose(BqlHelper.GetDecimalFieldsAggregate<INSiteStatus>(Base, true).ToArray());

				Type select = BqlTemplate.OfCommand<
					Select5<CSAnswers,
					InnerJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>>,
					InnerJoin<InventoryItem, On<CSAnswers.refNoteID, Equal<InventoryItem.noteID>,
						And<CSAttributeGroup.entityClassID, Equal<InventoryItem.itemClassID>>>,
					LeftJoin<INSiteStatus, On<INSiteStatus.inventoryID, Equal<InventoryItem.inventoryID>,
						And<INSiteStatus.subItemID, Equal<InventoryItem.defaultSubItemID>,
						And<Where<INSiteStatus.siteID, Equal<Current<EntryHeader.siteID>>, Or<Current<EntryHeader.siteID>, IsNull>>>>>,
					LeftJoin<INLocationStatus, On<INLocationStatus.inventoryID, Equal<InventoryItem.inventoryID>,
						And<INLocationStatus.subItemID, Equal<InventoryItem.defaultSubItemID>,
						And<INLocationStatus.siteID, Equal<Current<EntryHeader.siteID>>,
						And<INLocationStatus.locationID, Equal<Current<EntryHeader.locationID>>>>>>>>>>,
					Where<CSAttributeGroup.isActive, Equal<True>,
						And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
						And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
						And<InventoryItem.templateItemID, Equal<Current<EntryHeader.templateItemID>>>>>>,
					Aggregate<GroupBy<InventoryItem.inventoryID, GroupBy<CSAnswers.refNoteID, GroupBy<CSAnswers.attributeID, BqlPlaceholder.A>>>>,
					OrderBy<Asc<InventoryItem.inventoryID, Asc<CSAnswers.refNoteID, Asc<CSAnswers.attributeID>>>>>>
					.Replace<BqlPlaceholder.A>(sumDecimalFields)
					.ToType();

				var view = new PXView(Base, true, BqlCommand.CreateInstance(select));

				using (new PXFieldScope(view,
					typeof(InventoryItem.inventoryID), typeof(CSAnswers.attributeID), typeof(CSAnswers.value),
					typeof(INSiteStatus), typeof(INLocationStatus)))
				{
					return view.SelectMulti().Cast<PXResult<CSAnswers, CSAttributeGroup, InventoryItem>>().ToList();
				}
			}

			protected override InventoryMapValue CreateInventoryMapValue(int? inventoryID, PXResult<CSAnswers, CSAttributeGroup, InventoryItem> result)
			{
				var ret = base.CreateInventoryMapValue(inventoryID, result);
				ret.SiteStatus = result.GetItem<INSiteStatus>();
				ret.LocationStatus = (Header.Current.LocationID != null) ? result.GetItem<INLocationStatus>() : null;
				return ret;
			}

			public PXAction<InventoryItem> viewAllocationDetails;
			[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
			[PXLookupButton]
			public virtual IEnumerable ViewAllocationDetails(PXAdapter adapter)
			{
				if (Matrix.Current != null && Matrix.Current.SelectedColumn < Matrix.Current.InventoryIDs?.Length)
				{
					InventoryItem item = InventoryItem.PK.Find(Base, Matrix.Current.InventoryIDs[(int)Matrix.Current.SelectedColumn]);
					if (item != null)
					{
						var allocationDetEnq = PXGraph.CreateInstance<InventoryAllocDetEnq>();
						allocationDetEnq.Filter.Current = new InventoryAllocDetEnqFilter()
						{
							InventoryID = item.InventoryID,
							SiteID = Header.Current.SiteID,
							LocationID = Header.Current.LocationID
						};
						allocationDetEnq.Filter.Select();
						PXRedirectHelper.TryRedirect(allocationDetEnq, PXRedirectHelper.WindowMode.NewWindow);
					}
				}

				return adapter.Get();
			}
		}

		public MatrixItemsStatusInquiry()
		{
			Save.SetVisible(false);
			Insert.SetVisible(false);
			Delete.SetVisible(false);
			CopyPaste.SetVisible(false);
			Next.SetVisible(false);
			Previous.SetVisible(false);
			First.SetVisible(false);
			Last.SetVisible(false);
			Cancel.SetVisible(false);
		}

		public override bool CanClipboardCopyPaste() => false;
	}
}
