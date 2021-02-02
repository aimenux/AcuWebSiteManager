using System;
using SW.Data;
using SW.Data.Search;
using System.Collections.Generic;

namespace SW.Objects.CS
{
	public class BAccountSearch : SWSearchCacheable<BAccount>
	{
		private BusinessAccountMaint graph = new BusinessAccountMaint();

		public class Result : SWSearchResult
		{
			private string link;
			private string navigationKey;

			internal Result(string link, string navigationKey, string text, string descr)
			{
				this.link = link;
				this.navigationKey = navigationKey;
				this.linkText = text;
				this.description = descr;
			}

			public override string GetLink()
			{
				return this.link;
			}

			public string NavigationKey
			{
				get { return this.navigationKey; }
			}
		}

		public void NavigateToResult(int bacctId)
		{
			BAccount acct = SWSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.SelectWindowed(graph, 0, 1, bacctId);
			if (acct == null)
				return;

			if (acct.Type == "CU")
			{
				AR.Customer cust = SWSelect<AR.Customer, Where<AR.Customer.bAccountID, Equal<Required<BAccount.bAccountID>>>>.SelectWindowed(this.graph, 0, 1, acct.BAccountID);
				SWRedirectHelper.TryRedirect(graph.Caches[typeof(SW.Objects.AR.Customer)], cust, "");
			}
			else
			{
				AP.Vendor vend = SWSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<BAccount.bAccountID>>>>.SelectWindowed(this.graph, 0, 1, acct.BAccountID);
				SWRedirectHelper.TryRedirect(graph.Caches[typeof(SW.Objects.AP.Vendor)], vend, "");
			}
		}

		protected class BAccountCacheResult : CacheResult<int>
		{
			public BAccountCacheResult(string query, List<int> result)
			{
				this.query = query;
				this.results = result;
				this.searchType = typeof(BAccountSearch);
			}
		}

		protected override CacheResult CreateResult(string query)
		{
			List<int> resIds = new List<int>();
			if (string.IsNullOrEmpty(query))
				return null;

			string likequery = "%" + query + "%";
			SWResultset<BAccount> accts;
			accts = SWSelect<BAccount, Where<BAccount.acctName, Like<Required<BAccount.acctName>>, Or<BAccount.acctCD, Like<Required<BAccount.acctCD>>>>>.Select(this.graph, likequery, likequery);

			foreach (BAccount ba in accts)
				if (ba.Type != "UN")
					resIds.Add(ba.BAccountID.Value);
			foreach (BAccount ba in this.GetAccountsByAddress(likequery))
				resIds.Add(ba.BAccountID.Value);
			foreach (BAccount ba in this.GetAccountsByContact(likequery))
				resIds.Add(ba.BAccountID.Value);

			return new BAccountCacheResult(query, resIds);
		}

		protected override List<SWSearchResult> CreateSearchResult(CacheResult cacheRes, int first, int count)
		{
			List<SWSearchResult> result = new List<SWSearchResult>();
			foreach (BAccount ba in this.CreateRecordsResult(cacheRes, first, count))
				result.Add(new Result(CreateLink(ba), CreateKeys(ba), CreateCaption(ba), CreateDescription(ba)));
			return result;
		}

		protected override List<BAccount> CreateRecordsResult(CacheResult cacheRes, int first, int count)
		{
			List<BAccount> result = new List<BAccount>();
			BAccountCacheResult cache = cacheRes as BAccountCacheResult;

			if (cache == null || cache.Results == null)
				return result;
			if (first < 0 || first >= cache.Results.Count)
				return result;
			if (count == -1)
				count = cache.Results.Count;

			this.totalCount = cache.Results.Count;
			for (int i = first; i < first + count && i < cache.Results.Count; i++)
			{
				BAccount ba = SWSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.SelectWindowed(this.graph, 0, 1, cache.Results[i]);
				if (ba != null)
					result.Add(ba);
			}
			return result;
		}

