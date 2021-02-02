using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Specific;
using PX.Objects.CA;
using PX.Objects.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;

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
			return GetPluginType(typeName, true);
		}

		public static Type GetPluginType(string typeName, bool throwOnError)
		{
			Type pluginType = PXBuildManager.GetType(typeName, throwOnError);
			return pluginType;
		}

		public static object CreatePluginInstance(CCProcessingCenter procCenter)
		{
			Type pluginType = GetPluginTypeWithCheck(procCenter);
			return Activator.CreateInstance(pluginType);
		}

		/// <summary>
		/// Gets plug-in type by name and performs its validation.
		/// </summary>
		/// <param name="typeName">Name of the plug-in type to get.</param>
		/// <param name="pluginType">Contains type if found; otherwise, null/.</param>
		/// <returns>Result of plug-in type search and validation.</returns>
		public static CCPluginCheckResult TryGetPluginTypeWithCheck(string typeName, out Type pluginType)
		{
			pluginType = null;

			if (string.IsNullOrEmpty(typeName))
				return CCPluginCheckResult.Empty;

			if (InUnsupportedList(typeName))
				return CCPluginCheckResult.Unsupported;

			pluginType = GetPluginType(typeName, false);
			if (pluginType == null)
				return CCPluginCheckResult.Missing;

			if (CCProcessingHelper.IsV1ProcessingInterface(pluginType))
				return CCPluginCheckResult.Unsupported;

			return CCPluginCheckResult.Ok;
		}

		/// <summary>
		/// Checks whether the payment plug-in type that is configured for the processing center is supported by the processing center 
		/// and returns the Type object that corresponds to this plug-in type.
		/// </summary>
		public static Type GetPluginTypeWithCheck(CCProcessingCenter procCenter)
		{
			if (procCenter == null)
				throw new ArgumentNullException(nameof(procCenter));

			string typeName = procCenter.ProcessingTypeName;

			CCPluginCheckResult checkResult = TryGetPluginTypeWithCheck(typeName, out Type pluginType);
			switch (checkResult)
			{
				case CCPluginCheckResult.Empty:
					throw new PXException(AR.Messages.ERR_PluginTypeIsNotSelectedForProcessingCenter, procCenter.ProcessingCenterID);
				case CCPluginCheckResult.Missing:
					throw new PXException(CA.Messages.ProcCenterUsesMissingPlugin, procCenter.ProcessingCenterID);
				case CCPluginCheckResult.Unsupported:
					throw new PXException(CA.Messages.NotSupportedProcCenter, procCenter.ProcessingCenterID);
			}

			return pluginType;
		}

		[Obsolete(PX.Objects.Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2021R1)]
		public static bool CheckProcessingCenterSupported(PXGraph graph, string procCenterId)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
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

		/// <summary>
		/// Performs validation of the plug-in referenced by Processing Center.
		/// </summary>
		/// <param name="graph">A graph to perform Db queries</param>
		/// <param name="procCenterId">Procesing Center Id</param>
		/// <returns>Result of plug-in validation.</returns>
		public static CCPluginCheckResult CheckProcessingCenterPlugin(PXGraph graph, string procCenterId)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
			}
			if (procCenterId == null)
			{
				throw new ArgumentNullException(nameof(procCenterId));
			}

			CCProcessingCenter procCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>
				.Select(graph, procCenterId);

			string typeName = procCenter?.ProcessingTypeName;
			return TryGetPluginTypeWithCheck(typeName, out _);
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2021R1)]
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

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2021R1)]
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
