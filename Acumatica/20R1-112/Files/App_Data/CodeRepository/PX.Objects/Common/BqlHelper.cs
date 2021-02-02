using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace PX.Objects.Common
{
	public static class BqlHelper
	{
        public abstract class fieldStub : PX.Data.IBqlField { }

		private static readonly Dictionary<Tuple<Type, Type>, bool> ParametersEqualityCache =
			new Dictionary<Tuple<Type, Type>, bool>();

		/// <summary>
		/// Ensures that the first command's parameters have the same type as
		/// the second command's parameters. Can be helpful to keep graph 
		/// views and selects inside their delegates synchronized in terms 
		/// of BQL parameters.
		/// </summary>
		public static void EnsureParametersEqual(this BqlCommand firstCommand, BqlCommand secondCommand)
		{
			if (firstCommand == null) throw new ArgumentNullException(nameof(firstCommand));
			if (secondCommand == null) throw new ArgumentNullException(nameof(secondCommand));

			Tuple<Type, Type> cacheLookupKey = Tuple.Create(firstCommand.GetSelectType(), secondCommand.GetSelectType());
			Tuple<Type, Type> reverseCacheLookupKey = Tuple.Create(cacheLookupKey.Item2, cacheLookupKey.Item1);

			bool checkResult;

			if (!ParametersEqualityCache.ContainsKey(cacheLookupKey) &&
				!ParametersEqualityCache.ContainsKey(reverseCacheLookupKey))
			{
				IBqlParameter[] firstCommandParameters = firstCommand.GetParameters();
				IBqlParameter[] secondCommandParameters = secondCommand.GetParameters();

				if (firstCommandParameters.Length != secondCommandParameters.Length)
				{
					checkResult = false;
				}
				else
				{
					checkResult = firstCommandParameters
						.Zip(secondCommandParameters, (x, y) => x.GetType() == y.GetType())
						.All(x => x);
				}

				ParametersEqualityCache[cacheLookupKey] = checkResult;
			}
			else
			{
				checkResult = ParametersEqualityCache[cacheLookupKey];
			}

			if (!checkResult)
			{
				throw new PXException(Messages.BqlCommandsHaveDifferentParameters);
			}
		}

		public static IEnumerable<Type> GetDecimalFieldsAggregate<Table>(PXGraph graph)
			where Table : IBqlTable
		{
			return GetDecimalFieldsAggregate<Table>(graph, false);
		}

		public static IEnumerable<Type> GetDecimalFieldsAggregate<Table>(PXGraph graph, bool closing)
			where Table : IBqlTable
		{
			var result = new List<Type> { };

			PXCache cache = graph.Caches[typeof(Table)];
			if (cache == null)
				throw new PXException(Messages.FailedToGetCache, typeof(Table).FullName);

			List<Type> decimalFields = cache.BqlFields
				.Where(fieldType => cache.GetAttributesReadonly(cache.GetField(fieldType)).OfType<PXDBDecimalAttribute>().Any() || cache.GetAttributesReadonly(cache.GetField(fieldType)).OfType<PXDBCalcedAttribute>().Any())
				.ToList();

			for (int i = 0; i < decimalFields.Count; i++)
			{
				bool lastField = (i + 1 == decimalFields.Count);
				Type bqlField = decimalFields[i];
				result.Add(closing && lastField ? typeof(Sum<>) : typeof(Sum<,>));
				result.Add(bqlField);
			}

			return result;
		}

		public static object GetOperandValue(PXCache cache, object item, Type sourceType)
		{
			if (item == null)
				return null;

			if (typeof(IBqlField).IsAssignableFrom(sourceType))
			{
				if (BqlCommand.GetItemType(sourceType).IsAssignableFrom(cache.GetItemType()))
				{
					return cache.GetValue(item, sourceType.Name);
				}
			}

			throw new NotImplementedException();
		}

	    public static object GetOperandValue(PXGraph graph, object item, Type sourceType)
	    {
	        if (item == null)
	            return null;

	        if (typeof(IBqlField).IsAssignableFrom(sourceType))
	        {
	            Type cacheType = BqlCommand.GetItemType(sourceType);

	            PXCache cache = graph.Caches[cacheType];

	            return BqlHelper.GetOperandValue(cache, item, sourceType);
            }

	        throw new NotImplementedException();
	    }

        public static object GetOperandValue<TOperand>(PXCache cache, object item)
			where TOperand : IBqlOperand
		{
			return GetOperandValue(cache, item, typeof(TOperand));
		}

		public static object GetCurrentValue(PXGraph graph, Type sourceType, object row = null)
		{
			if (typeof(IBqlField).IsAssignableFrom(sourceType))
			{
				Type cacheType = BqlCommand.GetItemType(sourceType);

				PXCache cache = graph.Caches[cacheType];

				return BqlHelper.GetOperandValue(cache, row ?? cache.Current, sourceType);
			}

			throw new NotImplementedException();
		}

		public static object GetValuePendingOrRow<TField>(PXCache cache, object row)
			where TField : IBqlField
		{
			object value = cache.GetValuePending(row, typeof(TField).Name);

			if (value != PXCache.NotSetValue)
			{
				cache.RaiseFieldUpdating<TField>(row, ref value);

				return value;
			}

			return BqlHelper.GetOperandValue(cache, row, typeof(TField));
		}

		public static object GetValuePendingOrCurrent(PXCache cache, Type sourceType, object row)
	    {
	        object value = cache.GetValuePending(row, sourceType.Name);

	        if (value != PXCache.NotSetValue)
	        {
	            return value;
	        }

	        return BqlHelper.GetOperandValue(cache, row ?? cache.Current, sourceType);
        }

		public static object GetCurrentValue<TOperand>(PXGraph graph)
			where TOperand : IBqlOperand
		{
			return GetCurrentValue(graph, typeof(TOperand));
		}

		public static object GetParameterValue(PXGraph graph, IBqlParameter parameter)
		{
			if (parameter.HasDefault)
			{
				Type ft = parameter.GetReferencedType();

				if (ft.IsNested)
				{
					Type ct = BqlCommand.GetItemType(ft);
					PXCache paramcache = graph.Caches[ct];
					if (paramcache.Current != null)
					{
						return paramcache.GetValue(paramcache.Current, ft.Name);
					}
				}
			}

			return null;
		}

	    public static Type GetTypeNotStub(Type type)
	    {
	        if (type == typeof(BqlNone) || type == typeof(fieldStub))
	            return null;

	        return type;
	    }

	    public static Type GetTypeNotStub<T>()
	    {
	        return GetTypeNotStub(typeof(T));
	    }

		public readonly static Dictionary<Type, Type> SelectToSearch = new Dictionary<Type, Type>
		{
			{typeof(Select<>), typeof(Search<>)},
			{typeof(Select<,>), typeof(Search<,>)},
			{typeof(Select<,,>), typeof(Search<,,>)},
			{typeof(Select2<,>), typeof(Search2<,>)},
			{typeof(Select2<,,>), typeof(Search2<,,>)},
			{typeof(Select2<,,,>), typeof(Search2<,,,>)},
			{typeof(Select3<,>), typeof(Search3<,>)},
			{typeof(Select3<,,>), typeof(Search3<,,>)},
			{typeof(Select4<,>), typeof(Search4<,>)},
			{typeof(Select4<,,>), typeof(Search4<,,>)},
			{typeof(Select4<,,,>), typeof(Search4<,,,>)},
			{typeof(Select5<,,>), typeof(Search5<,,>)},
			{typeof(Select5<,,,>), typeof(Search5<,,,>)},
			{typeof(Select5<,,,,>), typeof(Search5<,,,,>)},
			{typeof(Select6<,,>), typeof(Search6<,,>)},
			{typeof(Select6<,,,>), typeof(Search6<,,,>)},
		};
    }

	public static class BqlExtensions
	{
		public static TTable SelectSingle<TTable>(this BqlCommand command, PXGraph graph, bool isReadonly, params object[] parameters)
			where TTable : IBqlTable
		{
			object data = command.CreateView(graph, mergeCache: !isReadonly).SelectSingle(parameters);
			PXResult result = data as PXResult;

			return result != null
				? (TTable)result[typeof(TTable)]
				: (TTable)data;
		}

		public static TTable SelectSingle<TTable>(this BqlCommand command, PXGraph graph, bool isReadonly, IBqlTable[] currents, params object[] parameters)
			where TTable : IBqlTable
		{
			object data = command.CreateView(graph, mergeCache: !isReadonly).SelectSingleBound(currents, parameters);
			PXResult result = data as PXResult;

			return result != null
				? (TTable)result[typeof(TTable)]
				: (TTable)data;
		}

		public static TTable SelectSingle<TTable>(this BqlCommand command, PXGraph graph, params object[] parameters)
			where TTable : IBqlTable
		{
			return command.SelectSingle<TTable>(graph, false, parameters);
		}

		public static TTable SelectSingleReadonly<TTable>(this BqlCommand command, PXGraph graph, params object[] parameters)
			where TTable : IBqlTable
		{
			return command.SelectSingle<TTable>(graph, true, parameters);
		}

		public static TTable SelectSingleReadonly<TTable>(this BqlCommand command, PXGraph graph, IBqlTable[] currents, params object[] parameters)
			where TTable : IBqlTable
		{
			return command.SelectSingle<TTable>(graph, true, currents, parameters);
		}

		public static IEnumerable<TTable> Select<TTable>(this BqlCommand command, PXGraph graph, bool isReadonly, params object[] parameters)
			where TTable : IBqlTable
		{
			return command.CreateView(graph, mergeCache: !isReadonly).SelectMulti(parameters).RowCast<TTable>();
		}

		public static IEnumerable<TTable> Select<TTable>(this BqlCommand command, PXGraph graph, params object[] parameters)
			where TTable : IBqlTable
		{
			return command.Select<TTable>(graph, false, parameters);
		}

		public static IEnumerable<TTable> SelectReadonly<TTable>(this BqlCommand command, PXGraph graph, params object[] parameters)
			where TTable : IBqlTable
		{
			return command.Select<TTable>(graph, true, parameters);
		}

		public static bool Any(this BqlCommand command, PXGraph graph, bool isReadonly, params object[] parameters)
		{
			return command.CreateView(graph, mergeCache: !isReadonly).SelectSingle(parameters) != null;
		}

		public static bool Any(this BqlCommand command, PXGraph graph, bool isReadonly, IBqlTable[] currents, params object[] parameters)
		{
			return command.CreateView(graph, mergeCache: !isReadonly).SelectSingleBound(currents, parameters) != null;
		}

		public static bool Any(this BqlCommand command, PXGraph graph, params object[] parameters)
		{
			return command.Any(graph, false, parameters);
		}

		public static bool Any(this BqlCommand command, PXGraph graph, IBqlTable[] currents, params object[] parameters)
		{
			return command.Any(graph, false, currents, parameters);
		}

		public static bool AnyReadonly(this BqlCommand command, PXGraph graph, params object[] parameters)
		{
			return command.Any(graph, true, parameters);
		}

		public static bool AnyReadonly(this BqlCommand command, PXGraph graph, IBqlTable[] currents, params object[] parameters)
		{
			return command.Any(graph, true, currents, parameters);
		}

		public static PXView CreateView(this BqlCommand command, PXGraph graph, bool clearQueryCache = false, bool mergeCache = false)
		{
			PXView view = new PXView(graph, !mergeCache, command);
			if (clearQueryCache)
			{
				view.Clear();
			}
			return view;
		}

	    public static object SelectFirst(this IBqlSearch command, PXGraph graph, object data, bool isReadOnly = true)
	    {
	        PXView view = graph.TypedViews.GetView((BqlCommand)command, isReadOnly);

            object row = view.SelectSingleBound(new object[] { data });

	        if (row is PXResult)
	        {
	            return ((PXResult)row)[command.GetField()];
	        }

	        return null;
	    }
	}
}
