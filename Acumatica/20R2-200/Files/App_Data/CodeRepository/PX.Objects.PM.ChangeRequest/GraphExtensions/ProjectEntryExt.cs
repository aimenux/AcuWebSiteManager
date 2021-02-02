using PX.Data;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.PM.ChangeRequest.GraphExtensions
{
	public class ProjectEntryExt : PXGraphExtension<PX.Objects.PM.ProjectEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.changeRequest>();
		}

		public override void Initialize()
		{
			Base.Actions.Move(nameof(Base.bill), nameof(CreateChangeRequest), true);

			base.Initialize();
		}

		[PXCopyPasteHiddenView]
        [PXViewName(Messages.Markup)]
		public PXSelect<PMMarkup, Where<PMMarkup.projectID, Equal<Current<PMProject.contractID>>>> Markups;

        [PXFilterable]
        [PXCopyPasteHiddenView]
        [PXViewName(Messages.ChangeRequest)]
        public PXSelect<PMChangeRequest, Where<PMChangeRequest.projectID, Equal<Current<PMProject.contractID>>>> ChangeRequests;

		#region DAC Attributes Override
				
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXDBInt(IsKey = true)]
		protected virtual void PMMarkup_ProjectID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(PMTask.taskID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void PMMarkup_TaskID_CacheAttached(PXCache sender) { }


		#endregion

		public PXAction<PMProject> viewChangeRequest;
        [PXUIField(DisplayName = Messages.ViewChangeRequest, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false)]
        public IEnumerable ViewChangeRequest(PXAdapter adapter)
        {
            if (ChangeRequests.Current != null)
            {
                ChangeRequestEntry target = PXGraph.CreateInstance<ChangeRequestEntry>();
                target.Document.Current = ChangeRequests.Current;

                throw new PXRedirectRequiredException(target, true, Messages.ViewChangeRequest) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            return adapter.Get();
        }

		public PXAction<PMProject> createChangeRequest;
		[PXUIField(DisplayName = "Create Change Request", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable CreateChangeRequest(PXAdapter adapter)
		{
			if (Base.Project.Current != null)
			{
				ChangeRequestEntry target = PXGraph.CreateInstance<ChangeRequestEntry>();
				target.Document.Insert();
				target.Document.SetValueExt<PMChangeRequest.projectID>(target.Document.Current, Base.Project.Current.ContractID);

				throw new PXRedirectRequiredException(target, false, Messages.ViewChangeRequest) { Mode = PXBaseRedirectException.WindowMode.Same };
			}
			return adapter.Get();
		}


		//stores Original TaskID by Markup LineNbr.
		private Dictionary<int, int?> markupMapping = new Dictionary<int, int?>();
		protected virtual void _(Events.RowInserted<PMProject> e)
		{
			int? sourceProjectID = ProjectDefaultAttribute.NonProject();
			if (Base.IsCopyPaste && Base.CopySource != null && Base.CopySource.Project.Current != null)
			{
				sourceProjectID = Base.CopySource.Project.Current.ContractID;
			}

			var select = new PXSelect<PMMarkup, Where<PMMarkup.projectID, Equal<Required<PMMarkup.projectID>>>>(Base);

			markupMapping.Clear();
			bool MarkupsCacheIsDirty = Markups.Cache.IsDirty;
			foreach (PMMarkup setup in select.Select(sourceProjectID))
			{
				PMMarkup markup = new PMMarkup();
				markup.ProjectID = e.Row.ContractID;
				markup.Type = setup.Type;
				markup.Description = setup.Description;
				markup.Value = setup.Value;
				markup.AccountGroupID = setup.AccountGroupID;
				markup.CostCodeID = setup.CostCodeID;
				markup.InventoryID = setup.InventoryID;
							   
				markup = Markups.Insert(markup);
				markupMapping.Add(markup.LineNbr.Value, setup.TaskID);
			}

			Markups.Cache.IsDirty = MarkupsCacheIsDirty;
		}

		protected virtual void _(Events.RowSelected<PMProject> e)
		{
			PXUIFieldAttribute.SetVisible<PMMarkup.inventoryID>(Markups.Cache, null, e.Row.BudgetLevel == BudgetLevels.Item);
			
			bool visible = CostCodeAttribute.UseCostCode() && e.Row.BudgetLevel == BudgetLevels.CostCode;
			PXUIFieldAttribute.SetVisible<PMMarkup.costCodeID>(Markups.Cache, null, visible);
			createChangeRequest.SetEnabled(Base.ChangeOrderVisible());
		}

		[PXOverride]
		public virtual void OnCopyPasteTasksInserted(ProjectEntry target, Dictionary<int, int> taskMap)
		{
			ProjectEntryExt ext = target.GetExtension<ProjectEntryExt>();
			if (ext != null)
			{
				foreach (PMMarkup markup in ext.Markups.Select())
				{
					int? originalTaskID;
					if (ext.markupMapping.TryGetValue(markup.LineNbr.Value, out originalTaskID) && originalTaskID != null)
					{
						markup.TaskID = taskMap[originalTaskID.Value];
					}
				}
			}

		}

		[PXOverride]
		public virtual void OnDefaultFromTemplateTasksInserted(PMProject project, PMProject template, Dictionary<int, int> taskMap)
		{
			foreach (PMMarkup markup in Markups.Select())
			{
				Markups.Delete(markup);
			}

			foreach(PMMarkup source in Markups.View.SelectMultiBound(new object[] { template }))
			{
				PMMarkup markup = PXCache<PMMarkup>.CreateCopy(source);
				markup.ProjectID = project.ContractID;
				markup.TaskID = null;
				markup.NoteID = null;
				markup = Markups.Insert(markup);
				markup.TaskID = null;//reset value set by DBDefault based on 'Current'

				if (source.TaskID != null)
				{
					int taskID;
					if (taskMap.TryGetValue(source.TaskID.Value, out taskID))
					{
						markup.TaskID = taskID;
					}
				}
			}
		}

		[PXOverride]
		public virtual void OnCreateTemplateTasksInserted(TemplateMaint target, PMProject template, Dictionary<int, int> taskMap)
		{
			TemplateMaintExt ext = target.GetExtension<TemplateMaintExt>();
			if (ext != null)
			{
				foreach (PMMarkup markup in ext.Markups.Select())
				{
					ext.Markups.Delete(markup);
				}

				foreach (PMMarkup source in Markups.Select())
				{
					PMMarkup markup = PXCache<PMMarkup>.CreateCopy(source);
					markup.ProjectID = template.ContractID;
					markup.TaskID = null;
					markup.NoteID = null;
					markup = ext.Markups.Insert(markup);
					markup.TaskID = null;//reset value set by DBDefault based on 'Current'

					if (source.TaskID != null)
					{
						int taskID;
						if (taskMap.TryGetValue(source.TaskID.Value, out taskID))
						{
							markup.TaskID = taskID;
						}
					}
				}
			}
		}
	}
}
