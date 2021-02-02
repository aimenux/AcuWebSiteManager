using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.SM;
using PX.Objects.EP;

namespace PX.TM
{
	[Serializable]
	public class ImportCompanyTreeMaint : PXGraph<ImportCompanyTreeMaint>
	{
		[Serializable]
		public partial class SelectedNode : IBqlTable
		{
			#region FolderID
			public abstract class folderID : PX.Data.BQL.BqlInt.Field<folderID> { }
			protected int? _FolderID;
			[PXDBInt(IsKey = true)]
			public virtual int? FolderID
			{
				get
				{
					return this._FolderID;
				}
				set
				{
					this._FolderID = value;
				}
			}
			#endregion

			#region WorkGroupID
			public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
			protected int? _WorkGroupID;
			[PXDBInt(IsKey = true)]
			public virtual int? WorkGroupID
			{
				get
				{
					return this._WorkGroupID;
				}
				set
				{
					this._WorkGroupID = value;
				}
			}
			#endregion
		}

		public PXSelect<EPCompanyTreeMaster, Where<EPCompanyTreeMaster.parentWGID, Equal<Argument<int?>>>, OrderBy<Asc<EPCompanyTreeMaster.sortOrder>>> Folders;
		public PXSelect<EPCompanyTreeMaster, Where<EPCompanyTreeMaster.parentWGID, Equal<Argument<int?>>>, OrderBy<Asc<EPCompanyTreeMaster.sortOrder>>> Items;
		public PXSelectJoin<EPCompanyTreeMember,
			LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>,
			LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>, And<EPEmployeePosition.isActive, Equal<True>>>>>,
			Where<EPCompanyTreeMember.workGroupID, Equal<Current<SelectedNode.workGroupID>>>,
			OrderBy<Asc<EPEmployee.acctCD>>> Members;

