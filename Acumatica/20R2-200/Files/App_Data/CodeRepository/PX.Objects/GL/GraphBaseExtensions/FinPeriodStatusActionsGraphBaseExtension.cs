using PX.Data;
using PX.Objects.GL.FinPeriods;
using System.Collections;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.Common.Extensions;

namespace PX.Objects.GL.GraphBaseExtensions
{
	public class FinPeriodStatusActionsGraphBaseExtension<TGraph, TYear> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TYear : class, IBqlTable, IFinYear, new()
	{
		public PXSetup<GLSetup> GLSetup;

		protected virtual void _(Events.RowSelected<TYear> e)
		{
			bool isReverseActionAvailable = e.Cache.Graph.GetService<IFinPeriodUtils>().CanPostToClosedPeriod();

			Reopen.SetEnabled(isReverseActionAvailable);
			Unlock.SetEnabled(isReverseActionAvailable);
		}

		private void RedirectToStatusProcessing(string action)
		{
			TYear finYear = (TYear)Base.Caches<TYear>().Current;
			FinPeriodStatusProcess statusProcessing = PXGraph.CreateInstance<FinPeriodStatusProcess>();

			FinPeriodStatusProcess.FinPeriodStatusProcessParameters filter = new FinPeriodStatusProcess.FinPeriodStatusProcessParameters
			{
				OrganizationID = finYear.OrganizationID == FinPeriod.organizationID.MasterValue
					? null
					: finYear.OrganizationID,
				Action = action
			};

			// Defaulting of the partial prefilled filter record
			// Common approach (by PXCache.Insert()) does not work because the current (and single inserted) filter record always exists (???)
			filter = statusProcessing.Caches<FinPeriodStatusProcess.FinPeriodStatusProcessParameters>().InitNewRow(filter);

			if (FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.GetDirection(action) == FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Direction.Direct)
			{
				filter.ToYear = finYear.Year;
				// Adjust ToYear value in according to allowed year range
				filter.ToYear = string.Compare(filter.ToYear, filter.FirstYear) >= 0 ? filter.ToYear : filter.FirstYear;
				filter.ToYear = string.Compare(filter.ToYear, filter.LastYear) <= 0 ? filter.ToYear : filter.LastYear;
			}
			else
			{
				filter.FromYear = finYear.Year;
				// Adjust FromYear value in according to allowed year range
				filter.FromYear = string.Compare(filter.FromYear, filter.FirstYear) >= 0 ? filter.FromYear : filter.FirstYear;
				filter.FromYear = string.Compare(filter.FromYear, filter.LastYear) <= 0 ? filter.FromYear : filter.LastYear;
			}

			statusProcessing.Filter.Current = filter;
			PXRedirectHelper.TryRedirect(statusProcessing, PXRedirectHelper.WindowMode.Same);
		}

		public PXMenuAction<TYear> ActionsMenu;

		public PXAction<TYear> Open;
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "Open Periods", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable open(PXAdapter adapter)
		{
			RedirectToStatusProcessing(FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Open);
			return adapter.Get();
		}

		public PXAction<TYear> Close;
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "Close Periods", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable close(PXAdapter adapter)
		{
			RedirectToStatusProcessing(FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Close);
			return adapter.Get();
		}

		public PXAction<TYear> Lock;
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "Lock Periods", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable @lock(PXAdapter adapter)
		{
			RedirectToStatusProcessing(FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Lock);
			return adapter.Get();
		}

		public PXAction<TYear> Deactivate;
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "Deactivate Periods", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable deactivate(PXAdapter adapter)
		{
			RedirectToStatusProcessing(FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Deactivate);
			return adapter.Get();
		}

		public PXAction<TYear> Reopen;
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "Reopen Periods", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable reopen(PXAdapter adapter)
		{
			RedirectToStatusProcessing(FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Reopen);
			return adapter.Get();
		}

		public PXAction<TYear> Unlock;
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "Unlock Periods", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable unlock(PXAdapter adapter)
		{
			RedirectToStatusProcessing(FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Unlock);
			return adapter.Get();
		}

		public override void Initialize()
		{
			base.Initialize();

			GLSetup glSetup = GLSetup.Current;
			AddMenuActions();
		}

		public virtual void AddMenuActions()
		{
			ActionsMenu.AddMenuAction(Open);
			ActionsMenu.AddMenuAction(Close);
			ActionsMenu.AddMenuAction(Lock);
			ActionsMenu.AddMenuAction(Deactivate);
			ActionsMenu.AddMenuAction(Reopen);
			ActionsMenu.AddMenuAction(Unlock);
		}
	}
}
