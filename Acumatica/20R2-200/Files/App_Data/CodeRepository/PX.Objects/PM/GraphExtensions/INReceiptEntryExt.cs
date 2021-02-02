using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    
	public class INReceiptEntryExt : PXGraphExtension<INReceiptEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
		}

		protected virtual bool IsPMVisible
		{
			get
			{
				PM.PMSetup setup = PXSelect<PM.PMSetup>.Select(Base);
				if (setup == null)
				{
					return false;
				}
				else
				{
					if (setup.IsActive != true)
						return false;
					else
						return setup.VisibleInIN == true;
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<INTran, INTran.projectID> e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;

			if (PM.ProjectAttribute.IsPMVisible(BatchModule.IN))
			{
				if (row.LocationID != null)
				{
					PXResultset<INLocation> result = PXSelectReadonly2<INLocation,
						LeftJoin<PMProject,
							On<PMProject.contractID, Equal<INLocation.projectID>>>,
						Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
						And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(e.Cache.Graph, row.SiteID, row.LocationID);

					foreach (PXResult<INLocation, PMProject> res in result)
					{
						PMProject project = (PMProject)res;
						if (project != null && project.ContractCD != null && project.VisibleInIN == true)
						{
							e.NewValue = project.ContractCD;
							return;
						}
					}
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<INTran, INTran.taskID> e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;

			if (PM.ProjectAttribute.IsPMVisible(BatchModule.IN))
			{
				if (row.LocationID != null)
				{
					PXResultset<INLocation> result = PXSelectReadonly2<INLocation,
						LeftJoin<PMTask,
							On<PMTask.projectID, Equal<INLocation.projectID>,
							And<PMTask.taskID, Equal<INLocation.taskID>>>>,
						Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
						And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(e.Cache.Graph, row.SiteID, row.LocationID);

					foreach (PXResult<INLocation, PMTask> res in result)
					{
						PMTask task = (PMTask)res;
						if (task != null && task.TaskCD != null && task.VisibleInIN == true && task.IsActive == true)
						{
							e.NewValue = task.TaskCD;
							return;
						}
					}

				}
			}
		}

		protected virtual void _(Events.FieldUpdated<INTran, INTran.locationID> e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;

			e.Cache.SetDefaultExt<INTran.projectID>(e.Row); //will set pending value for TaskID to null if project is changed. This is the desired behavior for all other screens.
			if (e.Cache.GetValuePending<INTran.taskID>(e.Row) == null) //To redefault the TaskID in currecnt screen - set the Pending value from NULL to NOTSET
				e.Cache.SetValuePending<INTran.taskID>(e.Row, PXCache.NotSetValue);
			e.Cache.SetDefaultExt<INTran.taskID>(e.Row);
		}

		protected virtual void _(Events.FieldVerifying<INTran, INTran.reasonCode> e)
		{
			INTran row = e.Row as INTran;
			if (row != null)
			{
				ReasonCode reasoncd = ReasonCode.PK.Find(Base, (string)e.NewValue);

				if (reasoncd != null && row.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(row.ProjectID))
				{
					PX.Objects.GL.Account account = PXSelect<PX.Objects.GL.Account, Where<PX.Objects.GL.Account.accountID, Equal<Required<PX.Objects.GL.Account.accountID>>>>.Select(Base, reasoncd.AccountID);
					if (account != null && account.AccountGroupID == null)
					{
						e.Cache.RaiseExceptionHandling<INTran.reasonCode>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
					}
				}
			}
		}

		protected virtual void _(Events.RowSelected<INRegister> e)
		{
			if (e.Row == null)
			{
				return;
			}
						
			PXUIFieldAttribute.SetVisible<INTran.projectID>(Base.transactions.Cache, null, IsPMVisible);
			PXUIFieldAttribute.SetVisible<INTran.taskID>(Base.transactions.Cache, null, IsPMVisible);
		}

		protected virtual void _(Events.RowPersisting<INTran> e)
		{
			INTran row = (INTran)e.Row;
			
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				CheckForSingleLocation(e.Cache, row);
				CheckSplitsForSameTask(e.Cache, row);
				CheckLocationTaskRule(e.Cache, row);
			}
		}

		protected virtual void CheckLocationTaskRule(PXCache sender, INTran row)
		{
			if (row.TaskID != null)
			{
				INLocation selectedLocation = INLocation.PK.Find(Base, row.LocationID);

				if (selectedLocation != null && selectedLocation.TaskID != row.TaskID)
				{
					sender.RaiseExceptionHandling<INTran.locationID>(row, selectedLocation.LocationCD,
						new PXSetPropertyException(IN.Messages.LocationIsMappedToAnotherTask, PXErrorLevel.Warning));

				}
			}
		}

		protected virtual void CheckForSingleLocation(PXCache sender, INTran row)
		{
			InventoryItem item = InventoryItem.PK.Find(Base, row.InventoryID);
			if (item != null && item.StkItem == true && row.TaskID != null && row.LocationID == null)
			{
				sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(IN.Messages.RequireSingleLocation));
			}
		}

		protected virtual void CheckSplitsForSameTask(PXCache sender, INTran row)
		{
			if (row.HasMixedProjectTasks == true)
			{
				sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(IN.Messages.MixedProjectsInSplits));
			}
		}
	}
}
