using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
	public class SOPickingWorksheetProcess : PXGraph<SOPickingWorksheetProcess>
	{
		public class ProcessAction
		{
			public const string None = "N";
			public const string PrintPickList = "P";
			public const string CreateWavePickList = "W";
			public const string CreateBatchPickList = "B";

			public class none : PX.Data.BQL.BqlString.Constant<none> { public none() : base(None) { } }
			public class printPickList : PX.Data.BQL.BqlString.Constant<printPickList> { public printPickList() : base(PrintPickList) { } }
			public class createWavePickList : PX.Data.BQL.BqlString.Constant<createWavePickList> { public createWavePickList() : base(CreateWavePickList) { } }
			public class createBatchPickList : PX.Data.BQL.BqlString.Constant<createBatchPickList> { public createBatchPickList() : base(CreateBatchPickList) { } }

			[PXLocalizable]
			public abstract class DisplayNames
			{
				public const string None = "<SELECT>";
				public const string PrintPickList = "Print Pick List";
				public const string CreateWavePickList = "Create and Print Wave Pick List";
				public const string CreateBatchPickList = "Create and Print Batch Pick List";
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					Pair(None, DisplayNames.None),
					Pair(PrintPickList, DisplayNames.PrintPickList),
					Pair(CreateWavePickList, DisplayNames.CreateWavePickList),
					Pair(CreateBatchPickList, DisplayNames.CreateBatchPickList)) { }
			}
		}

		#region DACs
		[PXCacheName(CacheNames.Filter)]
		public class HeaderFilter : IBqlTable
		{
			#region Action
			[PXString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Action")]
			[PXUnboundDefault(ProcessAction.None)]
			[ProcessAction.List]
			public virtual String Action { get; set; }
			public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
			#endregion
			#region SiteID
			[Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr), Required = true)]
			[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>))]
			public virtual Int32? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region StartDate
			[PXDate]
			[PXUIField(DisplayName = "Start Date")]
			public virtual DateTime? StartDate { get; set; }
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
			#endregion
			#region EndDate
			[PXDate]
			[PXUnboundDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "End Date")]
			public virtual DateTime? EndDate { get; set; }
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			#endregion
			#region CustomerID
			[Customer]
			public virtual Int32? CustomerID { get; set; }
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			#endregion
			#region CarrierPluginID
			[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
			[PXUIField(DisplayName = "Carrier", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID>))]
			public virtual String CarrierPluginID { get; set; }
			public abstract class carrierPluginID : PX.Data.BQL.BqlString.Field<carrierPluginID> { }
			#endregion
			#region ShipVia
			[PXString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Ship Via")]
			[PXSelector(typeof(Search<Carrier.carrierID>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
			public virtual String ShipVia { get; set; }
			public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
			#endregion
			#region PackagingType
			[PXString(1, IsFixed = true)]
			[PXUnboundDefault(SOPackageType.ForFiltering.Both)]
			[SOPackageType.ForFiltering.List]
			[PXUIField(DisplayName = "Packaging Type")]
			public virtual String PackagingType { get; set; }
			public abstract class packagingType : PX.Data.BQL.BqlString.Field<packagingType> { }
			#endregion
			#region InventoryID
			[Inventory]
			public virtual Int32? InventoryID { get; set; }
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
			#region LocationID
			[Location(typeof(siteID))]
			public virtual Int32? LocationID { get; set; }
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			#endregion
			#region MaxNumberOfLinesInShipment
			[PXInt]
			[PXUIField(DisplayName = "Max. Number of Lines in Shipment")]
			public int? MaxNumberOfLinesInShipment { get; set; }
			public abstract class maxNumberOfLinesInShipment : PX.Data.BQL.BqlInt.Field<maxNumberOfLinesInShipment> { }
			#endregion
			#region MaxQtyInLines
			[PXInt]
			[PXUIField(DisplayName = "Max. Quantity in Lines")]
			public int? MaxQtyInLines { get; set; }
			public abstract class maxQtyInLines : PX.Data.BQL.BqlInt.Field<maxQtyInLines> { }
			#endregion
		}

		public sealed class HeaderSettings : PXCacheExtension<HeaderFilter>, PX.SM.IPrintable
		{
			#region NumberOfPickers
			[PXInt(MinValue = 1)]
			[PXUnboundDefault(1)]
			[PXUIField(DisplayName = "Max. Number of Pickers")]
			[PXUIVisible(typeof(HeaderFilter.action.IsIn<ProcessAction.createWavePickList, ProcessAction.createBatchPickList>))]
			public int? NumberOfPickers { get; set; }
			public abstract class numberOfPickers : PX.Data.BQL.BqlInt.Field<numberOfPickers> { }
			#endregion
			#region NumberOfTotesPerPicker
			[PXInt(MinValue = 1)]
			[PXUnboundDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Max. Number of Totes per Picker")]
			[PXUIVisible(typeof(HeaderFilter.action.IsEqual<ProcessAction.createWavePickList>))]
			public int? NumberOfTotesPerPicker { get; set; }
			public abstract class numberOfTotesPerPicker : PX.Data.BQL.BqlInt.Field<numberOfTotesPerPicker> { }
			#endregion
			#region PrintWithDeviceHub
			[PXBool]
			[PXUnboundDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
			[PXUIField(DisplayName = "Print with DeviceHub", FieldClass = "DeviceHub")]
			public bool? PrintWithDeviceHub { get; set; }
			public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
			#endregion
			#region DefinePrinterManually
			[PXBool]
			[PXUnboundDefault(true)]
			[PXUIVisible(typeof(printWithDeviceHub))]
			[PXUIEnabled(typeof(printWithDeviceHub))]
			[PXFormula(typeof(False.When<printWithDeviceHub.IsEqual<False>>.Else<definePrinterManually>))]
			[PXUIField(DisplayName = "Define Printer Manually", FieldClass = "DeviceHub")]
			public bool? DefinePrinterManually { get; set; } = false;
			public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
			#endregion
			#region Printer
			[PX.SM.PXPrinterSelector]
			[PXUIVisible(typeof(definePrinterManually))]
			[PXUIEnabled(typeof(definePrinterManually))]
			[PXFormula(typeof(Null.When<definePrinterManually.IsEqual<False>>.Else<printerID>))]
			public Guid? PrinterID { get; set; }
			public abstract class printerID: PX.Data.BQL.BqlGuid.Field<printerID> { }
			#endregion
			#region NumberOfCopies
			[PXInt]
			public int? NumberOfCopies { get; set; }
			public abstract class numberOfCopies : PX.Data.BQL.BqlGuid.Field<numberOfCopies> { }
			#endregion
		}

		[PXCacheName(CacheNames.InventoryLinkFilter)]
		public class InventoryLinkFilter : IBqlTable
		{
			#region InventoryID
			[Inventory(IsKey = true)]
			public virtual Int32? InventoryID { get; set; }
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
			#region Descr
			[PXUIField(DisplayName = "Description", Enabled = false)]
			public class descr : PXFieldAttachedTo<InventoryLinkFilter>.By<SOPickingWorksheetProcess>.AsString.Named<descr>
			{
				public override string GetValue(InventoryLinkFilter Row) => InventoryItem.PK.Find(Base, Row?.InventoryID)?.Descr;
			}
			#endregion
		}

		[PXCacheName(CacheNames.LocationLinkFilter)]
		public class LocationLinkFilter : IBqlTable
		{
			#region LocationID
			[Location(typeof(HeaderFilter.siteID), IsKey = true)]
			public virtual Int32? LocationID { get; set; }
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			#endregion
			#region Descr
			[PXUIField(DisplayName = "Description", Enabled = false)]
			public class descr : PXFieldAttachedTo<LocationLinkFilter>.By<SOPickingWorksheetProcess>.AsString.Named<descr>
			{
				public override string GetValue(LocationLinkFilter Row) => INLocation.PK.Find(Base, Row?.LocationID)?.Descr;
			}
			#endregion
		}
		#endregion

		#region Views
		public PXFilter<HeaderFilter> Filter;

		[PXFilterable]
		public PXFilteredProcessing<SOShipment, HeaderFilter> Shipments;
		public IEnumerable shipments()
		{
			var result = new PXDelegateResult() { IsResultTruncated = true };
			result.AddRange(GetShipments());
			return result;
		}

		[PXVirtualDAC]
		public PXSelect<InventoryLinkFilter> SelectedItems;
		public IEnumerable selectedItems() => SelectedItems.Cache.Cached.Cast<InventoryLinkFilter>().Where(t => SelectedItems.Cache.GetStatus(t) != PXEntryStatus.Deleted).ToArray();

		[PXVirtualDAC]
		public PXSelect<LocationLinkFilter> SelectedLocations;
		public IEnumerable selectedLocations() => SelectedLocations.Cache.Cached.Cast<LocationLinkFilter>().Where(t => SelectedLocations.Cache.GetStatus(t) != PXEntryStatus.Deleted).ToArray();
		#endregion

		#region Actions
		public PXCancel<HeaderFilter> Cancel;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Shipments.Current != null)
			{
				if (Shipments.Current.ShipmentType == INDocType.DropShip)
				{
					PO.POReceiptEntry docgraph = PXGraph.CreateInstance<PO.POReceiptEntry>();
					docgraph.Document.Current = docgraph.Document.Search<PO.POReceipt.receiptNbr>(Shipments.Current.ShipmentNbr);

					throw new PXRedirectRequiredException(docgraph, true, PO.Messages.POReceipt) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
					docgraph.Document.Current = docgraph.Document.Search<SOShipment.shipmentNbr>(Shipments.Current.ShipmentNbr);

					throw new PXRedirectRequiredException(docgraph, true, Messages.SOShipment) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		[PXButton, PXUIField(DisplayName = "List")]
		public virtual void selectItems() => SelectedItems.AskExt();
		public PXAction<HeaderFilter> SelectItems;

		[PXButton, PXUIField(DisplayName = "List")]
		public virtual void selectLocations() => SelectedLocations.AskExt();
		public PXAction<HeaderFilter> SelectLocations;
		#endregion

		#region DAC overrides
		public SelectFrom<INSite>.View INSites;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.SiteDescr, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<INSite.descr> e) { }

		public SelectFrom<Carrier>.View Carriers;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.CarrierDescr, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<Carrier.description> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(SOPickingWorksheet.worksheetNbr))]
		protected void _(Events.CacheAttached<SOShipment.currentWorksheetNbr> e) { }
		#endregion

		#region Events
		protected virtual void _(Events.RowSelected<HeaderFilter> e)
		{
			string action = e.Row.Action;
			var settings = e.Row.GetExtension<HeaderSettings>();
			Shipments.SetProcessDelegate(list => ProcessShipmentsHandler(action, settings, list));
			SelectedItems.Cache.IsDirty = false;
			SelectedLocations.Cache.IsDirty = false;
		}


		protected virtual void _(Events.FieldSelecting<HeaderFilter.inventoryID> e)
		{
			if (selectedItems().Cast<object>().Any())
				e.ReturnValue = PXMessages.LocalizeNoPrefix(Msg.ListPlaceholder);
		}

		protected virtual void _(Events.FieldUpdated<HeaderFilter, HeaderFilter.inventoryID> e)
		{
			if (e.Row.InventoryID != null)
				SelectedItems.Cache.Clear();
		}

		protected virtual void _(Events.RowInserted<InventoryLinkFilter> e) => Filter.SetValueExt<HeaderFilter.inventoryID>(Filter.Current, null);
		protected virtual void _(Events.RowUpdated<InventoryLinkFilter> e) => Filter.SetValueExt<HeaderFilter.inventoryID>(Filter.Current, null);

		protected virtual void _(Events.RowPersisting<InventoryLinkFilter> e) => e.Cancel = true;


		protected virtual void _(Events.FieldSelecting<HeaderFilter.locationID> e)
		{
			if (selectedLocations().Cast<object>().Any())
				e.ReturnValue = Msg.ListPlaceholder;
		}
  
		protected virtual void _(Events.FieldUpdated<HeaderFilter, HeaderFilter.locationID> e)
		{
			if (e.Row.LocationID != null)
				SelectedLocations.Cache.Clear();
		}

		protected virtual void _(Events.RowInserted<LocationLinkFilter> e) => Filter.SetValueExt<HeaderFilter.locationID>(Filter.Current, null);
		protected virtual void _(Events.RowUpdated<LocationLinkFilter> e) => Filter.SetValueExt<HeaderFilter.locationID>(Filter.Current, null);
		
		protected virtual void _(Events.RowPersisting<LocationLinkFilter> e) => e.Cancel = true;
		#endregion


		protected virtual IEnumerable<SOShipment> GetShipments()
		{
			HeaderFilter filter = Filter.Current;

			int[] locations = filter.LocationID != null
				? new[] { filter.LocationID.Value }
				: selectedLocations().Cast<LocationLinkFilter>().Select(l => l.LocationID.Value).ToArray();
			int[] inventories = filter.InventoryID != null
				? new[] { filter.InventoryID.Value }
				: selectedItems().Cast<InventoryLinkFilter>().Select(l => l.InventoryID.Value).ToArray();

			var cmd = new
				SelectFrom<SOShipment>.
				LeftJoin<Carrier>.On<SOShipment.FK.Carrier>.
				InnerJoin<INSite>.On<SOShipment.FK.Site>.
				InnerJoin<SOOrderShipment>.On<
					SOOrderShipment.shipmentType.IsEqual<SOShipment.shipmentType>.
					And<SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>>>.
				InnerJoin<SOOrder>.On<SOOrderShipment.FK.Order>.
				LeftJoin<Customer>.On<SOOrder.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
				Where<
					Match<INSite, AccessInfo.userName.FromCurrent>.
					And<
						Customer.bAccountID.IsNull.
						Or<Match<Customer, AccessInfo.userName.FromCurrent>>>.
					And<SOShipment.shipDate.IsLessEqual<HeaderFilter.endDate.FromCurrent>>.
					And<SOShipment.siteID.IsEqual<HeaderFilter.siteID.FromCurrent>>.
					And<SOShipment.status.IsEqual<SOShipmentStatus.open>>.
					And<SOShipment.shipmentType.IsEqual<SOShipmentType.issue>>.
					And<SOShipment.operation.IsEqual<SOOperation.issue>>>.
				AggregateTo<
					GroupBy<SOShipment.shipmentNbr>>.
				View(this);
			var parameters = new List<object>();

			if (!string.IsNullOrEmpty(filter.CarrierPluginID))
				cmd.WhereAnd<Where<Carrier.carrierPluginID.IsEqual<HeaderFilter.carrierPluginID.FromCurrent>>>();

			if (!string.IsNullOrEmpty(filter.ShipVia))
				cmd.WhereAnd<Where<SOShipment.shipVia.IsEqual<HeaderFilter.shipVia.FromCurrent>>>();

			if (filter.StartDate != null)
				cmd.WhereAnd<Where<SOShipment.shipDate.IsGreaterEqual<HeaderFilter.startDate.FromCurrent>>>();

			if (filter.CustomerID != null)
				cmd.WhereAnd<Where<SOShipment.customerID.IsEqual<HeaderFilter.customerID.FromCurrent>>>();

			if (filter.PackagingType == SOPackageType.ForFiltering.Manual)
				cmd.WhereAnd<Where<SOOrder.isManualPackage.IsEqual<True>>>();
			else if (filter.PackagingType == SOPackageType.ForFiltering.Auto)
				cmd.WhereAnd<Where<SOOrder.isManualPackage.IsNotEqual<True>>>();

			if (filter.Action == ProcessAction.PrintPickList)
				cmd.WhereAnd<Where<SOShipment.pickListPrinted.IsEqual<False>>>();

			if (filter.Action.IsIn(ProcessAction.CreateBatchPickList, ProcessAction.CreateWavePickList))
				cmd.WhereAnd<Where<SOShipment.currentWorksheetNbr.IsNull>>();

			if (inventories.Any())
			{
				cmd.WhereAnd<Where<Not<Exists<
					SelectFrom<SOShipLine>.
					Where<
						SOShipLine.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
						And<SOShipLine.inventoryID.IsNotIn<@P.AsInt>>>
				>>>>();
				parameters.Add(inventories);
			}

			if (locations.Any())
			{
				cmd.WhereAnd<Where<Not<Exists<
					SelectFrom<SOShipLineSplit>.
					Where<
						SOShipLineSplit.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
						And<SOShipLineSplit.locationID.IsNotIn<@P.AsInt>>>
				>>>>();
				parameters.Add(locations);
			}

			List<PXFilterRow> newFilters = new List<PXFilterRow>();
			foreach (PXFilterRow f in PXView.Filters)
			{
				if (string.Equals(f.DataField, nameof(SOOrder.CustomerOrderNbr), StringComparison.OrdinalIgnoreCase))
					f.DataField = nameof(SOOrder) + "__" + nameof(SOOrder.CustomerOrderNbr);
				newFilters.Add(f);
			}

			foreach (var res in cmd.Select(parameters.ToArray()))
			{
				SOShipment shipment = PXResult.Unwrap<SOShipment>(res);
				SOOrder order = PXResult.Unwrap<SOOrder>(res);

				if (filter.MaxNumberOfLinesInShipment.IsNotIn(null, 0))
				{
					int linesCount =
						SelectFrom<SOShipLine>.
						Where<SOShipLine.shipmentNbr.IsEqual<@P.AsString>>.
						AggregateTo<Count>.
						View.ReadOnly
						.Select(this, shipment.ShipmentNbr)
						.RowCount ?? 0;

					if (linesCount > filter.MaxNumberOfLinesInShipment)
						continue;
				}

				if (filter.MaxQtyInLines.IsNotIn(null, 0))
				{
					int linesCount =
						SelectFrom<SOShipLine>.
						Where<
							SOShipLine.shipmentNbr.IsEqual<@P.AsString>.
							And<SOShipLine.shippedQty.IsGreater<@P.AsDecimal>>>.
						AggregateTo<Count>.
						View.ReadOnly
						.Select(this, shipment.ShipmentNbr, filter.MaxQtyInLines.Value)
						.RowCount ?? 0;

					if (linesCount > 0)
						continue;
				}

				bool hasPickedSplits =
					SelectFrom<SOShipLineSplit>.
					Where<
						SOShipLineSplit.shipmentNbr.IsEqual<@P.AsString>.
						And<SOShipLineSplit.pickedQty.IsGreater<decimal0>>>.
					View.ReadOnly
					.Select(this, shipment.ShipmentNbr)
					.Any();
				if (hasPickedSplits)
					continue;

				if (shipment.BilledOrderCntr + shipment.UnbilledOrderCntr + shipment.ReleasedOrderCntr == 1)
					shipment.CustomerOrderNbr = order.CustomerOrderNbr;

				SOShipment cached = Shipments.Locate(shipment);
				if (cached != null)
					shipment.Selected = cached.Selected;
				yield return shipment;
			}

			Shipments.Cache.IsDirty = false;
		}

		private static void ProcessShipmentsHandler(string action, HeaderSettings settings, IEnumerable<SOShipment> shipments)
			=> CreateInstance<SOPickingWorksheetProcess>().ProcessShipments(action, settings, shipments);

		protected virtual void ProcessShipments(string action, HeaderSettings settings, IEnumerable<SOShipment> shipments)
		{
			switch (action)
			{
				case ProcessAction.PrintPickList:
					PrintPickList(settings, shipments);
					break;
				case ProcessAction.CreateWavePickList:
					CreateWavePickList(settings, shipments);
					break;
				case ProcessAction.CreateBatchPickList:
					CreateBatchPickList(settings, shipments);
					break;
				default:
					break;
			}
		}

		protected virtual void PrintPickList(HeaderSettings settings, IEnumerable<SOShipment> shipments)
		{
			var shipmentEntry = CreateInstance<SOShipmentEntry>();

			var adapter = new PXAdapter(PXView.Dummy.For(this, shipments))
			{
				MassProcess = true,
				Arguments = 
				{
					["actionID"] = SOShipmentEntryActionsAttribute.PrintPickList,
					[nameof(PX.SM.IPrintable.PrintWithDeviceHub)] = settings.PrintWithDeviceHub,
					[nameof(PX.SM.IPrintable.DefinePrinterManually)] = settings.DefinePrinterManually,
					[nameof(PX.SM.IPrintable.PrinterID)] = settings.PrinterID,
					[nameof(PX.SM.IPrintable.NumberOfCopies)] = 1
				}
			};

			shipmentEntry.action.Press(adapter).Consume();
		}


		protected virtual void CreateWavePickList(HeaderSettings settings, IEnumerable<SOShipment> shipments)
		{
			if (shipments.Any() == false)
				return;

			if (shipments.GroupBy(s => s.SiteID).Count() > 1)
				throw new PXInvalidOperationException(Msg.SingleSiteIDForAllShipmentsRequired);

			int actualShipmentCountPerPicker = (settings.NumberOfTotesPerPicker ?? 0) != 0
				? settings.NumberOfTotesPerPicker ?? 0
				: (int)Math.Ceiling(Decimal.Divide(shipments.Count(), settings.NumberOfPickers ?? 0));
			int actualPickerCount = (int)Math.Ceiling(Decimal.Divide(shipments.Count(), actualShipmentCountPerPicker));

			if (actualPickerCount > (settings.NumberOfPickers ?? 0))
				throw new PXInvalidOperationException(Msg.OverallTotesShouldBeNotLessThanShipments);

			IEnumerable<SOPickingWorksheetLineSplit> worksheetLineSplits = MergeSplits(shipments);

			if (worksheetLineSplits.Any())
			{
				var worksheetGraph = CreateInstance<SOPickingWorksheetReview>();

				(SOPickingWorksheet worksheet, var lines, var splits) = CreateWorksheet(worksheetGraph, SOPickingWorksheet.worksheetType.Wave, shipments.First().SiteID, worksheetLineSplits);
				LinkShipments(worksheetGraph, shipments, worksheet);
				SpreadShipmentsAmongPickers(worksheetGraph, shipments, actualPickerCount, actualShipmentCountPerPicker);

				worksheetGraph.Save.Press();
			}
		}

		protected virtual void SpreadShipmentsAmongPickers(SOPickingWorksheetReview worksheetGraph, IEnumerable<SOShipment> shipments, int pickerCount, int shipmentCountPerPicker)
		{
			var shArr = shipments.ToArray();
			int? siteID = shArr[0].SiteID;

			IReadOnlyDictionary<string, IEnumerable<(SOShipLineSplit Split, SOShipLine Line)>> splitsOfShipment =
				shipments
				.ToDictionary(
					s => s.ShipmentNbr,
					s => GetShipmentSplits(s)
						.Select(ss => (Split: ss.GetItem<SOShipLineSplit>(), Line: ss.GetItem<SOShipLine>()))
						.ToArray()
						.AsEnumerable())
				.AsReadOnly();

			var fullPath = new WMSPath(
				"FullPath",
				splitsOfShipment
					.SelectMany(kvp => kvp.Value)
					.Select(s => s.Split.LocationID)
					.With(GetSortedLocations));

			IEnumerable<WMSPath> shipmentPaths =
				splitsOfShipment
				.Select(sh => fullPath.GetIntersectionWith(sh.Value.Select(s => s.Split.LocationID), newName: sh.Key))
				.With(paths => WMSPath.SortPaths(paths, includePathLength: true))
				.ToArray();

			IReadOnlyList<List<WMSPath>> shipmentsOfPicker =
				Enumerable
				.Range(0, pickerCount)
				.Select(n =>
					shipmentPaths
					.Skip(n * shipmentCountPerPicker)
					.Take(shipmentCountPerPicker)
					.ToList())
				.ToArray()
				.With(Array.AsReadOnly);

			List<WMSPath> lastPickerShipments = shipmentsOfPicker.Last();
			if (lastPickerShipments.Count < shipmentCountPerPicker)
				lastPickerShipments.AddRange(Enumerable.Range(0, shipmentCountPerPicker - lastPickerShipments.Count).Select(n => WMSPath.MakeFake($"fake{n}")));

			WMSPath[] fullPathOfPicker =
				shipmentsOfPicker
				.Select((shs, i) => WMSPath.MergePaths(shs, $"picker{i}"))
				.ToArray();

			if (shipmentCountPerPicker != 1)
				BalancePathsAmongPickers(shipmentsOfPicker, fullPathOfPicker);

			for (int pickerNbr = 0; pickerNbr < shipmentsOfPicker.Count; pickerNbr++)
			{
				WMSPath path = fullPathOfPicker[pickerNbr];
				List<WMSPath> assignedShipments = shipmentsOfPicker[pickerNbr];

				worksheetGraph.pickers.Insert(new SOPicker { NumberOfTotes = assignedShipments.Count, PathLength = path.PathLength });

				foreach (var shipmentPath in assignedShipments.Where(sh => sh.IsFake == false))
				{
					worksheetGraph.pickerShipments.Insert(new SOPickerToShipmentLink { ShipmentNbr = shipmentPath.Name, SiteID = siteID });

					foreach ((SOShipLineSplit split, SOShipLine line) in splitsOfShipment[shipmentPath.Name])
						worksheetGraph.pickerList.Insert(PickerListEntryFrom(split, line.UOM, shipmentPath.Name));
				}
			}
		}

		protected virtual void BalancePathsAmongPickers(IReadOnlyList<List<WMSPath>> pathsOfPicker, WMSPath[] fullPathOfPicker)
		{
			while (true)
			{
				bool exchangedOccurred = false;

				for (int pickerNbr = 0; pickerNbr < pathsOfPicker.Count - 1; pickerNbr++)
				{
					IReadOnlyCollection<int> pickersToExchange =
						Enumerable
						.Range(0, fullPathOfPicker.Length)
						.Skip(pickerNbr + 1)
						.Where(pn => pn == pickerNbr + 1 || fullPathOfPicker[pickerNbr].GetIntersectionWith(fullPathOfPicker[pn], null).With(inter => inter.IsEmpty == false))
						.ToArray()
						.With(Array.AsReadOnly);

					foreach (int pickerToExchange in pickersToExchange)
					{
						var pairsToExchange =
							(from leftPath in pathsOfPicker[pickerNbr].Where(lp =>
								lp.IsFake || fullPathOfPicker[pickerNbr].EndsWith(lp) || fullPathOfPicker[pickerToExchange].Contains(lp))
								from rightPath in pathsOfPicker[pickerToExchange].Where(rp =>
									rp.IsFake || fullPathOfPicker[pickerToExchange].EndsWith(rp) || fullPathOfPicker[pickerNbr].Contains(rp))
								where leftPath != null && rightPath != null && !(leftPath.IsFake && rightPath.IsFake)
								select (leftPath: leftPath, rightPath: rightPath))
								.ToArray();

						var modifiedPickerPaths =
							pairsToExchange
							.Select(pair => new
							{
								LeftPickerNewFullPath =
									pathsOfPicker[pickerNbr]
									.Except(pair.leftPath)
									.Append(pair.rightPath)
									.With(ps => WMSPath.MergePaths(ps, $"picker{pickerNbr}")),
								RightPickerNewFullPath =
									pathsOfPicker[pickerToExchange]
									.Except(pair.rightPath)
									.Append(pair.leftPath)
									.With(ps => WMSPath.MergePaths(ps, $"picker{pickerToExchange}"))
							})
							.ToArray();

						var exchageProfits =
							modifiedPickerPaths
							.Select(pair =>
								fullPathOfPicker[pickerNbr].PathLength - pair.LeftPickerNewFullPath.PathLength
								+ fullPathOfPicker[pickerToExchange].PathLength - pair.RightPickerNewFullPath.PathLength)
							.ToArray();

						if (exchageProfits.Any(profit => profit > 0) == false)
							continue;

						int maxProfit = exchageProfits.Max();
						int maxProfitIndex = exchageProfits.SelectIndexesWhere(profit => profit == maxProfit).First();
						var pairToExchange = pairsToExchange[maxProfitIndex];
						var newPaths = modifiedPickerPaths[maxProfitIndex];

						pathsOfPicker[pickerNbr].Remove(pairToExchange.leftPath);
						pathsOfPicker[pickerNbr].Add(pairToExchange.rightPath);
						fullPathOfPicker[pickerNbr] = newPaths.LeftPickerNewFullPath;

						pathsOfPicker[pickerToExchange].Remove(pairToExchange.rightPath);
						pathsOfPicker[pickerToExchange].Add(pairToExchange.leftPath);
						fullPathOfPicker[pickerToExchange] = newPaths.RightPickerNewFullPath;

						exchangedOccurred |= true;
					}
				}

				if (exchangedOccurred == false)
					break;
			}
		}


		protected virtual void CreateBatchPickList(HeaderSettings settings, IEnumerable<SOShipment> shipments)
		{
			if (shipments.Any() == false)
				return;

			if (shipments.GroupBy(s => s.SiteID).Count() > 1)
				throw new PXInvalidOperationException(Msg.SingleSiteIDForAllShipmentsRequired);

			IEnumerable<SOPickingWorksheetLineSplit> worksheetLineSplits = MergeSplits(shipments);

			if (worksheetLineSplits.Any())
			{
				var worksheetGraph = CreateInstance<SOPickingWorksheetReview>();

				(SOPickingWorksheet worksheet, var lines, var splits) = CreateWorksheet(worksheetGraph, SOPickingWorksheet.worksheetType.Batch, shipments.First().SiteID, worksheetLineSplits);
				LinkShipments(worksheetGraph, shipments, worksheet);
				SpreadSplitsAmongPickers(worksheetGraph, splits.Join(lines, s => s.LineNbr, l => l.LineNbr, (s, l) => (s, l.UOM)).ToArray(), settings.NumberOfPickers ?? 0);

				worksheetGraph.Save.Press();
			}
		}

		protected virtual void SpreadSplitsAmongPickers(SOPickingWorksheetReview worksheetGraph, IEnumerable<(SOPickingWorksheetLineSplit Split, string UOM)> splits, int maxPickersCount)
		{
			if (maxPickersCount == 1)
			{
				GiveAllSplitsToSinglePicker(worksheetGraph, splits);
				return;
			}

			INLocation[] locations = GetSortedLocations(splits.Select(s => s.Split.LocationID));
			if (locations.Length == 1)
			{
				SpreadSplitsByItems(worksheetGraph, splits, maxPickersCount);
				return;
			}

			int actualPickersCount = Math.Min(maxPickersCount, locations.Length);
			var sortedSplits = splits.OrderBy(s => s.Split.LocationID, locations.Select(loc => loc.LocationID).ToArray()).ToArray();
			INLocation[] separatingLocations = null;

			var distances = GetSortedDistancesBetween(locations).ToArray();

			decimal distanceMean = distances.Average(d => (decimal)d.distance);
			decimal distanceMedian = distances.OrderBy(d => d.distance).Skip((distances.Length % 2 == 0 ? distances.Length : distances.Length - 1) / 2).First().distance;

			var obviousGaps = distances.Where(d => d.distance > 4 * distanceMedian);
			bool hasObviousGaps = (distanceMean > 2 * distanceMedian) && obviousGaps.Any();
			if (hasObviousGaps && obviousGaps.Count() > actualPickersCount - 2)
			{
				separatingLocations = distances
					.Take(actualPickersCount - 1)
					.OrderBy(d => d.source.PathPriority)
					.ThenBy(d => d.source.LocationCD)
					.Select(d => d.target)
					.ToArray();
			}
			else
			{
				decimal fullPathLength = distances.Sum(d => d.distance);
				decimal weightedPathLength = 1.333m * fullPathLength / actualPickersCount;

				var separatingLocationList = new List<INLocation>(actualPickersCount - 1);
				var pathSegments = distances
					.OrderBy(l => l.source.PathPriority)
					.ThenBy(l => l.source.LocationCD)
					.ToArray();
				int currentPathSeg = 0;
				decimal currentPathLength = 0;
				while (true)
				{
					if (separatingLocationList.Count == actualPickersCount - 1)
						break;

					do
					{
						currentPathLength += pathSegments[currentPathSeg].distance;
						currentPathSeg++;
					}
					while (currentPathSeg < pathSegments.Length && currentPathLength + pathSegments[currentPathSeg].distance < weightedPathLength);

					if (currentPathSeg < pathSegments.Length)
					{
						separatingLocationList.Add(pathSegments[currentPathSeg].target);
						currentPathLength = 0;
					}
					else
						break;
				}
				separatingLocations = separatingLocationList.ToArray();
			}

			int currentPicker = 0;
			var pathOfPicker = new WMSPath[separatingLocations.Length + 1];
			var locationsOfCurrentPicker = new List<INLocation>();
			foreach (var location in locations)
			{
				if (currentPicker == separatingLocations.Length || location.LocationID != separatingLocations[currentPicker].LocationID)
				{
					locationsOfCurrentPicker.Add(location);
				}
				else
				{
					pathOfPicker[currentPicker] = new WMSPath($"picker{currentPicker}", locationsOfCurrentPicker);
					locationsOfCurrentPicker.Clear();
					locationsOfCurrentPicker.Add(location);
					currentPicker++;
				}
			}
			pathOfPicker[currentPicker] = new WMSPath($"picker{currentPicker}", locationsOfCurrentPicker);

			int[] separators =
				separatingLocations
				  .Select(loc => sortedSplits.SelectIndexesWhere(s => s.Split.LocationID == loc.LocationID).First())
				  .ToArray();

			currentPicker = 0;
			for (int i = 0; i < sortedSplits.Length; i++)
			{
				if (i == 0 || i.IsIn(separators))
					worksheetGraph.pickers.Insert(new SOPicker { PathLength = pathOfPicker[currentPicker++].PathLength });

				worksheetGraph.pickerList.Insert(PickerListEntryFrom(sortedSplits[i].Split, sortedSplits[i].UOM));
			}
		}

		protected virtual void GiveAllSplitsToSinglePicker(SOPickingWorksheetReview worksheetGraph, IEnumerable<(SOPickingWorksheetLineSplit Split, string UOM)> splits)
		{
			INLocation[] locations = GetSortedLocations(splits.Select(s => s.Split.LocationID));
			int fullPathLength = new WMSPath("fullPath", locations).PathLength;

			var sortedSplits = splits.OrderBy(s => s.Split.LocationID, locations.Select(loc => loc.LocationID).ToArray()).ToArray();

			for (int i = 0; i < sortedSplits.Length; i++)
			{
				if (i == 0)
					worksheetGraph.pickers.Insert(new SOPicker { PathLength = fullPathLength });

				worksheetGraph.pickerList.Insert(PickerListEntryFrom(sortedSplits[i].Split, sortedSplits[i].UOM));
			}
		}

		protected virtual void SpreadSplitsByItems(SOPickingWorksheetReview worksheetGraph, IEnumerable<(SOPickingWorksheetLineSplit Split, string UOM)> splits, int maxPickersCount)
		{
			var items = splits.Select(s => s.Split.InventoryID).Distinct().ToArray();
			var sortedSplits = splits.OrderBy(s => s.Split.InventoryID).ToArray();

			int actualPickersCount = Math.Min(maxPickersCount, items.Length);

			int itemPerPicker = (int)Math.Ceiling(Decimal.Divide(items.Length, actualPickersCount));
			var separatingItems = items.Batch(itemPerPicker, batch => batch.First()).Skip(1).ToArray();
			int[] separators =
				separatingItems
				  .Select(item => sortedSplits.SelectIndexesWhere(s => s.Split.InventoryID == item).First())
				  .ToArray();

			for (int i = 0; i < sortedSplits.Length; i++)
			{
				if (i == 0 || i.IsIn(separators))
					worksheetGraph.pickers.Insert(new SOPicker { PathLength = 0 });

				worksheetGraph.pickerList.Insert(PickerListEntryFrom(sortedSplits[i].Split, sortedSplits[i].UOM));
			}
		}

		protected virtual IEnumerable<SOPickingWorksheetLineSplit> MergeSplits(IEnumerable<SOShipment> shipments)
		{
			var worksheetLineSplits = new List<SOPickingWorksheetLineSplit>();

			foreach (var shipment in shipments)
			{
				var splits = GetShipmentSplits(shipment);
				foreach ((var split, var line) in splits)
				{
					var existingWSSplit = split.IsUnassigned == true
						? null
						: worksheetLineSplits.FirstOrDefault(s =>
							s.InventoryID == split.InventoryID &&
							s.SubItemID == split.SubItemID &&
							s.SiteID == split.SiteID &&
							s.LocationID == split.LocationID &&
							s.UOM == line.UOM &&
							s.LotSerialNbr == split.LotSerialNbr);

					var actualWSSplit = existingWSSplit
						?? new SOPickingWorksheetLineSplit
						{
							InventoryID = split.InventoryID,
							SubItemID = split.SubItemID,
							SiteID = split.SiteID,
							LocationID = split.LocationID,
							UOM = line.UOM,
							LotSerialNbr = split.LotSerialNbr,
							ExpireDate = split.ExpireDate,
							Qty = 0,
							IsUnassigned = split.IsUnassigned
						};

					actualWSSplit.Qty += split.Qty;
					if (existingWSSplit == null)
						worksheetLineSplits.Add(actualWSSplit);
				}
			}

			return worksheetLineSplits;
		}

		protected virtual 
			(SOPickingWorksheet worksheet, IEnumerable<SOPickingWorksheetLine> lines, IEnumerable<SOPickingWorksheetLineSplit> splits) 
			CreateWorksheet
			(SOPickingWorksheetReview worksheetGraph, string worksheetType, int? siteID, IEnumerable<SOPickingWorksheetLineSplit> worksheetLineSplits)
		{
			var worksheet = worksheetGraph.worksheet.Insert(new SOPickingWorksheet { WorksheetType = worksheetType, SiteID = siteID });
			var wsLines = new List<SOPickingWorksheetLine>();
			var wsSplits = new List<SOPickingWorksheetLineSplit>();

			var lines = worksheetLineSplits.GroupBy(s => new { s.SiteID, s.InventoryID, s.SubItemID, s.UOM }).ToArray();
			foreach (var line in lines)
			{
				InventoryItem item = InventoryItem.PK.Find(this, line.Key.InventoryID);
				var lotSerClass = item.With(ii => InventoryItem.FK.LotSerClass.FindParent(this, ii));

				var singleLocation = line.Select(s => s.LocationID).Distinct().Count() == 1;
				var singleLotSerial = line.Select(s => s.LotSerialNbr).Distinct().Count() == 1;

				var wsLine = worksheetGraph.worksheetLines.Insert(
					new SOPickingWorksheetLine
					{
						InventoryID = line.Key.InventoryID,
						SubItemID = line.Key.SubItemID,
						SiteID = line.Key.SiteID,
						UOM = line.Key.UOM,
						LocationID = singleLocation ? line.First().LocationID : null,
						LotSerialNbr = lotSerClass == null || lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered ? "" : singleLotSerial ? line.First().LotSerialNbr : null,
						ExpireDate = singleLotSerial ? line.First().ExpireDate : null,
						Qty = INUnitAttribute.ConvertFromBase(worksheetGraph.worksheetLines.Cache, line.Key.InventoryID, line.Key.UOM, line.Sum(s => s.Qty ?? 0), INPrecision.NOROUND)
					});
				wsLines.Add(wsLine);

				foreach (var split in line)
				{
					split.UOM = item.BaseUnit;
					var inserted = worksheetGraph.worksheetLineSplits.Insert(split);
					wsSplits.Add(inserted);
				}
			}

			return (worksheet, wsLines, wsSplits);
		}

		protected virtual void LinkShipments(SOPickingWorksheetReview worksheetGraph, IEnumerable<SOShipment> shipments, SOPickingWorksheet worksheet)
		{
			foreach (var shipment in shipments)
			{
				worksheet.Qty += shipment.ShipmentQty ?? 0;
				worksheet.ShipmentWeight += shipment.ShipmentWeight ?? 0;
				worksheet.ShipmentVolume += shipment.ShipmentVolume ?? 0;
				worksheet.PackageWeight += shipment.PackageWeight ?? 0;
				worksheet.PackageCount += SOPackageDetailEx.FK.Shipment.SelectChildren(this, shipment).Count();

				shipment.CurrentWorksheetNbr = worksheetGraph.worksheet.Current.WorksheetNbr;
				worksheetGraph.shipments.Update(shipment);
			}
			worksheetGraph.worksheet.Update(worksheet);
		}

		protected virtual PXResult<SOShipLineSplit, SOShipLine>[] GetShipmentSplits(SOShipment shipment)
		{
			var assigned =
				SelectFrom<SOShipLineSplit>.
				InnerJoin<SOShipLine>.On<SOShipLineSplit.FK.ShipLine>.
				Where<SOShipLine.FK.Shipment.SameAsCurrent>.
				View.SelectMultiBound(this, new[] { shipment })
				.AsEnumerable()
				.Cast<PXResult<SOShipLineSplit, SOShipLine>>();

			var unassigned =
				SelectFrom<Unassigned.SOShipLineSplit>.
				InnerJoin<SOShipLine>.On<Unassigned.SOShipLineSplit.FK.ShipLine>.
				Where<SOShipLine.FK.Shipment.SameAsCurrent>.
				View.SelectMultiBound(this, new[] { shipment })
				.AsEnumerable()
				.Cast<PXResult<Unassigned.SOShipLineSplit, SOShipLine>>()
				.Select(r => new PXResult<SOShipLineSplit, SOShipLine>(MakeAssigned(r), r));

			return assigned.Concat(unassigned).ToArray();
		}

		protected virtual INLocation[] GetSortedLocations(IEnumerable<int?> locationIDs)
		{
			var locations = locationIDs
				.Distinct()
				.Select(id => INLocation.PK.Find(this, id))
				.OrderBy(l => l.PathPriority)
				.ThenBy(l => l.LocationCD)
				.ToArray();

			// we don't want to affect shared records - those who are stored in global cache
			var locationsCopies = locations.Select(PXCache<INLocation>.CreateCopy).ToArray();
			int lastPathPriority = 0;
			foreach (var location in locationsCopies)
			{
				if (location.PathPriority.HasValue)
					lastPathPriority = location.PathPriority.Value;
				else
					location.PathPriority = ++lastPathPriority;
			}

			return locationsCopies;
		}

		protected virtual SOPickerListEntry PickerListEntryFrom(SOPickingWorksheetLineSplit split, string orderLineUom)
		{
			return new SOPickerListEntry
			{
				SiteID = split.SiteID,
				LocationID = split.LocationID,
				InventoryID = split.InventoryID,
				SubItemID = split.SubItemID,
				LotSerialNbr = split.LotSerialNbr,
				ExpireDate = split.ExpireDate,
				UOM = split.UOM,
				Qty = split.Qty,
				IsUnassigned = split.IsUnassigned,
				OrderLineUOM = orderLineUom
			};
		}

		protected virtual SOPickerListEntry PickerListEntryFrom(SOShipLineSplit split, string orderLineUom, string shipmentNbr)
		{
			return new SOPickerListEntry
			{
				ShipmentNbr = shipmentNbr,
				SiteID = split.SiteID,
				LocationID = split.LocationID,
				InventoryID = split.InventoryID,
				SubItemID = split.SubItemID,
				LotSerialNbr = split.LotSerialNbr,
				ExpireDate = split.ExpireDate,
				UOM = split.UOM,
				Qty = split.Qty,
				IsUnassigned = split.IsUnassigned,
				OrderLineUOM = orderLineUom
			};
		}

		protected IEnumerable<(INLocation source, INLocation target, int distance)> GetSortedDistancesBetween(IEnumerable<INLocation> locations)
		{
			var distances = new List<(INLocation source, INLocation target, int distance)>();
			using (var e = locations.GetEnumerator())
			{
				if (e.MoveNext())
				{
					INLocation prevLoc = prevLoc = e.Current;
					while (e.MoveNext())
					{
						distances.Add((prevLoc, e.Current, 1 + e.Current.PathPriority.Value - prevLoc.PathPriority.Value));
						prevLoc = e.Current;
					}
				}
			}
			return distances.OrderByDescending(d => d.distance).ThenBy(d => d.source.LocationCD).ToArray();
		}

		private SOShipLineSplit MakeAssigned(Unassigned.SOShipLineSplit unassignedSplit) => PropertyTransfer.Transfer(unassignedSplit, new SOShipLineSplit());

		[PXLocalizable]
		public abstract class Msg
		{
			public const string ListPlaceholder = "<LIST>";
			public const string SingleSiteIDForAllShipmentsRequired = "All selected shipments should have same Site ID.";
			public const string OverallTotesShouldBeNotLessThanShipments = "Overall number of totes should be at least as many as number of shipments.";
		}

		[PXLocalizable]
		public abstract class CacheNames
		{
			public const string Filter = "Filter";
			public const string InventoryLinkFilter = "Inventory List Filter";
			public const string LocationLinkFilter = "Location List Filter";
		}
	}

	public class WMSPath : IEnumerable<INLocation>, IEquatable<WMSPath>
	{
		public string Name { get; }
		public int PathLength { get; }
		private readonly INLocation[] _locations;
		public bool IsEmpty { get; }
		public bool IsDot { get; }
		public bool IsFake { get; private set; }

		public override string ToString() => $"{Name}:{(IsFake ? "IsFake" : PathLength.ToString())}";

		public static WMSPath MakeFake(string name) => new WMSPath(name, Enumerable.Empty<INLocation>()) { IsFake = true };

		public WMSPath(string name, IEnumerable<INLocation> locations)
		{
			Name = name;
			_locations = locations.ToArray();
			IsEmpty = _locations.Length == 0;
			IsDot = _locations.Length == 1;
			PathLength = GetFullPathLength(_locations);
		}

		public WMSPath MergeWith(WMSPath another, string newName)
		{
			if (this.IsEmpty && another.IsEmpty)
				return new WMSPath(newName, Enumerable.Empty<INLocation>());
			if (this.IsEmpty)
				return new WMSPath(newName, another);
			if (another.IsEmpty)
				return new WMSPath(newName, this);
			return
				new[] { this, another }
				.With(ps => SortPaths(ps))
				.Select(p => p.AsEnumerable())
				.Aggregate((pA, pB) => pA.Concat(pB))
				.Distinct(loc => loc.LocationID)
				.ToArray()
				.With(a => new WMSPath(newName, a));
		}

		public WMSPath GetIntersectionWith(WMSPath another, string newName) => GetIntersectionWith(another.AsEnumerable(), newName);
		public WMSPath GetIntersectionWith(IEnumerable<INLocation> locations, string newName) => GetIntersectionWith(locations.Select(loc => loc.LocationID), newName);
		public WMSPath GetIntersectionWith(IEnumerable<int?> locationIDs, string newName)
		{
			int currentIndex = 0;
			int firstLocationIndex = -1;
			int lastLocationIndex = 0;
			var ids = locationIDs.ToHashSet();

			foreach (var loc in _locations)
			{
				if (firstLocationIndex == -1 && ids.Contains(loc.LocationID))
					firstLocationIndex = currentIndex;
				if (ids.Contains(loc.LocationID))
					lastLocationIndex = currentIndex - firstLocationIndex;

				currentIndex++;
			}

			return firstLocationIndex == -1
				? new WMSPath(newName, Enumerable.Empty<INLocation>())
				: new WMSPath(newName, _locations.Skip(firstLocationIndex).Take(lastLocationIndex + 1).ToArray());
		}

		public bool Contains(WMSPath another) => this.GetIntersectionWith(another, "temp").PathLength == another.PathLength;
		public bool StartsWith(WMSPath another) => this.First().LocationID == another.First().LocationID && this.Contains(another);
		public bool EndsWith(WMSPath another) => this.Last().LocationID == another.Last().LocationID && this.Contains(another);

		public static IEnumerable<WMSPath> SortPaths(IEnumerable<WMSPath> paths, bool includePathLength = false)
		{
			return paths
				.OrderByDescending(p => p.Any())
				.OrderBy(p => p.First().PathPriority.Value)
				.ThenBy(p => p.First().LocationCD)
				.ThenBy(p => includePathLength ? p.PathLength : 0);
		}
		public static WMSPath MergePaths(IEnumerable<WMSPath> paths, string newName)
			=> paths.ToArray().With(a => a.Length == 1
				? new WMSPath(newName, a[0])
				: a.Aggregate((path, nextPath) => path.MergeWith(nextPath, newName)));

		private int GetFullPathLength(INLocation[] locations) =>
			locations.Length == 0
				? 0
				: locations[locations.Length - 1].PathPriority.Value - locations[0].PathPriority.Value;

		public IEnumerator<INLocation> GetEnumerator() => _locations.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override bool Equals(object obj) => Equals(obj as WMSPath);
		public bool Equals(WMSPath other) => other != null && Name == other.Name;
		public override int GetHashCode() => 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
	}
}