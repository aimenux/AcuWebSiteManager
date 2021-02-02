using System;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Helpers;

namespace PX.Objects.CA
{
	public class ProcCenterByPluginTypesSelectorAttribute : PXCustomSelectorAttribute
	{
		Type search;
		string[] pluginTypeNames;
		public ProcCenterByPluginTypesSelectorAttribute(Type search, Type selectType, string[] pluginTypeNames) : base(selectType)
		{
			this.search = search;
			this.pluginTypeNames = pluginTypeNames;
		}

		public IEnumerable GetRecords()
		{
			BqlCommand command = BqlCommand.CreateInstance(search);
			PXView view = new PXView(_Graph, false, command);
			foreach (object obj in view.SelectMulti())
			{
				CCProcessingCenter procCenter = PXResult.Unwrap<CCProcessingCenter>(obj);
				if (CheckPluginType(procCenter))
				{
					yield return obj;
				}
			}
		}

		private bool CheckPluginType(CCProcessingCenter procCenter)
		{
			string pluginTypeName = procCenter.ProcessingTypeName;
			bool res = pluginTypeNames.Contains(pluginTypeName);
			if (res)
			{
				return true;
			}

			try
			{
				Type pluginType = CCPluginTypeHelper.GetPluginType(pluginTypeName);
				foreach (string typeName in pluginTypeNames)
				{
					res = CCPluginTypeHelper.CheckParentClass(pluginType, typeName, 0, 3) ||
						CCPluginTypeHelper.CheckImplementInterface(pluginType, typeName);
					if (res)
					{
						return true;
					}
				}
			}
			catch { }
			return false;
		}
	}
}