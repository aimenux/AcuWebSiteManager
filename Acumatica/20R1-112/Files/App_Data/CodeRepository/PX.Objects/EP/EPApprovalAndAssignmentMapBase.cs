using PX.Data;
using PX.Objects.CR;
using PX.SM;
using PX.TM;
using PX.Web.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.SM.EMailSourceHelper;
using PX.Objects.CS;
using PX.Common;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.EP
{
	public class EPApprovalAndAssignmentMapBase<TGraph> : PXGraph<TGraph> where TGraph : PXGraph
	{
		#region Main Actions
		public PXSave<EPAssignmentMap> Save;
	    public PXCancel<EPAssignmentMap> Cancel;
        public PXInsert<EPAssignmentMap> Insert;
		public PXCopyPasteAction<EPAssignmentMap> CopyPaste;
		public PXDelete<EPAssignmentMap> Delete;
		#endregion

		public PXAction<EPAssignmentMap> up;
		public PXAction<EPAssignmentMap> down;

		public PXAction<EPAssignmentMap> conditionUp;
		public PXAction<EPAssignmentMap> conditionDown;
		public PXAction<EPAssignmentMap> conditionInsert;
		

		#region Views
		public PXSelect<EPAssignmentMap> AssigmentMap;

		public PXSelect
			<EPRuleTree,
			Where<EPRuleTree.assignmentMapID, Equal<Current<EPAssignmentMap.assignmentMapID>>>, OrderBy<Asc<EPRuleTree.sequence>>>
			NodesTree;

		public PXSelect
			<EPRule,
			Where<EPRule.assignmentMapID, Equal<Current<EPAssignmentMap.assignmentMapID>>>, OrderBy<Asc<EPRule.sequence>>>
			Nodes;

		public PXSelect<EPRule, Where<EPRule.ruleID, Equal<Current<EPRuleTree.ruleID>>>> CurrentNode;

		public PXSelect<EPRuleCondition, 
			Where<EPRuleCondition.ruleID, Equal<Current<EPRuleTree.ruleID>>>, 
			OrderBy<Asc<EPRuleCondition.rowNbr>>>
			Rules;
		
		protected virtual IEnumerable nodesTree([PXDBGuid] Guid? ruleID)
		{
			var list = Nodes.Select(ruleID);

			var treeList = new List<EPRuleTree>();
			foreach (EPRule rule in list)
			{
				var node = new EPRuleTree
				{
					StepName = rule.StepName,
					IsActive = rule.IsActive,
					Name = String.IsNullOrEmpty(rule.Name) ? Messages.NoName : rule.Name,
					RuleID = rule.RuleID,
					StepID = rule.StepID,
					ApproveType = rule.ApproveType,
					ReasonForApprove = rule.ReasonForApprove,
					ReasonForReject = rule.ReasonForReject,
					AssignmentMapID = rule.AssignmentMapID,
					Icon = rule.Icon,
					Sequence = rule.Sequence,
					OwnerID = rule.OwnerID,
					OwnerSource = rule.OwnerSource,
					RuleType = rule.RuleType,
					WaitTime = rule.WaitTime,
					WorkgroupID = rule.WorkgroupID,
					EmptyStepType = rule.EmptyStepType,
					ExecuteStep = rule.ExecuteStep,
					CreatedByID = rule.CreatedByID,
					CreatedByScreenID = rule.CreatedByScreenID,
					CreatedDateTime = rule.CreatedDateTime, 
					LastModifiedByID = rule.LastModifiedByID,
					LastModifiedByScreenID = rule.LastModifiedByScreenID,
					LastModifiedDateTime = rule.LastModifiedDateTime,
					tstamp = rule.tstamp,
				};
				treeList.Add(node);

			}
			return treeList;
		}

		public PXSelect<PX.TM.PXOwnerSelectorAttribute.EPEmployee, Where<PX.TM.PXOwnerSelectorAttribute.EPEmployee.pKID, Equal<Required<PX.TM.PXOwnerSelectorAttribute.EPEmployee.pKID>>, 
			And<PX.TM.PXOwnerSelectorAttribute.EPEmployee.status, Equal<BAccount.status.inactive>>>> EmployeeInactive;
		#endregion

		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Name", Required = true)]
		protected virtual void EPAssignmentMap_Name_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXStringList()]
		[PXUIField(DisplayName = "Entity Type", Required = true)]
		protected virtual void EPAssignmentMap_GraphType_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region Tree Selector Entity
		public PXSelect<CacheEntityItem,
			Where<CacheEntityItem.path, Equal<CacheEntityItem.path>>,
			OrderBy<Asc<CacheEntityItem.number>>> EntityItems;

        protected IEnumerable entityItems(string parent)
        {
            if (this.AssigmentMap.Current == null)
                yield break;
            Type type = GraphHelper.GetType(this.AssigmentMap.Current.EntityType); ;
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

		#region Actions
		public PXAction<EPAssignmentMap> addRule;
		[PXUIField(DisplayName = "Add Rule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton()]
		public virtual void AddRule()
		{
			var rule = Nodes.Insert();
			rule.Name = Messages.Rule;
			Nodes.Update(rule);
			Nodes.Cache.ActiveRow = rule;
		}

		public PXAction<EPAssignmentMap> deleteRoute;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Remove)]
		public virtual void DeleteRoute()
		{
			if (Nodes.Current == null)
				return;

			Nodes.Delete(Nodes.Current);
		}

		[PXButton(ImageKey = Sprite.Main.ArrowUp, Tooltip = ActionsMessages.ttipRowUp)]
		[PXUIField(DisplayName = ActionsMessages.RowUp, MapEnableRights = PXCacheRights.Update)]
		public virtual IEnumerable ConditionUp(PXAdapter adapter)
		{
			if (this.Rules.Current == null)
				return adapter.Get();
			EPRuleCondition prev = PXSelect<EPRuleCondition,
				Where<EPRuleCondition.ruleID, Equal<Current<EPRule.ruleID>>, And<EPRuleCondition.rowNbr, Less<Current<EPRuleCondition.rowNbr>>>>, OrderBy<Desc<EPRuleCondition.rowNbr>>>.Select(this);
			
			if (prev != null)
			{
				var current = this.Rules.Current;
				var prevCopy = (EPRuleCondition)this.Rules.Cache.CreateCopy(prev);
				var currentCopy = (EPRuleCondition)this.Rules.Cache.CreateCopy(this.Rules.Current);

				this.SwapItems(this.Rules.Cache, prev, current);
				RestoreConditionValues(prev, currentCopy);
				RestoreConditionValues(current, prevCopy);
				this.Rules.Cache.ActiveRow = prev;
			}
			return adapter.Get();
		}

		[PXButton(ImageKey = Sprite.Main.ArrowDown, Tooltip = ActionsMessages.ttipRowDown)]
		[PXUIField(DisplayName = ActionsMessages.RowDown, MapEnableRights = PXCacheRights.Update)]
		public virtual IEnumerable ConditionDown(PXAdapter adapter)
		{
			if (this.Rules.Current == null)
				return adapter.Get();
			EPRuleCondition next = PXSelect<EPRuleCondition,
				Where<EPRuleCondition.ruleID, Equal<Current<EPRule.ruleID>>, And<EPRuleCondition.rowNbr, Greater<Current<EPRuleCondition.rowNbr>>>>, OrderBy<Asc<EPRuleCondition.rowNbr>>>.Select(this);
			
			if (next != null)
			{
				var current = this.Rules.Current;
				var nextCopy = (EPRuleCondition)this.Rules.Cache.CreateCopy(next);
				var currentCopy = (EPRuleCondition)this.Rules.Cache.CreateCopy(this.Rules.Current);

				this.SwapItems(this.Rules.Cache, next, current);
				RestoreConditionValues(next, currentCopy);
				RestoreConditionValues(current, nextCopy);
				this.Rules.Cache.ActiveRow = next;
			}
			return adapter.Get();
		}

		[PXButton(ImageKey = Sprite.Main.RecordAdd,
			Tooltip = ActionsMessages.ttipRowInsertBefore)]
		[PXUIField(DisplayName = ActionsMessages.RowInsert, MapEnableRights = PXCacheRights.Update)]
		public virtual IEnumerable ConditionInsert(PXAdapter adapter)
		{
			if (this.Rules.Current == null)
				return adapter.Get();
			
			var allNext = PXSelect<EPRuleCondition,
				Where<EPRuleCondition.ruleID, Equal<Current<EPRule.ruleID>>, And<EPRuleCondition.rowNbr, GreaterEqual<Current<EPRuleCondition.rowNbr>>>>, OrderBy<Desc<EPRuleCondition.rowNbr>>>.Select(this);

			var currentNumber = Rules.Current.RowNbr;
			var currentCopy = (EPRuleCondition)Rules.Cache.CreateCopy(Rules.Current);
			Rules.Cache.ClearQueryCacheObsolete();
			foreach (EPRuleCondition rule in allNext)
			{
				var nextRule = (EPRuleCondition)Rules.Cache.CreateCopy(rule);
				nextRule.RowNbr++;
				Rules.Update(nextRule);
				RestoreConditionValues(nextRule, rule);
				Rules.Delete(rule);
			}

			currentCopy.Entity = null;
			currentCopy.Condition = 0;
			currentCopy.CloseBrackets = 0;
			currentCopy.OpenBrackets = 0;
			currentCopy.Operator = 0;
			Rules.Update(currentCopy);

			return adapter.Get();
		}

		public override bool CanClipboardCopyPaste()
		{
			return false;
		}
		#endregion

		#region Event Handlers
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

		protected virtual void EPRuleCondition_Entity_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (AssigmentMap.Current != null)
				e.ReturnState = CreateFieldStateForEntity(e.ReturnValue, AssigmentMap.Current.EntityType, AssigmentMap.Current.GraphType);
		}

		protected virtual void EPRuleCondition_Entity_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPRuleCondition row = e.Row as EPRuleCondition;
			Rules.Cache.SetValue<EPRuleCondition.fieldName>(row, null);
		}

		protected virtual void EPRuleCondition_FieldName_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			EPRuleCondition row = e.Row as EPRuleCondition;
			if (row == null || row.Entity == null) return;
			
			e.ReturnState = CreateFieldStateForFieldName(e.ReturnState, AssigmentMap.Current.EntityType, AssigmentMap.Current.GraphType, row.Entity, row);
		}

		protected virtual void EPAssignmentMap_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(CurrentNode.Cache, CurrentNode.Current, null, CurrentNode.Current != null);

			EPAssignmentMap row = e.Row as EPAssignmentMap;

			addRule.SetEnabled(row != null && row.EntityType != null);
			if (row != null)
			{
				if (row.EntityType != null && row.GraphType == null)
				{
					Type entityType = System.Web.Compilation.PXBuildManager.GetType(row.EntityType, false);
					var primary = EntityHelper.GetPrimaryGraphType(this, entityType);
					if (primary != null)
					{
						AssigmentMap.Current.GraphType = primary.FullName;
						AssigmentMap.Cache.SetStatus(AssigmentMap.Current, PXEntryStatus.Updated);
						AssigmentMap.Cache.IsDirty = true;
					}
				}
				this.Nodes.Cache.AllowInsert = row.EntityType != null;
				var nodes = Nodes.Select();
				PXUIFieldAttribute.SetEnabled<EPAssignmentMap.graphType>(sender, row, nodes == null || nodes.Count == 0);
			}
			
			up.SetEnabled(this.CurrentNode.Current != null);
			down.SetEnabled(this.CurrentNode.Current != null);
			deleteRoute.SetEnabled(this.CurrentNode.Current != null);

			if (NodesTree.Current == null)
			{
				var step = (EPRuleTree)NodesTree.SelectSingle();
				if (step != null)
				{
					var rule = (EPRuleTree)NodesTree.Select(step.RuleID);
					NodesTree.Cache.ActiveRow = rule ?? step;
					NodesTree.Current = (EPRuleTree)NodesTree.Cache.ActiveRow;
				}
			}
		}

		protected virtual void EPRuleCondition_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPRuleCondition oldRow = e.OldRow as EPRuleCondition;
			EPRuleCondition newRow = e.Row as EPRuleCondition;
			if (oldRow != null && newRow != null)
			{
				if (!String.Equals(newRow.Entity, oldRow.Entity, StringComparison.OrdinalIgnoreCase)) newRow.FieldName = newRow.Value = newRow.Value2 = null;
				if (!String.Equals(newRow.FieldName, oldRow.FieldName, StringComparison.OrdinalIgnoreCase)) newRow.Value = newRow.Value2 = null;
				EPRuleCondition row = e.Row as EPRuleCondition;

				if (row.Condition == null || (PXCondition)row.Condition == PXCondition.ISNULL || (PXCondition)row.Condition == PXCondition.ISNOTNULL)
				{
					newRow.Value = newRow.Value2 = null;
				}
				if (newRow.Value == null)
				{
					PXFieldState state = sender.GetStateExt<EPRuleCondition.value>(newRow) as PXFieldState;
					newRow.Value = state != null && state.Value != null ? state.Value.ToString() : null;
				}
			}
		}

		protected virtual void EPRuleCondition_Value_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			EPRuleCondition row = e.Row as EPRuleCondition;
			if (row != null && !string.IsNullOrEmpty(row.FieldName) && row.Condition != null &&
				(PXCondition)row.Condition != PXCondition.ISNULL && (PXCondition)row.Condition != PXCondition.ISNOTNULL)
			{
				var fieldState = CreateFieldStateForFieldValue(e.ReturnState, AssigmentMap.Current.EntityType, row.Entity,
					row.FieldName);
				if (fieldState != null)
					e.ReturnState = fieldState;
			}
		}
		protected virtual void EPRuleCondition_Value2_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var returnValue = e.ReturnValue;
			this.EPRuleCondition_Value_FieldSelecting(sender, e);
			e.ReturnValue = returnValue;
		}

		protected virtual void EPRuleCondition_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPRuleCondition row = (EPRuleCondition)e.Row;
			if (row == null) return;
			if (AssigmentMap.Current == null) return;
			Type type = GraphHelper.GetType(AssigmentMap.Current.EntityType);
			if (type != null)
			{
				Type cachetype = GraphHelper.GetType(row.Entity);
				if (cachetype == null)
					return;

				PXFieldState state = GetPXFieldState(cachetype, row.FieldName);
				if (state == null)
					throw new PXSetPropertyException<EPRuleCondition.fieldName>(Messages.FieldCannotBeFound, PXErrorLevel.Error);
			}
		}

		protected virtual void EPRule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPRule row = e.Row as EPRule;
			if (row == null)
				return;

			if (row.OwnerID != null)
			{
				var employee = EmployeeInactive.SelectSingle(row.OwnerID);
				if (employee != null)
					sender.RaiseExceptionHandling<EPRule.ownerID>(row, null, new PXSetPropertyException(Messages.EmployeeIsInactive, PXErrorLevel.Warning));
				else
					sender.RaiseExceptionHandling<EPRule.ownerID>(row, null, null);
			}

			PXUIFieldAttribute.SetVisible<EPRule.ownerID>(this.Nodes.Cache, this.Nodes.Current, row.RuleType == EPRuleType.Direct);
			PXUIFieldAttribute.SetVisible<EPRule.ownerSource>(this.Nodes.Cache, this.Nodes.Current, row.RuleType == EPRuleType.Document);

			PXUIFieldAttribute.SetEnabled(CurrentNode.Cache, null, null, true);

			up.SetEnabled(true);
			down.SetEnabled(true);
			deleteRoute.SetEnabled(true);
		}
		#endregion

		#region Utils
		private const string _FIELDNAME_STR = "FieldName";

		private PXFieldState GetPXFieldState(Type cachetype, string fieldName)
		{
			PXCache cache = this.Caches[cachetype];
			PXDBAttributeAttribute.Activate(cache);
			return cache.GetStateExt(null, fieldName) as PXFieldState;
		}

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
						if (e.SubKey != typeof(CS.CSAnswers).FullName)
						{
							allowedValues.Add(e.SubKey);
							allowedLabels.Add(e.Name);
						}
					}
			}

			return PXStringState.CreateInstance(returnState, 60, null, "Entity", false, 1, null,
												allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
		}

		protected PXFieldState CreateFieldStateForFieldName(object returnState, string entityType, string graphType, string cacheName, EPRuleBaseCondition condition = null)
		{
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
				return CreateFieldStateForFieldName(returnState, type, gType, viewName, cacheName, EPRuleCondition.FieldLength, condition);
			}
			return PXStringState.CreateInstance(returnState, EPRuleCondition.FieldLength, null, _FIELDNAME_STR, false, 1, null,
												(new List<string>()).ToArray(), (new List<string>()).ToArray(), false, null);
		}

		protected PXFieldState CreateFieldStateForFieldName(object returnState, Type entityType, Type gType, string viewName, string cacheName, int? fieldLength, EPRuleBaseCondition condition)
		{
			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();
            List<string> uniqueLabels = new List<string>();
            if (entityType != null)
			{
				Type cachetype = GraphHelper.GetType(cacheName);
				if (cachetype == null)
					return null;
				if (condition != null)
				{
					string fieldName = condition.FieldName;
					if (!string.IsNullOrEmpty(fieldName))
					{
						PXFieldState state = GetPXFieldState(cachetype, fieldName);
						if (state == null)
						{
							Rules.Cache.SetStatus(condition, PXEntryStatus.Updated);
							Rules.Cache.RaiseExceptionHandling<EPRuleCondition.fieldName>(condition, fieldName, new PXSetPropertyException(Messages.FieldCannotBeFound, PXErrorLevel.Error));
						}
					}
				}
				Dictionary<string, string> fields = new Dictionary<string, string>();
				foreach (CacheEntityItem c in
					EMailSourceHelper.TemplateEntity(this, viewName, 
                        entityType.FullName, 
                        gType != null ? gType.FullName : null, false))
				{
					fields[c.SubKey] = c.Name;
				}
                
                Action<string, string> addValueLabel = (value, label) =>
                {
                    allowedValues.Add(value);
                    allowedLabels.Add(label);
                    uniqueLabels.Add(label);
                };
				foreach (var item in fields.OrderBy(i => i.Value))
				{
					if (!item.Key.EndsWith("_Attributes") && item.Key.Contains('_')) continue;

					var attr = this.Caches[cachetype].GetAttributes(null, item.Key);

					var value = item.Value;

					if (attr != null)
					{
						var timeListAttribute = attr.FirstOrDefault(a => a is PXTimeListAttribute) as PXTimeListAttribute;
						var intAttribute = attr.FirstOrDefault(a => a is PXIntAttribute) as PXIntAttribute;
						if (timeListAttribute != null && intAttribute != null)
						{
							value += " (" + PXLocalizer.Localize("Minutes") + ")";
						}
					}

					if (!uniqueLabels.Contains(value))
                    {
                        addValueLabel(item.Key, value);
                    }
                    else
                    {
                        string label = string.Format("{0} - {1}", item.Key, value);
                        addValueLabel(item.Key, label);
                    }
				}
			}

			return PXStringState.CreateInstance(returnState, fieldLength, null, _FIELDNAME_STR, false, 1, null,
												allowedValues.ToArray(), allowedLabels.ToArray(), false, null);
		}

		protected PXFieldState CreateFieldStateForFieldValue(object returnState, string entityType, string cacheName, string fieldName)
		{
			Type type = GraphHelper.GetType(entityType);
			if (type != null)
			{
				Type cachetype = GraphHelper.GetType(cacheName);
				if (cachetype == null)
					return null;

				PXCache cache = this.Caches[cachetype];
				PXFieldState state = GetPXFieldState(cachetype, fieldName);
				if (state == null)
					state = PXFieldState.CreateInstance(returnState,
						null, // Type dataType,
						null, // bool? isKey,
						null, // bool? nullable,
						null, // int? required,
						null, // int? precision,
						10, // int? length,
						returnState, // object defaultValue,
						fieldName, // string fieldName,
						null, // string descriptionName,
						null, // string displayName,
						null, // string error,
						PXErrorLevel.Undefined, // PXErrorLevel errorLevel,
						true, // bool? enabled,
						true, // bool? visible,
						null, // bool? readOnly,
						PXUIVisibility.Visible, // PXUIVisibility visibility,
						null, // string viewName,
						null, // string[] fieldList,
						null); 

				state.DescriptionName = null;

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

					if (attr.Any(a => a is StateAttribute))
					{
						string viewName = "StatesNoFilterByCountry";

						if (cache.Graph.Views.ContainsKey(viewName) == false)
						{
							var StatesNoFilterByCountry =
								new SelectFrom<State>.
										InnerJoin<Country>.
											On<State.countryID.IsEqual<Country.countryID>>.
									View(cache.Graph);
							cache.Graph.Views.Add(viewName, StatesNoFilterByCountry.View);
						}

						state.ViewName = viewName;
						state.FieldList = new string[]
							{
								nameof(State.stateID),
								nameof(State.name),
								string.Concat(nameof(Country), "__", nameof(Country.countryID)),
								string.Concat(nameof(Country), "__", nameof(Country.description))
							};
						state.HeaderList = new string[]
							{
								PXUIFieldAttribute.GetDisplayName(cache.Graph.Caches<State>(), nameof(State.stateID)),
								PXUIFieldAttribute.GetDisplayName(cache.Graph.Caches<State>(), nameof(State.name)),
								PXUIFieldAttribute.GetDisplayName(cache.Graph.Caches<Country>(), nameof(Country.countryID)),
								PXUIFieldAttribute.GetDisplayName(cache.Graph.Caches<Country>(), nameof(Country.description))
							};
					}
				}

				if (state != null)
				{
					if (returnState == null)
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

					//PXView view;
					//if (state.ViewName != null &&
					//	this.Views.TryGetValue(state.ViewName, out view) &&
					//	view.BqlSelect.GetTables()[0] == typeof(EPEmployee))
					//{
					//	state.ViewName = "Employee";
					//}
				}

                if (attr != null)
                {
                    var intListAttribute = attr.FirstOrDefault(a => a.GetType().IsSubclassOf(typeof(PXIntListAttribute))) as PXIntListAttribute;
                    if (intListAttribute != null)
                        return state;
                }

                state = PXFieldState.CreateInstance((state as PXStringState)?.AllowedValues != null ? state : state.Value, state.DataType, state.PrimaryKey, state.Nullable, state.Required == true ? 1 : state.Required == null ? 0 : -1, state.Precision, state.Length, state.DefaultValue, fieldName, 
					state.DescriptionName, state.DisplayName, state.Error, state.ErrorLevel, true, true, false, PXUIVisibility.Visible, state.ViewName, state.FieldList, state.HeaderList);

				return state;
			}
			return null;
		}

		private static int Comparison(EPRule x, EPRule y)
		{
			return x.Sequence.Value.CompareTo(y.Sequence.Value);
		}

		private IList<EPRule> GetSortedItems()
		{
			List<EPRule> routes = new List<EPRule>();

			foreach (EPRule item in Nodes.Cache.Cached)
			{
				if (Nodes.Cache.GetStatus(item) != PXEntryStatus.Deleted &&
					Nodes.Cache.GetStatus(item) != PXEntryStatus.InsertedDeleted)
				{
					if (Nodes.Current == null || Nodes.Current.StepID == item.StepID)
						routes.Add(item);
				}
			}
			routes.Sort(Comparison);

			return routes;
		}

		protected virtual PXGraph CreateGraph(string graphName, string screenID)
		{
			Type gt = System.Web.Compilation.PXBuildManager.GetType(graphName, false);
			if (gt == null)
			{
				gt = Type.GetType(graphName);
			}
			if (gt != null)
			{
				gt = System.Web.Compilation.PXBuildManager.GetType(PX.Api.CustomizedTypeManager.GetCustomizedTypeFullName(gt), false) ?? gt;
				using (new PXPreserveScope())
				{
					try
					{
						return gt == typeof(PXGenericInqGrph) ? PXGenericInqGrph.CreateInstance(screenID) : (PXGraph)PXGraph.CreateInstance(gt);
					}
					catch (System.Reflection.TargetInvocationException ex)
					{
						throw PXException.ExtractInner(ex);
					}
				}
			}
			return null;
		}

		protected List<PXSiteMapNode> GetGraphTypes(MakeSiteMapCondition condition)
		{
			var list = new List<PXSiteMapNode>();
			foreach (EntityItemSource e in EMailSourceHelper.TemplateScreensByCondition(this, null, null, condition))
			{
				if (!String.IsNullOrEmpty(e.ScreenID))
				{
					PXSiteMapNode node = (PXSiteMapNode)PXSiteMap.Provider.FindSiteMapNodeFromKey(e.Key);
					if (node != null && !String.IsNullOrEmpty(node.GraphType))
					{
						e.SubKey = node.GraphType;
						list.Add(node);
					}
				}
			}
			return list;
		}

		private void SwapItems(PXCache cache, object first, object second)
		{
			object temp = cache.CreateCopy(first);
			foreach (Type field in cache.BqlFields)
				if (!cache.BqlKeys.Contains(field))
					cache.SetValue(first, field.Name, cache.GetValue(second, field.Name));
			foreach (Type field in cache.BqlFields)
				if (!cache.BqlKeys.Contains(field))
					cache.SetValue(second, field.Name, cache.GetValue(temp, field.Name));
			cache.Update(first);
			cache.Update(second);
		}

		private void RestoreConditionValues(EPRuleCondition row, EPRuleCondition restoreFrom)
		{
			this.Rules.Cache.SetValue<EPRuleCondition.fieldName>(row, restoreFrom.FieldName);
			this.Rules.Cache.Update(row);
			this.Rules.Cache.SetValueExt<EPRuleCondition.value>(row, restoreFrom.Value);
			this.Rules.Cache.SetValue<EPRuleCondition.value2>(row, restoreFrom.Value2);
			this.Rules.Cache.Update(row);
		}

		protected IList<EPRule> UpdateSequence()
		{
			IList<EPRule> routes = GetSortedItems();

			var current = Nodes.Current;
			for (int i = 0; i < routes.Count; i++)
			{
				if (routes[i].Sequence != i + 1)
				{
					routes[i].Sequence = i + 1;
					if (routes[i].RuleID == current.RuleID)
						current = Nodes.Update(routes[i]);
					else
						Nodes.Update(routes[i]);
				}
			}
			Nodes.Current = current;
			return routes;
		}
		#endregion
	}
}
