using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	public class TaxDetailProjectDefaultAttribute : ProjectDefaultAttribute
	{
		protected Type TaxCategoryField { get; set; }

		public TaxDetailProjectDefaultAttribute(Type taxCategoryField) : this(null, taxCategoryField)
		{
		}

		public TaxDetailProjectDefaultAttribute(string module, Type taxCategoryField) : base(module)
		{
			TaxCategoryField = taxCategoryField;
		}

		public TaxDetailProjectDefaultAttribute(string module, Type search, Type taxCategoryField) : this(module, search, null, taxCategoryField)
		{
		}

		public TaxDetailProjectDefaultAttribute(string module, Type search, Type account, Type taxCategoryField) : base(module, search, account)
		{
			TaxCategoryField = taxCategoryField;
		}

		public override void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && (string)cache.GetValue(e.Row, TaxCategoryField.Name) == TaxCategory.EmployerTax)
			{
				base.FieldDefaulting(cache, e);
			}
		}
	}
}
