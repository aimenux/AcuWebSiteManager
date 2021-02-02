using PX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	[PXLocalizable()]
	public static class BCObjectsMessages
	{
		public const string DuplicateCustomerEmail = "The {0} customer has a duplicate email. Please provide a unique email address for correct synchronization with BigCommerce.";

		public const string NoTranIDForCC = "The externally authenticated transaction could not be registered. The transaction ID is not provided.";
		public const string NoProfileForCC = "The externally authenticated transaction could not be registered. The Create Payment Profile flag is not provided.";
		public const string DuplicateLocationRows = "Duplicate locations or a combination of a specified location and an empty location cannot be assigned to the same warehouse.";

		public const string OrderMissingShipVia = "The order is missing the ship via code. Please map it with the shipping option in the store settings or specify it on the Customer Locations (AR303020) form.";
		public const string FailureToMatchSavedTaxes = "Taxes could not be imported from the external provider. The order was sent with taxes, but the system could not parse the provided tax field ({0}).";
		public const string TaxNameDoesntExist = "The received tax code is invalid or mapped to an empty string. Please check the tax code mapping on the Substitution Lists (SM206026) form.";
		public const string ExternalTaxProviderTaxFor = "External Tax Provider {0}";
		public const string WrongModeDefaultTaxZone = "This default tax zone cannot be set for with current tax synchronization settings.";
		public const string CannotFindSaveTaxIDs = "The following tax IDs received from the external system are too long and cannot be saved: {0}. Map them to tax IDs of up to {1} characters in the BCCTAXCODES substitution list on the Substitution Lists (SM206026) form.";
		public const string CannotFindTaxZoneVendor = "The order could not be created because the tax agency is not configured for the {0} tax zone. Please specify the tax agency for the tax zone on the Tax Zones (TX206000) form.";
		public const string CannotFindTaxZone = "The order could not be created because the tax zone is not found for the following tax IDs: {0}. Please review the tax configuration.";
		public const string CannotFindMatchingTaxAcu = "The following tax IDs from Acumatica ERP could not be matched: {0}. Make sure the taxes are configured for the {1} tax zone and tax categories, and that they have the same IDs in Acumatica ERP and the external system or are mapped in the substitution list on the Substitution Lists (SM206026) form.";
		public const string CannotFindMatchingTaxExt = "The following tax IDs from the external system could not be matched: {0}. Make sure the taxes are configured for the tax zone and tax categories, and that they have the same IDs in Acumatica ERP and the external system or are mapped in the substitution list on the Substitution Lists (SM206026) form.";

		public const string LineDiscount = "Line Discount";
		public const string DocumentDiscount = "Document Discount";
		public const string StoreCreditCaption = "Store Credit";
		public const string GiftCertificateCaption = "Gift Certificate";

	}
}
