using PX.Data;
using System.Configuration;

namespace PX.Objects.PR
{
	public class AatrixConfiguration : ConfigurationSection
	{
		public static string WebformsUrl { get => GetInstance().iWebformsUrl; set => GetInstance().iWebformsUrl = value; }
		public static string VendorID { get => GetInstance().iVendorID; set => GetInstance().iVendorID = value; }
		public static int TransactionTimeoutMs { get => GetInstance().iTransactionTimeoutMs; set => GetInstance().iTransactionTimeoutMs = value; }

		public static string GetEndpoint(AatrixOperation op)
		{
			switch(op)
			{
				case AatrixOperation.GetAvailableFormsList:
					return GetInstance().iGetAvailableFormsListEndpoint;
				case AatrixOperation.UploadAuf:
					return GetInstance().iUploadAufEndpoint;
			}

			throw new PXException(Messages.CantFindAatrixEndpoint);
		}

		[ConfigurationProperty("webformsUrl", IsRequired = true)]
		private string iWebformsUrl
		{
			get { return (string)this["webformsUrl"]; }
			set { this["webformsUrl"] = value; }
		}

		[ConfigurationProperty("vendorID", IsRequired = true)]
		private string iVendorID
		{
			get { return (string)this["vendorID"]; }
			set { this["vendorID"] = value; }
		}

		[ConfigurationProperty("transactionTimeoutMs", IsRequired = true)]
		private int iTransactionTimeoutMs
		{
			get { return (int)this["transactionTimeoutMs"]; }
			set { this["transactionTimeoutMs"] = value; }
		}

		[ConfigurationProperty("getAvailableFormsListEndpoint", IsRequired = true)]
		private string iGetAvailableFormsListEndpoint
		{
			get { return (string)this["getAvailableFormsListEndpoint"]; }
			set { this["getAvailableFormsListEndpoint"] = value; }
		}

		[ConfigurationProperty("uploadAufEndpoint", IsRequired = true)]
		private string iUploadAufEndpoint
		{
			get { return (string)this["uploadAufEndpoint"]; }
			set { this["uploadAufEndpoint"] = value; }
		}

		private static AatrixConfiguration _This = null;

		private static AatrixConfiguration GetInstance()
		{
			if (_This == null)
			{
				_This = (AatrixConfiguration)ConfigurationManager.GetSection("aatrixConfiguration");
			}
			return _This;
		}
	}

	public enum AatrixOperation
	{
		GetAvailableFormsList,
		UploadAuf
	}
}
