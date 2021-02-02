using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{

	[JsonObject(Description = "client_details")]
	[Description(ShopifyCaptions.ClientDetails)]
	public class OrderClientDetails : BCAPIEntity
	{
		/// <summary>
		/// The languages and locales that the browser understands.
		/// </summary>
		[JsonProperty("accept_language")]
		public virtual string AcceptLanguage { get; set; }

		/// <summary>
		/// The browser screen height in pixels, if available.
		/// </summary>
		[JsonProperty("browser_height")]
		public virtual int? BrowserHeight { get; set; }

		/// <summary>
		/// The browser IP address.
		/// </summary>
		[JsonProperty("browser_ip")]
		public virtual string BrowserIP { get; set; }

		/// <summary>
		/// The browser screen width in pixels, if available.
		/// </summary>
		[JsonProperty("browser_width")]
		public virtual int? BrowserWidth { get; set; }

		/// <summary>
		/// A hash of the session.
		/// </summary>
		[JsonProperty("session_hash")]
		public virtual string SessionHash { get; set; }

		/// <summary>
		/// Details of the browsing client, including software and operating versions.
		/// </summary>
		[JsonProperty("user_agent")]
		public string UserAgent { get; set; }
	}

}
