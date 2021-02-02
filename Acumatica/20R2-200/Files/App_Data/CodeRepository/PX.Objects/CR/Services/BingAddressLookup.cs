using PX.AddressValidator;
using PX.CCProcessingBase.Attributes;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CR.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

namespace PX.AddressLookup
{
	[PXDisplayTypeName("Bing Maps")]
	public class BingAddressLookup : IAddressLookupService
	{
		public static class BingAutosuggestReponseStatus
		{
			public const string OK = "200";
		}

		public static readonly string AddressValidatorID = "BingAddressLookup";
		#region Properties
		protected string ApiKey { get; set; }
		protected string Country { get; set; }
		#endregion

		#region IAddressConnectedService
		public virtual void Initialize(IEnumerable<IAddressValidatorSetting> settings)
		{
			foreach (IAddressValidatorSetting setting in settings)
			{
				if ("API_KEY".Equals(setting.SettingID, StringComparison.InvariantCultureIgnoreCase))
				{
					ApiKey = setting.Value;
				}
				if ("COUNTRY".Equals(setting.SettingID, StringComparison.InvariantCultureIgnoreCase))
				{
					if (string.IsNullOrEmpty(setting.Value))
					{
						Country = "\'\'";
					} else
					{
						char[] charsToTrim = { ' ', '"', '\'' };
						Country = string.Format("\'{0}\'", setting.Value.Trim(charsToTrim));
					}
				}
			}
		}

		public virtual PingResult Ping()
		{
			List<string> messages = new List<string>();
			bool responseStatus = true;

			string url = string.Format("https://dev.virtualearth.net/REST/v1/Locations/?q=ttt&o=xml&key={0}", ApiKey);
			XmlDocument xDoc = new XmlDocument();
			xDoc.Load(url);
			string status = xDoc.DocumentElement?.GetElementsByTagName("StatusCode")?.Item(0)?.InnerText;
			if (string.IsNullOrEmpty(status))
			{
				responseStatus = false;
				messages.Add("");
				messages.Add(Messages.AutocompleteServiceTestFailed);
				messages.Add(url);
			}
			else if (!status.Equals(BingAutosuggestReponseStatus.OK))
			{
				responseStatus = false;
				messages.Add("");
				messages.Add(string.Format("{0}-{1}", status, xDoc.DocumentElement?.GetElementsByTagName("StatusDescription")?.Item(0)?.InnerText));
				messages.Add(xDoc.DocumentElement?.GetElementsByTagName("ErrorDetails")?.Item(0)?.InnerText);
			}
			PingResult result = new PingResult
			{
				IsSuccess = responseStatus,
				Messages = messages.ToArray()
			};
			return result;
		}

		public virtual IAddressValidatorSetting[] DefaultSettings
		{
			get
			{
				IAddressValidatorSetting[] settings = new IAddressValidatorSetting[]
				{
					new AddressValidatorSetting(
						AddressValidatorID,
						"API_KEY",
						1,
						"API Key",
						"",
						AddressValidatorSettingControlType.Password),
					new AddressValidatorSetting(
							AddressValidatorID,
							"COUNTRY",
							2,
							"Country restriction",
							"US", 
							AddressValidatorSettingControlType.SingleCountryISO)
				};
				return settings;
			}
		}
		#endregion

		#region IAddressLookupService
		public string GetClientScript(PXGraph graph)
		{
			string language = "";
			if (string.IsNullOrEmpty(graph?.Culture?.TwoLetterISOLanguageName) == false)
			{
				language = graph.Culture.TwoLetterISOLanguageName;
			}
			string sSciprt = string.Format("<script type='text/javascript'>  var countryCodeSettings = {0};  </script>\n", Country);
			string sIncludeEntityTypes = "Address,Business"; //Default: Address,Place,Business
			sSciprt +=
				string.Format("<script type='text/javascript' src='https://www.bing.com/api/maps/mapcontrol?key={0}&setLang={1}&includeEntityTypes={2}' async defer></script>"
					, ApiKey
					, language
					, sIncludeEntityTypes) +
				@"<script type='text/javascript' src='..\..\Scripts\AddressLookup\BingMapsAPI.js' async defer></script>";
			return sSciprt;
		}
		#endregion
	}
}
