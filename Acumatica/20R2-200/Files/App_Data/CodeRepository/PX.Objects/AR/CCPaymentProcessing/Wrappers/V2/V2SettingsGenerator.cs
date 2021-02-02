using System;
using System.Collections.Generic;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using V1 = PX.CCProcessingBase;
using PX.Data;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class V2SettingsGenerator
	{
		private Repositories.ICardProcessingReadersProvider _provider;

		public V2SettingsGenerator(Repositories.ICardProcessingReadersProvider provider)
		{
			_provider = provider;
		}

		public IEnumerable<V2.SettingsValue> GetSettings()
		{
			Dictionary<string, string> settingsDict = new Dictionary<string, string>();
			_provider.GetProcessingCenterSettingsStorage().ReadSettings(settingsDict);
			List<V2.SettingsValue> result = new List<V2.SettingsValue>();
			foreach (var setting in settingsDict)
			{
				V2.SettingsValue newSetting = new V2.SettingsValue() { DetailID = setting.Key, Value = setting.Value };
				result.Add(newSetting);
			}
			return result;
		}
	}
}
