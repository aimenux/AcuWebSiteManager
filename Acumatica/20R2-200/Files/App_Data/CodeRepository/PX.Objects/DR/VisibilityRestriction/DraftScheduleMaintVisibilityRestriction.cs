using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.IN;

namespace PX.Objects.DR
{
	public class DraftScheduleMaintVisibilityRestriction : PXGraphExtension<DraftScheduleMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.Schedule.Join<LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<DRSchedule.bAccountID>>>>();
			Base.Schedule.WhereAnd<Where<BAccountR.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}

		[PXHidden]
		public PXSetup<BAccountR>.Where<BAccountR.bAccountID.IsEqual<DRSchedule.bAccountID.FromCurrent>> _dummyBAccountR;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search2<
							DR.DRSchedule.scheduleNbr, 
							LeftJoin<BAccountR, 
								On<DRSchedule.bAccountID, Equal<BAccountR.bAccountID>>>>),
					typeof(DRSchedule.scheduleNbr),
					typeof(DRSchedule.documentTypeEx),
					typeof(DRSchedule.refNbr),
					typeof(DRSchedule.bAccountID))]
		[RestrictCustomerByUserBranches(typeof(BAccountR.cOrgBAccountID))]
		public void _(Events.CacheAttached<DRSchedule.scheduleNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictBranchByCustomer(typeof(DRSchedule.bAccountID), typeof(BAccountR.cOrgBAccountID))]
		public void _(Events.CacheAttached<DRScheduleDetail.branchID> e)
		{
		}

		public delegate void DRScheduleDetail_BranchID_FieldDefaultingDelegate(PXCache sender, PXFieldDefaultingEventArgs e);

		[PXOverride]
		public void DRScheduleDetail_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e,
			DRScheduleDetail_BranchID_FieldDefaultingDelegate baseMethod)
		{
			baseMethod.Invoke(sender, e);

			DRSchedule dRSchedule = Base.Schedule.Cache.Current as DRSchedule;
			if (dRSchedule == null)
				return;

			bool result = false;
			var defaultBranch = PXAccess.GetBranch((int?)e.NewValue);

			BAccount bAccount = (BAccountR)PXSelect<
				BAccountR, 
				Where<BAccountR.bAccountID, Equal<Current<DRSchedule.bAccountID>>>>
				.Select(Base);

			var parents = RestrictByOrganization<IBqlParameter>.GetParents(bAccount?.COrgBAccountID ?? 0);

			foreach (int i in parents)
			{
				if (i.Equals(defaultBranch?.BAccountID))
				{
					result = true;
				}
			}

			if (!result)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		public void _(Events.RowUpdating<DRSchedule> e)
		{
			DRSchedule row = (DRSchedule)e.NewRow;
			if (!e.Cache.ObjectsEqual<DRSchedule.bAccountID>(e.Row, e.NewRow))
			{
				PXCache cache = Base.Caches<DRScheduleDetail>();
				foreach (PXResult<DRScheduleDetail, DRSchedule> res in Base.Components.Select())
				{
					DRScheduleDetail detail = res;
					detail.BAccountID = row.BAccountID;
					object newValue = detail.BranchID;
					try
					{
						cache.RaiseFieldVerifying<DRScheduleDetail.branchID>(row, ref newValue);
					}
					catch (PXSetPropertyException ex)
					{
						BAccountR baccount = _dummyBAccountR.Select();
						e.Cache.RaiseExceptionHandling<DRSchedule.bAccountID>(row, baccount?.AcctCD, ex);
					}
				}
			}
		}

		public void _(Events.RowPersisting<DRSchedule> e)
		{
			DRSchedule row = (DRSchedule)e.Row;
			PXCache cache = Base.Caches<DRScheduleDetail>();
			foreach (PXResult<DRScheduleDetail, DRSchedule> res in Base.Components.Select())
			{
				DRScheduleDetail detail = res;
				object newValue = detail.BranchID;
				try
				{
					cache.RaiseFieldVerifying<DRScheduleDetail.branchID>(row, ref newValue);
				}
				catch (PXSetPropertyException ex)
				{
					BAccountR baccount = _dummyBAccountR.Select();
					e.Cache.RaiseExceptionHandling<DRSchedule.bAccountID>(row, baccount?.AcctCD, ex);
				}
			}
		}

		public void _(Events.FieldUpdated<DRScheduleDetail.branchID> e)
		{
			DRScheduleDetail row = (DRScheduleDetail)e.Row;
			PXCache cache = Base.Caches<DRScheduleTran>();
			foreach (PXResult<DRScheduleTran> res in Base.Transactions.Select())
			{
				DRScheduleTran tran = res;
				cache.SetValue<DRScheduleTran.branchID>(tran, row.BranchID);
			}
		}
	}

	public sealed class DRScheduleDetailVisibilityRestriction : PXCacheExtension<DRScheduleDetail>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID), branchID: typeof(DRScheduleDetail.branchID))]
		public int? BAccountID { get; set; }
		#endregion
	}
}