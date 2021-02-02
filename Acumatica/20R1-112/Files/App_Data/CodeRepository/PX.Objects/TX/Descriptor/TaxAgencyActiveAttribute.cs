using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;

namespace PX.Objects.TX.Descriptor
{
	/// <summary>
	/// Displays only Active or OneTime tax agency
	/// </summary>
	[PXRestrictor(typeof(Where<Vendor.taxAgency, Equal<True>,
						And<Where<Vendor.status, Equal<BAccount.status.active>,
									Or<Vendor.status, Equal<BAccount.status.oneTime>>>>>), 
						Messages.TaxAgencyStatusIs, 
						typeof(Vendor.status))]
	public class TaxAgencyActiveAttribute : VendorAttribute
	{
		public TaxAgencyActiveAttribute(Type search)
			: base(search)
		{
		}

		public TaxAgencyActiveAttribute()
			: base()
		{
		}

		protected override void Initialize()
		{
			base.Initialize();

			DisplayName = "Tax Agency";
			DescriptionField = typeof(Vendor.acctName);
		}
	}
}
