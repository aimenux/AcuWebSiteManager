using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class DeductionActiveSelectorAttribute : PXSelectorAttribute
	{
		public DeductionActiveSelectorAttribute(Type where = null) : 
			base(BqlTemplate.OfCommand<SearchFor<PRDeductCode.codeID>
				.Where<PRDeductCode.isActive.IsEqual<True>
					.And<BqlPlaceholder.A>>>
				.Replace<BqlPlaceholder.A>(where == null ? typeof(Where<True.IsEqual<True>>) : where)
				.ToType())
		{
			SubstituteKey = typeof(PRDeductCode.codeCD);
			DescriptionField = typeof(PRDeductCode.description);
		}
	}
}
