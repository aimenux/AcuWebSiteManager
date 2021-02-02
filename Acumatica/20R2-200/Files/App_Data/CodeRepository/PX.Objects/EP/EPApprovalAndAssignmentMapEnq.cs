using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.TM;
using System.Linq;
using PX.Web.UI;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	public class EPAssignmentAndApprovalMapEnq : PXGraph<EPAssignmentAndApprovalMapEnq>
	{
		#region ctor

		public EPAssignmentAndApprovalMapEnq()
		{
			this.RestrictViewFields(nameof(Maps),
					typeof(EPAssignmentMap.entityType),
					typeof(EPAssignmentMap.name),
					typeof(EPAssignmentMap.graphType));
		}
		#endregion

		#region Select

		public	PXSelect<
					EPAssignmentMap,
				Where<EPAssignmentMap.mapType, Equal<EPMapType.assignment>,
					Or<FeatureInstalled<FeaturesSet.approvalWorkflow>>>,
				OrderBy<Desc<EPAssignmentMap.createdDateTime>>>
			Maps;

		public	PXSelect<
					EPAssignmentMap,
				Where2<
					Where<EPAssignmentMap.mapType, Equal<EPMapType.assignment>,
						Or<FeatureInstalled<FeaturesSet.approvalWorkflow>>>,
					And<EPAssignmentMap.assignmentMapID,
						Equal<Required<EPAssignmentMap.assignmentMapID>>>>>
			MapsForRedirect;
		#endregion

		#region Actions

		public PXCancel<EPAssignmentMap> Cancel;

		public PXAction<EPAssignmentMap> ViewDetails;
		[PXUIField(MapEnableRights = PXCacheRights.Select, DisplayName = "")]
		[PXButton(ImageKey = Sprite.Main.RecordEdit, Tooltip = Messages.NavigateToTheSelectedMap)]
		public virtual void viewDetails()
		{
			if (Maps.Current != null)
			{
				var mapId = Maps.Current.AssignmentMapID;
				Maps.Cache.Clear();
				var map = MapsForRedirect.SelectSingle(mapId);
				PXRedirectHelper.TryRedirect(this, map, PXRedirectHelper.WindowMode.InlineWindow);
			}
		}

		public PXAction<EPAssignmentMap> AddApprovalNew;
		[PXUIField(DisplayName = "Add Approval Map")]
		[PXInsertButton(Tooltip = Messages.AddNewApprovalMap, CommitChanges = true, ImageKey = "")]
		public void addApprovalNew()
		{
			EPApprovalMapMaint graph = CreateInstance<EPApprovalMapMaint>();
			graph.AssigmentMap.Current = graph.AssigmentMap.Insert();
			graph.AssigmentMap.Cache.IsDirty = false;
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<EPAssignmentMap> AddAssignmentNew;
		[PXUIField(DisplayName = "Add Assignment Map")]
		[PXInsertButton(Tooltip = Messages.AddNewAssignmentMap, CommitChanges = true, ImageKey = "")]
		protected void addAssignmentNew()
		{
			EPAssignmentMapMaint graph = CreateInstance<EPAssignmentMapMaint>();
			graph.AssigmentMap.Current = graph.AssigmentMap.Insert();
			graph.AssigmentMap.Cache.IsDirty = false;
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
		}
		#endregion

		#region Event Handlers

		protected virtual void EPAssignmentMap_EntityType_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			EPAssignmentMap row = e.Row as EPAssignmentMap;
			if (row != null)
				e.ReturnState = CreateFieldStateForEntity(e.ReturnValue, row.EntityType, row.GraphType);
		}

		private PXFieldState CreateFieldStateForEntity(object returnState, string entityType, string graphType)
		{
			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();


			Type gType = null;
			if (graphType != null)
				gType = GraphHelper.GetType(graphType);
			else if (entityType != null)
			{
				Type eType = System.Web.Compilation.PXBuildManager.GetType(entityType, false);
				gType = (eType == null) ? null : EntityHelper.GetPrimaryGraphType(this, eType);
			}

			if (gType != null)
			{
				PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeUnsecure(gType);
				if (node != null)
				{
					allowedValues.Add(entityType);
					allowedLabels.Add(node.Title);
				}
			}

			return PXStringState.CreateInstance(returnState, 60, null, "Entity", false, 1, null,
												allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
		}

		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Entity Type")]
		protected virtual void EPAssignmentMap_EntityType_CacheAttached(PXCache sender)
		{
		}
		#endregion
	}
}