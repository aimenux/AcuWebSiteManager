using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.IN;

namespace PX.Objects.PO.LandedCosts
{
	public class INAdjustmentWrapper
	{
		public INAdjustmentWrapper(INRegister doc, ICollection<INTran> transactions)
		{
			Document = doc;
			Transactions = transactions;
		}

		public INRegister Document { get; }

		public ICollection<INTran> Transactions { get; }
	}
}

