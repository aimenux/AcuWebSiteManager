using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR.Extensions
{
	/// <exclude/>
	public abstract class CRUpdateRelatedContactInfoGraphExt<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		protected bool ContactWasUpdated(PXCache cache, Contact contact)
		{
			var original = cache.GetOriginal(contact) as Contact;

			if (original == null
				|| contact.FirstName != original.FirstName
				|| contact.LastName != original.LastName
				|| contact.FullName != original.FullName
				|| contact.Salutation != original.Salutation
				|| contact.EMail != original.EMail
				|| contact.WebSite != original.WebSite
				|| contact.Phone1Type != original.Phone1Type
				|| contact.Phone1 != original.Phone1
				|| contact.Phone2Type != original.Phone2Type
				|| contact.Phone2 != original.Phone2
				|| contact.Phone3Type != original.Phone3Type
				|| contact.Phone3 != original.Phone3
				|| contact.FaxType != original.FaxType
				|| contact.Fax != original.Fax
				|| contact.ConsentAgreement != original.ConsentAgreement
				|| contact.ConsentDate != original.ConsentDate
				|| contact.ConsentExpirationDate != original.ConsentExpirationDate)
			{
				return true;
			}

			return false;
		}

		protected bool AddressWasUpdated(PXCache cache, Address address)
		{
			var original = cache.GetOriginal(address) as Address;

			if (original == null
				|| address.AddressLine1 != original.AddressLine1
				|| address.AddressLine2 != original.AddressLine2
				|| address.City != original.City
				|| address.State != original.State
				|| address.PostalCode != original.PostalCode
				|| address.CountryID != original.CountryID)
			{
				return true;
			}

			return false;
		}
	}
}
