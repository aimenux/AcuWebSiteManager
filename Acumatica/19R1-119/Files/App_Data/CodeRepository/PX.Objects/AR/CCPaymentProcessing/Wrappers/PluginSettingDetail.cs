using System.Collections.Generic;
using V1 = PX.CCProcessingBase;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class PluginSettingDetail : V1.ISettingsDetail
	{
		public string DetailID { get; set; }
		public string Descr { get; set; }
		public string Value { get; set; }
		public int? ControlType { get; set; }
		public bool? IsEncryptionRequired { get; set; }

		private IList<KeyValuePair<string, string>> comboValues;

		public PluginSettingDetail(string detailID, string descr, string value) : this(detailID, descr, value, 1) { }
		public PluginSettingDetail(string detailID, string descr, string value, int? controlType)
		{
			this.DetailID = detailID;
			this.Descr = descr;
			this.Value = value;
			this.ControlType = controlType;
			this.IsEncryptionRequired = controlType == 4;
			comboValues = new List<KeyValuePair<string, string>>();
		}

		public PluginSettingDetail()
		{
		}

		public IList<KeyValuePair<string, string>> GetComboValues()
		{
			return comboValues;
		}

		public void SetComboValues(IList<KeyValuePair<string, string>> list)
		{
			comboValues = list;
		}
	}
}
