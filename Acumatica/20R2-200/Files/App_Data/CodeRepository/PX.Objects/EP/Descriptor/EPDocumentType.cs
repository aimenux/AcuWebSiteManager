using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.Common;

namespace PX.Objects.EP.Descriptor
{
	public class EPDocumentType
	{
		public IEnumerable<ValueLabelPair> ValueLabelList = new ValueLabelList
		{
			{ EPExpenseClaimDetails.DocType, Messages.ExpenseReceipt}
		};
	}
}