		private List<BAccount> GetAccountsByAddress(string likequery)
		{
			List<BAccount> result = new List<BAccount>();
			foreach (Address addr in SWSelect<Address, Where<Address.city, Like<Required<Address.city>>,
				Or<Address.postalCode, Like<Required<Address.postalCode>>>>>.Select(this.graph, likequery, likequery))
			{
				BAccount ba = SWSelect<BAccount, Where<BAccount.defAddressID, Equal<Required<Address.addressID>>>>.SelectWindowed(this.graph, 0, 1, addr.AddressID);
				if (ba != null && ba.Type != "UN")
					result.Add(ba);
			}

			foreach (Address addr in this.GetAddressesByCountry(likequery))
			{
				BAccount ba = SWSelect<BAccount, Where<BAccount.defAddressID, Equal<Required<Address.addressID>>>>.SelectWindowed(this.graph, 0, 1, addr.AddressID);
				if (ba != null && ba.Type != "UN")
					result.Add(ba);
			}
			return result;
		}

		private List<Address> GetAddressesByCountry(string likequery)
		{
			List<Address> result = new List<Address>();
			foreach(Country country in SWSelect<Country, Where<Country.description, Like<Required<Country.description>>>>.Select(this.graph, likequery))
			{
				Address addr = SWSelect<Address, Where<Address.countryID, Equal<Required<Country.countryID>>>>.SelectWindowed(this.graph, 0, 1, country.CountryID);
				if (addr != null)
					result.Add(addr);
			}
			return result;
		}

		private List<BAccount> GetAccountsByContact(string likequery)
		{
			List<BAccount> result = new List<BAccount>();
			foreach (Contact cont in SWSelect<Contact, Where<Contact.salutation, Like<Required<Contact.salutation>>,
			Or<Contact.eMail, Like<Required<Contact.eMail>>,
			Or<Contact.webSite, Like<Required<Contact.webSite>>,
			Or<Contact.phone1, Like<Required<Contact.phone1>>,
			Or<Contact.phone2, Like<Required<Contact.phone2>>,
			Or<Contact.phone3, Like<Required<Contact.phone3>>>>>>>>>.Select(this.graph, likequery, likequery, likequery, likequery, likequery, likequery))
			{
				BAccount ba = SWSelect<BAccount, Where<BAccount.defContactID, Equal<Required<Contact.contactID>>>>.SelectWindowed(this.graph, 0, 1, cont.ContactID);
				if (ba != null && ba.Type != "UN")
					result.Add(ba);
			}
			return result;
		}

		private string CreateLink(BAccount ba)
		{
			switch (ba.Type)
			{
				case "VE": return "~/Pages/AP/AP202000.aspx";
				case "CU": return "~/Pages/AR/AR202000.aspx";
				default: return "~/Pages/AP/AP202000.aspx";
			}
		}

		private string CreateKeys(BAccount ba)
		{
			return ba.BAccountID.Value.ToString();
		}

		private string CreateCaption(BAccount ba)
		{
			return ba.AcctCD + " - " + ba.AcctName;
		}

		private string CreateDescription(BAccount ba)
		{
			string result = "";
			Address addr = SWSelect<Address, Where<Address.addressID, Equal<Required<BAccount.defAddressID>>>>.SelectWindowed(this.graph, 0, 1, ba.DefAddressID);
			Contact cont = SWSelect<Contact, Where<Contact.contactID, Equal<Required<BAccount.defContactID>>>>.SelectWindowed(this.graph, 0, 1, ba.DefContactID);
			if (addr != null)
			{
				result += string.IsNullOrEmpty(addr.AddressLine1) ? "" : "Address 1: " + addr.AddressLine1 + "<br />";
				result += string.IsNullOrEmpty(addr.AddressLine2) ? "" : "Address 2: " + addr.AddressLine2 + "<br />";
				result += string.IsNullOrEmpty(addr.City) ? "" : "City: " + addr.City + "<br />";
				result += string.IsNullOrEmpty(addr.PostalCode) ? "" : "Zip Code: " + addr.PostalCode;
				if (!string.IsNullOrEmpty(addr.CountryID))
				{
					result += string.IsNullOrEmpty(addr.PostalCode) ? "" : ", ";
					result += "Country code: " + addr.CountryID + "<br />";
				}
				else
					result += "<br />";
			}
			if (cont != null)
			{
				result += string.IsNullOrEmpty(cont.Phone1) ? "" : "Phone: " + cont.Phone1;
				if (!string.IsNullOrEmpty(cont.Salutation))
				{
					result += string.IsNullOrEmpty(cont.Phone1) ? "" : ", ";
					result += "Name: " + cont.Salutation;
				}
			}
			return result;
		}
	}
}