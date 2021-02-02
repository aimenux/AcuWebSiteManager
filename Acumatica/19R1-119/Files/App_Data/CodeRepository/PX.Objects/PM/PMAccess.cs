using System;
using PX.Data;
using System.Collections;

namespace PX.Objects.PM
{
	public class PMAccessDetail : PX.SM.UserAccess
	{
		#region Select
		public PXSelect<PMProject, Where<PMProject.baseType, Equal<CT.CTPRType.project>>> Project;

		protected override IEnumerable groups()
		{
			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(this))
			{
				if (group.SpecificModule == null || group.SpecificModule == typeof(PMProject).Namespace || PX.SM.UserAccess.IsIncluded(getMask(), group))
				{
					Groups.Current = group;
					yield return group;
				}
			}
		}
		#endregion

		#region Actions
		public PXSave<PMProject> Save;
		public PXCancel<PMProject> Cancel;
		public PXFirst<PMProject> First;
		public PXPrevious<PMProject> Prev;
		public PXNext<PMProject> Next;
		public PXLast<PMProject> Last;
		#endregion

		#region Constructor
		public PMAccessDetail()
		{
			Project.Cache.AllowDelete = false;
			Project.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetRequired(Project.Cache, null, false);
			Views.Caches.Remove(Groups.GetItemType());
			Views.Caches.Add(Groups.GetItemType());
		}
		#endregion

		#region Runtime
		protected override byte[] getMask()
		{
			byte[] mask = null;
			if (User.Current != null)
			{
				mask = User.Current.GroupMask;
			}
			else if (Project.Current != null)
			{
				mask = Project.Current.GroupMask;
			}
			return mask;
		}

		public override void Persist()
		{
			if (User.Current != null)
			{
				PopulateNeighbours<PX.SM.Users>(User, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.SM.Users>();
			}
			else if (Project.Current != null)
			{
				PopulateNeighbours<PMProject>(Project, Groups);
				PXSelectorAttribute.ClearGlobalCache<PMProject>();
			}
			else
			{
				return;
			}
			base.Persist();
		}
		#endregion

		#region DAC overrides
		[PXDimensionSelector(ProjectAttribute.DimensionName,
			typeof(Search<PMProject.contractCD, Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<Match<Current<AccessInfo.userName>>>>>),
			typeof(PMProject.contractCD),
			typeof(PMProject.contractCD), typeof(PMProject.customerID), typeof(PMProject.description), typeof(PMProject.status), DescriptionField = typeof(PMProject.description), Filterable = true)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Project ID", Visibility = PXUIVisibility.SelectorVisible)]
		[Data.EP.PXFieldDescription]
		public void PMProject_ContractCD_CacheAttached(PXCache cache)
		{
		}
		#endregion

	}

	public class PMAccess : PX.SM.BaseAccess
	{
		#region Select
		public PXSelect<PMProject, Where<Current<PX.SM.RelationGroup.groupName>, IsNotNull>> Project;

		protected virtual IEnumerable group()
		{
			PXResultset<PX.SM.Neighbour> set = PXSelectGroupBy<PX.SM.Neighbour,
				Where<PX.SM.Neighbour.leftEntityType, Equal<projectType>>,
				Aggregate<GroupBy<PX.SM.Neighbour.coverageMask,
					GroupBy<PX.SM.Neighbour.inverseMask,
					GroupBy<PX.SM.Neighbour.winCoverageMask,
					GroupBy<PX.SM.Neighbour.winInverseMask>>>>>>.Select(this);

			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(this))
			{
				if ((group.SpecificModule == null || group.SpecificModule == typeof(PMProject).Namespace)
					&& (group.SpecificType == null || group.SpecificType == typeof(PMProject).FullName)
					|| PX.SM.UserAccess.InNeighbours(set, group))
				{
					yield return group;
				}
			}

		}
		#endregion

		#region Event

		protected virtual void PMProject_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PMProject project = e.Row as PMProject;
			PX.SM.RelationGroup group = Group.Current;
			if (project != null && project.GroupMask != null && group != null && group.GroupMask != null && sender.GetStatus(project) == PXEntryStatus.Notchanged)
			{
				for (int i = 0; i < project.GroupMask.Length && i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] != 0x00 && (project.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
					{
						project.Included = true;
					}
				}
			}
		}
		protected virtual void PMProject_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PMProject project = e.Row as PMProject;
			PX.SM.RelationGroup group = Group.Current;
			if (project != null && project.GroupMask != null && group != null && group.GroupMask != null && e.Operation != PXDBOperation.Delete)
			{
				if (project.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = project.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					project.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (project.Included == true)
					{
						project.GroupMask[i] = (byte)(project.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						project.GroupMask[i] = (byte)(project.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}

		protected virtual void RelationGroup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PX.SM.RelationGroup group = e.Row as PX.SM.RelationGroup;
			if (group != null)
			{
				if (String.IsNullOrEmpty(group.GroupName))
				{
					Save.SetEnabled(false);
				}
				else
				{
					Save.SetEnabled(true);
				}
			}
		}

		protected override void RelationGroup_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			base.RelationGroup_RowInserted(cache, e);
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			group.SpecificModule = typeof(PMProject).Namespace;
			group.SpecificType = typeof(PMProject).FullName;
		}

		#endregion

		#region Constructor
		public PMAccess()
		{
			Project.Cache.AllowInsert = false;
			Project.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Project.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<PMProject.included>(Project.Cache, null);
		}
		#endregion

		#region Runtime
		public override void Persist()
		{
			populateNeighbours<PX.SM.Users>(Users);
			populateNeighbours<PMProject>(Project);
			populateNeighbours<PX.SM.Users>(Users);
			base.Persist();
			PXSelectorAttribute.ClearGlobalCache<PX.SM.Users>();
			PXSelectorAttribute.ClearGlobalCache<PMProject>();
			PXDimensionAttribute.Clear();
		}
		#endregion

		#region DAC overrides
		[GL.SubAccount(DisplayName = "Default Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(GL.Sub.description))]
		public void PMProject_DefaultSubID_CacheAttached(PXCache cache)
		{
		}
		#endregion


	}


}
