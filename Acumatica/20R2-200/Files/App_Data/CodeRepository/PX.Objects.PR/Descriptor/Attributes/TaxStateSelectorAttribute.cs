using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class TaxStateSelectorAttribute : PXSelectorAttribute
	{
		// ToDo: AC-138220, In the Payroll Phase 2 review all the places where the country is set to "US" by the default
		public TaxStateSelectorAttribute()
			: base(typeof(Search<State.stateID, Where<State.countryID, Equal<LocationConstants.CountryUS>>>))
		{
			DescriptionField = typeof(State.name);
			Filterable = true;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (e.ReturnValue == null)
			{
				e.ReturnValue = LocationConstants.FederalStateCode;
			}
		}
	}
}
