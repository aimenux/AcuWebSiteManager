using System;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.AR.CCPaymentProcessing.Specific;
namespace PX.Objects.AR.CCPaymentProcessing.Helpers
{
	/// <summary>A class that contains auxiliary methods for working with plug-in types.</summary>
	public static class CCPluginTypeHelper
	{
		/// <summary> The list of plug-in types that have been removed from the Acumatica ERP code in Version 2019 R2.</summary>
		public static string[] NotSupportedPluginTypeNames
		{
			get {
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

		public static object CreatePluginInstance(CCProcessingCenter procCenter)
		{
			Type pluginType = GetPluginTypeWithCheck(procCenter);
			return Activator.CreateInstance(pluginType);
		}

		/// <summary>
		/// Checks whether the payment plug-in type that is configured for the processing center is supported by the processing center 
		/// and returns the Type object that corresponds to this plug-in type.
		/// </summary>
		public static Type GetPluginTypeWithCheck(CCProcessingCenter procCenter)
		{
			string typeName = procCenter?.ProcessingTypeName;
			if (typeName == null)
			{
				throw new ArgumentNullException(nameof(CCProcessingCenter.ProcessingTypeName));
			}
			if (IsUnsupportedPluginType(typeName))
			{
				throw new PXException(Messages.TryingToUseNotSupportedPlugin, procCenter.ProcessingCenterID);
			}
			Type pluginType = GetPluginType(typeName);
			return pluginType;
		}

		public static bool CheckProcessingCenterSupported(PXGraph graph, string procCenterId)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(PXGraph));
			}
			if (procCenterId == null)
			{
				throw new ArgumentNullException(nameof(procCenterId));
			}

			CCProcessingCenter procCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>
				.Select(graph, procCenterId);

			string typeName = procCenter.ProcessingTypeName;
			return !IsUnsupportedPluginType(typeName);
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
