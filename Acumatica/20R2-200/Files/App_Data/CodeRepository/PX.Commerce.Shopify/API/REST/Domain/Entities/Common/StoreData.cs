using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.Shopify.API.REST
{
	public class StoreResponse : IEntityResponse<StoreData>
	{
		[JsonProperty("shop")]
		public StoreData Data { get; set; }
	}

	public class StoreData
	{
		/// <summary>
		/// The shop's street address.
		/// </summary>
		[JsonProperty("address1")]
		public string Address1 { get; set; }

		/// <summary>
		/// The the optional second line of the shop's street address.
		/// </summary>
		[JsonProperty("address2")]
		public string Address2 { get; set; }

		/// <summary>
		/// Whether the shop is capable of accepting payments directly through the Checkout API.
		/// </summary>
		[JsonProperty("checkout_api_supported")]
		public bool? CheckoutApiSupported { get; set; }

		/// <summary>
		/// The shop's city.
		/// </summary>
		[JsonProperty("city")]
		public string City { get; set; }

		/// <summary>
		/// The shop's country. In most cases matches the country_code.
		/// </summary>
		[JsonProperty("country")]
		public string Country { get; set; }

		/// <summary>
		/// The two-letter country code corresponding to the shop's country.
		/// </summary>
		[JsonProperty("country_code")]
		public string CountryCode { get; set; }

		/// <summary>
		/// The shop's normalized country name.
		/// </summary>
		[JsonProperty("country_name")]
		public string CountryName { get; set; }

		/// <summary>
		/// Whether the shop is applying taxes on a per-county basis. Only applicable to shops based in the US. Valid values: true or null."
		/// </summary>
		[JsonProperty("county_taxes")]
		public bool? CountyTaxes { get; set; }

		/// <summary>
		/// The date and time (ISO 8601) when the shop was created.
		/// </summary>
		[JsonProperty("created_at")]
		public string DataCreatedAt { get; set; }

		/// <summary>
		/// The contact email used for communication between the shop owner and the customer.
		/// </summary>
		[JsonProperty("customer_email")]
		public string CustomerEmail { get; set; }

		/// <summary>
		/// The three-letter code (ISO 4217 format) for the shop's default currency.
		/// </summary>
		[JsonProperty("currency")]
		public string Currency { get; set; }

		/// <summary>
		/// The shop's domain.
		/// </summary>
		[JsonProperty("domain")]
		public string Domain { get; set; }

		/// <summary>
		/// A list of enabled currencies (ISO 4217 format) that the shop accepts. Merchants can enable currencies from their Shopify Payments settings in the Shopify admin.
		/// </summary>
		[JsonProperty("enabled_presentment_currencies")]
		public string[] EnabledPresentmentCurrencies { get; set; }

		/// <summary>
		/// Whether the shop is eligible to receive a free credit card reader from Shopify.
		/// </summary>
		[JsonProperty("eligible_for_card_reader_giveaway")]
		public bool? EligibleForCardReaderGiveaway { get; set; }

		/// <summary>
		/// Whether the shop is eligible to use Shopify Payments.
		/// </summary>
		[JsonProperty("eligible_for_payments")]
		public bool? EligibleForPayments { get; set; }

		/// <summary>
		/// The contact email used for communication between Shopify and the shop owner.
		/// </summary>
		[JsonProperty("email")]
		public string Email { get; set; }

		/// <summary>
		/// Whether the shop forces requests made to its resources to be made over SSL using the HTTPS protocol. Valid values: true or false.
		/// </summary>
		[JsonProperty("force_ssl")]
		public bool? ForceSSL { get; set; }

		/// <summary>
		/// The GSuite URL for the store, if applicable.
		/// </summary>
		[JsonProperty("google_apps_domain")]
		public string GoogleAppsDomain { get; set; }

		/// <summary>
		/// Whether the GSuite login is enabled. Shops with this feature will be able to log in through the GSuite login page. Valid values: true, null
		/// </summary>
		[JsonProperty("google_apps_login_enabled")]
		public bool? GoogleAppsLoginEnabled { get; set; }

		/// <summary>
		/// Whether any active discounts exist for the shop.
		/// </summary>
		[JsonProperty("has_discounts")]
		public bool? HasDiscounts { get; set; }

		/// <summary>
		/// Whether any active gift cards exist for the shop.
		/// </summary>
		[JsonProperty("has_gift_cards")]
		public bool? HasGiftCards { get; set; }

		/// <summary>
		/// Whether the shop has an online storefront.
		/// </summary>
		[JsonProperty("has_storefront")]
		public bool? HasStorefront { get; set; }

		/// <summary>
		/// The name of the timezone assigned by the IANA.
		/// </summary>
		[JsonProperty("iana_timezone")]
		public string IANATimezone { get; set; }

		/// <summary>
		/// The ID for the shop. A 64-bit unsigned integer.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The latitude of the shop's location.
		/// </summary>
		[JsonProperty("latitude")]
		public decimal? Latitude { get; set; }

		/// <summary>
		/// The longitude of the shop's location.
		/// </summary>
		[JsonProperty("longitude")]
		public decimal? Longitude { get; set; }

		/// <summary>
		/// A string representing the way currency is formatted when the currency isn't specified.
		/// </summary>
		[JsonProperty("money_format")]
		public string MoneyFormat { get; set; }

		/// <summary>
		/// A string representing the way currency is formatted in email notifications when the currency isn't specified.
		/// </summary>
		[JsonProperty("money_in_emails_format")]
		public string MoneyInEmailsFormat { get; set; }

		/// <summary>
		/// Whether the shop has enabled multiple locations.
		/// </summary>
		[JsonProperty("multi_location_enabled")]
		public bool? MultiLocationEnabled { get; set; }

		/// <summary>
		/// The shop's myshopify.com domain.
		/// </summary>
		[JsonProperty("myshopify_domain")]
		public string MyshopifyDomain { get; set; }

		/// <summary>
		/// The name of the shop.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// Whether the password protection page is enabled on the online storefront.
		/// </summary>
		[JsonProperty("password_enabled")]
		public bool? PasswordEnabled { get; set; }

		/// <summary>
		/// The contact phone number for the shop.
		/// </summary>
		[JsonProperty("phone")]
		public string Phone { get; set; }

		/// <summary>
		/// The display name of the Shopify plan the shop is on.
		/// </summary>
		[JsonProperty("plan_display_name")]
		public string PlanDisplayName { get; set; }

		/// <summary>
		/// Whether the pre-launch page is enabled on the online storefront.
		/// </summary>
		[JsonProperty("pre_launch_enabled")]
		public bool? PreLaunchEnabled { get; set; }

		/// <summary>
		/// The cookie consent level defined on the online storefront.
		/// </summary>
		[JsonProperty("cookie_consent_level")]
		public string CookieConsentLevel { get; set; }

		/// <summary>
		/// The name of the Shopify plan the shop is on.
		/// </summary>
		[JsonProperty("plan_name")]
		public string PlanName { get; set; }

		/// <summary>
		/// The shop's primary locale, as configured in the language settings of the shop's theme.
		/// </summary>
		[JsonProperty("primary_locale")]
		public string PrimaryLocal { get; set; }

		/// <summary>
		/// The shop's normalized province or state name.
		/// </summary>
		[JsonProperty("province")]
		public string Province { get; set; }

		/// <summary>
		/// The two-letter code for the shop's province or state.
		/// </summary>
		[JsonProperty("province_code")]
		public string ProvinceCode { get; set; }

		/// <summary>
		/// Whether the shop requires an extra Shopify Payments agreement.
		/// </summary>
		[JsonProperty("requires_extra_payments_agreement")]
		public bool? RequiredExtraPaymentsAgreement { get; set; }

		/// <summary>
		/// Whether the shop has any outstanding setup steps or not.
		/// </summary>
		[JsonProperty("setup_required")]
		public bool? SetupRequired { get; set; }

		/// <summary>
		/// The username of the shop owner.
		/// </summary>
		[JsonProperty("shop_owner")]
		public string ShopOwner { get; set; }

		/// <summary>
		/// The handle of the partner account that referred the merchant to Shopify, if applicable.
		/// </summary>
		[JsonProperty("source")]
		public string Source { get; set; }

		/// <summary>
		/// Whether applicable taxes are included in product prices. Valid values: true or null.
		/// </summary>
		[JsonProperty("taxes_included")]
		public bool? TaxesIncluded { get; set; }

		/// <summary>
		/// Whether taxes are charged for shipping. Valid values: true or false.
		/// </summary>
		[JsonProperty("tax_shipping")]
		public bool? TaxShipping { get; set; }

		/// <summary>
		/// The name of the timezone the shop is in.
		/// </summary>
		[JsonProperty("timezone")]
		public string Timezone { get; set; }

		/// <summary>
		/// The date and time (ISO 8601) when the shop was last updated.
		/// </summary>
		[JsonProperty("updated_at")]
		public string DataModifiedAt { get; set; }

		/// <summary>
		/// The default unit of weight measurement for the shop.
		/// </summary>
		[JsonProperty("weight_unit")]
		public string WeightUnit { get; set; }

		/// <summary>
		/// The shop's zip or postal code.
		/// </summary>
		[JsonProperty("zip")]
		public string PostCode { get; set; }

		/// <summary>
		/// The API response time from Shopify
		/// </summary>
		[JsonIgnore]
		public DateTime ResponseTime { get; set; }

		/// <summary>
		/// The API call capacity
		/// </summary>
		[JsonIgnore]
		public int ApiCapacity { get; set; }

		/// <summary>
		/// The API call available currently
		/// </summary>
		[JsonIgnore]
		public int ApiAvailable { get; set; }

		/// <summary>
		/// The API call version
		/// </summary>
		[JsonIgnore]
		public string ApiVersion { get; set; }
	}

}
