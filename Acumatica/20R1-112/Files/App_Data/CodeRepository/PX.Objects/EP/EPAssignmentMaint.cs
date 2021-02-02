#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PX.Data;
using PX.Objects.GL;
using PX.TM;
using PX.SM;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.RQ;
using PX.Objects.CA;

#endregion

namespace PX.Objects.EP
{
	public class EPAssignmentMaint : PXGraph<EPAssignmentMaint>
	{		
		#region Main Actions
		public PXSave<EPAssignmentMap> Save;
		public PXCancel<EPAssignmentMap> Cancel;
		public PXInsert<EPAssignmentMap> Insert;
		public PXDelete<EPAssignmentMap> Delete;		
		#endregion
		public PXSelect<BAccount> bAccount;
		public PXSelect<Vendor> vendor;
		public PXSelect<Customer> customer;
		public PXSelect<EPEmployee> employee;

		[PXViewName(Messages.AssignmentMap)]
		public PXSelect<EPAssignmentMap, Where<EPAssignmentMap.mapType, IsNull, Or<EPAssignmentMap.mapType, Equal<Zero>>>> AssigmentMap;
		public PXSetup<Company> company;
		public PXSetup<EPSetup> setup;
		
		public PXAction<EPAssignmentMap> down;

		public PXSelectJoin<EPAssignmentRoute,
			LeftJoin<EPCompanyTree, On<EPAssignmentRoute.workgroupID, Equal<EPCompanyTree.workGroupID>>>, Where<EPAssignmentRoute.assignmentRouteID, IsNotNull,
			And<EPAssignmentRoute.parent, Equal<Argument<int?>>>>, OrderBy<Asc<EPAssignmentRoute.sequence>>>
			Nodes;

		public PXSelectJoin<EPAssignmentRoute,
			LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPAssignmentRoute.ownerID>>>,
			Where<EPAssignmentRoute.sequence, IsNotNull>, OrderBy<Asc<EPAssignmentRoute.sequence>>>
			Items;

		public PXSelect<EPAssignmentRoute, Where<EPAssignmentRoute.assignmentRouteID, Equal<Current<EPAssignmentRoute.assignmentRouteID>>>>CurrentItem;

		public PXFilter<Position> PositionFilter;

		public PXSelect<EPAssignmentRule> Rules;
		public PXAction<EPAssignmentMap> up;

	    public PXSelect<EPEmployee> Employee;

        [PXHidden]
        public PXSelect<CT.Contract> Dummy_Contract; //Cache Inheritance issues
        [PXHidden]
        public PXSelect<PM.PMProject> Dummy_Project; //Cache Inheritance issues

        private const int RootNodeID = -99;

