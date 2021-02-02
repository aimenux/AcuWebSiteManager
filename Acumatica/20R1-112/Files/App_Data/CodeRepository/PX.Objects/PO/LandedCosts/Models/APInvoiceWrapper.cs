using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.AP;

namespace PX.Objects.PO.LandedCosts
{
	public class APInvoiceWrapper
	{
		public APInvoiceWrapper(APInvoice doc, ICollection<APTran> transactions, ICollection<APTaxTran> taxes)
		{
			Document = doc;
			Transactions = transactions;
			Taxes = taxes;
		}

		public APInvoice Document { get; }

		public ICollection<APTran> Transactions { get; }

		public ICollection<APTaxTran> Taxes { get; }
	}
}