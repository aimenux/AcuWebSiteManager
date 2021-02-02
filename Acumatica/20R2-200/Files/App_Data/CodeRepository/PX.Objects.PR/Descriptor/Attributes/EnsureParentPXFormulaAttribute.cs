using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class EnsureParentPXFormulaAttribute : PXFormulaAttribute
	{
		private Type[] _ParentFields;
		private Type[] _ChildFields;

		public EnsureParentPXFormulaAttribute(Type formula, Type aggregate, Type[] parentFields, Type[] childFields) : base(formula, aggregate) 
		{
			_ParentFields = parentFields;
			_ChildFields = childFields;
		}

		protected override object EnsureParent(PXCache cache, object Row, object NewRow)
		{
			object parent = base.EnsureParent(cache, Row, NewRow);
			if (parent != null)
			{
				return parent;
			}
			
			if (_ParentFields.Length != _ChildFields.Length)
			{
				return null;
			}

			PXCache parentCache = cache.Graph.Caches[BqlCommand.GetItemType(_ParentFieldType)];
			parent = parentCache.CreateInstance();
			for (int i = 0; i < _ParentFields.Length; i++)
			{
				parentCache.SetValue(parent, _ParentFields[i].Name, cache.GetValue(Row, _ChildFields[i].Name));
			}

			return parentCache.Insert(parent);
		}
	}
}
