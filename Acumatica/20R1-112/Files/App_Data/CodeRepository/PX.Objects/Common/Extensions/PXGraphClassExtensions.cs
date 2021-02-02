using CommonServiceLocator;
using System;
using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.Common.Extensions
{
	internal struct CachePermission
	{
		public bool AllowSelect;
		public bool AllowInsert;
		public bool AllowUpdate;
		public bool AllowDelete;
	}

	public static class PXGraphClassExtensions
	{
		public static void DisableCaches(this PXGraph graph)
		{
			foreach (PXCache graphCache in graph.Caches.Values)
			{
				graphCache.AllowInsert =
				graphCache.AllowUpdate =
				graphCache.AllowDelete = false;
			}
		}

		internal static void EnableCaches(this PXGraph graph)
		{
			foreach (PXCache graphCache in graph.Caches.Values)
			{
				graphCache.AllowInsert =
				graphCache.AllowUpdate =
				graphCache.AllowDelete = true;
			}
		}

		internal static Dictionary<Type, CachePermission> SaveCachesPermissions(this PXGraph graph, bool saveEnabledOnly=false)
		{
			Dictionary<Type, CachePermission> permissions = new Dictionary<Type, CachePermission>();
			Type type;
			foreach (PXCache graphCache in graph.Caches.Values)
			{
				if (saveEnabledOnly && !(graphCache.AllowInsert || graphCache.AllowUpdate || graphCache.AllowDelete)) 
					continue;

				type = graphCache.GetType();
				if (permissions.ContainsKey(type))
					continue;

				permissions.Add(type,
					new CachePermission
					{
						AllowSelect = graphCache.AllowSelect,
						AllowInsert = graphCache.AllowInsert,
						AllowUpdate = graphCache.AllowUpdate,
						AllowDelete = graphCache.AllowDelete
					});
			}
			return permissions;
		}

		internal static void LoadCachesPermissions(this PXGraph graph, Dictionary<Type, CachePermission> allows)
		{
			CachePermission permission;
			foreach (PXCache graphCache in graph.Caches.Values)
			{
				if (allows.TryGetValue(graphCache.GetType(), out permission)) 
				{
					graphCache.AllowSelect = permission.AllowSelect;
					graphCache.AllowInsert = permission.AllowInsert;
					graphCache.AllowUpdate = permission.AllowUpdate;
					graphCache.AllowDelete = permission.AllowDelete;
				}
			}
		}

		internal static TService GetService<TService>(this PXGraph graph)
		{
			Func<PXGraph, TService> factoryFunc = (Func<PXGraph, TService>)ServiceLocator.Current.GetService(typeof(Func<PXGraph, TService>));
			return factoryFunc(graph);
		}

		public delegate string DescriptionMaker(string originalDescription);

		public static DescriptionMaker GetLocalizedDescriptionMaker(this string formatTemplate) =>
			(originalDescription) => PXMessages.LocalizeFormatNoPrefix(formatTemplate, originalDescription);

		public static DescriptionMaker GetDescriptionMaker(this string formatTemplate) =>
			(originalDescription) => string.Format(formatTemplate, originalDescription);

		public static string MakeDescription<TField>(this PXGraph graph, string originalDescription, DescriptionMaker maker)
			where TField: IBqlField
		{
			string generatedDescription = maker(originalDescription);
			int maxDescriptionLength = 0;
			graph.Caches[BqlCommand.GetItemType<TField>()].Adjust<PXDBStringAttribute>().For<TField>(attribute => maxDescriptionLength = attribute?.Length ?? -1);

			if (maxDescriptionLength == -1) // PXDBStringAttribute not found, cutting is not needed
			{
				return generatedDescription;
			}

			int delta = generatedDescription.Length - maxDescriptionLength;
			if (delta > 0) // cutting is needed
			{
				if (delta > originalDescription.Length)
				{
					throw new PXException(Messages.DescriptionCannotBeGenerated, maxDescriptionLength, typeof(TField).FullName, originalDescription, generatedDescription);
				}

				generatedDescription = maker(originalDescription.Substring(0, originalDescription.Length - delta));
			}
			return generatedDescription;
		}
		public static string MakeDescription<TField>(this PXGraph graph, string formatTemplate, string originalDescription)
			where TField : IBqlField
		{
			return graph.MakeDescription<TField>(originalDescription, formatTemplate.GetDescriptionMaker());
		}

		public static string MakeLocalizedDescription<TField>(this PXGraph graph, string formatTemplate, string originalDescription)
			where TField : IBqlField
		{
			return graph.MakeDescription<TField>(originalDescription, formatTemplate.GetLocalizedDescriptionMaker());
		}
	}
}