		public EPAssignmentMaint()
		{
			this.Views.Caches.Remove(bAccount.Cache.GetItemType());
			this.Views.Caches.Remove(vendor.Cache.GetItemType());
			this.Views.Caches.Remove(customer.Cache.GetItemType());
			this.Views.Caches.Remove(employee.Cache.GetItemType());
		}
		protected virtual IEnumerable nodes([PXDBInt] int? assignmentRouteID)
		{
			List<EPAssignmentRoute> list = new List<EPAssignmentRoute>();

			if (!assignmentRouteID.HasValue)
			{
				EPAssignmentRoute root = new EPAssignmentRoute();
				root.AssignmentRouteID = RootNodeID;
				root.AssignmentMapID = PositionFilter.Current.MapID;

				root.Name = this.Accessinfo.CompanyName ?? CR.Messages.Company;

				root.Icon = PX.Web.UI.Sprite.Main.GetFullUrl(PX.Web.UI.Sprite.Main.Folder);
				list.Add(root);
			}
			else
			{
				IEnumerable resultSet;
				if (assignmentRouteID == RootNodeID)
				{
					resultSet = PXSelectJoin
					<EPAssignmentRoute, LeftJoin<EPCompanyTree, On<EPAssignmentRoute.workgroupID, Equal<EPCompanyTree.workGroupID>>>,
						Where<EPAssignmentRoute.assignmentMapID, Equal<Current<Position.mapID>>,
							And<EPAssignmentRoute.parent, IsNull>>>.Select(this);
				}
				else
				{
					resultSet = PXSelectJoin
					<EPAssignmentRoute, LeftJoin<EPCompanyTree, On<EPAssignmentRoute.workgroupID, Equal<EPCompanyTree.workGroupID>>>,
						Where<EPAssignmentRoute.assignmentMapID, Equal<Current<Position.mapID>>,
							And<EPAssignmentRoute.parent, Equal<Required<EPAssignmentRoute.parent>>>>>.Select(this, assignmentRouteID);
				}

				foreach (PXResult<EPAssignmentRoute, EPCompanyTree> record in resultSet)
				{
					EPAssignmentRoute route = record;

					if (route.RouterType == EPRouterType.Workgroup)
					{
						route.Icon = PX.Web.UI.Sprite.Main.GetFullUrl(PX.Web.UI.Sprite.Main.Roles);
					}
					else if (route.RouteID.HasValue)
					{
						route.Icon = PX.Web.UI.Sprite.Main.GetFullUrl(PX.Web.UI.Sprite.Main.Redo);
					}
					else if (route.OwnerID.HasValue)
					{
						route.Icon = PX.Web.UI.Sprite.Main.GetFullUrl(PX.Web.UI.Sprite.Main.Users);
					}
					else
					{
						route.Icon = PX.Web.UI.Sprite.Main.GetFullUrl(PX.Web.UI.Sprite.Main.Folder);
					}					
					list.Add(route);
				}
			}

			PositionFilter.Current.UseCurrentTreeItem = PXView.Searches != null && PXView.Searches.Length > 0 && PXView.Searches[0] != null;

			return list;
		}

		protected virtual IEnumerable items([PXDBInt] int? parent)
		{
			if ((IsExport || IsImport) && parent == null && Nodes.Current != null && !PXGraph.ProxyIsActive)
			{
				parent = Nodes.Current.AssignmentRouteID;
			}

			PositionFilter.Current.NodeID = parent;

			if (parent == RootNodeID)
				PositionFilter.Current.NodeID = null;

			PXResultset<EPAssignmentRoute> resultSet;
			if (parent == null || parent == RootNodeID)
			{
				resultSet = PXSelectJoin<EPAssignmentRoute,
				LeftJoin<EPCompanyTree, On<EPAssignmentRoute.workgroupID, Equal<EPCompanyTree.workGroupID>>,
				LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPAssignmentRoute.ownerID>>>>,
					Where<EPAssignmentRoute.assignmentMapID, Equal<Current<Position.mapID>>,
						And<EPAssignmentRoute.parent, IsNull>>>.Select(this);
			}
			else
			{
				resultSet = PXSelectJoin<EPAssignmentRoute,
					LeftJoin<EPCompanyTree, On<EPAssignmentRoute.workgroupID, Equal<EPCompanyTree.workGroupID>>,
					LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPAssignmentRoute.ownerID>>>>,
					Where<EPAssignmentRoute.assignmentMapID, Equal<Current<Position.mapID>>,
						And<EPAssignmentRoute.parent, Equal<Required<EPAssignmentRoute.parent>>>>>.Select(this, parent);
			}

			//EPAssignmentRoute selectedNode = PXSelect<EPAssignmentRoute>.Search<EPAssignmentRoute.iD>(this, parent);
			//if (selectedNode != null && (selectedNode.WorkgroupID != null || selectedNode.RouteID != null))
			//{
			//    this.Items.Cache.AllowInsert = false;
			//    this.Items.Cache.AllowUpdate = false;
			//    this.Items.Cache.AllowDelete = false;
			//}
			//else
			//{

			//    this.Items.Cache.AllowInsert = true;
			//    this.Items.Cache.AllowUpdate = true;
			//    this.Items.Cache.AllowDelete = true;
			//}

			this.Rules.Cache.AllowInsert = resultSet.Count > 0;
			this.Rules.Cache.AllowUpdate = resultSet.Count > 0;
			this.Rules.Cache.AllowDelete = resultSet.Count > 0;

			foreach (PXResult<EPAssignmentRoute, EPCompanyTree> record in resultSet)
			{
				yield return record;
			}
		}

