using System.Collections.Generic;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public class PluginSettingDetail
	{
		/// <summary>The ID of the key required by the plug-in developed for the processing center.
		public string DetailID { get; set; }
		/// <summary>A description of the key.
		public string Descr { get; set; }
		/// <summary>The value selected by the user for the key.</summary>
		public string Value { get; set; }
		/// <summary>The type of the control.</summary>
		public int? ControlType { get; set; }
		/// <summary>Indicates (if set to <tt>true</tt>) that encryption of the key value is required.</summary>
		public bool? IsEncryptionRequired { get; set; }
		/// <summary>Sets/Retrieves the values in the combo box if the <see cref="ControlType" /> is <see cref="SettingsControlType.Combo" />.</summary>
		public ICollection<KeyValuePair<string,string>> ComboValuesCollection { get; set; }

		public PluginSettingDetail(string detailID, string descr, string value) : this(detailID, descr, value, 1) { }
		public PluginSettingDetail(string detailID, string descr, string value, int? controlType)
		{
			this.DetailID = detailID;
			this.Descr = descr;
			this.Value = value;
			this.ControlType = controlType;
			ComboValuesCollection = new Dictionary<string, string>();
		}

		public PluginSettingDetail()
		{
		}
	}
}
