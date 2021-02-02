using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PX.Data;
using PX.Data.MassProcess;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public class UpdateWizardSummaryAttribute : PXWizardSummaryAttribute
	{
		protected override List<Tuple<string, object>> CollectValues(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXCache<FieldValue> fvcache = sender.Graph.Caches<FieldValue>();
			return (fvcache.Inserted.OfType<FieldValue>()
				.Where(f => f.Hidden != true && f.Selected == true)
				.OrderBy(f => f.Order)
				.Select(field => new {field, state = fvcache.GetStateExt<FieldValue.value>(field) as PXFieldState})
				.Where(@t => @t.state != null)
				.Select(@t => new Tuple<string, object>(@t.field.DisplayName, StateValue(@t.state as PXStringState) ?? StateValue(@t.state as PXIntState) ?? @t.state.Value))).ToList();
		}
	}

	[Serializable]
	[PXHidden]
	public partial class UpdateSummary : IBqlTable
	{
		#region Summary
		public abstract class summary : PX.Data.BQL.BqlString.Field<summary> { }
		[UpdateWizardSummary(IsUnicode = true)]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual string Summary { get; set; }
		#endregion
	}

	public abstract class CRBaseUpdateProcess<TGraph, TPrimary, TMarkAttribute, TClassField> : CRBaseMassProcess<TGraph, TPrimary>, IMassProcess<TPrimary>
		where TGraph : PXGraph, IMassProcess<TPrimary>, new()
		where TPrimary : class, IBqlTable, IPXSelectable, new() 
		where TMarkAttribute: PXEventSubscriberAttribute
		where TClassField : IBqlField
    {
		public PXSelect<FieldValue, Where<FieldValue.attributeID, IsNull>, OrderBy<Asc<FieldValue.order>>> Fields;
		public PXSelect<FieldValue, Where<FieldValue.attributeID, IsNotNull>, OrderBy<Asc<FieldValue.order>>> Attributes; 
		public PXSelect<CSAnswers> answers;
		public PXFilter<UpdateSummary> wizardSummary; 

		protected CRBaseUpdateProcess()
		{
			//Init PXVirtual Static constructor
			Actions["Schedule"].SetVisible(false);
			typeof(FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);
			GetAttributeSuffixes(this, ref _suffixes);
		}

		public IEnumerable fields(PXAdapter adapter)
		{
			return Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().Where(row => row.AttributeID == null);
		}

		public IEnumerable attributes(PXAdapter adapter)
		{
			RestrictAttributesByClass();
			return Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().Where(row => row.AttributeID != null && row.Hidden != true);
		}

		protected virtual void FieldValue_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			FieldValue val = (FieldValue)e.NewRow;
			FieldValue oldValue = (FieldValue)e.Row;
			val.Selected = val.Selected == true || (oldValue.Value != val.Value);
		}

		protected virtual void FieldValue_Value_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = PXMassProcessHelper.InitValueFieldState(Caches[typeof(TPrimary)], e.Row as FieldValue);
		}

		private readonly List<string> _suffixes; 

		protected static void GetAttributeSuffixes(PXGraph graph , ref List<string> suffixes)
		{
			suffixes = suffixes ?? new List<string>(graph.Caches[typeof(TPrimary)].BqlTable
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.SelectMany(p => p.GetCustomAttributes(true).Where(atr => atr is PXDBAttributeAttribute), (p, atr) => p.Name));
		}

		public static IEnumerable<FieldValue> GetMarkedProperties(PXGraph graph, ref int firstSortOrder)
		{
			PXCache cache = graph.Caches[typeof(TPrimary)];
			int order = firstSortOrder;
			List<FieldValue> res = (cache.Fields.Where(
				fieldname => cache.GetAttributesReadonly(fieldname).OfType<TMarkAttribute>().Any())
				.Select(fieldname => new {fieldname, state = cache.GetStateExt(null, fieldname) as PXFieldState})
				.Where(@t => @t.state != null)
				.Select(@t => new FieldValue()
				{
					Selected = false,
					CacheName = typeof (TPrimary).FullName,
					Name = @t.fieldname,
					DisplayName = @t.state.DisplayName,
					AttributeID = null,
					Order = order++
				})).ToList();

			firstSortOrder = order;
			return res;
		}

		public static IEnumerable<FieldValue> GetAttributeProperties(PXGraph graph, ref int firstSortOrder)
		{
			return GetAttributeProperties(graph, ref firstSortOrder, null);
		}

		public static IEnumerable<FieldValue> GetAttributeProperties(PXGraph graph, ref int firstSortOrder, List<string> suffixes)
		{
			PXCache cache = graph.Caches[typeof(TPrimary)];
			int order = firstSortOrder;
			List<FieldValue> res = new List<FieldValue>();
			GetAttributeSuffixes(graph, ref suffixes);
			foreach (string field in graph.Caches[typeof(TPrimary)].Fields)
			{
				if (!suffixes.Any(suffix => field.EndsWith(string.Format("_{0}", suffix)))) continue;
				PXFieldState state = cache.GetStateExt(null, field) as PXFieldState;
				if (state == null) continue;

				string displayName = state.DisplayName;
				string attrID = field;
				string local = field;
				foreach (string suffix in suffixes.Where(suffix => local.EndsWith(string.Format("_{0}", suffix))))
				{
					attrID = field.Replace(string.Format("_{0}", suffix), string.Empty);
					displayName = state.DisplayName.Replace(string.Format("${0}$-", suffix), string.Empty);
					break;
				}
				res.Add( new FieldValue
					{
						Selected = false,
						CacheName = typeof(TPrimary).FullName,
						Name = field,
						DisplayName = displayName,
						AttributeID = attrID,
						Order = order++ + 1000
					});
			}
			firstSortOrder = order;
			return res;
		}

		public static IEnumerable<FieldValue> GetProcessingProperties(PXGraph graph, ref int firstSortOrder)
		{
			return GetProcessingProperties(graph, ref firstSortOrder, null);
		}

		public static IEnumerable<FieldValue> GetProcessingProperties(PXGraph graph, ref int firstSortOrder, List<string> suffixes)
		{
			return GetMarkedProperties(graph, ref firstSortOrder).Union(GetAttributeProperties(graph, ref firstSortOrder, suffixes));
		}

		protected virtual IEnumerable<FieldValue> ProcessingProperties
		{
			get
			{
				int _firstOrder = 0;
				return GetProcessingProperties(this, ref _firstOrder, _suffixes);
			}
		}

		protected void FillPropertyValue(PXGraph graph, string viewName)
		{
			PXCache cache = Caches[typeof(FieldValue)];
			cache.Clear();
			foreach (FieldValue field in ProcessingProperties)
			{
				cache.Insert(field);
			}
			cache.IsDirty = false;
		}

		protected Dictionary<string, bool> GetClassAttributes(string ClassID)
		{
		    return PXSelectJoin<CSAttributeGroup,
		        InnerJoin<CSAttribute, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
		        Where<CSAttributeGroup.entityClassID, Equal<Required<TClassField>>,
		            And<CSAttributeGroup.entityType, Equal<Required<CSAttributeGroup.entityType>>>>,
		        OrderBy<Asc<CSAttributeGroup.sortOrder>>>
		        .Select(this, ClassID, typeof (TClassField).ReflectedType.FullName)
		        .RowCast<CSAttributeGroup>()
		        .ToDictionary(g => g.AttributeID, g => g.Required == true);
		}

		public virtual void RestrictAttributesByClass()
		{
			PXCache<TPrimary> cache = this.Caches<TPrimary>();
			HashSet<object> classes = new HashSet<object>(cache.Updated.Cast<TPrimary>().Where(entity => entity.Selected == true).Select(cache.GetValue<TClassField>).ToList());
			FieldValue classFld = Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().FirstOrDefault(field => String.Equals(field.Name, typeof(TClassField).Name, StringComparison.CurrentCultureIgnoreCase));
			string classID = (classFld == null || classFld.Selected == false || classFld.Value == null ? null : classFld.Value) ?? (classes.Count == 1 ? classes.FirstOrDefault() as string : null);
			Dictionary<string, bool> classAttrs = GetClassAttributes(classID);
			foreach (FieldValue attr in Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().Where(f => f.AttributeID != null))
			{
				attr.Hidden = !classAttrs.ContainsKey(attr.AttributeID);
				attr.Required = !(bool)attr.Hidden && classAttrs[attr.AttributeID];
			}

			Attributes.AllowSelect = !string.IsNullOrEmpty(classID);
		}

		protected override bool AskParameters()
		{
			return Fields.AskExt(FillPropertyValue) == WebDialogResult.OK;
		}

		public override void ProccessItem(PXGraph graph, TPrimary item)
		{
			PXCache cache = graph.Caches[typeof (TPrimary)];
			TPrimary newItem = (TPrimary)cache.CreateInstance();
			PXCache<TPrimary>.RestoreCopy(newItem, item);

            var refNoteIdField = EntityHelper.GetNoteField(cache.GetItemType());

            PXView primaryView = graph.Views[graph.PrimaryView];
			object[] searches = new object[primaryView.Cache.BqlKeys.Count];
			string[] sortcolumns = new string[primaryView.Cache.BqlKeys.Count];
			for (int i = 0; i < cache.BqlKeys.Count(); i++)
			{
				sortcolumns[i] = cache.BqlKeys[i].Name;
				searches[i] = cache.GetValue(newItem, sortcolumns[i]);
			}
			int startRow = 0, totalRows = 0;
			
			List<object> result = primaryView.Select(null, null, searches, sortcolumns, null, null, ref startRow, 1, ref totalRows);
			newItem = (TPrimary)cache.CreateCopy(PXResult.Unwrap<TPrimary>(result[0]));

			foreach (FieldValue fieldValue in Fields.Cache.Cached.Cast<FieldValue>().Where(o => o.AttributeID == null && o.Selected == true))
			{								
				PXFieldState state = cache.GetStateExt(newItem, fieldValue.Name) as PXFieldState;
				PXIntState intState = state as PXIntState;
				PXStringState strState = state as PXStringState;
				if ((intState != null && intState.AllowedValues != null && intState.AllowedValues.Length > 0 &&
					intState.AllowedValues.All(v => v != int.Parse(fieldValue.Value))) 
					||
					(strState != null && strState.AllowedValues != null && strState.AllowedValues.Length > 0 &&
					strState.AllowedValues.All(v => v != fieldValue.Value)))
				{					
					throw new PXSetPropertyException(ErrorMessages.UnallowedListValue, fieldValue.Value, fieldValue.Name);
				}
				if (state != null && !Equals(state.Value, fieldValue.Value))
				{
					cache.SetValueExt(newItem, fieldValue.Name, fieldValue.Value);
					cache.Update(newItem);
				}

				result = primaryView.Select(null, null, searches, sortcolumns, null, null, ref startRow, 1, ref totalRows);
				newItem = (TPrimary)cache.CreateCopy(PXResult.Unwrap<TPrimary>(result[0]));
			}

			PXCache attrCache = cache.Graph.Caches[typeof(CSAnswers)];

			foreach (FieldValue attrValue in Attributes.Cache.Cached.Cast<FieldValue>().Where(o => o.AttributeID != null && o.Selected == true))
			{
				CSAnswers attr = (CSAnswers)attrCache.CreateInstance();
				attr.AttributeID = attrValue.AttributeID;
			    attr.RefNoteID = cache.GetValue(newItem, refNoteIdField) as Guid?;
                attr.Value = attrValue.Value;
				attrCache.Update(attr);
			}
		}

		public PXAction<TPrimary> WizardNext;
		[PXUIField(DisplayName = "Next", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable wizardNext(PXAdapter adapter)
		{
			return adapter.Get();
		}
	}
}