		protected virtual IEnumerable rules([PXDBInt] int? routeID)
		{
			if ((IsExport || IsImport) && routeID == null && Items.Current != null && !PXGraph.ProxyIsActive && PositionFilter.Current.NodeID != Items.Current.AssignmentRouteID)
			{
				routeID = Items.Current.AssignmentRouteID;
			}

			PositionFilter.Current.ItemID = routeID;

			List<EPAssignmentRule> list = new List<EPAssignmentRule>();

			PXResultset<EPAssignmentRule> resultSet;

			if (routeID == null)
			{
				resultSet = PXSelect<EPAssignmentRule,
					Where<EPAssignmentRule.assignmentRouteID, IsNull>>.Select(this);
			}
			else
			{
				resultSet = PXSelect<EPAssignmentRule,
					Where<EPAssignmentRule.assignmentRouteID, Equal<Required<EPAssignmentRoute.parent>>>
					>.Select(this, routeID);
			}

			foreach (EPAssignmentRule item in resultSet)
			{
				list.Add(item);
			}

			return list;
		}

		#region Tree Selector Entity
		public PXSelect<CacheEntityItem,
			Where<CacheEntityItem.path, Equal<CacheEntityItem.path>>,
			OrderBy<Asc<CacheEntityItem.number>>> EntityItems;

		protected IEnumerable entityItems(string parent)
		{
			if (this.AssigmentMap.Current == null)
				yield break;
			Type type = GraphHelper.GetType(this.AssigmentMap.Current.EntityType);;
		    Type graphType;
		    if (AssigmentMap.Current.GraphType == null)
		    {
                if (type == null && parent != null) yield break;
                graphType = EntityHelper.GetPrimaryGraphType(this, type);
		    }
		    else
		    {
		        graphType = GraphHelper.GetType(this.AssigmentMap.Current.GraphType);
		    }

			foreach (CacheEntityItem e in EMailSourceHelper.TemplateEntity(this, parent, type.FullName, graphType != null ? graphType.FullName : null))
			{				
				yield return e;
			}
		}

		#endregion

