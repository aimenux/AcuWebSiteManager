using System;
using System.Reflection;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CR
{
	internal abstract class CRListAttributesHelper
	{
		#region String List
		private class CRStringListAttHelper : CRListAttributesHelper
		{
			PXStringListAttribute _listAtt;
			public CRStringListAttHelper(PXStringListAttribute listAtt)
				: base()
			{
				_listAtt = listAtt;
			}

			public override List<string> GetLabels(List<object> values)
			{
				List<string> result = new List<string>(values.Count);
				Dictionary<string, string> labels = _listAtt.ValueLabelDic;
				foreach (object item in values)
				{
					if (item == null) continue;
					string value = item.ToString();
					if (labels.ContainsKey(value)) result.Add(labels[value]);
				}
				return result;
			}
		}
		#endregion

		#region Int List
		private class CRIntListAttHelper : CRListAttributesHelper
		{
			PXIntListAttribute _listAtt;
			public CRIntListAttHelper(PXIntListAttribute listAtt)
				: base()
			{
				_listAtt = listAtt;
			}

			public override List<string> GetLabels(List<object> values)
			{
				List<string> result = new List<string>(values.Count);
				Dictionary<int, string> labels = _listAtt.ValueLabelDic;
				foreach (object item in values)
				{
					if (item == null) continue;
					int value = Convert.ToInt32(item);
					if (labels.ContainsKey(value)) result.Add(labels[value]);
				}
				return result;
			}
		}
		#endregion

		#region Table List
		private class CRBaseListAttHelper : CRListAttributesHelper
		{
			CRBaseListAttribute _listAtt;
			public CRBaseListAttHelper(CRBaseListAttribute listAtt)
				: base()
			{
				_listAtt = listAtt;
			}

			public override List<string> GetLabels(List<object> values)
			{
				List<string> result = new List<string>(values.Count);

				Dictionary<object, string> labels = _listAtt.ValueLabelDic(Graph);

				foreach (object item in values)
					if (item != null && labels.ContainsKey(item)) result.Add(labels[item]);

				return result;
			}
		}
		#endregion

		#region Fields
		protected PXGraph Graph;
		#endregion

		#region Ctor
		private CRListAttributesHelper() { }

		public static CRListAttributesHelper CreateFrom(PXGraph graph, PXCache fieldCache, string field)
		{
			CRListAttributesHelper result = null;
			PXStringListAttribute stringListAtt = GetAttribute<PXStringListAttribute>(fieldCache, field);
			if (stringListAtt != null) result = new CRStringListAttHelper(stringListAtt);
			PXIntListAttribute intListAtt = GetAttribute<PXIntListAttribute>(fieldCache, field);
			if (intListAtt != null) result = new CRIntListAttHelper(intListAtt);
			CRBaseListAttribute baseListAtt = GetAttribute<CRBaseListAttribute>(fieldCache, field);
			if (baseListAtt != null) result = new CRBaseListAttHelper(baseListAtt);
			if (result != null) result.Graph = graph;
			return result;
		}

		private static TAtt GetAttribute<TAtt>(PXCache cache, string memberInfo)
			where TAtt : PXEventSubscriberAttribute
		{
			foreach (PXEventSubscriberAttribute attribute in cache.GetAttributes(memberInfo))
				if (attribute is TAtt) return (TAtt)attribute;
			return null;
		}
		#endregion

		#region List
		public abstract List<string> GetLabels(List<object> values);
		#endregion
	}
}
