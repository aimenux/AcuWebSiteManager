using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PX.Common;
using PX.Data;

namespace PX.Objects.CR.MassProcess
{
	public abstract class MassProcessPropertyValueAttribute : MassProcessAttribute
	{
		private void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var val = (PropertyValue)e.Row;
			var oldValue = (PropertyValue)e.OldRow;
			if (Equals(val.Value, oldValue.Value)) return;

			var itemsCache = View.Cache;
			foreach (PropertyValue item in sender.Cached.
				Cast<PropertyValue>().
				Where(_ => _.Order > val.Order).
				OrderBy(_ => _.Order))
			{
				if (string.IsNullOrWhiteSpace(item.Name)) continue;

				var propertyInfo = itemsCache.GetType().GetProperty(item.Name);
				if (propertyInfo == null) continue;

				var verifier = Attribute.GetCustomAttribute(propertyInfo, PropertyMarkAttribute) as PXMassProcessFieldAttribute;
				if (verifier == null || verifier.SearchCommand == null) continue;

				var searchCommand = (BqlCommand)Activator.CreateInstance(verifier.SearchCommand);
				var verifingParams = new List<object>();
				var itemOrder = item.Order;
				foreach(IBqlParameter param in searchCommand.GetParameters())
				{
					var refType = param.GetReferencedType();
					if (refType == null) continue;

					var propVal = sender.Cached.Cast<PropertyValue>().FirstOrDefault(_ => _.Order < itemOrder && Equals(refType.Name, _.Name));
					verifingParams.Add(propVal.With(_ => _.Value));
				}

				int startRow = 0;
				int totalRows = 0;
				var searchResult = new PXView(Graph, true, searchCommand).
									Select(null, verifingParams.ToArray(),
										new object[] { item.Value },
										new string[] { ((IBqlSearch)searchCommand).GetField().Name },
										new bool[] { false },
										null, ref startRow, 1, ref totalRows);
				if (searchResult == null || searchResult.Count == 0)
				{
					item.Value = null;
				}
			}
		}

		protected abstract string PropertiesViewName { get; }

		protected abstract Type PropertyMarkAttribute { get; }

		protected virtual IEnumerable<string> EditableProperties
		{
			get
			{
				return View.Cache.BqlTable
						.GetProperties(BindingFlags.Instance | BindingFlags.Public)
						.SelectMany(p => p.GetCustomAttributes(true).Where(atr => PropertyMarkAttribute.IsInstanceOfType(atr)),
									(p, atr) => p.Name)
					  .Union(View.Cache.Fields.Where(o=>o.EndsWith("_Attributes")));
			}
		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			base.ViewCreated(graph, viewName);

			CheckPropertiesViewName();
			CheckPropertyMarkAttribute();

			InitializePropertyValueView(graph);

			AttachEventHandlers(graph);
		}

		private void InitializePropertyValueView(PXGraph graph)
		{
			//Init PXVirtual Static constructor
			typeof (PropertyValue).GetCustomAttributes(typeof (PXVirtualAttribute), false);

			var propertiesSelect = new PXSelectOrderBy<PropertyValue,
				OrderBy<Asc<PropertyValue.order>>>(graph, 
				new PXSelectDelegate(() => graph.Caches[typeof(PropertyValue)].Cached.Cast<PropertyValue>().Where(item => item.Hidden != true)));
			graph.Views.Add(PropertiesViewName, propertiesSelect.View);
			if (View.Cache.Fields.Any(o=>o.EndsWith("_Attributes")) && !graph.Views.Caches.Contains(typeof(CS.CSAnswers)))
			{
					graph.Views.Caches.Add(typeof(CS.CSAnswers));
			}
		}

		private void AttachEventHandlers(PXGraph graph)
		{
			graph.FieldSelecting.AddHandler(typeof (PropertyValue), typeof (PropertyValue.value).Name, FieldSelecting);
			graph.RowUpdating.AddHandler(typeof (PropertyValue), RowUpdating);
			graph.RowUpdated.AddHandler(typeof (PropertyValue), RowUpdated);
			//graph.RowUpdated.AddHandler(View.Cache.GetItemType(), (sender, e) => sender.Graph.Caches[typeof (PropertyValue)].Clear());
			graph.RowUpdated.AddHandler(Operations.Cache.GetItemType(), (sender, e) => sender.Graph.Caches[typeof(PropertyValue)].Clear());
		}

		private void CheckPropertiesViewName()
		{
			if (string.IsNullOrWhiteSpace(PropertiesViewName))
				throw new ArgumentNullException("PropertiesViewName");
		}

		private void CheckPropertyMarkAttribute()
		{
			if (PropertyMarkAttribute == null)
				throw new ArgumentNullException("PropertyMarkAttribute");
		}

		private void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var val = (PropertyValue)e.NewRow;
			var oldValue = (PropertyValue)e.Row;
			val.Selected = val.Selected == true || (oldValue.Value != val.Value);
		}

		private void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (CurrentAction != ActionName) return;

			var val = e.Row as PropertyValue;

			e.ReturnState = val == null ? InitDefaultValueFieldSate() : InitValueFieldState(val);
		}

		private PXFieldState InitDefaultValueFieldSate()
		{
			var res = PXStringState.CreateInstance(null, null, null, typeof(PropertyValue.value).Name,
				false, 0, null, null, null, null, null);
			res.DisplayName = Messages.PropertyValue;
			return res;
		}

		protected abstract PXFieldState InitValueFieldState(PropertyValue field);

		protected override bool AskAdditionalParameters(IEnumerable<object> targets)
		{	
			return PXView.AskExt(Graph, PropertiesViewName, null, (graph, name) => FillPropertyValue()) == WebDialogResult.OK;			
		}

		protected void FillPropertyValue()
		{
			var itemsCache = View.Cache;
			var cache = Graph.Caches[typeof(PropertyValue)];
			cache.Clear();
			int order = 0;
			foreach (string property in EditableProperties)
			{
				var state = itemsCache.GetStateExt(null, property) as PXFieldState;
				var displayName = state.With(_ => _.DisplayName).Replace("$Attributes$-", "");
				InsertPropertyValue(new PropertyValue
				{
					Selected = false,
					Name = property,
					DisplayName = string.IsNullOrWhiteSpace(displayName) ? property : displayName,
					Order = order++
				});
			}
			cache.IsDirty = false;	
		}

		protected virtual void InsertPropertyValue(PropertyValue field)
		{
			Graph.Caches[typeof(PropertyValue)].Insert(field);
		}

		protected IEnumerable<PropertyValue> GetPropertyValues()
		{
			return Graph.Views[PropertiesViewName].Cache.Cached
				.Cast<PropertyValue>()
				.Where(p => p.Selected == true);
		}
	}
}
