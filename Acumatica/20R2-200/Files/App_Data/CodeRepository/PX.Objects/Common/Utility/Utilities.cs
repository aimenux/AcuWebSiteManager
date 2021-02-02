using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.Common
{
	public static class Utilities
	{
		public static void Swap<T>(ref T first, ref T second)
		{
			T temp = first;
			first = second;
			second = temp;
		}

		[Obsolete("This method is obsolete and will be removed in 2020R1. Use PX.Objects.GL.BatchModule.GetDisplayName instead.")]
		public static string GetSubledgerTitle(this PXGraph graph, string subledgerPrefix)
		{
			return PX.Objects.GL.BatchModule.GetDisplayName(subledgerPrefix);
		}

		[Obsolete("This method is obsolete and will be removed in 2020R1. Use PX.Objects.GL.BatchModule.GetDisplayName instead.")]
		public static string GetSubledgerTitle<TSubledgerConst>(this PXGraph graph)
			where TSubledgerConst : IConstant<string>, IBqlOperand, new()
		{
			return PX.Objects.GL.BatchModule.GetDisplayName<TSubledgerConst>();
		}

		public static TDestinationDAC Clone<TSourceDAC, TDestinationDAC>(PXGraph graph, TSourceDAC source)
			where TSourceDAC : class, IBqlTable, new()
			where TDestinationDAC : class, IBqlTable, new()
		{
			PXCache sourceCache = graph.Caches<TSourceDAC>();
			PXCache destinationCache = graph.Caches<TDestinationDAC>();
			TDestinationDAC result = (TDestinationDAC)destinationCache.CreateInstance();
			foreach (string field in destinationCache.Fields)
			{
				if (sourceCache.Fields.Contains(field))
				{
					destinationCache.SetValue(result, field, sourceCache.GetValue(source, field));
				}
			}

			return result;
		}

		public static PXResultset<TSourceDAC> ToResultset<TSourceDAC>(TSourceDAC item)
			where TSourceDAC : class, IBqlTable, new()
		{
			return new PXResultset<TSourceDAC>()
			{
				new PXResult<TSourceDAC>(item)
			};
		}

		public static void SetDependentFieldsAfterBranch(List<Api.Models.Command> script,
						(string name, string viewName) firstField,
						List<(string name, string viewName)> fieldList)
		{
			// 1) insert dependent fields after the firstField
			// 2) all fields must belong to the same view.
			int firstFieldIDIndex = script.FindIndex(cmd => cmd.FieldName == firstField.name && cmd.ObjectName == firstField.viewName);

			if (firstFieldIDIndex < 0)
				return;

			List<Api.Models.Command> commandList = new List<Api.Models.Command>();
			foreach (var item in fieldList)
			{
				Api.Models.Command cmdItem = script.Where(cmd => cmd.FieldName == item.name && cmd.ObjectName == item.viewName).SingleOrDefault();

				if (cmdItem == null)
					return;

				// All fields can be located on different views.
				// Set the same view for processing together.
				cmdItem.ObjectName = firstField.viewName;
				cmdItem.Commit = false;
				commandList.Add(cmdItem);
			}

			Api.Models.Command[] commands = commandList.ToArray();

			//last field should invoke Commit
			commands[commands.Length - 1].Commit = true;

			foreach (Api.Models.Command command in commands)
			{
				script.Remove(command);
			}
			firstFieldIDIndex = script.FindIndex(cmd => cmd.FieldName == firstField.name && cmd.ObjectName == firstField.viewName);
			script.InsertRange(firstFieldIDIndex + 1, commands);
		}
	}
}
