using System;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using PX.Objects.AR.CCPaymentProcessing.Specific;

namespace PX.Objects.AR.CCPaymentProcessing.Helpers
{
	public class CCPluginTypeHelper
	{
		public static string[] NotSupportedPluginTypeNames
		{
			get
			{
				return new[] {
					AuthnetConstants.AIMPluginFullName,
					AuthnetConstants.CIMPluginFullName };
			}
		}

		public static Type GetPluginType(string typeName)
		{
			Type pluginType = PXBuildManager.GetType(typeName, true);
			return pluginType;
		}

		public static bool InUnsupportedList(string typeStr)
		{
			if (typeStr == null)
			{
				return false;
			}
			bool ret = NotSupportedPluginTypeNames.Any(i => i == typeStr);
			return ret;
		}

		public static bool IsUnsupportedPluginType(string typeName)
		{
			return InUnsupportedList(typeName) || !CheckPluginTypeSupported(typeName);
		}

		public static bool CheckPluginTypeSupported(string typeName)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException(typeName);
			}
			Type pluginType = GetPluginType(typeName);
			if (CCProcessingHelper.IsV1ProcessingInterface(pluginType))
			{
				return false;
			}
			return true;
		}

		/// <summary>Checks whether the plug-in type is inherited from the parent class.</summary>
		/// <param name="pluginType">The Type object that should be checked.</param>
		/// <param name="checkTypeName">The full name of the parent class.</param>
		/// <param name="currLevel">The current level of recursion iteration.</param>
		/// <param name="maxLevel">The maximum level of recursion iteration.</param>
		public static bool CheckParentClass(Type pluginType, string checkTypeName, int currLevel, int maxLevel)
		{
			if (pluginType == null)
			{
				return false;
			}
			if (maxLevel == currLevel)
			{
				return false;
			}
			string fName = pluginType.FullName;
			if (fName == checkTypeName)
			{
				return true;
			}
			return CheckParentClass(pluginType.BaseType, checkTypeName, currLevel + 1, maxLevel);
		}

		/// <summary>Checks whether the plug-in type implements the interface.</summary>
		/// <param name="pluginType">The Type object that should be checked.</param>
		/// <param name="interfaceTypeName">The full name of the interface.</param>
		public static bool CheckImplementInterface(Type pluginType, string interfaceTypeName)
		{
			TypeFilter filter = new TypeFilter(delegate (Type t, object o) {
				return t.FullName == o.ToString();
			});
			Type[] interfaces = pluginType.FindInterfaces(filter, interfaceTypeName);
			if (interfaces.Length > 0)
			{
				return true;
			}
			return false;
		}
	}
}
