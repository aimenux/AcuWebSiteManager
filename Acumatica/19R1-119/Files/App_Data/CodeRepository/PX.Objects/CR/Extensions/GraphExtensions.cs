using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.EP;

namespace PX.Objects.CR
{
	[Obsolete("Will be removed in 7.0 version")]
	internal static class GraphExtensions
	{
		public static PXView GetView(this PXGraph graph, Type fieldType, BqlCommand select)
		{
			PXView view;
			graph.Views.TryGetValue(fieldType.FullName, out view);

			if (view == null)
			{
				view = new PXView(graph, false, @select);
				graph.Views.Add(fieldType.FullName, view);
				graph.EnsureCachePersistence(fieldType.DeclaringType);
			}
			return view;
		}

		public static List<object> SelectById(this PXGraph graph, Type fieldType, object id)
		{
			var select = Utils.CreateSelectCommand(fieldType);
			var view = GetView(graph, fieldType, select);
			return view.SelectMulti(id);
		}

		public static List<T> SelectById<T, TField>(this PXGraph graph, object id)
			where TField : class,IBqlField
			where T : class,IBqlTable
		{
			return SelectById(graph, typeof(TField), id).Cast<T>().ToList();
		}


		public static object SelectFirst(this PXGraph graph, Type fieldType, object id)
		{
			PXCache cache = graph.Caches[fieldType.DeclaringType];
			object result = cache.Cached
								 .Cast<object>()
								 .FirstOrDefault(i => cache.GetValue(i,fieldType.Name).Equals(id))
							?? 
							SelectById(graph, fieldType, id).FirstOrDefault();

			if (result == null)
				throw new Exception(String.Format("Cant find {0} with value = {1}", fieldType, id));

			return result;
		}

		public static T SelectFirst<T, TField>(this PXGraph graph, object id)
			where TField : class,IBqlField
			where T : class,IBqlTable
		{
			return (T)SelectFirst(graph, typeof(TField), id);
		}
	}
}
