using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
	public class SOPickingWorksheetReview : PXGraph<SOPickingWorksheetReview>
	{
		#region Views
		public PXSetup<SOSetup> setup;
		public PXSetup<SOPickPackShipSetup>.Where<SOPickPackShipSetup.branchID.IsEqual<AccessInfo.branchID.FromCurrent>> pickSetup;
		public SelectFrom<SOPickingWorksheet>.View worksheet;
		public
			SelectFrom<SOPickingWorksheetLine>.
			LeftJoin<INLocation>.On<SOPickingWorksheetLine.FK.Location>.
			Where<SOPickingWorksheetLine.FK.Worksheet.SameAsCurrent>.
			OrderBy<INLocation.pathPriority.Asc, INLocation.locationCD.Asc>.
			View worksheetLines;
		public
			SelectFrom<SOPickingWorksheetLineSplit>.
			LeftJoin<INLocation>.On<SOPickingWorksheetLineSplit.FK.Location>.
			Where<SOPickingWorksheetLineSplit.FK.WorksheetLine.SameAsCurrent.And<SOPickingWorksheetLineSplit.isUnassigned.IsEqual<False>>>.
			OrderBy<INLocation.pathPriority.Asc, INLocation.locationCD.Asc>.
			View worksheetLineSplits;

		public SelectFrom<SOShipment>.Where<SOShipment.FK.Worksheet.SameAsCurrent>.View shipments;
		public SelectFrom<SOPicker>.View shipmentPickers;
		public virtual IEnumerable ShipmentPickers()
		{
			if (worksheet.Current?.WorksheetType == SOPickingWorksheet.worksheetType.Batch)
				return shipmentPickersForBatch.Select().AsEnumerable().RowCast<SOPicker>().ToArray();
			else if (worksheet.Current?.WorksheetType == SOPickingWorksheet.worksheetType.Wave)
				return shipmentPickersForWave.Select().AsEnumerable().RowCast<SOPicker>().ToArray();
			return Array.Empty<SOPicker>();
		}

		public
			SelectFrom<SOPicker>.
			InnerJoin<SOPickerListEntry>.On<SOPickerListEntry.FK.Picker>.
			Where<SOPickerListEntry.FK.Shipment.SameAsCurrent.
				And<SOPicker.FK.Worksheet.SameAsCurrent>>.
			AggregateTo<
				GroupBy<SOPicker.worksheetNbr>,
				GroupBy<SOPicker.pickerNbr>>.
			View shipmentPickersForWave;

		public
			SelectFrom<SOPicker>.
			InnerJoin<SOPickerListEntry>.On<SOPickerListEntry.FK.Picker>.
			InnerJoin<SOShipLineSplit>.On<
				SOShipLineSplit.inventoryID.IsEqual<SOPickerListEntry.inventoryID>.
				And<SOShipLineSplit.subItemID.IsEqual<SOPickerListEntry.subItemID>>.
				And<SOShipLineSplit.siteID.IsEqual<SOPickerListEntry.siteID>>.
				And<SOShipLineSplit.locationID.IsEqual<SOPickerListEntry.locationID>>.
				And<AreSame<SOShipLineSplit.lotSerialNbr, SOPickerListEntry.lotSerialNbr>>>.
			Where<SOPicker.FK.Worksheet.SameAsCurrent.
				And<SOShipLineSplit.FK.Shipment.SameAsCurrent>>.
			AggregateTo<
				GroupBy<SOPicker.worksheetNbr>,
				GroupBy<SOPicker.pickerNbr>>.
			View shipmentPickersForBatch;

		public SelectFrom<SOPicker>.Where<SOPicker.FK.Worksheet.SameAsCurrent>.View pickers;
		public 
			SelectFrom<SOPickerListEntry>.
			InnerJoin<INLocation>.On<SOPickerListEntry.FK.Location>.
			InnerJoin<InventoryItem>.On<SOPickerListEntry.FK.InventoryItem>.
			Where<SOPickerListEntry.FK.Picker.SameAsCurrent>.
			AggregateTo<
				GroupBy<SOPickerListEntry.siteID>,
				GroupBy<SOPickerListEntry.locationID>,
				GroupBy<SOPickerListEntry.inventoryID>,
				GroupBy<SOPickerListEntry.subItemID>,
				GroupBy<SOPickerListEntry.lotSerialNbr>,
				Sum<SOPickerListEntry.qty>,
				Sum<SOPickerListEntry.baseQty>,
				Sum<SOPickerListEntry.pickedQty>,
				Sum<SOPickerListEntry.basePickedQty>>.
			OrderBy<
				INLocation.pathPriority.Asc,
				INLocation.locationCD.Asc,
				InventoryItem.inventoryCD.Asc,
				SOPickerListEntry.lotSerialNbr.Asc>.
			View pickerList;
		public
			SelectFrom<SOPickerToShipmentLink>.
			Where<SOPickerToShipmentLink.FK.Picker.SameAsCurrent>.
			View pickerShipments;
		public
			SelectFrom<SOPickerListEntry>.
			InnerJoin<INLocation>.On<SOPickerListEntry.FK.Location>.
			InnerJoin<InventoryItem>.On<SOPickerListEntry.FK.InventoryItem>.
			Where<
				SOPickerListEntry.FK.Picker.SameAsCurrent.
				And<SOPickerListEntry.shipmentNbr.IsEqual<SOPickerToShipmentLink.shipmentNbr.FromCurrent>>>.
			OrderBy<
				INLocation.pathPriority.Asc,
				INLocation.locationCD.Asc,
				InventoryItem.inventoryCD.Asc,
				SOPickerListEntry.lotSerialNbr.Asc>.
			View pickerListByShipment;
		#endregion

		#region Buttons
		public PXCancel<SOPickingWorksheet> Cancel;
		public PXSave<SOPickingWorksheet> Save;
		public PXDelete<SOPickingWorksheet> Delete;
		public PXFirst<SOPickingWorksheet> First;
		public PXPrevious<SOPickingWorksheet> Prev;
		public PXNext<SOPickingWorksheet> Next;
		public PXLast<SOPickingWorksheet> Last;

		[PXButton, PXUIField(DisplayName = "Allocations")]
		public virtual void showSplits() => worksheetLineSplits.AskExt();
		public PXAction<SOPickingWorksheet> ShowSplits;

		[PXButton, PXUIField(DisplayName = "View Pickers")]
		public virtual void showPickers() => shipmentPickers.AskExt();
		public PXAction<SOPickingWorksheet> ShowPickers;

		[PXButton, PXUIField(DisplayName = "View Shipments")]
		public virtual void showShipments() => pickerShipments.AskExt();
		public PXAction<SOPickingWorksheet> ShowShipments;

		[PXButton, PXUIField(DisplayName = "View Pick List")]
		public virtual void showPickList() => pickerList.AskExt();
		public PXAction<SOPickingWorksheet> ShowPickList;

		[PXButton, PXUIField(DisplayName = "Print Pick Lists")]
		public virtual IEnumerable printPickList(PXAdapter a)
		{
			PrintPickListImpl(a);
			return a.Get();
		}
		public PXAction<SOPickingWorksheet> PrintPickList;

		[PXButton, PXUIField(DisplayName = "Print Packing Slips")]
		public virtual IEnumerable printPackSlips(PXAdapter a)
		{
			PrintPackSlipsImpl(a);
			return a.Get();
		}
		public PXAction<SOPickingWorksheet> PrintPackSlips;
		#endregion

		public SOPickingWorksheetReview()
		{
			worksheet.AllowInsert = false;
		}

		#region DAC overrides
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXParent(typeof(SOShipment.FK.Worksheet), LeaveChildren = true)]
		[PXDBDefault(typeof(SOPickingWorksheet.worksheetNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		protected void _(Events.CacheAttached<SOShipment.currentWorksheetNbr> e) { }
		#endregion

		#region Event handlers
		protected void _(Events.RowSelected<SOPickingWorksheet> e)
		{
			if (e.Row == null) return;

			worksheet.AllowDelete = e.Row.Status.IsIn(SOPickingWorksheet.status.Hold, SOPickingWorksheet.status.Open);
			ShowShipments.SetVisible(e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Wave);
			PrintPackSlips.SetEnabled((e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Batch).Implies(() => shipments.SelectMain().Any(sh => sh.Picked == true)));

			worksheetLineSplits.Cache.AdjustUI().For<SOPickingWorksheetLineSplit.sortingLocationID>(a => a.Visible = e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Batch);
			pickers.Cache.AdjustUI().For<SOPicker.sortingLocationID>(a => a.Visible = e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Batch);
		}
		#endregion

		private void PrintPickListImpl(PXAdapter adapter)
		{
			PXReportRequiredException report = null;
			if (worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Batch)
			{
				// print only pick lists - pack slips will be printed on shipment fulfilment
				var parameters = new Dictionary<string, string>
				{
					[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr
				};
				report = new PXReportRequiredException(parameters, SOReports.PrintPickerPickList);
			}
			else if (worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Wave)
			{
				if (pickSetup.Current.PrintPickListsAndPackSlipsTogether == true)
				{
					// print both pick lists and pack slips at the same time
					foreach (SOPicker picker in pickers.Select())
					{
						pickers.Current = picker;

						var pickListParameters = new Dictionary<string, string>
						{
							[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr,
							[nameof(SOPicker.PickerNbr)] = picker.PickerNbr.ToString()
						};
						if (report == null)
							report = new PXReportRequiredException(pickListParameters, SOReports.PrintPickerPickList);
						else
							report.AddSibling(SOReports.PrintPickerPickList, pickListParameters);

						foreach (SOPickerToShipmentLink shipment in pickerShipments.Select())
						{
							var packSlipParameters = new Dictionary<string, string>
							{
								[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr,
								[nameof(SOShipment.ShipmentNbr)] = shipment.ShipmentNbr
							};
							report.AddSibling(SOReports.PrintPackSlipWave, packSlipParameters);
						}
					}
				}
				else
				{
					// print only pick lists
					var parameters = new Dictionary<string, string>
					{
						[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr
					};
					report = new PXReportRequiredException(parameters, SOReports.PrintPickerPickList);
				}
			}

			if (report != null)
			{
				if (PXAccess.FeatureInstalled<CS.FeaturesSet.deviceHub>())
					PX.SM.SMPrintJobMaint.CreatePrintJobGroup(
						new PX.SM.PrintSettings
						{
							PrintWithDeviceHub = true,
							DefinePrinterManually = true,
							PrinterID = new CR.NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintPickerPickList, Accessinfo.BranchID),
							NumberOfCopies = 1
						}, report, null);

				throw report;
			}
		}

		private void PrintPackSlipsImpl(PXAdapter adapter)
		{
			PXReportRequiredException report = null;
			if (worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Batch)
			{
				foreach (SOShipment shipment in shipments.Select())
				{
					if (shipment.Picked == false)
						continue;

					var packSlipParameters = new Dictionary<string, string>
					{
						[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr,
						[nameof(SOShipment.ShipmentNbr)] = shipment.ShipmentNbr
					};
					if (report == null)
						report = new PXReportRequiredException(packSlipParameters, SOReports.PrintPackSlipBatch);
					else
						report.AddSibling(SOReports.PrintPackSlipBatch, packSlipParameters);
				}

				if (report != null)
				{
					if (PXAccess.FeatureInstalled<CS.FeaturesSet.deviceHub>())
						PX.SM.SMPrintJobMaint.CreatePrintJobGroup(
							new PX.SM.PrintSettings
							{
								PrintWithDeviceHub = true,
								DefinePrinterManually = true,
								PrinterID = new CR.NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintPackSlipBatch, Accessinfo.BranchID),
								NumberOfCopies = 1
							}, report, null);

					throw report;
				}
			}
			else if (worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Wave)
			{
				var packSlipParameters = new Dictionary<string, string>
				{
					[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr
				};
				report = new PXReportRequiredException(packSlipParameters, SOReports.PrintPackSlipWave);

				if (report != null)
				{
					if (PXAccess.FeatureInstalled<CS.FeaturesSet.deviceHub>())
						PX.SM.SMPrintJobMaint.CreatePrintJobGroup(
							new PX.SM.PrintSettings
							{
								PrintWithDeviceHub = true,
								DefinePrinterManually = true,
								PrinterID = new CR.NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintPackSlipWave, Accessinfo.BranchID),
								NumberOfCopies = 1
							}, report, null);

					throw report;
				}
			}
		}

		public SOPickingWorksheetPickListConfirmation PickListConfirmation => FindImplementation<SOPickingWorksheetPickListConfirmation>();
	}

	public class SOPickingWorksheetPickListConfirmation : PXGraphExtension<SOPickingWorksheetReview>
	{
		private Func<int?, bool> isEnterable;

		public override void Initialize()
		{
			isEnterable = Func.Memorize(
				(int? inventoryID) =>
					InventoryItem.PK.Find(Base, inventoryID)
					.With(ii => InventoryItem.FK.LotSerClass.FindParent(Base, ii))
					.With(lsc => lsc.IsUnassigned == true));
		}

		#region DACs
		[PXHidden, Accumulator(BqlTable = typeof(SOPickingWorksheetLine))]
		public class SOPickingWorksheetLineDelta : IBqlTable
		{
			#region WorksheetNbr
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
			public virtual String WorksheetNbr { get; set; }
			public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
			#endregion
			#region LineNbr
			[PXDBInt(IsKey = true)]
			public virtual Int32? LineNbr { get; set; }
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			#endregion

			#region PickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? PickedQty { get; set; }
			public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
			#endregion
			#region BasePickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? BasePickedQty { get; set; }
			public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
			#endregion

			public class AccumulatorAttribute : PXAccumulatorAttribute
			{
				public AccumulatorAttribute() => _SingleRecord = true;

				protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
				{
					if (!base.PrepareInsert(sender, row, columns))
						return false;

					var returnRow = (SOPickingWorksheetLineDelta)row;

					columns.Update<pickedQty>(returnRow.PickedQty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<basePickedQty>(returnRow.BasePickedQty, PXDataFieldAssign.AssignBehavior.Summarize);

					return true;
				}
			}
		}

		[PXHidden, Accumulator(BqlTable = typeof(SOPickingWorksheetLineSplit))]
		public class SOPickingWorksheetLineSplitDelta : IBqlTable
		{
			#region WorksheetNbr
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
			public virtual String WorksheetNbr { get; set; }
			public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
			#endregion
			#region LineNbr
			[PXDBInt(IsKey = true)]
			public virtual Int32? LineNbr { get; set; }
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			#endregion
			#region SplitNbr
			[PXDBInt(IsKey = true)]
			public virtual Int32? SplitNbr { get; set; }
			public abstract class splitNbr : PX.Data.BQL.BqlInt.Field<splitNbr> { }
			#endregion

			#region Qty
			[PXDBDecimal(6)]
			public virtual Decimal? Qty { get; set; }
			public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
			#endregion
			#region BaseQty
			[PXDBDecimal(6)]
			public virtual Decimal? BaseQty { get; set; }
			public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
			#endregion
			#region PickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? PickedQty { get; set; }
			public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
			#endregion
			#region BasePickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? BasePickedQty { get; set; }
			public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
			#endregion
			#region SortingLocationID
			[PXDBInt]
			public virtual int? SortingLocationID { get; set; }
			public abstract class sortingLocationID : PX.Data.BQL.BqlInt.Field<sortingLocationID> { }
			#endregion

			public class AccumulatorAttribute : PXAccumulatorAttribute
			{
				public AccumulatorAttribute() => _SingleRecord = true;

				protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
				{
					if (!base.PrepareInsert(sender, row, columns))
						return false;

					var returnRow = (SOPickingWorksheetLineSplitDelta)row;

					columns.Update<qty>(returnRow.Qty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<baseQty>(returnRow.BaseQty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<pickedQty>(returnRow.PickedQty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<basePickedQty>(returnRow.BasePickedQty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<sortingLocationID>(returnRow.SortingLocationID, PXDataFieldAssign.AssignBehavior.Initialize);
					return true;
				}
			}
		}
		#endregion

		#region Views
		public SelectFrom<SOPickingWorksheetLineDelta>.View WSLineDelta;
		public SelectFrom<SOPickingWorksheetLineSplitDelta>.View WSSplitDelta;

		public SelectFrom<INCartSplit>.View CartSplits;
		public SelectFrom<SOPickListEntryToCartSplitLink>.View CartLinks;
		#endregion

		#region Actions
		[PXButton, PXUIField(DisplayName = "Pick All Shipments")]
		protected virtual void fulfillShipments() => FulfillShipmentsAndConfirmWorksheet(Base.worksheet.Current);
		public PXAction<SOPickingWorksheet> FulfillShipments;
		#endregion

		#region Event Handlers
		protected virtual void _(Events.RowSelected<SOPickingWorksheet> e)
		{
			FulfillShipments.SetEnabled(
				Base.worksheet.Current != null &&
				Base.worksheet.Current.Status.IsIn(SOPickingWorksheet.status.Picking, SOPickingWorksheet.status.Picked) &&
				Base.shipments.SelectMain().Any(sh => sh.Picked == false) &&
				Base.pickers.SelectMain().Any(p => p.Confirmed == true));
		}
		#endregion

		public virtual void ConfirmPickList(SOPicker pickList, int? sortingLocationID)
		{
			if (pickList == null)
				throw new PXArgumentException(nameof(pickList), ErrorMessages.ArgumentNullException);

			Base.Clear();

			Base.worksheet.Current = Base.worksheet.Search<SOPickingWorksheet.worksheetNbr>(pickList.WorksheetNbr).TopFirst.With(ValidateMutability);
			Base.pickers.Current = Base.pickers.Search<SOPicker.worksheetNbr, SOPicker.pickerNbr>(pickList.WorksheetNbr, pickList.PickerNbr).TopFirst.With(ValidateMutability);

			var wsSplits =
				SelectFrom<SOPickingWorksheetLine>.
				InnerJoin<SOPickingWorksheetLineSplit>.On<SOPickingWorksheetLineSplit.FK.WorksheetLine>.
				Where<SOPickingWorksheetLine.FK.Worksheet.SameAsCurrent>.
				View.ReadOnly.Select(Base)
				.AsEnumerable()
				.Select(r => (Line: r.GetItem<SOPickingWorksheetLine>(), Split: r.GetItem<SOPickingWorksheetLineSplit>()))
				.ToArray();

			var entries = new
				SelectFrom<SOPickerListEntry>.
				InnerJoin<INLocation>.On<SOPickerListEntry.FK.Location>.
				InnerJoin<InventoryItem>.On<SOPickerListEntry.FK.InventoryItem>.
				Where<SOPickerListEntry.FK.Picker.SameAsCurrent>.
				OrderBy<
					INLocation.pathPriority.Asc,
					INLocation.locationCD.Asc,
					InventoryItem.inventoryCD.Asc,
					SOPickerListEntry.lotSerialNbr.Asc>.
				View(Base)
				.SelectMain()
				.GroupBy(e => (InventoryID: e.InventoryID, SubItemID: e.SubItemID, OrderLineUOM: e.OrderLineUOM, LocationID: e.LocationID, LotSerialNbr: e.LotSerialNbr))
				.Select(g => (Key: g.Key, PickedQty: g.Sum(e => e.PickedQty ?? 0), ExpireDate: g.Min(e => e.ExpireDate)))
				.Where(e => e.PickedQty > 0)
				.ToArray();

			var pickListEnterableItems = entries
				.Select(e => e.Key.InventoryID)
				.Distinct()
				.Select(itemID => InventoryItem.PK.Find(Base, itemID))
				.Where(item => item.StkItem == true && InventoryItem.FK.LotSerClass.FindParent(Base, item).IsUnassigned)
				.Select(item => item.InventoryID)
				.ToHashSet();

			(var entriesEnterable, var entriesFixed) = entries.DisuniteBy(e => pickListEnterableItems.Contains(e.Key.InventoryID.Value));
			(var wsSplitsEnterable, var wsSplitsFixed) = wsSplits.DisuniteBy(e => pickListEnterableItems.Contains(e.Split.InventoryID.Value));

			UpdateFixedWorksheetSplits(wsSplitsFixed, entriesFixed, sortingLocationID);
			UpdateEnterableWorksheetSplits(wsSplitsEnterable, entriesEnterable, sortingLocationID);

			Base.pickers.Current.Confirmed = true;
			if (sortingLocationID != null)
				Base.pickers.Current.SortingLocationID = sortingLocationID;
			Base.pickers.UpdateCurrent();

			if (sortingLocationID != null)
				RemoveItemsFromPickerCart(Base.pickers.Current);

			Base.Save.Press();
		}

		protected virtual void RemoveItemsFromPickerCart(SOPicker picker)
		{
			var links = SOPickListEntryToCartSplitLink.FK.Picker.SelectChildren(Base, picker);
			foreach (var link in links)
			{
				decimal linkQty = link.Qty.Value;
				CartLinks.Delete(link);

				var cartSplit = INCartSplit.PK.Find(Base, link.SiteID, link.CartID, link.CartSplitLineNbr);
				if (cartSplit != null)
				{
					decimal restQty = cartSplit.Qty.Value - linkQty;
					if (restQty <= 0)
						CartSplits.Delete(cartSplit);
					else
					{
						CartSplits.Cache.SetValueExt<INCartSplit.qty>(cartSplit, restQty);
						CartSplits.Update(cartSplit);
					}
				}
			}
		}

		protected virtual void UpdateFixedWorksheetSplits(
			IEnumerable<(SOPickingWorksheetLine Line, SOPickingWorksheetLineSplit Split)> wsSplits,
			IEnumerable<((int? InventoryID, int? SubItemID, string OrderLineUOM, int? LocationID, string LotSerialNbr) Key, decimal PickedQty, DateTime? ExpireDate)> entries,
			int? sortingLocationID)
		{
			var affectedFixedLines =
				(from s in wsSplits
				 join e in entries
					 on (s.Line.InventoryID, s.Line.SubItemID, s.Line.UOM, s.Split.LocationID, s.Split.LotSerialNbr)
					 equals (e.Key.InventoryID, e.Key.SubItemID, e.Key.OrderLineUOM, e.Key.LocationID, e.Key.LotSerialNbr)
				 select (Line: s.Line, Split: s.Split, Entry: e))
				 .GroupBy(t => t.Line.LineNbr)
				 .ToArray();

			foreach (var splitsByLine in affectedFixedLines)
			{
				var line = splitsByLine.First().Line;
				var lineBasePickedQty = splitsByLine.Sum(s => s.Entry.PickedQty);

				var lineDelta = PropertyTransfer.Transfer(line, new SOPickingWorksheetLineDelta());
				lineDelta.BasePickedQty = lineBasePickedQty;
				lineDelta.PickedQty = INUnitAttribute.ConvertFromBase(WSLineDelta.Cache, line.InventoryID, line.UOM, lineBasePickedQty, INPrecision.NOROUND);
				WSLineDelta.Insert(lineDelta);

				foreach (var (Line, Split, Entry) in splitsByLine)
				{
					var splitDelta = PropertyTransfer.Transfer(Split, new SOPickingWorksheetLineSplitDelta());
					splitDelta.BasePickedQty = Entry.PickedQty;
					splitDelta.PickedQty = Entry.PickedQty;
					splitDelta.BaseQty = 0;
					splitDelta.Qty = 0;
					splitDelta.SortingLocationID = sortingLocationID;
					WSSplitDelta.Insert(splitDelta);
				}
			}
		}

		protected virtual void UpdateEnterableWorksheetSplits(
			IEnumerable<(SOPickingWorksheetLine Line, SOPickingWorksheetLineSplit Split)> wsSplits,
			IEnumerable<((int? InventoryID, int? SubItemID, string OrderLineUOM, int? LocationID, string LotSerialNbr) Key, decimal PickedQty, DateTime? ExpireDate)> entries,
			int? sortingLocationID)
		{
			var wsSplitsEnterableQueue = wsSplits
				.Where(r => r.Split.IsUnassigned == true)
				.OrderBy(r => r.Split.InventoryID)
				.ThenBy(r => r.Split.SubItemID)
				.ThenBy(r => r.Line.UOM)
				.ThenBy(r => r.Split.LocationID)
				.ToQueue();

			var entriesEnterableQueue = entries
				.OrderBy(r => r.Key.InventoryID)
				.ThenBy(r => r.Key.SubItemID)
				.ThenBy(r => r.Key.OrderLineUOM)
				.ThenBy(r => r.Key.LocationID)
				.ToQueue();

			while (wsSplitsEnterableQueue.Count > 0 && entriesEnterableQueue.Count > 0)
			{
				(var line, var split) = wsSplitsEnterableQueue.Peek();
				(var entry, decimal pickedQty, DateTime? expireDate) = entriesEnterableQueue.Peek();

				var entryKey =
				(
					InventoryID: entry.InventoryID,
					SubItemID: entry.SubItemID,
					UOM: entry.OrderLineUOM,
					LocationID: entry.LocationID
				);
				var splitKey =
				(
					InventoryID: split.InventoryID,
					SubItemID: split.SubItemID,
					UOM: line.UOM,
					LocationID: split.LocationID
				);

				if (!entryKey.Equals(splitKey))
				{
					bool bypassSubItemID = entryKey.InventoryID != splitKey.InventoryID;
					bool bypassUOM = bypassSubItemID || entryKey.SubItemID != splitKey.SubItemID;
					bool bypassLocationID = bypassUOM || entryKey.UOM != splitKey.UOM;

					if (entryKey.CompareTo(splitKey) > 0)
					{
						int dequeueCount = wsSplitsEnterableQueue
							.TakeWhile(r =>
								(r.Split.InventoryID == split.InventoryID) &&
								(bypassSubItemID || r.Split.SubItemID == split.SubItemID) &&
								(bypassUOM || r.Line.UOM == line.UOM) &&
								(bypassLocationID || r.Split.LocationID == split.LocationID))
							.Count();
						wsSplitsEnterableQueue.Dequeue(dequeueCount).Consume();
					}
					else
					{
						int dequeueCount = entriesEnterableQueue
							.TakeWhile(e =>
								(e.Key.InventoryID == entry.InventoryID) &&
								(bypassSubItemID || e.Key.SubItemID == entry.SubItemID) &&
								(bypassUOM || e.Key.OrderLineUOM == entry.OrderLineUOM) &&
								(bypassLocationID || e.Key.LocationID == entry.LocationID))
							.Count();
						entriesEnterableQueue.Dequeue(dequeueCount).Consume();
					}

					continue;
				}
				else
				{
					do
					{
						decimal actualQty = Math.Min(split.Qty.Value, pickedQty);
						pickedQty -= actualQty;
						split.Qty -= actualQty;

						var lineDelta = PropertyTransfer.Transfer(line, new SOPickingWorksheetLineDelta());
						lineDelta.BasePickedQty = actualQty;
						lineDelta.PickedQty = INUnitAttribute.ConvertFromBase(WSLineDelta.Cache, line.InventoryID, line.UOM, actualQty, INPrecision.NOROUND);
						var existingLine = WSLineDelta.Locate(lineDelta);
						if (existingLine == null)
						{
							WSLineDelta.Insert(lineDelta);
						}
						else
						{
							existingLine.BasePickedQty += lineDelta.BasePickedQty;
							existingLine.PickedQty += lineDelta.PickedQty;
						}

						var unassignedDelta = PropertyTransfer.Transfer(split, new SOPickingWorksheetLineSplitDelta());
						unassignedDelta.BaseQty = -actualQty;
						unassignedDelta.Qty = -actualQty;
						var existingUnassignedDelta = WSSplitDelta.Locate(unassignedDelta);
						if (existingUnassignedDelta == null)
						{
							WSSplitDelta.Insert(unassignedDelta);
						}
						else
						{
							existingUnassignedDelta.BaseQty -= actualQty;
							existingUnassignedDelta.Qty -= actualQty;
						}

						Base.worksheetLines.Current = line;
						var assignedSplit = PropertyTransfer.Transfer(split, new SOPickingWorksheetLineSplit());
						assignedSplit.SplitNbr = null;
						assignedSplit.LotSerialNbr = entry.LotSerialNbr;
						assignedSplit.ExpireDate = expireDate;
						assignedSplit.PickedQty = actualQty;
						assignedSplit.Qty = actualQty;
						assignedSplit.BasePickedQty = actualQty;
						assignedSplit.BaseQty = actualQty;
						assignedSplit.SortingLocationID = sortingLocationID;
						assignedSplit.IsUnassigned = false;
						assignedSplit = Base.worksheetLineSplits.Insert(assignedSplit);

						if (pickedQty == 0)
						{
							entriesEnterableQueue.Dequeue();
							if (entriesEnterableQueue.Count == 0)
								break;

							(entry, pickedQty, expireDate) = entriesEnterableQueue.Peek();
							entryKey =
							(
								InventoryID: entry.InventoryID,
								SubItemID: entry.SubItemID,
								UOM: entry.OrderLineUOM,
								LocationID: entry.LocationID
							);
						}
						
						if (split.Qty == 0)
						{
							wsSplitsEnterableQueue.Dequeue();
							if (wsSplitsEnterableQueue.Count == 0)
								break;

							(line, split) = wsSplitsEnterableQueue.Peek();
							splitKey =
							(
								InventoryID: split.InventoryID,
								SubItemID: split.SubItemID,
								UOM: line.UOM,
								LocationID: split.LocationID
							);
						}
					}
					while (entryKey.Equals(splitKey));
				}
			}
		}

		public virtual IEnumerable<string> TryFulfillShipments(SOPickingWorksheet worksheet)
		{
			if (worksheet == null)
				throw new PXArgumentException(nameof(worksheet), ErrorMessages.ArgumentNullException);

			return worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Wave
				? FulfillShipmentsWave(worksheet)
				: FulfillShipmentsBatch(worksheet);
		}


		protected virtual IEnumerable<string> FulfillShipmentsBatch(SOPickingWorksheet worksheet)
		{
			Base.Clear();

			Base.worksheet.Current = Base.worksheet.Search<SOPickingWorksheet.worksheetNbr>(worksheet.WorksheetNbr).TopFirst.With(ValidateMutability);

			var wsSplits =
				SelectFrom<SOPickingWorksheetLine>.
				InnerJoin<SOPickingWorksheetLineSplit>.On<SOPickingWorksheetLineSplit.FK.WorksheetLine>.
				Where<
					SOPickingWorksheetLine.FK.Worksheet.SameAsCurrent.
					And<SOPickingWorksheetLineSplit.isUnassigned.IsEqual<False>>>.
				View.Select(Base)
				.AsEnumerable()
				.Select(r =>
				(
					Line: r.GetItem<SOPickingWorksheetLine>(),
					Split: r.GetItem<SOPickingWorksheetLineSplit>()
				))
				.ToArray();

			Func<int?, bool> isEnterableDate = Func.Memorize(
				(int? inventoryID) =>
					InventoryItem.PK.Find(Base, inventoryID)
					.With(ii => InventoryItem.FK.LotSerClass.FindParent(Base, ii))
					.With(lsc => lsc.LotSerTrackExpiration == true));
			var expirationDates = wsSplits
				.Where(s => isEnterableDate(s.Line.InventoryID))
				.ToDictionary(s => (s.Split.InventoryID, s.Split.LotSerialNbr), s => s.Split.ExpireDate.Value)
				.AsReadOnly();

			var baseAvailability =
				wsSplits
				.GroupBy(s =>
				(
					InventoryID: s.Line.InventoryID,
					SubItemID: s.Line.SubItemID,
					UOM: s.Line.UOM,
					LocationID: s.Split.LocationID,
					SortLocationID: s.Split.SortingLocationID,
					LotSerialNbr: s.Split.LotSerialNbr
				))
				.ToDictionary(g => g.Key, g => g.Sum(s => s.Split.PickedQty ?? 0));

			var shipmentAssignedSplits =
				SelectFrom<SOShipment>.
				InnerJoin<SOShipLine>.On<SOShipLine.FK.Shipment>.
				InnerJoin<SOShipLineSplit>.On<SOShipLineSplit.FK.ShipLine>.
				LeftJoin<INTranSplit>.On<
					INTranSplit.FK.ShipLineSplit.
					And<INTranSplit.locationID.IsNotEqual<INTranSplit.toLocationID>>>.
				Where<SOShipment.FK.Worksheet.SameAsCurrent>.
				View.Select(Base)
				.AsEnumerable()
				.Select(r =>
				(
					Shipment: r.GetItem<SOShipment>(),
					Line: r.GetItem<SOShipLine>(),
					Split: r.GetItem<SOShipLineSplit>(),
					TransferSplit: r.GetItem<INTranSplit>()
				))
				.ToArray();

			var shipmentUnassignedSplits =
				SelectFrom<SOShipment>.
				InnerJoin<SOShipLine>.On<SOShipLine.FK.Shipment>.
				InnerJoin<Unassigned.SOShipLineSplit>.On<Unassigned.SOShipLineSplit.FK.ShipLine>.
				LeftJoin<INTranSplit>.On<True.IsEqual<False>>.
				Where<SOShipment.FK.Worksheet.SameAsCurrent>.
				View.Select(Base)
				.AsEnumerable()
				.Select(r =>
				(
					Shipment: r.GetItem<SOShipment>(),
					Line: r.GetItem<SOShipLine>(),
					Split: PropertyTransfer.Transfer(r.GetItem<Unassigned.SOShipLineSplit>(), new SOShipLineSplit()), // make of assigned-type
					TransferSplit: r.GetItem<INTranSplit>()
				))
				.ToArray();

			var shipmentSplits = shipmentAssignedSplits
				.Concat(shipmentUnassignedSplits)
				.OrderBy(r => r.Shipment.ShipDate)
				.ThenBy(r => r.Shipment.LineTotal)
				.ThenBy(r => r.Shipment.ShipmentNbr)
				.ToArray();

			(var pickedShipmentSplits, var vacantShipmentSplits) = shipmentSplits.DisuniteBy(s => s.Shipment.Picked == true).With(pair => (pair.Affirmatives.ToArray(), pair.Negatives.ToArray()));
			if (pickedShipmentSplits.Length + vacantShipmentSplits.Length != shipmentSplits.Length || pickedShipmentSplits.IntersectBy(vacantShipmentSplits, s => s.Shipment.ShipmentNbr).Count() != 0)
				throw new PXInvalidOperationException();

			var alreadyPickedQty =
				pickedShipmentSplits
				.GroupBy(s =>
				(
					InventoryID: s.Line.InventoryID,
					SubItemID: s.Line.SubItemID,
					UOM: s.Line.UOM,
					LocationID: s.TransferSplit.LocationID,
					SortLocationID: s.Split.LocationID,
					LotSerialNbr: s.Split.LotSerialNbr
				))
				.ToDictionary(g => g.Key, g => g.Sum(s => s.Split.PickedQty ?? 0));

			if (alreadyPickedQty.Any())
			{
				foreach (var deduction in alreadyPickedQty)
					baseAvailability[deduction.Key] -= deduction.Value;
				baseAvailability.RemoveRange(baseAvailability.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key).ToArray());
			}
			var availability = baseAvailability
				.GroupBy(kvp =>
				(
					InventoryID: kvp.Key.InventoryID,
					SubItemID: kvp.Key.SubItemID,
					UOM: kvp.Key.UOM,
					LocationID: kvp.Key.LocationID,
					LotSerialNbr: kvp.Key.LotSerialNbr
				))
				.Where(g => g.Any(e => e.Value > 0))
				.ToDictionary(
					g => g.Key,
					g => g.Select(e => (SortLocationID: e.Key.SortLocationID, PickedQty: e.Value)).OrderByDescending(e => e.PickedQty).ToList());

			var itemVariety = availability.Select(kvp => kvp.Key.InventoryID).ToHashSet();
			var locationVariety = availability.Select(kvp => kvp.Key.LocationID).ToHashSet();
			var uomVariety = availability.Select(kvp => kvp.Key.UOM).ToHashSet();

			var shipmentsApplicableByAllVarieties =
				vacantShipmentSplits
				.GroupBy(s => s.Shipment.ShipmentNbr)
				.Where(g => g.All(s =>
					itemVariety.Contains(s.Split.InventoryID) &&
					locationVariety.Contains(s.Split.LocationID) &&
					uomVariety.Contains(s.Line.UOM)))
				.Select(splitsByShipment =>
				(
					ShipmentNbr: splitsByShipment.Key,
					SiteID: splitsByShipment.First().Shipment.SiteID,
					Demands: splitsByShipment
						.GroupBy(s =>
						(
							InventoryID: s.Line.InventoryID,
							SubItemID: s.Line.SubItemID,
							UOM: s.Line.UOM,
							LocationID: s.Split.LocationID,
							LotSerialNbr: s.Split.LotSerialNbr
						))
						.ToDictionary(g => g.Key, g => g.Sum(s => s.Split.Qty ?? 0)),
					Details: splitsByShipment.Select(d => (Line: d.Line, Split: d.Split)).ToArray()
				))
				.ToArray();

			var transferGraph = Lazy.By(() => PXGraph.CreateInstance<INTransferEntry>());
			var shipmentGraph = Lazy.By(() => PXGraph.CreateInstance<SOShipmentEntry>());
			var fulfilledShipments = new List<string>();
			using (var ts = new PXTransactionScope())
			{
				foreach (var shipment in shipmentsApplicableByAllVarieties)
				{
					if (shipment.Demands.All(demand => isEnterable(demand.Key.InventoryID)
						? availability
							.Where(kvp =>
								kvp.Key.InventoryID == demand.Key.InventoryID &&
								kvp.Key.SubItemID == demand.Key.SubItemID &&
								kvp.Key.UOM == demand.Key.UOM &&
								kvp.Key.LocationID == demand.Key.LocationID)
							.Sum(kvp => kvp.Value.Sum(s => s.PickedQty)) >= demand.Value
						: availability.ContainsKey(demand.Key) && availability[demand.Key].Sum(s => s.PickedQty) >= demand.Value))
					{
						HoldShipment(shipmentGraph.Value, shipment.ShipmentNbr);

						var soSplitToTransferSplit = CreateTransferSplits(availability, shipment.Details);

						CreateTransferToStorageLocations(transferGraph.Value, shipment.SiteID, soSplitToTransferSplit.Select(s => s.tranSplit));

						var assignedSplits = MakeAllSplitsAssigned(shipmentGraph.Value, shipment.ShipmentNbr, availability, shipment.Details, expirationDates);

						foreach (var assigned in assignedSplits)
							DecreaseAvailability(availability, assigned);

						PutShipmentOnStorageLocations(
							shipmentGraph.Value,
							shipment.ShipmentNbr,
							(from asp in assignedSplits
							join tsp in soSplitToTransferSplit on asp.Split.LineNbr equals tsp.soSplit.LineNbr
							select (Split: asp.Split, SortLocationID: tsp.tranSplit.ToLocationID))
							.Distinct(r => r.Split.SplitLineNbr));

						fulfilledShipments.Add(shipment.ShipmentNbr);
					}
				}
				ts.Complete();
			}

			return fulfilledShipments;
		}

		protected virtual IEnumerable<(SOShipLine Line, SOShipLineSplit Split)>
			MakeAllSplitsAssigned(
				SOShipmentEntry shipmentEntry,
				string shipmentNbr,
				IReadOnlyDictionary<
					(int? InventoryID, int? SubItemID, string UOM, int? LocationID, string LotSerialNbr),
					List<(int? SortLocationID, decimal PickedQty)>
				> availability,
				IEnumerable<(SOShipLine Line, SOShipLineSplit Split)> details,
				IReadOnlyDictionary<(int? InventoryID, string LotSerialNbr), DateTime> expirationDates)
		{
			(var enterableSplits, var fixedSplits) = details.DisuniteBy(r => r.Split.IsUnassigned == true);
			if (enterableSplits.Any() == false)
				return fixedSplits;

			shipmentEntry.Clear();
			shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);
			PXSelectBase<SOShipLine> shLines = shipmentEntry.Transactions;
			PXSelectBase<SOShipLineSplit> shSplits = shipmentEntry.splits;

			var enterableItems = enterableSplits.Select(s => s.Line.InventoryID).ToHashSet();
			var availabilityQueue =
				availability
				.Where(a => enterableItems.Contains(a.Key.InventoryID))
				.SelectMany(a => a.Value.Select(e =>
				(
					InventoryID: a.Key.InventoryID,
					SubItemID: a.Key.SubItemID,
					UOM: a.Key.UOM,
					LocationID: a.Key.LocationID,
					LotSerialNbr: a.Key.LotSerialNbr,
					PickedQty: e.PickedQty,
					Key: (a.Key.InventoryID, a.Key.SubItemID, a.Key.UOM, a.Key.LocationID)
				)))
				.OrderBy(e => e.InventoryID)
				.ThenBy(e => e.SubItemID)
				.ThenBy(e => e.UOM)
				.ThenBy(e => e.LocationID)
				.ThenBy(e => e.LotSerialNbr)
				.ToQueue();

			enterableSplits = enterableSplits
				.OrderBy(e => e.Line.InventoryID)
				.ThenBy(e => e.Line.SubItemID)
				.ThenBy(e => e.Line.UOM)
				.ThenBy(e => e.Split.LocationID)
				.ThenBy(e => e.Line.LineNbr)
				.ToArray();

			var currentEntry = availabilityQueue.Dequeue();
			decimal availableQty = currentEntry.PickedQty;

			var assignedSplits = new List<(SOShipLine Line, SOShipLineSplit Split)>();
			foreach (var detail in enterableSplits)
			{
				shLines.Current = shLines.Search<SOShipLine.shipmentNbr, SOShipLine.lineNbr>(shipmentNbr, detail.Line.LineNbr);
				decimal unassignedQty = detail.Split.Qty.Value;
				var detailKey = (detail.Line.InventoryID, detail.Line.SubItemID, detail.Line.UOM, detail.Split.LocationID);

				while (unassignedQty > 0 && availableQty > 0 && detailKey.Equals(currentEntry.Key))
				{
					var qty = Math.Min(unassignedQty, availableQty);
					unassignedQty -= qty;
					availableQty -= qty;

					var newSplit = shSplits.Insert();
					newSplit.Qty = qty;
					newSplit.BaseQty = qty;
					newSplit.LocationID = currentEntry.LocationID;
					newSplit.LotSerialNbr = currentEntry.LotSerialNbr;
					if (expirationDates.TryGetValue((currentEntry.InventoryID, currentEntry.LotSerialNbr), out DateTime expireDate))
						newSplit.ExpireDate = expireDate;
					newSplit = shSplits.Update(newSplit);
					assignedSplits.Add((shLines.Current, newSplit));

					if (availableQty == 0)
					{
						if (availabilityQueue.Count == 0)
							break;

						currentEntry = availabilityQueue.Dequeue();
						availableQty = currentEntry.PickedQty;
					}
				}
			}

			shipmentEntry.Save.Press();

			return fixedSplits
				.Concat(assignedSplits)
				.OrderBy(r => r.Line.LineNbr)
				.ThenBy(r => r.Split.SplitLineNbr)
				.ToArray();
		}

		protected virtual IEnumerable<(SOShipLineSplit soSplit, INTranSplit tranSplit)>
			CreateTransferSplits(
				IReadOnlyDictionary<
					(int? InventoryID, int? SubItemID, string UOM, int? LocationID, string LotSerialNbr),
					List<(int? SortLocationID, decimal PickedQty)>
				> availability,
				IEnumerable<(SOShipLine Line, SOShipLineSplit Split)> details)
		{
			var availabilityForTransfer = availability
				.Select(kvp =>
				(
					Key:
					(
						InventoryID: kvp.Key.InventoryID,
						SubItemID: kvp.Key.SubItemID,
						UOM: kvp.Key.UOM,
						LocationID: kvp.Key.LocationID,
						LotSerialNbr: isEnterable(kvp.Key.InventoryID) ? "" : kvp.Key.LotSerialNbr
					),
					Value: kvp.Value
				))
				.GroupBy(r => r.Key)
				.ToDictionary(
					r => r.Key,
					r => r.Aggregate(
								Enumerable.Empty<(int? SortLocationID, decimal PickedQty)>(),
								(acc, elem) => acc.Concat(elem.Value))
							.GroupBy(t => t.SortLocationID, t => t.PickedQty)
							.Select(t => (SortLocationID: t.Key, PickedQty: t.Sum()))
							.ToList());

			var soSplitToTransferSplit = new List<(SOShipLineSplit soSplit, INTranSplit tranSplit)>();
			foreach (var detail in details)
			{
				(int? SortLocationID, decimal PickedQty) available = DecreaseAvailability(availabilityForTransfer, detail);

				var tranSplit = new INTranSplit
				{
					SiteID = detail.Split.SiteID,
					ToSiteID = detail.Split.SiteID,
					LocationID = detail.Split.LocationID,
					ToLocationID = available.SortLocationID,

					InventoryID = detail.Split.InventoryID,
					SubItemID = detail.Split.SubItemID,
					LotSerialNbr = detail.Split.LotSerialNbr,
					ExpireDate = detail.Split.ExpireDate,

					Qty = detail.Split.Qty,
					UOM = detail.Split.UOM,
					BaseQty = detail.Split.BaseQty,

					ShipmentNbr = detail.Split.ShipmentNbr,
					ShipmentLineNbr = detail.Split.LineNbr,
					ShipmentLineSplitNbr = detail.Split.SplitLineNbr
				};

				soSplitToTransferSplit.Add((detail.Split, tranSplit));
			}

			return soSplitToTransferSplit;
		}

		protected virtual (int? SortLocationID, decimal PickedQty) DecreaseAvailability(
			IReadOnlyDictionary<
				(int? InventoryID, int? SubItemID, string UOM, int? LocationID, string LotSerialNbr),
				List<(int? SortLocationID, decimal PickedQty)>
			> availability,
			(SOShipLine Line, SOShipLineSplit Split) detail)
		{
			var detailKey =
			(
				InventoryID: detail.Line.InventoryID,
				SubItemID: detail.Line.SubItemID,
				UOM: detail.Line.UOM,
				LocationID: detail.Split.LocationID,
				LotSerialNbr: detail.Split.LotSerialNbr
			);

			var available = availability[detailKey].First(a => a.PickedQty >= detail.Split.BaseQty);
			if (available.PickedQty > detail.Split.BaseQty)
			{
				availability[detailKey].Insert(
					index: availability[detailKey]
						.TakeWhile(a => a.PickedQty < available.PickedQty - detail.Split.BaseQty.Value)
						.Count(),
					item:
					(
						SortLocationID: available.SortLocationID,
						PickedQty: available.PickedQty - detail.Split.BaseQty.Value
					));
			}
			availability[detailKey].Remove(available);
			return available;
		}

		protected virtual void HoldShipment(SOShipmentEntry shipmentEntry, string shipmentNbr)
		{
			shipmentEntry.Clear();
			shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);
			shipmentEntry.Document.SetValueExt<SOShipment.hold>(shipmentEntry.Document.Current, true);
			shipmentEntry.Document.UpdateCurrent();
			shipmentEntry.Save.Press();
		}

		protected virtual void CreateTransferToStorageLocations(INTransferEntry transferEntry, int? siteID, IEnumerable<INTranSplit> tranSplits)
		{
			transferEntry.Clear();

			transferEntry.insetup.Current.RequireControlTotal = false;
			transferEntry.transfer.With(_ => _.Insert() ?? _.Insert());
			transferEntry.transfer.SetValueExt<INRegister.siteID>(transferEntry.transfer.Current, siteID);
			transferEntry.transfer.SetValueExt<INRegister.toSiteID>(transferEntry.transfer.Current, siteID);
			transferEntry.transfer.UpdateCurrent();
			foreach (var protoTranSplit in tranSplits)
			{
				INTran tran = transferEntry.transactions.With(_ => _.Insert() ?? _.Insert());
				tran.InventoryID = protoTranSplit.InventoryID;
				tran.SubItemID = protoTranSplit.SubItemID;
				tran.LotSerialNbr = protoTranSplit.LotSerialNbr;
				tran.ExpireDate = protoTranSplit.ExpireDate;
				tran.UOM = protoTranSplit.UOM;
				tran.SiteID = protoTranSplit.SiteID;
				tran.LocationID = protoTranSplit.LocationID;
				tran.ToSiteID = protoTranSplit.SiteID;
				tran.ToLocationID = protoTranSplit.ToLocationID;
				tran = transferEntry.transactions.Update(tran);
				INTranSplit tranSplit = transferEntry.splits.Search<INTranSplit.lineNbr>(tran.LineNbr);
				if (tranSplit == null)
				{
					tranSplit = transferEntry.splits.With(_ => _.Insert() ?? _.Insert());
					tranSplit.LotSerialNbr = protoTranSplit.LotSerialNbr;
					tranSplit.ExpireDate = protoTranSplit.ExpireDate;
					tranSplit.ToSiteID = protoTranSplit.SiteID;
					tranSplit.ToLocationID = protoTranSplit.ToLocationID;
				}
				tranSplit.ShipmentNbr = protoTranSplit.ShipmentNbr;
				tranSplit.ShipmentLineNbr = protoTranSplit.ShipmentLineNbr;
				tranSplit.ShipmentLineSplitNbr = protoTranSplit.ShipmentLineSplitNbr;

				tranSplit.Qty = protoTranSplit.Qty;
				tranSplit = transferEntry.splits.Update(tranSplit);
			}
			transferEntry.transfer.SetValueExt<INRegister.hold>(transferEntry.transfer.Current, false);
			transferEntry.release.Press();
		}

		protected virtual void PutShipmentOnStorageLocations(SOShipmentEntry shipmentEntry, string shipmentNbr, IEnumerable<(SOShipLineSplit Split, int? SortLocationID)> soSplitToSortLocation)
		{
			shipmentEntry.Clear();

			shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);
			PXSelectBase<SOShipLine> shLines = shipmentEntry.Transactions;
			PXSelectBase<SOShipLineSplit> shSplits = shipmentEntry.splits;

			decimal docQty = 0;
			foreach (var line in soSplitToSortLocation.GroupBy(d => d.Split.LineNbr))
			{
				shLines.Current = shLines.Search<SOShipLine.shipmentNbr, SOShipLine.lineNbr>(shipmentNbr, line.Key);

				decimal lineQty = 0;
				foreach (var split in line)
				{
					shSplits.Current = shSplits.Search<SOShipLineSplit.shipmentNbr, SOShipLineSplit.lineNbr, SOShipLineSplit.splitLineNbr>(shipmentNbr, line.Key, split.Split.SplitLineNbr);
					shSplits.SetValueExt<SOShipLineSplit.locationID>(shSplits.Current, split.SortLocationID);
					shSplits.SetValueExt<SOShipLineSplit.pickedQty>(shSplits.Current, shSplits.Current.Qty);
					shSplits.SetValueExt<SOShipLineSplit.basePickedQty>(shSplits.Current, shSplits.Current.BaseQty);
					shSplits.UpdateCurrent();

					lineQty += shSplits.Current.BaseQty.Value;
				}
				shLines.Current.BasePickedQty = lineQty;
				shLines.Current.PickedQty = INUnitAttribute.ConvertFromBase(shLines.Cache, shLines.Current.InventoryID, shLines.Current.UOM, lineQty, INPrecision.NOROUND);
				shLines.Cache.MarkUpdated(shLines.Current);
				docQty += shLines.Current.PickedQty.Value;
			}
			shipmentEntry.Document.SetValueExt<SOShipment.hold>(shipmentEntry.Document.Current, false);
			shipmentEntry.Document.SetValueExt<SOShipment.picked>(shipmentEntry.Document.Current, true);
			shipmentEntry.Document.SetValueExt<SOShipment.pickedQty>(shipmentEntry.Document.Current, docQty);
			shipmentEntry.Document.SetValueExt<SOShipment.pickedViaWorksheet>(shipmentEntry.Document.Current, true);
			shipmentEntry.Document.UpdateCurrent();
			shipmentEntry.Save.Press();
		}


		protected virtual IEnumerable<string> FulfillShipmentsWave(SOPickingWorksheet worksheet)
		{
			Base.Clear();

			Base.worksheet.Current = Base.worksheet.Search<SOPickingWorksheet.worksheetNbr>(worksheet.WorksheetNbr).TopFirst.With(ValidateMutability);

			var recentlyPickedShipments =
				SelectFrom<SOShipment>.
				InnerJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Shipment>.
				InnerJoin<SOPicker>.On<SOPickerToShipmentLink.FK.Picker>.
				Where<SOPicker.confirmed.IsEqual<True>>.
				View.Select(Base)
				.AsEnumerable()
				.Cast<PXResult<SOShipment, SOPickerToShipmentLink, SOPicker>>()
				.GroupBy(
					row => (SOPicker)row,
					row => (SOShipment)row,
					Base.pickers.Cache.GetComparer())
				.Where(g => g.All(sh => sh.Picked == false))
				.SelectMany(g => g)
				.ToArray();

			var shipmentGraph = Lazy.By(() => PXGraph.CreateInstance<SOShipmentEntry>());
			var fulfilledShipments = new List<string>();
			using (var ts = new PXTransactionScope())
			{
				foreach (var shipment in recentlyPickedShipments)
				{
					MarkShipmentPicked(shipmentGraph.Value, shipment.ShipmentNbr);
					fulfilledShipments.Add(shipment.ShipmentNbr);
				}
				ts.Complete();
			}

			return fulfilledShipments;
		}

		protected virtual void MarkShipmentPicked(SOShipmentEntry shipmentEntry, string shipmentNbr)
		{
			shipmentEntry.Clear();

			var picker =
				SelectFrom<SOPicker>.
				InnerJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Picker>.
				Where<
					SOPicker.worksheetNbr.IsEqual<@P.AsString>.
					And<SOPickerToShipmentLink.shipmentNbr.IsEqual<@P.AsString>>>.
				View.Select(shipmentEntry, Base.worksheet.Current.WorksheetNbr, shipmentNbr).TopFirst;

			var pickerSplits =
				SelectFrom<SOPickerListEntry>.
				LeftJoin<SOPickListEntryToCartSplitLink>.On<SOPickListEntryToCartSplitLink.FK.PickListEntry>.
				Where<
					SOPickerListEntry.worksheetNbr.IsEqual<@P.AsString>.
					And<SOPickerListEntry.pickerNbr.IsEqual<@P.AsInt>>.
					And<SOPickerListEntry.shipmentNbr.IsEqual<@P.AsString>>>.
				View
				.Select(shipmentEntry, picker.WorksheetNbr, picker.PickerNbr, shipmentNbr)
				.AsEnumerable()
				.Select(r => 
				(
					Entry: r.GetItem<SOPickerListEntry>(),
					Link: r.GetItem<SOPickListEntryToCartSplitLink>()
				))
				.ToArray();

			var availability = pickerSplits
				.GroupBy(e =>
				(
					InventoryID: e.Entry.InventoryID,
					SubItemID: e.Entry.SubItemID,
					OrderLineUOM: e.Entry.OrderLineUOM,
					LocationID: e.Entry.LocationID,
					LotSerialNbr: e.Entry.LotSerialNbr
				))
				.Select(g =>
				(
					Key: g.Key,
					PickedQty: g.Sum(e => e.Entry.PickedQty ?? 0),
					Links: g.Select(e => e.Link).WhereNotNull().ToList().AsReadOnly()
				))
				.ToDictionary(r => r.Key, r => (r.PickedQty, r.Links));

			shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);
			PXSelectBase<SOShipLine> shLines = shipmentEntry.Transactions;
			PXSelectBase<SOShipLineSplit> shSplits = shipmentEntry.splits;
			PXSelectBase<Unassigned.SOShipLineSplit> shSplitsUnassigned = shipmentEntry.unassignedSplits;

			decimal docQty = 0;
			foreach (SOShipLine line in shLines.Select())
			{
				shLines.Current = shLines.Search<SOShipLine.shipmentNbr, SOShipLine.lineNbr>(shipmentNbr, line.LineNbr);

				decimal lineBaseQty = 0;
				foreach (SOShipLineSplit split in shSplits.Select())
				{
					shSplits.Current = shSplits.Search<SOShipLineSplit.shipmentNbr, SOShipLineSplit.lineNbr, SOShipLineSplit.splitLineNbr>(shipmentNbr, line.LineNbr, split.SplitLineNbr);

					var splitKey =
					(
						InventoryID: shSplits.Current.InventoryID,
						SubItemID: shSplits.Current.SubItemID,
						OrderLineUOM: shLines.Current.UOM,
						LocationID: shSplits.Current.LocationID,
						LotSerialNbr: shSplits.Current.LotSerialNbr
					);
					decimal newQty = availability.ContainsKey(splitKey)
						? Math.Min(availability[splitKey].PickedQty, shSplits.Current.Qty.Value)
						: 0;

					if (shSplits.Current.PickedQty != newQty)
					{
						shSplits.SetValueExt<SOShipLineSplit.pickedQty>(shSplits.Current, newQty);
						shSplits.UpdateCurrent(); 
					}

					lineBaseQty += newQty;
					if (newQty > 0)
					{
						availability[splitKey] = (availability[splitKey].PickedQty - newQty, availability[splitKey].Links);
						shipmentEntry.CartSupportExt?.TransformCartLinks(shSplits.Current, availability[splitKey].Links);
					}
				}

				foreach (Unassigned.SOShipLineSplit usplit in shSplitsUnassigned.Select())
				{
					var usplitKey =
					(
						InventoryID: usplit.InventoryID,
						SubItemID: usplit.SubItemID,
						OrderLineUOM: usplit.UOM,
						LocationID: usplit.LocationID
					);

					decimal restQty = usplit.Qty.Value;
					foreach (var item in availability.Where(t => usplitKey.Equals((t.Key.InventoryID, t.Key.SubItemID, t.Key.OrderLineUOM, t.Key.LocationID)) && t.Value.PickedQty > 0).ToArray())
					{
						if (restQty == 0) break;

						decimal newQty = Math.Min(item.Value.PickedQty, restQty);

						if (newQty > 0)
						{
							var newSplit = PropertyTransfer.Transfer(usplit, new SOShipLineSplit());
							newSplit.SplitLineNbr = null;
							newSplit.LotSerialNbr = item.Key.LotSerialNbr;
							newSplit.Qty = newQty;
							newSplit.PickedQty = newQty;
							newSplit.PackedQty = 0;
							newSplit.IsUnassigned = false;
							newSplit.PlanID = null;

							newSplit = shSplits.Insert(newSplit);

							lineBaseQty += newQty;
							availability[item.Key] = (availability[item.Key].PickedQty - newQty, availability[item.Key].Links);
							restQty -= newQty;

							shipmentEntry.CartSupportExt?.TransformCartLinks(newSplit, availability[item.Key].Links);
						}
					}
				}

				if (shLines.Current.BasePickedQty != lineBaseQty)
				{
					shLines.Current.BasePickedQty = lineBaseQty;
					shLines.Current.PickedQty = INUnitAttribute.ConvertFromBase(shLines.Cache, shLines.Current.InventoryID, shLines.Current.UOM, lineBaseQty, INPrecision.NOROUND);
					shLines.Cache.MarkUpdated(shLines.Current);
				}

				docQty += shLines.Current.PickedQty.Value;
			}

			if (docQty > 0)
			{
				shipmentEntry.Document.SetValueExt<SOShipment.hold>(shipmentEntry.Document.Current, false);
				shipmentEntry.Document.SetValueExt<SOShipment.picked>(shipmentEntry.Document.Current, true);
				shipmentEntry.Document.SetValueExt<SOShipment.pickedQty>(shipmentEntry.Document.Current, docQty);
				shipmentEntry.Document.SetValueExt<SOShipment.pickedViaWorksheet>(shipmentEntry.Document.Current, true);
				shipmentEntry.Document.UpdateCurrent();
			}
			shipmentEntry.Save.Press();
		}

		public virtual bool TryMarkWorksheetPicked(SOPickingWorksheet worksheet)
		{
			if (worksheet == null)
				throw new PXArgumentException(nameof(worksheet), ErrorMessages.ArgumentNullException);

			Base.Clear();

			Base.worksheet.Current = Base.worksheet.Search<SOPickingWorksheet.worksheetNbr>(worksheet.WorksheetNbr).TopFirst.With(ValidateMutability);

			if (Base.pickers.SelectMain().All(p => p.Confirmed == true))
			{
				Base.worksheet.Current.Status = SOPickingWorksheet.status.Picked;
				Base.worksheet.UpdateCurrent();
				Base.Save.Press();
				return true;
			}
			return false;
		}

		protected virtual SOPickingWorksheet ValidateMutability(SOPickingWorksheet worksheet)
		{
			if (worksheet.Status.IsIn(SOPickingWorksheet.status.Hold, SOPickingWorksheet.status.Completed))
				throw new PXInvalidOperationException(Msg.WorksheetCannotBeUpdatedInCurrentStatus, worksheet.WorksheetNbr, Base.worksheet.Cache.GetStateExt<SOPickingWorksheet.status>(worksheet));
			return worksheet;
		}

		protected virtual SOPicker ValidateMutability(SOPicker pickList)
		{
			if (pickList.Confirmed == true)
				throw new PXInvalidOperationException(Msg.PickListIsAlreadyConfirmed, pickList.PickListNbr);
			return pickList;
		}


		public virtual void FulfillShipmentsAndConfirmWorksheet(SOPickingWorksheet worksheet)
		{
			PXReportRequiredException report = null;
			using (var ts = new PXTransactionScope())
			{
				var fulfilledShipments = TryFulfillShipments(worksheet);
				if (fulfilledShipments.Any())
				{
					if (worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Batch && PXAccess.FeatureInstalled<CS.FeaturesSet.deviceHub>())
					{
						foreach (var shipmentNbr in fulfilledShipments)
						{
							var packSlipParameters = new Dictionary<string, string>
							{
								[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.WorksheetNbr,
								[nameof(SOShipment.ShipmentNbr)] = shipmentNbr
							};
							if (report == null)
								report = new PXReportRequiredException(packSlipParameters, SOReports.PrintPackSlipBatch);
							else
								report.AddSibling(SOReports.PrintPackSlipBatch, packSlipParameters);
						}

						if (report != null)
							PX.SM.SMPrintJobMaint.CreatePrintJobGroup(
								new PX.SM.PrintSettings
								{
									PrintWithDeviceHub = true,
									DefinePrinterManually = true,
									PrinterID = new CR.NotificationUtility(Base).SearchPrinter(SONotificationSource.Customer, SOReports.PrintPackSlipBatch, Base.Accessinfo.BranchID),
									NumberOfCopies = 1
								}, report, null);
					}

					TryMarkWorksheetPicked(worksheet);
				}
				ts.Complete();
			}
			if (report != null)
				throw report;
		}

		[PXLocalizable]
		public static class Msg
		{
			public const string PickListIsAlreadyConfirmed = "The {0} pick list is already confirmed.";
			public const string WorksheetCannotBeUpdatedInCurrentStatus = "The {0} picking worksheet cannot be modified because it has the {1} status.";
		}
	}

	public class SOShipmentEntryUnlinkWorksheetExt : PXGraphExtension<SOShipmentEntry>
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.action.AddMenuAction(UnlinkFromWorksheet);
		}

		public
			SelectFrom<SOPickerToShipmentLink>.
			Where<
				SOPickerToShipmentLink.worksheetNbr.IsEqual<SOShipment.currentWorksheetNbr.FromCurrent>.
				And<SOPickerToShipmentLink.shipmentNbr.IsEqual<SOShipment.shipmentNbr.FromCurrent>>>.
			View pickerLink;

		[PXButton, PXUIField(DisplayName = "Remove From Worksheet")]
		protected virtual void unlinkFromWorksheet()
		{
			if (Base.Document.Current.PickedQty > 0)
			{
				Base.CartSupportExt?.RemoveItemsFromCart();

				PXSelectBase<SOShipLine> shLines = Base.Transactions;
				PXSelectBase<SOShipLineSplit> shSplits = Base.splits;

				foreach (SOShipLine line in shLines.Select())
				{
					shLines.Current = shLines.Search<SOShipLine.shipmentNbr, SOShipLine.lineNbr>(Base.Document.Current.ShipmentNbr, line.LineNbr);

					foreach (SOShipLineSplit split in shSplits.Select())
					{
						shSplits.Current = shSplits.Search<
						  SOShipLineSplit.shipmentNbr, SOShipLineSplit.lineNbr, SOShipLineSplit.splitLineNbr>(
						  Base.Document.Current.ShipmentNbr, line.LineNbr, split.SplitLineNbr);

						shSplits.SetValueExt<SOShipLineSplit.pickedQty>(shSplits.Current, 0m);
						shSplits.SetValueExt<SOShipLineSplit.basePickedQty>(shSplits.Current, 0m);
						shSplits.UpdateCurrent();
					}
					shLines.Current.BasePickedQty = 0;
					shLines.Current.PickedQty = 0;
					shLines.Cache.MarkUpdated(shLines.Current);
				}

				Base.Document.SetValueExt<SOShipment.picked>(Base.Document.Current, false);
				Base.Document.SetValueExt<SOShipment.pickedQty>(Base.Document.Current, 0m);
				Base.Document.SetValueExt<SOShipment.pickedViaWorksheet>(Base.Document.Current, false);
				Base.Document.UpdateCurrent();
			}

			if (SOPickingWorksheet.PK.Find(Base, Base.Document.Current.CurrentWorksheetNbr).WorksheetType == SOPickingWorksheet.worksheetType.Wave)
			{
				var pickershipmentLink = pickerLink.Select().TopFirst;

				if (pickershipmentLink != null)
					pickerLink.Delete(pickershipmentLink);
			}

			Base.Document.SetValueExt<SOShipment.currentWorksheetNbr>(Base.Document.Current, null);
			Base.Document.UpdateCurrent();

			Base.Save.Press();
		}
		public PXAction<SOShipment> UnlinkFromWorksheet;

		protected virtual void _(Events.RowSelected<SOShipment> e)
		{
			bool unlinkable = e.Row != null && e.Row.Confirmed == false && e.Row.CurrentWorksheetNbr != null;
			if (unlinkable)
			{
				unlinkable &= SOPickingWorksheet.PK.Find(Base, e.Row.CurrentWorksheetNbr).Status == SOPickingWorksheet.status.Picked;
				unlinkable &=
				  SelectFrom<SOShipLineSplit>.
				  Where<
					SOShipLineSplit.shipmentNbr.IsEqual<@P.AsString>.
					And<SOShipLineSplit.packedQty.IsNotEqual<CS.decimal0>>>.
				  View.Select(Base).Any() == false;
			}

			UnlinkFromWorksheet.SetVisible(false);
			UnlinkFromWorksheet.SetEnabled(unlinkable);
		}
	}
}