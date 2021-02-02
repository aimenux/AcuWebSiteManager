using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.IN.INScanWarehousePath, PX.Objects.IN.INScanWarehousePathHost, PX.Objects.IN.INSite, PX.Objects.IN.INScanWarehousePath.Header>;

namespace PX.Objects.IN
{
	public class INScanWarehousePathHost : INSiteMaint
	{
		public override Type PrimaryItemType => typeof(INScanWarehousePath.Header);
		public PXFilter<INScanWarehousePath.Header> HeaderView;
	}

	public class INScanWarehousePath : WMSBase
	{
		#region DACs
		public class Header : WMSHeader
		{
			#region NextPathIndex
			[PXInt]
			[PXUnboundDefault(1)]
			[PXUIField(DisplayName = "Next Path Index", Enabled = false)]
			public virtual int? NextPathIndex { get; set; }
			public abstract class nextPathIndex : PX.Data.BQL.BqlInt.Field<nextPathIndex> { }
			#endregion
			#region SiteID
			[Site(Enabled = false)]
			public virtual int? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region LocationID
			[Location]
			public virtual int? LocationID { get; set; }
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			#endregion
		}
		#endregion

		#region Views
		public override PXFilter<Header> HeaderView => Base.HeaderView;

		protected virtual IEnumerable Location()
		{
			var rows =
				SelectFrom<INLocation>.
				Where<INLocation.siteID.IsEqual<INSite.siteID.FromCurrent>>.
				OrderBy<INLocation.pathPriority.Asc, INLocation.locationCD.Asc>.
				View.Select(Base);

			var result = new PXDelegateResult { IsResultSorted = true };
			result.AddRange(rows);
			return result;
		}
		#endregion

		#region Buttons
		public PXAction<Header> ScanNextPathIndex;
		[PXButton, PXUIField(DisplayName = "Set Next Path Index")]
		protected virtual IEnumerable scanNextPathIndex(PXAdapter adapter) => scanBarcode(adapter, ScanCommands.SetNextIndex);

		public PXAction<Header> Review;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable review(PXAdapter adapter) => adapter.Get();
		#endregion

		#region Event Handlers
		protected override void _(Events.RowSelected<Header> e)
		{
			base._(e);

			ScanQty.SetVisible(false);
			ScanRemove.SetVisible(false);
			ScanConfirm.SetVisible(false);
			ScanNextPathIndex.SetEnabled(e.Row?.ScanState != ScanStates.NextIndex);
			Review.SetVisible(Base.IsMobile);

			Logs.AllowInsert = Logs.AllowDelete = Logs.AllowUpdate = false;
			Base.location.AllowInsert = Base.location.AllowDelete = Base.location.AllowUpdate = false;
		}
		#endregion

		protected override WMSModeOf<INScanWarehousePath, INScanWarehousePathHost> DefaultMode => Modes.ScanPath;
		public override string CurrentModeName => Msg.ScanWarehousePathMode;
		protected override string GetModePrompt()
		{
			if (HeaderView.Current.SiteID == null)
				return Localize(Msg.WarehousePrompt);
			if (HeaderView.Current.LocationID == null)
				return Localize(Msg.LocationPrompt);
			return null;
		}

		protected override string GetDefaultState(Header header = null) => ScanStates.Warehouse;

		protected override bool ProcessCommand(string barcode)
		{
			switch (barcode)
			{
				case ScanCommands.SetNextIndex:
					SetScanState(ScanStates.NextIndex);
					return true;
			}
			return false;
		}

		protected override bool ProcessByState(Header doc)
		{
			switch (doc.ScanState)
			{
				case ScanStates.Warehouse:
					ProcessWarehouseBarcode(doc.Barcode);
					return true;
				case ScanStates.NextIndex:
					ProcessNextIndex(doc.Barcode);
					return true;
				default:
					return base.ProcessByState(doc);
			}
		}

		protected override void ApplyState(string state)
		{
			switch (state)
			{
				case ScanStates.NextIndex:
					Prompt(Msg.NextIndexPrompt);
					break;
				case ScanStates.Warehouse:
					Prompt(Msg.WarehousePrompt);
					break;
				case ScanStates.Location:
					Prompt(Msg.LocationPrompt);
					break;
				case ScanStates.Confirm:
					ProcessConfirm();
					break;
			}
		}

