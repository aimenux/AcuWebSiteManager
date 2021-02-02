using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.TM;
using System.Linq;
using PX.Objects.CR;
using PX.Common;

namespace PX.Objects.EP
{
	public class EPAssignmentMapMaint : EPApprovalAndAssignmentMapBase<EPAssignmentMapMaint>
	{
		public EPAssignmentMapMaint()
		{
			base.AssigmentMap.View = new PXView(this, false, BqlCommand.CreateInstance(BqlCommand.Compose(
				typeof(Select<,>), typeof(EPAssignmentMap),
				typeof(Where<,>), typeof(EPAssignmentMap.mapType), typeof(Equal<>), typeof(EPMapType.assignment))));
		}

		protected virtual IEnumerable nodes([PXDBGuid] Guid? ruleID)
		{
			List<EPRule> list = new List<EPRule>();

			if (ruleID.HasValue)
				return list;

			IEnumerable resultSet = PXSelectJoin
				<EPRule, LeftJoin<EPCompanyTree, On<EPRule.workgroupID, Equal<EPCompanyTree.workGroupID>>>,
					Where<EPRule.assignmentMapID, Equal<Current<EPAssignmentMap.assignmentMapID>>>>.Select(this);

			foreach (PXResult<EPRule, EPCompanyTree> record in resultSet)
			{
				EPRule route = record;

				route.Icon = PX.Web.UI.Sprite.Tree.GetFullUrl(PX.Web.UI.Sprite.Tree.Leaf);
				list.Add(route);
			}
			return list;
		}

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowUp)]
		public virtual IEnumerable Up(PXAdapter adapter)
		{
			IList<EPRule> routes = UpdateSequence();
			int currentItemIndex = (int)Nodes.Current.Sequence - 1;

			if (currentItemIndex > 0)
			{
				routes[currentItemIndex].Sequence--;
				routes[currentItemIndex - 1].Sequence++;

				Nodes.Update(routes[currentItemIndex]);
				Nodes.Update(routes[currentItemIndex - 1]);
				Nodes.Cache.ActiveRow = routes[currentItemIndex];
			}

			return adapter.Get();
		}

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown)]
		public virtual IEnumerable Down(PXAdapter adapter)
		{
			IList<EPRule> routes = UpdateSequence();
			int currentItemIndex = (int)Nodes.Current.Sequence - 1;

			if (currentItemIndex < routes.Count - 1)
			{
				routes[currentItemIndex].Sequence++;
				routes[currentItemIndex + 1].Sequence--;

				Nodes.Update(routes[currentItemIndex]);
				Nodes.Update(routes[currentItemIndex + 1]);
				Nodes.Cache.ActiveRow = routes[currentItemIndex];
			}

			return adapter.Get();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[EPAssignmentMapSelector(MapType = EPMapType.Assignment)]
		[PXUIField(DisplayName = "Map")]
		protected virtual void EPAssignmentMap_AssignmentMapID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(EPMapType.Assignment)]
		protected virtual void EPAssignmentMap_MapType_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Assign Ownership To")]
		[EPAssignRuleType.List()]
		protected virtual void EPRuleTree_RuleType_CacheAttached(PXCache sender)
		{
		}

		protected override void EPAssignmentMap_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.EPAssignmentMap_RowSelected(sender, e);
			Rules.Cache.AllowInsert = CurrentNode.Current != null;
		}

		protected virtual IEnumerable<string> GetEntityTypeScreens()
		{
			return new string[]
			{
				"CR303000",//Business Account
				"CR306000",//Case
				"CR302000",//Contact
				"CR306015",//Email Activity
				"CR301000",//Lead
				"CR304000",//Opportunity
				"PO302000",//Purchase Receipt
				"RQ301000",//Purchase Request
				"RQ302000",//Purchase Requisition
			};
		}

		protected virtual void EPAssignmentMap_GraphType_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as EPAssignmentMap;
			if (row != null)
			{
				var list = Definitions.SiteMapNodes.OrderBy(x => x.Title);
				PXStringListAttribute.SetLocalizable<EPAssignmentMap.graphType>(sender, null, false);
				PXStringListAttribute.SetList<EPAssignmentMap.graphType>(sender, row,
					list.Select(x => x.GraphType).ToArray(), list.Select(x => x.Title).ToArray());
			}
		}
		#region GraphTypes Cache
		private Definition Definitions
		{
			get
			{
				Definition defs = PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition, EPAssignmentMapMaint>(typeof(Definition).FullName, this, typeof(SiteMap)));
				}
				return defs;
			}
		}
		private class Definition : IPrefetchable<EPAssignmentMapMaint>
		{
			public List<PXSiteMapNode> SiteMapNodes = new List<PXSiteMapNode>();

			public void Prefetch(EPAssignmentMapMaint graph)
			{
				var types = graph.GetGraphTypes(type => type.IsDefined(typeof(PXEMailSourceAttribute), true) && typeof(PX.Data.EP.IAssign).IsAssignableFrom(type));
				SiteMapNodes = types.Where(x => graph.GetEntityTypeScreens().Contains(x.ScreenID)).GroupBy(x => x.ScreenID).Select(x => x.First()).ToList();
			}
		}
		#endregion

		protected virtual void EPRule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPRule row = e.Row as EPRule;
			if (row == null)
				return;
			if (row.OwnerSource == null && row.RuleType == EPRuleType.Document)
				throw new PXSetPropertyException<EPRule.ownerSource>(Messages.EmployeeCannotBeEmpty, PXErrorLevel.Error);
		}

		protected virtual void EPRule_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			EPRule row = e.Row as EPRule;
			if (row != null)
			{
				var rules = UpdateSequence();
				var isCurrent = false;
				foreach (var rule in rules)
				{
					if (isCurrent)
					{
						rule.Sequence++;
						Nodes.Update(rule);
					}
					else if (Nodes.Current != null && rule.RuleID == Nodes.Current.RuleID)
					{
						isCurrent = true;
						row.Sequence = rule.Sequence + 1;
					}
				}
			}
		}
	}
}