		private IEnumerable<EntityItemSource> GraphTypeList()
		{
			if (this.AssigmentMap.Current == null)
				yield break;

			var aMap = AssigmentMap.Current;
			EPAssignmentRoute route = PXSelect<EPAssignmentRoute,
					Where<EPAssignmentRoute.assignmentMapID, Equal<Required<EPAssignmentRoute.assignmentMapID>>>>
					.Select(this, aMap.AssignmentMapID);
			if (route != null && aMap.EntityType != null)
			{
				Type graphType;

				if (aMap.GraphType != null)
					graphType = GraphHelper.GetType(aMap.GraphType);
				else
				{
					Type entityType = System.Web.Compilation.PXBuildManager.GetType(aMap.EntityType, false);
					graphType = EntityHelper.GetPrimaryGraphType(this, entityType);
					aMap.GraphType = graphType.FullName;
				}

				if (graphType != null)
				{
					PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeUnsecure(graphType);
					if (node != null)
						yield return new EntityItemSource(node, aMap.GraphType);
				}
				else
				{
					foreach (var e in EMailSourceHelper.TemplateScreens(this, null, null))
					{
						yield return e;
					}
				}

			}
			else
			{
				foreach (EntityItemSource e in EMailSourceHelper.TemplateScreensByCondition(this, null, null,
					type => type.IsDefined(typeof(PXEMailSourceAttribute), true) && typeof(PX.Data.EP.IAssign).IsAssignableFrom(type)))
				{
					if (!String.IsNullOrEmpty(e.ScreenID))
					{
						PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(e.ScreenID);
						if (node != null && !String.IsNullOrEmpty(node.GraphType))
						{
							e.SubKey = node.GraphType;
							yield return e;
						}
					}
				}
			}
		}

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowUp)]
		public virtual IEnumerable Up(PXAdapter adapter)
		{
			IList<EPAssignmentRoute> routes = GetSortedItems();

			int? selectedItem = Items.Current.AssignmentRouteID;
			int currentItemIndex = 0;
			for (int i = 0; i < routes.Count; i++)
			{
				if (routes[i].AssignmentRouteID == selectedItem)
				{
					currentItemIndex = i;
				}

				routes[i].Sequence = i + 1;
				Items.Update(routes[i]);
			}

			if (currentItemIndex > 0)
			{
				routes[currentItemIndex].Sequence--;
				routes[currentItemIndex - 1].Sequence++;

				Items.Update(routes[currentItemIndex]);
				Items.Update(routes[currentItemIndex - 1]);
			}

			return adapter.Get();
		}

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown)]
		public virtual IEnumerable Down(PXAdapter adapter)
		{
			IList<EPAssignmentRoute> routes = GetSortedItems();

			int? selectedItem = Items.Current.AssignmentRouteID;
			int currentItemIndex = 0;
			for (int i = 0; i < routes.Count; i++)
			{
				if (routes[i].AssignmentRouteID == selectedItem)
				{
					currentItemIndex = i;
				}

				routes[i].Sequence = i + 1;
				Items.Update(routes[i]);
			}

			if (currentItemIndex < routes.Count - 1)
			{
				routes[currentItemIndex].Sequence++;
				routes[currentItemIndex + 1].Sequence--;

				Items.Update(routes[currentItemIndex]);
				Items.Update(routes[currentItemIndex + 1]);
			}

			return adapter.Get();
		}

		#region Event Handlers
		[PXDBString(255)]
		protected virtual void EPAssignmentMap_EntityType_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXStringList()]
		protected virtual void EPAssignmentMap_GraphType_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<EPAssignmentRoute.routerType>))]
		protected virtual void EPAssignmentRoute_WorkgroupID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void EPAssignmentRoute_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			EPAssignmentRoute row = (EPAssignmentRoute)e.NewRow;
			EPAssignmentRoute oldrow = (EPAssignmentRoute)e.Row;
			if(row.Sequence != oldrow.Sequence && e.ExternalCall)
			{				
				if(oldrow.Sequence < row.Sequence)
					UpdateSequence(sender, row, oldrow.Sequence+1, row.Sequence, -1);				
				else
					UpdateSequence(sender, row, row.Sequence, oldrow.Sequence, +1);
				this.AssigmentMap.View.RequestRefresh();
			}
		}
		private void UpdateSequence(PXCache sender,EPAssignmentRoute route, int? from, int? to, int step)
		{
			foreach (EPAssignmentRoute r in PXSelect<EPAssignmentRoute,
				Where<EPAssignmentRoute.assignmentMapID, Equal<Required<EPAssignmentRoute.assignmentMapID>>,
					And<EPAssignmentRoute.sequence,
						Between<Required<EPAssignmentRoute.sequence>, Required<EPAssignmentRoute.sequence>>>>>
				.Select(this, route.AssignmentMapID, from, to))
			{
				if (r.AssignmentRouteID != route.AssignmentRouteID)
				{
					r.Sequence += step;
					sender.SetStatus(r, PXEntryStatus.Updated);
				}
			}
		}

		protected virtual void EPAssignmentRoute_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			EPAssignmentRoute row = e.Row as EPAssignmentRoute;
			if (row != null && AssigmentMap.Current != null)
			{
				IEnumerable list = items(PositionFilter.Current.NodeID);

				int maxSequence = 0;
				foreach (PXResult<EPAssignmentRoute, EPCompanyTree> item in list)
				{
					if (((EPAssignmentRoute)item[0]).Sequence.Value > maxSequence)
						maxSequence = ((EPAssignmentRoute)item[0]).Sequence.Value;
				}

				if (IsImport && !PXGraph.ProxyIsActive)
				{
					if (PositionFilter.Current.UseCurrentTreeItem == true && Nodes.Current != null)
					{
						PositionFilter.Current.RouteParentID = Nodes.Current.AssignmentRouteID != RootNodeID ? Nodes.Current.AssignmentRouteID : null;
					}
					PositionFilter.Current.UseCurrentTreeItem = false;
					row.Parent = PositionFilter.Current.RouteParentID;
				}
				else
				{
					row.Parent = PositionFilter.Current.NodeID;
				}

				row.Sequence = maxSequence + 1;
				row.AssignmentMapID = AssigmentMap.Current.AssignmentMapID;
			}
		}
		protected virtual void EPAssignmentRoute_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			EPAssignmentRoute row = e.Row as EPAssignmentRoute;
			if (row != null && AssigmentMap.Current != null)
			{
				PositionFilter.Current.NodeID = row.RouteID;
				PositionFilter.Current.RouteItemID = row.AssignmentRouteID;
			}
		}

        protected virtual void EPAssignmentRoute_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            // \en This check for processing sequence only parent nodes without childs.
            if (sender.Current == null) return;
            IList<EPAssignmentRoute> routes = GetSortedItems();
            int i = 1;
            foreach (EPAssignmentRoute route in routes)
            {
                route.Sequence = i++;
                sender.SetStatus(route, PXEntryStatus.Updated);
            }
            Items.View.RequestRefresh();
        }

		protected virtual void EPAssignmentRoute_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPAssignmentRoute row = e.Row as EPAssignmentRoute;
			if (row == null) return;

			this.Rules.Cache.AllowInsert = e.Row != null && row.Sequence != null;
			this.Rules.Cache.AllowUpdate = e.Row != null && row.Sequence != null;
			this.Rules.Cache.AllowDelete = e.Row != null && row.Sequence != null;

			PXUIFieldAttribute.SetEnabled<EPAssignmentRoute.workgroupID>(sender, row, row == null || row.RouterType == EPRouterType.Workgroup);
			PXUIFieldAttribute.SetEnabled<EPAssignmentRoute.ownerID>(sender, row, row == null || row.RouterType == EPRouterType.Workgroup);
			PXUIFieldAttribute.SetEnabled<EPAssignmentRoute.routeID>(sender, row, row != null && row.RouterType == EPRouterType.Router);			
		}

		private EPAssignmentRoute current;
		protected virtual void EPAssignmentRoute_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPAssignmentRoute row = e.Row as EPAssignmentRoute;
			if (row == null || e.Operation == PXDBOperation.Delete) return;

			PXDefaultAttribute.SetPersistingCheck<EPAssignmentRoute.routeID>(sender, e.Row,
			                                                                 row.RouterType == EPRouterType.Router ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			if (e.Operation == PXDBOperation.Insert &&
			    PositionFilter.Current.MapID == row.AssignmentMapID &&
			    (PositionFilter.Current.NodeID == row.AssignmentRouteID || PositionFilter.Current.NodeID == null))
			{
				current = row;
			}
		}
		
		protected virtual void EPAssignmentRoute_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			EPAssignmentRoute row = (EPAssignmentRoute)e.Row;
			if(row == current && e.TranStatus == PXTranStatus.Completed)
			{
				PositionFilter.Current.MapID = row.AssignmentMapID;
				PositionFilter.Current.NodeID = row.AssignmentRouteID;
			}
		}

	    protected virtual void EPAssignmentMap_GraphType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
	    {
	        EPAssignmentMap row = e.Row as EPAssignmentMap;
            if (row != null && row.GraphType != null)
            {
                Type type = GraphHelper.GetType(row.GraphType);
                var graph = PXGraph.CreateInstance(type);
                Type primary = graph.Views[graph.PrimaryView].Cache.GetItemType();
                row.EntityType = primary.FullName;
            }
	    }

		protected virtual void EPAssignmentMap_EntityType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null)
			{
				Type type = GraphHelper.GetType((string)e.NewValue);
				if (type == null)
					throw new PXSetPropertyException(Messages.AssigmentMapEntityType, PXErrorLevel.Error);
			}
		}

		protected virtual void EPAssignmentMap_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			//EPSetup ep = setup.Current;
			EPAssignmentMap row = e.Row as EPAssignmentMap;
			if (row != null)
			{
				if (row.EntityType != null && row.GraphType == null)
				{
					Type entityType = System.Web.Compilation.PXBuildManager.GetType(row.EntityType, false);
					var primary = (entityType == null) ? null : EntityHelper.GetPrimaryGraphType(this, entityType);
					if (primary != null)
					{
						AssigmentMap.Current.GraphType = primary.FullName;
						AssigmentMap.Cache.SetStatus(AssigmentMap.Current, PXEntryStatus.Updated);
						AssigmentMap.Cache.IsDirty = true;
					}
				}
				PositionFilter.Current.MapID = row.AssignmentMapID;
				this.Nodes.Cache.AllowInsert = row.EntityType != null;
				EPAssignmentRoute route = PXSelect<EPAssignmentRoute,
						Where<EPAssignmentRoute.assignmentMapID, Equal<Required<EPAssignmentRoute.assignmentMapID>>>>
					.Select(this, row.AssignmentMapID);
				PXUIFieldAttribute.SetEnabled<EPAssignmentMap.graphType>(sender, row, route == null);

				var list = GraphTypeList().ToList().OrderBy(o => o.Name);
				PXStringListAttribute.SetLocalizable<EPAssignmentMap.graphType>(sender, null, false);
				PXStringListAttribute.SetList<EPAssignmentMap.graphType>(sender, row,
					list.Select(x => x.SubKey).ToArray(), list.Select(x => x.Name).ToArray());
			}
		}		
		protected virtual void EPAssignmentRule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPAssignmentRule row = e.Row as EPAssignmentRule;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<EPAssignmentRule.fieldValue>(sender, row, 
				row.Condition != (int)PXCondition.ISNULL && row.Condition != (int)PXCondition.ISNOTNULL);		
		}

		protected virtual void EPAssignmentRule_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			EPAssignmentRule row = e.Row as EPAssignmentRule;
			if (row != null)
			{
				if (!IsImport)
					row.AssignmentRouteID = PositionFilter.Current.ItemID;
				else if (PositionFilter.Current.RouteItemID != null && PositionFilter.Current.RouteItemID != row.AssignmentRouteID)
				{
					row.AssignmentRouteID = PositionFilter.Current.RouteItemID;
					Items.Current = Items.Locate(new EPAssignmentRoute { AssignmentMapID = PositionFilter.Current.MapID, AssignmentRouteID = row.AssignmentRouteID });
				}
			}
		}

		protected virtual void EPAssignmentRule_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPAssignmentRule oldRow = e.OldRow as EPAssignmentRule;
			EPAssignmentRule newRow = e.Row as EPAssignmentRule;
			if (oldRow != null && newRow != null)
			{
				if (!String.Equals(newRow.Entity, oldRow.Entity, StringComparison.OrdinalIgnoreCase)) newRow.FieldName = newRow.FieldValue = null;
				if (!String.Equals(newRow.FieldName, oldRow.FieldName, StringComparison.OrdinalIgnoreCase)) newRow.FieldValue = null;
				EPAssignmentRule row = e.Row as EPAssignmentRule;

				if (row.Condition == null || (PXCondition)row.Condition == PXCondition.ISNULL || (PXCondition)row.Condition == PXCondition.ISNOTNULL)
				{
					newRow.FieldValue = null;
				}
				if (newRow.FieldValue == null)
				{
					PXFieldState state = sender.GetStateExt<EPAssignmentRule.fieldValue>(newRow) as PXFieldState;
					newRow.FieldValue = state != null && state.Value != null ? state.Value.ToString() : null;
				}
			}
		}

		protected virtual void EPAssignmentRule_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Items.Cache.IsDirty = true;
		}
		
		protected virtual void EPAssignmentRule_Entity_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (AssigmentMap.Current != null)
				e.ReturnState = CreateFieldStateForEntity(e.ReturnValue, AssigmentMap.Current.EntityType, AssigmentMap.Current.GraphType);
		}
		
		protected virtual void EPAssignmentRule_FieldName_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			EPAssignmentRule row = e.Row as EPAssignmentRule;
			if (row != null && row.Entity != null) 
				e.ReturnState = CreateFieldStateForFieldName(e.ReturnState, AssigmentMap.Current.EntityType, AssigmentMap.Current.GraphType, row.Entity);
		}

		protected virtual void EPAssignmentRule_FieldValue_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			EPAssignmentRule row = e.Row as EPAssignmentRule;
		    if (row != null && !string.IsNullOrEmpty(row.FieldName) && row.Condition != null &&
		        (PXCondition) row.Condition != PXCondition.ISNULL && (PXCondition) row.Condition != PXCondition.ISNOTNULL)
		    {
                var fieldState = CreateFieldStateForFieldValue(e.ReturnState, AssigmentMap.Current.EntityType, row.Entity,
                    row.FieldName);
		        if (fieldState != null)
		            e.ReturnState = fieldState;
		    }
		}
		
		#endregion

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public override int ExecuteInsert(string viewName, IDictionary values, params object[] parameters)
		{
			return base.ExecuteInsert(viewName, values, parameters);
		}
		
		#region Utils

		private const string _FIELDNAME_STR = "FieldName";

		private PXFieldState CreateFieldStateForEntity(object returnState, string entityType, string graphType)
		{
			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();
			
			Type type = GraphHelper.GetType(entityType);
			if (type != null)
			{
				Type gType = EntityHelper.GetPrimaryGraphType(this, type);
			    if (!String.IsNullOrEmpty(graphType))
			        gType = GraphHelper.GetType(graphType);
                if (gType == null)
				{
					PXCacheNameAttribute[] a = (PXCacheNameAttribute[])type.GetCustomAttributes(typeof(PXCacheNameAttribute), true);

					if (type.IsSubclassOf(typeof(CS.CSAnswers)))
					{
						allowedValues.Add(type.FullName);
						allowedLabels.Add(a.Length > 0 ? a[0].Name : type.Name);
					}
				}
                else foreach (CacheEntityItem e in EMailSourceHelper.TemplateEntity(this, null, type.FullName, gType.FullName))
					{
						if (e.SubKey != typeof (CS.CSAnswers).FullName)
						{
							allowedValues.Add(e.SubKey);
							allowedLabels.Add(e.Name);
						}
					}
			}

			return PXStringState.CreateInstance(returnState, EPAssignmentRule.FieldLength, null, "Entity", false, 1, null,
												allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
		}

		private PXFieldState CreateFieldStateForFieldName(object returnState, string entityType, string graphType, string cacheName)
		{
			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();

			Type type = GraphHelper.GetType(entityType);			
			if (type != null)
			{

                Type gType = EntityHelper.GetPrimaryGraphType(this, type);
                if (!String.IsNullOrEmpty(graphType))
                    gType = GraphHelper.GetType(graphType);
				string viewName = null;
                if (gType != null)				
					foreach (
						CacheEntityItem view in
                            EMailSourceHelper.TemplateEntity(this, null, type.FullName, gType.FullName))
					{
						if (view.SubKey == cacheName)

						{ 
							viewName = view.Key;
							break;
						}
					}				

				Dictionary<string, string> fields = new Dictionary<string, string>();
				foreach (CacheEntityItem e in
                    EMailSourceHelper.TemplateEntity(this, viewName, type.FullName, gType != null ? gType.FullName : null, false))
				{
					fields[e.SubKey] = e.Name;
				}

				foreach (var item in fields.OrderBy(i => i.Value))
				{
					if (!item.Key.EndsWith("_Attributes") && item.Key.Contains('_')) continue;
					allowedValues.Add(item.Key);
					allowedLabels.Add(item.Value);
				}				
			}
            

			return PXStringState.CreateInstance(returnState, EPAssignmentRule.FieldLength, null, _FIELDNAME_STR, false, 1, null,
												allowedValues.ToArray(), allowedLabels.ToArray(), false, null);
		}

		private PXFieldState CreateFieldStateForFieldValue(object returnState, string entityType, string cacheName, string fieldName)
		{			
			Type type = GraphHelper.GetType(entityType);
			if (type != null)
			{
				Type cachetype = GraphHelper.GetType(cacheName);
				if (cachetype == null)
					return null;

				PXCache cache = this.Caches[cachetype];
				PXDBAttributeAttribute.Activate(cache);
				PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
				if(state != null) state.DescriptionName = null;
				
                var attr = cache.GetAttributes(null, fieldName);

                if (attr != null)
                {
                    var timeListAttribute = attr.FirstOrDefault(a => a is PXTimeListAttribute) as PXTimeListAttribute;
                    var intAttribute = attr.FirstOrDefault(a => a is PXIntAttribute) as PXIntAttribute;
                    
                    if (timeListAttribute != null && intAttribute != null)
                    {
                        state = PXTimeState.CreateInstance((PXIntState)state, null, null);
                        state.SelectorMode = PXSelectorMode.Undefined;
                    }
                }

                if (state != null)
				{
					if(returnState == null)
					{
						object item = cache.CreateInstance();
						object newValue;
						cache.RaiseFieldDefaulting(fieldName, item, out newValue);
						if (newValue != null)
							cache.RaiseFieldSelecting(fieldName, item, ref newValue, false);
						state.Value = newValue;
					}
					else
						state.Value = returnState;
					state.Enabled = true;
                    
                    PXView view;
				    if (state.ViewName != null && 
                        this.Views.TryGetValue(state.ViewName, out view) &&
                        view.BqlSelect.GetTables()[0] == typeof (EPEmployee))
                    {
                        state.ViewName = "Employee";
                    }

					if (attr != null)
					{
						var intListAttribute = attr.FirstOrDefault(a => a.GetType().IsSubclassOf(typeof(PXIntListAttribute))) as PXIntListAttribute;
						if (intListAttribute != null)
							return state;
					}

					state = PXFieldState.CreateInstance((state as PXStringState)?.AllowedValues != null ? state : state.Value, state.DataType, state.PrimaryKey, state.Nullable, state.Required == true ? 1 : state.Required == null ? 0 : -1, state.Precision, state.Length, state.DefaultValue, fieldName, 
						state.DescriptionName, state.DisplayName, state.Error, state.ErrorLevel, true, true, false, PXUIVisibility.Visible, state.ViewName, state.FieldList, state.HeaderList);
				}

				return state;
			}
			return null;			
		}

		private static int Comparison(EPAssignmentRoute x, EPAssignmentRoute y)
		{
			return x.Sequence.Value.CompareTo(y.Sequence.Value);
		}

		private IList<EPAssignmentRoute> GetSortedItems()
		{
			List<EPAssignmentRoute> routes = new List<EPAssignmentRoute>();
			if (CurrentItem.Current == null)
				return routes;

			foreach (EPAssignmentRoute item in Items.Cache.Cached)
			{
				if (item.Parent != CurrentItem.Current.Parent) continue;

				if (Items.Cache.GetStatus(item) != PXEntryStatus.Deleted &&
					Items.Cache.GetStatus(item) != PXEntryStatus.InsertedDeleted)
				{
					routes.Add(item);
				}
			}
			routes.Sort(Comparison);

			return routes;
		}
		
		#endregion
	}

	[Serializable]
	[DebuggerDisplay("MapID={MapID} NodeID={NodeID} ItemID={ItemID}")]	
	[PXHidden]
	public partial class Position : IBqlTable
	{
		#region MapID

		protected Int32? _MapID;

		[PXDBInt]
		public virtual Int32? MapID
		{
			get { return _MapID; }
			set { _MapID = value; }
		}

		public abstract class mapID : PX.Data.BQL.BqlInt.Field<mapID> { }

		#endregion

		#region NodeID

		protected Int32? _NodeID;

		[PXDBInt]
		public virtual Int32? NodeID
		{
			get { return _NodeID; }
			set { _NodeID = value; }
		}

		public abstract class nodeID : PX.Data.BQL.BqlInt.Field<nodeID> { }

		#endregion

		#region ItemID

		protected Int32? _ItemID;

		[PXDBInt]
		public virtual Int32? ItemID
		{
			get { return _ItemID; }
			set { _ItemID = value; }
		}

		[PXDBInt]
		public virtual Int32? RouteItemID
		{
			get;
			set;
		}

		[PXDBInt]
		public virtual Int32? RouteParentID
		{
			get;
			set;
		}

		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? UseCurrentTreeItem
		{
			get;
			set;
		}

		public abstract class itemID : PX.Data.BQL.BqlInt.Field<itemID> { }

		#endregion
	}
}
