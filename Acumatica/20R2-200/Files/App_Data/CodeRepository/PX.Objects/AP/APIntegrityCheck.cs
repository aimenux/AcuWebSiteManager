using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.GL;
using PX.SM;

namespace PX.Objects.AP
{
    [Serializable]
	public partial class APIntegrityCheckFilter : PX.Data.IBqlTable
	{
		#region VendorClassID
		public abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		protected String _VendorClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(VendorClass.vendorClassID), DescriptionField = typeof(VendorClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Vendor Class")]
		public virtual String VendorClassID
		{
			get
			{
				return this._VendorClassID;
			}
			set
			{
				this._VendorClassID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FinPeriodNonLockedSelector]
		[PXDefault]
		[PXUIField(DisplayName = GL.Messages.FinPeriod)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion	
	}



	[TableAndChartDashboardType]
	public class APIntegrityCheck : PXGraph<APIntegrityCheck>
	{
		public PXFilter<APIntegrityCheckFilter> Filter;
		public PXCancel<APIntegrityCheckFilter> Cancel;
		[PXFilterable]
		public PXFilteredProcessing<Vendor, APIntegrityCheckFilter,
			Where<Match<Current<AccessInfo.userName>>>> APVendorList;
		public PXSelect<Vendor,
			Where<Vendor.vendorClassID, Equal<Current<APIntegrityCheckFilter.vendorClassID>>,
			And<Match<Current<AccessInfo.userName>>>>>
			APVendorList_ByVendorClassID;

		public APIntegrityCheck()
		{
			APSetup setup = APSetup.Current;
		}

		protected virtual void APIntegrityCheckFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			bool errorsOnForm = PXUIFieldAttribute.GetErrors(sender, null, PXErrorLevel.Error, PXErrorLevel.RowError).Count > 0;
			APVendorList.SetProcessEnabled(!errorsOnForm);
			APVendorList.SetProcessAllEnabled(!errorsOnForm);
			APVendorList.SuppressMerge = true;
			APVendorList.SuppressUpdate = true;

			APIntegrityCheckFilter filter = Filter.Current;

			APVendorList.SetProcessDelegate<APReleaseProcess>(
				delegate(APReleaseProcess re, Vendor vend)
				{
					re.Clear(PXClearOption.PreserveTimeStamp);
					re.IntegrityCheckProc(vend, filter.FinPeriodID);
				}
			);

			//For perfomance recomended select not more than maxVendorCount vendors, 
			//because the operation is performed for a long time.
			const int maxVendorCount = 5;
			APVendorList.SetParametersDelegate(delegate(List<Vendor> list)
			{
				bool processing = true;
				if (PX.Common.PXContext.GetSlot<AUSchedule>() == null && list.Count > maxVendorCount)
				{
					WebDialogResult wdr = APVendorList.Ask(Messages.ContinueValidatingBalancesForMultipleVendors, MessageButtons.OKCancel);
					processing = wdr == WebDialogResult.OK;
				}
				return processing;
			});
		}

		public virtual IEnumerable apvendorlist()
		{
			if (Filter.Current != null && Filter.Current.VendorClassID != null)
			{
				return APVendorList_ByVendorClassID.Select();
			}
			return null;
		}

		public PXSetup<APSetup> APSetup;

		public PXAction<APIntegrityCheckFilter> viewVendor;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewVendor(PXAdapter adapter)
		{
			VendorMaint graph = CreateInstance<VendorMaint>();
			graph.BAccount.Current = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<Vendor.bAccountID>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, "View Vendor") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

	}
}