		public PXSave<EPCompanyTreeMaster> Save;
		public PXCancel<EPCompanyTreeMaster> Cancel;


		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search2<Users.pKID, LeftJoin<EPEmployee, On<Users.pKID, Equal<EPEmployee.userID>>, LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>, And<EPEmployeePosition.isActive, Equal<True>>>>>>),
			typeof(Users.username),
			typeof(EPEmployee.acctCD),
			typeof(EPEmployee.acctName),
			typeof(EPEmployeePosition.positionID),
			typeof(EPEmployee.departmentID),
			SubstituteKey = typeof(Users.username), CacheGlobal = true)]
		[PXRestrictor(typeof(Where<EPEmployee.acctCD, IsNotNull>), Objects.EP.Messages.UserWithoutEmployee, typeof(Users.username))]
		[PXUIField(DisplayName = "User", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void EPCompanyTreeMember_UserID_CacheAttached(PXCache sender) { }

		protected virtual IEnumerable folders(
			[PXInt]
			int? WorkGroupID
		)
		{
			if (WorkGroupID == null)
			{
				yield return new EPCompanyTreeMaster()
				{
					WorkGroupID = 0,
					Description = PXSiteMap.RootNode.Title
				};

			}
            else
            {
                foreach (EPCompanyTreeMaster item in PXSelect<EPCompanyTreeMaster,
                    Where<EPCompanyTreeMaster.parentWGID,
                        Equal<Required<EPCompanyTreeMaster.workGroupID>>>>.Select(this, WorkGroupID))
                {
                    if (!string.IsNullOrEmpty(item.Description))
                        yield return item;
                }
            }
        }

		protected virtual IEnumerable items(
			[PXInt]
			int? WorkGroupID
		)
		{
			if (WorkGroupID == null)
				WorkGroupID = this.Folders.Current != null
								  ? this.Folders.Current.WorkGroupID
								  : 0;

			this.CurrentSelected.FolderID = WorkGroupID;

			return PXSelect<EPCompanyTreeMaster,
				 Where<EPCompanyTreeMaster.parentWGID,
					Equal<Required<EPCompanyTreeMaster.workGroupID>>>>.Select(this, WorkGroupID);
		}


		protected virtual void EPCompanyTreeMaster_SortOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPCompanyTreeMaster row = (EPCompanyTreeMaster)e.Row;
			if (row == null) return;

			e.NewValue = 1;
			e.Cancel = true;

			PXResultset<EPCompanyTreeMaster> list = Items.Select(row.ParentWGID);
			if (list.Count > 0)
			{
				EPCompanyTreeMaster last = list[list.Count - 1];
				e.NewValue = last.SortOrder + 1;
			}
		}

		protected virtual void EPCompanyTreeMaster_ParentWGID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPCompanyTreeMaster row = e.Row as EPCompanyTreeMaster;
			if (row == null) return;
			e.NewValue = this.CurrentSelected.FolderID ?? 0;
			e.Cancel = true;
		}

		protected virtual void EPCompanyTreeMaster_BypassEscalation_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPCompanyTreeMaster row = e.Row as EPCompanyTreeMaster;
			if (row == null) return;
			if (row.BypassEscalation == true)
				row.WaitTime = 0;
		}

		protected virtual void EPCompanyTreeMaster_Description_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPCompanyTreeMaster item = PXSelect
				<EPCompanyTreeMaster, Where<EPCompanyTreeMaster.description, Equal<Required<EPCompanyTreeMaster.description>>>>.
				SelectWindowed(this, 0, 1, e.NewValue);
			if (item != null)
				throw new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded, "Description");
		}

		protected virtual void EPCompanyTreeMaster_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPCompanyTreeMaster row = e.Row as EPCompanyTreeMaster;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<EPCompanyTree.waitTime>(sender, row,
				row.BypassEscalation != true && row.ParentWGID != 0);
			PXUIFieldAttribute.SetEnabled<EPCompanyTree.bypassEscalation>(sender, row, row.ParentWGID != 0);
		}

		protected virtual void EPCompanyTreeMaster_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			EPCompanyTreeMaster row = e.Row as EPCompanyTreeMaster;
			if (row == null) return;
			insertHRecords(row);
		}

		protected virtual void EPCompanyTreeMaster_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			EPCompanyTreeMaster row = e.Row as EPCompanyTreeMaster, oldRow = e.OldRow as EPCompanyTreeMaster;
			if (row == null || oldRow == null) return;

			if (row.ParentWGID == 0 || row.BypassEscalation == true)
				row.WaitTime = 0;

			if (row.ParentWGID != oldRow.ParentWGID)
			{
				updateHRecordParent(row);
			}
			else if (row.WaitTime != oldRow.WaitTime)
			{
				updateHRecordTime(row.WorkGroupID,
					row.WaitTime.GetValueOrDefault() - oldRow.WaitTime.GetValueOrDefault());
			}
		}

		protected virtual void EPCompanyTreeMaster_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			this.CurrentSelected.WorkGroupID = null;
			EPCompanyTreeMaster row = e.Row as EPCompanyTreeMaster;
			if (row == null || row.WorkGroupID == null) return;

			deleteHRecords(row.WorkGroupID);
			deleteRecurring(row);
		}

		public virtual void EPCompanyTreeMaster_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			EPCompanyTreeMaster row = (EPCompanyTreeMaster)e.Row;
			if (row == null) return;

			EPAssignmentRoute route = PXSelect<EPAssignmentRoute,
										Where<EPAssignmentRoute.workgroupID, Equal<Required<EPCompanyTreeMaster.workGroupID>>>>.
										Select(this, row.WorkGroupID);

			if (route != null)
			{
				EPAssignmentMap map = PXSelect<EPAssignmentMap,
										Where<EPAssignmentMap.assignmentMapID, Equal<Required<EPAssignmentRoute.assignmentMapID>>>>.
										Select(this, route.AssignmentMapID);
				if (map == null)
					throw new PXException(Objects.EP.Messages.WorkgroupIsInUse);
				throw new PXException(Objects.EP.Messages.WorkgroupIsInUseAtAssignmentMap, map.Name);
			}
		}

		protected virtual void EPCompanyTreeMember_UserID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			EPCompanyTreeMember doc = e.Row as EPCompanyTreeMember;
			if (doc == null) return;
			Guid pkid = new Guid();
			if (!Guid.TryParse(e.NewValue.ToString(), out pkid))
				throw new PXSetPropertyException(PX.Objects.EP.Messages.UserCannotBeFound);
		}

		private void deleteRecurring(EPCompanyTreeMaster map)
		{
			if (map != null)
			{
				foreach (EPCompanyTreeMaster child in PXSelect<EPCompanyTreeMaster,
				 Where<EPCompanyTreeMaster.parentWGID,
					Equal<Required<EPCompanyTreeMaster.workGroupID>>>>.Select(this, map.WorkGroupID))
					deleteRecurring(child);
				Items.Cache.Delete(map);
			}
		}

		#region Moving Actions

		public PXAction<EPCompanyTreeMaster> UpdateTree;
		[PXUIField(DisplayName = "Update Tree", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXProcessButton]
		public virtual IEnumerable updateTree(PXAdapter adapter)
		{
			foreach (EPCompanyTreeH h in this.treeH.Select())
				this.treeH.Delete(h);

			AddRecursive(0);
			return adapter.Get();
		}
		private void AddRecursive(int? parentID)
		{
			if (parentID == null) return;
			foreach (EPCompanyTreeMaster t in PXSelect<EPCompanyTreeMaster,
					Where<EPCompanyTreeMaster.parentWGID, Equal<Required<EPCompanyTreeMaster.parentWGID>>>>.Select(this, parentID))
			{
				insertHRecords(t);
				AddRecursive(t.WorkGroupID);
			}
		}

		public PXSelect<EPCompanyTreeMaster, Where<EPCompanyTreeMaster.workGroupID, Equal<Required<EPCompanyTreeMaster.workGroupID>>>> Item;
		public PXAction<EPCompanyTreeMaster> down;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown)]
		public virtual IEnumerable Down(PXAdapter adapter)
		{
			int currentItemIndex;
			PXResultset<EPCompanyTreeMaster> items =
				SelectSiblings(CurrentSelected.FolderID, CurrentSelected.WorkGroupID, out currentItemIndex);

			if (currentItemIndex >= 0 && currentItemIndex < items.Count - 1)
			{
				EPCompanyTreeMaster current = items[currentItemIndex];
				EPCompanyTreeMaster next = items[currentItemIndex + 1];

				current.SortOrder += 1;
				next.SortOrder -= 1;

				Items.Update(current);
				Items.Update(next);
			}
			return adapter.Get();
		}

		public PXAction<EPCompanyTreeMaster> up;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowUp)]
		public virtual IEnumerable Up(PXAdapter adapter)
		{
			int currentItemIndex;
			PXResultset<EPCompanyTreeMaster> items =
				SelectSiblings(CurrentSelected.FolderID, CurrentSelected.WorkGroupID, out currentItemIndex);
			if (currentItemIndex > 0)
			{
				EPCompanyTreeMaster current = items[currentItemIndex];
				EPCompanyTreeMaster prev = items[currentItemIndex - 1];

				current.SortOrder -= 1;
				prev.SortOrder += 1;

				Items.Update(current);
				Items.Update(prev);
			}
			return adapter.Get();
		}

		public PXAction<EPCompanyTreeMaster> left;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowLeft)]
		public virtual IEnumerable Left(PXAdapter adapter)
		{
			EPCompanyTreeMaster current = Item.SelectWindowed(0, 1, CurrentSelected.FolderID);

			if (current != null && current.ParentWGID != 0)
			{
				EPCompanyTreeMaster parent = Item.SelectWindowed(0, 1, current.ParentWGID);
				if (parent != null)
				{
					int parentIndex;
					PXResultset<EPCompanyTreeMaster> items = SelectSiblings(parent.ParentWGID, parent.WorkGroupID, out parentIndex);
					if (parentIndex >= 0)
					{
						EPCompanyTreeMaster last = items[items.Count - 1];
						current = (EPCompanyTreeMaster)Items.Cache.CreateCopy(current);
						current.ParentWGID = parent.ParentWGID;
						current.SortOrder = last.SortOrder + 1;
						Items.Update(current);
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<EPCompanyTreeMaster> right;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowRight)]
		public virtual IEnumerable Right(PXAdapter adapter)
		{
			EPCompanyTreeMaster current = Item.SelectWindowed(0, 1, CurrentSelected.FolderID);
			if (current != null)
			{
				int currentItemIndex;
				PXResultset<EPCompanyTreeMaster> items =
					SelectSiblings(current.ParentWGID, current.WorkGroupID, out currentItemIndex);
				if (currentItemIndex > 0)
				{
					EPCompanyTreeMaster prev = items[currentItemIndex - 1];
					items = SelectSiblings(prev.WorkGroupID);
					int index = 1;
					if (items.Count > 0)
					{
						EPCompanyTreeMaster last = items[items.Count - 1];
						index = (last.SortOrder ?? 0) + 1;
					}
					current = (EPCompanyTreeMaster)Items.Cache.CreateCopy(current);
					current.ParentWGID = prev.WorkGroupID;
					current.SortOrder = index;
					Items.Update(current);
				}
			}
			return adapter.Get();
		}

		public PXAction<EPCompanyTreeMaster> viewEmployee;
		[PXUIField(DisplayName = "View Employee", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void ViewEmployee()
		{
			EmployeeMaint graph = CreateInstance<EmployeeMaint>();
			graph.Employee.Current = graph.Employee.Search<EPEmployee.userID>(Members.Current.UserID);
			throw new PXRedirectRequiredException(graph, "ViewEmployee");
		}

		private PXResultset<EPCompanyTreeMaster> SelectSiblings(int? patentWGID)
		{
			int currentIndex;
			return SelectSiblings(patentWGID, 0, out currentIndex);
		}

		private PXResultset<EPCompanyTreeMaster> SelectSiblings(int? parentWGID, int? workGroupID, out int currentIndex)
		{
			currentIndex = -1;
			if (parentWGID == null) return null;
			PXResultset<EPCompanyTreeMaster> items = this.Items.Select(parentWGID);

			int i = 0;
			foreach (EPCompanyTreeMaster item in items)
			{
				if (item.WorkGroupID == workGroupID)
					currentIndex = i;
				item.SortOrder = i + 1;
				Items.Update(item);
				i += 1;
			}
			return items;
		}
		#endregion

		#region Members events
		protected virtual IEnumerable members(
			[PXInt]
			int? WorkGroupID)
		{
			this.Members.Cache.AllowInsert = (WorkGroupID != null);
			CurrentSelected.WorkGroupID = WorkGroupID;
			foreach (PXResult<EPCompanyTreeMember, EPEmployee, EPEmployeePosition, Users> res in PXSelectJoin<EPCompanyTreeMember,
				LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>,
				LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>, And<EPEmployeePosition.isActive, Equal<True>>>,
				LeftJoin<Users, On<Users.pKID, Equal<EPCompanyTreeMember.userID>>>>>,
				Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>>>.Select(this, WorkGroupID))
			{
				EPCompanyTreeMember member = res;
				EPEmployee emp = res;
				EPEmployeePosition pos = res;
				Users user = res;

				if (string.IsNullOrEmpty(emp.AcctCD))
				{
					Members.Cache.RaiseExceptionHandling<EPCompanyTreeMember.userID>(member, null, new PXSetPropertyException(PX.Objects.EP.Messages.UserWithoutEmployee, PXErrorLevel.RowWarning, user.Username));
				}
				yield return new PXResult<EPCompanyTreeMember, EPEmployee, EPEmployeePosition>(member, emp, pos);
			}
		}
		protected virtual void EPCompanyTreeMember_WorkGroupID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPCompanyTreeMember row = e.Row as EPCompanyTreeMember;
			if (row == null) return;
			e.NewValue = this.CurrentSelected.WorkGroupID ?? 0;
			e.Cancel = true;
		}

		protected virtual void EPCompanyTreeMember_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			UpdateOwner(e.Row);
		}

		protected virtual void EPCompanyTreeMember_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			UpdateOwner(e.Row);
		}

		protected virtual void EPCompanyTreeMember_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			EPCompanyTreeMember row = e.Row as EPCompanyTreeMember;
			if (row == null || row.WorkGroupID == null) return;

			Items.Cache.IsDirty = true;
		}

		private void UpdateOwner(object row)
		{
			EPCompanyTreeMember curr = (EPCompanyTreeMember)row;
			bool isRefreshNeeded = false;

			if (curr.IsOwner == true)
			{
				foreach (EPCompanyTreeMember item in Members.Select(curr.WorkGroupID))
				{
					if (item.IsOwner != true || item.UserID == curr.UserID) continue;

					item.IsOwner = false;
					Members.Cache.Update(item);
					isRefreshNeeded = true;
				}
			}
			if (isRefreshNeeded)
				Members.View.RequestRefresh();
		}


		#endregion

		#region Extended Tree
		public PXSelect<EPCompanyTreeH> treeH;
		public PXSelect<EPCompanyTreeH,
			Where<EPCompanyTreeH.workGroupID, Equal<Required<EPCompanyTreeH.workGroupID>>>,
			OrderBy<Desc<EPCompanyTreeH.parentWGLevel>>> parents;
		public PXSelect<EPCompanyTreeH,
			Where<EPCompanyTreeH.parentWGID, Equal<Required<EPCompanyTreeH.parentWGID>>>,
			OrderBy<Asc<EPCompanyTreeH.workGroupLevel>>> childrens;

		private void insertHRecords(EPCompanyTreeMaster record)
		{
			int level = -1;
			foreach (EPCompanyTreeH item in parents.Select(record.ParentWGID))
			{
				if (level == -1)
					level = item.WorkGroupLevel.GetValueOrDefault() + 1;
				EPCompanyTreeH ins = (EPCompanyTreeH)treeH.Cache.CreateCopy(item);
				ins.WorkGroupID = record.WorkGroupID;
				ins.WorkGroupLevel = level;
				ins.WaitTime += record.WaitTime;
				treeH.Cache.Insert(ins);
			}
			EPCompanyTreeH self = new EPCompanyTreeH();
			self.WorkGroupID = record.WorkGroupID;
			self.WorkGroupLevel = level;
			self.ParentWGID = record.WorkGroupID;
			self.ParentWGLevel = level;
			self.WaitTime = 0;
			treeH.Cache.Insert(self);
		}

		private void deleteHRecords(int? workGroupID)
		{
			foreach (EPCompanyTreeH item in parents.Select(workGroupID))
				treeH.Cache.Delete(item);
		}

		private void updateHRecordTime(int? workGroupID, int delta)
		{
			foreach (EPCompanyTreeH child in childrens.Select(workGroupID))
			{
				foreach (EPCompanyTreeH parent in parents.Select(child.WorkGroupID))
				{
					if (parent.WorkGroupID != parent.ParentWGID &&
							parent.ParentWGLevel < child.ParentWGLevel)
					{
						parent.WaitTime += delta;
						treeH.Cache.Update(parent);
					}
				}
			}
		}
		private void updateHRecordParent(EPCompanyTreeMaster item)
		{
			PXResultset<EPCompanyTreeH> newParents = parents.Select(item.ParentWGID);
			int parentLevel = -1;
			if (newParents.Count > 0)
			{
				EPCompanyTreeH newParent = newParents[newParents.Count - 1];
				parentLevel = newParent.WorkGroupLevel.GetValueOrDefault();
			}

			int levelDelta = 0;
			EPCompanyTreeH root = null;
			foreach (EPCompanyTreeH child in childrens.Select(item.WorkGroupID))
			{
				if (root == null)
				{
					root = (EPCompanyTreeH)treeH.Cache.CreateCopy(child);
					levelDelta = parentLevel + 1 - child.WorkGroupLevel.GetValueOrDefault();
				}

				foreach (EPCompanyTreeH parent in parents.Select(child.WorkGroupID))
				{
					if (parent.WorkGroupID != parent.ParentWGID &&
							parent.ParentWGLevel < root.WorkGroupLevel)
					{
						treeH.Cache.Delete(parent);
					}
					else
					{
						EPCompanyTreeH upd = (EPCompanyTreeH)treeH.Cache.CreateCopy(parent);
						upd.WorkGroupLevel += levelDelta;
						upd.ParentWGLevel += levelDelta;
						treeH.Cache.Update(upd);
					}
				}

				foreach (EPCompanyTreeH parent in newParents)
				{
					EPCompanyTreeH ins = (EPCompanyTreeH)treeH.Cache.CreateCopy(parent);
					ins.WorkGroupID = child.WorkGroupID;
					ins.WorkGroupLevel = child.WorkGroupLevel;
					ins.WaitTime += child.WaitTime + item.WaitTime;
					treeH.Cache.Insert(ins);
				}
			}
		}

		#endregion

		private SelectedNode CurrentSelected
		{
			get
			{
				PXCache cache = this.Caches[typeof(SelectedNode)];
				if (cache.Current == null)
				{
					cache.Insert();
					cache.IsDirty = false;
				}
				return (SelectedNode)cache.Current;
			}
		}
	}
}
