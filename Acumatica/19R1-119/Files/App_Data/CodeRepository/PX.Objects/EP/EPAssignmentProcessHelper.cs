using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.Data;
using System.Collections;
using PX.SM;
using PX.TM;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	/// <summary>
	/// Implements the Workgroup assignment process logic.
	/// </summary>
	public class EPAssignmentProcessHelper<Table> : PXGraph<EPAssignmentProcessHelper<Table>>
		where Table : class, IBqlTable
	{
		public PXSelect<BAccount> baccount;
		public PXSelect<PX.Objects.AR.Customer> customer;
		public PXSelect<PX.Objects.AP.Vendor> vendor;
		public PXSelect<EPEmployee> employee;
		public PXView attributeView;

		/// <summary>
		/// This list is used to track the path passed to check and prevent cyclic references.
		/// </summary>
		private readonly List<int> path = new List<int>();
		private PXGraph processGraph;
		private Type processMapType;
		private IBqlTable currentItem;
		private CSAnswers currentAttribute;

		private readonly PXGraph _Graph;

		public EPAssignmentProcessHelper(PXGraph graph)
			: this()
		{
			_Graph = graph;
		}

		public EPAssignmentProcessHelper()
		{
			attributeView = new PXView(this, false, new Select<CSAnswers>(), (PXSelectDelegate)getAttributeRecord);
		}

		/// <summary>
		/// Assigns Owner and Workgroup to the given IAssign instance based on the assigmentment rules.
		/// </summary>
		/// <param name="item">IAssign object</param>
		/// <param name="assignmentMapID">Assignment map</param>
		/// <returns>True if workgroup was assigned; otherwise false</returns>
		/// <remarks>
		/// You have to manualy persist the IAssign object to save the changes.
		/// </remarks>
		public virtual bool Assign(Table item, int? assignmentMapID)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			if (assignmentMapID < 0)
				throw new ArgumentOutOfRangeException(nameof(assignmentMapID));

			path.Clear();
			EPAssignmentMap map =
				PXSelect<EPAssignmentMap, Where<EPAssignmentMap.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>>>
				.SelectWindowed(this, 0, 1, assignmentMapID);
			if (map == null)
				return false;

			if (map.MapType != EPMapType.Legacy)
				throw new ArgumentOutOfRangeException(nameof(assignmentMapID));

			return Assign(item, map, false)?.Any() ?? false;
		}

		public virtual IEnumerable<ApproveInfo> Assign(Table item, EPAssignmentMap map, bool isApprove)
		{
			path.Clear();

			processMapType = GraphHelper.GetType(map.EntityType);
			Type itemType = item.GetType();

			PXSelectBase<EPAssignmentRoute> rs = new
				PXSelectReadonly<EPAssignmentRoute,
				Where<EPAssignmentRoute.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>,
				And<EPAssignmentRoute.parent, IsNull>>,
				OrderBy<Asc<EPAssignmentRoute.sequence>>>(this);

			PXResultset<EPAssignmentRoute> routes = rs.Select(map.AssignmentMapID, null);
            Type mapgraphtype = GraphHelper.GetType(map.GraphType);

            //impossible, but written to support purpose of legacy db states
            if (mapgraphtype == null)
                mapgraphtype = EntityHelper.GetPrimaryGraphType(this, processMapType);

            if (_Graph != null && mapgraphtype.IsAssignableFrom(_Graph.GetType()))
		    {
                processGraph = _Graph;
		    }
		    else
		    {
                processGraph = CreateInstance(GraphHelper.GetType(map.GraphType)?? mapgraphtype);
		    }
			
			if (processGraph != null && processMapType != null)
			{
				if (processMapType.IsAssignableFrom(itemType))
					this.processGraph.Caches[itemType].Current = item;
				else if (itemType.IsAssignableFrom(processMapType))
				{
					object placed = this.processGraph.Caches[processMapType].CreateInstance();
					PXCache cache = (PXCache)Activator.CreateInstance(typeof(PXCache<>).MakeGenericType(itemType), this);
					cache.RestoreCopy(placed, item);
					this.processGraph.Caches[processMapType].Current = placed;
				}
				else
					return null;
			}

			var result =  ProcessLevel(item, map.AssignmentMapID, routes).ToList();

			if (result.Any()) return result;

			PXTrace.WriteWarning(Messages.DocumentPreApproved);
			throw new RequestApproveException();
		}

		private IEnumerable<ApproveInfo> ProcessLevel(Table item, int? assignmentMap, PXResultset<EPAssignmentRoute> routes)
		{
			PXSelectReadonly<EPAssignmentRoute,
				Where<EPAssignmentRoute.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>,
					And<EPAssignmentRoute.parent, Equal<Required<EPAssignmentRoute.assignmentRouteID>>>>, OrderBy<Asc<EPAssignmentRoute.sequence>>> rs = new PXSelectReadonly<EPAssignmentRoute, Where<EPAssignmentRoute.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>, And<EPAssignmentRoute.parent, Equal<Required<EPAssignmentRoute.assignmentRouteID>>>>, OrderBy<Asc<EPAssignmentRoute.sequence>>>(this);

			foreach (EPAssignmentRoute route in routes)
			{
				if (route.AssignmentRouteID == null) continue;

				path.Add(route.AssignmentRouteID.Value);

				if (IsPassed(item, route))
				{
					if (route.WorkgroupID != null || route.OwnerID != null || route.OwnerSource != null)
					{
						Guid? OwnerID = null;
						int? WorkgroupID = route.WorkgroupID;

						if (route.OwnerSource != null)
						{
							PXGraph graph = processGraph;
							string code = PXTemplateContentParser.Instance.Process(route.OwnerSource,
								graph, 
								typeof(Table), 
								null
								);
							EPEmployee emp =
							PXSelect<EPEmployee, Where<EPEmployee.acctCD, Equal<Required<EPEmployee.acctCD>>>>
								.SelectWindowed(this, 0, 1, code);
							OwnerID = emp != null ? emp.UserID : GUID.CreateGuid(code);
						}

						if (OwnerID == null)
							OwnerID = route.OwnerID;

						if(route.UseWorkgroupByOwner == true && WorkgroupID != null && OwnerID != null)
						{
							EPCompanyTreeMember member = 
							PXSelectJoin<EPCompanyTreeMember,
								InnerJoin<EPCompanyTreeH, On<EPCompanyTreeH.workGroupID, Equal<EPCompanyTreeMember.workGroupID>>,
								InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>>>,
								Where<EPCompanyTreeH.parentWGID, Equal<Required<EPCompanyTreeH.parentWGID>>,
									And<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>>>>
									.SelectWindowed(this, 0, 1, WorkgroupID, OwnerID);
							if (member != null)
								WorkgroupID = member.WorkGroupID;
						}

						if (WorkgroupID != null && OwnerID == null)
						{
							EPCompanyTreeMember owner = PXSelectJoin<EPCompanyTreeMember,
								InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>>,
                                Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
									And<EPCompanyTreeMember.isOwner, Equal<boolTrue>>>>.Select(this, WorkgroupID);

							if (owner != null)
								OwnerID = owner.UserID;
						}
						
						PXTrace.WriteInformation(Messages.ProcessRouteSequence, route.Sequence);
						return new ApproveInfo()
						{
							OwnerID = OwnerID,
							WorkgroupID = WorkgroupID,
							WaitTime = route.WaitTime,
						}.AsSingleEnumerable();
					}

					if (route.RouteID != null && !path.Contains(route.RouteID.Value))
						return ProcessLevel(item, assignmentMap, PXSelectReadonly<EPAssignmentRoute>.Search<EPAssignmentRoute.assignmentRouteID>(this, route.RouteID));
					
					PXResultset<EPAssignmentRoute> result = rs.Select(assignmentMap, route.AssignmentRouteID);
					return ProcessLevel(item, assignmentMap, result);
				}
			}

			return Enumerable.Empty<ApproveInfo>();
		}

		private bool IsPassed(Table item, EPAssignmentRoute route)
		{
			try
			{				
				List<EPAssignmentRule> rules = new List<EPAssignmentRule>();
				foreach(EPAssignmentRule rule in PXSelectReadonly<EPAssignmentRule, 
					Where<EPAssignmentRule.assignmentRouteID, Equal<Required<EPAssignmentRoute.assignmentRouteID>>>>.Select(this,route.AssignmentRouteID))
					rules.Add(rule);

				if (rules.Count == 0) return true;
				switch (route.RuleType)
				{
					case RuleType.AllTrue:
						return rules.All(rule => IsTrue(item, rule));
					case RuleType.AtleastOneConditionIsTrue:
						return rules.Any(rule => IsTrue(item, rule));
					case RuleType.AtleastOneConditionIsFalse:
						return rules.Any(rule => !IsTrue(item, rule));
					default:
						return false;
				}
			}
			catch
			{
				return false;
			}
		}

		private bool IsTrue(Table item, EPAssignmentRule rule)
		{
			var target = GetItemRecord(rule, item); // gets actual item for the rule to be applied to
			return 
				rule.FieldName.EndsWith("_Attributes")?
				IsAttributeRuleTrue(target, rule) :
				IsItemRuleTrue(target, rule);
		}

		//Refactor - Generalize if possible!!!
		private IBqlTable GetItemRecord(EPAssignmentRule rule, IBqlTable item)
		{
			PXGraph graph = this.processGraph;
			Type itemType = item.GetType();
			Type ruleType = GraphHelper.GetType(rule.Entity);

			if (ruleType.IsAssignableFrom(itemType)) return item;
			if (processMapType.IsAssignableFrom(ruleType) && graph != null) 
				return graph.Caches[processMapType].Current as IBqlTable;

			if (graph != null)
			{
				foreach (CacheEntityItem entry in EMailSourceHelper.TemplateEntity(this, null, item.GetType().FullName, graph.GetType().FullName))
				{
					Type entityType = GraphHelper.GetType(entry.SubKey);
					if (ruleType.IsAssignableFrom(entityType) && graph.Views.ContainsKey(entry.Key))
					{
						PXView view = graph.Views[entry.Key];
						object result = view.SelectSingleBound(new object[] {item});
						return (result is PXResult ? ((PXResult) result)[0] : result) as IBqlTable;
					}
				}
			}
			return item;
		}

		private bool IsItemRuleTrue(IBqlTable item, EPAssignmentRule rule)
		{
			if (item == null) return false;

			if (item is EPEmployee && rule.FieldName.Equals(typeof(EPEmployee.workgroupID).Name, StringComparison.InvariantCultureIgnoreCase))
			{
				return IsEmployeeInWorkgroup((EPEmployee)item, rule);
			}

			currentItem = item;
			Type viewType = BqlCommand.Compose(typeof(Select<>), item.GetType());
			PXView itemView = new PXView(this, false, BqlCommand.CreateInstance(viewType),
				(PXSelectDelegate)getItemRecord);

			if (rule.Condition == null) return false;

			PXFilterRow filter = new PXFilterRow(
				rule.FieldName,
				(PXCondition)rule.Condition.Value,
				GetFieldValue(item, rule.FieldName, rule.FieldValue),
				null);
			int startRow = 0;
			int totalRows = 0;

			List<object> result = itemView.Select(null, null, null, null, null, new PXFilterRow[] { filter }, ref startRow, 1, ref totalRows);

			return result.Count > 0;
		}

		private bool IsEmployeeInWorkgroup(EPEmployee employee, EPAssignmentRule rule)
		{			
			PXFilterRow filter = new PXFilterRow(
				typeof(EPCompanyTree.description).Name,
				(PXCondition)rule.Condition.Value,
				rule.FieldValue,
				null);

			PXSelectBase<EPCompanyTree> select = new PXSelectJoin<EPCompanyTree,
					InnerJoin<EPCompanyTreeMember, On<EPCompanyTree.workGroupID, Equal<EPCompanyTreeMember.workGroupID>>>, 
					Where<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>,
						And<EPCompanyTreeMember.active, Equal<True>>>>(this);

			int startRow = 0;
			int totalRows = 0;

			List<object> result = 
				select.View.Select(null, new object[]{employee.UserID}, null, null, null, new PXFilterRow[] { filter }, ref startRow, 1, ref totalRows);
				//select.SelectSingle(employee.UserID, workgroupID);
			return result.Count > 0;			
		}

		private bool IsAttributeRuleTrue(object item, EPAssignmentRule rule)
		{
			string field = rule.FieldName.Substring(0, rule.FieldName.Length - "_Attribute".Length-1);
			CSAttribute attribute = PXSelectReadonly<CSAttribute>.Search<CSAttribute.attributeID>(this, field);

			if (attribute == null || rule.Condition == null)
				//Field Name is not a valid question.
				return false;

		    var noteId = new EntityHelper(_Graph).GetEntityNoteID(item);

			CSAnswers ans = PXSelect<CSAnswers,
				Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>,
				And<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>>.Select(_Graph ?? this, noteId, field);

			if (ans == null)
			{
				//Answer for the given question doesnot exist.
				switch (rule.Condition.Value)
				{
					case (int)PXCondition.ISNULL:
						return true;				
					case (int)PXCondition.ISNOTNULL:
						return false;				
					case (int) PXCondition.EQ:
						return string.IsNullOrEmpty(rule.FieldValue);				
					case (int) PXCondition.NE:
						return !string.IsNullOrEmpty(rule.FieldValue);
				}
				return false;
			}

			this.currentAttribute = ans;

			PXFilterRow filter = new PXFilterRow(typeof(CSAnswers.value).Name, (PXCondition)rule.Condition.Value, rule.FieldValue, null);
			int startRow = 0;
			int totalRows = 0;

			List<object> result = attributeView.Select(null, null, null, null, null, new PXFilterRow[] { filter }, ref startRow, 1, ref totalRows);

			return result.Count > 0;
		}

		private IEnumerable getItemRecord()
		{
			yield return currentItem;
		}

		private IEnumerable getAttributeRecord()
		{
			yield return currentAttribute;
		}

		private object GetFieldValue(IBqlTable item, string fieldname, string fieldvalue = null)
		{
		    try
		    {
                PXCache sourceCache = this.Caches[item.GetType()];
                object copy = sourceCache.CreateCopy(item);
                if (fieldvalue != null)
                    sourceCache.SetValueExt(copy, fieldname, fieldvalue);
                else
                {
                    object newValue;
                    sourceCache.RaiseFieldDefaulting(fieldname, copy, out newValue);
                    sourceCache.SetValue(copy, fieldname, newValue);
                }
                return sourceCache.GetValueExt(copy, fieldname);
		    }
		    catch (Exception)
		    {
		        return fieldvalue;		        
		    }
			
		}
	}

	public class CRAssigmentScope : IDisposable
	{
		private readonly PX.Data.EP.IAssign source;
		private readonly int? workgroupID;
		private readonly Guid? ownerID;
		public CRAssigmentScope(PX.Data.EP.IAssign source)
		{
			this.source = source;
			this.workgroupID = source.WorkgroupID;
			this.ownerID = source.OwnerID;
		}
		#region IDisposable Members
		public virtual void Dispose()
		{
			this.source.WorkgroupID = workgroupID;
			this.source.OwnerID = ownerID;
		}
		#endregion
	}
}