		protected virtual void ProcessNextIndex(string barcode)
		{
			if (ushort.TryParse(barcode, out ushort nextIndex))
			{
				HeaderSetter.Set(h => h.NextPathIndex, nextIndex);
				SetScanState(HeaderView.Current.SiteID == null ? ScanStates.Warehouse : ScanStates.Location, Msg.PathIndexSet, nextIndex);
			}
			else
			{
				ReportError(Msg.QtyBadFormat);
			}
		}

		protected virtual void ProcessWarehouseBarcode(string barcode)
		{
			INSite site =
				SelectFrom<INSite>.
				Where<INSite.siteCD.IsEqual<@P.AsString>>.
				View.ReadOnly.Select(Base, barcode);

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
				Base.site.Current = site;
				HeaderSetter.Set(h => h.SiteID, site.SiteID);
				SetScanState(ScanStates.Location, Msg.WarehouseReady, site.SiteCD);
			}
		}

		protected override void ProcessLocationBarcode(string barcode)
		{
			INLocation location = ReadLocationByBarcode(HeaderView.Current.SiteID, barcode);
			if (location == null)
				return;

			Base.location.Current = location;
			HeaderSetter.Set(h => h.LocationID, location.LocationID);
			Report(Msg.LocationReady, location.LocationCD);

			SetScanState(ScanStates.Confirm);
		}

		protected virtual void ProcessConfirm()
		{
			ExecuteAndCompleteFlow(() =>
			{
				Base.location.SetValueExt<INLocation.pathPriority>(Base.location.Current, HeaderView.Current.NextPathIndex);
				Base.location.UpdateCurrent();

				SetScanState(ScanStates.Location, Msg.PathIndexAssignedToLocation, HeaderView.Current.NextPathIndex, Base.location.Current.LocationCD);
				HeaderSetter.Set(h => h.NextPathIndex, HeaderView.Current.NextPathIndex + 1);

				int? siteID = HeaderView.Current.SiteID;
				return WMSFlowStatus.Ok.WithPostAction(() => HeaderSetter.Set(h => h.SiteID, siteID));
			});
		}

		protected override void ClearHeaderInfo(bool redirect = false)
		{
			base.ClearHeaderInfo(redirect);
			HeaderSetter.Set(h => h.SiteID, null);
			HeaderSetter.Set(h => h.LocationID, null);
		}

		protected override void ClearMode()
		{
			ClearHeaderInfo(true);
			Report(Msg.ScreenCleared);
			Base.site.Current = null;
			SetScanState(ScanStates.Warehouse);
		}

		protected override bool UseQtyCorrectection => false;
		protected override bool ExplicitLineConfirmation => false;

		protected override void ProcessDocumentNumber(string barcode) => throw new NotImplementedException();
		protected override void ProcessCartBarcode(string barcode) => throw new NotImplementedException();
		protected override void ProcessItemBarcode(string barcode) => throw new NotImplementedException();
		protected override void ProcessLotSerialBarcode(string barcode) => throw new NotImplementedException();
		protected override void ProcessExpireDate(string barcode) => throw new NotImplementedException();

		#region Constants & Messages
		public new abstract class Modes : WMSBase.Modes
		{
			public static WMSModeOf<INScanWarehousePath, INScanWarehousePathHost> ScanPath { get; } = WMSMode("PATH");
			public class scanPath : BqlString.Constant<scanPath> { public scanPath() : base(ScanPath) { } }
		}

		public new abstract class ScanStates : WMSBase.ScanStates
		{
			public const string NextIndex = "NIDX";
			public const string Warehouse = "SITE";
			public const string Confirm = "CONF";
		}

		public new abstract class ScanCommands : WMSBase.ScanCommands
		{
			public const string SetNextIndex = Marker + "NEXT";
		}

		[PXLocalizable]
		public new abstract class Msg : WMSBase.Msg
		{
			public const string ScanWarehousePathMode = "SCAN PATH";
			public const string NextIndexPrompt = "Enter the new next path index.";
			public const string PathIndexAssignedToLocation = "The {0} path index is assigned to the {1} location.";
			public const string PathIndexSet = "The next path index is set to {0}.";
		}
		#endregion
	}
}